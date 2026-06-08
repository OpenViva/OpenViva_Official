using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MealData", menuName = "Scriptable Objects/MealData")]
[Serializable]
public class MealData : ScriptableObject
{
    // public float Price;
    // public float FavorGain;
    // public float Lifetime;
    public float BurnTimer;
    public GameObject BurnVariant;
}
