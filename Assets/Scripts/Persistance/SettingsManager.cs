using System;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [SerializeField] private GameSettingsData currentSettings = new();
    private string savePath;

    [Header("URP Quality Presets")]
    [SerializeField] private UniversalRenderPipelineAsset[] presetAssets;

    [SerializeField] private UniversalRenderPipelineAsset customAssetTemplate;

    [SerializeField] private UniversalRenderPipelineAsset runtimeCustomAsset;

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer _audioMixer;

    // Getter helpers
    public GameSettingsData Current => currentSettings;
    public int CurrentPresetIndex => currentSettings.qualityLevel;
    public int CurrentResolutionIndex => currentSettings.resolutionIndex;
    public bool IsFullscreen => currentSettings.fullscreen;
    public float MasterVolume => currentSettings.masterVolume;
    public float MusicVolume => currentSettings.musicVolume;
    public float SFXVolume => currentSettings.sfxVolume;
    public float VoiceVolume => currentSettings.voiceVolume;

    [Header("Events")]
    public UnityEvent OnSettingsChanged = new();

    private readonly int[] aaValues = { 0, 2, 4, 8 };

    [Header("Sun/Directional Tag")]
    [SerializeField] private string mainLightTag = "MainLight";

    private Light _mainDirLight;

    #region Unity Methods
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        savePath = Path.Combine(Application.persistentDataPath, "gameSettings.json");

        if (customAssetTemplate != null)
        {
            runtimeCustomAsset = Instantiate(customAssetTemplate);
            runtimeCustomAsset.name = "Runtime Custom URP Asset";
        }
        else
        {
            Debug.LogError("[Settings Manager] Please assign a Custom Asset Template in the Inspector!");
        }

        var mainLight = GameObject.FindGameObjectWithTag(mainLightTag);
        if (mainLight != null)
        {
            _mainDirLight = mainLight.GetComponent<Light>();
            if (_mainDirLight == null || _mainDirLight.type != LightType.Directional)
            {
                Debug.LogError($"Tagget object '{mainLight.name}' not a directional light!");
            }
        }
        else
        {
            Debug.LogWarning($"No GameObject with tag '{mainLightTag}'. Light overrides might fail.");
        }

        LoadSettings();
        ApplyAllSettings();
        OnSettingsChanged.Invoke();
    }
    #endregion

    #region Structs & Enums
    public enum SettingType
    {
        QualityPreset,
        Fullscreen, VSync, Bloom, AntiAliasing, ShadowQuality,
        FPSLimit, ReflectionDistance, ResolutionScale, LODDistance
    }
    #endregion

    #region Auto-Custom Logic
    /// <summary>
    /// If on standard preset, change to the custom one.
    /// Call this inside any specific graphics settings.
    /// </summary>
    private void EnsureCustomPreset()
    {
        int customIndex = presetAssets != null ? presetAssets.Length : 0;

        if (currentSettings.qualityLevel != customIndex)
        {
            currentSettings.qualityLevel = customIndex;
        }
    }
    #endregion

    #region UI Helpers
    private void CycleFPSLimit(int direction)
    {
        int currentIndex = Array.IndexOf(currentSettings.allowedFpsValues, currentSettings.targetFramerate);
        currentIndex = Mathf.Clamp(currentIndex + direction, 0, currentSettings.allowedFpsValues.Length - 1);
        SetFPSLimit(currentIndex);
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

    #region Methods
    public void SetQualityPreset(int presetIndex)
    {
        // Clamp to allowed preset + 1 for custom
        int maxIndex = presetAssets != null ? presetAssets.Length : 0;
        presetIndex = Mathf.Clamp(presetIndex, 0, maxIndex);

        currentSettings.qualityLevel = presetIndex;

        ApplyGraphicsSettings();
        OnSettingsChanged.Invoke();
    }

    public void SetQualityPresetFromSlider(float sliderValue)
    {
        int presetIndex = Mathf.RoundToInt(sliderValue);

        SetQualityPreset(presetIndex);
    }

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
        EnsureCustomPreset();
        index = ((index % aaValues.Length) + aaValues.Length) % aaValues.Length;
        currentSettings.antiAliasing = index;

        ApplyGraphicsSettings();
        OnSettingsChanged.Invoke();
    }

    public void SetShadowQuality(int index)
    {
        EnsureCustomPreset();
        index = Mathf.Clamp(index, 0, 3);
        currentSettings.shadowLevel = index;

        ApplyGraphicsSettings();
        OnSettingsChanged.Invoke();
    }

    private void SetReflectionDistance(float delta)
    {
        EnsureCustomPreset();
        ChangeFloatSetting(ref currentSettings.reflectionDistance,
                 currentSettings.reflectionDistance + delta,
                 min: 0f, max: 2000f);

        ApplyGraphicsSettings();
    }

    public void SetFPSLimit(int index)
    {
        if (index >= 0 && index < currentSettings.allowedFpsValues.Length)
        {
            int value = currentSettings.allowedFpsValues[index];
            currentSettings.targetFramerate = value;

            ApplyGraphicsSettings();
        }
    }

    // Audio Setters
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

    // Cycles

    public void IncreaseFpsLimit() => CycleFPSLimit(+1);
    public void DecreaseFpsLimit() => CycleFPSLimit(-1);

    public void IncreaseReflectionDistance() => SetReflectionDistance(+100f);
    public void DecreaseReflectionDistance() => SetReflectionDistance(-100f);

    public void IncreaseResolutionScale()
    {
        EnsureCustomPreset();
        ChangeIntSetting(ref currentSettings.resolutionScale, 10, 70, 150);
    }
    public void DecreaseResolutionScale()
    {
        EnsureCustomPreset();
        ChangeIntSetting(ref currentSettings.resolutionScale, -10, 70, 150);
    }

    public void IncreaseLodDistance()
    {
        EnsureCustomPreset();
        ChangeFloatSetting(ref currentSettings.lodDistance, 100f, 100f, 1000f);
    }
    public void DecreaseLodDistance()
    {
        EnsureCustomPreset();
        ChangeFloatSetting(ref currentSettings.lodDistance, -100f, 100f, 1000f);
    }

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
            case SettingType.FPSLimit: if (mode == ButtonTextUpdater.ButtonMode.Plus) IncreaseFpsLimit(); else DecreaseFpsLimit(); break;
            case SettingType.ReflectionDistance: if (mode == ButtonTextUpdater.ButtonMode.Plus) IncreaseReflectionDistance(); else DecreaseReflectionDistance(); break;
            case SettingType.ResolutionScale: if (mode == ButtonTextUpdater.ButtonMode.Plus) IncreaseResolutionScale(); else DecreaseResolutionScale(); break;
            case SettingType.LODDistance: if (mode == ButtonTextUpdater.ButtonMode.Plus) IncreaseLodDistance(); else DecreaseLodDistance(); break;
        }

        OnSettingsChanged.Invoke(); // Always refresh UI
    }
    #endregion

    private void ApplyAllSettings()
    {
        ApplyGraphicsSettings();
        ApplyAudioSettings();
    }

    private void ApplyGraphicsSettings()
    {
        // Determine which URP Asset to use (Preset vs Custom)
        bool isCustom = currentSettings.qualityLevel >= presetAssets.Length;
        UniversalRenderPipelineAsset activeAsset = isCustom ? runtimeCustomAsset : presetAssets[currentSettings.qualityLevel];

        if (activeAsset != null)
        {
            // Set URP Asset
            GraphicsSettings.defaultRenderPipeline = activeAsset;

            // Sync the Unity graphics level if using preset
            if (!isCustom)
            {
                QualitySettings.SetQualityLevel(currentSettings.qualityLevel, true);
            }

            // Apply custom overrides to the custom asset
            if (isCustom)
            {
                ApplyCustomURPOverrides(activeAsset);
            }
        }

        // Apply global settings that do not concern the URP Asset
        Application.targetFrameRate = currentSettings.targetFramerate;
        Screen.fullScreen = currentSettings.fullscreen;
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        QualitySettings.vSyncCount = currentSettings.vSync ? 1 : 0;
        QualitySettings.lodBias = currentSettings.lodDistance / 1000f;

        SaveSettings();
    }

    private void ApplyCustomURPOverrides(UniversalRenderPipelineAsset urpAsset)
    {
        // Resolution scale
        urpAsset.renderScale = currentSettings.resolutionScale / 100f;

        // Anti Aliasing
        int msaaSamples = aaValues[currentSettings.resolutionScale % aaValues.Length];
        urpAsset.msaaSampleCount = msaaSamples <= 0 ? 1 : msaaSamples;

        if(_mainDirLight != null)
        {
            switch (currentSettings.shadowLevel)
            {
                case 0:
                    _mainDirLight.shadows = LightShadows.None;
                    urpAsset.shadowCascadeCount = 1;
                    urpAsset.shadowDistance = 64f;
                    break;
                case 1:
                    _mainDirLight.shadows = LightShadows.Soft;
                    urpAsset.shadowDistance = 128f;
                    urpAsset.mainLightShadowmapResolution = 512;
                    urpAsset.additionalLightsShadowmapResolution = 512;
                    urpAsset.shadowCascadeCount = 2;
                    break;
                case 2:
                    _mainDirLight.shadows = LightShadows.Soft;
                    urpAsset.shadowDistance = 256f;
                    urpAsset.mainLightShadowmapResolution = 1024;
                    urpAsset.additionalLightsShadowmapResolution = 1024;
                    urpAsset.shadowCascadeCount = 3;
                    break;
                case 3:
                    _mainDirLight.shadows = LightShadows.Soft;
                    urpAsset.shadowDistance = 512f;
                    urpAsset.mainLightShadowmapResolution = 2048;
                    urpAsset.additionalLightsShadowmapResolution = 2048;
                    urpAsset.shadowCascadeCount = 4;
                    break;
                default:
                    _mainDirLight.shadows = LightShadows.Soft;
                    urpAsset.shadowDistance = 128f;
                    urpAsset.mainLightShadowmapResolution = 1024;
                    urpAsset.additionalLightsShadowmapResolution = 1024;
                    urpAsset.shadowCascadeCount = 3;
                    break;
            }
        }
    }

    private void ApplyAudioSettings()
    {
        if (_audioMixer != null)
        {
            _audioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Clamp(currentSettings.masterVolume, 0.0001f, 1f)) * 20);
            _audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(currentSettings.musicVolume, 0.0001f, 1f)) * 20);
            _audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(currentSettings.sfxVolume, 0.0001f, 1f)) * 20);
            _audioMixer.SetFloat("VoiceVolume", Mathf.Log10(Mathf.Clamp(currentSettings.voiceVolume, 0.0001f, 1f)) * 20);
        }

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
            catch (Exception ex)
            {
                Debug.LogError("Failed to load settings: " + ex.Message + "\nUsing defaults.");
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
        catch (Exception e)
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

    #region Public UI Access
    public string GetDisplayText(SettingType type, string[] customCycleTexts = null)
    {
        var s = currentSettings;
        switch (type)
        {
            case SettingType.QualityPreset: return ReturnQualityLevel();
            case SettingType.Fullscreen: return s.fullscreen ? "Fullscreen" : "Windowed";
            case SettingType.VSync: return s.vSync ? "Enabled" : "Disabled";
            case SettingType.ShadowQuality: return ReturnShadowLevel();
            case SettingType.AntiAliasing: return ReturnAntiAliasingLevel();
            case SettingType.FPSLimit: return s.targetFramerate == -1 ? "Unlimited" : s.targetFramerate.ToString();
            case SettingType.ResolutionScale: return s.resolutionScale.ToString();
            default: return "meow";
        }
    }

    string ReturnQualityLevel()
    {
        switch (currentSettings.qualityLevel)
        {
            case 0: return "Potato";
            case 1: return "Low";
            case 2: return "Medium";
            case 3: return "High";
            case 4: return "Ultra";
            case 5: return "Custom";
            default: return "meow";
        }
    }

    string ReturnShadowLevel()
    {
        switch (currentSettings.shadowLevel)
        {
            case 0: return "OFF";
            case 1: return "Low";
            case 2: return "Medium";
            case 3: return "High";
            default: return "meow";
        }
    }

    string ReturnAntiAliasingLevel()
    {
        switch (currentSettings.antiAliasing)
        {
            case 0: return "OFF";
            case 1: return "2X";
            case 2: return "4X";
            case 3: return "8X";
            default: return "meow";
        }
    }
    #endregion

    public void ResetToDefaults()
    {
        SetDefaultSettings();
        ApplyAllSettings();
        SaveSettings();
    }
}