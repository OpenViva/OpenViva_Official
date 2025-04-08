#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct FriendGameInfo
    {
        public FriendGameInfo_t data;
        public readonly GameData Game => data.m_gameID;
        public readonly string IpAddress => API.Utilities.IPUintToString(data.m_unGameIP);
        public readonly uint IpInt => data.m_unGameIP;
        public readonly ushort GamePort => data.m_usGamePort;
        public readonly ushort QueryPort => data.m_usQueryPort;
        public readonly LobbyData Lobby => data.m_steamIDLobby;

        public static implicit operator FriendGameInfo(FriendGameInfo_t native) => new() { data = native };
        public static implicit operator FriendGameInfo_t(FriendGameInfo heathen) => heathen.data;
    }
}
#endif