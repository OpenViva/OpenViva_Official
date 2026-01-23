using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    private PlayerKB_GrabObject _bagScript; // Script to check which hand the bag is held in
    private int _grabbedIn = 0; // 0 - Neither, 1 - Left, 2 - Right
    [SerializeField] private PlayerManager _playerManager; // The player manager script for checking what item the player is holding
    private int _itemInOtherHand = -1;
    private Animator _animator; // Animator used to open or close the bag
    private DesktopInput _keybinds;
    private bool _isOpen; // Check if the bag is open
    [SerializeField] private List<string> _inventory = new List<string>();
    private int _numElements = 0;
    [SerializeField] private List<GameObject> _objects = new List<GameObject>();
    private ItemIndexes _itemIndexes;
    private int _selectedItem = 1;
    [SerializeField] PlayerKB_BasicActions _basicActions;

    [SerializeField] private PlayerKB_HUD _hud;
    private bool _start = true;

    private void Start()
    {
        _bagScript = GetComponent<PlayerKB_GrabObject>();
        _animator = GetComponent<Animator>();

        _keybinds = new DesktopInput();
        _keybinds.Viva.Enable();
        _keybinds.Viva.ToggleBag.performed += ToggleBag;
        _keybinds.Viva.LeftGrab.performed += PlaceInBagWithRightHand;
        _keybinds.Viva.RightGrab.performed += PlaceInBagWithLeftHand;
        _keybinds.Viva.ScrollUp.performed += ScrollUp;
        _keybinds.Viva.ScrollDown.performed += ScrollDown;
        _keybinds.Viva.Interact.performed += TakeSelectedItem;

        _itemIndexes = new ItemIndexes();
    }

    private void Update()
    {
        _grabbedIn = _bagScript.GetIsGrabbed();
        _bagScript.SetIsOpen(_isOpen);
        if (_grabbedIn != 0)
        {
            if (_grabbedIn == 1)
            {
                _itemInOtherHand = _playerManager.GetItemRight();
            }
            else if ( _grabbedIn == 2)
            {
                _itemInOtherHand = _playerManager.GetItemLeft();
            }

            if (_start)
            {
                _hud.CreateHint("[Q]: Open Bag");
                _hud.ClearHint("[scrollwheel]: Select Items");
                _hud.ClearHint("[E]: Take Item");
                _start = false;
            }
        }

        if (_grabbedIn == 0)
        {
            _start = true;
        }
    }

    private void ToggleBag(InputAction.CallbackContext context)
    {
        if (_grabbedIn > 0)
        {
            if (_isOpen)
            {
                _animator.Play("Close");
                _hud.ClearHint("[Q]: Close Bag");
                _hud.CreateHint("[Q]: Open Bag");
                _hud.CreateHint("[Q]: Open Bag");
                _hud.ClearHint("[scrollwheel]: Select Items");
                _hud.ClearHint("[E]: Take Item");
                if (_grabbedIn == 1)
                {
                    _hud.CreateHint("[LMB]: Drop");
                }
                else if (_grabbedIn == 2)
                {
                    _hud.CreateHint("[RMB]: Drop");
                }
                _hud.ClearHint("[LMB]: Place Item");
                _hud.ClearHint("[RMB]: Place Item");
            }
            else
            {
                _animator.Play("Open");
                _hud.ClearHint("[LMB]: Drop");
                _hud.ClearHint("[RMB]: Drop");
                _hud.ClearHint("[Q]: Open Bag");
                _hud.CreateHint("[Q]: Close Bag");
                _hud.CreateHint("[scrollwheel]: Select Items");
                _hud.CreateHint("[E]: Take Item");
                if (_grabbedIn == 1)
                {
                    _hud.CreateHint("[LMB]: Place Item");
                }
                else if (_grabbedIn == 2)
                {
                    _hud.CreateHint("[RMB]: Place Item");
                }
            }
            _isOpen = ! _isOpen;
            _basicActions.SetBagOpen(_isOpen);
        }
    }

    private void PlaceInBagWithRightHand(InputAction.CallbackContext context)
    {
        if (_isOpen && _grabbedIn == 1 && _numElements < 10)
        {
            string itemName = _itemIndexes.GetItemName(_itemInOtherHand);
            if (itemName != "Error")
            {
                _inventory.Add(itemName);
                GameObject item = _playerManager.GetObjectRight();
                if (item != null)
                {
                    _objects.Add(item);
                    PlayerKB_GrabObject itemScript = item.GetComponent<PlayerKB_GrabObject>();
                    itemScript.SetIsActive(false, 0);
                    _numElements++;
                }
                
            }  
        }
    }

    private void PlaceInBagWithLeftHand(InputAction.CallbackContext context)
    {
        if (_isOpen && _grabbedIn == 2 && _numElements < 10)
        {
            string itemName = _itemIndexes.GetItemName(_itemInOtherHand);
            if (itemName != "Error")
            {
                _inventory.Add(itemName);
                GameObject item = _playerManager.GetObjectLeft();
                if (item != null)
                {
                    _objects.Add(item);
                    PlayerKB_GrabObject itemScript = item.GetComponent<PlayerKB_GrabObject>();
                    itemScript.SetIsActive(false, 0);
                    _numElements++;
                }
            }
        }
    }

    private void ScrollUp(InputAction.CallbackContext context)
    {
        if (!_isOpen || _numElements == 0) return;

        if (_selectedItem < _numElements)
        {
            _selectedItem++;
            Debug.Log("Selected Item: " + _inventory[_selectedItem - 1]);
        }
    }

    private void ScrollDown(InputAction.CallbackContext context)
    {
        if (!(_isOpen && _numElements > 0)) return;

        if (_selectedItem > 1)
        {
            _selectedItem--;
            Debug.Log("Selected Item: " + _inventory[_selectedItem - 1]);
        }
    }   

    private void TakeSelectedItem(InputAction.CallbackContext context)
    {
        if (_isOpen && _numElements > 0)
        {
            GameObject item = _objects[_selectedItem - 1];
            _objects.Remove(item);
            _inventory.RemoveAt(_selectedItem - 1);
            PlayerKB_GrabObject itemScript = item.GetComponent<PlayerKB_GrabObject>();
            if (_grabbedIn == 1)
            {
                itemScript.SetIsActive(true, 2);
            }
            else if (_grabbedIn == 2)
            {
                itemScript.SetIsActive(true, 1);
            }
            _numElements--;

            if (_numElements == 0)
            {
                _selectedItem = 1;
            }
            else if (_selectedItem >= _numElements)
            {
                _selectedItem = _numElements - 1;
            }

            if (_numElements != 0)
            {
                Debug.Log("Selected Item: " + _inventory[_selectedItem]);
            }
        }
    }

    private void OnDestroy()
    {
        _keybinds.Viva.ToggleBag.performed -= ToggleBag;
        _keybinds.Viva.LeftGrab.performed -= PlaceInBagWithRightHand;
        _keybinds.Viva.RightGrab.performed -= PlaceInBagWithLeftHand;
        _keybinds.Viva.ScrollUp.performed -= ScrollUp;
        _keybinds.Viva.ScrollDown.performed -= ScrollDown;
        _keybinds.Viva.Interact.performed -= TakeSelectedItem;
        _keybinds.Viva.Disable();
    }
}
