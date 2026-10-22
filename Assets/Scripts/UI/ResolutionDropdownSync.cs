using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionDropdownSync : MonoBehaviour
{
    [Tooltip("Parent panel to show only in Fullscreen. Leave empty to always show")]
    [SerializeField] private GameObject dropdownRoot;

    private TMP_Dropdown dropdown;
    private SettingsManager settings;
    private bool optionsBuilt;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    private void OnEnable()
    {
        settings = SettingsManager.Instance;
        settings.OnSettingsChanged.AddListener(Refresh);

        if (!optionsBuilt)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string>(settings.ResolutionOptions));
            optionsBuilt = true;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (settings != null)
            settings.OnSettingsChanged.RemoveListener(Refresh);
    }

    private void OnDestroy()
    {
        dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    private void OnDropdownChanged(int index)
    {
        settings.SetResolutionIndex(index);
    }

    private void Refresh()
    {
        dropdown.SetValueWithoutNotify(settings.CurrentResolutionIndex);

        if (dropdownRoot != null)
        {
            dropdownRoot.SetActive(settings.IsFullscreen);
        }
    }
}
