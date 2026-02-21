using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTextUpdater : MonoBehaviour
{
    [Header("What setting does this button control?")]
    public SettingsManager.SettingType setting;

    [Header("Button Behavior")]
    public ButtonMode mode = ButtonMode.Toggle; // Toggle = ON/OFF, Cycle = next state, Plus/Minus = numeric

    public enum ButtonMode { Toggle, CycleNext, CyclePrevious, Plus, Minus }

    [Header("Display")]
    public TextMeshProUGUI displayText;
    public string[] cycleTexts;         // e.g. "Off", "2x", "4x", "8x" for AA
    public string suffix = "";          // e.g. "%", " m", " fps"

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (displayText == null)
            displayText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (button != null)
            button.onClick.AddListener(OnClick);
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnSettingsChanged.AddListener(UpdateDisplay);
        UpdateDisplay();
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClick);
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnSettingsChanged.RemoveListener(UpdateDisplay);
    }

    void OnClick()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.ChangeSetting(setting, mode);
    }

    void UpdateDisplay()
    {
        if (SettingsManager.Instance == null || displayText == null)
            return;
        string text = SettingsManager.Instance.GetDisplayText(setting, cycleTexts);
        displayText.text = text + suffix;
    }
}
