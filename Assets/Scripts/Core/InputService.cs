using System;
using UnityEngine;

namespace SlopJam.Core
{
    public class InputService : MonoBehaviour
    {
        public event Action<Vector2> OnMove;
        public event Action<Vector2> OnAim;
        public event Action<bool> OnShoot;

        public Vector2 Move { get; private set; }
        public Vector2 Aim { get; private set; }
        public bool IsShooting { get; private set; }

        [SerializeField] private Transform aimOrigin;
        private Camera explicitCamera;

        public void SetAimOrigin(Transform origin)
        {
            aimOrigin = origin;
        }

        public void SetCamera(Camera camera)
        {
            explicitCamera = camera;
        }

        private void Update()
        {
            ReadMoveInput();
            ReadAimInput();
            ReadShootInput();
        }

        private void ReadMoveInput()
        {
            var move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if ((move - Move).sqrMagnitude > 0.0001f)
            {
                Move = Vector2.ClampMagnitude(move, 1f);
                OnMove?.Invoke(Move);
            }
        }

        private void ReadAimInput()
        {
            if (aimOrigin == null)
            {
                return;
            }

            var camera = explicitCamera != null ? explicitCamera : Camera.main;
            if (camera == null)
            {
                return;
            }

            var mouse = Input.mousePosition;
            // For ScreenToWorldPoint, we need to set Z to the distance from camera to the target plane
            // For orthographic cameras, this is typically the distance along the camera's forward axis
            mouse.z = Mathf.Abs(camera.transform.position.z - aimOrigin.position.z);

            var world = camera.ScreenToWorldPoint(mouse);
            var direction = world - aimOrigin.position;
            direction.z = 0f;

            var aim = new Vector2(direction.x, direction.y);
            if (aim.sqrMagnitude < 0.0001f)
            {
                return;
            }

            aim.Normalize();
            if ((aim - Aim).sqrMagnitude > 0.0001f)
            {
                Aim = aim;
                OnAim?.Invoke(Aim);
            }
        }

        private void ReadShootInput()
        {
            var shooting = Input.GetMouseButton(0);
            if (shooting != IsShooting)
            {
                IsShooting = shooting;
                OnShoot?.Invoke(IsShooting);
            }
        }
    }
}

