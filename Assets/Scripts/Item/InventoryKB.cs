using NUnit.Framework;
using Steamworks.Ugc;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryKB : MonoBehaviour
{
    public static InventoryKB Instance;

    private Player _player;
    [SerializeField] private HintManager _hud;
    private PlayerKB_GrabObject _grabScript;
    private Animator _animator;

    private int _grabbedIn;
    public bool IsOpen;

    private List<ItemInList> _inventory = new List<ItemInList>();
    private int _numElements;
    [SerializeField] private int _maxInventorySize = 10;

    [Serializable]
    private class ItemInList
    {
        private string name;
        private GameObject prefab;

        public ItemInList(string name, GameObject prefab)
        {
            this.name = name;
            this.prefab = prefab;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _grabScript = GetComponent<PlayerKB_GrabObject>();
    }

    private void Start()
    {
        _player = FindFirstObjectByType<Player>();
        _animator = GetComponent<Animator>();
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
        }

        if (handedness == 2)
        {
            _player.Controls.Viva.InteractLeft.performed -= ToggleBag;
            _player.Controls.Viva.InteractRight.performed += ToggleBag;
            _player.Controls.Viva.LeftGrab.performed -= PlaceInBag;
            _player.Controls.Viva.RightGrab.performed += PlaceInBag;
        }
    }

    private void ToggleBag(InputAction.CallbackContext context)
    {
        if (IsOpen)
        {
            _animator.Play("Close");
            IsOpen = false;
        }
        else
        {
            _animator.Play("Open");
            IsOpen = true;
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

        switch (handedness)
        {
            case 1:
                if (!IsOpen)
                { 
                    _hud.CreateHint(HintConstants.LeftOpenBagHint); 
                    _hud.CreateHint(HintConstants.LeftPlaceItemHint);
                }
                else
                { 
                    _hud.CreateHint(HintConstants.LeftCloseBagHint); 
                }
                break;
            case 2:
                if (!IsOpen)
                { 
                    _hud.CreateHint(HintConstants.RightOpenBagHint); 
                    _hud.CreateHint(HintConstants.RightPlaceItemHint);
                }
                else
                { 
                    _hud.CreateHint(HintConstants.RightCloseBagHint); 
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

        string itemName = CropIndexes.Instance.GetItemName(itemIndex);
        _inventory.Add(new ItemInList(itemName, itemObject));
        itemObject.GetComponent<PlayerKB_GrabObject>().SetIsActive(false, 3 - _grabbedIn);
        _numElements++;
    }
}
