using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerKB_BasicActions : MonoBehaviour
{
    // This class manages basic actions for a keyboard player.
    // Code by Saien

    // FIELDS
    [SerializeField] private InputActionReference[] inputActionReferences = new InputActionReference[1];
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerKB_Movement playerMovement;
    private bool isCrouching = false;

    // PROPERTIES
    void Start()
    {
        for (int i = 0; i < inputActionReferences.Length; i++)
        {
            inputActionReferences[i].action.Enable();
        }
        inputActionReferences[0].action.performed += crouch;
    }

    void OnDestroy()
    {
        for (int i = 0; i < inputActionReferences.Length; i++)
        {
            inputActionReferences[i].action.Disable();
        }
        inputActionReferences[0].action.performed -= crouch;
    }

    private void crouch(InputAction.CallbackContext context)
    {
        if (!isCrouching)
        {
            playerMovement.setMovementSpeed(1f);
            playerMovement.disableRunning(true);
            characterController.height /= 4;
            isCrouching = true;
        }
        else
        {
            playerMovement.setMovementSpeed(3.5f);
            playerMovement.disableRunning(false);
            characterController.height *= 4;
            isCrouching = false;
        }
    }

    void OnDestory()
    {
        for (int i = 0; i < inputActionReferences.Length; i++)
        {
            inputActionReferences[i].action.Disable();
        }
        inputActionReferences[0].action.performed -= crouch;
    }
}
