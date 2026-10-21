using UnityEngine;
using Viva.Interaction;

public class GenericGrab : MonoBehaviour, IItemInteractable
{
    public InteractionType CurrentType { get; private set; } = InteractionType.Grab;

    public bool ExecuteAction(GameObject interactor, HandSide hand, Transform handTransform)
    {
        if (CurrentType == InteractionType.Grab)
        {
            transform.SetParent(handTransform);
            gameObject.layer = LayerMask.NameToLayer("Default");

            CurrentType = InteractionType.Use;
            return true;
        }

        return false;
    }

    public void OnDropped(HandSide hand)
    {
        transform.SetParent(null);
        gameObject.layer = LayerMask.NameToLayer("Interactable");

        CurrentType = InteractionType.Grab;
    }
}
