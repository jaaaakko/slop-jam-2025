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

        private void TryDamage(Collider2D other)
        {
            if (!other.TryGetComponent(out HealthComponent health))
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

            if (other.TryGetComponent(out PlayerController player))
            {
                ApplyKnockback(player.transform, player);
            }
        }

        private void ApplyKnockback(Transform targetTransform, PlayerController player)
        {
            var center = centerOverride != null ? centerOverride.position : Vector3.zero;
            var direction = (center - targetTransform.position).normalized;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.up;
            }

            player.ApplyKnockback(direction * knockbackForce);
        }

        private void SetupCollider()
        {
            hazardCollider = GetComponent<Collider2D>();
            if (hazardCollider == null)
            {
                hazardCollider = gameObject.AddComponent<BoxCollider2D>();
            }
            hazardCollider.isTrigger = true;
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

