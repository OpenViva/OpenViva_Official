using UnityEngine;

public class DoughBake : Ingredient
{
    [SerializeField] private float _bakeTimer = 60;
    private bool _inOven = false;

    protected override void Update()
    {
        base.Update();
        if (_inOven) { _bakeTimer -= Time.deltaTime; }
        if (_bakeTimer < 0 && _inOven) 
        { 
            OvenSFXController.Instance.PlayBurnSFX();
            SpawnConversion(); 
        }
    }

    protected override void OnTriggerEnter(Collider collider)
    {
        base.OnTriggerEnter(collider);
        if (collider.gameObject.name.Contains("Oven"))
        {
            _inOven = true;
        }
    }

    protected override void OnTriggerExit(Collider collider)
    {
        base.OnTriggerExit(collider);
        if (collider.gameObject.name.Contains("Oven"))
        {
            _inOven = false;
        }
    }
}
