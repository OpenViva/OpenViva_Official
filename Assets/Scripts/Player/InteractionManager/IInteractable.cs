using UnityEngine;

namespace Viva.Interaction
{
    public enum InteractionType
    {
        Grab,
        Interact,
        Use
    }

    public interface IInteractable
    {
        InteractionType CurrentType { get; }

        bool ExecuteAction(GameObject interactor, HandSide hand, Transform handTransform);
    }
}