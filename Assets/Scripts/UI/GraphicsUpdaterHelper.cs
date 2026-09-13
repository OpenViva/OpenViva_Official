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
            case SettingType.FpsLimit: SettingsManagerOld.Instance.DecreaseFpsLimit(); break;
            case SettingType.ReflectionDistance: SettingsManagerOld.Instance.DecreaseReflectionDistance(); break;
            case SettingType.ResolutionScale: SettingsManagerOld.Instance.DecreaseResolutionScale(); break;
            case SettingType.LodDistance: SettingsManagerOld.Instance.DecreaseLodDistance(); break;
        }
        UpdateDisplay();
    }

    void OnPlus()
    {
        switch (setting)
        {
            case SettingType.FpsLimit: SettingsManagerOld.Instance.IncreaseFpsLimit(); break;
            case SettingType.ReflectionDistance: SettingsManagerOld.Instance.IncreaseReflectionDistance(); break;
            case SettingType.ResolutionScale: SettingsManagerOld.Instance.IncreaseResolutionScale(); break;
            case SettingType.LodDistance: SettingsManagerOld.Instance.IncreaseLodDistance(); break;
        }
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        GameSettingsData gameSettingsData = SettingsManagerOld.Instance.Current;
        switch (setting)
        {
            case SettingType.FpsLimit:
                valueText.text = gameSettingsData.targetFramerate == -1 ? "Unlimited" : gameSettingsData.targetFramerate.ToString();
                break;
            case SettingType.ReflectionDistance:
                valueText.text = gameSettingsData.reflectionDistance.ToString("F0") + " m";
                break;
            case SettingType.ResolutionScale:
                valueText.text = gameSettingsData.resolutionScale + "%";
                break;
            case SettingType.LodDistance:
                valueText.text = gameSettingsData.lodDistance.ToString("F0") + " m";
                break;
        }
    }
}
