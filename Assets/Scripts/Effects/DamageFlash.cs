using System.Collections;
using SlopJam.Player;
using UnityEngine;

namespace SlopJam.Effects
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(HealthComponent))]
    public class DamageFlash : MonoBehaviour
    {
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField, Min(0.01f)] private float flashDuration = 0.1f;
        [SerializeField] private bool loopDuringInvulnerability = true;

        private SpriteRenderer spriteRenderer;
        private HealthComponent health;
        private Material flashMaterial;
        private Coroutine flashRoutine;
        private WaitForSeconds flashDelay;
        private Color originalColor;
        private int lastHealth;
        private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            health = GetComponent<HealthComponent>();
            originalColor = spriteRenderer.color;
            flashDelay = new WaitForSeconds(Mathf.Max(0.01f, flashDuration));
            
            var shader = Shader.Find("SlopJam/SpriteFlash");
            if (shader != null)
            {
                flashMaterial = new Material(shader);
                spriteRenderer.material = flashMaterial;
            }
        }

        private void Start()
        {
            lastHealth = health.CurrentHealth;
            health.OnHealthChanged += HandleHealthChanged;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnHealthChanged -= HandleHealthChanged;
            }
            
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                ResetFlashVisuals();
            }
            
            if (flashMaterial != null)
            {
                Destroy(flashMaterial);
            }
        }

        private void HandleHealthChanged(int current, int max)
        {
            if (current < lastHealth)
            {
                Flash();
            }
            lastHealth = current;
        }

        private void Flash()
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                ResetFlashVisuals();
            }
            flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            do
            {
                ApplyFlashVisuals();
                yield return flashDelay;
                ResetFlashVisuals();
                yield return flashDelay;
            }
            while (loopDuringInvulnerability && health.IsInvulnerable);

            flashRoutine = null;
        }

        private void ApplyFlashVisuals()
        {
            if (flashMaterial != null)
            {
                flashMaterial.SetColor(FlashColorId, flashColor);
                flashMaterial.SetFloat(FlashAmountId, 1f);
            }
            else
            {
                spriteRenderer.color = flashColor;
            }
        }

        private void ResetFlashVisuals()
        {
            if (flashMaterial != null)
            {
                flashMaterial.SetFloat(FlashAmountId, 0f);
            }
            else
            {
                spriteRenderer.color = originalColor;
            }
        }
    }
}

