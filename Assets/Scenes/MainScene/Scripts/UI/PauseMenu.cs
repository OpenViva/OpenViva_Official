using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Animator bookAnimator;
    [SerializeField] private GameObject referencePlayer;
    [SerializeField] private Vector3 offsetLocation = new Vector3(1f, 0, 0);
    [SerializeField] private Quaternion offsetRotation = Quaternion.Euler(0, 90, 0);

    private void Start()
    {
        if (TryGetComponent(out Animator foundAnimator))
        {
            bookAnimator = foundAnimator;
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        // Check phase: Started (pressed), Performed (held if needed), Canceled (released)
        if (context.performed)
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        if (!Globals.isMenuOpen)
        {
            OnBeginPauseInput();
        }
        else
        {
            OnExitPauseInput();
        }
    }

    void OnBeginPauseInput()
    {
        PlayBookAnimation("OpenBook");
        OrientPauseMenuToPlayer();
        Globals.isMenuOpen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnExitPauseInput()
    {
        PlayBookAnimation("CloseBook");
        Globals.isMenuOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OrientPauseMenuToPlayer()
    {
        // Apply position offset in the player's local space
        Vector3 worldOffset = referencePlayer.transform.rotation * offsetLocation;
        Vector3 targetPosition = referencePlayer.transform.position + worldOffset;

        // Apply rotation: player's rotation + additional rotation offset
        Quaternion targetRotation = referencePlayer.transform.rotation * offsetRotation;

        transform.SetPositionAndRotation(targetPosition, targetRotation);
    }

    private void PlayBookAnimation(string name)
    {
        bookAnimator.CrossFade(name, 0.0f);
    }
}
