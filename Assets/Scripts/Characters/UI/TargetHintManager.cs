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
        [SerializeField] private Vector3 worldOffset = new Vector3(0.6f, 0.6f, 0f);

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

            Globals.OnCameraChanged += HandleCameraChanged;

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

            Globals.OnCameraChanged -= HandleCameraChanged;
        }

        private void OnDestroy()
        {
            if (detector != null)
            {
                detector.OnTargetChanged -= HandleTargetChanged;
            }

            Globals.OnCameraChanged -= HandleCameraChanged;
        }

        private void HandleCameraChanged(Camera newCamera)
        {
            cachedCamera = newCamera;
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
            else if (newTarget.TryGetComponent(out IInteractable interactable))
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

                if (cachedCamera != null)
                {
                    activeHintTransform.LookAt(activeHintTransform.position + cachedCamera.transform.rotation * Vector3.forward,
                                         cachedCamera.transform.rotation * Vector3.up);
                }
            }
        }

        private void UpdateHintPosition()
        {
            Vector3 targetPos = targetTransform.position;

            if (hasCollider && targetCollider != null)
            {
                targetPos = targetTransform.TransformPoint(cachedLocalCenterOffset);
            }

            activeHintTransform.position = targetPos + worldOffset;
        }

        private void SetHintActive(GameObject hint, bool active)
        {
            if (hint != null) hint.SetActive(active);
        }
    }
}