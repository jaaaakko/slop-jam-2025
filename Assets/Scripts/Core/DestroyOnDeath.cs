using SlopJam.Player;
using UnityEngine;

namespace SlopJam.Core
{
    [RequireComponent(typeof(HealthComponent))]
    public class DestroyOnDeath : MonoBehaviour
    {
        [SerializeField] private float delay = 0f;

        private void Start()
        {
            var health = GetComponent<HealthComponent>();
            health.OnDeath += HandleDeath;
        }

        private void HandleDeath()
        {
            Destroy(gameObject, delay);
        }
    }
}

