#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using UnityEngine;
using UnityEngine.InputSystem;

public class Grab_Bag : MonoBehaviour
{
    // This script allows the player to grab and release a rubber ducky.
    // Code by Saien

    [SerializeField] private InputActionReference[] inputActionReferences = new InputActionReference[2];
    [SerializeField] private GameObject playerLeftHand;
    [SerializeField] private GameObject playerRightHand;
    private Collider playerLeftCollider;
    private Collider playerRightCollider;
    [SerializeField] private GameObject bag;
    [SerializeField] private Rigidbody bagRB;

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
            bagRB.useGravity = false;
            bagRB.isKinematic = true;
            bag.transform.SetParent(playerLeftHand.transform);
            bag.transform.localPosition = new Vector3(0.0456f, 0.0211f, -0.0027f);
            bag.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            isGrabbedInLeft = true;
        }
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
        if (playerInRange && !isGrabbedInRight && !isGrabbedInLeft)
        {
            bagRB.useGravity = false;
            bagRB.isKinematic = true;
            bag.transform.SetParent(playerRightHand.transform);
            bag.transform.localPosition = new Vector3(-0.04559939f, 0.02110242f, -0.003600158f);
            bag.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            isGrabbedInRight = true;
        }
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

#endif