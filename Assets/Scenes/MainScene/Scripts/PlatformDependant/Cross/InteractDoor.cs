using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Door interaction — supports both trigger-collider overlap (legacy) and IInteractable (new system).
/// The player can interact with the door via:
/// - Desktop: E key (via PlayerKB_GrabController → IInteractable) or LMB/RMB when in trigger range
/// - VR: grip button when in trigger range
/// </summary>
public class InteractDoor : MonoBehaviour, IInteractable
{

    [SerializeField] private InputActionReference[] inputActionReferences = new InputActionReference[4]; // 0: LMB, 1: RMB, 2: Left Grip, 3: Right Grip

    // Fetch the player's hand objects and their colliders based on the platform
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
    [SerializeField] private GameObject playerLeftHandKB;
    [SerializeField] private GameObject playerRightHandKB;
    private Collider playerLeftColliderKB;
    private Collider playerRightColliderKB;
#endif

#if UNITY_ANDROID
    [SerializeField] private GameObject playerLeftHandVR;
    [SerializeField] private GameObject playerRightHandVR;
    private Collider playerLeftColliderVR;
    private Collider playerRightColliderVR;
#endif

    private bool playerInRange = false; // To check if the player is in range to interact with the door
    private bool isOpen = false; // Check if the door is open or closed
    private bool isMoving = false; // The door cannot be interacted with while it is moving

    [SerializeField] Animator doorAnimator; // Animator component for the door

    private Outline _outline;

    [SerializeField] private PlayerKB_HUD _hud;
    private bool _doOnce = true;

    void Start()
    {
        // Set the colliders of the player's hands based on the platform
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        if (playerLeftHandKB != null)
            playerLeftColliderKB = playerLeftHandKB.GetComponent<Collider>();
        if (playerRightHandKB != null)
            playerRightColliderKB = playerRightHandKB.GetComponent<Collider>();
#endif

#if UNITY_ANDROID
        if (playerLeftHandVR != null)
            playerLeftColliderVR = playerLeftHandVR.GetComponent<Collider>();
        if (playerRightHandVR != null)
            playerRightColliderVR = playerRightHandVR.GetComponent<Collider>();
#endif

        // Enable input actions and subscribe to performed events
        for (int i = 0; i < inputActionReferences.Length; i++)
        {
            inputActionReferences[i].action.Enable();
            inputActionReferences[i].action.performed += interactDoor;
        }

        _outline = GetComponent<Outline>();
    }

    // Open or close the door when the player interacts if they are in range
    private void interactDoor(InputAction.CallbackContext context)
    {
        if (playerInRange && !isMoving)
        {
            ToggleDoor();
        }
    }

    /// <summary>
    /// IInteractable implementation — called by PlayerKB_GrabController (E key) or VR grab controller.
    /// Does NOT require trigger overlap — works via camera raycast.
    /// </summary>
    public void Interact(GameObject interactor)
    {
        if (!isMoving)
        {
            ToggleDoor();
        }
    }

    public bool CanInteract => !isMoving;

    void ToggleDoor()
    {
        isMoving = true;
        if (!isOpen)
        {
            doorAnimator.Play("Opening");
            StartCoroutine(setIsMoving());
            isOpen = true;
        }
        else
        {
            doorAnimator.Play("Closing");
            StartCoroutine(setIsMoving());
            isOpen = false;
        }
    }

    private IEnumerator setIsMoving()
    {
        yield return new WaitForSeconds(1.0f);
        isMoving = false;
    }

    // Detect when the player's hand colliders enter the door handle's trigger collider
    private void OnTriggerEnter(Collider collider)
    {
        bool isPlayerHand = false;

        #if UNITY_STANDALONE_WIN || UNITY_EDITOR
        if (collider == playerLeftColliderKB || collider == playerRightColliderKB)
            isPlayerHand = true;
        #endif
        #if UNITY_ANDROID
        if (collider == playerLeftColliderVR || collider == playerRightColliderVR)
            isPlayerHand = true;
        #endif

        if (isPlayerHand)
        {
            playerInRange = true;
            if (_outline != null) _outline.enabled = true;
            if (_doOnce && _hud != null)
            {
                _hud.CreateHint("[E]: Interact");
                _doOnce = false;
            }
        }
    }

    // Detect when the player's hand colliders exit the door handle's trigger collider
    private void OnTriggerExit(Collider collider)
    {
        bool isPlayerHand = false;

        #if UNITY_STANDALONE_WIN || UNITY_EDITOR
        if (collider == playerLeftColliderKB || collider == playerRightColliderKB)
            isPlayerHand = true;
        #endif
        #if UNITY_ANDROID
        if (collider == playerLeftColliderVR || collider == playerRightColliderVR)
            isPlayerHand = true;
        #endif

        if (isPlayerHand)
        {
            playerInRange = false;
            if (_outline != null) _outline.enabled = false;
            if (_hud != null) _hud.ClearHint("[E]: Interact");
            _doOnce = true;
        }
    }

    void OnDestroy()
    {
        // Disable input actions and unsubscribe from performed events
        for (int i = 0; i < inputActionReferences.Length; i++)
        {
            inputActionReferences[i].action.Disable();
            inputActionReferences[i].action.performed -= interactDoor;
        }
    }
}
