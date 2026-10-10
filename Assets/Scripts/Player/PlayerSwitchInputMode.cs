using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine.XR.Management;

public class PlayerSwitchInputMode : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private GameObject _playerPrefab;

    [Header("KB Components:")]
    [SerializeField] private PlayerController _playerController;

    [Header("VR Components:")]
    private XROrigin _XROrigin;
    private XRInputModalityManager _XRInputModalityManager;
    [SerializeField] private InputActionManager _XRInputActionMananger;
    [SerializeField] private XRInteractionManager _XRInteractionManager;
    [SerializeField] private XRTransformStabilizer _teleportStablizedOrigin;

    private ControllerInputActionManager _controllerInputActionManager;
    private XRInteractionGroup _XRInteractionGroup;

    [Header("VR Locomotion")]
    [SerializeField] private GameObject _VRLocomotion;
    [SerializeField] private GameObject _XRPokeInteractor;
    [SerializeField] private GameObject _nearFarInteractor;
    [SerializeField] private GameObject _teleportInteractor;

    [Header("Tracked GameObjects:")]
    [SerializeField] private GameObject _camera;
    [SerializeField] private GameObject _leftWrist;
    [SerializeField] private GameObject _rightWrist;

    private Vector3 _cameraOriginalPosition;
    private Vector3 _leftWristOriginalPosition;
    private Vector3 _rightWristOriginalPosition;

    private Quaternion _cameraOriginalRotation;
    private Quaternion _leftWristOriginalRotation;
    private Quaternion _rightWristOriginalRotation;

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
        _XROrigin = GetComponent<XROrigin>();
        _XRInputModalityManager = GetComponent<XRInputModalityManager>();
        _controllerInputActionManager = _rightWrist.GetComponent<ControllerInputActionManager>();
        _XRInteractionGroup = _rightWrist.GetComponent<XRInteractionGroup>();

        _cameraOriginalPosition = _camera.transform.localPosition;
        _cameraOriginalRotation = _camera.transform.localRotation;

        _leftWristOriginalPosition = _leftWrist.transform.localPosition;
        _leftWristOriginalRotation = _leftWrist.transform.localRotation;

        _rightWristOriginalPosition = _rightWrist.transform.localPosition;
        _rightWristOriginalRotation = _rightWrist.transform.localRotation;

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
        PerformSwitch();
    }

    public void SwitchInputMode()
    {
        _VRActive = !_VRActive;
        PerformSwitch();
    }

    // Extra overload method in case it's needed.
    private void SwitchInputMode(bool toVRActive)
    {
        _VRActive = toVRActive;
        PerformSwitch();
    }

    private void PerformSwitch()
    {
        // KB Components
        _playerController.enabled = !_VRActive;

        // VR Components
        _XROrigin.enabled = _VRActive;
        _XRInputModalityManager.enabled = _VRActive;
        _XRInputModalityManager.enabled = _VRActive;
        _XRInteractionManager.enabled = _VRActive;
        _teleportStablizedOrigin.enabled = _VRActive;
        _controllerInputActionManager.enabled = _VRActive;
        _XRInteractionGroup.enabled = _VRActive;

        _VRLocomotion.SetActive(_VRActive);
        _XRPokeInteractor.SetActive(_VRActive);
        _nearFarInteractor.SetActive(_VRActive);
        _teleportInteractor.SetActive(_VRActive);

        _cameraTPD.enabled = _VRActive;
        _leftWristTPD.enabled = _VRActive;
        _rightWristTPD.enabled = _VRActive;

        if (!_VRActive)
        {
            _playerPrefab.transform.SetParent(_camera.transform);

            _camera.transform.localPosition = _cameraOriginalPosition;
            _camera.transform.localRotation = _cameraOriginalRotation;
            _leftWrist.transform.localPosition = _leftWristOriginalPosition;
            _leftWrist.transform.localRotation = _leftWristOriginalRotation;
            _rightWrist.transform.localPosition = _rightWristOriginalPosition;
            _rightWrist.transform.localRotation = _rightWristOriginalRotation;
        }

        if (_VRActive)
        {
            _playerPrefab.transform.SetParent(_player.transform);

            XRGeneralSettings.Instance.Manager.InitializeLoader();
            XRGeneralSettings.Instance.Manager.StartSubsystems();
        }
        else if (!_VRActive && XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }

        Globals.isDesktopMode = !_VRActive;
    }

    private void OnDisable()
    {
        _player.Controls.Viva.ChangeInputType.performed += SwitchInputMode;
    }

}