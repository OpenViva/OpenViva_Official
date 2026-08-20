using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsUpdaterHelper : MonoBehaviour
{
    [Header("References")]
    public Button minusButton;
    public Button plusButton;
    public TextMeshProUGUI valueText;

    public enum SettingType { FpsLimit, ReflectionDistance, ResolutionScale, LodDistance }
    public SettingType setting;

    private GameSettingsData _gameSettingsData;

    private void Start()
    {
        _gameSettingsData = SettingsManager.Instance.Current;
    }

    private void OnEnable()
    {
        minusButton.onClick.AddListener(OnMinus);
        plusButton.onClick.AddListener(OnPlus);
        UpdateDisplay();
    }

    private void OnDisable()
    {
        minusButton.onClick.RemoveListener(OnMinus);
        plusButton.onClick.RemoveListener(OnPlus);
    }

    void OnMinus()
    {
        switch (setting)
        {
            case SettingType.FpsLimit: SettingsManager.Instance.DecreaseFpsLimit(); break;
            case SettingType.ReflectionDistance: SettingsManager.Instance.DecreaseReflectionDistance(); break;
            case SettingType.ResolutionScale: SettingsManager.Instance.DecreaseResolutionScale(); break;
            case SettingType.LodDistance: SettingsManager.Instance.DecreaseLodDistance(); break;
        }
        UpdateDisplay();
    }

    void OnPlus()
    {
        switch (setting)
        {
            case SettingType.FpsLimit: SettingsManager.Instance.IncreaseFpsLimit(); break;
            case SettingType.ReflectionDistance: SettingsManager.Instance.IncreaseReflectionDistance(); break;
            case SettingType.ResolutionScale: SettingsManager.Instance.IncreaseResolutionScale(); break;
            case SettingType.LodDistance: SettingsManager.Instance.IncreaseLodDistance(); break;
        }
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        switch (setting)
        {
            case SettingType.FpsLimit:
                valueText.text = _gameSettingsData.targetFramerate == -1 ? "Unlimited" : _gameSettingsData.targetFramerate.ToString();
                break;
            case SettingType.ReflectionDistance:
                valueText.text = _gameSettingsData.reflectionDistance.ToString("F0") + " m";
                break;
            case SettingType.ResolutionScale:
                valueText.text = _gameSettingsData.resolutionScale + "%";
                break;
            case SettingType.LodDistance:
                valueText.text = _gameSettingsData.lodDistance.ToString("F0") + " m";
                break;
        }
    }
}
