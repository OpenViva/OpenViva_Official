using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryVR : MonoBehaviour
{
    [SerializeField] private InputActionReference _XRIButtonDownY;
    [SerializeField] private InputActionReference _XRIButtonDownB;
    [SerializeField] private InputActionReference _rightThumbstick;

    [SerializeField] private HintManager _controlHintsL;
    [SerializeField] private HintManager _controlHintsR;

    private Animator _animator;
    private bool _isOpen = false;
    private PlayerVR_GrabObject _grabScript;
    private ItemIndexes _itemIndexes;

    private List<ItemInList> _inventory = new();
    private int _selectedItem = 0;
    [SerializeField] private int _maxInventorySize = 10;

    private GameObject _itemBeingRemoved;
    private bool _canScroll;

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
    private void Awake()
    {
        _grabScript = GetComponentInParent<PlayerVR_GrabObject>();
    }

    private void Start()
    {
        _animator = GetComponentInParent<Animator>();
        _itemIndexes = new ItemIndexes();
        _rightThumbstick.action.performed += Scroll;
    }

    private void OnEnable()
    {
        _XRIButtonDownB.action.Enable();
        _XRIButtonDownY.action.Enable();
        _rightThumbstick.action.Enable();
        _grabScript.OnGrabbedStateChanged += ShowHints;
        _grabScript.OnGrabbedStateChanged += SwitchInputs;
    }

    private void OnDisable()
    {
        _XRIButtonDownB.action.Disable();
        _XRIButtonDownY.action.Disable();
        _rightThumbstick.action.Disable();
        _grabScript.OnGrabbedStateChanged -= ShowHints;
        _grabScript.OnGrabbedStateChanged -= SwitchInputs;
    }

    private void SwitchInputs(int handedness)
    {
        if (handedness == 1)
        {
            _XRIButtonDownB.action.performed -= ToggleBag;
            _XRIButtonDownY.action.performed += ToggleBag;
        }
        else if (handedness == 2)
        {
            _XRIButtonDownY.action.performed -= ToggleBag;
            _XRIButtonDownB.action.performed += ToggleBag;
        }
    }

    private void ToggleBag(InputAction.CallbackContext context)
    {
        if (_isOpen)
        {
            _animator.Play("Close");
            _isOpen = false;
        }
        else
        {
            _animator.Play("Open");
            _isOpen = true;
        }
        ShowHints(_grabScript.IsGrabbed);
    }

    private void ShowHints(int handedness)
    {
        _controlHintsL.ClearHint(HintConstants.LeftOpenBagHintVR);
        _controlHintsR.ClearHint(HintConstants.RightOpenBagHintVR);
        _controlHintsL.ClearHint(HintConstants.LeftCloseBagHintVR);
        _controlHintsR.ClearHint(HintConstants.RightCloseBagHintVR);

        switch (handedness)
        {
            case 1:
                if (!_isOpen)
                { _controlHintsL.CreateHint(HintConstants.LeftOpenBagHintVR); }
                else
                { _controlHintsL.CreateHint(HintConstants.LeftCloseBagHintVR); }
                break;
            case 2:
                if (!_isOpen)
                { _controlHintsR.CreateHint(HintConstants.RightOpenBagHintVR); }
                else
                { _controlHintsR.CreateHint(HintConstants.RightCloseBagHintVR); }
                break;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!_isOpen) { return; }

        GameObject other = collider.gameObject;

        /*
         * 'out var script':
         * Instantiate a new variable called 'script' of type <Component> using the found component.
         * This variable can now be used within the code block.
         * 'script' is null if component not found.
         */
        if (other.TryGetComponent<PlayerVR_GrabObject>(out var script))
        {
            if (script.IsGrabbed == 0 && !other.GetComponent<Rigidbody>().isKinematic && other != _itemBeingRemoved)
            {
                PlaceInBag(other);
            }
        }

        if ((other.name == "Controller_BaseLeft" || other.name == "Controller_BaseRight")
            && ((_grabScript.IsGrabbed == 1 && PlayerManager.Instance.GetObjectRight() == null)
            || (_grabScript.IsGrabbed == 2 && PlayerManager.Instance.GetObjectLeft() == null)))
        {
                PrepareToRemove();
        }
    }

    private void PlaceInBag(GameObject item)
    {
        if (_inventory.Count >= _maxInventorySize) { return; }
        string name = _itemIndexes.GetItemName(item.GetComponent<PlayerVR_GrabObject>().ObjectIndex);
        _inventory.Add(new ItemInList(name, item));
        item.GetComponent<Rigidbody>().isKinematic = true;
        item.SetActive(false);
    }

    private void Scroll(InputAction.CallbackContext context)
    {
        if (!_canScroll) { return; }

        Vector2 input = context.ReadValue<Vector2>();
        if (input.y > 0.8f && _selectedItem < _inventory.Count - 1)
        {
            _selectedItem++;
        }
        else if (input.y < -0.8f && _selectedItem > 0)
        {
            _selectedItem--;
        }

        StartCoroutine(ScrollCooldown());
    }

    private void PrepareToRemove()
    {
        if (_inventory.Count == 0) { return; }
        _itemBeingRemoved = _inventory[_selectedItem].item;
        _itemBeingRemoved.transform.SetParent(transform);
        _itemBeingRemoved.transform.position = transform.position;
        _itemBeingRemoved.SetActive(true);
    }

    private void OnTriggerExit(Collider collider)
    {
        if (!_isOpen) { return; }

        GameObject other = collider.gameObject;
        if (other == _itemBeingRemoved)
        {
            _itemBeingRemoved.transform.SetParent(null);
            RemoveItem(_itemBeingRemoved);
        }

        if (other.name == "Controller_BaseLeft" || other.name == "Controller_BaseRight")
        {
            CancelRemove();
        }
    }

    private void RemoveItem(GameObject item)
    {
        for (int i = 0; i < _inventory.Count; i++)
        {
            if (_inventory[i].item == item)
            {
                _inventory.RemoveAt(i);
                _itemBeingRemoved = null;
                break;
            }
        }
        ClampSelectedIndex();
    }

    private void CancelRemove()
    {
        if (_itemBeingRemoved == null) { return; }
        if (_itemBeingRemoved.GetComponent<PlayerVR_GrabObject>().IsGrabbed != 0) { return; }

        for (int i = 0; i < _inventory.Count; i++)
        {
            if (_inventory[i].item == _itemBeingRemoved)
            {
                _itemBeingRemoved.SetActive(false);
                _itemBeingRemoved.transform.SetParent(null);
                _itemBeingRemoved = null;
                break;
            }
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

    private IEnumerator ScrollCooldown()
    {
        _canScroll = false;
        yield return new WaitForSeconds(0.25f);
        _canScroll = true;
    }
}
