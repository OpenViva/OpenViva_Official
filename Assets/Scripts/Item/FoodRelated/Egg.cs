using UnityEngine;

public class Egg : Ingredient
{
    private Rigidbody rb;
    private float _speed = 0;
    private float t = 0;
    [SerializeField] private GameObject _uncookedEggPrefab;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        t += Time.deltaTime;
        if (t > (3/60f))
        {
            t = 0;
            if ((rb.linearVelocity.magnitude + 2) < _speed)
            {
                Instantiate(_uncookedEggPrefab, transform.position, transform.rotation);
                Destroy(gameObject);
            }
            else
            {
                _speed = rb.linearVelocity.magnitude;
            }
        }
    }
}
