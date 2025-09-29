using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grab_Bag : MonoBehaviour
{
    // This script allows the player to grab and release a bag
    // Code by Saien

    [SerializeField] private InputActionReference[] inputActionReferences = new InputActionReference[2];
    [SerializeField] private GameObject playerLeftHand;
    [SerializeField] private GameObject playerRightHand;
    [SerializeField] private Collider playerLeftCollider;
    [SerializeField] private Collider playerRightCollider;
    [SerializeField] private GameObject bag;
    [SerializeField] private Rigidbody bagRB;

    private bool playerInRange = false;
    private bool isGrabbed = false;

    void Start()
    {
        for (int i = 0; i < inputActionReferences.Length; i++)
        {
            inputActionReferences[i].action.Enable();
        }
        inputActionReferences[0].action.performed += grabLeft;
        inputActionReferences[1].action.performed += grabRight;
    }

    private void grabLeft(InputAction.CallbackContext context)
    {
        if (playerInRange && !isGrabbed)
        {
            bagRB.useGravity = false;
            bagRB.isKinematic = true;
            bag.transform.SetParent(playerLeftHand.transform);
            isGrabbed = true;
        }
        else
        {
            bag.transform.SetParent(null);
            bagRB.isKinematic = false;
            bagRB.useGravity = true;
            isGrabbed = false;
        }
    }

    private void grabRight(InputAction.CallbackContext context)
    {
        if (playerInRange && !isGrabbed)
        {
            bagRB.useGravity = false;
            bagRB.isKinematic = true;
            bag.transform.SetParent(playerRightHand.transform);
            isGrabbed = true;
        }
        else
        {
            bag.transform.SetParent(null);
            bagRB.isKinematic = false;
            bagRB.useGravity = true;
            isGrabbed = false;
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
