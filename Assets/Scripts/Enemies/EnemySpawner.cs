using System.Collections.Generic;
using SlopJam.Player;
using UnityEngine;

namespace SlopJam.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyBrain enemyPrefab;
        [SerializeField] private EnemyConfig enemyConfig;
        [SerializeField] private List<Transform> spawnPoints = new();
        [SerializeField] private int simultaneousEnemies = 3;

        private readonly List<EnemyBrain> activeEnemies = new();
        private PlayerRuntime player;

        public void Initialize(PlayerRuntime playerRuntime)
        {
            player = playerRuntime;
        }

        private void Start()
        {
            if (player == null)
            {
                player = FindFirstObjectByType<PlayerRuntime>();
                if (player == null)
                {
                    Debug.LogError("[EnemySpawner] PlayerRuntime not found in scene!");
                }
                else
                {
                    Debug.Log("[EnemySpawner] PlayerRuntime found and assigned.");
                }
            }

            if (spawnPoints.Count == 0)
            {
                Debug.LogError("[EnemySpawner] No spawn points configured!");
            }

            if (enemyPrefab == null)
            {
                Debug.LogError("[EnemySpawner] Enemy Prefab is missing!");
            }

            if (enemyConfig == null)
            {
                Debug.LogError("[EnemySpawner] Enemy Config is missing!");
            }
        }

        private void Update()
        {
            if (player == null || enemyPrefab == null || enemyConfig == null)
            {
                return;
            }

            CleanupDeadEnemies();
            
            // Prevent infinite loop: Stop attempting to spawn this frame if we fail to spawn one.
            // Also add a safety cap to attempts per frame.
            int attempts = 0;
            while (activeEnemies.Count < simultaneousEnemies && attempts < simultaneousEnemies * 2)
            {
                attempts++;
                if (!SpawnEnemy())
                {
                    break;
                }
            }
        }

        private bool SpawnEnemy()
        {
            if (spawnPoints.Count == 0)
            {
                Debug.LogWarning("EnemySpawner has no spawn points configured.");
                return false;
            }

            // Try up to 10 times to find a valid spawn point
            for (int i = 0; i < 10; i++)
            {
                var point = spawnPoints[Random.Range(0, spawnPoints.Count)];
                // Simple check to ensure we don't spawn inside a solid wall.
                // Assuming enemy radius is roughly 0.4f.
                // We filter out triggers (like damage zones) if we only care about getting stuck in solids.
                var hit = Physics2D.OverlapCircle(point.position, 0.4f);
                bool isSolid = hit != null && !hit.isTrigger;

                if (!isSolid)
                {
                    var enemy = Instantiate(enemyPrefab, point.position, point.rotation, transform);
                    enemy.SetTarget(player.transform);
                    activeEnemies.Add(enemy);
                    return true;
                }
            }

            Debug.LogWarning("[EnemySpawner] Could not find empty space to spawn enemy after multiple attempts.");
            return false;
        }

        private void CleanupDeadEnemies()
        {
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i] == null || !activeEnemies[i].gameObject.activeInHierarchy)
                {
                    activeEnemies.RemoveAt(i);
                }
            }
        }
    }
}

