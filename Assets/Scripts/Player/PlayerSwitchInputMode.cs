using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class PlayerSwitchInputMode : MonoBehaviour
{
    [SerializeField] private Player _player;

    [Header("KB Components:")]
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private GameObject _playerHands;

    [Header("VR Components:")]
    [SerializeField] private XROrigin _XROrigin;
    [SerializeField] private GameObject _VRLocomotion;
    [SerializeField] private XRInputModalityManager _XRInputModalityManager;
    [SerializeField] private TrackedPoseDriver _cameraTrackedPoseDriver;
    [SerializeField] private GameObject _leftController;
    [SerializeField] private GameObject _rightController;

    [Header("Misc:")]
    [SerializeField] private GameObject _camera;
    private Transform _initialCameraTransform;
    [SerializeField] private bool _VRActive = false;

    // Debug
    private bool _assignmentFailed = false;

    private void Awake()
    {
        _initialCameraTransform = _camera.transform;
    }

    private void OnEnable()
    {
        if (_player.Controls != null) { _player.Controls.Viva.ChangeInputType.performed += OnSwitchAction; }
        else { _assignmentFailed = true; }
    }

    private void Start()
    {
        if (_assignmentFailed)
        {
            _player.Controls.Viva.ChangeInputType.performed += OnSwitchAction;
            _assignmentFailed = false;
        }
    }

    private void OnSwitchAction(InputAction.CallbackContext context)
    {
        // Debug.Log("Method called.");

        _VRActive = !_VRActive;
        _camera.transform.SetPositionAndRotation(_initialCameraTransform.position, _initialCameraTransform.rotation);

        _playerController.enabled = !_VRActive;
        _playerHands.SetActive(!_VRActive);

        _XROrigin.enabled = _VRActive;
        _VRLocomotion.SetActive(_VRActive);
        _XRInputModalityManager.enabled = _VRActive;
        _cameraTrackedPoseDriver.enabled = _VRActive;
        _leftController.SetActive(_VRActive);
        _rightController.SetActive(_VRActive);
    }

    private void OnDisable()
    {
        _player.Controls.Viva.ChangeInputType.performed += OnSwitchAction;
    }
}