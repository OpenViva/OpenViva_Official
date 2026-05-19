using System;
using UnityEngine;

public class Crop : PlayerKB_GrabObject
{
    public event Action OnIsGrabbed;

    protected override void OnGrabbed()
    {
        OnIsGrabbed?.Invoke();
    }
}
