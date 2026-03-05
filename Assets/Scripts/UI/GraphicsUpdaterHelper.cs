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
        var s = SettingsManager.Instance.Current;
        switch (setting)
        {
            case SettingType.FpsLimit:
                valueText.text = s.targetFramerate == -1 ? "Unlimited" : s.targetFramerate.ToString();
                break;
            case SettingType.ReflectionDistance:
                valueText.text = s.reflectionDistance.ToString("F0") + " m";
                break;
            case SettingType.ResolutionScale:
                valueText.text = s.resolutionScale + "%";
                break;
            case SettingType.LodDistance:
                valueText.text = s.lodDistance.ToString("F0") + " m";
                break;
        }
    }
}
