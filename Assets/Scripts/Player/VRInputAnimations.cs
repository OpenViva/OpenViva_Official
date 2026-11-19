using UnityEngine;

public class VRInputAnimations : MonoBehaviour
{
    private Player _player;

    private readonly string GRIP_DOWN = "GripDown";

    [Header("Animators")]
    [SerializeField] private Animator _leftAnimator; 
    [SerializeField] private Animator _rightAnimator;

    private bool _assignmentSuccessful = false;

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
        _player.Controls.Viva.VRLeftDrop.started += context => _leftAnimator.SetBool(GRIP_DOWN, true);
        _player.Controls.Viva.VRLeftDrop.canceled += context => _leftAnimator.SetBool(GRIP_DOWN, false);

        _player.Controls.Viva.VRRightDrop.started += context => _rightAnimator.SetBool(GRIP_DOWN, true);
        _player.Controls.Viva.VRRightDrop.canceled += context => _rightAnimator.SetBool(GRIP_DOWN, false);

        _assignmentSuccessful = true;
    }

    private void UnassignInputs()
    {
        _player.Controls.Viva.VRLeftDrop.started -= context => _leftAnimator.SetBool(GRIP_DOWN, true);
        _player.Controls.Viva.VRLeftDrop.canceled -= context => _leftAnimator.SetBool(GRIP_DOWN, false);

        _player.Controls.Viva.VRRightDrop.started -= context => _rightAnimator.SetBool(GRIP_DOWN, true);
        _player.Controls.Viva.VRRightDrop.canceled -= context => _rightAnimator.SetBool(GRIP_DOWN, false);
    }

    private void OnDisable()
    {
        if (_player != null) UnassignInputs();
    }
}
