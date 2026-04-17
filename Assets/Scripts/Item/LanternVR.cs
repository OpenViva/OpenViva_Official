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

    private void Start()
    {
        _lanternKB = GetComponent<LanternKB>();
        _grabScript = GetComponent<PlayerVR_GrabObject>();

        _isOn = _lanternKB._isOn;
        _diode = _lanternKB._diode;

        _XRIButtonDownY.action.Enable();
        _XRIButtonDownB.action.Enable();
    }

    private void Update()
    {
        _isGrabbed = _grabScript.IsGrabbed;

        if (_isGrabbed != 0)
        {
            if (!_doOnce) return;

            if (_isGrabbed == 1)
            {
                _controlHintsL.CreateHint(HintConstants.LeftLanternHintVR);
                _XRIButtonDownB.action.performed -= ToggleLantern;
                _XRIButtonDownY.action.performed += ToggleLantern;
            }
            else if (_isGrabbed == 2)
            {
                _controlHintsR.CreateHint(HintConstants.RightLanternHintVR);
                _XRIButtonDownY.action.performed -= ToggleLantern;
                _XRIButtonDownB.action.performed += ToggleLantern;  
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

    private void ToggleLantern(InputAction.CallbackContext context)
    {
        if (_isGrabbed == 0) return;

        _diode.SetActive(!_isOn);
        _isOn = !_isOn;
    }
}
