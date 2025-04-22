#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Used in the <see cref="API.Clans.Client.eventGameConnectedChatLeave"/> event to carry leave data for a specific user
    /// </summary>
    [Serializable]
    public struct UserLeaveData
    {
        /// <summary>
        /// The room this message relates to
        /// </summary>
        /// <remarks>
        /// The room.id will always be populated however under some conditions it is possible to receive a clan chat room message from a room the internal system is not aware of.
        /// In that event the clan.id will be invalid and the room.enterResponse will be Failed
        /// </remarks>
        public ChatRoom room;
        /// <summary>
        /// The user that left
        /// </summary>
        public UserData user;
        /// <summary>
        /// Was the user kicked out (rare)
        /// </summary>
        public bool kicked;
        /// <summary>
        /// Did the user's connection drop
        /// </summary>
        public bool dropped;
    }
}
#endif