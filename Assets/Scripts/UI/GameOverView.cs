using SlopJam.Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace SlopJam.UI
{
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] private HUDDocumentView documentView;
        [SerializeField] private UnityEvent onShown;
        [SerializeField] private UnityEvent onHidden;

        private void Start()
        {
            var player = FindFirstObjectByType<PlayerRuntime>();
            if (player != null)
            {
                player.Health.OnDeath += HandlePlayerDeath;
            }

            if (documentView != null)
            {
                documentView.RetryRequested += HandleRetry;
                documentView.QuitRequested += HandleQuit;
            }

            Hide();
        }

        private void HandleRetry()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("GameBootstrap");
        }

        private void HandleQuit()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        private void HandlePlayerDeath()
        {
            Debug.Log("Player died - stopping gameplay loop.");
            Time.timeScale = 0f;
            Show();
        }

        public void Show()
        {
            documentView?.ShowGameOver();
            onShown?.Invoke();
        }

        public void Hide()
        {
            documentView?.HideGameOver();
            onHidden?.Invoke();
        }
    }
}

