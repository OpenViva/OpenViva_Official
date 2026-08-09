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
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponentInChildren<AudioSource>();
    }

    private void Start()
    {
        _grabScript = GetComponent<PlayerKB_GrabObject>();

        _player = FindFirstObjectByType<Player>();
        _controls = _player.Controls;

        _controls.Viva.InteractLeft.performed += ToggleFlashlightLeft;
        _controls.Viva.InteractRight.performed += ToggleFlashlightRight;
    }

    private void Update()
    {
        _isGrabbed = _grabScript.GetIsGrabbed();

        if (_doOnce)
        {
            if (_isGrabbed == 1 && _doOnce)
            {
                _hud.CreateHint(HintConstants.LeftFlashlightHint);
                _doOnce = false;
            }
            else if (_isGrabbed == 2 && _doOnce)
            {
                _hud.CreateHint(HintConstants.RightFlashlightHint);
                _doOnce = false;
            }
        }
        else
        {
            if (_isGrabbed == 0)
            {
                _hud.ClearHint(HintConstants.LeftFlashlightHint);
                _hud.ClearHint(HintConstants.RightFlashlightHint);
                _doOnce = true;
            }
        }
    }

    public void ToggleFlashlightLeft(InputAction.CallbackContext context)
    {
        if (_isGrabbed == 1)
        {
            _diode.SetActive(!_isOn);
            _vfx.SetActive(!_isOn);
            _isOn = !_isOn;
            _audioSource?.Play();
        }
    }

    public void ToggleFlashlightRight(InputAction.CallbackContext context)
    {
        if (_isGrabbed == 2)
        {
            _diode.SetActive(!_isOn);
            _vfx.SetActive(!_isOn);
            _isOn = !_isOn;
            _audioSource?.Play();
        }
    }

    private void OnDisable()
    {
        if (_isGrabbed == 1)
        {
            _hud.ClearHint(HintConstants.LeftFlashlightHint);
            _hud.ClearHint(HintConstants.LeftReleaseHint);
        }
        else
        {
            _hud.ClearHint(HintConstants.RightFlashlightHint);
            _hud.ClearHint(HintConstants.RightReleaseHint);
        }
    }
}