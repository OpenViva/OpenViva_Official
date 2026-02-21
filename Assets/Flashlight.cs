using UnityEngine;

/// <summary>
/// Flashlight item — toggles light on/off when interacted with (E key) while held.
/// Uses the new GrabbableItem + IInteractable system.
/// </summary>
public class Flashlight : MonoBehaviour, IInteractable
{
    [SerializeField] private bool _isOn;
    [SerializeField] private GameObject _diode;
    [SerializeField] private GameObject _vfx;

    GrabbableItem _grabbable;

    void Start()
    {
        _grabbable = GetComponent<GrabbableItem>();
    }

    public bool CanInteract => _grabbable != null && _grabbable.IsHeld;

    public void Interact(GameObject interactor)
    {
        if (!CanInteract) return;

        _isOn = !_isOn;
        if (_diode != null)
            _diode.SetActive(_isOn);
        if (_vfx != null)
            _vfx.SetActive(_isOn);
    }
}
