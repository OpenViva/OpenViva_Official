using UnityEngine;
using UnityEngine.InputSystem;

public class InteractDoor : MonoBehaviour
{

    [SerializeField] private InputActionReference[] inputActionReferences = new InputActionReference[4];

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

    private bool playerInRange = false;
    private bool isOpen = false;

    [SerializeField] Animator doorAnimator;

    void Start()
    {
        #if UNITY_STANDALONE_WIN

        playerLeftColliderKB = playerLeftHandKB.GetComponent<Collider>();
        playerRightColliderKB = playerRightHandKB.GetComponent<Collider>();

#elif UNITY_ANDROID || UNITY_EDITOR
        playerLeftColliderVR = playerLeftHandVR.GetComponent<Collider>();
        playerRightColliderVR = playerRightHandVR.GetComponent<Collider>();

#endif

        for (int i = 0; i < inputActionReferences.Length; i++)
        {
            inputActionReferences[i].action.Enable();
            inputActionReferences[i].action.performed += interactDoor;
        }
    }

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
        for (int i = 0; i < inputActionReferences.Length; i++)
        {
            inputActionReferences[i].action.Disable();
            inputActionReferences[i].action.performed -= interactDoor;
        }
    }
}