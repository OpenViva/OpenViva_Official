using UnityEngine;

public class Ingredient : MonoBehaviour
{
    protected Rigidbody rb;
    [SerializeField] protected GameObject _conversionPrefab;
    protected HintManager _hud;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        _hud = GameObject.Find("HUD").GetComponent<HintManager>();
    }

    protected virtual void Update() { }

    protected virtual void SpawnConversion()
    {
        if (_conversionPrefab != null)
        {
            Instantiate(_conversionPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
