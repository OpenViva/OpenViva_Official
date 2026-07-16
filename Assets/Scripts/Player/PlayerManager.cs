using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]

// Class that manages the player

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private InputActionReference _changeInputType; // Keybind to change input type ([1] key)
    [SerializeField] private GameObject _playerKB; // Player GameObject for Keyboard/Mouse
    [SerializeField] private GameObject _playerVR; // Player GameObject for VR

    [SerializeField] private SkinnedMeshRenderer _handSkin;
    [SerializeField] private GameObject _leftHandKB; // PlayerKB's left hand
    [SerializeField] private GameObject _rightHandKB; // PlayerKB's right hand
    private List<PlayerVR_GrabObject> _itemList;

    public GameObject AnimationKB;
    public GameObject AnimationVR;

    // Static Events
    public static event Action OnMoveCharaToCamera;

    private ItemIndexes _itemIndexes;
    private Player _player;

    public bool LeftHandOccupied = false;
    public bool RightHandOccupied = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        Globals.isDesktopMode = false;
        ChangeInputType(); // TODO: Needs fixing, doesn't change to desktop by default.

        // Set up change input type action
        _changeInputType.action.Enable();
        _changeInputType.action.performed += context => ChangeInputType();
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;

        _itemIndexes = new ItemIndexes();
        _itemList = new List<PlayerVR_GrabObject>(FindObjectsByType<PlayerVR_GrabObject>(FindObjectsSortMode.None));

        _player = GetComponent<Player>();

        InitializeInputEvents();
    }

    private void FixedUpdate()
    {
        // Sync positions between the KBM and VR player objects
        if (Globals.isDesktopMode)
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
        _changeInputType.action.performed -= context => ChangeInputType();
    }

    private void ChangeInputType()
    {
        // ToggleLanternLeft between Keyboard/Mouse and VR input types
        if (Globals.isDesktopMode)
        {
            Globals.isDesktopMode = false;
            _playerKB.SetActive(false);
            _playerVR.SetActive(true);
            Debug.Log("Input type changed to VR");
        }
        else
        {
            Globals.isDesktopMode = true;
            _playerVR.SetActive(false);
            _playerKB.SetActive(true);
            Debug.Log("Input type changed to KBM");
        }
    }
    
    // Check which items are being held in the player's hands, if any
    private GameObject CheckItemInHandsKB(Transform parent)
    {
        List<GameObject> children = new();
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
        GameObject item;
        if (Globals.isDesktopMode)
        {
            item = CheckItemInHandsKB(_leftHandKB.transform);
        }
        else
        {
            item = CheckItemInHandsVR(1);
        }

        if (item != null)
        {
            return _itemIndexes.GetItemIndex(item.name);
        }
        return -1;
    }

    public int GetItemRight()
    {
        GameObject item;
        if (Globals.isDesktopMode)
        {
            item = CheckItemInHandsKB(_rightHandKB.transform);
        }
        else
        {
            item = CheckItemInHandsVR(2);
        }
        if (item != null)
        {
            return _itemIndexes.GetItemIndex(item.name);
        }
        return -1;
    }

    // Specifically get the GameObject of the item held in each hand
    public GameObject GetObjectLeft()
    {
        if (Globals.isDesktopMode)
        {
            return CheckItemInHandsKB(_leftHandKB.transform);
        }
        else
        {
            return CheckItemInHandsVR(1);
        }
    }

    public GameObject GetObjectRight()
    {
        if (Globals.isDesktopMode)
        {
            return CheckItemInHandsKB(_rightHandKB.transform);
        }
        else
        {
            return CheckItemInHandsVR(2);
        }
    }

    private GameObject CheckItemInHandsVR(int handedness)
    {
        for (int i = 0; i < _itemList.Count; i++)
        {
            if (_itemList[i].IsGrabbed == handedness)
            {
                return _itemList[i].gameObject;
            }
        }
        return null;
    }

    void RaiseMoveCharaToCamera()
    {
        OnMoveCharaToCamera?.Invoke();
    }

    void InitializeInputEvents()
    {
        _player.Controls.Viva.UniversalInteract.performed += context => RaiseMoveCharaToCamera();
    }

    public void HideHands(bool input)
    {
        _handSkin.enabled = !input;
    }
}
