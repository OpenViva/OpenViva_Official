#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Full desktop (KB&M) interaction controller. Handles all non-VR hand interactions:
/// - LMB/RMB: grab/drop items (toggle mode, like old system)
/// - E: interact with world objects (doors, buttons, NPCs)
/// - Q: toggle bag open/close
/// - Scroll: extend/retract hands
/// - Crosshair-based item detection (raycast from camera center)
/// - Item held in hand stays locked relative to head (like old freezeKeyboardLocalPosition)
/// - HUD hints for available actions
///
/// Replaces: PlayerKB_GrabObject (per-item), parts of PlayerKB_BasicActions (scroll),
///           and adds missing interact/use/gesture functionality.
/// </summary>
public class PlayerKB_GrabController : MonoBehaviour
{
    [Header("Hand References")]
    [Tooltip("HandGrabSystem on the left hand")]
    public HandGrabSystem leftHand;
    [Tooltip("HandGrabSystem on the right hand")]
    public HandGrabSystem rightHand;

    [Header("Hand Driver")]
    [Tooltip("Controls desktop hand positioning (extend/retract, head-lock)")]
    public DesktopHandDriver handDriver;

    [Header("Hand Animators (Optional)")]
    public HandAnimator leftHandAnimator;
    public HandAnimator rightHandAnimator;

    [Header("Grab Detection")]
    [Tooltip("Camera used for screen-center raycasting")]
    public Camera playerCamera;
    [Tooltip("Max raycast distance from camera for grab/interact detection")]
    public float cameraReachDistance = 2.5f;
    [Tooltip("Layer mask for grabbable objects")]
    public LayerMask grabLayerMask = ~0;
    [Tooltip("Layer mask for interactable objects (doors, buttons, etc.)")]
    public LayerMask interactLayerMask = ~0;

    [Header("HUD")]
    public PlayerKB_HUD hud;

    [Header("Crosshair")]
    [Tooltip("Optional crosshair UI element — enabled when not in menu")]
    public GameObject crosshairUI;

    // Input — shared single instance
    DesktopInput playerInput;

    // Highlight tracking
    GrabbableItem highlightedItem;
    IInteractable highlightedInteractable;

    // Interaction cooldown
    float interactCooldown;

    void Awake()
    {
        playerInput = new DesktopInput();

        // Grab bindings
        playerInput.Viva.LeftGrab.performed += OnLeftGrab;
        playerInput.Viva.RightGrab.performed += OnRightGrab;

        // Interact (E key)
        playerInput.Viva.Interact.performed += OnInteract;

        // Scroll (hand extend/retract)
        playerInput.Viva.ScrollUp.performed += OnScrollUp;
        playerInput.Viva.ScrollDown.performed += OnScrollDown;
    }

    void OnEnable()
    {
        playerInput.Viva.Enable();
        SubscribeToHands();
    }

    void OnDisable()
    {
        playerInput.Viva.Disable();
        UnsubscribeFromHands();
    }

    /// <summary>
    /// Re-subscribe in Start() in case references were set after OnEnable
    /// (happens when components are added dynamically at runtime by bootstrap).
    /// </summary>
    void Start()
    {
        SubscribeToHands();
    }

    void SubscribeToHands()
    {
        // Avoid double-subscription
        UnsubscribeFromHands();

        if (leftHand != null)
        {
            leftHand.OnItemGrabbed += OnLeftHandGrabbed;
            leftHand.OnItemReleased += OnLeftHandReleased;
        }
        if (rightHand != null)
        {
            rightHand.OnItemGrabbed += OnRightHandGrabbed;
            rightHand.OnItemReleased += OnRightHandReleased;
        }
    }

    void UnsubscribeFromHands()
    {
        if (leftHand != null)
        {
            leftHand.OnItemGrabbed -= OnLeftHandGrabbed;
            leftHand.OnItemReleased -= OnLeftHandReleased;
        }
        if (rightHand != null)
        {
            rightHand.OnItemGrabbed -= OnRightHandGrabbed;
            rightHand.OnItemReleased -= OnRightHandReleased;
        }
    }

    void Update()
    {
        if (Globals.isMenuOpen)
        {
            if (crosshairUI != null && crosshairUI.activeSelf)
                crosshairUI.SetActive(false);
            return;
        }

        if (crosshairUI != null && !crosshairUI.activeSelf)
            crosshairUI.SetActive(true);

        if (interactCooldown > 0f)
            interactCooldown -= Time.deltaTime;

        UpdateCameraHighlight();
    }

    #region Input Handlers

    void OnLeftGrab(InputAction.CallbackContext ctx)
    {
        if (Globals.isMenuOpen || leftHand == null) return;

        if (leftHand.IsHolding)
        {
            // Toggle drop
            leftHand.ReleaseItem(true);
        }
        else
        {
            TryGrabWithHand(leftHand);
        }
    }

    void OnRightGrab(InputAction.CallbackContext ctx)
    {
        if (Globals.isMenuOpen || rightHand == null) return;

        if (rightHand.IsHolding)
        {
            rightHand.ReleaseItem(true);
        }
        else
        {
            TryGrabWithHand(rightHand);
        }
    }

    void TryGrabWithHand(HandGrabSystem hand)
    {
        // 1. Try camera raycast (what the player is looking at)
        var target = FindItemFromCamera();
        if (target != null)
        {
            hand.GrabItem(target);
            return;
        }

        // 2. Fall back to hand proximity
        hand.AttemptGrab();
    }

    void OnInteract(InputAction.CallbackContext ctx)
    {
        if (Globals.isMenuOpen) return;
        if (interactCooldown > 0f) return;

        // Try to interact with whatever we're looking at
        if (highlightedInteractable != null)
        {
            highlightedInteractable.Interact(this.gameObject);
            interactCooldown = 0.3f;
            return;
        }

        // Also check for any Collider with IInteractable via raycast
        if (playerCamera != null)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, cameraReachDistance, interactLayerMask))
            {
                var interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact(this.gameObject);
                    interactCooldown = 0.3f;
                }
            }
        }
    }

    void OnScrollUp(InputAction.CallbackContext ctx)
    {
        if (Globals.isMenuOpen) return;
        if (handDriver != null)
            handDriver.ExtendHands();
    }

    void OnScrollDown(InputAction.CallbackContext ctx)
    {
        if (Globals.isMenuOpen) return;
        if (handDriver != null)
            handDriver.RetractHands();
    }

    #endregion

    #region Camera-based detection (Desktop specific)

    /// <summary>
    /// Raycasts from screen center to find what the player is looking at.
    /// </summary>
    GrabbableItem FindItemFromCamera()
    {
        if (playerCamera == null) return null;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, cameraReachDistance, grabLayerMask))
        {
            var item = hit.collider.GetComponentInParent<GrabbableItem>();
            if (item != null && item.canBePickedUp && !item.IsHeld)
                return item;
        }
        return null;
    }

    void UpdateCameraHighlight()
    {
        bool bothFull = (leftHand != null && leftHand.IsHolding) && (rightHand != null && rightHand.IsHolding);

        GrabbableItem foundItem = null;
        IInteractable foundInteractable = null;

        if (playerCamera != null)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, cameraReachDistance))
            {
                // Check for grabbable items
                if (!bothFull)
                {
                    foundItem = hit.collider.GetComponentInParent<GrabbableItem>();
                    if (foundItem != null && (!foundItem.canBePickedUp || foundItem.IsHeld))
                        foundItem = null;
                }

                // Check for interactables
                if (foundItem == null)
                    foundInteractable = hit.collider.GetComponentInParent<IInteractable>();
            }
        }

        // Update item highlight
        if (foundItem != highlightedItem)
        {
            if (highlightedItem != null)
            {
                highlightedItem.SetHighlight(false);
                ClearHudHint("[LMB] or [RMB]: Grab");
            }

            highlightedItem = foundItem;

            if (highlightedItem != null)
            {
                highlightedItem.SetHighlight(true);
                ShowHudHint("[LMB] or [RMB]: Grab");
            }
        }

        // Update interactable highlight
        if (foundInteractable != highlightedInteractable)
        {
            if (highlightedInteractable != null)
                ClearHudHint("[E]: Interact");

            highlightedInteractable = foundInteractable;

            if (highlightedInteractable != null)
                ShowHudHint("[E]: Interact");
        }
    }

    #endregion

    #region Grab/Release Callbacks

    void OnLeftHandGrabbed(GrabbableItem item)
    {
        ClearHudHint("[LMB] or [RMB]: Grab");
        ShowHudHint("[LMB]: Drop");

        if (leftHandAnimator != null)
            leftHandAnimator.SetHoldPose(item);
    }

    void OnLeftHandReleased(GrabbableItem item)
    {
        ClearHudHint("[LMB]: Drop");

        if (leftHandAnimator != null)
            leftHandAnimator.ClearHoldPose();
    }

    void OnRightHandGrabbed(GrabbableItem item)
    {
        ClearHudHint("[LMB] or [RMB]: Grab");
        ShowHudHint("[RMB]: Drop");

        if (rightHandAnimator != null)
            rightHandAnimator.SetHoldPose(item);
    }

    void OnRightHandReleased(GrabbableItem item)
    {
        ClearHudHint("[RMB]: Drop");

        if (rightHandAnimator != null)
            rightHandAnimator.ClearHoldPose();
    }

    #endregion

    #region HUD Helpers

    void ShowHudHint(string text)
    {
        if (hud != null)
            hud.CreateHint(text);
    }

    void ClearHudHint(string text)
    {
        if (hud != null)
            hud.ClearHint(text);
    }

    #endregion

    #region Public Queries

    /// <summary>
    /// Get what item the player is currently looking at (for other systems to use).
    /// </summary>
    public GrabbableItem GetLookedAtItem() => highlightedItem;

    /// <summary>
    /// Check if a specific hand is holding something.
    /// </summary>
    public bool IsHandHolding(bool left) => left ? (leftHand != null && leftHand.IsHolding) : (rightHand != null && rightHand.IsHolding);

    /// <summary>
    /// Get the held item for a hand.
    /// </summary>
    public GrabbableItem GetHeldItem(bool left) => left ? leftHand?.HeldItem : rightHand?.HeldItem;

    #endregion

    void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.Viva.LeftGrab.performed -= OnLeftGrab;
            playerInput.Viva.RightGrab.performed -= OnRightGrab;
            playerInput.Viva.Interact.performed -= OnInteract;
            playerInput.Viva.ScrollUp.performed -= OnScrollUp;
            playerInput.Viva.ScrollDown.performed -= OnScrollDown;
            playerInput.Dispose();
        }
    }
}

#endif
