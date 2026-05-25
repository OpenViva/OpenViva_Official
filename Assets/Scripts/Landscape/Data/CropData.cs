using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CropData", menuName = "Scriptable Objects/CropData")]
[Serializable]
public class CropData : ScriptableObject
{
    public float GrowTimer;
    public float MaxScale;
    public Color FinalColor;
    public Color Phase1Color;
    public Color Phase2Color;
}
