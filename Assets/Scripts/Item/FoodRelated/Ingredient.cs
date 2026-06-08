using UnityEngine;

public class Ingredient : MonoBehaviour
{
    protected Rigidbody rb;
    [SerializeField] protected GameObject _conversionPrefab;
    protected HintManager _hud;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _hud = GameObject.Find("HUD").GetComponent<HintManager>();
    }

    protected virtual void Start() { }

    protected virtual void Update() { }

    protected virtual void OnTriggerEnter(Collider collider) { }

    protected virtual void OnTriggerExit(Collider collider) { }

    protected virtual void SpawnConversion()
    {
        if (_conversionPrefab != null)
        {
            GameObject newItem = Instantiate(_conversionPrefab, transform.position, Quaternion.identity);
            newItem.name = _conversionPrefab.name; // don't want it to be '{name}(clone)'
        }
        Destroy(gameObject);
    }
}
