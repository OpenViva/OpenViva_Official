#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using UnityEngine;
using UnityEngine.InputSystem;

// This script allows the player to grab and release objects (KB&M only)

public class PlayerKB_GrabObject : MonoBehaviour
{
    [SerializeField] private InputActionReference[] inputActionReferences = new InputActionReference[2]; // 0 - LBM, 1 - RMB
    private GameObject playerLeftHand; // The player's left hand object
    private GameObject playerRightHand; // The player's right hand object
    private Collider playerLeftCollider; // The collider of the player's left hand
    private Collider playerRightCollider; // The collider of the player's right hand
    private GameObject grabbableObject; // The object to be grabbed
    private Rigidbody grabbableObjectRB; // The Rigidbody of the object to be grabbed

    private bool playerInRange = false; // Check whether the player is in range to grab the object
    private bool isGrabbedInLeft = false; // Check whether the object is grabbed in the left hand
    private bool isGrabbedInRight = false; // Check whether the object is grabbed in the right hand

    private ObjectHoldPositions holdPositions = new ObjectHoldPositions(); // Calls the script that holds the positions and rotations of all grabbable objects
    [SerializeField] private int objectIndex; // The index of the objec in the ObjectHoldPositions script

    private void Start()
    {
        // Find the player's hand objects in the scene
        playerLeftHand = GameObject.Find("hand_l");
        playerRightHand = GameObject.Find("hand_r");

        // Set the grabbable object and its Rigidbody
        grabbableObject = this.gameObject;
        grabbableObjectRB = grabbableObject.GetComponent<Rigidbody>();

        // Set the colliders of the player's hands
        playerLeftCollider = playerLeftHand.GetComponent<Collider>();
        playerRightCollider = playerRightHand.GetComponent<Collider>();

        // Enable input actions and bind the grab functions
        for (int i = 0; i < inputActionReferences.Length; i++)
        {
            inputActionReferences[i].action.Enable();
        }
        inputActionReferences[0].action.performed += grabLeft;
        inputActionReferences[1].action.performed += grabRight;
    }

    private void grabLeft(InputAction.CallbackContext context)
    {
        // If the player is in range and the object is not already grabbed, grab it with the left hand
        if (playerInRange && !isGrabbedInLeft && !isGrabbedInRight)
        {
            grabbableObjectRB.useGravity = false;
            grabbableObjectRB.isKinematic = true;
            grabbableObject.transform.SetParent(playerLeftHand.transform);
            grabbableObject.transform.localPosition = holdPositions.GetObjectPositionLeft(objectIndex);
            grabbableObject.transform.localRotation = holdPositions.GetObjectRotationLeft(objectIndex);
            isGrabbedInLeft = true;
        }
        // If the object is already grabbed in the left hand, release it
        else if (isGrabbedInLeft)
        {
            grabbableObject.transform.SetParent(null);
            grabbableObjectRB.isKinematic = false;
            grabbableObjectRB.useGravity = true;
            isGrabbedInLeft = false;
        }
    }

    private void grabRight(InputAction.CallbackContext context)
    {
        // If the player is in range and the object is not already grabbed, grab it with the right hand
        if (playerInRange && !isGrabbedInRight && !isGrabbedInLeft)
        {
            grabbableObjectRB.useGravity = false;
            grabbableObjectRB.isKinematic = true;
            grabbableObject.transform.SetParent(playerRightHand.transform);
            grabbableObject.transform.localPosition = holdPositions.GetObjectPositionRight(objectIndex);
            grabbableObject.transform.localRotation = holdPositions.GetObjectRotationRight(objectIndex);
            isGrabbedInRight = true;
        }
        // If the object is already grabbed in the right hand, release it
        else if (isGrabbedInRight)
        {
            grabbableObject.transform.SetParent(null);
            grabbableObjectRB.isKinematic = false;
            grabbableObjectRB.useGravity = true;
            isGrabbedInRight = false;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        // Check if the player is in range to grab the bag
        if (collider == playerLeftCollider || collider == playerRightCollider)
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        // Check if the player is out of range to grab the bag
        if (collider == playerLeftCollider || collider == playerRightCollider)
        {
            playerInRange = false;
        }
    }

    void OnDestroy()
    {
        // Disable input actions and unbind the grab functions
        for (int i = 0; i < inputActionReferences.Length; i++)
        {
            inputActionReferences[i].action.Disable();
        }
        inputActionReferences[0].action.performed -= grabLeft;
        inputActionReferences[1].action.performed -= grabRight;
    }
}
#endif