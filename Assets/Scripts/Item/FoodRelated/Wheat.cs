using UnityEngine;

public class Wheat : Ingredient
{
    private bool _didOnce = false; // DO NOT REMOVE.

    private void OnTriggerEnter(Collider collider)
    {
        if (rb.isKinematic || _didOnce) { return; }
        if (collider.TryGetComponent(out Mortar mortar))
        {
            if (mortar.WheatQuantity >= 300) { return; }
            mortar.AddWheat();
            Destroy(gameObject);
            _didOnce = true;
        }
    }
}
