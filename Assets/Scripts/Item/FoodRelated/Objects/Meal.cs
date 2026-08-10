using UnityEngine;

public class Meal : MonoBehaviour
{
    [SerializeField] private MealData _mealData;

    public float BurnTimer { private set; get; } = 60f;
    private bool _inOven = false;
    private GameObject _burnVariant;
    private float _price;
    private PlayerKB_GrabObject _grabScript;

    private void Awake()
    {
        BurnTimer = _mealData.BurnTimer;
        _burnVariant = _mealData.BurnVariant;
        _price = _mealData.Price;
        _grabScript = GetComponent<PlayerKB_GrabObject>();
    }

    private void Update()
    {
        if (_inOven)
        {
            BurnTimer -= Time.deltaTime;
        }

        if (BurnTimer < 0)
        {
            OvenSFXController.Instance.PlayBurnSFX();

            if (_grabScript == null) { return; }
            if (_grabScript.GetIsGrabbed() != 0) { return; }

            GameObject newObject = Instantiate(_burnVariant, transform.position, transform.rotation);
            newObject.name = _burnVariant.name;

            _grabScript.IsActive = false;
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Oven"))
        {
            _inOven = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Contains("Oven"))
        {
            _inOven = false;
        }
    }

    public void IncreasePrice(float amount) { _price += amount; }
}
