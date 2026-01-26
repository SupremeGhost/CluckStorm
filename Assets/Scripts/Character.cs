using UnityEngine;

namespace Cluckstorm.Characters
{
    /// <summary>
    /// Defines character physics modifiers for Cluckstorm.
    /// All characters are armed chickens with physics properties.
    /// </summary>
    [System.Serializable]
    public class CharacterPhysicsModifier
    {
        [SerializeField] public float MassMultiplier = 1f;
        [SerializeField] public float RecoilForceMultiplier = 1f;
        [SerializeField] public float MomentumMultiplier = 1f;
        [SerializeField] public float FallDamageMultiplier = 1f;
        [SerializeField] public float ControlSensitivity = 1f;
    }

    public enum CharacterType
    {
        DefaultChicken
    }

    /// <summary>
    /// Manages character selection for the player.
    /// </summary>
    public class Character : MonoBehaviour
    {
        [SerializeField] private CharacterType characterType = CharacterType.DefaultChicken;
        [SerializeField] private float baseHealth = 100f;

        private CharacterPhysicsModifier physicsModifier;
        private float currentHealth;
        private bool isDead = false;

        #region Properties
        public CharacterType Type => characterType;
        public CharacterPhysicsModifier PhysicsModifier => physicsModifier;
        public float Health => currentHealth;
        public bool IsDead => isDead;
        #endregion

        #region MonoBehaviour Callbacks
        private void Awake()
        {
            InitializeCharacter();
            currentHealth = baseHealth;
        }
        #endregion

        #region Character Setup
        private void InitializeCharacter()
        {
            physicsModifier = GetPhysicsModifier(characterType);
        }

        public static CharacterPhysicsModifier GetPhysicsModifier(CharacterType type)
        {
            return new CharacterPhysicsModifier
            {
                MassMultiplier = 1f,
                RecoilForceMultiplier = 1f,
                MomentumMultiplier = 1f,
                FallDamageMultiplier = 1f,
                ControlSensitivity = 1f
            };
        }
        #endregion

        #region Health Management
        public void TakeDamage(float damage)
        {
            if (isDead) return;

            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (isDead) return;
            currentHealth = Mathf.Min(currentHealth + amount, baseHealth);
        }

        private void Die()
        {
            isDead = true;
            gameObject.SetActive(false);
        }
        #endregion
    }
}
