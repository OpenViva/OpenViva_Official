#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class LobbyDataUpdateEvent : UnityEvent<LobbyDataUpdateEventData> { }
}
#endif