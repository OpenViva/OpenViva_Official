[System.Serializable]
public class GameSettingsData
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
    public int targetFramerate = 90;
    public bool fullscreen = true;
    public int resolutionIndex;
    public bool vSync = true;

    // Post Processing
    public float brightness;

    // Misc
    public string language = "en";
}
