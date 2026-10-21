using UnityEngine;

namespace Viva.Interaction
{
    public class TargetHintManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("Auto-finds if left empty.")]
        private SmartTargetDetector detector;

        [Header("Pre-allocated Hint GameObjects (Canvases)")]
        [SerializeField] private GameObject grabHintObject;
        [SerializeField] private GameObject interactHintObject;
        [SerializeField] private GameObject useHintObject;
        [SerializeField] private GameObject characterHintObject;

        [Header("Positioning Settings")]
        [SerializeField] private float leftOffsetDistance = 1.2f;
        [SerializeField] private float verticalOffset = 0.5f;

        private GameObject activeHint;
        private Transform activeHintTransform;

        // --- Cached Camera Reference ---
        private Camera cachedCamera;

        // --- Cached Variables ---
        private Transform targetTransform;
        private Collider targetCollider;
        private Vector3 cachedLocalCenterOffset;
        private bool hasCollider;

        private void Awake()
        {
            if (detector == null)
            {
                detector = FindFirstObjectByType<SmartTargetDetector>();
            }

            SetHintActive(grabHintObject, false);
            SetHintActive(interactHintObject, false);
            SetHintActive(useHintObject, false);
            SetHintActive(characterHintObject, false);
        }

        private void OnEnable()
        {
            if (detector != null)
            {
                detector.OnTargetChanged += HandleTargetChanged;
            }

            // Initialize camera reference if already available
            if (Camera.main != null)
            {
                cachedCamera = Camera.main;
            }
        }

        private void OnDisable()
        {
            if (detector != null)
            {
                detector.OnTargetChanged -= HandleTargetChanged;
            }
        }

        private void OnDestroy()
        {
            if (detector != null)
            {
                detector.OnTargetChanged -= HandleTargetChanged;
            }
        }

        private void HandleTargetChanged(GameObject newTarget)
        {
            // 1. Hide current hint
            if (activeHint != null)
            {
                activeHint.SetActive(false);
                activeHint = null;
                activeHintTransform = null;
            }

            // Reset cache
            targetTransform = null;
            targetCollider = null;
            hasCollider = false;

            if (newTarget == null) return;

            // 2. Cache target references once
            targetTransform = newTarget.transform;
            targetCollider = newTarget.GetComponent<Collider>();

            if (targetCollider != null)
            {
                hasCollider = true;
                cachedLocalCenterOffset = targetTransform.InverseTransformPoint(targetCollider.bounds.center);
            }

            // 3. Determine hint type
            GameObject targetHint = null;

            if (newTarget.TryGetComponent(out CharacterInteractable _))
            {
                targetHint = characterHintObject != null ? characterHintObject : interactHintObject;
            }
            else if (newTarget.TryGetComponent(out IItemInteractable interactable))
            {
                switch (interactable.CurrentType)
                {
                    case InteractionType.Grab:
                        targetHint = grabHintObject;
                        break;
                    case InteractionType.Interact:
                        targetHint = interactHintObject;
                        break;
                    case InteractionType.Use:
                        targetHint = useHintObject;
                        break;
                }
            }

            // 4. Activate and position
            if (targetHint != null)
            {
                activeHint = targetHint;
                activeHintTransform = activeHint.transform;
                activeHint.SetActive(true);
                UpdateHintPosition();
            }
        }

        private void LateUpdate()
        {
            if (activeHint != null && activeHint.activeSelf && targetTransform != null)
            {
                UpdateHintPosition();
            }
        }

        private void UpdateHintPosition()
        {
            Vector3 targetCenter = targetTransform.position;
            if (hasCollider && targetCollider != null)
            {
                targetCenter = targetTransform.TransformPoint(cachedLocalCenterOffset);
            }

            if (cachedCamera != null)
            {
                Vector3 leftDirection = -cachedCamera.transform.right;
                activeHintTransform.position = targetCenter + (leftDirection * leftOffsetDistance) + (Vector3.up * verticalOffset);

                //activeHintTransform.rotation = cachedCamera.transform.rotation;
                activeHintTransform.LookAt(cachedCamera.transform);
            }
            else
            {
                // Fallback if camera reference isn't set yet
                activeHintTransform.position = targetCenter + new Vector3(0f, verticalOffset, 0f);
            }
        }

        private void SetHintActive(GameObject hint, bool active)
        {
            if (hint != null) hint.SetActive(active);
        }
    }
}