using UnityEngine;
using UnityEngine.InputSystem;

public class Grab_RubberDucky : MonoBehaviour
{
    // This script allows the player to grab and release a rubber ducky.
    // Code by Saien

    [SerializeField] private InputActionReference[] inputActionReferences = new InputActionReference[2];
    [SerializeField] private GameObject playerLeftHand;
    [SerializeField] private GameObject playerRightHand;
    private Collider playerLeftCollider;
    private Collider playerRightCollider;
    [SerializeField] private GameObject rubberDucky;
    [SerializeField] private Rigidbody rubberDuckyRB;

    private bool playerInRange = false;
    private bool isGrabbedInLeft = false;
    private bool isGrabbedInRight = false;

    void Start()
    {
        playerLeftCollider = playerLeftHand.GetComponent<Collider>();
        playerRightCollider = playerRightHand.GetComponent<Collider>();
        for (int i = 0; i < inputActionReferences.Length; i++)
        {
            inputActionReferences[i].action.Enable();
        }
        inputActionReferences[0].action.performed += grabLeft;
        inputActionReferences[1].action.performed += grabRight;
    }

    private void grabLeft(InputAction.CallbackContext context)
    {
        if (playerInRange && !isGrabbedInLeft && !isGrabbedInRight)
        {
            rubberDuckyRB.useGravity = false;
            rubberDuckyRB.isKinematic = true;
            rubberDucky.transform.SetParent(playerLeftHand.transform);
            rubberDucky.transform.localPosition = new Vector3(0.0133f, 0.0137f, -0.007f);
            rubberDucky.transform.localRotation = Quaternion.Euler(-15.865f, 22.223f, 34.353f);
            isGrabbedInLeft = true;
        }
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
        if (playerInRange && !isGrabbedInRight && !isGrabbedInLeft)
        {
            rubberDuckyRB.useGravity = false;
            rubberDuckyRB.isKinematic = true;
            rubberDucky.transform.SetParent(playerRightHand.transform);
            rubberDucky.transform.localPosition = new Vector3(-0.004600528f, 0.00939743f, -0.007199669f);
            rubberDucky.transform.localRotation = Quaternion.Euler(0f, 159.498f, 9.144f);
            isGrabbedInRight = true;
        }
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
        if (collider == playerLeftCollider || collider == playerRightCollider)
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider == playerLeftCollider || collider == playerRightCollider)
        {
            playerInRange = false;
        }
    }

    void OnDestroy()
    {
        for (int i = 0; i < inputActionReferences.Length; i++)
        {
            inputActionReferences[i].action.Disable();
        }
        inputActionReferences[0].action.performed -= grabLeft;
        inputActionReferences[1].action.performed -= grabRight;
    }

}
