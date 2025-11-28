using System;
using SlopJam.Combat;
using UnityEngine;

namespace SlopJam.Player
{
    public class HealthComponent : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 5;
        [SerializeField] private bool enableInvulnerability = true;
        [SerializeField] private float invulnerabilityDuration = 1f;

        public event Action<int, int> OnHealthChanged;
        public event Action OnDeath;
        public event Action<DamageRequest> OnDamaged;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public bool IsAlive => CurrentHealth > 0;
        public bool IsInvulnerable => enableInvulnerability && Time.time < invulnerableUntil;
        public float InvulnerabilityDuration => enableInvulnerability ? Mathf.Max(0f, invulnerabilityDuration) : 0f;

        private float invulnerableUntil;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            NotifyHealthChanged();
        }

        public void SetMaxHealth(int value, bool healToFull = true)
        {
            maxHealth = Mathf.Max(1, value);
            if (healToFull)
            {
                CurrentHealth = maxHealth;
            }

            NotifyHealthChanged();
        }

        public bool ApplyDamage(DamageRequest request)
        {
            if (!IsAlive)
            {
                return false;
            }

            if (IsInvulnerable)
            {
                return false;
            }

            var damageAmount = Mathf.Max(0, request.Amount);
            if (damageAmount == 0)
            {
                return false;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - damageAmount);
            NotifyHealthChanged();
            OnDamaged?.Invoke(request);

            BeginInvulnerability();

            if (CurrentHealth == 0)
            {
                OnDeath?.Invoke();
            }

            return true;
        }

        private void BeginInvulnerability()
        {
            if (!enableInvulnerability)
            {
                return;
            }

            var duration = Mathf.Max(0f, invulnerabilityDuration);
            if (duration <= 0f)
            {
                return;
            }

            invulnerableUntil = Time.time + duration;
        }

        private void NotifyHealthChanged()
        {
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }
}

