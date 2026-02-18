[System.Serializable]
public class GameSettingsData
{
    // Keyboard
    public float mouseSensitivity = 2f;

    // Audio
    public float masterVolume = 1.0f;
    public float musicVolume = 0.6f;
    public float sfxVolume = 1.0f;
    public float voiceVolume = 0.6f;

    // Graphics
    public int qualityLevel = 3;
    public int resolutionScale = 100;
    public int resolutionIndex = 100;
    public float reflectionDistance = 100f;
    public float lodDistance = 200f;
    public int antiAliasing = 1;
    public int shadowLevel = 3;
    public int targetFramerate = 90;
    public int[] allowedFpsValues = new int[] { 30, 60, 90, 120, 144, 240, -1 };
    public bool fullscreen = true;
    public bool vSync = true;

    // Post Processing
    public float brightness;

    // Misc
    public string language = "en";
}
