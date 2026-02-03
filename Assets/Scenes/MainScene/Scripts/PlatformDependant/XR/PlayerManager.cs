#if UNITY_ANDROID || UNITY_EDITOR

using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Class that manages the player

public class PlayerManager : MonoBehaviour
{

    [SerializeField] private Player_InputTypes.InputType _inputType; // Current input type
    [SerializeField] private InputActionReference _changeInputType; // Keybind to change input type ([1] key)
    [SerializeField] private GameObject _playerKB; // Player GameObject for Keyboard/Mouse
    [SerializeField] private GameObject _playerVR; // Player GameObject for VR

    [SerializeField] private GameObject _leftHandKB; // PlayerKB's left hand
    [SerializeField] private GameObject _rightHandKB; // PlayerKB's right hand
    [SerializeField] private GameObject _leftHandVR; // PlayerVR's left hand
    [SerializeField] private GameObject _rightHandVR; // PlayerVR's right hand
    CropIndexes _itemIndexes; // A script to fetch item indexes

    void Start()
    {
        // Set up change input type action
        _changeInputType.action.Enable();
        _changeInputType.action.performed += ChangeInputType;
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;

        Globals.isDesktopMode = true;

        _itemIndexes = new CropIndexes();
    }

    private void FixedUpdate()
    {
        // Sync positions between the KBM and VR player objects
        if (_inputType == Player_InputTypes.InputType.KBM)
        {
            _playerVR.transform.position = _playerKB.transform.position;
        }
        else
        {
            _playerKB.transform.position = _playerVR.transform.position;
        }
    }

    void OnDestroy()
    {
        // Clean up change input type action
        _changeInputType.action.Disable();
        _changeInputType.action.performed -= ChangeInputType;
    }

    private void ChangeInputType(InputAction.CallbackContext context)
    {
        // Toggle between Keyboard/Mouse and VR input types
        if (_inputType == Player_InputTypes.InputType.KBM)
        {
            _inputType = Player_InputTypes.InputType.VR;
            _playerKB.SetActive(false);
            _playerVR.SetActive(true);
            Globals.isDesktopMode = true;
            Debug.Log("Input type changed to VR");
        }
        else
        {
            _inputType = Player_InputTypes.InputType.KBM;
            _playerVR.SetActive(false);
            _playerKB.SetActive(true);
            Globals.isDesktopMode = false;
            Debug.Log("Input type changed to KBM");
        }
    }
    
    // Check which items are being held in the player's hands, if any
    private GameObject CheckItemInHands(Transform parent)
    {
        List<GameObject> children = new List<GameObject>();
        int numElements = 0;

        // Get all child objects of the hand
        foreach (Transform child in parent)
        {
            children.Add(child.gameObject);
            numElements++;
        }

        // Get the last child object, which will be the held item
        GameObject item = children[numElements - 1];

        // Return the item if it has the "Item" tag
        if (item.CompareTag("Item"))
        {
            return item;
        }
        else
        {
            return null;
        }
    }

    // Specifically get the index of the item held in each hand
    public int GetItemLeft()
    {
        GameObject item = CheckItemInHands(_leftHandKB.transform);
        if (item != null)
        {
            return _itemIndexes.GetItemIndex(item.name);
        }
        return -1;
    }

    public int GetItemRight()
    {
        GameObject item = CheckItemInHands(_rightHandKB.transform);
        if (item != null)
        {
            return _itemIndexes.GetItemIndex(item.name);
        }
        return -1;
    }

    // Specifically get the GameObject of the item held in each hand
    public GameObject GetObjectLeft()
    {
        return CheckItemInHands(_leftHandKB.transform);
    }

    public GameObject GetObjectRight()
    {
        return CheckItemInHands(_rightHandKB.transform);
    }
}

#endif