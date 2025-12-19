#if UNITY_EDITOR || UNITY_ANDROID

using UnityEngine;
using UnityEngine.InputSystem;

// Allows the player to perform basic actions (KB&M)

public class PlayerVR_BasicActions : MonoBehaviour
{

    // FIELDS
    [SerializeField] private InputActionReference[] inputActionReferences = new InputActionReference[4]; // 0: Left Grip, 1: Right Grip, 2: Left Trigger, 3: Right Trigger
    [SerializeField] private Animator animatorL; // Left hand animator
    [SerializeField] private Animator animatorR; // Right hand animator
    private bool LeftTriggerDown = false; // Is left trigger pressed
    private bool LeftGripDown = false; // Is left grip pressed
    private bool RightTriggerDown = false; // Is right trigger pressed
    private bool RightGripDown = false; // Is right grip pressed

    // PROPERTIES
    void Start()
    {
        // Enable input actions and bind events
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
        // Disable input actions and unbind events
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
        // Set left grip down and play point animation if trigger is not down
        LeftGripDown = true;
        if (!LeftTriggerDown)
        {
            animatorL.Play("Player_LeftPoint");
        }
    }

    private void LeftGrab(InputAction.CallbackContext context)
    {
        // Set left trigger down and play grab animation or point-to-grab animation if grip is down
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
        // Set left grip up and play point-end animation or point-to-grab animation if trigger is down
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
        // Set left trigger up and play grab-end animation or grab-to-point animation if grip is down
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
        // Set right grip down and play point animation if trigger is not down
        RightGripDown = true;
        if (!RightTriggerDown)
        {
            animatorR.Play("Player_RightPoint");
        }
    }

    private void RightGrab(InputAction.CallbackContext context)
    {
        // Set right trigger down and play grab animation or point-to-grab animation if grip is down
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
        // Set right grip up and play point-end animation or point-to-grab animation if trigger is down
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
        // Set right trigger up and play grab-end animation or grab-to-point animation if grip is down
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

#endif