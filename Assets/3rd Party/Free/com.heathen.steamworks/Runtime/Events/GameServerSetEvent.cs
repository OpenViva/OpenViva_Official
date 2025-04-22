#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class GameServerSetEvent : UnityEvent<LobbyGameServer>
    { }
}
#endif