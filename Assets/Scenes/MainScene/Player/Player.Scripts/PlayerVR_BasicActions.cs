using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class PlayerVR_BasicActions: MonoBehaviour
{
    // This class detects a VR player's inputs and executes the correct response.

    // FIELDS
    [SerializeField] private InputActionReference[] inputActionReferences = new InputActionReference[3];
    [SerializeField] private Animator animator;
    private bool LeftTriggerDown = false;
    private bool RightTriggerDown = false;
    private bool LeftGripDown = false;
    private bool RightGripDown = false;

    // PROPERTIES
    void Start()
    {
        for (int i = 0; i < inputActionReferences.Length; i++)
        {
            inputActionReferences[i].action.Enable();
        }
        inputActionReferences[0].action.performed += LeftPoint;
        inputActionReferences[0].action.canceled += LeftPointEnd;
        inputActionReferences[1].action.performed += LeftGrab;
        inputActionReferences[1].action.canceled += LeftGrabEnd;
        inputActionReferences[2].action.performed += RightPoint;
        inputActionReferences[2].action.canceled += RightPointEnd;
        inputActionReferences[3].action.performed += RightGrab;
        inputActionReferences[3].action.canceled += RightGrabEnd;
    }

    void OnDestroy()
    {
        for (int i = 0; i < inputActionReferences.Length; i++)
        {
            inputActionReferences[i].action.Disable();
        }
        inputActionReferences[0].action.performed -= LeftPoint;
        inputActionReferences[0].action.canceled -= LeftPointEnd;
        inputActionReferences[1].action.performed -= LeftGrab;
        inputActionReferences[1].action.canceled -= LeftGrabEnd;
        inputActionReferences[2].action.performed -= RightPoint;
        inputActionReferences[2].action.canceled -= RightPointEnd;
        inputActionReferences[3].action.performed -= RightGrab;
        inputActionReferences[3].action.canceled -= RightGrabEnd;

    }

    
    private void LeftPoint(InputAction.CallbackContext context)
    {
        if (!LeftTriggerDown)
        {
            animator.Play("Player_LeftPoint_Ani");
        }
        LeftGripDown = true;
    }

    private void LeftGrab(InputAction.CallbackContext context)
    {
        if (LeftGripDown)
        {
            animator.Play("Player_LeftPointToGrab_Ani");
        } else
        {
            animator.Play("Player_LeftGrab_Ani");
        }
        LeftTriggerDown = true;
    }

    private void RightPoint(InputAction.CallbackContext context)
    {
        if (!RightTriggerDown)
        {
            animator.Play("Player_RightPoint_Ani");
        }
        RightGripDown = true;
    }

    private void RightGrab(InputAction.CallbackContext context)
    {
        if (RightGripDown)
        {
            animator.Play("Player_RightPointToGrab_Ani");
        } else
        {
            animator.Play("Player_RightGrab_Ani");
        }
        RightTriggerDown = true;
    }

    private void LeftPointEnd(InputAction.CallbackContext context)
    {
        if (!LeftTriggerDown)
        {
            animator.Play("Player_LeftPoint_End");
        }
        LeftGripDown = false;
    }

    private void LeftGrabEnd(InputAction.CallbackContext context)
    {
        if(LeftGripDown)
        {
            animator.Play("Player_LeftGrabToPoint_Ani");
        } else
        {
            animator.Play("Player_LeftGrab_End");
        }
        LeftTriggerDown = false;
    }

    private void RightPointEnd(InputAction.CallbackContext context)
    {
        if (!RightTriggerDown)
        {
            animator.Play("Player_RightPoint_End");
        }
        RightGripDown = false;
    }

    private void RightGrabEnd(InputAction.CallbackContext context)
    {
        if(RightGripDown)
        {
            animator.Play("Player_RightGrabToPoint_Ani");
        } else
        {
            animator.Play("Player_RightGrab_End");
        }
        RightTriggerDown = false;
    }
}
