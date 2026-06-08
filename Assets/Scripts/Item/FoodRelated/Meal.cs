using UnityEngine;

public class Meal : MonoBehaviour
{
    [SerializeField] private MealData _mealData;

    public float BurnTimer { private set; get; } = 60f;
    private bool _inOven = false;
    private GameObject _burnVariant;

    private void Awake()
    {
        BurnTimer = _mealData.BurnTimer;
        _burnVariant = _mealData.BurnVariant;
    }

    private void Update()
    {
        if (_inOven)
        {
            BurnTimer -= Time.deltaTime;
        }

        if (BurnTimer < 0)
        {
            GameObject newObject = Instantiate(_burnVariant, transform.position, transform.rotation);
            newObject.name = _burnVariant.name;
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
}
