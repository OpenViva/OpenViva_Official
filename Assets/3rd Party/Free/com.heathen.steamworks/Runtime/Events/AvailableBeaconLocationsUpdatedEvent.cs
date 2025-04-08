#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class AvailableBeaconLocationsUpdatedEvent : UnityEvent<AvailableBeaconLocationsUpdated_t> { }
}
#endif