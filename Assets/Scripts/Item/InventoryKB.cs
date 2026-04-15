using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryKB : MonoBehaviour
{
    private Player _player;
    [SerializeField] private HintManager _hud;
    private PlayerKB_GrabObject _grabScript;
    private Animator _animator;
    private ItemIndexes _itemIndexes;

    private int _grabbedIn;
    private bool _isOpen;

    private List<ItemInList> _inventory = new();
    [SerializeField] private int _maxInventorySize = 10;
    private int _selectedItem = 0;

    [Serializable]
    private class ItemInList
    {
        private string name;
        public GameObject item;

        public ItemInList(string name, GameObject item)
        {
            this.name = name;
            this.item = item;
        }
    }

    private void Start()
    {
        _player = FindFirstObjectByType<Player>();
        _animator = GetComponent<Animator>();
        _grabScript = GetComponent<PlayerKB_GrabObject>();
        _itemIndexes = new ItemIndexes();

        _player.Controls.Viva.ScrollDown.performed += ScrollDown;
        _player.Controls.Viva.ScrollUp.performed += ScrollUp;
    }

    private void Update()
    {
        int fromScript = _grabScript.GetIsGrabbed();
        if (_grabbedIn != fromScript)
        {
            SwitchInputs(fromScript);
            ShowHints(fromScript);
            _grabbedIn = fromScript;
        }
    }

    private void SwitchInputs(int handedness)
    {
        if (handedness == 1)
        {
            _player.Controls.Viva.InteractRight.performed -= ToggleBag;
            _player.Controls.Viva.InteractLeft.performed += ToggleBag;
            _player.Controls.Viva.RightGrab.performed -= PlaceInBag;
            _player.Controls.Viva.LeftGrab.performed += PlaceInBag;
            _player.Controls.Viva.InteractLeft.performed -= TakeFromBag;
            _player.Controls.Viva.InteractRight.performed += TakeFromBag;
        }

        if (handedness == 2)
        {
            _player.Controls.Viva.InteractLeft.performed -= ToggleBag;
            _player.Controls.Viva.InteractRight.performed += ToggleBag;
            _player.Controls.Viva.LeftGrab.performed -= PlaceInBag;
            _player.Controls.Viva.RightGrab.performed += PlaceInBag;
            _player.Controls.Viva.InteractRight.performed -= TakeFromBag;
            _player.Controls.Viva.InteractLeft.performed += TakeFromBag;
        }
    }

    private void ToggleBag(InputAction.CallbackContext context)
    {
        if (_isOpen)
        {
            _animator.Play("Close");
            _isOpen = false;
            _grabScript.SetIsOpen(false);
            PlayerKB_BasicActions.Instance.IsBagOpen = false;
        }
        else
        {
            _animator.Play("Open");
            _isOpen = true;
            _grabScript.SetIsOpen(true);
            PlayerKB_BasicActions.Instance.IsBagOpen = true;
        }
        ShowHints(_grabbedIn);
    }

    private void ShowHints(int handedness)
    {
        _hud.ClearHint(HintConstants.LeftOpenBagHint);
        _hud.ClearHint(HintConstants.RightOpenBagHint);
        _hud.ClearHint(HintConstants.LeftCloseBagHint);
        _hud.ClearHint(HintConstants.RightCloseBagHint);
        _hud.ClearHint(HintConstants.LeftPlaceItemHint);
        _hud.ClearHint(HintConstants.RightPlaceItemHint);
        _hud.ClearHint(HintConstants.LeftTakeItemHint);
        _hud.ClearHint(HintConstants.RightTakeItemHint);

        switch (handedness)
        {
            case 1:
                if (!_isOpen)
                {
                    _hud.CreateHint(HintConstants.LeftOpenBagHint);
                }
                else
                {
                    _hud.CreateHint(HintConstants.LeftCloseBagHint);
                    _hud.CreateHint(HintConstants.LeftPlaceItemHint);
                    _hud.CreateHint(HintConstants.LeftTakeItemHint);
                }
                break;
            case 2:
                if (!_isOpen)
                {
                    _hud.CreateHint(HintConstants.RightOpenBagHint);
                }
                else
                {
                    _hud.CreateHint(HintConstants.RightCloseBagHint);
                    _hud.CreateHint(HintConstants.RightPlaceItemHint);
                    _hud.CreateHint(HintConstants.RightTakeItemHint);
                }
                break;
        }
    }

    private void PlaceInBag(InputAction.CallbackContext context)
    {
        int itemIndex;
        GameObject itemObject;
        if (_grabbedIn == 1)
        {
            itemIndex = PlayerManager.Instance.GetItemRight();
            itemObject = PlayerManager.Instance.GetObjectRight();
        }
        else
        {
            itemIndex = PlayerManager.Instance.GetItemLeft();
            itemObject = PlayerManager.Instance.GetObjectLeft();
        }
        if (itemIndex == -1 || itemObject == null) { return; }

        string itemName = _itemIndexes.GetItemName(itemIndex);
        _inventory.Add(new ItemInList(itemName, itemObject));
        itemObject.GetComponent<PlayerKB_GrabObject>().SetIsActive(false, 3 - _grabbedIn);
    }

    private void ScrollDown(InputAction.CallbackContext context)
    {
        if (_isOpen && _selectedItem > 0)
        {
            _selectedItem--;
        }
    }

    private void ScrollUp(InputAction.CallbackContext context)
    {
        if (_isOpen && _selectedItem < _inventory.Count - 1)
        {
            _selectedItem++;
        }
    }

    private void TakeFromBag(InputAction.CallbackContext context)
    {
        if (_isOpen && _inventory.Count > 0)
        {
            GameObject itemObject = _inventory[_selectedItem].item;
            _inventory.RemoveAt(_selectedItem);
            itemObject.GetComponent<PlayerKB_GrabObject>().SetIsActive(true, 3 - _grabbedIn);
            ClampSelectedIndex();
        }
    }

    private void ClampSelectedIndex()
    {
        if (_inventory.Count == 0)
        {
            _selectedItem = 0;
        }
        else if (_selectedItem >= _inventory.Count)
        {
            _selectedItem = _inventory.Count - 1;
        }
    }
}