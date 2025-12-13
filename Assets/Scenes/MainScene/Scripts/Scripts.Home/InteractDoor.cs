using UnityEngine;
using UnityEngine.InputSystem;

public class InteractDoor : MonoBehaviour
{

    [SerializeField] private InputActionReference[] inputActionReferences = new InputActionReference[4];

    [SerializeField] private GameObject playerLeftHandKB;
    [SerializeField] private GameObject playerRightHandKB;
    [SerializeField] private GameObject playerLeftHandVR;
    [SerializeField] private GameObject playerRightHandVR;

    private Collider playerLeftColliderKB;
    private Collider playerRightColliderKB;
    private Collider playerLeftColliderVR;
    private Collider playerRightColliderVR;

    private bool playerInRange = false;
    private bool isOpen = false;

    [SerializeField] Animator doorAnimator;

    void Start()
    {
        playerLeftColliderKB = playerLeftHandKB.GetComponent<Collider>();
        playerRightColliderKB = playerRightHandKB.GetComponent<Collider>();
        playerLeftColliderVR = playerLeftHandVR.GetComponent<Collider>();
        playerRightColliderVR = playerRightHandVR.GetComponent<Collider>();

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
        if (collider == playerLeftColliderKB || collider == playerRightColliderKB || collider == playerLeftColliderVR || collider == playerRightColliderVR)
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider == playerLeftColliderKB || collider == playerRightColliderKB || collider == playerLeftColliderVR || collider == playerRightColliderVR)
        {
            playerInRange = false;
        }
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
