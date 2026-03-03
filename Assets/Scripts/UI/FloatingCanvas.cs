using UnityEngine;

public class FloatingCanvas : MonoBehaviour
{
    public static FloatingCanvas Instance { get; private set; }
    [SerializeField] private Camera _VRCamera;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void LateUpdate()
    {
        if (_VRCamera == null) { return; }
        transform.LookAt(transform.position + _VRCamera.transform.rotation * Vector3.forward, _VRCamera.transform.rotation * Vector3.up);
    }

    public void WarpToObject (Transform location)
    {
        transform.position = location.position;
    }

    public void WarpToObject (Vector3 location)
    {
        transform.position = location;
    }

    public void WarpToOrigin()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }
}
