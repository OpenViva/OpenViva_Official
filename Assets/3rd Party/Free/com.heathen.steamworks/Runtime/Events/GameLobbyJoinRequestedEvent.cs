#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class GameLobbyJoinRequestedEvent : UnityEvent<LobbyData, UserData> { }
}
#endif