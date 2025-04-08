#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Clan Chat Room aka Group Chat Room
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is returned by the API.Clans.Client.JoinChatRoom and similar methods
    /// </para>
    /// <para>
    /// You should listen on the API.Clans.Client.EventChatMessageReceived to receive clan chat messages.
    /// That event will be raised for any clan you have joined the chat for API.Clans.Client.JoinChatRoom or by calling the Clan.JoinChat member on a clan returned from the API.Clans.Client.GetClans()
    /// </para>
    /// </remarks>
    [Serializable]
    public struct ChatRoom : IEquatable<ChatRoom>
    {
        public ClanData clan;
        public CSteamID id;
        public EChatRoomEnterResponse enterResponse;

        public readonly UserData[] Members => API.Clans.Client.GetChatMembers(clan);
        /// <summary>
        /// Checks if the Steam Group chat room is open in the Steam UI.
        /// </summary>
        public readonly bool IsOpenInSteam => API.Clans.Client.IsClanChatWindowOpenInSteam(id);

        /// <summary>
        /// Sends a message to a Steam group chat room.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendMessage(string message) => SteamFriends.SendClanChatMessage(id, message);
        /// <summary>
        /// Opens this chat in the Steam Overlay
        /// </summary>
        /// <returns></returns>
        public bool OpenChatWindowInSteam() => API.Clans.Client.OpenChatWindowInSteam(id);
        
        public void Leave() => API.Clans.Client.LeaveChatRoom(id);

    #region Boilerplate
        public bool Equals(ChatRoom other)
        {
            return clan == other.clan && id == other.id;
        }
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(ChatRoom))
            {
                var other = (ChatRoom)obj;
                return Equals(other);
            }
            else
                return base.Equals(obj);
        }
        public override int GetHashCode() => clan.GetHashCode() ^ id.GetHashCode();

        public static bool operator ==(ChatRoom l, ChatRoom r) => l.Equals(r);
        public static bool operator !=(ChatRoom l, ChatRoom r) => !l.Equals(r);
    #endregion
    }
}
#endif