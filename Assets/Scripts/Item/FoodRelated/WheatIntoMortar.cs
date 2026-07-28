using UnityEngine;

public class WheatIntoMortar : Ingredient
{
    private bool _didOnce = false; // DO NOT REMOVE.

    protected override void OnTriggerEnter(Collider collider)
    {
        base.OnTriggerEnter(collider);

        if (rb.isKinematic || _didOnce) { return; }
        if (collider.TryGetComponent(out MortarLogic mortar))
        {
            if ((mortar.WheatQuantity + mortar.FlourQuantity) >= 250) { return; }
            mortar.AddWheat();
            SpawnConversion();
            _didOnce = true; // For some reason, this block runs twice even after object is destroyed. This line fixes that.
        }
    }
}
