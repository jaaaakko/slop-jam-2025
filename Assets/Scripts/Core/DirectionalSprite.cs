using UnityEngine;

namespace SlopJam.Core
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class DirectionalSprite : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] private Sprite spriteUp;
        [SerializeField] private Sprite spriteDown;
        [SerializeField] private Sprite spriteLeft;
        [SerializeField] private Sprite spriteRight;

        [Header("Settings")]
        [SerializeField] private Transform rotationTarget; // Optional: object to read rotation from
        [SerializeField] private bool useVelocity; // Optional: use Rigidbody velocity instead of rotation
        [SerializeField] private bool autoUpdateFromSelf = true; // If true, use own rotation when other targets are missing
        [SerializeField] private float velocityThreshold = 0.1f;

        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private Vector2 lastDirection = Vector2.right;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponentInParent<Rigidbody2D>();
        }

        private void LateUpdate()
        {
            Vector2 direction = Vector2.zero;

            if (useVelocity && rb != null)
            {
                if (rb.linearVelocity.sqrMagnitude > velocityThreshold * velocityThreshold)
                {
                    direction = rb.linearVelocity.normalized;
                }
            }
            else if (rotationTarget != null)
            {
                // Convert Z-rotation to direction vector
                float angleRad = rotationTarget.eulerAngles.z * Mathf.Deg2Rad;
                direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            }
            else if (autoUpdateFromSelf)
            {
                // Default to own transform rotation
                float angleRad = transform.eulerAngles.z * Mathf.Deg2Rad;
                direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
                // Keep the sprite upright visually
                transform.rotation = Quaternion.identity;
            }

            if (direction != Vector2.zero)
            {
                lastDirection = direction;
                UpdateSprite(direction);
            }
        }

        public void SetDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.001f)
            {
                lastDirection = direction.normalized;
                UpdateSprite(lastDirection);
            }
        }

        private void UpdateSprite(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;

            // 0 is Right. 90 is Up. 180 is Left. 270 is Down.
            // Right: 315 to 45
            // Up: 45 to 135
            // Left: 135 to 225
            // Down: 225 to 315

            Sprite targetSprite = spriteRight; // Default

            if (angle > 45f && angle <= 135f)
            {
                targetSprite = spriteUp != null ? spriteUp : spriteRight;
            }
            else if (angle > 135f && angle <= 225f)
            {
                targetSprite = spriteLeft != null ? spriteLeft : spriteRight;
            }
            else if (angle > 225f && angle <= 315f)
            {
                targetSprite = spriteDown != null ? spriteDown : spriteRight;
            }

            if (targetSprite != null && spriteRenderer.sprite != targetSprite)
            {
                spriteRenderer.sprite = targetSprite;
            }
        }
    }
}

