using UnityEngine;

public class HandsFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform _handAttach;
    [SerializeField] private Transform _camera;
    private Quaternion _offset;

    private void Awake()
    {
        _offset = transform.rotation;
    }

    private void LateUpdate()
    {
        transform.position = _handAttach.position;
        transform.rotation = _camera.rotation * _offset;
    }
}
