using UnityEngine;

public class Ingredient : MonoBehaviour
{
    protected Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void Update() { }
}
