using SlopJam.Combat;
using SlopJam.Core;
using UnityEngine;

namespace SlopJam.Player
{
    [RequireComponent(typeof(PlayerRuntime))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float rotationSmoothTime = 0.05f;
        [SerializeField] private float rotationOffset = 0f; // Adjust if sprite faces Up (90) instead of Right (0)

        [Header("Aiming")]
        [SerializeField] private Transform aimPivot;
        [SerializeField] private DirectionalSprite directionalSprite;

        [Header("Knockback")]
        [SerializeField] private float knockbackDamping = 8f;

        private PlayerRuntime runtime;
        private InputService inputService;
        private Vector3 aimDirection = Vector3.up;
        private Vector3 externalVelocity = Vector3.zero;
        
        private void Awake()
        {
            runtime = GetComponent<PlayerRuntime>();
            if (directionalSprite == null) directionalSprite = GetComponent<DirectionalSprite>();
            if (knockbackDamping <= 0f)
            {
                knockbackDamping = 8f;
            }
        }

        private void Start()
        {
            TryResolveInputService();

            // Fallback: try to find Muzzle if aimPivot is missing
            if (aimPivot == null && runtime.Weapon != null)
            {
                // This assumes WeaponController has a reference to Muzzle, but we can't access it easily if it's private.
                // Let's just look for a child named "Muzzle"
                aimPivot = transform.Find("Muzzle");
            }
        }

        private void Update()
        {
            if (inputService == null)
            {
                TryResolveInputService();
            }

            if (inputService == null || runtime.Config == null)
            {
                return;
            }

            HandleMovement();
            HandleShooting();
        }

        private void HandleMovement()
        {
            var moveInput = inputService.Move;
            var movement = new Vector3(moveInput.x, moveInput.y, 0f);
            var baseVelocity = movement * runtime.Config.moveSpeed;
            var displacement = (baseVelocity + externalVelocity) * Time.deltaTime;
            transform.position += displacement;

            if (externalVelocity.sqrMagnitude > 0.0001f)
            {
                externalVelocity = Vector3.MoveTowards(externalVelocity, Vector3.zero, knockbackDamping * Time.deltaTime);
            }

            var aim = inputService.Aim;
            if (aim.sqrMagnitude > 0.0001f)
            {
                aimDirection = new Vector3(aim.x, aim.y, 0f).normalized;
                
                if (directionalSprite != null)
                {
                    directionalSprite.SetDirection(aimDirection);
                }

                if (aimPivot != null)
                {
                    // Rotate the pivot (weapon) instead of the whole player
                    var angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
                    var targetRotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
                    aimPivot.rotation = Quaternion.Slerp(aimPivot.rotation, targetRotation, Time.deltaTime / rotationSmoothTime);
                    
                    // Keep player body rotation fixed (optional, but good for top-down sprites)
                    transform.rotation = Quaternion.identity;
                }
                else
                {
                    // Fallback: Rotate the whole player if no pivot assigned (old behavior)
                    var angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
                    var targetRotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / rotationSmoothTime);
                }
            }
        }

        private void HandleShooting()
        {
            if (!inputService.IsShooting || runtime.Weapon == null)
            {
                return;
            }

            var direction = aimDirection.sqrMagnitude > 0.0001f ? aimDirection : transform.up;
            runtime.Weapon.TryShoot(direction);
        }

        public void ApplyKnockback(Vector3 impulse)
        {
            externalVelocity += impulse;
        }

        private void TryResolveInputService()
        {
            if (ServiceLocator.TryResolve(out InputService resolved))
            {
                inputService = resolved;
            }
            else
            {
                inputService = FindFirstObjectByType<InputService>();
            }

            if (inputService != null)
            {
                inputService.SetAimOrigin(transform);
            }
        }
    }
}

