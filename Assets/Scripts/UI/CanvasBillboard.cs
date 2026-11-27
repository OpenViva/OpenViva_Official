using UnityEngine;

public class CanvasBillboard : MonoBehaviour
{
    [SerializeField] private bool _lockVerticalRotation = false;
    private Transform _camera;

    private void Start()
    {
        if (Camera.main != null) _camera = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (_camera == null) return;

        Vector3 direction = transform.position - _camera.position;
        direction.y = _lockVerticalRotation ? 0 : direction.y;
        if (direction != Vector3.zero) transform.rotation = Quaternion.LookRotation(direction);
    }
}
