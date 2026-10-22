using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTextUpdater : MonoBehaviour
{
    public enum ButtonMode { Toggle, CycleNext, CyclePrevious, Plus, Minus }

    [Header("Setting")]
    [SerializeField] private SettingsManager.SettingType setting;

    [Header("Behavior")]
    [Tooltip("Toggle/Cycle for on/off or multi-state. Plus/Minus for numeric steps")]
    [SerializeField] private ButtonMode mode = ButtonMode.Toggle;

    [Header("Display")]
    [SerializeField] private TMP_Text displayText;

    private Button button;
    private SettingsManager settings;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (displayText == null)
            displayText = GetComponentInChildren<TMP_Text>();

        button.onClick.AddListener(OnClick);
    }

    private void OnEnable()
    {
        settings = SettingsManager.Instance;
        settings.OnSettingsChanged.AddListener(Refresh);
        Refresh();
    }

    private void OnDisable()
    {
        if (settings != null)
        {
            settings.OnSettingsChanged.RemoveListener(Refresh);
        }
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnClick);
    }

    void OnClick()
    {
        settings.ChangeSetting(setting, mode);
    }

    private void Refresh()
    {
        if (displayText != null)
        {
            displayText.text = settings.GetDisplayText(setting);
        }
    }
}
