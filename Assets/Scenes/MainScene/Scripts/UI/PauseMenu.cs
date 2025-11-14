using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("Offsets")]
    [SerializeField] private Vector3 offsetLocation = new(1f, 0, 0);
    [SerializeField] private Quaternion offsetRotation = Quaternion.Euler(0, 180, 0);

    [Header("Pages Settings")]
    [SerializeField] private GameObject rootPage;
    [SerializeField] private GameObject bookMesh;
    [SerializeField] private GameObject LeftPage;
    [SerializeField] private GameObject RightPage;

    [Header("General References")]
    [SerializeField] private GameObject referencePlayer;

    [Header("Animation Settings")]
    [SerializeField] private Animator bookAnimator;
    [Tooltip("Name of the animation state that opens the book")]
    [SerializeField] private string openAnimationStateName = "OpenBook";
    [Tooltip("Name of the animation state that closes the book")]
    [SerializeField] private string closeAnimationStateName = "CloseBook";

    [Header("Animation Clip Timing")]
    [Tooltip("Extra seconds added to the clip length – useful if you have exit-time transitions")]
    [SerializeField] private float extraWaitSeconds = 0.1f;
    [SerializeField] private float _openClipLength = 0.6f;
    [SerializeField] private float _closeClipLength = 1.2f;

    // --- Private Fields ---
    [SerializeField] private List<GameObject> leftPages = new();
    [SerializeField] private List<GameObject> rightPages = new();

    private void Start()
    {
        if (TryGetComponent(out Animator foundAnimator))
        {
            bookAnimator = foundAnimator;
        }

        rootPage.SetActive(false);
        bookMesh.SetActive(false);
        LeftPage.SetActive(false);
        RightPage.SetActive(false);

        leftPages = GetDirectChildren(LeftPage);
        rightPages = GetDirectChildren(RightPage);
    }

    #region Input Methods
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
        StartOpenSequence();

        OrientPauseMenuToPlayer();
        Globals.isMenuOpen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnExitPauseInput()
    {
        StartCloseSequence();

        Globals.isMenuOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    #endregion

    #region Animation Coroutines
    public void StartOpenSequence()
    {
        StopAllCoroutines();
        StartCoroutine(OpenSequenceRoutine());
    }

    public void StartCloseSequence()
    {
        StopAllCoroutines();
        StartCoroutine(CloseSequenceRoutine());
    }

    private IEnumerator OpenSequenceRoutine()
    {
        // 1. Activate the book mesh
        if (bookMesh != null) bookMesh.SetActive(true);
        else Debug.LogWarning("bookMesh reference is null!", this);

        // 2. Play the open animation
        PlayBookAnimation(openAnimationStateName);

        // 3. Wait for it
        yield return new WaitForSeconds(_openClipLength + extraWaitSeconds);

        // 4. Enable the root page
        if (rootPage != null && LeftPage != null && RightPage != null)
        {
            LeftPage.SetActive(true);
            RightPage.SetActive(true);

            rootPage.SetActive(true);
        }
        else Debug.LogWarning("rootPage reference is null!", this);
    }

    private IEnumerator CloseSequenceRoutine()
    {
        // 1. Hide pages first
        if (rootPage != null && LeftPage != null && RightPage != null)
        {
            leftPages.DeactivateAllGameobjects();
            rightPages.DeactivateAllGameobjects();

            LeftPage.SetActive(false);
            RightPage.SetActive(false);
        }
        else Debug.LogWarning("rootPage reference is null!", this);

        // 2. Play close animation
        PlayBookAnimation(closeAnimationStateName);

        // 3. Wait for it
        yield return new WaitForSeconds(_closeClipLength + extraWaitSeconds);

        // 4. Hide mesh (optional – comment out if you want to keep it visible)
        if (bookMesh != null) bookMesh.SetActive(false);
    }
    #endregion

    #region Helper Methods
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
        if (bookAnimator != null && !string.IsNullOrEmpty(name))
        {
            bookAnimator.CrossFade(name, 0f);
        }
        else
        {
            Debug.LogError("Cannot play animation: Animator missing or state name empty.", this);
        }
    }

    List<GameObject> GetDirectChildren(GameObject parent)
    {
        List<GameObject> children = new List<GameObject>();

        if (parent == null) return children;

        for (int i = 0; i < parent.transform.childCount; i++)
        {
            Transform childTransform = parent.transform.GetChild(i);
            children.Add(childTransform.gameObject);
        }

        return children;
    }
    #endregion

    public void OnQuitGame()
    {
        Application.Quit();
    }
}
