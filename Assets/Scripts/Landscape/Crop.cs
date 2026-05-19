using System;
using UnityEngine;

public class Crop : PlayerKB_GrabObject
{
    public event Action OnIsGrabbed;
    public float GrowTimer = 480f; // Should this depend on Day/Night cycle speed?
    public float MaxScale = 6.33f;
    private float _timer;
    private bool _isGrowing = true;

    protected override void OnGrabbed()
    {
        OnIsGrabbed?.Invoke();
    }

    protected override void Start()
    {
        base.Start();
        _timer = GrowTimer;
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
            float value = 1 - (_timer / GrowTimer);
            Vector3 scale = new Vector3(MaxScale * value, MaxScale * value, MaxScale * value);
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
