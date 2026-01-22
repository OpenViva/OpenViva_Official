using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{
    private PlayerKB_GrabObject _grabScript;
    private int _isGrabbed = 0;
    private DesktopInput _keybinds;
    [SerializeField] private bool _isOn;
    [SerializeField] private GameObject _diode;
    [SerializeField] private GameObject _vfx;

    [SerializeField] private PlayerKB_HUD _hud;
    private bool _doOnce = true;

    private void Start()
    {
        _grabScript = GetComponent<PlayerKB_GrabObject>();

        _keybinds = new DesktopInput();
        _keybinds.Viva.Enable();
        _keybinds.Viva.Interact.performed += ToggleFlashlight;
    }

    private void Update()
    {
        _isGrabbed = _grabScript.GetIsGrabbed();

        if (_isGrabbed != 0)
        {
            if (_doOnce)
            {
                _hud.CreateHint("[E]: Toggle Flashlight");
                _doOnce = false;
            }
        }
        else
        {
            _hud.ClearHint("[E]: Toggle Flashlight");
            _doOnce = true;
        }
    }

    private void ToggleFlashlight(InputAction.CallbackContext context)
    {
        if (_isGrabbed == 0) return;

        _diode.SetActive(!_isOn);
        if (_vfx != null)
        {
            _vfx.SetActive(!_isOn);
        }
        _isOn = !_isOn;
    }

    private void OnDestroy()
    {
        _keybinds.Viva.Interact.performed -= ToggleFlashlight;
        _keybinds.Disable();
    }
}
