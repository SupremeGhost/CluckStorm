using UnityEngine;

namespace Cluckstorm.Weapons
{
    /// <summary>
    /// Interface for objects that can be damaged by weapons.
    /// Used by enemies, players, and destructible objects.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float damage);
    }
}
