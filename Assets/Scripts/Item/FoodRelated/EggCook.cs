using UnityEngine;

public class EggCook : Ingredient
{
    [SerializeField] private float _cookTimer = 30f;
    private bool _isCooking = false;

    protected override void Update()
    {
        base.Update();
        if (_isCooking) { _cookTimer -= Time.deltaTime; }
        if (_cookTimer < 0) { SpawnConversion(); }
    }

    protected override void OnTriggerEnter(Collider collider)
    {
        base.OnTriggerEnter(collider);
        if (collider.gameObject.name.Contains("Oven"))
        {
            _isCooking = true;
        }
    }

    protected override void OnTriggerExit(Collider collider)
    {
        base.OnTriggerExit(collider);
        if (collider.gameObject.name.Contains("Oven"))
        {
            _isCooking = false;
        }
    }

    public float GetCookTimer() { return _cookTimer; }
}
