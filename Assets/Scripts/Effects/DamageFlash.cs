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
        [SerializeField] private float flashDuration = 0.1f;

        private SpriteRenderer spriteRenderer;
        private HealthComponent health;
        private Material flashMaterial;
        private Coroutine flashRoutine;
        private int lastHealth;
        private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            health = GetComponent<HealthComponent>();
            
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
                // Reset
                if (flashMaterial != null) flashMaterial.SetFloat(FlashAmountId, 0f);
                else spriteRenderer.color = Color.white; // Assuming white is default tint
            }
            flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            if (flashMaterial != null)
            {
                flashMaterial.SetColor(FlashColorId, flashColor);
                flashMaterial.SetFloat(FlashAmountId, 1f);
                yield return new WaitForSeconds(flashDuration);
                flashMaterial.SetFloat(FlashAmountId, 0f);
            }
            else
            {
                // Fallback
                var prevColor = spriteRenderer.color;
                spriteRenderer.color = flashColor;
                yield return new WaitForSeconds(flashDuration);
                spriteRenderer.color = prevColor;
            }
            flashRoutine = null;
        }
    }
}

