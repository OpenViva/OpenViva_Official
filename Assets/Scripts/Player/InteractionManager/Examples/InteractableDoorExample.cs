using UnityEngine;
using Viva.Interaction;

public class InteractableDoorExample : MonoBehaviour, IItemInteractable
{
    public InteractionType CurrentType => InteractionType.Interact;

    private bool isOpen = false;

    public bool ExecuteAction(GameObject interactor, HandSide hand, Transform handTransform)
    {
        isOpen = !isOpen;
        Debug.Log($"Door is now {(isOpen ? "Open" : "Closed")}. Triggered by {hand} hand.");

        // Add animation or sound logic here
        return true;
    }

    void IItemInteractable.OnDropped(HandSide hand)
    {
    }
}
