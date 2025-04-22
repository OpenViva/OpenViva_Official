#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct LobbyEnter
    {
        public LobbyEnter_t data;
        public LobbyData Lobby => data.m_ulSteamIDLobby;
        public EChatRoomEnterResponse Response => (EChatRoomEnterResponse)data.m_EChatRoomEnterResponse;
        public bool Locked => data.m_bLocked;

        public static implicit operator LobbyEnter(LobbyEnter_t native) => new LobbyEnter { data = native };
        public static implicit operator LobbyEnter_t(LobbyEnter heathen) => heathen.data;
    }
}
#endif