using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightKB : MonoBehaviour
{
    private PlayerKB_GrabObject _grabScript;
    private int _isGrabbed = 0;
    [SerializeField] public bool _isOn;
    [SerializeField] public GameObject _diode;
    [SerializeField] public GameObject _vfx;

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

        _controls.Viva.Interact.performed += ToggleFlashlight;
    }

    private void Update()
    {
        _isGrabbed = _grabScript.GetIsGrabbed();

        if (_isGrabbed != 0)
        {
            if (_doOnce)
            {
                _hud.CreateHint(HintConstants.FlashlightHint);
                _doOnce = false;
            }
        }
        else
        {
            _hud.ClearHint(HintConstants.FlashlightHint);
            _doOnce = true;
        }
    }

    public void ToggleFlashlight(InputAction.CallbackContext context)
    {
        if (_isGrabbed == 0) return;

        _diode.SetActive(!_isOn);
        _vfx.SetActive(!_isOn);
        _isOn = !_isOn;
    }
}