using System.IO;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [SerializeField] private GameSettingsData currentSettings = new();
    private string savePath;

    // Getter helpers
    public GameSettingsData Current => currentSettings;
    public int CurrentQualityLevel => currentSettings.qualityLevel;
    public int CurrentResolutionIndex => currentSettings.resolutionIndex;
    public bool IsFullscreen => currentSettings.fullscreen;
    public float MasterVolume => currentSettings.masterVolume;
    public float Brightness => currentSettings.brightness;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "gamesettings.json");

        LoadSettings();
        ApplyAllSettings();
    }

    #region Setter Methods
    public void SetQualityLevel(int level)
    {
        level = Mathf.Clamp(level, 0, QualitySettings.names.Length - 1);
        currentSettings.qualityLevel = level;
        ApplyGraphicsSettings();
        SaveSettings();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution[] resolutions = Screen.resolutions;
        resolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutions.Length - 1);
        currentSettings.resolutionIndex = resolutionIndex;
        ApplyGraphicsSettings();
        SaveSettings();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        currentSettings.fullscreen = isFullscreen;
        ApplyGraphicsSettings();
        SaveSettings();
    }

    public void SetVSync(bool enabled)
    {
        currentSettings.vSync = enabled; // ← make sure you added this field to GameSettings
        ApplyGraphicsSettings();
        SaveSettings();
    }

    public void SetTargetFrameRate(int fps)
    {
        currentSettings.targetFramerate = fps; // 0 = unlimited, -1 = platform default, 30/60/120 etc.
        ApplyGraphicsSettings();
        SaveSettings();
    }

    public void SetMasterVolume(float volume)
    {
        currentSettings.masterVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
        SaveSettings();
    }

    public void SetMusicVolume(float volume)
    {
        currentSettings.musicVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
        SaveSettings();
    }

    public void SetSfxVolume(float volume)
    {
        currentSettings.sfxVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
        SaveSettings();
    }

    public void SetVoiceVolume(float volume)
    {
        currentSettings.voiceVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
        SaveSettings();
    }

    public void SetBrightness(float value)
    {
        currentSettings.brightness = Mathf.Clamp01(value);
        ApplyGraphicsSettings();
        SaveSettings();
    }

    public void SetMouseSensitivity(float value)
    {
        currentSettings.mouseSensitivity = Mathf.Clamp(value, 1f, 20f);
        SaveSettings();
    }

    public void SetLanguage(string languageCode)
    {
        currentSettings.language = languageCode;
        ApplyLanguage();
        SaveSettings();
    }
    #endregion

    private void ApplyAllSettings()
    {
        ApplyGraphicsSettings();
        ApplyAudioSettings();
        ApplyLanguage();
        // Add more Apply---() calls here
    }

    private void ApplyGraphicsSettings()
    {
        QualitySettings.SetQualityLevel(currentSettings.qualityLevel, true);

        Resolution[] resolutions = Screen.resolutions;
        if (currentSettings.resolutionIndex >= 0 && currentSettings.resolutionIndex < resolutions.Length)
        {
            Resolution target = resolutions[currentSettings.resolutionIndex];
            Screen.SetResolution(target.width, target.height, currentSettings.fullscreen);
        }

        Screen.fullScreen = currentSettings.fullscreen;

        QualitySettings.vSyncCount = currentSettings.vSync ? 1 : 0;

        Application.targetFrameRate = currentSettings.targetFramerate > 0
            ? currentSettings.targetFramerate
            : -1;

        // TODO: Brightness setup (Post-processing Volume, Material, or RenderSettings)
        // RenderSettings.ambientIntensity = currentSettings.brightness;
        // or PostProcessVolume.profile.GetSetting<Bloom>().intensity = currentSettings.brightness * 50f;
    }

    private void ApplyAudioSettings()
    {
        AudioListener.volume = currentSettings.masterVolume;

        // TODO: Audio mixer setup
        // audioMixer.SetFloat("MasterVolume", Mathf.Log10(currentSettings.masterVolume) * 20);
        // audioMixer.SetFloat("MusicVolume", Mathf.Log10(currentSettings.musicVolume) * 20);
        // audioMixer.SetFloat("SfxVolume",   Mathf.Log10(currentSettings.sfxVolume) * 20);
    }

    private void ApplyLanguage()
    {
        // TODO: Add Localization system here
        // LocalizationSettings.SelectedLocale = Locale.CreateLocale(currentSettings.language);
    }

    public void LoadSettings()
    {
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                JsonUtility.FromJsonOverwrite(json, currentSettings);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to load settings: " + e.Message + "\nUsing defaults.");
                SetDefaultSettings();
            }
        }
        else
        {
            SetDefaultSettings();
        }
    }

    public void SaveSettings()
    {
        try
        {
            string json = JsonUtility.ToJson(currentSettings, true);
            File.WriteAllText(savePath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save settings: " + e.Message);
        }
    }

    private void SetDefaultSettings()
    {
        currentSettings = new GameSettingsData
        {
            qualityLevel = QualitySettings.names.Length - 2, // "High" by default
            resolutionIndex = GetCurrentResolutionIndex(),
            fullscreen = true,
            vSync = true,
            targetFramerate = 90,
            masterVolume = 1f,
            musicVolume = 0.6f,
            sfxVolume = 1f,
            voiceVolume = 0.6f,
            brightness = 1f,
            mouseSensitivity = 10f,
            language = "en"
        };
    }

    private int GetCurrentResolutionIndex()
    {
        Resolution[] resolutions = Screen.resolutions;
        Resolution current = Screen.currentResolution;
        for (int i = resolutions.Length - 1; i >= 0; i--)
        {
            if (resolutions[i].width == current.width && resolutions[i].height == current.height)
                return i;
        }
        return resolutions.Length - 1;
    }

    public void ResetToDefaults()
    {
        SetDefaultSettings();
        ApplyAllSettings();
        SaveSettings();
    }
}
