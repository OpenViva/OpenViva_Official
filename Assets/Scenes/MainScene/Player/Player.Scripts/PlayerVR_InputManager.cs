using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class PlayerVR_InputManager : MonoBehaviour
{
    // This class detects a VR player's inputs and executes the correct response.

    // FIELDS
    [SerializeField] private InputActionReference[] inputActionReferences = new InputActionReference[3];
    [SerializeField] private Animator animator;

    // PROPERTIES
    void Start()
    {
        for (int i = 0; i < inputActionReferences.Length; i++)
        {
            inputActionReferences[i].action.Enable();
        }
        inputActionReferences[0].action.performed += LeftPoint;
        inputActionReferences[1].action.performed += LeftGrab;
        inputActionReferences[2].action.performed += RightPoint;
        inputActionReferences[3].action.performed += RightGrab;
    }

    void OnDestroy()
    {
        for (int i = 0; i < inputActionReferences.Length; i++)
        {
            inputActionReferences[i].action.Disable();
        }
        inputActionReferences[0].action.performed -= LeftPoint;
        inputActionReferences[1].action.performed -= LeftGrab;
        inputActionReferences[2].action.performed -= RightPoint;
        inputActionReferences[3].action.performed -= RightGrab;
    }

    private void LeftPoint(InputAction.CallbackContext context)
    {
        animator.Play("Player_LeftPoint_Ani");
        Debug.Log("Left Point Triggered");
    }

    private void LeftGrab(InputAction.CallbackContext context)
    {
        animator.Play("Player_LeftGrab_Ani");
        Debug.Log("Left Grab Triggered");
    }

    private void RightPoint(InputAction.CallbackContext context)
    {
        animator.Play("Player_RightPoint_Ani");
        Debug.Log("Right Point Triggered");
    }

    private void RightGrab(InputAction.CallbackContext context)
    {
        animator.Play("Player_RightGrab_Ani");
        Debug.Log("Right Grab Triggered");
    }
}
