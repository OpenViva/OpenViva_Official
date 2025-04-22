#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [System.Serializable]
    public class GameConnectedChatJoinEvent : UnityEvent<ChatRoom, UserData> { }
}
#endif