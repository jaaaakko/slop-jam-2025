using SlopJam.Player;
using UnityEngine;

namespace SlopJam.Hazards
{
    public class ClosingSpikeWalls : MonoBehaviour
    {
        [SerializeField] private Transform leftWall;
        [SerializeField] private Transform rightWall;
        [SerializeField] private float maxGap = 14f;
        [SerializeField] private float minGap = 3.5f;
        [SerializeField] private float cycleDuration = 5f;
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private AnimationCurve gapCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [SerializeField] private Transform centerOverride;
        [SerializeField] private bool followPlayerCenter = true;

        private Transform player;
        private Vector3 defaultCenter;

        private void Awake()
        {
            if (leftWall == null || rightWall == null)
            {
                Debug.LogError("ClosingSpikeWalls requires references to both left and right wall transforms.", this);
                enabled = false;
                return;
            }

            defaultCenter = (leftWall.position + rightWall.position) * 0.5f;
            cycleDuration = Mathf.Max(0.1f, cycleDuration);
            maxGap = Mathf.Max(minGap + 0.01f, maxGap);
        }

        private void Start()
        {
            player = FindFirstObjectByType<PlayerRuntime>()?.transform;
        }

        private void Update()
        {
            var center = ResolveCenter();
            var normalizedTime = Mathf.PingPong(Time.time / cycleDuration, 1f);
            var gapFactor = gapCurve.Evaluate(normalizedTime);
            var currentGap = Mathf.Lerp(minGap, maxGap, gapFactor);

            var leftTarget = leftWall.position;
            leftTarget.x = center.x - currentGap * 0.5f;

            var rightTarget = rightWall.position;
            rightTarget.x = center.x + currentGap * 0.5f;

            leftWall.position = Vector3.MoveTowards(leftWall.position, leftTarget, followSpeed * Time.deltaTime);
            rightWall.position = Vector3.MoveTowards(rightWall.position, rightTarget, followSpeed * Time.deltaTime);
        }

        private Vector3 ResolveCenter()
        {
            if (centerOverride != null)
            {
                return centerOverride.position;
            }

            if (followPlayerCenter && player != null)
            {
                var center = defaultCenter;
                center.x = player.position.x;
                return center;
            }

            return defaultCenter;
        }
    }
}

