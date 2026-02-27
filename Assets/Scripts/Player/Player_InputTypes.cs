using UnityEngine;

[CreateAssetMenu(fileName = "Player_InputTypes", menuName = "Scriptable Objects/Player_InputTypes")]
// The current input type being used by the player

public class Player_InputTypes : ScriptableObject
{
    public enum InputType
    {
        KBM,    // Keyboard and Mouse
        VR      // Virtual Reality
    }
}
