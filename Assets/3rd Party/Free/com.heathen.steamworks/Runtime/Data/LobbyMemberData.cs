#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a member of a Steam lobby
    /// </summary>
    [Serializable]
    public struct LobbyMemberData: IEquatable<LobbyMemberData>
    {
        /// <summary>
        /// The lobby the user is a member of
        /// </summary>
        public LobbyData lobby;
        /// <summary>
        /// The user that is a member of the lobby
        /// </summary>
        public UserData user;
        /// <summary>
        /// Return the user's metadata from this lobby
        /// </summary>
        /// <param name="metadataKey">The metadata field key</param>
        /// <returns>The metadata field value</returns>
        public readonly string this[string metadataKey]
        {
            get
            {
                return API.Matchmaking.Client.GetLobbyMemberData(lobby, user, metadataKey);
            }
            set
            {
                if (user == API.User.Client.Id)
                    API.Matchmaking.Client.SetLobbyMemberData(lobby, metadataKey, value);
            }
        }
        /// <summary>
        /// Has this user indicated they are ready to play
        /// </summary>
        public readonly bool IsReady
        {
            get => this[LobbyData.DataReady] == "true";
            set => this[LobbyData.DataReady] = value.ToString().ToLower();
        }
        /// <summary>
        /// The version this user has indicated they are ruining the game as
        /// </summary>
        public readonly string GameVersion
        {
            get => this[LobbyData.DataVersion];
            set => this[LobbyData.DataVersion] = value;
        }
        /// <summary>
        /// Is this member the owner of the lobby
        /// </summary>
        public readonly bool IsOwner => lobby.Owner.Equals(this);

        public readonly bool Equals(LobbyMemberData other)
        {
            return other.lobby == lobby && other.user == user;
        }

        public readonly void Kick() => lobby.KickMember(user);

        public static LobbyMemberData Get(LobbyData lobby, UserData user) => new LobbyMemberData() { lobby = lobby, user = user };
    }
}
#endif