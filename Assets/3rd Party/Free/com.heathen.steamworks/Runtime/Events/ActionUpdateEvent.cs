#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Unity Event for <see cref="InputActionUpdate"/> data
    /// </summary>
    [System.Serializable]
    public class ActionUpdateEvent : UnityEvent<InputActionUpdate>
    { }
}
#endif