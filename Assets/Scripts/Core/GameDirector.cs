using SlopJam.Core;
using SlopJam.Enemies;
using SlopJam.Player;
using SlopJam.UI;
using UnityEngine;

namespace SlopJam.Gameplay
{
    public class GameDirector : MonoBehaviour
    {
        [SerializeField] private PlayerRuntime player;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private HUDController hudController;
        [SerializeField] private GameOverView gameOverView;
        [SerializeField] private Camera gameplayCamera;

        private void Start()
        {
            if (gameplayCamera == null) 
            {
                gameplayCamera = Camera.main;
            }

            if (ServiceLocator.TryResolve(out InputService inputService))
            {
                // Ensure InputService uses the correct camera for this scene
                inputService.SetCamera(gameplayCamera);
            }

            if (player == null)
            {
                Debug.LogError("GameDirector requires a PlayerRuntime reference.");
                enabled = false;
                return;
            }

            if (enemySpawner != null)
            {
                enemySpawner.Initialize(player);
            }

            if (hudController != null)
            {
                hudController.Bind(player);
            }

            if (gameOverView != null)
            {
                gameOverView.Hide();
            }

            player.Health.OnDeath += HandlePlayerDeath;
        }

        private void HandlePlayerDeath()
        {
            Debug.Log("Player died - stopping gameplay loop.");
            Time.timeScale = 0f;
            gameOverView?.Show();
        }
    }
}

