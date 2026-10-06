using System.Collections.Generic;
using UnityEngine;

namespace Viva.Interaction
{
    public class PlayerInteractionHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SmartTargetDetector detector;

        protected Player _player;

        // Tracks what each hand is currently holding
        private Dictionary<HandSide, IInteractable> heldObjects = new();

        private void Awake()
        {
            heldObjects[HandSide.Left] = null;
            heldObjects[HandSide.Right] = null;
        }

        private void Start()
        {
            _player = FindFirstObjectByType<Player>();
            detector = GetComponent<SmartTargetDetector>();

            AssignInputs();
        }

        private void ProcessInteraction(HandSide hand)
        {
            // 1. If the hand is holding something, Use it.
            if (heldObjects[hand] != null)
            {
                if (heldObjects[hand].CurrentType == InteractionType.Use)
                {
                    switch (hand)
                    {
                        case HandSide.Any:
                            heldObjects[hand].ExecuteAction(gameObject, hand, detector.rightHandTransform);
                            break;
                        case HandSide.Left:
                            heldObjects[hand].ExecuteAction(gameObject, hand, detector.leftHandTransform);
                            break;
                        case HandSide.Right:
                            heldObjects[hand].ExecuteAction(gameObject, hand, detector.rightHandTransform);
                            break;
                        default:
                            break;
                    }
                }
                return; // Skip world interaction
            }

            // 2. If hand is empty, check the world.
            IPointerSource pointerSource = detector.GetActivePointer(hand);
            GameObject target = detector.PerformCast(pointerSource);

            if (target != null && target.TryGetComponent(out IInteractable interactable))
            {
                bool success = false;
                switch (hand)
                {
                    case HandSide.Any:
                        success = interactable.ExecuteAction(gameObject, hand, detector.rightHandTransform);
                        break;
                    case HandSide.Left:
                        success = interactable.ExecuteAction(gameObject, hand, detector.leftHandTransform);
                        break;
                    case HandSide.Right:
                        success = interactable.ExecuteAction(gameObject, hand, detector.rightHandTransform);
                        break;
                    default:
                        break;
                }

                // 3. If the interaction was a Grab and it succeeded, register it to the hand
                if (success && interactable.CurrentType == InteractionType.Use)
                {
                    heldObjects[hand] = interactable;

                    // Optional: Clear the outline immediately upon grabbing
                    target.GetComponent<Outline>().enabled = false;
                }
            }
        }

        // Public method allowing objects to free up the hand
        public void ReleaseObject(HandSide hand)
        {
            heldObjects[hand] = null;
        }

        private void OnDesktopLeftGrab()
        {
            if (!Globals.isDesktopMode) return;
            ProcessInteraction(HandSide.Left);
        }

        private void OnDesktopRightGrab()
        {
            if (!Globals.isDesktopMode) return;
            ProcessInteraction(HandSide.Right);
        }

        private void OnVRLeftGrab()
        {
            if (Globals.isDesktopMode) return;
            ProcessInteraction(HandSide.Left);
        }

        private void OnVRRightGrab()
        {
            if (Globals.isDesktopMode) return;
            ProcessInteraction(HandSide.Right);
        }

        #region Helper Methods
        private void AssignInputs()
        {
            _player.Controls.Viva.DesktopLeftGrab.performed += context => OnDesktopLeftGrab();
            _player.Controls.Viva.DesktopRightGrab.performed += context => OnDesktopRightGrab();

            _player.Controls.Viva.VRLeftGrab.performed += context => OnVRLeftGrab();
            _player.Controls.Viva.VRRightGrab.performed += context => OnVRRightGrab();
        }
        #endregion
    }
}