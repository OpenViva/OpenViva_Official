using UnityEngine;

public class VRInputAnimations : MonoBehaviour
{
    private Player _player;

    private readonly string GRIP_DOWN = "GripDown";
    private readonly string TRIGGER_DOWN = "TriggerDown";
    private readonly string THUMBSTICK_TOUCHED = "ThumbstickTouched";

    [Header("Animators")]
    [SerializeField] private Animator _leftAnimator; 
    [SerializeField] private Animator _rightAnimator;

    private bool _assignmentSuccessful = false;

    [Header("Development")]
    [Tooltip("Check this before entering playmode if you don't have a headset and want to test input actions." +
        "If checked, ThumbstickTouched is replaced with WASD instead.")]
    [SerializeField] private bool _headsetUnavailable = false;

    private void OnEnable()
    {
        if (_player != null) AssignInputs();
    }

    private void Start()
    {
        _player = FindFirstObjectByType<Player>();
        if (!_assignmentSuccessful) AssignInputs();
    }

    private void AssignInputs()
    {
        // Point
        _player.Controls.Viva.VRLeftDrop.started += context => _leftAnimator.SetBool(GRIP_DOWN, true);
        _player.Controls.Viva.VRRightDrop.started += context => _rightAnimator.SetBool(GRIP_DOWN, true);
        _player.Controls.Viva.VRLeftDrop.canceled += context => _leftAnimator.SetBool(GRIP_DOWN, false);
        _player.Controls.Viva.VRRightDrop.canceled += context => _rightAnimator.SetBool(GRIP_DOWN, false);
        
        // Thumbs up
        _player.Controls.Viva.VRLeftGrab.started += context => _leftAnimator.SetBool(TRIGGER_DOWN, true);
        _player.Controls.Viva.VRRightGrab.started += context => _rightAnimator.SetBool(TRIGGER_DOWN, true);
        _player.Controls.Viva.VRLeftGrab.canceled += context => _leftAnimator.SetBool(TRIGGER_DOWN, false);
        _player.Controls.Viva.VRRightGrab.canceled += context => _rightAnimator.SetBool(TRIGGER_DOWN, false);

        // Fist
        if (_headsetUnavailable)
        {
            _player.Controls.Viva.Move.started += context => _leftAnimator.SetBool(THUMBSTICK_TOUCHED, true);
            _player.Controls.Viva.Move.started += context => _rightAnimator.SetBool(THUMBSTICK_TOUCHED, true);
            _player.Controls.Viva.Move.canceled += context => _leftAnimator.SetBool(THUMBSTICK_TOUCHED, false);
            _player.Controls.Viva.Move.canceled += context => _rightAnimator.SetBool(THUMBSTICK_TOUCHED, false);
        }
        else
        {
            _player.Controls.Viva.VRLeftThumbstickTouched.started += context => _leftAnimator.SetBool(THUMBSTICK_TOUCHED, true);
            _player.Controls.Viva.VRLeftThumbstickTouched.started += context => _rightAnimator.SetBool(THUMBSTICK_TOUCHED, true);
            _player.Controls.Viva.VRRightThumbstickTouched.canceled += context => _leftAnimator.SetBool(THUMBSTICK_TOUCHED, false);
            _player.Controls.Viva.VRRightThumbstickTouched.canceled += context => _rightAnimator.SetBool(THUMBSTICK_TOUCHED, false);
        }

        _assignmentSuccessful = true;
    }

    private void UnassignInputs()
    {
        // Point
        _player.Controls.Viva.VRLeftDrop.started -= context => _leftAnimator.SetBool(GRIP_DOWN, true);
        _player.Controls.Viva.VRRightDrop.started -= context => _rightAnimator.SetBool(GRIP_DOWN, true);
        _player.Controls.Viva.VRLeftDrop.canceled -= context => _leftAnimator.SetBool(GRIP_DOWN, false);
        _player.Controls.Viva.VRRightDrop.canceled -= context => _rightAnimator.SetBool(GRIP_DOWN, false);

        // Thumbs up
        _player.Controls.Viva.VRLeftGrab.started -= context => _leftAnimator.SetBool(TRIGGER_DOWN, true);
        _player.Controls.Viva.VRRightGrab.started -= context => _rightAnimator.SetBool(TRIGGER_DOWN, true);
        _player.Controls.Viva.VRLeftGrab.canceled -= context => _leftAnimator.SetBool(TRIGGER_DOWN, false);
        _player.Controls.Viva.VRRightGrab.canceled -= context => _rightAnimator.SetBool(TRIGGER_DOWN, false);

        // Fist
        if (_headsetUnavailable)
        {
            _player.Controls.Viva.Move.started -= context => _leftAnimator.SetBool(THUMBSTICK_TOUCHED, true);
            _player.Controls.Viva.Move.started -= context => _rightAnimator.SetBool(THUMBSTICK_TOUCHED, true);
            _player.Controls.Viva.Move.canceled -= context => _leftAnimator.SetBool(THUMBSTICK_TOUCHED, false);
            _player.Controls.Viva.Move.canceled -= context => _rightAnimator.SetBool(THUMBSTICK_TOUCHED, false);
        }
        else
        {
            _player.Controls.Viva.VRLeftThumbstickTouched.started -= context => _leftAnimator.SetBool(THUMBSTICK_TOUCHED, true);
            _player.Controls.Viva.VRLeftThumbstickTouched.started -= context => _rightAnimator.SetBool(THUMBSTICK_TOUCHED, true);
            _player.Controls.Viva.VRRightThumbstickTouched.canceled -= context => _leftAnimator.SetBool(THUMBSTICK_TOUCHED, false);
            _player.Controls.Viva.VRRightThumbstickTouched.canceled -= context => _rightAnimator.SetBool(THUMBSTICK_TOUCHED, false);
        }
    }

    private void OnDisable()
    {
        if (_player != null) UnassignInputs();
    }
}
