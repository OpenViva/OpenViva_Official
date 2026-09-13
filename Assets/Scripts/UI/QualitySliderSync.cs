
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QualitySliderSync : MonoBehaviour
{
    private Slider _slider;

    [SerializeField] private TMP_Text _presetLabel;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void Start()
    {
        SettingsManager.Instance.OnSettingsChanged.AddListener(UpdateSliderUI);
        UpdateSliderUI();
    }

    private void UpdateSliderUI()
    {
        _slider.onValueChanged.RemoveAllListeners();

        _slider.value = SettingsManager.Instance.CurrentPresetIndex;

        if (_presetLabel != null)
        {
            _presetLabel.text = SettingsManager.Instance.GetDisplayText(SettingsManager.SettingType.QualityPreset);
        }

        _slider.onValueChanged.AddListener(SettingsManager.Instance.SetQualityPresetFromSlider);
    }

    private void OnDestroy()
    {
        if(SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnSettingsChanged.RemoveListener(UpdateSliderUI);
        }
    }
}
