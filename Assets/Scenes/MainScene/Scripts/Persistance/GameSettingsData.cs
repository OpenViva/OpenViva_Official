using UnityEngine;

public class GameSettingsData : ScriptableObject, ISerializationCallbackReceiver
{
    // Keyboard
    public float mouseSensitivity = 10f;

    // Audio
    public float masterVolume = 1.0f;
    public float musicVolume = 0.6f;
    public float sfxVolume = 1.0f;
    public float voiceVolume = 0.6f;

    // Graphics
    public int qualityLevel = 3;
    public float lodDistance = 1f;
    public int antiAliasing = 3;
    public int shadowLevel = 3;
    public int fpsLimit = 90;
    public bool fullscreen = true;
    public bool vSync = true;

    public void OnAfterDeserialize()
    {
        throw new System.NotImplementedException();
    }

    public void OnBeforeSerialize()
    {
        throw new System.NotImplementedException();
    }
}
