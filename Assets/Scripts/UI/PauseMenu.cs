using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("Offsets")]
    [SerializeField] private Vector3 positionOffset = new(0, -1f, 0f);
    [SerializeField] private Vector3 rotationOffset = new(45f, 180f, 0f);
    [SerializeField] private GameObject menuHandleKB;
    [SerializeField] private GameObject menuHandleVR;

    [Header("Pages Settings")]
    [SerializeField] private GameObject rootPage;
    [SerializeField] private GameObject bookMesh;
    [SerializeField] private GameObject LeftPage;
    [SerializeField] private GameObject RightPage;

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

    // --- Fields ---
    [SerializeField] private List<GameObject> leftPages = new();
    [SerializeField] private List<GameObject> rightPages = new();

    private Player _player;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private void Start()
    {
        _player = GetComponentInParent<Player>();

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

        AssignInputs();
    }

    #region Input Methods
    private void AssignInputs()
    {
        _player.Controls.Viva.Pause.performed += ctx => TogglePauseMenu();
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
        // 0. Disable all pages first to prevent showing multiple
        if (rootPage != null && LeftPage != null && RightPage != null)
        {
            StartCoroutine(leftPages.DeactivateListAsync(batchSize: 3));
            StartCoroutine(rightPages.DeactivateListAsync(batchSize: 3));

            LeftPage.SetActive(false);
            RightPage.SetActive(false);
        }

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
            StartCoroutine(leftPages.DeactivateListAsync(batchSize: 3));
            StartCoroutine(rightPages.DeactivateListAsync(batchSize: 3));

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
        if (menuHandleKB.activeSelf)
        {
            menuHandleKB.transform.GetPositionAndRotation(out targetPosition, out targetRotation);
        }
        else
        {
            menuHandleVR.transform.GetPositionAndRotation(out targetPosition, out targetRotation);
        }

        // Apply rotation offset FIRST (order matters!)
        targetRotation *= Quaternion.Euler(rotationOffset);

        // Apply position offset in the rotated space (so "forward" respects the new rotation)
        targetPosition += targetRotation * positionOffset;

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
