using UnityEngine;

namespace Viva.Interaction
{
    public enum InteractionType
    {
        Grab,
        Interact,
        Use
    }

    public interface IItemInteractable
    {
        InteractionType CurrentType { get; }

        bool ExecuteAction(GameObject interactor, HandSide hand, Transform handTransform);

        void OnDropped(HandSide hand);
    }
}