/*Copyright © Spoiled Unknown*/
/*2024*/

using System.Collections.Generic;
using UnityEngine;

namespace Cluckstorm.Pooling
{
    /// <summary>
    /// Object pool manager for Cluckstorm.
    /// Efficiently manages bullet impacts, particle effects, and other reusable objects.
    /// Reduces GC allocations and improves performance in combat scenarios.
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [System.Serializable]
        public class PoolItem
        {
            public GameObject prefab;
            public int initialSize = 10;
            public bool canExpand = true;
        }

        [SerializeField] private List<PoolItem> poolItems = new List<PoolItem>();
        private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();
        private Dictionary<GameObject, GameObject> poolParents = new Dictionary<GameObject, GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePools();
        }

        private void InitializePools()
        {
            foreach (PoolItem item in poolItems)
            {
                if (item.prefab == null)
                {
                    Debug.LogError("Pool item prefab is null!");
                    continue;
                }

                // Create parent object for organization
                GameObject parentObj = new GameObject(item.prefab.name + " Pool");
                parentObj.transform.SetParent(transform);
                poolParents[item.prefab] = parentObj;

                // Create pool
                Queue<GameObject> pool = new Queue<GameObject>();
                for (int i = 0; i < item.initialSize; i++)
                {
                    GameObject obj = CreatePoolObject(item.prefab, parentObj);
                    pool.Enqueue(obj);
                }

                pools[item.prefab] = pool;
            }
        }

        private GameObject CreatePoolObject(GameObject prefab, GameObject parent)
        {
            GameObject obj = Instantiate(prefab, parent.transform);
            obj.SetActive(false);
            return obj;
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!pools.ContainsKey(prefab))
            {
                Debug.LogWarning($"Prefab {prefab.name} not found in pool!");
                return null;
            }

            Queue<GameObject> pool = pools[prefab];
            GameObject obj = null;

            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
            }
            else if (poolItems.Find(p => p.prefab == prefab).canExpand)
            {
                obj = CreatePoolObject(prefab, poolParents[prefab]);
            }
            else
            {
                Debug.LogWarning($"Pool for {prefab.name} is empty and cannot expand!");
                return null;
            }

            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            return obj;
        }

        public void Return(GameObject prefab, GameObject obj)
        {
            if (!pools.ContainsKey(prefab))
            {
                Debug.LogWarning($"Prefab {prefab.name} not found in pool!");
                Destroy(obj);
                return;
            }

            obj.SetActive(false);
            pools[prefab].Enqueue(obj);
        }

        public void Clear()
        {
            foreach (var pool in pools.Values)
            {
                foreach (var obj in pool)
                {
                    Destroy(obj);
                }
                pool.Clear();
            }
            pools.Clear();
        }
    }
}