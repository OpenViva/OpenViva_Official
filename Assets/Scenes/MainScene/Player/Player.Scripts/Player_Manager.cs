using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Manager : MonoBehaviour
{
    // This script manages the player in the game.
    // Code by Saien

    // FIELDS
    [SerializeField] private Player_InputTypes.InputType _inputType;
    [SerializeField] private InputActionReference changeInputType;
    [SerializeField] private GameObject PlayerVR;
    [SerializeField] private GameObject PlayerKB;

    // PROPERTIES
    void Start()
    {
        changeInputType.action.Enable();
        changeInputType.action.performed += ChangeInputType;
    }

    void Update()
    {
        if (_inputType == Player_InputTypes.InputType.KBM)
        {
            this.transform.position = PlayerKB.transform.position;
        }
        else
        {
            this.transform.position = PlayerVR.transform.position;
        }
    }

    void OnDestroy()
    {
        changeInputType.action.Disable();
        changeInputType.action.performed -= ChangeInputType;
    }

    private void ChangeInputType(InputAction.CallbackContext context)
    {
        // Toggle between Keyboard/Mouse and VR input types
        if (_inputType == Player_InputTypes.InputType.KBM)
        {
            _inputType = Player_InputTypes.InputType.VR;
            PlayerKB.SetActive(false);
            PlayerVR.SetActive(true);
            Debug.Log("Input type changed to VR");
        }
        else
        {
            _inputType = Player_InputTypes.InputType.KBM;
            PlayerVR.SetActive(false);
            PlayerKB.SetActive(true);
            Debug.Log("Input type changed to KBM");
        }
    }
}
