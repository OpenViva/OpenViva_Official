using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [SerializeField] private GameSettingsData currentSettings = new();

    [Header("URP Quality Presets")]
    [SerializeField] private UniversalRenderPipelineAsset[] presetAssets;
    [SerializeField] private UniversalRenderPipelineAsset customAssetTemplate;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Lighting")]
    [SerializeField] private string mainLightTag = "MainLight";

    [Header("Events")]
    public UnityEvent OnSettingsChanged = new();

    // --- Constants & lookup tables ---
    private const float LodBiasMin = 0.5f;
    private const float LodBiasMax = 3f;
    private const float LodBiasStep = 0.5f;

    private const int ScaleMin = 70;
    private const int ScaleMax = 150;
    private const int ScaleStep = 10;

    private const float SaveDelay = 0.5f;

    private static readonly int[] AntiAliasingValues = { 0, 2, 4, 8 };
    private static readonly string[] AntiAliasingNames = { "OFF", "2X", "4X", "8X" };
    private static readonly string[] PresetNames = { "Potato", "Low", "Medium", "High", "Ultra" };
    private static readonly string[] ShadowNames = { "OFF", "Low", "Medium", "High" };

    private struct ShadowProfile
    {
        public float distance;
        public int resolution;
        public int cascades;
    }

    private static readonly ShadowProfile[] ShadowProfiles =
    {
        new() { distance = 64f,  resolution = 512,  cascades = 1 },
        new() { distance = 128f, resolution = 512,  cascades = 2 },
        new() { distance = 256f, resolution = 1024, cascades = 3 },
        new() { distance = 512f, resolution = 2048, cascades = 4 },
    };

    // --- Runtime State ---
    private string savePath;
    private Light mainDirLight;
    private Resolution[] resolutionOptions;
    private bool appliedFullscreen;
    private Vector2Int appliedSize = new Vector2Int(-1, -1);

    private bool saveDirty;
    private float saveTimer;

    // --- Getters ---
    public GameSettingsData Current => currentSettings;
    public int PresetCount => presetAssets != null ? presetAssets.Length : 0;
    public int CustomPresetIndex => PresetCount;
    public bool IsCustomPreset => currentSettings.qualityLevel >= CustomPresetIndex;
    public int CurrentPresetIndex => currentSettings.qualityLevel;
    public int CurrentResolutionIndex => currentSettings.resolutionIndex;
    public bool IsFullscreen => currentSettings.fullscreen;
    public int ResolutionScale => currentSettings.resolutionScale;
    public float LodBias => currentSettings.lodBias;
    public float MasterVolume => currentSettings.masterVolume;
    public float MusicVolume => currentSettings.musicVolume;
    public float SFXVolume => currentSettings.sfxVolume;
    public float VoiceVolume => currentSettings.voiceVolume;

    /// <summary> Labels for the resolution dropdown: "1920 x 1080".</summary>
    public string[] ResolutionOptions =>
        resolutionOptions.Select(r => $"{r.width} x {r.height}").ToArray();

    #region Unity Methods
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        savePath = Path.Combine(Application.persistentDataPath, "gameSettings.json");

        BuildResolutionList();
        CacheMainLight();

        LoadSettings();
        ApplyAllSettings();
        OnSettingsChanged.Invoke();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (!saveDirty) return;
        saveTimer -= Time.unscaledDeltaTime;
        if (saveTimer <= 0f) FlushSave();
    }

    private void OnApplicationPause(bool paused) { if (paused) FlushSave(); }
    private void OnApplicationQuit() => FlushSave();
    private void OnDestroy() { if (Instance == this) FlushSave(); }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // The directional light is scene-specific, so re-find it after every load
        CacheMainLight();
        ApplyGraphicsSettings();
    }
    #endregion

    #region Setup Helpers
    private void BuildResolutionList()
    {
        resolutionOptions = Screen.resolutions
            .GroupBy(r => (r.width, r.height))
            .Select(g => g.OrderByDescending(r => r.refreshRateRatio.value).First())
            .OrderByDescending(r => r.width)
            .ThenByDescending(r => r.height)
            .ToArray();

        if (resolutionOptions.Length == 0)
        {
            resolutionOptions = new[] { Screen.currentResolution };
        }
    }

    private void CacheMainLight()
    {
        mainDirLight = null;
        GameObject go = GameObject.FindGameObjectWithTag(mainLightTag);
        if (go == null)
        {
            Debug.LogWarning($"[Settings Manager] No GameObject with tag '{mainLightTag}'. Shadow overrides won't apply.");
            return;
        }

        Light light = go.GetComponent<Light>();
        if (light == null || light.type != LightType.Directional)
        {
            Debug.LogError($"[Settings Manager] Tagged object '{go.name}' is not a directional light!");
            return;
        }
        mainDirLight = light;
    }
    #endregion

    #region Save / Load
    public void LoadSettings()
    {
        currentSettings = NewDefaults();

        if (TryLoad(savePath) || TryLoad(savePath + ".bak"))
        {
            SanitizeSettings();
            return;
        }

        if (File.Exists(savePath))
        {
            try
            {
                File.Copy(savePath, savePath + ".corrupt", true);
            }
            catch (Exception)
            {
            }

            Debug.LogError($"[Settings Manager] Settings were unreadable. Using defaults.");
        }

        SaveSettings();
        SanitizeSettings();
    }

    private bool TryLoad(string path)
    {
        if (!File.Exists(path)) return false;

        try
        {
            GameSettingsData loaded = NewDefaults();
            JsonUtility.FromJsonOverwrite(File.ReadAllText(path), loaded);
            currentSettings = loaded;
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Settings Manager] Could not read '{path}': {ex.Message}");
            return false;
        }
    }

    public void SaveSettings()
    {
        try
        {
            string json = JsonUtility.ToJson(currentSettings, true);
            string tmp = savePath + ".tmp";
            File.WriteAllText(tmp, json);

            if (File.Exists(savePath))
            {
                File.Replace(tmp, savePath, savePath + ".bak");
            }
            else
            {
                File.Move(tmp, savePath);
            }

            saveDirty = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Settings Manager] Failed to save settings: {ex.Message}");
        }
    }

    public void SetDefaultSettings()
    {
        currentSettings = NewDefaults();
        ApplyAllSettings();
        SaveSettings();
        OnSettingsChanged.Invoke();
    }

    public void ResetToDefaults() => SetDefaultSettings();

    private GameSettingsData NewDefaults()
    {
        GameSettingsData s = new GameSettingsData();
        s.resolutionIndex = FindCurrentResolutionIndex();
        return s;
    }

    private void SanitizeSettings()
    {
        GameSettingsData s = currentSettings;
        s.qualityLevel = Mathf.Clamp(s.qualityLevel, 0, CustomPresetIndex);
        s.antiAliasing = Wrap(s.antiAliasing, AntiAliasingValues.Length);
        s.shadowLevel = Mathf.Clamp(s.shadowLevel, 0, ShadowProfiles.Length - 1);
        s.resolutionScale = Mathf.Clamp(s.resolutionScale, ScaleMin, ScaleMax);
        s.resolutionIndex = Mathf.Clamp(s.resolutionIndex, 0, resolutionOptions.Length - 1);
        s.lodBias = SnapLodBias(s.lodBias);
        s.masterVolume = Mathf.Clamp01(s.masterVolume);
        s.musicVolume = Mathf.Clamp01(s.musicVolume);
        s.sfxVolume = Mathf.Clamp01(s.sfxVolume);
        s.voiceVolume = Mathf.Clamp01(s.voiceVolume);
    }

    // Only mark as dirty and save later to not save to disk every time a slider is moved
    private void MarkDirty()
    {
        saveDirty = true;
        saveTimer = SaveDelay;
        OnSettingsChanged.Invoke();
    }

    private void FlushSave()
    {
        if (saveDirty) SaveSettings();
    }
    #endregion

    #region Commit Helpers
    private void CommitGraphics()
    {
        ApplyGraphicsSettings();
        MarkDirty();
    }

    private void CommitAudio()
    {
        ApplyAudioSettings();
        MarkDirty();
    }

    private void ApplyAllSettings()
    {
        ApplyGraphicsSettings();
        ApplyAudioSettings();
    }
    #endregion

    #region Quality / Graphics Setters
    public void SetQualityPreset(int index)
    {
        currentSettings.qualityLevel = Mathf.Clamp(index, 0, CustomPresetIndex);
        CommitGraphics();
    }

    public void SetQualityPresetFromSlider(float sliderValue) =>
        SetQualityPreset(Mathf.RoundToInt(sliderValue));

    public void SetFullscreen(bool on)
    {
        currentSettings.fullscreen = on;
        CommitGraphics();
    }

    public void ToggleFullscreen() => SetFullscreen(!currentSettings.fullscreen);

    public void SetVSync(bool on)
    {
        currentSettings.vSync = on;
        CommitGraphics();
    }

    public void SetAntiAliasing(int index)
    {
        EnsureCustomPreset();
        currentSettings.antiAliasing = Wrap(index, AntiAliasingValues.Length);
        CommitGraphics();
    }

    public void CycleAntiAliasing() => SetAntiAliasing(currentSettings.antiAliasing + 1);

    public void SetShadowQuality(int index)
    {
        EnsureCustomPreset();
        currentSettings.shadowLevel = Mathf.Clamp(index, 0, ShadowProfiles.Length - 1);
        CommitGraphics();
    }

    public void CycleShadowQuality() =>
        SetShadowQuality((currentSettings.shadowLevel + 1) % ShadowProfiles.Length);

    public void SetFPSLimit(int index)
    {
        int[] values = currentSettings.allowedFpsValues;
        if (values == null || index < 0 || index >= values.Length) return;

        currentSettings.targetFramerate = values[index];
        CommitGraphics();
    }

    public void IncreaseFpsLimit() => CycleFPSLimit(+1);
    public void DecreaseFpsLimit() => CycleFPSLimit(-1);

    private void CycleFPSLimit(int direction)
    {
        int[] values = currentSettings.allowedFpsValues;
        if (values == null || values.Length == 0) return;

        int idx = Array.IndexOf(values, currentSettings.targetFramerate);
        SetFPSLimit(Mathf.Clamp(idx + direction, 0, values.Length - 1));
    }

    public void SetResolutionScale(int percent)
    {
        EnsureCustomPreset();
        currentSettings.resolutionScale = Mathf.Clamp(percent, ScaleMin, ScaleMax);
        CommitGraphics();
    }

    public void IncreaseResolutionScale() => SetResolutionScale(currentSettings.resolutionScale + ScaleStep);
    public void DecreaseResolutionScale() => SetResolutionScale(currentSettings.resolutionScale - ScaleStep);

    public void SetResolutionIndex(int index)
    {
        currentSettings.resolutionIndex = Mathf.Clamp(index, 0, resolutionOptions.Length - 1);
        CommitGraphics();
    }

    public void IncreaseResolution() => SetResolutionIndex(currentSettings.resolutionIndex + 1);
    public void DecreaseResolution() => SetResolutionIndex(currentSettings.resolutionIndex - 1);

    public void SetLodBias(float value)
    {
        currentSettings.lodBias = SnapLodBias(value);
        CommitGraphics();
    }

    public void IncreaseLodBias() => SetLodBias(currentSettings.lodBias + LodBiasStep);
    public void DecreaseLodBias() => SetLodBias(currentSettings.lodBias - LodBiasStep);
    #endregion

    #region Audio Setters
    public void SetMasterVolume(float volume) { currentSettings.masterVolume = Mathf.Clamp01(volume); CommitAudio(); }
    public void SetMusicVolume(float volume) { currentSettings.musicVolume = Mathf.Clamp01(volume); CommitAudio(); }
    public void SetSfxVolume(float volume) { currentSettings.sfxVolume = Mathf.Clamp01(volume); CommitAudio(); }
    public void SetVoiceVolume(float volume) { currentSettings.voiceVolume = Mathf.Clamp01(volume); CommitAudio(); }
    #endregion

    #region Button Dispatcher
    public void ChangeSetting(SettingType type, ButtonTextUpdater.ButtonMode mode)
    {
        bool plus = mode == ButtonTextUpdater.ButtonMode.Plus;

        switch (type)
        {
            case SettingType.Fullscreen:
                if (mode == ButtonTextUpdater.ButtonMode.Toggle) ToggleFullscreen();
                break;
            case SettingType.VSync: SetVSync(!currentSettings.vSync); break;
            case SettingType.AntiAliasing: CycleAntiAliasing(); break;
            case SettingType.ShadowQuality: CycleShadowQuality(); break;
            case SettingType.FPSLimit: if (plus) IncreaseFpsLimit(); else DecreaseFpsLimit(); break;
            case SettingType.ResolutionScale: if (plus) IncreaseResolutionScale(); else DecreaseResolutionScale(); break;
            case SettingType.Resolution: if (plus) IncreaseResolution(); else DecreaseResolution(); break;
            case SettingType.LODBias: if (plus) IncreaseLodBias(); else DecreaseLodBias(); break;
        }

        OnSettingsChanged.Invoke();
    }
    #endregion

    #region Apply to Engine
    private void ApplyGraphicsSettings()
    {
        int level = currentSettings.qualityLevel;
        if (QualitySettings.GetQualityLevel() != level)
        {
            QualitySettings.SetQualityLevel(level, true);
        }

        if (IsCustomPreset && QualitySettings.renderPipeline is UniversalRenderPipelineAsset urp)
        {
            ApplyCustomURPOverrides(urp);
        }

        QualitySettings.vSyncCount = currentSettings.vSync ? 1 : 0;
        QualitySettings.lodBias = currentSettings.lodBias;
        Application.targetFrameRate = currentSettings.targetFramerate;

        ApplyWindowMode();
    }

    private void ApplyCustomURPOverrides(UniversalRenderPipelineAsset urp)
    {
        urp.renderScale = currentSettings.resolutionScale / 100f;

        int samples = Mathf.Max(1, AntiAliasingValues[currentSettings.antiAliasing]);
        if (urp.msaaSampleCount != samples) urp.msaaSampleCount = samples;

        ShadowProfile profile = ShadowProfiles[currentSettings.shadowLevel];
        urp.shadowDistance = profile.distance;
        urp.shadowCascadeCount = profile.cascades;
        urp.mainLightShadowmapResolution = profile.resolution;
        urp.additionalLightsShadowmapResolution = profile.resolution;

        if (mainDirLight != null)
        {
            mainDirLight.shadows = currentSettings.shadowLevel == 0 ? LightShadows.None : LightShadows.Soft;
        }
    }

    private void ApplyWindowMode()
    {
        bool fullscreen = currentSettings.fullscreen;
        Vector2Int size = GetTargetSize();

        if (fullscreen == appliedFullscreen && size == appliedSize) return;

        FullScreenMode mode = fullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;
        Screen.SetResolution(size.x, size.y, mode);

        appliedFullscreen = fullscreen;
        appliedSize = size;
    }

    private Vector2Int GetTargetSize()
    {
        int i = Mathf.Clamp(currentSettings.resolutionIndex, 0, resolutionOptions.Length - 1);
        Resolution r = resolutionOptions[i];
        Vector2Int size = new Vector2Int(r.width, r.height);

        if (!currentSettings.fullscreen)
        {
            Display display = Display.main;
            size.x = Mathf.Min(size.x, display.systemWidth);
            size.y = Mathf.Min(size.y, display.systemHeight);
        }
        return size;
    }

    private void ApplyAudioSettings()
    {
        if (audioMixer == null) return;

        SetMixerVolume("MasterVolume", currentSettings.masterVolume);
        SetMixerVolume("MusicVolume", currentSettings.musicVolume);
        SetMixerVolume("SFXVolume", currentSettings.sfxVolume);
        SetMixerVolume("VoiceVolume", currentSettings.voiceVolume);
    }

    private void SetMixerVolume(string param, float linear) =>
        audioMixer.SetFloat(param, Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20);
    #endregion

    #region Helpers
    private void EnsureCustomPreset()
    {
        if (!IsCustomPreset) currentSettings.qualityLevel = CustomPresetIndex;
    }

    private int FindCurrentResolutionIndex()
    {
        Resolution curr = Screen.currentResolution;
        int idx = Array.FindIndex(resolutionOptions, r => r.width == curr.width && r.height == curr.height);
        return idx >= 0 ? idx : resolutionOptions.Length - 1;
    }

    private static float SnapLodBias(float value) =>
        Mathf.Clamp(Mathf.Round(value / LodBiasStep) * LodBiasStep, LodBiasMin, LodBiasMax);

    private static int Wrap(int value, int count) => ((value % count) + count) % count;
    #endregion

    #region Text Display
    public string GetDisplayText(SettingType type)
    {
        GameSettingsData s = currentSettings;
        switch (type)
        {
            case SettingType.QualityPreset:
                return IsCustomPreset ? "Custom" : PresetNames[Mathf.Clamp(s.qualityLevel, 0, PresetNames.Length - 1)];
            case SettingType.Fullscreen: return s.fullscreen ? "Fullscreen" : "Windowed";
            case SettingType.VSync: return s.vSync ? "Enabled" : "Disabled";
            case SettingType.Bloom:
            case SettingType.AntiAliasing: return AntiAliasingNames[s.antiAliasing];
            case SettingType.ShadowQuality: return ShadowNames[s.shadowLevel];
            case SettingType.FPSLimit: return s.targetFramerate == -1 ? "Unlimited" : s.targetFramerate.ToString();
            case SettingType.ResolutionScale: return $"{s.resolutionScale}%";
            case SettingType.LODBias: return s.lodBias.ToString("0.#", CultureInfo.InvariantCulture);
            case SettingType.Resolution:
                Resolution r = resolutionOptions[s.resolutionIndex];
                return $"{r.width} x {r.height}";
            default: return "Unknown";
        }
    }
    #endregion

    #region Enums
    public enum SettingType
    {
        QualityPreset,
        Fullscreen, VSync, Bloom, AntiAliasing, ShadowQuality,
        FPSLimit, ResolutionScale, LODBias,
        Resolution
    }
    #endregion
}