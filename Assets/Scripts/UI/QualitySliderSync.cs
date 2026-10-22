using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class QualitySliderSync : MonoBehaviour
{
    [SerializeField] private TMP_Text presetLabel;

    private Slider slider;
    private SettingsManager settings;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnSliderChanged);
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
        slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        settings.SetQualityPresetFromSlider(value);
    }

    private void Refresh()
    {
        slider.SetValueWithoutNotify(settings.CurrentPresetIndex);

        if (presetLabel != null)
        {
            presetLabel.text = settings.GetDisplayText(SettingsManager.SettingType.QualityPreset);
        }
    }
}
