using UnityEngine;
using Viva.Interaction;

public class DesktopPointerSource : MonoBehaviour, IPointerSource
{
    [SerializeField] private Camera mainCamera;

    public bool IsActive => enabled && gameObject.activeInHierarchy && (mainCamera != null || Camera.main != null);

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    public Ray GetRay()
    {
        Camera cam = mainCamera != null ? mainCamera : Camera.main;

        if (cam == null)
        {
            return new Ray(transform.position, transform.forward);
        }

        return cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
    }
}
