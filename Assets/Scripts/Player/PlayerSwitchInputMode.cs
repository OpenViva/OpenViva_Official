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
    [SerializeField] private HandsFollowCamera _playerPrefab;

    [Header("VR Components:")]
    [SerializeField] private XROrigin _XROrigin;
    [SerializeField] private GameObject _VRLocomotion;
    [SerializeField] private GameObject _teleportStabilizedOrigin;

    [Header("Tracked GameObjects:")]
    [SerializeField] private GameObject _camera;
    [SerializeField] private GameObject _leftWrist;
    [SerializeField] private GameObject _rightWrist;

    private Transform _cameraOrigin;
    private Transform _leftWristOrigin;
    private Transform _rightWristOrigin;

    private TrackedPoseDriver _cameraTPD;
    private TrackedPoseDriver _leftWristTPD;
    private TrackedPoseDriver _rightWristTPD;

    [Header("Development:")]
    [Tooltip("Can be used to immediately enter playmode in VR, if you wish.")]
    [SerializeField] private bool _VRActive = false;

    // Debug
    private bool _assignmentFailed = true;

    private void Awake()
    {
        _cameraOrigin = _camera.transform;
        _leftWristOrigin = _leftWrist.transform;
        _rightWristOrigin = _rightWrist.transform;

        _cameraTPD = _camera.GetComponent<TrackedPoseDriver>();
        _leftWristTPD = _leftWrist.GetComponent<TrackedPoseDriver>();
        _rightWristTPD = _rightWrist.GetComponent<TrackedPoseDriver>();
    }

    private void OnEnable()
    {
        if (_player.Controls != null) 
        { 
            _player.Controls.Viva.ChangeInputType.performed += SwitchInputMode;
            _assignmentFailed = false;
        }
    }

    private void Start()
    {
        if (_assignmentFailed)
        {
            _player.Controls.Viva.ChangeInputType.performed += SwitchInputMode;
            _assignmentFailed = false;
        }

        // Editor intervention. Default should be false
        if (_VRActive) { SwitchInputMode(true); }
    }

    // This method will be replaced by one below once this script is attached to UI
    private void SwitchInputMode(InputAction.CallbackContext context)
    {
        _VRActive = !_VRActive;

        // KB Components
        _playerController.enabled = !_VRActive;
        _playerPrefab.enabled = !_VRActive;

        // VR Components
        _XROrigin.enabled = _VRActive;
        _VRLocomotion.SetActive(_VRActive);
        _teleportStabilizedOrigin.SetActive(_VRActive);
        _cameraTPD.enabled = _VRActive;
        _leftWristTPD.enabled = _VRActive;
        _rightWristTPD.enabled = _VRActive;

        if (!_VRActive)
        {
            _camera.transform.position = _cameraOrigin.position;
            _camera.transform.rotation = _cameraOrigin.rotation;
            _leftWrist.transform.position = _leftWristOrigin.position;
            _leftWrist.transform.rotation = _leftWristOrigin.rotation;
            _rightWrist.transform.position = _rightWristOrigin.position;
            _rightWrist.transform.rotation = _rightWristOrigin.rotation;
        }
    }

    private void SwitchInputMode()
    {
        _VRActive = !_VRActive;

        // KB Components
        _playerController.enabled = !_VRActive;
        _playerPrefab.enabled = !_VRActive;

        // VR Components
        _XROrigin.enabled = _VRActive;
        _VRLocomotion.SetActive(_VRActive);
        _teleportStabilizedOrigin.SetActive(_VRActive);
        _cameraTPD.enabled = _VRActive;
        _leftWristTPD.enabled = _VRActive;
        _rightWristTPD.enabled = _VRActive;

        if (!_VRActive)
        {
            _camera.transform.position = _cameraOrigin.position;
            _camera.transform.rotation = _cameraOrigin.rotation;
            _leftWrist.transform.position = _leftWristOrigin.position;
            _leftWrist.transform.rotation = _leftWristOrigin.rotation;
            _rightWrist.transform.position = _rightWristOrigin.position;
            _rightWrist.transform.rotation = _rightWristOrigin.rotation;
        }
    }

    // Extra overload method in case it's needed.
    private void SwitchInputMode(bool toVRActive)
    {
        _VRActive = toVRActive;

        // KB Components
        _playerController.enabled = !_VRActive;
        _playerPrefab.enabled = !_VRActive;

        // VR Components
        _XROrigin.enabled = _VRActive;
        _VRLocomotion.SetActive(_VRActive);
        _teleportStabilizedOrigin.SetActive(_VRActive);
        _cameraTPD.enabled = _VRActive;
        _leftWristTPD.enabled = _VRActive;
        _rightWristTPD.enabled = _VRActive;

        if (!_VRActive)
        {
            _camera.transform.position = _cameraOrigin.position;
            _camera.transform.rotation = _cameraOrigin.rotation;
            _leftWrist.transform.position = _leftWristOrigin.position;
            _leftWrist.transform.rotation = _leftWristOrigin.rotation;
            _rightWrist.transform.position = _rightWristOrigin.position;
            _rightWrist.transform.rotation = _rightWristOrigin.rotation;
        }
    }

    private void OnDisable()
    {
        _player.Controls.Viva.ChangeInputType.performed += SwitchInputMode;
    }
}