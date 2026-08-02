using UnityEngine;
using UnityEngine.Rendering;

public class Ingredient : MonoBehaviour
{
    protected Rigidbody rb;
    [SerializeField] protected GameObject _conversionPrefab;
    protected HintManager _hud;
    protected PlayerKB_GrabObject _grabScript;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _hud = GameObject.Find("HUD").GetComponent<HintManager>();
        _grabScript = GetComponent<PlayerKB_GrabObject>();
    }

    protected virtual void Start() { }

    protected virtual void Update() { }

    protected virtual void OnTriggerEnter(Collider collider) { }

    protected virtual void OnTriggerExit(Collider collider) { }

    protected virtual void SpawnConversion()
    {
        if (_grabScript == null) { return; }
        if (_grabScript.GetIsGrabbed() != 0) { return; }

        if (_conversionPrefab != null)
        {
            GameObject newItem = Instantiate(_conversionPrefab, transform.position, Quaternion.identity);
            newItem.name = _conversionPrefab.name; // don't want it to be '{name}(clone)'
        }

        PlayerKB_GrabObject grabScript = gameObject.GetComponent<PlayerKB_GrabObject>();
        if (grabScript != null) { grabScript.IsActive = false; }
        Destroy(gameObject);
    }
}
