#if UNITY_ANDROID || UNITY_EDITOR || UNITY_STANDALONE_WIN

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages switching between Desktop (KBM) and VR player rigs.
/// Works with the new HandGrabSystem — queries held items from HandGrabSystem instead of old PlayerKB_GrabObject.
/// </summary>
public class PlayerManager : MonoBehaviour
{
    [SerializeField] private Player_InputTypes.InputType _inputType;
    [SerializeField] private InputActionReference _changeInputType;
    [SerializeField] private GameObject _playerKB;
    [SerializeField] private GameObject _playerVR;

    [Header("Hand References (New System)")]
    [Tooltip("Left hand HandGrabSystem on the KBM player")]
    [SerializeField] private HandGrabSystem _leftHandGrab;
    [Tooltip("Right hand HandGrabSystem on the KBM player")]
    [SerializeField] private HandGrabSystem _rightHandGrab;

    void Start()
    {
        if (_changeInputType != null)
        {
            _changeInputType.action.Enable();
            _changeInputType.action.performed += ChangeInputType;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Globals.isDesktopMode = true;
    }

    void FixedUpdate()
    {
        if (_playerKB == null || _playerVR == null) return;

        if (_inputType == Player_InputTypes.InputType.KBM)
            _playerVR.transform.position = _playerKB.transform.position;
        else
            _playerKB.transform.position = _playerVR.transform.position;
    }

    void OnDestroy()
    {
        if (_changeInputType != null)
        {
            _changeInputType.action.Disable();
            _changeInputType.action.performed -= ChangeInputType;
        }
    }

    void ChangeInputType(InputAction.CallbackContext context)
    {
        if (_inputType == Player_InputTypes.InputType.KBM)
        {
            _inputType = Player_InputTypes.InputType.VR;
            _playerKB.SetActive(false);
            _playerVR.SetActive(true);
            Globals.isDesktopMode = false;
            Debug.Log("Input type changed to VR");
        }
        else
        {
            _inputType = Player_InputTypes.InputType.KBM;
            _playerVR.SetActive(false);
            _playerKB.SetActive(true);
            Globals.isDesktopMode = true;
            Debug.Log("Input type changed to KBM");
        }
    }

    /// <summary>
    /// Get the item held in the left hand (new system).
    /// Returns -1 if nothing held, or the HoldPose index.
    /// </summary>
    public int GetItemLeft()
    {
        if (_leftHandGrab != null && _leftHandGrab.IsHolding)
            return (int)_leftHandGrab.HeldItem.holdPose;
        return -1;
    }

    /// <summary>
    /// Get the item held in the right hand (new system).
    /// Returns -1 if nothing held, or the HoldPose index.
    /// </summary>
    public int GetItemRight()
    {
        if (_rightHandGrab != null && _rightHandGrab.IsHolding)
            return (int)_rightHandGrab.HeldItem.holdPose;
        return -1;
    }

    /// <summary>
    /// Get the actual GameObject held in the left hand.
    /// </summary>
    public GameObject GetObjectLeft()
    {
        if (_leftHandGrab != null && _leftHandGrab.IsHolding)
            return _leftHandGrab.HeldItem.gameObject;
        return null;
    }

    /// <summary>
    /// Get the actual GameObject held in the right hand.
    /// </summary>
    public GameObject GetObjectRight()
    {
        if (_rightHandGrab != null && _rightHandGrab.IsHolding)
            return _rightHandGrab.HeldItem.gameObject;
        return null;
    }
}

#endif