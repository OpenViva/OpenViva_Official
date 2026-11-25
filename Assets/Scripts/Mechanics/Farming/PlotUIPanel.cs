using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlotUIPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _plotNumber;
    [SerializeField] private TextMeshProUGUI _plotName;
    [SerializeField] private TextMeshProUGUI _completionValue;
    [SerializeField] private GameObject _emptyText;
    [SerializeField] private GameObject _seedsText;
    [SerializeField] private TextMeshProUGUI _seedsValue;
    [SerializeField] private GameObject _yieldText;
    [SerializeField] private TextMeshProUGUI _yieldValue;
    [SerializeField] private Slider _irrigationSlider;
    [SerializeField] private TextMeshProUGUI _irrigationValue;
    [SerializeField] private Slider _conditionSlider;
    [SerializeField] private TextMeshProUGUI _conditionValue;

    public void InitializePanel(int number, string name, int yield)
    {
        _completionValue.gameObject.SetActive(false);
        _irrigationSlider.gameObject.SetActive(false);
        _irrigationValue.gameObject.SetActive(false);
        _conditionSlider.gameObject.SetActive(false);
        _conditionValue.gameObject.SetActive(false);

        _plotNumber.SetText($"#{number}");
        _plotName.SetText(name);
        _yieldValue.SetText(yield.ToString());
        _seedsValue.SetText((yield / FarmManager.Instance.CropsPerSeed).ToString());
    }
}
