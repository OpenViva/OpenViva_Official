#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Unity Event for <see cref="ActiveBeaconsUpdated_t"/> data
    /// </summary>
    [System.Serializable]
    public class ActiveBeaconsUpdatedEvent : UnityEvent<ActiveBeaconsUpdated_t> { }
}
#endif