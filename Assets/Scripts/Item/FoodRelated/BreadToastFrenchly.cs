using System;
using UnityEngine;

public class BreadToastFrenchly : Ingredient
{
    public float ToastTimer { private set; get; } // Can be negative
    public float EggTimer { private set; get; }
    [SerializeField] private Material _soakedBreadMaterial;
    private bool _inOven = false;

    private bool _soaked = false;
    private event Action OnSoaked;
    private bool Soaked
    {
        get => _soaked;
        set
        {
            if (_soaked == value) return;
            _soaked = value;
            OnSoaked?.Invoke();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        OnSoaked += SoakBread;
    }

    protected override void Update()
    {
        if (_inOven && _soaked)
        {
            ToastTimer -= Time.deltaTime;
            EggTimer -= Time.deltaTime;
        }

        if (ToastTimer < 0 && EggTimer < 0) { SpawnConversion(); }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        GameObject gameObject = other.gameObject;

        if (gameObject.name.Contains("Oven"))
        {
            _inOven = true;
        }

        if (gameObject.TryGetComponent(out EggCook eggScript) && !Soaked)
        {
            Soaked = true;
            EggTimer = eggScript.GetCookTimer();
            Destroy(gameObject);
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        if (other.gameObject.name.Contains("Oven"))
        {
            _inOven = false;
        }
    }

    private void SoakBread()
    {
        if (TryGetComponent(out Renderer r))
        {
            r.material = _soakedBreadMaterial;
        }
        Meal mealScript = GetComponent<Meal>();
        ToastTimer = mealScript.BurnTimer;
        Destroy(mealScript);
    }

    private void OnDestroy()
    {
        OnSoaked -= SoakBread;
    }
}
