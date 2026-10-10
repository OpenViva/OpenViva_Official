using System;
using UnityEngine;

namespace Viva.Interaction
{
    public enum HandSide { Any, Left, Right }

    public class SmartTargetDetector : MonoBehaviour
    {
        [SerializeField] private DesktopPointerSource desktopPointerComponent;
        [SerializeField] private VRPointerSource leftHandVRPointerComponent;
        [SerializeField] private VRPointerSource rightHandVRPointerComponent;

        public Transform leftHandTransform;
        public Transform rightHandTransform;

        [Header("Performance & Scan Settings")]
        [SerializeField, Tooltip("Scan interval in seconds. 0.05 = 20 scans/sec")]
        private float scanInterval = 0.05f;
        [SerializeField] private float maxDistance = 4f;
        [SerializeField] private float sphereCastRadius = 0.4f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private LayerMask obstacleLayer;

        [Header("Scoring Weights")]
        [Range(0f, 1f)][SerializeField] private float angleWeight = 0.7f;
        [Range(0f, 1f)][SerializeField] private float distanceWeight = 0.3f;

        [Header("Buffer Limits")]
        [SerializeField] private int maxHitBuffer = 16;

        [Header("Gizmos Debug Settings")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private Color rayColor = new Color(0f, 0.8f, 1f, 0.5f); // Cyan
        [SerializeField] private Color candidateColor = Color.red;
        [SerializeField] private Color selectedTargetColor = Color.green;

        public GameObject CurrentTarget { get; private set; }
        public event Action<GameObject> OnTargetChanged;

        private IPointerSource desktopPointer;
        private IPointerSource leftHandPointer;
        private IPointerSource rightHandPointer;

        private RaycastHit[] hitBuffer;
        private float scanTimer;

        private void Awake()
        {
            hitBuffer = new RaycastHit[maxHitBuffer];
            desktopPointer = desktopPointerComponent as IPointerSource;
            leftHandPointer = leftHandVRPointerComponent as IPointerSource;
            rightHandPointer = rightHandVRPointerComponent as IPointerSource;
        }

        private void Update()
        {
            scanTimer += Time.deltaTime;
            if (scanTimer >= scanInterval)
            {
                scanTimer = 0f;
                EvaluateContinuousTarget();
            }
        }

        public IPointerSource GetActivePointer(HandSide hand = HandSide.Any)
        {
            if (Globals.isDesktopMode) return desktopPointer;

            return hand switch
            {
                HandSide.Left => leftHandPointer,
                HandSide.Right => rightHandPointer,
                _ => rightHandPointer ?? leftHandPointer
            };
        }

        private void EvaluateContinuousTarget()
        {
            IPointerSource activePointer = GetActivePointer();
            GameObject bestTarget = PerformCast(activePointer);
            SetTarget(bestTarget);
        }

        public GameObject PerformCast(IPointerSource pointer)
        {
            if (pointer == null || !pointer.IsActive) return null;

            Ray ray = pointer.GetRay();
            int hitCount = Physics.SphereCastNonAlloc(ray, sphereCastRadius, hitBuffer, maxDistance, interactableLayer);

            GameObject bestCandidate = null;
            float highestScore = float.MinValue;
            float maxDistanceSqr = maxDistance * maxDistance;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = hitBuffer[i];
                if (hit.collider == null)
                {
                    continue;
                }

                Transform targetTransform = hit.transform;
                Vector3 targetPos = targetTransform.position;
                Vector3 dirToTarget = targetPos - ray.origin;
                float sqrDist = dirToTarget.sqrMagnitude;

                if (sqrDist > maxDistanceSqr || sqrDist <= 0.0001f) continue;

                float distance = Mathf.Sqrt(sqrDist);
                Vector3 dirNoarmalized = dirToTarget / distance;

                // Linen of sight check
                if (Physics.Linecast(ray.origin, targetPos, obstacleLayer)) continue;

                float dot = Vector3.Dot(ray.direction, dirNoarmalized);
                if (dot <= 0f) continue;

                float distScore = 1f - Mathf.Clamp01(distance / maxDistance);
                float score = (dot * angleWeight) + (distScore * distanceWeight);

                if (score > highestScore)
                {
                    highestScore = score;
                    bestCandidate = targetTransform.gameObject;
                }
            }

            return bestCandidate;
        }

        private void SetTarget(GameObject newTarget)
        {
            if (CurrentTarget != newTarget)
            {
                CurrentTarget = newTarget;
                OnTargetChanged?.Invoke(CurrentTarget);
            }
        }

        #region Debug Gizmos
        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            // Resolve active pointer (handles both Edit Mode and Play Mode previewing)
            IPointerSource pointer = null;
            if (Application.isPlaying)
            {
                pointer = GetActivePointer();
            }
            else
            {
                pointer = (desktopPointerComponent as IPointerSource) ?? (rightHandVRPointerComponent as IPointerSource);
            }

            if (pointer == null || !pointer.IsActive) return;

            Ray ray = pointer.GetRay();

            Gizmos.color = rayColor;
            Gizmos.DrawRay(ray.origin, ray.direction * maxDistance);

            // Start and End boundary spheres of the SphereCast
            Gizmos.DrawWireSphere(ray.origin, sphereCastRadius);
            Gizmos.DrawWireSphere(ray.origin + ray.direction * maxDistance, sphereCastRadius);

            if (Application.isPlaying && hitBuffer != null)
            {
                for (int i = 0; i < hitBuffer.Length; i++)
                {
                    RaycastHit hit = hitBuffer[i];
                    if (hit.collider == null) continue;

                    // Skip drawing candidate if it's the winning target
                    if (CurrentTarget != null && hit.transform.gameObject == CurrentTarget) continue;

                    Gizmos.color = candidateColor;
                    Gizmos.DrawWireSphere(hit.transform.position, sphereCastRadius * 0.8f);
                    Gizmos.DrawLine(ray.origin, hit.transform.position);
                }
            }

            if (CurrentTarget != null)
            {
                Gizmos.color = selectedTargetColor;
                Vector3 targetPos = CurrentTarget.transform.position;

                Gizmos.DrawWireSphere(targetPos, sphereCastRadius * 1.2f);
                Gizmos.DrawSphere(targetPos, 0.1f);
                Gizmos.DrawLine(ray.origin, targetPos);
            }
        }
        #endregion
    }
}
