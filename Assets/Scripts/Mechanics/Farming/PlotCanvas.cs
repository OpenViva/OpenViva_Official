using System.Collections;
using TMPro;
using UnityEngine;

public class PlotCanvas : MonoBehaviour
{
    [SerializeField] private GameObject _root;

    [Header("Unpurchased State")]
    [SerializeField] private TextMeshProUGUI _purchaseText;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private GameObject _keybindHint;
    [SerializeField] private GameObject _noFundsText;

    [Header("Purchased State")]
    [SerializeField] private GameObject _purchasedText;

    private FarmPlot _parentPlot;
    private float _messageDuration = 2f;
    private bool _purchaseFailed = false;

    private void OnEnable()
    {
        if (_parentPlot != null) AddListeners();
    }

    private void Start()
    {
        _parentPlot = GetComponentInParent<FarmPlot>();
        AddListeners();
    }

    private void AddListeners()
    {
        _parentPlot.OnStateChanged += EnterState;
        _parentPlot.OnPurchaseFailed += PurchaseFailed;
        _parentPlot.OnInRangeChanged += value => SetActive(value);
    }

    private void SetActive(bool setActive)
    {
        _root.SetActive(setActive);
    }

    private void EnterState()
    {
        switch (_parentPlot.CurrentState)
        {
            case FarmPlot.PlotStates.Unpurchased: EnterUnpurchasedState(); break;
            case FarmPlot.PlotStates.Purchased: EnterPurchasedState(); break;
            case FarmPlot.PlotStates.None: break;
        }
    }

    private void EnterUnpurchasedState()
    {
        _purchaseText.gameObject.SetActive(true);
        _priceText.gameObject.SetActive(true);
        _keybindHint.SetActive(true);

        _purchaseText.SetText($"Purchase '{_parentPlot.Name}'?");
        _priceText.SetText($"{_parentPlot.Price} Taill");
    }

    private void PurchaseFailed()
    {
        StartCoroutine(PurchaseFailedCoroutine());
    }

    private void EnterPurchasedState()
    {
        _purchaseText.gameObject.SetActive(false);
        _priceText.gameObject.SetActive(false);
        _keybindHint.SetActive(false);

        _purchasedText.SetActive(true);
    }

    private IEnumerator PurchaseFailedCoroutine()
    {
        if (_purchaseFailed) yield break;

        _purchaseFailed = true;
        _noFundsText.SetActive(true);
        yield return new WaitForSeconds(_messageDuration);
        _noFundsText.SetActive(false);
        _purchaseFailed = false;
    }

    private void RemoveListeners()
    {
        _parentPlot.OnStateChanged -= EnterState;
        _parentPlot.OnPurchaseFailed -= PurchaseFailed;
        _parentPlot.OnInRangeChanged -= value => SetActive(value);
    }

    private void OnDisable()
    {
        RemoveListeners();
    }
}
