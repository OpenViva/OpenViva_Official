using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTextUpdater : MonoBehaviour
{
    [Header("What setting does this button control?")]
    public SettingsManagerOld.SettingType setting;

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
        button.onClick.AddListener(OnClick);
        SettingsManagerOld.Instance.OnSettingsChanged.AddListener(UpdateDisplay);
        UpdateDisplay();
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnClick);
        SettingsManagerOld.Instance.OnSettingsChanged.RemoveListener(UpdateDisplay);
    }

    void OnClick()
    {
        SettingsManagerOld.Instance.ChangeSetting(setting, mode);
    }

    void UpdateDisplay()
    {
        string text = SettingsManagerOld.Instance.GetDisplayText(setting, cycleTexts);
        if (displayText != null)
            displayText.text = text + suffix;
    }
}
