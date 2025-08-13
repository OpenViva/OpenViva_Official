using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class PlayerVR_BasicActions : MonoBehaviour
{
    // This class detects a VR player's inputs and executes the correct response.
    // Code by Saien

    // FIELDS
    [SerializeField] private InputActionReference[] inputActionReferences = new InputActionReference[3];
    [SerializeField] private Animator animatorL;
    [SerializeField] private Animator animatorR;
    private bool LeftTriggerDown = false;
    private bool LeftGripDown = false;
    private bool RightTriggerDown = false;
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

    }

    
    private void LeftPoint(InputAction.CallbackContext context)
    {
        LeftGripDown = true;
        if (!LeftTriggerDown)
        {
            animatorL.Play("Player_LeftPoint");
        }
    }

    private void LeftGrab(InputAction.CallbackContext context)
    {
        LeftTriggerDown = true;
        if (LeftGripDown)
        {
            animatorL.Play("Player_LeftPointToGrab");
        } else
        {
            animatorL.Play("Player_LeftGrab");
        }
    }

    private void LeftPointEnd(InputAction.CallbackContext context)
    {
        LeftGripDown = false;
        if (LeftTriggerDown)
        {
            animatorL.Play("Player_LeftPointToGrab");
        } else
        {
            animatorL.Play("Player_LeftPointEnd");
        }

    }

    private void LeftGrabEnd(InputAction.CallbackContext context)
    {
        LeftTriggerDown = false;
        if (LeftGripDown)
        {
            animatorL.Play("Player_LeftGrabToPoint");
        } else
        {
            animatorL.Play("Player_LeftGrabEnd");
        }
    }

    private void RightPoint(InputAction.CallbackContext context)
    {
        RightGripDown = true;
        if (!RightTriggerDown)
        {
            animatorR.Play("Player_RightPoint");
        }
    }

    private void RightGrab(InputAction.CallbackContext context)
    {
        RightTriggerDown = true;
        if (RightGripDown)
        {
            animatorR.Play("Player_RightPointToGrab");
        } else
        {
            animatorR.Play("Player_RightGrab");
        }
    }

    private void RightPointEnd(InputAction.CallbackContext context)
    {
        RightGripDown = false;
        if (RightTriggerDown)
        {
            animatorR.Play("Player_RightPointToGrab");
        } else
        {
            animatorR.Play("Player_RightPointEnd");
        }
    }

    private void RightGrabEnd(InputAction.CallbackContext context)
    {
        RightTriggerDown = false;
        if (RightGripDown)
        {
            animatorR.Play("Player_RightGrabToPoint");
        } else
        {
            animatorR.Play("Player_RightGrabEnd");
        }
    }
}
