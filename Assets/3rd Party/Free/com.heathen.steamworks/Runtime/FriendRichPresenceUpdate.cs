#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct FriendRichPresenceUpdate
    {
        public FriendRichPresenceUpdate_t data;
        public readonly UserData Friend => data.m_steamIDFriend;
        public readonly AppData App => data.m_nAppID;

        public static implicit operator FriendRichPresenceUpdate(FriendRichPresenceUpdate_t native) => new() { data = native };
        public static implicit operator FriendRichPresenceUpdate_t(FriendRichPresenceUpdate heathen) => heathen.data;
    }
}
#endif