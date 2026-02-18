using UnityEngine;

[CreateAssetMenu(fileName = "CycleSpeeds", menuName = "Scriptable Objects/CycleSpeeds")]
// Enum representing different cycle speeds

public class CycleSpeeds : ScriptableObject
{
    public enum Speed 
    { 
        FiveMinutes, 
        TwentyMinutes, 
        OneHour,
        ThreeHours,
        SixHours,
        TwelveHours,
        OneDay
    }

}
