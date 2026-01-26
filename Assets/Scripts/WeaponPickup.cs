using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Cluckstorm.Weapons;
using Cluckstorm.Player;

namespace Cluckstorm.Pickups
{
    /// <summary>
    /// Weapon pickup system for Cluckstorm.
    /// Allows players to pick up and drop weapons in the world.
    /// Synchronizes pickup state across network using FishNet.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(NetworkObject))]
    public class WeaponPickup : NetworkBehaviour
    {
        [SerializeField] private BaseWeapon weaponPrefab;
        [SerializeField] private float pickupDistance = 1f;

        private Rigidbody rb;
        private Collider pickupCollider;
        private BaseWeapon currentWeapon;

        [SyncVar] private uint OwnerClientId = uint.MaxValue;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            pickupCollider = GetComponent<Collider>();
            pickupCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider collision)
        {
            if (!IsServer) return;

            // Check if player entered the pickup zone
            PlayerController player = collision.GetComponentInParent<PlayerController>();
            if (player != null && OwnerClientId == uint.MaxValue)
            {
                // Pickup available
                if (player.IsOwner)
                {
                    PickUpWeapon(player);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void PickUpWeapon(PlayerController player)
        {
            if (OwnerClientId != uint.MaxValue) return; // Already picked up

            OwnerClientId = player.GetComponent<NetworkObject>().OwnerId;

            // Move weapon to player
            transform.SetParent(player.transform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            // Disable physics
            rb.isKinematic = true;
            pickupCollider.enabled = false;

            // Enable weapon component
            if (currentWeapon != null)
                currentWeapon.enabled = true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void DropWeapon(PlayerController player)
        {
            OwnerClientId = uint.MaxValue;

            // Return to world
            transform.SetParent(null);
            rb.isKinematic = false;
            pickupCollider.enabled = true;

            // Add drop velocity
            rb.linearVelocity = player.CurrentVelocity;
            rb.AddForce(player.transform.forward * 5f, ForceMode.Impulse);

            // Disable weapon
            if (currentWeapon != null)
                currentWeapon.enabled = false;
        }

        public bool IsEquipped => OwnerClientId != uint.MaxValue;
        public BaseWeapon GetWeapon => currentWeapon;
    }
}
