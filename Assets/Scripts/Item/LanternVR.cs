using UnityEngine;
using UnityEngine.InputSystem;

public class LanternVR : MonoBehaviour
{
    [SerializeField] private InputActionReference _XRIButtonDownY;
    [SerializeField] private InputActionReference _XRIButtonDownB;

    [SerializeField] private HintManager _controlHintsL;
    [SerializeField] private HintManager _controlHintsR;

    LanternKB _lanternKB;
    private PlayerVR_GrabObject _grabScript;
    private int _isGrabbed = 0;
    private bool _doOnce = true;

    private bool _isOn;
    private GameObject _diode;
    private GameObject _vfx;

    private void Start()
    {
        _lanternKB = GetComponent<LanternKB>();
        _grabScript = GetComponent<PlayerVR_GrabObject>();

        _isOn = _lanternKB._isOn;
        _diode = _lanternKB._diode;

        _XRIButtonDownY.action.performed += ToggleFlashlight;
        _XRIButtonDownB.action.performed += ToggleFlashlight;
        _XRIButtonDownY.action.Enable();
        _XRIButtonDownB.action.Enable();
    }

    private void Update()
    {
        _isGrabbed = _grabScript.GetIsGrabbed();

        if (_isGrabbed != 0)
        {
            if (!_doOnce) return;

            if (_isGrabbed == 1)
            {
                _controlHintsL.CreateHint(HintConstants.LeftLanternHintVR);
            }
            else if (_isGrabbed == 2)
            {
                _controlHintsR.CreateHint(HintConstants.RightLanternHintVR);
            }

            _doOnce = false;
        }
        else
        {
            _controlHintsL.ClearHint(HintConstants.LeftLanternHintVR);
            _controlHintsR.ClearHint(HintConstants.RightLanternHintVR);
            _doOnce = true;
        }
    }

    private void ToggleFlashlight(InputAction.CallbackContext context)
    {
        if (_isGrabbed == 0) return;

        _diode.SetActive(!_isOn);
        _isOn = !_isOn;
    }
}
