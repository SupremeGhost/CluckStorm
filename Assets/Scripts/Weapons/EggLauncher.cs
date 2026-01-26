using UnityEngine;
using FishNet.Object;
using Cluckstorm.Weapons;

namespace Cluckstorm.Weapons.Implementations
{
    /// <summary>
    /// Egg Launcher - Projectile weapon with limited ammo.
    /// High recoil, explosive damage, strong knockback for movement plays.
    /// </summary>
    public class EggLauncher : BaseWeapon
    {
        #region Egg Launcher Specific Settings
        [SerializeField] private GameObject eggProjectilePrefab;
        [SerializeField] private float eggSpeed = 20f;
        [SerializeField] private float explosionRadius = 5f;
        [SerializeField] private int maxEggs = 24;
        #endregion

        protected override void Awake()
        {
            base.Awake();
            
            weaponName = "Egg Launcher";
            damagePerShot = 30f;
            fireRate = 0.5f; // One shot every 0.5 seconds
            magazineSize = maxEggs;
            reloadTime = 3f;
            recoilForce = new Vector3(0f, 15f, 25f); // Very strong recoil for movement
            isFullAuto = false; // Semi-auto

            currentAmmo = magazineSize;
        }

        protected override void ServerFire()
        {
            base.Fire();

            // Spawn projectile
            if (eggProjectilePrefab != null)
            {
                GameObject egg = Instantiate(eggProjectilePrefab, shootPoint.position, Quaternion.identity);
                
                // Setup projectile
                if (egg.TryGetComponent<EggProjectile>(out var projectile))
                {
                    projectile.Initialize(
                        shootPoint.position,
                        shootPoint.forward,
                        eggSpeed,
                        damagePerShot,
                        explosionRadius
                    );
                }
            }

            // Apply massive recoil to player
            ApplyRecoil();
        }
    }
}
