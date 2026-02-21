using UnityEngine;

/// <summary>
/// Interface for any world object that can be interacted with via the E key (desktop)
/// or trigger button (VR). Doors, buttons, NPCs, containers, etc.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Called when the player interacts with this object.
    /// </summary>
    /// <param name="interactor">The GameObject performing the interaction (typically the player)</param>
    void Interact(GameObject interactor);

    /// <summary>
    /// Whether this object can currently be interacted with.
    /// </summary>
    bool CanInteract { get; }
}
