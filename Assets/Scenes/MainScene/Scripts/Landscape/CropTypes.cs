using UnityEngine;

[CreateAssetMenu(fileName = "CropTypes", menuName = "Scriptable Objects/CropTypes")]
// Define an enumeration for different types of crops

public class CropTypes : ScriptableObject
{
    public enum CropType
    {
        Cantaloupe,
        Wheat,
        Blueberry,
        Peach,
        Strawberry
    }
}
