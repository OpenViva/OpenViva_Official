using UnityEngine;

public class InventoryVR : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameObject @object = other.gameObject;
        if (@object.CompareTag("Item") && other.GetComponent<Rigidbody>().isKinematic)
        {
            PlaceInBag(@object);
        }

        if (@object.name == "Controller_BaseLeft")
        {
            PrepareToRemove(0);
        }
        if (@object.name == "Controller_BaseRight")
        {
            PrepareToRemove(1);
        }
    }

    private void PlaceInBag(GameObject item)
    {

    }

    private void PrepareToRemove(int handedness)
    {

    }

}
