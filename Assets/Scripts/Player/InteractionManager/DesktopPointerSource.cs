using UnityEngine;
using Viva.Interaction;

public class DesktopPointerSource : MonoBehaviour, IPointerSource
{
    [SerializeField] private Camera mainCamera;

    public bool IsActive => enabled && gameObject.activeInHierarchy && mainCamera != null;

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    public Ray GetRay()
    {
        if (mainCamera == null) return new Ray(transform.position, transform.forward);

        return mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
    }
}
