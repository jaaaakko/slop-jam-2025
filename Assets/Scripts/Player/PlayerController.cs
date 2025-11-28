using SlopJam.Combat;
using SlopJam.Core;
using UnityEngine;

namespace SlopJam.Player
{
    [RequireComponent(typeof(PlayerRuntime))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float rotationSmoothTime = 0.05f;
        [SerializeField] private float rotationOffset = -90f; // Adjust if sprite faces Up (90) instead of Right (0)

        private PlayerRuntime runtime;
        private InputService inputService;
        private Vector3 aimDirection = Vector3.up;
        
        private void Awake()
        {
            runtime = GetComponent<PlayerRuntime>();
        }

        private void Start()
        {
            if (ServiceLocator.TryResolve(out InputService resolved))
            {
                inputService = resolved;
                inputService.SetAimOrigin(transform);
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
                // Calculate angle in degrees for 2D rotation (rotate around Z-axis)
                var angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
                // Apply offset
                var targetRotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
                // Use Slerp for smooth rotation
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / rotationSmoothTime);
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

