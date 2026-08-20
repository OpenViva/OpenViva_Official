using System;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [SerializeField] private GameSettingsData currentSettings = new();
    private string savePath;

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer _audioMixer;

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

    [Header("Sun/Directional Tag")]
    [SerializeField] private string mainLightTag = "sunLight";

    // Cached reference – never look it up again after this.
    private Light _mainDirLight;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "gameSettings.json");

        // Find the main Light
        var mainLight = GameObject.FindWithTag(mainLightTag);
        if (mainLight == null)
        {
            Debug.LogError($"No GameObject found with tag '{mainLightTag}'.");
            return;
        }

        _mainDirLight = mainLight.GetComponent<Light>();
        if (_mainDirLight == null || _mainDirLight.type != LightType.Directional)
        {
            Debug.LogError($"Tagged object '{mainLight.name}' is not a directional light.");
            _mainDirLight = null;
        }

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

    #region Cycle Settings
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
    }

    //public void SetResolution(int resolutionIndex)
    //{
    //    Resolution[] resolutions = Screen.resolutions;
    //    resolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutions.Length - 1);
    //    currentSettings.resolutionIndex = resolutionIndex;
    //    ApplyGraphicsSettings();
    //}

    public void SetFullscreen(bool isFullscreen)
    {
        currentSettings.fullscreen = isFullscreen;
        ApplyGraphicsSettings();
        OnSettingsChanged.Invoke();
    }

    public void SetVSync(bool enabled)
    {
        currentSettings.vSync = enabled;
        ApplyGraphicsSettings();
        OnSettingsChanged.Invoke();
    }

    public void SetAntiAliasing(int index)
    {
        index = ((index % aaValues.Length) + aaValues.Length) % aaValues.Length;

        currentSettings.antiAliasing = index;
        QualitySettings.antiAliasing = aaValues[index];

        ApplyGraphicsSettings();
        OnSettingsChanged.Invoke();
    }

    public void SetShadowQuality(int index)
    {
        index = Mathf.Clamp(index, 0, 3);
        currentSettings.shadowLevel = index;
        ApplyGraphicsSettings();
        OnSettingsChanged.Invoke();
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
    }

    public void SetMusicVolume(float volume)
    {
        currentSettings.musicVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
        OnSettingsChanged.Invoke();
    }

    public void SetSfxVolume(float volume)
    {
        currentSettings.sfxVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
        OnSettingsChanged.Invoke();
    }

    public void SetVoiceVolume(float volume)
    {
        currentSettings.voiceVolume = Mathf.Clamp01(volume);
        ApplyAudioSettings();
        OnSettingsChanged.Invoke();
    }

    public void SetBrightness(float value)
    {
        currentSettings.brightness = Mathf.Clamp01(value);
        ApplyGraphicsSettings();
        OnSettingsChanged.Invoke();
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
        // TODO: Update UI text
    }

    public void SetFpsLimit(int index)
    {
        if (index >= 0 && index < currentSettings.allowedFpsValues.Length)
        {
            int value = currentSettings.allowedFpsValues[index];
            currentSettings.targetFramerate = value;
            ApplyGraphicsSettings();
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
    }

    private void ChangeIntSetting(ref int field, int delta, int min, int max)
    {
        field = Mathf.Clamp(field + delta, min, max);
        ApplyGraphicsSettings();
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
        // Resolution Scale set
        UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urpAsset =
            UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
        if (urpAsset) urpAsset.renderScale = currentSettings.resolutionScale / 100f;

        // Quality Level set
        QualitySettings.SetQualityLevel(currentSettings.qualityLevel, true);

        Debug.LogWarning($"-- Setting Quality Level: {currentSettings.qualityLevel}");

        // Anti Aliasing set
        QualitySettings.antiAliasing = currentSettings.antiAliasing;

        // Framerate Target set
        Application.targetFrameRate = currentSettings.targetFramerate;

        // LOD Bias set
        QualitySettings.lodBias = currentSettings.lodDistance / 100f;

        // Shadow Level set
        switch (currentSettings.shadowLevel)
        {
            case 0:
                _mainDirLight.shadows = LightShadows.None;
                urpAsset.shadowCascadeCount = 1;
                urpAsset.shadowDistance = 20f;
                break;

            case 1:
                _mainDirLight.shadows = LightShadows.Soft;
                urpAsset.shadowDistance = 40f;
                urpAsset.mainLightShadowmapResolution = 256;
                urpAsset.additionalLightsShadowmapResolution = 256;
                urpAsset.shadowCascadeCount = 2;
                break;

            case 2:
                _mainDirLight.shadows = LightShadows.Soft;
                urpAsset.shadowDistance = 60f;
                urpAsset.mainLightShadowmapResolution = 1024;
                urpAsset.additionalLightsShadowmapResolution = 1024;
                urpAsset.shadowCascadeCount = 3;
                break;

            case 3:
                _mainDirLight.shadows = LightShadows.Soft;
                urpAsset.shadowDistance = 120f;
                urpAsset.mainLightShadowmapResolution = 2048;
                urpAsset.additionalLightsShadowmapResolution = 2048;
                urpAsset.shadowCascadeCount = 4;
                break;

            default:
                Debug.LogWarning("Invalid shadow level.");
                break;
        }

        // Fullscreen set
        Screen.fullScreen = currentSettings.fullscreen;

        // VSync set
        QualitySettings.vSyncCount = currentSettings.vSync ? 1 : 0;

        // TODO: Brightness setup (Post-processing Volume, Material, or RenderSettings)
        // RenderSettings.ambientIntensity = currentSettings.brightness;
        // or PostProcessVolume.profile.GetSetting<Bloom>().intensity = currentSettings.brightness * 50f;

        Debug.LogWarning(
            $"Quality Level: {QualitySettings.names[QualitySettings.GetQualityLevel()]} | " +
            $"URP Shadows → " +
            $"Distance: {urpAsset.shadowDistance:F0}m | " +
            $"Cascades: {urpAsset.shadowCascadeCount} | " +
            $"MainRes: {(ShadowResolution)urpAsset.mainLightShadowmapResolution} | " +
            $"AddRes: {(ShadowResolution)urpAsset.additionalLightsShadowmapResolution} | " +
            $"LightMode: {_mainDirLight.shadows}");

        SaveSettings();
    }

    private void ApplyAudioSettings()
    {
        // TODO: Audio mixer setup
        if (_audioMixer != null)
        {
            _audioMixer.SetFloat("MasterVolume", Mathf.Log10(currentSettings.masterVolume) * 20);
            _audioMixer.SetFloat("MusicVolume", Mathf.Log10(currentSettings.musicVolume) * 20);
            _audioMixer.SetFloat("SFXVolume", Mathf.Log10(currentSettings.sfxVolume) * 20);
            _audioMixer.SetFloat("VoiceVolume", Mathf.Log10(currentSettings.voiceVolume) * 20);
        }
        else
        {
            Debug.LogError("[Settings Manager] Audio Mixer reference missing!");
        }

        SaveSettings();
    }

    private void ApplyLanguage()
    {
        // TODO: Add Localization system here
        // LocalizationSettings.SelectedLocale = Locale.CreateLocale(currentSettings.language);

        SaveSettings();
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

    public void SetDefaultSettings()
    {
        currentSettings = new GameSettingsData();

        SaveSettings();
        OnSettingsChanged.Invoke();
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
