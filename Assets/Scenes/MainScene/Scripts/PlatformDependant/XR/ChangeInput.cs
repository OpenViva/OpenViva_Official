#if UNITY_ANDROID || UNITY_EDITOR

using UnityEngine;
using UnityEngine.InputSystem;

// Class that manages the player

public class Player_Manager : MonoBehaviour
{

    [SerializeField] private Player_InputTypes.InputType _inputType; // Current input type
    [SerializeField] private InputActionReference changeInputType; // Keybind to change input type ([1] key)
    [SerializeField] private GameObject PlayerKB; // Player GameObject for Keyboard/Mouse
    [SerializeField] private GameObject PlayerVR; // Player GameObject for VR

    void Start()
    {
        // Set up change input type action
        changeInputType.action.Enable();
        changeInputType.action.performed += ChangeInputType;
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;

        Globals.isDesktopMode = true;
    }

    private void FixedUpdate()
    {
        // Sync positions between the KBM and VR player objects
        if (_inputType == Player_InputTypes.InputType.KBM)
        {
            PlayerVR.transform.position = PlayerKB.transform.position;
        }
        else
        {
            PlayerKB.transform.position = PlayerVR.transform.position;
        }
    }

    void OnDestroy()
    {
        // Clean up change input type action
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
            Globals.isDesktopMode = true;
            Debug.Log("Input type changed to VR");
        }
        else
        {
            _inputType = Player_InputTypes.InputType.KBM;
            PlayerVR.SetActive(false);
            PlayerKB.SetActive(true);
            Globals.isDesktopMode = false;
            Debug.Log("Input type changed to KBM");
        }
    }
}

#endif