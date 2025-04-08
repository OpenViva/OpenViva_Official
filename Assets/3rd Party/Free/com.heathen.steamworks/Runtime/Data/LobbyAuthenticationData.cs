#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Used with the <see cref="LobbyData.Authenticate(LobbyAuthenticationData)"/> feature to authenticate a lobby member with the host of the lobby optionally validating inventory ownership as well.
    /// </summary>
    [Serializable]
    public struct LobbyAuthenticationData
    {
        /// <summary>
        /// Who the ticket is generated for
        /// </summary>
        public ulong to;
        /// <summary>
        /// The ticekt data its self
        /// </summary>
        public byte[] ticket;
        /// <summary>
        /// Optional serialized inventory results, this can be used to enable the host for exmaple to validate this user owns this inventory
        /// </summary>
        public byte[] inventory;
    }
}
#endif