#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using UnityEngine;
using UnityEngine.InputSystem;

// This script allows the player to grab and release a bag object (KB&M only)

public class Grab_Bag : MonoBehaviour
{

    [SerializeField] private InputActionReference[] inputActionReferences = new InputActionReference[2]; // 0 - LBM, 1 - RMB
    [SerializeField] private GameObject playerLeftHand; // The player's left hand object
    [SerializeField] private GameObject playerRightHand; // The player's right hand object
    private Collider playerLeftCollider; // The collider of the player's left hand
    private Collider playerRightCollider; // The collider of the player's right hand
    [SerializeField] private GameObject bag; // The bag object
    [SerializeField] private Rigidbody bagRB; // The Rigidbody of the bag object

    private bool playerInRange = false; // Check whether the player is in range to grab the bag
    private bool isGrabbedInLeft = false; // Check whether the bag is grabbed in the left hand
    private bool isGrabbedInRight = false; // Check whether the bag is grabbed in the right hand

    void Start()
    {
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
        // If the player is in range and the bag is not already grabbed, grab it with the left hand
        if (playerInRange && !isGrabbedInLeft && !isGrabbedInRight)
        {
            bagRB.useGravity = false;
            bagRB.isKinematic = true;
            bag.transform.SetParent(playerLeftHand.transform);
            bag.transform.localPosition = new Vector3(0.0456f, 0.0211f, -0.0027f);
            bag.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            isGrabbedInLeft = true;
        }
        // If the bag is already grabbed in the left hand, release it
        else if (isGrabbedInLeft)
        {
            bag.transform.SetParent(null);
            bagRB.isKinematic = false;
            bagRB.useGravity = true;
            isGrabbedInLeft = false;
        }
    }

    private void grabRight(InputAction.CallbackContext context)
    {
        // If the player is in range and the bag is not already grabbed, grab it with the right hand
        if (playerInRange && !isGrabbedInRight && !isGrabbedInLeft)
        {
            bagRB.useGravity = false;
            bagRB.isKinematic = true;
            bag.transform.SetParent(playerRightHand.transform);
            bag.transform.localPosition = new Vector3(-0.04559939f, 0.02110242f, -0.003600158f);
            bag.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            isGrabbedInRight = true;
        }
        // If the bag is already grabbed in the right hand, release it
        else if (isGrabbedInRight)
        {
            bag.transform.SetParent(null);
            bagRB.isKinematic = false;
            bagRB.useGravity = true;
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