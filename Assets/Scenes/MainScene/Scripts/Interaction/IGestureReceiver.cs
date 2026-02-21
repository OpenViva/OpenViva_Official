using UnityEngine;

/// <summary>
/// Interface for NPCs/characters that respond to player gestures (wave, follow, point).
/// </summary>
public interface IGestureReceiver
{
    /// <summary>
    /// Called when the player performs a gesture nearby.
    /// </summary>
    /// <param name="gestureName">Name of the gesture ("wave", "follow", "point")</param>
    /// <param name="playerTransform">The player's transform for distance/direction checks</param>
    void OnPlayerGesture(string gestureName, Transform playerTransform);
}
