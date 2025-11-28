using SlopJam.Combat;
using SlopJam.Core;
using SlopJam.Player;
using UnityEngine;

namespace SlopJam.Hazards
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class SpikeWallDamage : MonoBehaviour
    {
        [SerializeField] private int damage = 1;
        [SerializeField] private float knockbackForce = 10f;
        [SerializeField] private Transform centerOverride;
        [SerializeField] private bool shareCenterFromParent = true;

        private DamageSystem damageSystem;
        private Collider2D hazardCollider;
        private Rigidbody2D hazardBody;

        private void Reset()
        {
            SetupCollider();
            SetupBody();
        }

        private void Awake()
        {
            SetupCollider();
            SetupBody();

            if (shareCenterFromParent && centerOverride == null)
            {
                centerOverride = transform.parent;
            }
        }

        private void Start()
        {
            if (!ServiceLocator.TryResolve(out damageSystem))
            {
                damageSystem = FindFirstObjectByType<DamageSystem>();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryDamage(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDamage(other);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryDamage(collision.collider, collision.GetContact(0).normal);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryDamage(collision.collider, collision.GetContact(0).normal);
        }

        private void TryDamage(Collider2D other, Vector3? overrideDirection = null)
        {
            // Check on object or parent for HealthComponent
            HealthComponent health = other.GetComponent<HealthComponent>();
            if (health == null)
            {
                health = other.GetComponentInParent<HealthComponent>();
            }

            if (health == null)
            {
                return;
            }

            var request = new DamageRequest(health, damage, gameObject, other.ClosestPoint(transform.position));
            var applied = damageSystem != null
                ? damageSystem.ApplyDamage(request)
                : health.ApplyDamage(request);

            if (!applied)
            {
                return;
            }

            // Check on object or parent for IKnockbackable
            IKnockbackable knockbackable = other.GetComponent<IKnockbackable>();
            if (knockbackable == null)
            {
                knockbackable = other.GetComponentInParent<IKnockbackable>();
            }

            if (knockbackable != null)
            {
                ApplyKnockback(health.transform, knockbackable, overrideDirection);
            }
        }

        private void ApplyKnockback(Transform targetTransform, IKnockbackable target, Vector3? overrideDirection)
        {
            Vector3 direction;
            
            if (overrideDirection.HasValue)
            {
                direction = overrideDirection.Value;
            }
            else
            {
                var center = centerOverride != null ? centerOverride.position : transform.position;
                // Direction from center TO target (push away)
                direction = (targetTransform.position - center).normalized;
            }

            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.up;
            }

            target.ApplyKnockback(direction * knockbackForce);
        }

        private void SetupCollider()
        {
            hazardCollider = GetComponent<Collider2D>();
            if (hazardCollider == null)
            {
                hazardCollider = gameObject.AddComponent<BoxCollider2D>();
            }
            // Ensure it is solid, not a trigger, to prevent walking through
            hazardCollider.isTrigger = false;
        }

        private void SetupBody()
        {
            hazardBody = GetComponent<Rigidbody2D>();
            if (hazardBody == null)
            {
                hazardBody = gameObject.AddComponent<Rigidbody2D>();
            }
            hazardBody.bodyType = RigidbodyType2D.Kinematic;
            hazardBody.gravityScale = 0f;
            hazardBody.constraints = RigidbodyConstraints2D.FreezeRotation;
            hazardBody.useFullKinematicContacts = true;
        }
    }
}

