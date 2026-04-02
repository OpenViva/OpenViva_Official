using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryXR : MonoBehaviour

    // Manages the inventory system linked to the bag.
{
    private PlayerKB_GrabObject _bagScript; // This script is used to check which hand the bag is grabbed in
    private int _grabbedIn = 0; // 0 - Neither, 1 - Left, 2 - Right
    [SerializeField] private PlayerManager _playerManager; // This script is used to check what item the player is holding
    private int _itemInOtherHand = -1; // The index of the item in the hand that isn't holding the bag
    private Animator _animator; // Animator used to open or close the bag
    private bool _isOpen; // Check if the bag is open
    [SerializeField] private List<string> _inventory = new List<string>(); // The names of every item in the bag
    private int _numElements = 0; // The number of items in the bag
    [SerializeField] private List<GameObject> _objects = new List<GameObject>(); // The game objects of every item in the bag
    private CropIndexes _itemIndexes; // Script that keeps track of the index for each item
    private int _selectedItem = 1; // The item that the player has selected to take out of the bag
    [SerializeField] PlayerKB_BasicActions _basicActions; // Prevent the player from extending/retracting hands when the bag is open

    [SerializeField] private HintManager _hud; // HUD script to show hints
    private bool _start = true; // Certain hints must only be shown once when the bag is grabbed

    // --- Fields ---
    private Player _player;

    // Initialize variables and set up input actions
    private void Start()
    {
        _bagScript = GetComponent<PlayerKB_GrabObject>();
        _animator = GetComponent<Animator>();

        _player = FindFirstObjectByType<Player>();

        _itemIndexes = new CropIndexes();

        AssignInputs();
    }

    private void Update()
    {
        // Check which hand the bag is grabbed in
        _grabbedIn = _bagScript.GetIsGrabbed();
        // Tell the grab object script whether the bag is open or closed
        _bagScript.SetIsOpen(_isOpen);

        // Get the item in the other hand
        if (_grabbedIn != 0)
        {
            if (_grabbedIn == 1)
            {
                _itemInOtherHand = _playerManager.GetItemRight();
            }
            else if (_grabbedIn == 2)
            {
                _itemInOtherHand = _playerManager.GetItemLeft();
            }

            // Show hints when the bag is grabbed
            if (_start)
            {
                _hud.CreateHint(HintConstants.OpenBagHint);
                _hud.ClearHint(HintConstants.SelectItemsHint);
                _hud.ClearHint(HintConstants.TakeItemHint);
                _start = false;
            }
        }
        else
        {
            _hud.ClearHint(HintConstants.OpenBagHint); 
        }

        // Reset start variable when bag is dropped
        if (_grabbedIn == 0)
        {
            _start = true;
        }
    }

    // Open or close the bag and show/hide relevant hints
    private void ToggleBag(InputAction.CallbackContext context)
    {
        if (_grabbedIn > 0)
        {
            if (_isOpen)
            {
                _animator.Play("Close");
                _hud.ClearHint(HintConstants.CloseBagHint);
                _hud.CreateHint(HintConstants.OpenBagHint);
                _hud.ClearHint(HintConstants.SelectItemsHint);
                _hud.ClearHint(HintConstants.TakeItemHint);
                if (_grabbedIn == 1)
                {
                    _hud.CreateHint(HintConstants.LeftReleaseHint);
                }
                else if (_grabbedIn == 2)
                {
                    _hud.CreateHint(HintConstants.RightReleaseHint);
                }
                _hud.ClearHint(HintConstants.LeftPlaceItemHint);
                _hud.ClearHint(HintConstants.RightPlaceItemHint);
            }
            else
            {
                _animator.Play("Open");
                _hud.ClearHint(HintConstants.LeftReleaseHint);
                _hud.ClearHint(HintConstants.RightReleaseHint);
                _hud.ClearHint(HintConstants.OpenBagHint);
                _hud.CreateHint(HintConstants.CloseBagHint);
                _hud.CreateHint(HintConstants.SelectItemsHint);
                _hud.CreateHint(HintConstants.TakeItemHint);
                if (_grabbedIn == 1)
                {
                    _hud.CreateHint(HintConstants.LeftPlaceItemHint);
                }
                else if (_grabbedIn == 2)
                {
                    _hud.CreateHint(HintConstants.RightPlaceItemHint);
                }
            }
            _isOpen = ! _isOpen;
            _basicActions.SetBagOpen(_isOpen);
        }
    }

    // Place the item in the right hand into the bag
    private void PlaceInBagWithRightHand(InputAction.CallbackContext context)
    {
        // If the bag is open, the bag is in the left hand, and there is space in the bag
        if (_isOpen && _grabbedIn == 1 && _numElements < 10)
        {
            // Get the name of the item in the other hand
            string itemName = _itemIndexes.GetItemName(_itemInOtherHand);
            if (itemName != "Error")
            {
                // Add the item to the inventory
                _inventory.Add(itemName);
                // Get the game object of the item in the other hand
                GameObject item = _playerManager.GetObjectRight();
                if (item != null)
                {
                    // Add the item to the list of objects in the bag
                    _objects.Add(item);
                    // Disable the item
                    PlayerKB_GrabObject itemScript = item.GetComponent<PlayerKB_GrabObject>();
                    itemScript.SetIsActive(false, 0);
                    _numElements++;
                }
                
            }  
        }
    }

    // Ditto.
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

    // Select item in the bag
    private void ScrollUp(InputAction.CallbackContext context)
    {
        if (!_isOpen || _numElements == 0) return;

        if (_selectedItem < _numElements)
        {
            _selectedItem++;
            // Debug.Log("Selected Item: " + _inventory[_selectedItem - 1]);
        }
    }

    private void ScrollDown(InputAction.CallbackContext context)
    {
        if (!(_isOpen && _numElements > 0)) return;

        if (_selectedItem > 1)
        {
            _selectedItem--;
            // Debug.Log("Selected Item: " + _inventory[_selectedItem - 1]);
        }
    }

    // Take the selected item out of the bag and place it in the free hand
    private void TakeSelectedItem(InputAction.CallbackContext context)
    {
        // If the bag is open and there is at least one item in the bag
        if (_isOpen && _numElements > 0)
        {
            // Get the item's game object and remove it from the lists
            GameObject item = _objects[_selectedItem - 1];
            _objects.Remove(item);
            _inventory.RemoveAt(_selectedItem - 1);
            PlayerKB_GrabObject itemScript = item.GetComponent<PlayerKB_GrabObject>();
            // Activate the item in the free hand
            if (_grabbedIn == 1)
            {
                itemScript.SetIsActive(true, 2);
            }
            else if (_grabbedIn == 2)
            {
                itemScript.SetIsActive(true, 1);
            }
            _numElements--;

            // Correct selected item index if necessary
            if (_numElements == 0)
            {
                _selectedItem = 1;
            }
            else if (_selectedItem >= _numElements)
            {
                _selectedItem = _numElements - 1;
            }
        }
    }

    private void AssignInputs()
    {
        _player.Controls.Viva.ToggleBag.performed += ToggleBag;
        _player.Controls.Viva.LeftGrab.performed += PlaceInBagWithRightHand;
        _player.Controls.Viva.RightGrab.performed += PlaceInBagWithLeftHand;
        _player.Controls.Viva.ScrollUp.performed += ScrollUp;
        _player.Controls.Viva.ScrollDown.performed += ScrollDown;
        _player.Controls.Viva.Interact.performed += TakeSelectedItem;
    }
}
