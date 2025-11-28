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

        private PlayerRuntime runtime;
        private InputService inputService;
        private Vector3 aimDirection = Vector3.up;
        
        private void Awake()
        {
            runtime = GetComponent<PlayerRuntime>();
            if (directionalSprite == null) directionalSprite = GetComponent<DirectionalSprite>();
        }

        private void Start()
        {
            if (ServiceLocator.TryResolve(out InputService resolved))
            {
                inputService = resolved;
                inputService.SetAimOrigin(transform);
            }

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
            var displacement = movement * runtime.Config.moveSpeed * Time.deltaTime;
            transform.position += displacement;

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
    }
}

