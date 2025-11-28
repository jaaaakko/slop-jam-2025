using System.Collections;
using SlopJam.Combat;
using SlopJam.Core;
using SlopJam.Player;
using UnityEngine;

namespace SlopJam.Enemies
{
    [RequireComponent(typeof(HealthComponent))]
    public class EnemyBrain : MonoBehaviour, IKnockbackable
    {
        [SerializeField] private EnemyConfig config;
        [SerializeField] private float stoppingDistance = 0.75f;
        [SerializeField] private float knockbackDamping = 5f;

        private Transform target;
        private HealthComponent health;
        private DamageSystem damageSystem;
        private bool canAttack = true;
        private Vector3 externalVelocity;

        public void SetTarget(Transform player)
        {
            target = player;
        }

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
            health.SetMaxHealth(config != null ? config.maxHealth : 1);
        }

        private void Start()
        {
            ServiceLocator.TryResolve(out damageSystem);
        }

        private void Update()
        {
            if (target == null || config == null || !health.IsAlive)
            {
                return;
            }

            if (externalVelocity.sqrMagnitude > 0.0001f)
            {
                externalVelocity = Vector3.MoveTowards(externalVelocity, Vector3.zero, knockbackDamping * Time.deltaTime);
            }

            var direction = (target.position - transform.position);
            direction.z = 0f; // 2D Game uses XY plane
            var distance = direction.magnitude;

            Vector3 voluntaryMovement = Vector3.zero;

            if (distance > stoppingDistance)
            {
                voluntaryMovement = direction.normalized * config.moveSpeed * Time.deltaTime;
                
                // Rotate to face player (2D)
                if (direction.sqrMagnitude > 0.001f)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    // Assuming sprite faces Right by default. If Up, subtract 90.
                    transform.rotation = Quaternion.Euler(0f, 0f, angle);
                }
            }
            else if (canAttack)
            {
                AttemptAttack();
            }

            transform.position += voluntaryMovement + (externalVelocity * Time.deltaTime);
        }

        public void ApplyKnockback(Vector3 impulse)
        {
            externalVelocity += impulse;
        }

        private void AttemptAttack()
        {
            if (target.TryGetComponent(out HealthComponent targetHealth))
            {
                var request = new DamageRequest(targetHealth, config.damage, gameObject, target.position);
                _ = damageSystem?.ApplyDamage(request);
            }

            StartCoroutine(AttackCooldown());
        }

        private IEnumerator AttackCooldown()
        {
            canAttack = false;
            yield return new WaitForSeconds(config.attackCooldown);
            canAttack = true;
        }
    }
}

