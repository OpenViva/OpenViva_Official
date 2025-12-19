#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using UnityEngine;
using UnityEngine.InputSystem;

// This script allows the player to grab and release a rubber ducky object (KB&M only)

public class Grab_RubberDucky : MonoBehaviour
{

    [SerializeField] private InputActionReference[] inputActionReferences = new InputActionReference[2]; // 0 - LBM, 1 - RMB
    [SerializeField] private GameObject playerLeftHand; // The player's left hand object
    [SerializeField] private GameObject playerRightHand; // The player's right hand object
    private Collider playerLeftCollider; // The collider of the player's left hand
    private Collider playerRightCollider; // The collider of the player's right hand
    [SerializeField] private GameObject rubberDucky; // The rubber ducky object
    [SerializeField] private Rigidbody rubberDuckyRB; // The Rigidbody of the rubber ducky object

    private bool playerInRange = false; // Check whether the player is in range to grab the rubber ducky
    private bool isGrabbedInLeft = false; // Check whether the rubber ducky is grabbed in the left hand
    private bool isGrabbedInRight = false; // Check whether the rubber ducky is grabbed in the right hand

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
            rubberDuckyRB.useGravity = false;
            rubberDuckyRB.isKinematic = true;
            rubberDucky.transform.SetParent(playerLeftHand.transform);
            rubberDucky.transform.localPosition = new Vector3(0.0133f, 0.0137f, -0.007f);
            rubberDucky.transform.localRotation = Quaternion.Euler(-15.865f, 22.223f, 34.353f);
            isGrabbedInLeft = true;
        }
        // If the bag is already grabbed in the left hand, release it
        else if (isGrabbedInLeft)
        {
            rubberDucky.transform.SetParent(null);
            rubberDuckyRB.isKinematic = false;
            rubberDuckyRB.useGravity = true;
            isGrabbedInLeft = false;
        }
    }

    private void grabRight(InputAction.CallbackContext context)
    {
        // If the player is in range and the bag is not already grabbed, grab it with the right hand
        if (playerInRange && !isGrabbedInRight && !isGrabbedInLeft)
        {
            rubberDuckyRB.useGravity = false;
            rubberDuckyRB.isKinematic = true;
            rubberDucky.transform.SetParent(playerRightHand.transform);
            rubberDucky.transform.localPosition = new Vector3(-0.004600528f, 0.00939743f, -0.007199669f);
            rubberDucky.transform.localRotation = Quaternion.Euler(0f, 159.498f, 9.144f);
            isGrabbedInRight = true;
        }
        // If the bag is already grabbed in the right hand, release it
        else if (isGrabbedInRight)
        {
            rubberDucky.transform.SetParent(null);
            rubberDuckyRB.isKinematic = false;
            rubberDuckyRB.useGravity = true;
            isGrabbedInRight = false;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        // Check if the player is in range to grab the ducky
        if (collider == playerLeftCollider || collider == playerRightCollider)
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        // Check if the player is out of range to grab the ducky
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