using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

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

    [Header("Events")]
    public UnityEvent OnSettingsChanged = new();

    private readonly int[] aaValues = { 0, 2, 4, 8 };

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
        OnSettingsChanged.Invoke();
    }

    #region Structs
    public enum SettingType
    {
        Fullscreen, VSync, Bloom, AntiAliasing, ShadowQuality, 
        Anisotropic, FpsLimit, ReflectionDistance, ResolutionScale, LodDistance
    }
    #endregion

    #region Cycles
    private void CycleFpsLimit(int direction)
    {
        int currentIndex = Array.IndexOf(currentSettings.allowedFpsValues, currentSettings.targetFramerate);
        currentIndex = Mathf.Clamp(currentIndex + direction, 0, currentSettings.allowedFpsValues.Length - 1);
        SetFpsLimit(currentIndex);
    }

    public void CycleAntiAliasing()
    {
        SetAntiAliasing(currentSettings.antiAliasing + 1);
    }

    public void CycleShadowQuality()
    {
        SetShadowQuality((currentSettings.shadowLevel + 1) % 4);
    }
    #endregion

    #region Setter Methods
    public void SetQualityLevel(int level)
    {
        level = Mathf.Clamp(level, 0, QualitySettings.names.Length - 1);
        currentSettings.qualityLevel = level;
        ApplyGraphicsSettings();
        SaveSettings();
    }

    //public void SetResolution(int resolutionIndex)
    //{
    //    Resolution[] resolutions = Screen.resolutions;
    //    resolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutions.Length - 1);
    //    currentSettings.resolutionIndex = resolutionIndex;
    //    ApplyGraphicsSettings();
    //    SaveSettings();
    //}

    public void SetFullscreen(bool isFullscreen)
    {
        currentSettings.fullscreen = isFullscreen;
        ApplyGraphicsSettings();
        OnSettingsChanged.Invoke();
        SaveSettings();
    }

    public void SetVSync(bool enabled)
    {
        currentSettings.vSync = enabled;
        ApplyGraphicsSettings();
        OnSettingsChanged.Invoke();
        SaveSettings();
    }

    public void SetAntiAliasing(int index)
    {
        index = ((index % aaValues.Length) + aaValues.Length) % aaValues.Length;

        currentSettings.antiAliasing = index;
        QualitySettings.antiAliasing = aaValues[index];

        ApplyGraphicsSettings();
        OnSettingsChanged.Invoke();
        SaveSettings();
    }

    public void SetShadowQuality(int index)
    {
        index = Mathf.Clamp(index, 0, 3);
        currentSettings.shadowLevel = index;
        ApplyGraphicsSettings();
        OnSettingsChanged.Invoke();
        SaveSettings();
    }

    private void SetReflectionDistance(float delta)
    {
        ChangeFloatSetting(ref currentSettings.reflectionDistance,
                 currentSettings.reflectionDistance + delta,
                 min: 0f, max: 2000f);
        ApplyGraphicsSettings();
    }

    //public void SetTargetFrameRate(int fps)
    //{
    //    currentSettings.targetFramerate = fps; // 0 = unlimited, -1 = platform default, 30/60/120 etc.
    //    ApplyGraphicsSettings();
    //    SaveSettings();
    //}

    public void SetMasterVolume(float volume)
    {
        currentSettings.masterVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
        OnSettingsChanged.Invoke();
        SaveSettings();
    }

    public void SetMusicVolume(float volume)
    {
        currentSettings.musicVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
        OnSettingsChanged.Invoke();
        SaveSettings();
    }

    public void SetSfxVolume(float volume)
    {
        currentSettings.sfxVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
        OnSettingsChanged.Invoke();
        SaveSettings();
    }

    public void SetVoiceVolume(float volume)
    {
        currentSettings.voiceVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
        OnSettingsChanged.Invoke();
        SaveSettings();
    }

    public void SetBrightness(float value)
    {
        currentSettings.brightness = Mathf.Clamp01(value);
        ApplyGraphicsSettings();
        OnSettingsChanged.Invoke();
        SaveSettings();
    }

    public void SetMouseSensitivity(float value)
    {
        currentSettings.mouseSensitivity = Mathf.Clamp(value, 1f, 20f);
        SaveSettings();
        // TODO: Update UI text
    }

    public void SetLanguage(string languageCode)
    {
        currentSettings.language = languageCode;
        ApplyLanguage();
        SaveSettings();
        // TODO: Update UI text
    }

    public void SetFpsLimit(int index)
    {
        if (index >= 0 && index < currentSettings.allowedFpsValues.Length)
        {
            int value = currentSettings.allowedFpsValues[index];
            currentSettings.targetFramerate = value;
            ApplyGraphicsSettings();
            SaveSettings();
        }
    }

    public void IncreaseFpsLimit() => CycleFpsLimit(+1);
    public void DecreaseFpsLimit() => CycleFpsLimit(-1);

    public void IncreaseReflectionDistance() => SetReflectionDistance(+50f);
    public void DecreaseReflectionDistance() => SetReflectionDistance(-50f);

    public void IncreaseResolutionScale() => ChangeIntSetting(ref currentSettings.resolutionScale, 10, 70, 150);
    public void DecreaseResolutionScale() => ChangeIntSetting(ref currentSettings.resolutionScale, -10, 70, 150);

    public void IncreaseLodDistance() => ChangeFloatSetting(ref currentSettings.lodDistance, 50f, 50f, 1000f);
    public void DecreaseLodDistance() => ChangeFloatSetting(ref currentSettings.lodDistance, -50f, 50f, 1000f);

    // Helper methods
    private void ChangeFloatSetting(ref float field, float delta, float min = float.MinValue, float max = float.MaxValue)
    {
        field = Mathf.Clamp(field + delta, min, max);
        ApplyGraphicsSettings();
        SaveSettings();
    }

    private void ChangeIntSetting(ref int field, int delta, int min, int max)
    {
        field = Mathf.Clamp(field + delta, min, max);
        ApplyGraphicsSettings();
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
        QualitySettings.antiAliasing = currentSettings.antiAliasing;

        QualitySettings.SetQualityLevel(currentSettings.qualityLevel, true);

        Application.targetFrameRate = currentSettings.targetFramerate;

        QualitySettings.lodBias = currentSettings.lodDistance / 100f;

        switch (currentSettings.shadowLevel)
        {
            case 0: QualitySettings.shadows = ShadowQuality.Disable; break;
            case 1: QualitySettings.shadows = ShadowQuality.HardOnly; break;
            case 2: QualitySettings.shadows = ShadowQuality.All; QualitySettings.shadowResolution = ShadowResolution.Low; break;
            case 3: QualitySettings.shadows = ShadowQuality.All; QualitySettings.shadowResolution = ShadowResolution.VeryHigh; break;
        }

        UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urpAsset =
            UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
        if (urpAsset) urpAsset.renderScale = currentSettings.resolutionScale / 100f;

        Screen.fullScreen = currentSettings.fullscreen;

        QualitySettings.vSyncCount = currentSettings.vSync ? 1 : 0;

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
            antiAliasing = 2,
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

    public void ChangeSetting(SettingType type, ButtonTextUpdater.ButtonMode mode)
    {
        var s = currentSettings;

        switch (type)
        {
            // ── Toggles ─────────────────────────────────────
            case SettingType.Fullscreen: SetFullscreen(mode == ButtonTextUpdater.ButtonMode.Toggle ? !s.fullscreen : s.fullscreen); break;
            case SettingType.VSync: SetVSync(!s.vSync); break;

            // ── Cycle (multiple states) ───────────────────────
            case SettingType.AntiAliasing: CycleAntiAliasing(); break;
            case SettingType.ShadowQuality: CycleShadowQuality(); break;

            // ── Numeric + / – ─────────────────────────────────
            case SettingType.FpsLimit: if (mode == ButtonTextUpdater.ButtonMode.Plus) IncreaseFpsLimit(); else DecreaseFpsLimit(); break;
            case SettingType.ReflectionDistance: if (mode == ButtonTextUpdater.ButtonMode.Plus) IncreaseReflectionDistance(); else DecreaseReflectionDistance(); break;
            case SettingType.ResolutionScale: if (mode == ButtonTextUpdater.ButtonMode.Plus) IncreaseResolutionScale(); else DecreaseResolutionScale(); break;
            case SettingType.LodDistance: if (mode == ButtonTextUpdater.ButtonMode.Plus) IncreaseLodDistance(); else DecreaseLodDistance(); break;
        }

        OnSettingsChanged.Invoke(); // Always refresh UI
    }

    public string GetDisplayText(SettingType type, string[] customCycleTexts = null)
    {
        var s = currentSettings;
        switch (type)
        {
            case SettingType.Fullscreen: return s.fullscreen ? "Fullscreen" : "Windowed";
            case SettingType.VSync: return s.vSync ? "Enabled" : "Disabled";
            case SettingType.ShadowQuality: return ReturnShadowLevel();
            case SettingType.AntiAliasing: return ReturnAntiAliasingLevel();
            case SettingType.FpsLimit: return s.targetFramerate == -1 ? "Unlimited" : s.targetFramerate.ToString();
            case SettingType.ResolutionScale: return s.resolutionScale.ToString();
            default: return "meow";
        }
    }

    string ReturnShadowLevel()
    {
        switch (currentSettings.shadowLevel)
        {
            case 0:
                return "OFF";
            case 1:
                return "Low";
            case 2:
                return "Medium";
            case 3:
                return "High";
            default:
                return "";
        }
    }

    string ReturnAntiAliasingLevel()
    {
        switch (currentSettings.antiAliasing)
        {
            case 0:
                return "OFF";
            case 1:
                return "2X";
            case 2:
                return "4X";
            case 3:
                return "8X";
            default:
                return "";
        }
    }

    public void ResetToDefaults()
    {
        SetDefaultSettings();
        ApplyAllSettings();
        SaveSettings();
    }
}
