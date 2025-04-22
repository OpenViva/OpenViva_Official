#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class LobbyGameCreatedEvent : UnityEvent<LobbyGameCreated_t> { }
}
#endif