using UnityEngine;

public class EggDisappear : MonoBehaviour
{
    [SerializeField] private float _timer = 3f;

    private float _initialX;
    private float _initialY;
    private float _initialZ;

    private void Awake()
    {
        _initialX = transform.localScale.x;
        _initialY = transform.localScale.y;
        _initialZ = transform.localScale.z;
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0) { Destroy(gameObject); }
        if (_timer <= 1) { transform.localScale = new(_initialX * _timer, _initialY * _timer, _initialZ * _timer); }
    }
}
