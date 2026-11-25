using UnityEngine;

public class EnterCanvasView : MonoBehaviour
{
    private Player _player;
    private bool _isViewEntered = false;
    private Camera _playerCamera;
    private Camera canvasCamera;

    private void Awake()
    {
        _player = FindFirstObjectByType<Player>();
        _playerCamera = Camera.main;
        canvasCamera = GetComponent<Camera>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) { AssignInputs(); } 
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) { UnassignInputs(); }
    }

    private void ToggleView()
    {
        _isViewEntered = !_isViewEntered;

        Globals.handleMovement = !_isViewEntered;
        Globals.handleKBLook = !_isViewEntered;

        _playerCamera.enabled = !_isViewEntered;
        canvasCamera.enabled = _isViewEntered;

        Cursor.lockState = _isViewEntered ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = _isViewEntered;
    }

    private void OnDisable()
    {
        UnassignInputs();
    }

    private void AssignInputs()
    {
        Debug.Log("Entered range.");
        _player.Controls.Viva.UniversalInteract.performed += context => ToggleView();
    }

    private void UnassignInputs()
    {
        _player.Controls.Viva.UniversalInteract.performed -= context => ToggleView();
    }
}
