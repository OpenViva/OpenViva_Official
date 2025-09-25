using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerKB_BasicActions : MonoBehaviour
{
    // This class manages basic actions for a keyboard player.
    // Code by Saien

    // FIELDS
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerKB_Movement playerMovement;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject map;

    [SerializeField] private InputActionReference[] inputActionReferences = new InputActionReference[4];
    private bool isCrouching = false;
    private int currentHandPos = 10;
    private bool mapOpen = false;

    // PROPERTIES
    void Start()
    {
        for (int i = 0; i < inputActionReferences.Length; i++)
        {
            inputActionReferences[i].action.Enable();
        }
        inputActionReferences[0].action.performed += crouch;
        inputActionReferences[1].action.performed += extendHands;
        inputActionReferences[2].action.performed += retractHands;
        inputActionReferences[3].action.performed += changeMapVisibility;
    }

    void OnDestroy()
    {
        for (int i = 0; i < inputActionReferences.Length; i++)
        {
            inputActionReferences[i].action.Disable();
        }
        inputActionReferences[0].action.performed -= crouch;
        inputActionReferences[1].action.performed -= extendHands;
        inputActionReferences[2].action.performed -= retractHands;
        inputActionReferences[3].action.performed -= changeMapVisibility;
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

    private void extendHands(InputAction.CallbackContext context)
    {
        if (currentHandPos <= 50)
        {
            playerPrefab.transform.Translate(Vector3.right * 0.01f);
            currentHandPos++;
        }
        
    }

    private void retractHands(InputAction.CallbackContext context)
    {
        if (currentHandPos >= 0)
        {
            playerPrefab.transform.Translate(Vector3.left * 0.01f);
            currentHandPos--;
        }
    }

    private void changeMapVisibility(InputAction.CallbackContext context)
    {
        mapOpen = !mapOpen;
        map.SetActive(mapOpen);
    }
}
