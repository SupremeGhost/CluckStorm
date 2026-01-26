using UnityEngine;
using Cluckstorm.Weapons;

namespace Cluckstorm.Weapons.Implementations
{
    /// <summary>
    /// Glock 18 - Hitscan weapon with infinite ammo.
    /// Fast fire rate, medium recoil, core movement tool.
    /// </summary>
    public class Glock18 : BaseWeapon
    {
        #region Glock Specific Settings
        [SerializeField] private float raycastDistance = 1000f;
        [SerializeField] private LayerMask hitLayer = -1;
        #endregion

        protected override void Awake()
        {
            base.Awake();
            
            weaponName = "Glock 18";
            damagePerShot = 15f;
            fireRate = 0.08f; // ~12-13 rounds per second (automatic)
            magazineSize = int.MaxValue; // Infinite ammo
            reloadTime = 0f; // No reload needed
            recoilForce = new Vector3(0f, 5f, 10f); // Moderate recoil
            isFullAuto = true;

            currentAmmo = magazineSize;
        }

        protected override void ProcessFire()
        {
            // Hitscan raycast
            RaycastHit hit;
            if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, raycastDistance, hitLayer))
            {
                // Deal damage to hit object
                if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(damagePerShot);
                }

                // Create impact effect at hit point
                // (Pool bullets/effects here)
            }

            // Apply recoil to player
            ApplyRecoil();
        }
    }
}
