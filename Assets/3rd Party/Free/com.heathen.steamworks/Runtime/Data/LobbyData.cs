#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Heathen.SteamworksIntegration.API;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a Steam Lobby and exposes its core functionality
    /// </summary>
    [Serializable]
    public struct LobbyData : IEquatable<CSteamID>, IEquatable<ulong>, IEquatable<LobbyData>
    {
        private ulong id;
        /// <summary>
        /// The native <see cref="CSteamID"/> of the lobby
        /// </summary>
        public readonly CSteamID SteamId => new(id);
        
        /// <summary>
        /// The native <see cref="AccountID_t"/> of the lobby
        /// </summary>
        public readonly AccountID_t AccountId
        {
            get => SteamId.GetAccountID();
        }
        /// <summary>
        /// The primitive <see cref="uint"/> expression of the account id related to the lobby aka the Friend ID
        /// </summary>
        public readonly uint FriendId
        {
            get => AccountId.m_AccountID;
        }
        /// <summary>
        /// Returns the Friend aka Account ID encoded to a Hexadecimal value
        /// </summary>
        public readonly string HexId => FriendId.ToString("X");
        /// <summary>
        /// Is this Lobby value a valid value.
        /// This does not indicate it is a lobby simply that structurally the data is possibly a lobby
        /// </summary>
        public readonly bool IsValid
        {
            get
            {
                var sId = SteamId;
                if (sId == CSteamID.Nil
                    || sId.GetEAccountType() != EAccountType.k_EAccountTypeChat
                    || sId.GetEUniverse() != EUniverse.k_EUniversePublic)
                    return false;
                else
                    return true;
            }
        }
        /// <summary>
        /// Get or set the lobby name
        /// </summary>
        /// <remarks>
        /// <para>
        /// The lobby name is a metadata field whose key is "name". Setting this field will update the lobby metadata accordingly and this update will be reflected to all members.
        /// Only the owner of the lobby can set this value.
        /// </para>
        /// </remarks>
        public readonly string Name
        {
            get => this[DataName];
            set => this[DataName] = value;
        }
        /// <summary>
        /// The current owner of the lobby.
        /// </summary>
        public readonly LobbyMemberData Owner
        {
            get => new LobbyMemberData { lobby = this, user = API.Matchmaking.Client.GetLobbyOwner(id) };
            set => API.Matchmaking.Client.SetLobbyOwner(id, value.user);
        }
        /// <summary>
        /// The member data for this user
        /// </summary>
        public readonly LobbyMemberData Me => new LobbyMemberData { lobby = this, user = API.User.Client.Id };
        /// <summary>
        /// The collection of all members of this lobby including the owner of the lobby.
        /// </summary>
        public readonly LobbyMemberData[] Members => API.Matchmaking.Client.GetLobbyMembers(id);
        /// <summary>
        /// True if the data type metadata is set
        /// </summary>
        public readonly bool IsTypeSet => !string.IsNullOrEmpty(API.Matchmaking.Client.GetLobbyData(id, DataType));
        /// <summary>
        /// Returns the type of the lobby if set, if not set this will default to Private, you can check if the type is set with <see cref="IsTypeSet"/>
        /// </summary>
        public readonly ELobbyType Type
        {
            get
            {
                if (int.TryParse(API.Matchmaking.Client.GetLobbyData(id, DataType), out int enumVal))
                {
                    return (ELobbyType)enumVal;
                }
                else
                    return ELobbyType.k_ELobbyTypePrivate;
            }
            set => API.Matchmaking.Client.SetLobbyType(id, value);
        }
        /// <summary>
        /// Gets or sets the version of the game the lobby is configured for ... this should match the owners version
        /// </summary>
        public readonly string GameVersion
        {
            get => this[DataVersion];
            set => this[DataVersion] = value;
        }
        /// <summary>
        /// Is the user the host of this lobby
        /// </summary>
        /// <remarks>
        /// <para>
        /// Calls <see cref="SteamMatchmaking.GetLobbyOwner(CSteamID)"/> and compares the results to <see cref="SteamUser.GetSteamID()"/>.
        /// This returns true if the provided lobby ID is a legitimate ID and if Valve indicates that the lobby has members and if the owner of the lobby is the current player.
        /// </para>
        /// </remarks>
        public readonly bool IsOwner
        {
            get
            {
                return SteamUser.GetSteamID() == SteamMatchmaking.GetLobbyOwner(this);
            }
        }
        /// <summary>
        /// Indicates rather or not this lobby is a party lobby
        /// </summary>
        public readonly bool IsGroup
        {
            get
            {
                return this[DataMode] == "Group";
            }
            set
            {
                if (IsOwner)
                {
                    if (value)
                    {
                        SetType(ELobbyType.k_ELobbyTypeInvisible);
                        this[DataMode] = "Group";
                    }
                    else
                    {
                        this[DataMode] = "General";
                    }
                }
            }
        }
        /// <summary>
        /// Indicates rather or not this lobby is a party lobby
        /// </summary>
        public readonly bool IsSession
        {
            get
            {
                return this[DataMode] == "Session";
            }
            set
            {
                if (IsOwner)
                {
                    if (value)
                    {
                        this[DataMode] = "Session";
                    }
                    else
                    {
                        this[DataMode] = "General";
                    }
                }
            }
        }
        /// <summary>
        /// Does this lobby have a game server registered to it
        /// </summary>
        /// <remarks>
        /// <para>
        /// Calls <see cref="SteamMatchmaking.GetLobbyGameServer(CSteamID, out uint, out ushort, out CSteamID)"/> and cashes the data to <see cref="GameServer"/>.
        /// It is not usually necessary to check this value since the set game server callback from Steamworks will automatically update these values if the user was connected to the lobby when the set game server data was called.
        /// </para>
        /// </remarks>
        public readonly bool HasServer => SteamMatchmaking.GetLobbyGameServer(this, out _, out _, out _);
        /// <summary>
        /// Returns the <see cref="LobbyGameServer"/> data containing server connection inforation related to the lobby if any
        /// </summary>
        public readonly LobbyGameServer GameServer => API.Matchmaking.Client.GetLobbyGameServer(id);
        /// <summary>
        /// Returns true if all of the players 'IsReady' is true
        /// </summary>
        /// <remarks>
        /// <para>
        /// This can be used to determine if the players are ready to play the game.
        /// </para>
        /// </remarks>
        public readonly bool AllPlayersReady
        {
            get
            {
                //If we have any that are not ready then return false ... else return true
                return !Members.Any(p => !p.IsReady);
            }
        }
        /// <summary>
        /// Returns true if all of the players 'IsReady' is false
        /// </summary>
        /// <remarks>
        /// <para>
        /// This can be used to determine if all players have reset the ready flag such as when some change is made after a previous ready check had already passed.
        /// </para>
        /// </remarks>
        public readonly bool AllPlayersNotReady
        {
            get
            {
                //If we have any that are not ready then return false ... else return true
                return Members.Any(p => p.IsReady) ? false : true;
            }
        }
        /// <summary>
        /// Are all members of the lobby set as ready
        /// </summary>
        public readonly bool IsReady
        {
            get => API.Matchmaking.Client.GetLobbyMemberData(id, API.User.Client.Id, DataReady) == "true";
            set => API.Matchmaking.Client.SetLobbyMemberData(id, DataReady, value.ToString().ToLower());
        }
        /// <summary>
        /// Is the lobby full e.g. the number of members is equal to or greater than the number of slots listed for the lobby
        /// </summary>
        public readonly bool Full => API.Matchmaking.Client.GetLobbyMemberLimit(id) <= SteamMatchmaking.GetNumLobbyMembers(this);
        /// <summary>
        /// The max number of members this lobby can take
        /// </summary>
        public readonly int MaxMembers
        {
            get => API.Matchmaking.Client.GetLobbyMemberLimit(id);
            set => API.Matchmaking.Client.SetLobbyMemberLimit(id, value);
        }
        /// <summary>
        /// The number of members in this lobby
        /// </summary>
        public readonly int MemberCount => Steamworks.SteamMatchmaking.GetNumLobbyMembers(this);
        /// <summary>
        /// Read and write metadata values to the lobby
        /// </summary>
        /// <param name="metadataKey">The key of the value to be read or writen</param>
        /// <returns>The value of the key if any otherwise returns and empty string.</returns>
        public readonly string this[string metadataKey]
        {
            get
            {
                return API.Matchmaking.Client.GetLobbyData(id, metadataKey);
            }
            set
            {
                API.Matchmaking.Client.SetLobbyData(id, metadataKey, value);
            }
        }
        /// <summary>
        /// If this user is a member of this lobby get the related <see cref="LobbyMemberData"/>
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public readonly LobbyMemberData this[UserData user]
        {
            get
            {
                if (GetMember(user, out var member))
                    return member;
                else
                    return default;
            }
        }
        /// <summary>
        /// Get the LobbyMember object for a given user
        /// </summary>
        /// <param name="id">The ID of the member to fetch</param>
        /// <param name="member">The member found</param>
        /// <returns>True if the user is a member of the lobby, false if they are not</returns>
        public readonly bool GetMember(UserData user, out LobbyMemberData member) => API.Matchmaking.Client.GetMember(this, user, out member);
        /// <summary>
        /// Checks if a user is a member of this lobby
        /// </summary>
        /// <param name="id">The user to check for</param>
        /// <returns>True if they are, false if not</returns>
        public readonly bool IsAMember(CSteamID id) => API.Matchmaking.Client.IsAMember(this, id);
        /// <summary>
        /// Updates the lobby type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public readonly bool SetType(ELobbyType type) => API.Matchmaking.Client.SetLobbyType(id, type);
        /// <summary>
        /// Mark the lobby as joinable or not
        /// </summary>
        /// <param name="makeJoinable"></param>
        /// <returns></returns>
        public readonly bool SetJoinable(bool makeJoinable) => API.Matchmaking.Client.SetLobbyJoinable(id, makeJoinable);
        /// <summary>
        /// Gets the dictionary of metadata values assigned to this lobby.
        /// </summary>
        public readonly Dictionary<string, string> GetMetadata()
        {
            Dictionary<string, string> result = new();

            var count = SteamMatchmaking.GetLobbyDataCount(this);

            for (int i = 0; i < count; i++)
            {
                SteamMatchmaking.GetLobbyDataByIndex(this, i, out string key, Constants.k_nMaxLobbyKeyLength, out string value, Constants.k_cubChatMetadataMax);
                result.Add(key, value);
            }

            return result;
        }
        public static void Create(CreateArguments createArguments, Action<EResult, LobbyData, bool> callback)
        {
            Create(createArguments.type, createArguments.slots, (eResult, lobby, ioError) =>
            {
                if (!ioError && eResult == EResult.k_EResultOK)
                {
                    foreach (var metadata in createArguments.metadata)
                    {
                        lobby[metadata.key] = metadata.value;
                    }
                    callback?.Invoke(eResult, lobby, ioError);
                }
            });
        }
        /// <summary>
        /// Create a new lobby with the provided parameters
        /// </summary>
        /// <param name="type">The type of lobby to be created, typically this would be <see cref="ELobbyType.k_ELobbyTypePublic"/>, it should never be <see cref="ELobbyType.k_ELobbyTypePrivateUnique"/></param>
        /// <param name="slots">The number of slots this lobby should have, this indicates the max number of members that can join the lobby including its owner</param>
        /// <param name="callback">A delegate of the form (<see cref="EResult"/> result, <see cref="LobbyData"/> lobby, <see cref="bool"/> ioError) that is invoked when the process completes</param>
        public static void Create(ELobbyType type, int slots, Action<EResult, LobbyData, bool> callback) => API.Matchmaking.Client.CreateLobby(type, slots, callback);
        /// <summary>
        /// Create a new lobby and indicate that it is used as a party.
        /// All party lobbies are of type <see cref="ELobbyType.k_ELobbyTypeInvisible"/>
        /// <para>Steam lobby is not just a networking tool, it can be used for many things including a "party".
        /// A party is a group of friends, you see this used in MOBA, Shooters, MMOs and other "team" based games.
        /// The idea is that friends will join a party and then look for a match that can take the party.
        /// Steam treats the party as a lobby, for more information see the Heathen Knowledge Base on Steam Lobby</para>
        /// </summary>
        /// <param name="slots">The number of slots this party lobby should have</param>
        /// <param name="callback">A delegate of the form (<see cref="EResult"/> result, <see cref="LobbyData"/> lobby, <see cref="bool"/> ioError) that is invoked when the process completes</param>
        public static void CreateParty(int slots, Action<EResult, LobbyData, bool> callback)
        {
            API.Matchmaking.Client.CreateLobby(ELobbyType.k_ELobbyTypeInvisible, slots, (r,l,e) =>
            {
                if(!e && r == EResult.k_EResultOK)
                    l.IsGroup = true;

                callback?.Invoke(r, l, e);
            });
        }
        /// <summary>
        /// Creates a new lobby and indicate that it is used as a session
        /// <para>Steam lobby is not just a networking tool but it can be used for matchmaking a session.
        /// A "session" lobby is a lobby that is used for matchmaking a game session and contrasts with "Party" use lobbies.</para>
        /// </summary>
        /// <param name="type">The type of lobby to be created, typically this would be <see cref="ELobbyType.k_ELobbyTypePublic"/>, it should never be <see cref="ELobbyType.k_ELobbyTypePrivateUnique"/></param>
        /// <param name="slots">The number of slots this lobby should have, this indicates the max number of members that can join the lobby including its owner</param>
        /// <param name="callback">A delegate of the form (<see cref="EResult"/> result, <see cref="LobbyData"/> lobby, <see cref="bool"/> ioError) that is invoked when the process completes</param>
        public static void CreateSession(ELobbyType type, int slots, Action<EResult, LobbyData, bool> callback)
        {
            API.Matchmaking.Client.CreateLobby(type, slots, (r, l, e) =>
            {
                if (!e && r == EResult.k_EResultOK)
                    l.IsSession = true;

                callback?.Invoke(r, l, e);
            });
        }
        /// <summary>
        /// Create a new public lobby and indicate that it is used as a session.
        /// <para>Steam lobby is not just a networking tool but it can be used for matchmaking a session.
        /// A "session" lobby is a lobby that is used for matchmaking a game session and contrasts with "Party" use lobbies.</para>
        /// </summary>
        /// <param name="slots">The number of slots this lobby should have, this indicates the max number of members that can join the lobby including its owner</param>
        /// <param name="callback">A delegate of the form (<see cref="EResult"/> result, <see cref="LobbyData"/> lobby, <see cref="bool"/> ioError) that is invoked when the process completes</param>
        public static void CreatePublicSession(int slots, Action<EResult, LobbyData, bool> callback)
        {
            API.Matchmaking.Client.CreateLobby(ELobbyType.k_ELobbyTypePublic, slots, (r, l, e) =>
            {
                if (!e && r == EResult.k_EResultOK)
                    l.IsSession = true;

                callback?.Invoke(r, l, e);
            });
        }
        /// <summary>
        /// Create a new private lobby and indicate that it is used as a session.
        /// <para>Steam lobby is not just a networking tool but it can be used for matchmaking a session.
        /// A "session" lobby is a lobby that is used for matchmaking a game session and contrasts with "Party" use lobbies.</para>
        /// </summary>
        /// <param name="slots">The number of slots this lobby should have, this indicates the max number of members that can join the lobby including its owner</param>
        /// <param name="callback">A delegate of the form (<see cref="EResult"/> result, <see cref="LobbyData"/> lobby, <see cref="bool"/> ioError) that is invoked when the process completes</param>
        public static void CreatePrivateSession(int slots, Action<EResult, LobbyData, bool> callback)
        {
            API.Matchmaking.Client.CreateLobby(ELobbyType.k_ELobbyTypePrivate, slots, (r, l, e) =>
            {
                if (!e && r == EResult.k_EResultOK)
                    l.IsSession = true;

                callback?.Invoke(r, l, e);
            });
        }
        /// <summary>
        /// Create a new friend only lobby and indicate that it is used as a session.
        /// <para>Steam lobby is not just a networking tool but it can be used for matchmaking a session.
        /// A "session" lobby is a lobby that is used for matchmaking a game session and contrasts with "Party" use lobbies.</para>
        /// </summary>
        /// <param name="slots">The number of slots this lobby should have, this indicates the max number of members that can join the lobby including its owner</param>
        /// <param name="callback">A delegate of the form (<see cref="EResult"/> result, <see cref="LobbyData"/> lobby, <see cref="bool"/> ioError) that is invoked when the process completes</param>
        public static void CreateFriendOnlySession(int slots, Action<EResult, LobbyData, bool> callback)
        {
            API.Matchmaking.Client.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, slots, (r, l, e) =>
            {
                if (!e && r == EResult.k_EResultOK)
                    l.IsSession = true;

                callback?.Invoke(r, l, e);
            });
        }
        /// <summary>
        /// Join this lobby
        /// </summary>
        /// <param name="callback">A delegate of the form (<see cref="LobbyEnter"/> result, <see cref="bool"/> ioError) that will be invoked when the process completes</param>
        public readonly void Join(Action<LobbyEnter, bool> callback)
        {
            API.Matchmaking.Client.JoinLobby(this, callback);
        }
        /// <summary>
        /// Leaves the current lobby if any
        /// </summary>
        public readonly void Leave()
        {
            if (SteamId == CSteamID.Nil)
                return;

            API.Matchmaking.Client.LeaveLobby(this);
        }
        /// <summary>
        /// Clear the indicated lobby data field from the current metadata
        /// </summary>
        /// <param name="dataKey">The metadata field to be cleared</param>
        /// <returns>True if request was accepted</returns>
        public readonly bool DeleteLobbyData(string dataKey) => API.Matchmaking.Client.DeleteLobbyData(id, dataKey);
        /// <summary>
        /// Invite the indicated user to the lobby
        /// </summary>
        /// <param name="targetUser">The user to be invited</param>
        /// <returns>True if the request was accepted</returns>
        public readonly bool InviteUserToLobby(UserData targetUser) => API.Matchmaking.Client.InviteUserToLobby(id, targetUser);
        /// <summary>
        /// Send a chat message as a <see cref="string"/> to all members of the lobby
        /// </summary>
        /// <param name="message">The message to be sent</param>
        /// <returns>True if the request was accepted</returns>
        public readonly bool SendChatMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return false;

            byte[] MsgBody = System.Text.Encoding.UTF8.GetBytes(message);
            return SteamMatchmaking.SendLobbyChatMsg(this, MsgBody, MsgBody.Length);
        }
        /// <summary>
        /// Send a chat message as <see cref="byte"/>[] data to all members
        /// </summary>
        /// <param name="data">The data to be sent</param>
        /// <returns>True if the request was accepted</returns>
        public readonly bool SendChatMessage(byte[] data)
        {
            if (data == null || data.Length < 1)
                return false;

            return SteamMatchmaking.SendLobbyChatMsg(this, data, data.Length);
        }
        /// <summary>
        /// Send a chat message as a serialized object
        /// </summary>
        /// <param name="jsonObject">This can be any object that is serializable. The system will use <see cref="JsonUtility"/> to serialize the object and encode it as a UTF8 byte array</param>
        /// <returns>True if the request was accepted</returns>
        public readonly bool SendChatMessage(object jsonObject)
        {
            return SendChatMessage(System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(jsonObject)));
        }
        /// <summary>
        /// <para>
        /// Sets the game server associated with the lobby.
        /// This can only be set by the owner of the lobby.
        /// Either the IP/Port or the Steamworks ID of the game server must be valid, depending on how you want the clients to be able to connect.
        /// A LobbyGameCreated_t callback will be sent to all players in the lobby, usually at this point, the users will join the specified game server.
        /// </para>
        /// </summary>
        public readonly void SetGameServer(string address, ushort port, CSteamID gameServerId)
        {
            API.Matchmaking.Client.SetLobbyGameServer(id, address, port, gameServerId);
        }
        /// <summary>
        /// <para>
        /// Sets the game server associated with the lobby.
        /// This can only be set by the owner of the lobby.
        /// Either the IP/Port or the Steamworks ID of the game server must be valid, depending on how you want the clients to be able to connect.
        /// A LobbyGameCreated_t callback will be sent to all players in the lobby, usually at this point, the users will join the specified game server.
        /// </para>
        /// </summary>
        public readonly void SetGameServer(string address, ushort port)
        {
            API.Matchmaking.Client.SetLobbyGameServer(id, address, port, CSteamID.Nil);
        }
        /// <summary>
        /// <para>
        /// Sets the game server associated with the lobby.
        /// This can only be set by the owner of the lobby.
        /// Either the IP/Port or the Steamworks ID of the game server must be valid, depending on how you want the clients to be able to connect.
        /// A LobbyGameCreated_t callback will be sent to all players in the lobby, usually at this point, the users will join the specified game server.
        /// </para>
        /// </summary>
        public readonly void SetGameServer(CSteamID gameServerId)
        {
            API.Matchmaking.Client.SetLobbyGameServer(id, 0, 0, gameServerId);
        }
        /// <summary>
        /// <para>
        /// This overload uses the lobby owner's CSteamID as the server ID which is typical of P2P session.
        /// </para>
        /// <para>
        /// Sets the game server associated with the lobby.
        /// This can only be set by the owner of the lobby.
        /// Either the IP/Port or the Steamworks ID of the game server must be valid, depending on how you want the clients to be able to connect.
        /// A LobbyGameCreated_t callback will be sent to all players in the lobby, usually at this point, the users will join the specified game server.
        /// </para>
        /// </summary>
        public readonly void SetGameServer()
        {
            API.Matchmaking.Client.SetLobbyGameServer(id, 0, 0, API.User.Client.Id);
        }
        /// <summary>
        /// Marks the user to be removed
        /// </summary>
        /// <param name="memberId"></param>
        /// <remarks>
        /// This creates an entry in the metadata named z_heathenKick which contains a string array of Ids of users that should leave the lobby.
        /// When users detect their ID in the string they will automatically leave the lobby on leaving the lobby the users ID will be removed from the array.
        /// </remarks>
        public readonly bool KickMember(UserData memberId)
        {
            if (!IsOwner)
                return false;

            var kickList = API.Matchmaking.Client.GetLobbyData(id, DataKick);

            if (kickList == null)
                kickList = string.Empty;

            if (!kickList.Contains("[" + memberId.ToString() + "]"))
                kickList += "[" + memberId.ToString() + "]";

            return API.Matchmaking.Client.SetLobbyData(id, DataKick, kickList);
        }
        /// <summary>
        /// Check if the indicated user is contained in the list of members that should leave the lobby
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public readonly bool KickListContains(UserData memberId)
        {
            var kickList = API.Matchmaking.Client.GetLobbyData(id, DataKick);
            return kickList.Contains("[" + memberId.ToString() + "]");
        }
        /// <summary>
        /// Removes the indicated member from the kick list
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public readonly bool RemoveFromKickList(UserData memberId)
        {
            if (!IsOwner)
                return false;

            var kickList = API.Matchmaking.Client.GetLobbyData(id, DataKick);

            kickList = kickList.Replace("[" + memberId.ToString() + "]", string.Empty);

            return API.Matchmaking.Client.SetLobbyData(id, DataKick, kickList);
        }
        /// <summary>
        /// Clears the kick list
        /// </summary>
        /// <returns></returns>
        public readonly bool ClearKickList()
        {
            if (!IsOwner)
                return false;

            return API.Matchmaking.Client.DeleteLobbyData(id, DataKick);
        }
        /// <summary>
        /// Use this sparingly it requires string parsing and is not performant
        /// </summary>
        /// <returns></returns>
        public readonly UserData[] GetKickList()
        {
            var list = API.Matchmaking.Client.GetLobbyData(id, DataKick);
            if (!string.IsNullOrEmpty(list))
            {
                var sArray = list.Split(new string[] { "][" }, StringSplitOptions.RemoveEmptyEntries);
                var resultList = new List<UserData>();
                for (int i = 0; i < sArray.Length; i++)
                {
                    var user = UserData.Get(sArray[i].Replace("[", string.Empty).Replace("]", string.Empty));
                    if (user.IsValid)
                        resultList.Add(user);
                }

                return resultList.ToArray();
            }
            else
                return new UserData[0];
        }
        /// <summary>
        /// Sets metadata for the player on the first lobby
        /// </summary>
        /// <param name="key">The key of the metadata to set</param>
        /// <param name="value">The value of the metadata to set</param>
        public readonly void SetMemberMetadata(string key, string value)
        {
            API.Matchmaking.Client.SetLobbyMemberData(id, key, value);
        }
        /// <summary>
        /// Returns the metadata field of the local user
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public readonly string GetMemberMetadata(string key)
        {
            return API.Matchmaking.Client.GetLobbyMemberData(id, API.User.Client.Id, key);
        }
        /// <summary>
        /// Returns the metadata field of the user indicated by <paramref name="memberId"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public readonly string GetMemberMetadata(UserData memberId, string key)
        {
            return API.Matchmaking.Client.GetLobbyMemberData(id, memberId, key);
        }
        /// <summary>
        /// Returns the metadata field of the user indicated by <paramref name="member"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public readonly string GetMemberMetadata(LobbyMemberData member, string key)
        {
            return API.Matchmaking.Client.GetLobbyMemberData(id, member.user, key);
        }
        /// <summary>
        /// Refreshes all of the metadata for a lobby that you're not in right now.
        /// </summary>
        /// <remarks>
        /// You will never do this for lobbies you're a member of, that data will always be up to date. You can use this to refresh lobbies that you have obtained from RequestLobbyList or that are available via friends.
        /// </remarks>
        /// <returns>True if request was accepted, False indicates an failure to issue the request.</returns>
        public readonly bool RequestData() => API.Matchmaking.Client.RequestLobbyData(id);
        /// <summary>
        /// Gets a lobby based on a provided account aka friend ID
        /// </summary>
        /// <param name="accountId">The ID of the lobby as a string to return</param>
        /// <returns>The related lobby</returns>
        public static LobbyData Get(string accountId)
        {
            uint id = Convert.ToUInt32(accountId, 16);
            if (id > 0)
                return Get(id);
            else
                return CSteamID.Nil;
        }
        /// <summary>
        /// Get the lobby represented by this account ID
        /// </summary>
        /// <param name="accountId">The account ID of the lobby to return</param>
        /// <returns>The related lobby</returns>
        public static LobbyData Get(uint accountId) => new CSteamID(new AccountID_t(accountId), 393216, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeChat);
        /// <summary>
        /// Get the lobby represented by this account ID
        /// </summary>
        /// <param name="accountId">The account ID of the lobby to return</param>
        /// <returns>The related lobby</returns>
        public static LobbyData Get(AccountID_t accountId) => new CSteamID(accountId, 393216, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeChat);
        /// <summary>
        /// Get the lobby represented by this CSteamID value
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static LobbyData Get(ulong id) => new LobbyData { id = id };
        /// <summary>
        /// Get the lobby represented by this CSteamID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static LobbyData Get(CSteamID id) => new LobbyData { id = id.m_SteamID };
        /// <summary>
        /// Returns the first lobby where lobby.IsGroup was set to true
        /// </summary>
        /// <param name="lobby"></param>
        /// <returns></returns>
        public static bool GroupLobby(out LobbyData lobby)
        {
            lobby = API.Matchmaking.Client.memberOfLobbies.FirstOrDefault(p => p.IsGroup);
            return lobby.IsValid;
        }
        /// <summary>
        /// Returns the first lobby where lobby.IsSession was set to true
        /// </summary>
        /// <param name="lobby"></param>
        /// <returns></returns>
        public static bool SessionLobby(out LobbyData lobby)
        {
            lobby = API.Matchmaking.Client.memberOfLobbies.FirstOrDefault(p => p.IsSession);
            return lobby.IsValid;
        }
        /// <summary>
        /// Join the lobby represented by this account Id
        /// </summary>
        /// <param name="accountId">Must be a valid uint as a string, this can be the Hex ID value as System.Convert.ToUInt can handle hex among other expressions</param>
        /// <param name="callback">Invoked when the process is complete, handler(LobbyEnter_t result, bool IOError)</param>
        public static void Join(string accountId, Action<LobbyEnter, bool> callback) => API.Matchmaking.Client.JoinLobby(Get(accountId), callback);
        /// <summary>
        /// Join the lobby
        /// </summary>
        /// <param name="lobby">The lobby to join</param>
        /// <param name="callback">Invoked when the process is complete, handler(LobbyEnter_t result, bool IOError)</param>
        public static void Join(LobbyData lobby, Action<LobbyEnter, bool> callback) => API.Matchmaking.Client.JoinLobby(lobby, callback);
        /// <summary>
        /// Join the lobby represented by this account Id
        /// </summary>
        /// <param name="accountId">Must be a valid uint as a string</param>
        /// <param name="callback">Invoked when the process is complete, handler(LobbyEnter_t result, bool IOError)</param>
        public static void Join(AccountID_t accountId, Action<LobbyEnter, bool> callback) => API.Matchmaking.Client.JoinLobby(Get(accountId), callback);
        /// <summary>
        /// Request a list of lobbies that matches the input filters
        /// </summary>
        /// <param name="distanceFilter">The Valve distance category to check within</param>
        /// <param name="openSlotsRequired">The number of open slots required ... typically 1 for a singple player but if your player is in a party this should be the size of the party including the local player</param>
        /// <param name="maxResultsToReturn">The maximum number of results to return ... this will not return more than 50 no matter what you enter here. Ideally you would return 1 and join it e.g. "quick match"</param>
        /// <param name="stringFilters">A set of string filters to compare against. Steam is smart (sorta) and when you use a numeric type comparison it will try to cast values to numbers</param>
        /// <param name="nearFilters">A set of "near" filters, these do not reduce the results they are simply used to sort the results</param>
        /// <param name="numericFilters">A set of numeric filters these are always treated as whole numbers</param>
        /// <param name="callback">A delegate of the form (<see cref="LobbyData"/>[] results, <see cref="bool"/> ioError) that will be invoked when the process completes.</param>
        public static void Request(ELobbyDistanceFilter distanceFilter, int openSlotsRequired, int maxResultsToReturn, IEnumerable<StringFilter> stringFilters, IEnumerable<NearFilter> nearFilters, IEnumerable<NumericFilter> numericFilters, Action<LobbyData[], bool> callback)
        {
            API.Matchmaking.Client.AddRequestLobbyListDistanceFilter(distanceFilter);
            API.Matchmaking.Client.AddRequestLobbyListFilterSlotsAvailable(openSlotsRequired);
            if (stringFilters != null && stringFilters.Count() > 0)
            {
                foreach (var filter in stringFilters)
                    API.Matchmaking.Client.AddRequestLobbyListStringFilter(filter.key, filter.value, filter.comparison);
            }
            if (nearFilters != null && nearFilters.Count() > 0)
            {
                foreach (var filter in nearFilters)
                    API.Matchmaking.Client.AddRequestLobbyListNearValueFilter(filter.key, filter.value);
            }
            if (numericFilters != null && numericFilters.Count() > 0)
            {
                foreach (var filter in numericFilters)
                    API.Matchmaking.Client.AddRequestLobbyListNumericalFilter(filter.key, filter.value, filter.comparison);
            }
            API.Matchmaking.Client.AddRequestLobbyListResultCountFilter(maxResultsToReturn);
            API.Matchmaking.Client.RequestLobbyList(callback);
        }
        public static void Request(SearchArguments searchArguments, int maxResultsToReturn, Action<LobbyData[], bool> callback)
        {
            API.Matchmaking.Client.AddRequestLobbyListDistanceFilter(searchArguments.distance);
            API.Matchmaking.Client.AddRequestLobbyListFilterSlotsAvailable(searchArguments.slots);
            if (searchArguments.stringFilters != null && searchArguments.stringFilters.Count() > 0)
            {
                foreach (var filter in searchArguments.stringFilters)
                    API.Matchmaking.Client.AddRequestLobbyListStringFilter(filter.key, filter.value, filter.comparison);
            }
            if (searchArguments.nearValues != null && searchArguments.nearValues.Count() > 0)
            {
                foreach (var filter in searchArguments.nearValues)
                    API.Matchmaking.Client.AddRequestLobbyListNearValueFilter(filter.key, filter.value);
            }
            if (searchArguments.numericFilters != null && searchArguments.numericFilters.Count() > 0)
            {
                foreach (var filter in searchArguments.numericFilters)
                    API.Matchmaking.Client.AddRequestLobbyListNumericalFilter(filter.key, filter.value, filter.comparison);
            }
            API.Matchmaking.Client.AddRequestLobbyListResultCountFilter(maxResultsToReturn);
            API.Matchmaking.Client.RequestLobbyList(callback);
        }
        /// <summary>
        /// Searches for a lobby that matches the arguments in <paramref name="searchArguments"/> if a lobby is found it will be joined. 
        /// If no lobby is found during the search a new lobby will be created using the <paramref name="createArguments"/>.
        /// </summary>
        /// <param name="searchArguments">Paramiters to be used when searching for a matching lobby</param>
        /// <param name="createArguments">Paramiters to be used when creating a new lobby</param>
        /// <param name="callback">A deligate of the form (<see cref="EResult"/> eResult, <see cref="LobbyData"/> lobby, <see cref="bool"/> ioError) this will be invoked when the process compeltes.</param>
        public static void QuickMatch(SearchArguments searchArguments, CreateArguments createArguments, Action<EResult, LobbyData, bool> callback)
        {
            Request(searchArguments, 1, (results, error) =>
            {
                if(error)
                {
                    callback?.Invoke(EResult.k_EResultIOFailure, default, error);
                }
                else
                {
                    if(results == null
                    || results.Length < 1)
                    {
                        CreateSession(createArguments.type, createArguments.slots, (eResult, lobby, ioError) =>
                        {
                            if(!ioError && eResult == EResult.k_EResultOK)
                            {
                                foreach(var metadata in createArguments.metadata)
                                {
                                    lobby[metadata.key] = metadata.value;
                                }

                                callback?.Invoke(eResult, lobby, ioError);
                            }
                        });
                    }
                    else
                    {
                        results[0].Join((lEnter, error) =>
                        {
                            if (error)
                                callback?.Invoke(EResult.k_EResultIOFailure, default, error);
                            else
                            {
                                switch(lEnter.Response)
                                {
                                    case EChatRoomEnterResponse.k_EChatRoomEnterResponseBanned:
                                    case EChatRoomEnterResponse.k_EChatRoomEnterResponseCommunityBan:
                                    case EChatRoomEnterResponse.k_EChatRoomEnterResponseMemberBlockedYou:
                                    case EChatRoomEnterResponse.k_EChatRoomEnterResponseYouBlockedMember:
                                        callback?.Invoke(EResult.k_EResultBanned, lEnter.Lobby, error);
                                        break;
                                    case EChatRoomEnterResponse.k_EChatRoomEnterResponseFull:
                                        callback?.Invoke(EResult.k_EResultLimitExceeded, lEnter.Lobby, error);
                                        break;
                                    case EChatRoomEnterResponse.k_EChatRoomEnterResponseLimited:
                                        callback?.Invoke(EResult.k_EResultLimitedUserAccount, lEnter.Lobby, error);
                                        break;
                                    case EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess:
                                        callback?.Invoke(EResult.k_EResultOK, lEnter.Lobby, error);
                                        break;
                                    case EChatRoomEnterResponse.k_EChatRoomEnterResponseRatelimitExceeded:
                                        callback?.Invoke(EResult.k_EResultRateLimitExceeded, lEnter.Lobby, error);
                                        break;
                                    default:
                                        callback?.Invoke(EResult.k_EResultFail, lEnter.Lobby, error);
                                        break;
                                }
                                
                            }
                        });
                    }
                }
            });
        }
        /// <summary>
        /// Attempts to send a newly generated ticket to the lobby for authentication
        /// </summary>
        /// <param name="callback">(AuthenticationTicket ticketSent, bool ioError) invoked when the process completes and indicates the ticket that was sent and rather or not there was an IO error.</param>
        public readonly void Authenticate(Action<AuthenticationTicket, bool> callback)
        {
            var owningUser = Owner.user;
            var thisLobby = this;
            Authentication.GetAuthSessionTicket(owningUser, (ticket, ioError) =>
            {
                thisLobby.Authenticate(new LobbyAuthenticationData()
                {
                    to = owningUser,
                    ticket = ticket.Data,
                    inventory = null
                });

                callback.Invoke(ticket, ioError);
            });
        }
        /// <summary>
        /// Sends a prebuilt authentication data request to the lobby chat system
        /// </summary>
        /// <param name="data">The authentication request data to process</param>
        /// <returns>true if the message was sent</returns>
        public readonly bool Authenticate(LobbyAuthenticationData data)
        {
            return Matchmaking.Client.SendLobbyChatMsg(this, System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(data)));
        }

        #region Constants
        /// <summary>
        /// Standard metadata field representing the name of the lobby.
        /// This field is typically only used in lobby metadata
        /// </summary>
        public const string DataName = "name";
        /// <summary>
        /// Heathen standard metadata field representing the version of the game.
        /// This field is commonly used in lobby and member metadata
        /// </summary>
        public const string DataVersion = "z_heathenGameVersion";
        /// <summary>
        /// Heathen standard metadata field indicating that the user is ready to play.
        /// This field is commonly only used on member metadata
        /// </summary>
        public const string DataReady = "z_heathenReady";
        /// <summary>
        /// Heathen standard metadata field indicating that these users should leave the lobby.
        /// This is a string containing each CSteamID of members that should not join this lobby and if present should leave it.
        /// Data in this list is in the form of [ + CSteamID + ] e.g. [123456789][987654321] would indicate 2 users that should leave
        /// This field is commonly only used on lobby metadata
        /// </summary>
        public const string DataKick = "z_heathenKick";
        /// <summary>
        /// Heathen standard metadata field indicating the mode of the lobby e.g. group or general
        /// If this is blank its assumed to be general
        /// </summary>
        public const string DataMode = "z_heathenMode";
        /// <summary>
        /// Heathen standard metadata field indicating the type of lobby e.g. private, friend, public or invisible
        /// </summary>
        public const string DataType = "z_heathenType";
    #endregion

    #region Boilerplate
        public int CompareTo(CSteamID other)
        {
            return id.CompareTo(other);
        }

        public int CompareTo(ulong other)
        {
            return id.CompareTo(other);
        }

        public override string ToString()
        {
            return HexId;
        }

        public bool Equals(CSteamID other)
        {
            return id.Equals(other);
        }

        public bool Equals(ulong other)
        {
            return id.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return id.Equals(obj);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public bool Equals(LobbyData other)
        {
            return id.Equals(other.id);
        }

        public static bool operator ==(LobbyData l, LobbyData r) => l.id == r.id;
        public static bool operator ==(CSteamID l, LobbyData r) => l.m_SteamID == r.id;
        public static bool operator ==(LobbyData l, CSteamID r) => l.id == r.m_SteamID;
        public static bool operator ==(LobbyData l, ulong r) => l.id == r;
        public static bool operator ==(ulong l, LobbyData r) => l == r.id;
        public static bool operator !=(LobbyData l, LobbyData r) => l.id != r.id;
        public static bool operator !=(CSteamID l, LobbyData r) => l.m_SteamID != r.id;
        public static bool operator !=(LobbyData l, CSteamID r) => l.id != r.m_SteamID;
        public static bool operator !=(LobbyData l, ulong r) => l.id != r;
        public static bool operator !=(ulong l, LobbyData r) => l != r.id;

        public static implicit operator CSteamID(LobbyData c) => c.SteamId;
        public static implicit operator LobbyData(CSteamID id) => new LobbyData { id = id.m_SteamID };
        public static implicit operator ulong(LobbyData id) => id.id;
        public static implicit operator LobbyData(ulong id) => new LobbyData { id = id };
        public static implicit operator LobbyData(AccountID_t id) => Get(id);
        public static implicit operator LobbyData(uint id) => Get(id);
        public static implicit operator LobbyData(string id) => Get(id);

    #endregion
    }
}
#endif