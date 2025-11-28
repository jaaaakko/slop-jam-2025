using SlopJam.Player;
using UnityEngine;

namespace SlopJam.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 3f;

        private Rigidbody2D rb;
        private DamageSystem damageSystem;
        private int damage;
        private GameObject instigator;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(int damageAmount, float speed, DamageSystem system, GameObject source)
        {
            damage = damageAmount;
            damageSystem = system;
            instigator = source;
            rb.linearVelocity = (Vector2)transform.up * speed;
            Destroy(gameObject, lifeTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject == instigator)
            {
                return;
            }

            if (other.TryGetComponent(out HealthComponent health))
            {
                var request = new DamageRequest(health, damage, instigator, transform.position);
                _ = damageSystem?.ApplyDamage(request);
            }

            Destroy(gameObject);
        }
    }
}

