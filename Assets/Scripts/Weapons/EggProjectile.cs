using UnityEngine;

namespace Cluckstorm.Weapons
{
    /// <summary>
    /// Egg projectile for the Egg Launcher.
    /// Parabolic trajectory with gravity and explosive damage.
    /// </summary>
    public class EggProjectile : MonoBehaviour
    {
        private Vector3 startPosition;
        private Vector3 velocity;
        private float damage;
        private float explosionRadius;
        private float lifetime = 10f;
        private float creationTime;

        private const float GRAVITY = 9.81f;

        public void Initialize(Vector3 position, Vector3 direction, float speed, float damageAmount, float radius)
        {
            startPosition = position;
            velocity = direction * speed;
            damage = damageAmount;
            explosionRadius = radius;
            creationTime = Time.time;

            transform.position = position;
            transform.rotation = Quaternion.LookRotation(direction);
        }

        private void FixedUpdate()
        {
            // Apply gravity
            velocity.y -= GRAVITY * Time.fixedDeltaTime;

            // Update position
            transform.position += velocity * Time.fixedDeltaTime;

            // Rotate to face direction of travel
            if (velocity.magnitude > 0)
                transform.rotation = Quaternion.LookRotation(velocity);

            // Check lifetime
            if (Time.time - creationTime > lifetime)
            {
                Explode();
            }
        }

        private void OnTriggerEnter(Collider collision)
        {
            // Don't explode on trigger zones, only on colliders
            if (collision.isTrigger) return;

            // Don't hit the shooter immediately
            if (collision.GetComponentInParent<PlayerController>() != null)
            {
                // Check if it's the owner (could improve with network ownership check)
                return;
            }

            Explode();
        }

        private void Explode()
        {
            // Damage all objects in radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

            foreach (Collider col in colliders)
            {
                if (col.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(damage);
                }

                // Apply knockback
                if (col.TryGetComponent<Rigidbody>(out var rb))
                {
                    Vector3 direction = (col.transform.position - transform.position).normalized;
                    rb.AddForce(direction * damage * 2f, ForceMode.Impulse);
                }
            }

            // Destroy projectile
            Destroy(gameObject);
        }
    }
}
