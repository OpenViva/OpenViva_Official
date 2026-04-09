using UnityEngine;
using UnityEngine.InputSystem;

public class LanternKB : MonoBehaviour
{
    private PlayerKB_GrabObject _grabScript;
    private int _isGrabbed = 0;
    [SerializeField] public bool _isOn;
    [SerializeField] public GameObject _diode;

    [SerializeField] private HintManager _hud;
    private bool _doOnce = true;

    // --- Fields ---
    private Player _player;
    private PlayerControls _controls;

    private void Start()
    {
        _grabScript = GetComponent<PlayerKB_GrabObject>();

        _player = FindFirstObjectByType<Player>();
        _controls = _player.Controls;

        _controls.Viva.InteractLeft.performed += ToggleLanternLeft;
        _controls.Viva.InteractRight.performed += ToggleLanternRight;
    }

    private void Update()
    {
        _isGrabbed = _grabScript.GetIsGrabbed();

        if (_doOnce)
        {
            if (_isGrabbed == 1)
            {
                _hud.CreateHint(HintConstants.LeftLanternHint);
                _doOnce = false;
            }
            else if (_isGrabbed == 2)
            {
                _hud.CreateHint(HintConstants.RightLanternHint);
                _doOnce = false;
            }
            else
            {
                _hud.ClearHint(HintConstants.LeftLanternHint);
                _hud.ClearHint(HintConstants.RightLanternHint);
                _doOnce = true;
            }
        }
    }

    private void ToggleLanternLeft(InputAction.CallbackContext context)
    {
        if (_isGrabbed == 1)
        {
            _diode.SetActive(!_isOn);
            _isOn = !_isOn;
        }
    }

    private void ToggleLanternRight(InputAction.CallbackContext context)
    {
        if (_isGrabbed == 2)
        {
            _diode.SetActive(!_isOn);
            _isOn = !_isOn;
        }
    }
}