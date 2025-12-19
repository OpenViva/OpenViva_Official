using UnityEngine;
using UnityEngine.InputSystem;

// This class allows the player to open and close the door

public class InteractDoor : MonoBehaviour
{

    [SerializeField] private InputActionReference[] inputActionReferences = new InputActionReference[4]; // 0: LMB, 1: RMB, 2: Left Grip, 3: Right Grip

    // Fetch the player's hand objects and their colliders based on the platform
#if UNITY_STANDALONE_WIN
    [SerializeField] private GameObject playerLeftHandKB;
    [SerializeField] private GameObject playerRightHandKB;
    private Collider playerLeftColliderKB;
    private Collider playerRightColliderKB;

#elif UNITY_ANDROID || UNITY_EDITOR
    [SerializeField] private GameObject playerLeftHandVR;
    [SerializeField] private GameObject playerRightHandVR;
    private Collider playerLeftColliderVR;
    private Collider playerRightColliderVR;

#endif

    private bool playerInRange = false; // To check if the player is in range to interact with the door
    private bool isOpen = false; // Check if the door is open or closed

    [SerializeField] Animator doorAnimator; // Animator component for the door

    void Start()
    {
        // Set the colliders of the player's hands based on the platform
#if UNITY_STANDALONE_WIN

        playerLeftColliderKB = playerLeftHandKB.GetComponent<Collider>();
        playerRightColliderKB = playerRightHandKB.GetComponent<Collider>();

#elif UNITY_ANDROID || UNITY_EDITOR
        playerLeftColliderVR = playerLeftHandVR.GetComponent<Collider>();
        playerRightColliderVR = playerRightHandVR.GetComponent<Collider>();

#endif

        // Enable input actions and subscribe to performed events
        for (int i = 0; i < inputActionReferences.Length; i++)
        {
            inputActionReferences[i].action.Enable();
            inputActionReferences[i].action.performed += interactDoor;
        }
    }

    // Open or close the door when the player interacts if they are in range
    private void interactDoor(InputAction.CallbackContext context)
    {
        if (playerInRange)
        {
            if (!isOpen)
            {
                doorAnimator.Play("Opening");
                isOpen = true;
            }
            else
            {
                doorAnimator.Play("Closing");
                isOpen = false;
            }
        }
    }

    // Detect when the player's hand colliders enter the door handle's trigger collider
    private void OnTriggerEnter(Collider collider)
    {
        #if UNITY_STANDALONE_WIN
        if (collider == playerLeftColliderKB || collider == playerRightColliderKB)
        {
            playerInRange = true;
        }
        #elif UNITY_ANDROID || UNITY_EDITOR
        if (collider == playerLeftColliderVR || collider == playerRightColliderVR)
        {
            playerInRange = true;
        }
        #endif
    }

    // Detect when the player's hand colliders exit the door handle's trigger collider
    private void OnTriggerExit(Collider collider)
    {
        #if UNITY_STANDALONE_WIN
        if (collider == playerLeftColliderKB || collider == playerRightColliderKB)
        {
            playerInRange = false;
        }
        #elif UNITY_ANDROID || UNITY_EDITOR
        if (collider == playerLeftColliderVR || collider == playerRightColliderVR)
        {
            playerInRange = false;
        }
        #endif
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