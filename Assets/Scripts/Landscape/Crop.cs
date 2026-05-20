using System;
using UnityEngine;

public class Crop : PlayerKB_GrabObject
{
    public event Action OnIsGrabbed;

    [SerializeField] private CropData _data;
    private float _growTimer; // Should this depend on Day/Night cycle speed?
    private float _maxScale;
    private float _timer;
    private bool _isGrowing = true;

    protected override void OnGrabbed()
    {
        base.OnGrabbed();
        OnIsGrabbed?.Invoke();
    }

    protected override void Start()
    {
        base.Start();
        _growTimer = _data.GrowTimer;
        _maxScale = _data.MaxScale;
        _timer = _growTimer;
    }

    private void Update()
    {
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
        }
        else
        {
            _isGrowing = false;
        }

        if (_isGrowing)
        {
            float value = 1 - (_timer / _growTimer);
            Vector3 scale = new Vector3(_maxScale * value, _maxScale * value, _maxScale * value);
            transform.localScale = scale;
        }
    }

    protected override void GrabLeft()
    {
        if (!_isGrowing)
        {
            base.GrabLeft();
        }
    }

    protected override void GrabRight()
    {
        if (!_isGrowing)
        {
            base.GrabRight();
        }
    }

    protected override void OnTriggerEnter(Collider collider)
    {
        if (!_isGrowing)
        {
            base.OnTriggerEnter(collider);
        }
    }

    protected override void OnTriggerExit(Collider collider)
    {
        if (!_isGrowing)
        {
            base.OnTriggerExit(collider);
        }
    }
}
