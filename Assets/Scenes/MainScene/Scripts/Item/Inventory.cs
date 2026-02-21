using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Bag inventory system. Attach to the Bag GrabbableItem.
/// When the bag is held and Q is pressed, it opens/closes.
/// While open: the item in the other hand can be stored, scroll selects stored items, E takes one out.
/// Works with the new HandGrabSystem/GrabbableItem system.
/// </summary>
public class Inventory : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Left hand HandGrabSystem")]
    [SerializeField] private HandGrabSystem _leftHandGrab;
    [Tooltip("Right hand HandGrabSystem")]
    [SerializeField] private HandGrabSystem _rightHandGrab;
    [SerializeField] private PlayerKB_HUD _hud;

    [Header("Settings")]
    [SerializeField] private int _maxItems = 10;

    // Bag state
    GrabbableItem _bagItem;
    Animator _animator;
    bool _isOpen;
    DesktopInput _keybinds;

    // Stored items
    [SerializeField] List<string> _inventoryNames = new List<string>();
    List<GameObject> _storedObjects = new List<GameObject>();
    int _selectedIndex; // 0-based

    // HUD tracking
    bool _wasHeld;

    void Start()
    {
        _bagItem = GetComponent<GrabbableItem>();
        _animator = GetComponent<Animator>();

        _keybinds = new DesktopInput();
        _keybinds.Viva.Enable();
        _keybinds.Viva.ToggleBag.performed += OnToggleBag;
        _keybinds.Viva.LeftGrab.performed += OnLeftClick;
        _keybinds.Viva.RightGrab.performed += OnRightClick;
        _keybinds.Viva.ScrollUp.performed += OnScrollUp;
        _keybinds.Viva.ScrollDown.performed += OnScrollDown;
        _keybinds.Viva.Interact.performed += OnTakeItem;
    }

    void Update()
    {
        bool isHeld = _bagItem != null && _bagItem.IsHeld;

        // Show/clear hints when bag is first picked up or dropped
        if (isHeld && !_wasHeld)
        {
            if (_hud != null)
                _hud.CreateHint("[Q]: Open Bag");
        }
        else if (!isHeld && _wasHeld)
        {
            CloseBag();
            if (_hud != null)
            {
                _hud.ClearHint("[Q]: Open Bag");
                _hud.ClearHint("[Q]: Close Bag");
                _hud.ClearHint("[scrollwheel]: Select Items");
                _hud.ClearHint("[E]: Take Item");
            }
        }

        _wasHeld = isHeld;
    }

    /// <summary>
    /// Which hand is the bag NOT in? Returns the other hand's HandGrabSystem, or null.
    /// </summary>
    HandGrabSystem GetOtherHand()
    {
        if (_bagItem == null || !_bagItem.IsHeld) return null;

        if (_bagItem.heldByHand == _leftHandGrab)
            return _rightHandGrab;
        if (_bagItem.heldByHand == _rightHandGrab)
            return _leftHandGrab;

        return null;
    }

    bool IsBagHeld => _bagItem != null && _bagItem.IsHeld;

    void OnToggleBag(InputAction.CallbackContext ctx)
    {
        if (!IsBagHeld) return;

        if (_isOpen)
            CloseBag();
        else
            OpenBag();
    }

    void OpenBag()
    {
        if (_isOpen) return;
        _isOpen = true;

        if (_animator != null)
            _animator.Play("Open");

        if (_hud != null)
        {
            _hud.ClearHint("[Q]: Open Bag");
            _hud.CreateHint("[Q]: Close Bag");
            _hud.CreateHint("[scrollwheel]: Select Items");
            _hud.CreateHint("[E]: Take Item");
        }
    }

    void CloseBag()
    {
        if (!_isOpen) return;
        _isOpen = false;

        if (_animator != null)
            _animator.Play("Close");

        if (_hud != null)
        {
            _hud.ClearHint("[Q]: Close Bag");
            _hud.ClearHint("[scrollwheel]: Select Items");
            _hud.ClearHint("[E]: Take Item");
            if (IsBagHeld)
                _hud.CreateHint("[Q]: Open Bag");
        }
    }

    /// <summary>
    /// Try to store the item from the other hand into the bag.
    /// </summary>
    void TryStoreItem()
    {
        if (!_isOpen || _storedObjects.Count >= _maxItems) return;

        var otherHand = GetOtherHand();
        if (otherHand == null || !otherHand.IsHolding) return;

        var item = otherHand.HeldItem;
        if (item == null || item == _bagItem) return; // Don't store the bag in itself

        // Release item from hand without throw
        otherHand.ReleaseItem(false);

        // Store it
        _inventoryNames.Add(item.gameObject.name);
        _storedObjects.Add(item.gameObject);
        item.gameObject.SetActive(false);

        if (_storedObjects.Count == 1)
            _selectedIndex = 0;

        Debug.Log($"Stored: {item.gameObject.name} ({_storedObjects.Count}/{_maxItems})");
    }

    void OnLeftClick(InputAction.CallbackContext ctx)
    {
        // If bag is open and held in left hand, try to store the right hand's item
        if (_isOpen && IsBagHeld && _bagItem.heldByHand == _leftHandGrab)
            TryStoreItem();
    }

    void OnRightClick(InputAction.CallbackContext ctx)
    {
        // If bag is open and held in right hand, try to store the left hand's item
        if (_isOpen && IsBagHeld && _bagItem.heldByHand == _rightHandGrab)
            TryStoreItem();
    }

    void OnScrollUp(InputAction.CallbackContext ctx)
    {
        if (!_isOpen || _storedObjects.Count == 0) return;

        if (_selectedIndex < _storedObjects.Count - 1)
        {
            _selectedIndex++;
            Debug.Log($"Selected: {_inventoryNames[_selectedIndex]}");
        }
    }

    void OnScrollDown(InputAction.CallbackContext ctx)
    {
        if (!_isOpen || _storedObjects.Count == 0) return;

        if (_selectedIndex > 0)
        {
            _selectedIndex--;
            Debug.Log($"Selected: {_inventoryNames[_selectedIndex]}");
        }
    }

    void OnTakeItem(InputAction.CallbackContext ctx)
    {
        if (!_isOpen || _storedObjects.Count == 0) return;

        var otherHand = GetOtherHand();
        if (otherHand == null || otherHand.IsHolding) return; // Other hand must be free

        // Retrieve the stored item
        int idx = Mathf.Clamp(_selectedIndex, 0, _storedObjects.Count - 1);
        GameObject itemObj = _storedObjects[idx];
        _storedObjects.RemoveAt(idx);
        _inventoryNames.RemoveAt(idx);

        // Re-activate and place in hand
        itemObj.SetActive(true);
        var grabbable = itemObj.GetComponent<GrabbableItem>();
        if (grabbable != null)
            otherHand.GrabItem(grabbable);

        // Adjust selection
        if (_storedObjects.Count == 0)
            _selectedIndex = 0;
        else if (_selectedIndex >= _storedObjects.Count)
            _selectedIndex = _storedObjects.Count - 1;

        Debug.Log($"Took: {itemObj.name} ({_storedObjects.Count} remaining)");
    }

    void OnDestroy()
    {
        if (_keybinds != null)
        {
            _keybinds.Viva.ToggleBag.performed -= OnToggleBag;
            _keybinds.Viva.LeftGrab.performed -= OnLeftClick;
            _keybinds.Viva.RightGrab.performed -= OnRightClick;
            _keybinds.Viva.ScrollUp.performed -= OnScrollUp;
            _keybinds.Viva.ScrollDown.performed -= OnScrollDown;
            _keybinds.Viva.Interact.performed -= OnTakeItem;
            _keybinds.Viva.Disable();
            _keybinds.Dispose();
        }
    }
}
