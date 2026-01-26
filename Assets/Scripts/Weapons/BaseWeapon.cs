using UnityEngine;
using Cluckstorm.Input;
using Cluckstorm.Player;

namespace Cluckstorm.Weapons
{
    /// <summary>
    /// Base class for all weapons in Cluckstorm.
    /// Handles firing, reloading, ammo management, and recoil application.
    /// Each weapon type (Glock) inherits from this.
    /// </summary>
    public abstract class BaseWeapon : MonoBehaviour
    {
        #region Weapon Stats
        [SerializeField] protected string weaponName = "Weapon";
        [SerializeField] protected float damagePerShot = 10f;
        [SerializeField] protected float fireRate = 0.1f; // Time between shots
        [SerializeField] protected int magazineSize = 30;
        [SerializeField] protected float reloadTime = 2f;
        [SerializeField] protected Vector3 recoilForce = Vector3.zero;

        protected int currentAmmo;
        protected float lastFireTime = -1f;
        protected bool isReloading = false;
        #endregion

        #region References
        protected PlayerController playerController;
        protected InputManager inputManager;
        protected Transform shootPoint;
        [SerializeField] protected ParticleSystem muzzleFlash;
        #endregion

        #region Firing Type
        [SerializeField] protected bool isFullAuto = false;
        #endregion

        #region Properties
        public int CurrentAmmo => currentAmmo;
        public int MagazineSize => magazineSize;
        public bool IsReloading => isReloading;
        public string WeaponName => weaponName;
        #endregion

        #region MonoBehaviour Callbacks
        protected virtual void Awake()
        {
            playerController = GetComponentInParent<PlayerController>();
            inputManager = InputManager.Instance;
            shootPoint = GetComponentInChildren<Transform>(); // Usually "ShootPoint" child

            currentAmmo = magazineSize;
        }

        protected virtual void Update()
        {
            if (!IsOwner) return;

            HandleInput();
        }
        #endregion

        #region Input Handling
        private void HandleInput()
        {
            // Reload
            if (inputManager.ReloadPressed && !isReloading && currentAmmo < magazineSize)
            {
                Reload();
            }

            // Fire
            if (isFullAuto && inputManager.FireHeld && CanFire())
            {
                Fire();
            }
            else if (!isFullAuto && inputManager.FirePressed && CanFire())
            {
                Fire();
            }
        }
        #endregion

        #region Firing
        protected bool CanFire()
        {
            return !isReloading && currentAmmo > 0 && Time.time - lastFireTime >= fireRate;
        }

        protected virtual void Fire()
        {
            if (!CanFire()) return;

            lastFireTime = Time.time;
            currentAmmo--;

            // Process the shot locally
            ProcessFire();

            // Visual feedback on client
            PlayMuzzleFlash();
        }

        protected abstract void ProcessFire();

        protected virtual void PlayMuzzleFlash()
        {
            if (muzzleFlash != null)
                muzzleFlash.Play();
        }
        #endregion

        #region Reloading
        protected virtual void Reload()
        {
            if (isReloading || currentAmmo == magazineSize) return;

            isReloading = true;
            Invoke(nameof(FinishReload), reloadTime);
        }

        private void FinishReload()
        {
            currentAmmo = magazineSize;
            isReloading = false;
        }
        #endregion

        #region Recoil
        protected void ApplyRecoil()
        {
            // Get direction opposite to aim
            Vector3 recoilDirection = -shootPoint.forward;
            Vector3 finalRecoil = recoilDirection * recoilForce.magnitude;

            // Apply character physics modifier
            float modifier = playerController.GetComponent<Characters.Character>().PhysicsModifier.RecoilForceMultiplier;
            playerController.ApplyRecoil(finalRecoil * modifier);
        }
        #endregion

        #region Getters/Setters
        public void SetShootPoint(Transform newShootPoint)
        {
            shootPoint = newShootPoint;
        }
        #endregion
    }
}
