#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Functions for clients to access matchmaking services, favorites, and to operate on game lobbies and the game server browser.
    /// </summary>
    public static class Matchmaking
    {
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                memberOfLobbies = new List<LobbyData>();
                eventLobbyEnter = new LobbyEnterEvent();
                eventLobbyDataUpdate = new LobbyDataUpdateEvent();
                eventLobbyChatMsg = new LobbyChatMsgEvent();
                eventFavoritesListChanged = new FavoritesListChangedEvent();
                eventLobbyChatUpdate = new LobbyChatUpdateEvent();
                eventLobbyGameCreated = new LobbyGameCreatedEvent();
                eventLobbyInvite = new LobbyInviteEvent();
                eventLobbyLeave = new LobbyDataEvent();
                eventLobbyAskedToLeave = new LobbyDataEvent();
                m_LobbyCreated_t = null;
                m_LobbyMatchList_t = null;
                m_LobbyEnter_t2 = null;
                m_LobbyEnter_t = null;
                m_LobbyChatMsg_t = null;
                m_LobbyDataUpdate_t = null;
                m_FavoritesListChanged_t = null;
                m_LobbyChatUpdate_t = null;
                m_LobbyGameCreated_t = null;
                m_LobbyInvite_t = null;
            }

            /// <summary>
            /// This list is populated by the system as the user creates, joins and leaves lobbies.
            /// </summary>
            public static List<LobbyData> memberOfLobbies = new List<LobbyData>();
            /// <summary>
            /// Occurs when a lobby enter callback is received and the response code is a success
            /// </summary>
            public static LobbyEnterEvent EventLobbyEnterSuccess
            {
                get
                {
                    m_LobbyEnter_t ??= Callback<LobbyEnter_t>.Create(LobbyEnterHandler);

                    return eventLobbyEnterSuccess;
                }
            }
            /// <summary>
            /// Occurs when a lobby enter callback is received and the response code is not a success.
            /// </summary>
            /// <remarks>
            /// You can cast the m_EChatRoomEnterResponse value to a EChatRoomEnterResponse enum to determine the reason for failure
            /// </remarks>
            public static LobbyEnterEvent EventLobbyEnterFailed
            {
                get
                {
                    m_LobbyEnter_t ??= Callback<LobbyEnter_t>.Create(LobbyEnterHandler);

                    return eventLobbyEnterFailed;
                }
            }
            /// <summary>
            /// The lobby metadata has changed.
            /// </summary>
            public static LobbyDataUpdateEvent EventLobbyDataUpdate
            {
                get
                {
                    m_LobbyDataUpdate_t ??= Callback<LobbyDataUpdate_t>.Create((response) =>
                        {
                            if (response.m_ulSteamIDLobby == response.m_ulSteamIDMember)
                            {
                                //This is a metadata change for the lobby ... check for kick update
                                LobbyData lobby = response.m_ulSteamIDLobby;
                                if (lobby[LobbyData.DataKick].Contains("[" + User.Client.Id.ToString() + "]"))
                                    eventLobbyAskedToLeave.Invoke(lobby);
                            }

                            eventLobbyDataUpdate.Invoke(response);
                        });

                    return eventLobbyDataUpdate;
                }
            }
            /// <summary>
            /// A chat (text or binary) message for this lobby has been received. After getting this you must use GetLobbyChatEntry to retrieve the contents of this message.
            /// </summary>
            public static LobbyChatMsgEvent EventLobbyChatMsg
            {
                get
                {
                    m_LobbyChatMsg_t ??= Callback<LobbyChatMsg_t>.Create((result) =>
                    {
                        byte[] data = new byte[4096];
                        var lobby = new CSteamID(result.m_ulSteamIDLobby);
                        int ret = SteamMatchmaking.GetLobbyChatEntry(lobby, (int)result.m_iChatID, out CSteamID user, data, data.Length, out EChatEntryType chatEntryType);
                        Array.Resize(ref data, ret);

                        var chatMsg = new LobbyChatMsg
                        {
                            lobby = lobby,
                            type = chatEntryType,
                            data = data,
                            receivedTime = DateTime.Now,
                            sender = user,
                        };

                        //If this is a chat message
                        //and is not from me
                        //and I am the owner of the lobby its from
                        if (chatMsg.type == EChatEntryType.k_EChatEntryTypeChatMsg
                         && chatMsg.sender != UserData.Me
                         && chatMsg.lobby.IsOwner)
                        {
                            //Check if its an authentication request
                            if (chatMsg.TryFromJson<LobbyAuthenticationData>(out var authRequest)
                            && authRequest.ticket != null)
                                eventLobbyAuthentication.Invoke(chatMsg.lobby, chatMsg.sender, authRequest.ticket, authRequest.inventory);
                            else
                                eventLobbyChatMsg.Invoke(chatMsg);
                        }
                        else
                        {
                            if (!chatMsg.TryFromJson<LobbyAuthenticationData>(out var authRequest)
                            || authRequest.ticket == null)
                                eventLobbyChatMsg.Invoke(chatMsg);
                            //else we are not the owner but this is an auth request so ignore it
                        }
                    });

                    return eventLobbyChatMsg;
                }
            }

            public static LobbyAuthenticationEvent EventLobbyAuthenticationRequest
            {
                get
                {
                    m_LobbyChatMsg_t ??= Callback<LobbyChatMsg_t>.Create((result) =>
                    {
                        byte[] data = new byte[4096];
                        var lobby = new CSteamID(result.m_ulSteamIDLobby);
                        int ret = SteamMatchmaking.GetLobbyChatEntry(lobby, (int)result.m_iChatID, out CSteamID user, data, data.Length, out EChatEntryType chatEntryType);
                        Array.Resize(ref data, ret);

                        var chatMsg = new LobbyChatMsg
                        {
                            lobby = lobby,
                            type = chatEntryType,
                            data = data,
                            receivedTime = DateTime.Now,
                            sender = user,
                        };

                        //If this is a chat message
                        //and is not from me
                        //and I am the owner of the lobby its from
                        if (chatMsg.type == EChatEntryType.k_EChatEntryTypeChatMsg
                         && chatMsg.sender != UserData.Me
                         && chatMsg.lobby.IsOwner)
                        {
                            //Check if its an authentication request
                            if (chatMsg.TryFromJson<LobbyAuthenticationData>(out var authRequest)
                           && authRequest.ticket != null)
                                eventLobbyAuthentication.Invoke(chatMsg.lobby, chatMsg.sender, authRequest.ticket, authRequest.inventory);
                            else
                                eventLobbyChatMsg.Invoke(chatMsg);
                        }
                        else
                        {
                            if (!chatMsg.TryFromJson<LobbyAuthenticationData>(out var authRequest)
                           || authRequest.ticket == null)
                                eventLobbyChatMsg.Invoke(chatMsg);
                            //else we are not the owner but this is an auth request so ignore it
                        }
                    });

                    return eventLobbyAuthentication;
                }
            }

            /// <summary>
            /// A server was added/removed from the favorites list, you should refresh now.
            /// </summary>
            public static FavoritesListChangedEvent EventFavoritesListChanged
            {
                get
                {
                    m_FavoritesListChanged_t ??= Callback<FavoritesListChanged_t>.Create(eventFavoritesListChanged.Invoke);

                    return eventFavoritesListChanged;
                }
            }
            /// <summary>
            /// A lobby chat room state has changed, this is usually sent when a user has joined or left the lobby.
            /// </summary>
            public static LobbyChatUpdateEvent EventLobbyChatUpdate
            {
                get
                {
                    m_LobbyChatUpdate_t ??= Callback<LobbyChatUpdate_t>.Create(eventLobbyChatUpdate.Invoke);

                    return eventLobbyChatUpdate;
                }
            }
            /// <summary>
            /// A game server has been set via SetLobbyGameServer for all of the members of the lobby to join. It's up to the individual clients to take action on this; the typical game behavior is to leave the lobby and connect to the specified game server; but the lobby may stay open throughout the session if desired.
            /// </summary>
            public static LobbyGameCreatedEvent EventLobbyGameCreated
            {
                get
                {
                    m_LobbyGameCreated_t ??= Callback<LobbyGameCreated_t>.Create(eventLobbyGameCreated.Invoke);

                    return eventLobbyGameCreated;
                }
            }
            /// <summary>
            /// Someone has invited you to join a Lobby. Normally you don't need to do anything with this, as the Steam UI will also display a '&lt;user&gt; has invited you to the lobby, join?' notification and message.
            /// </summary>
            /// <remarks>
            /// If the user outside a game chooses to join, your game will be launched with the parameter +connect_lobby &lt;64-bit lobby id&gt;, or with the callback GameLobbyJoinRequested_t if they're already in-game.
            /// </remarks>
            public static LobbyInviteEvent EventLobbyInvite
            {
                get
                {
                    m_LobbyInvite_t ??= Callback<LobbyInvite_t>.Create((e) => eventLobbyInvite.Invoke(e));

                    return eventLobbyInvite;
                }
            }
            public static LobbyDataEvent EventLobbyLeave => eventLobbyLeave;
            public static LobbyDataEvent EventLobbyAskedToLeave => eventLobbyAskedToLeave;

            private static LobbyEnterEvent eventLobbyEnter = new LobbyEnterEvent();
            private static LobbyEnterEvent eventLobbyEnterSuccess = new LobbyEnterEvent();
            private static LobbyEnterEvent eventLobbyEnterFailed = new LobbyEnterEvent();
            private static LobbyDataUpdateEvent eventLobbyDataUpdate = new LobbyDataUpdateEvent();
            private static LobbyChatMsgEvent eventLobbyChatMsg = new LobbyChatMsgEvent();
            private static LobbyAuthenticationEvent eventLobbyAuthentication = new LobbyAuthenticationEvent();
            private static FavoritesListChangedEvent eventFavoritesListChanged = new FavoritesListChangedEvent();
            private static LobbyChatUpdateEvent eventLobbyChatUpdate = new LobbyChatUpdateEvent();
            private static LobbyGameCreatedEvent eventLobbyGameCreated = new LobbyGameCreatedEvent();
            private static LobbyInviteEvent eventLobbyInvite = new LobbyInviteEvent();
            private static LobbyDataEvent eventLobbyLeave = new LobbyDataEvent();
            private static LobbyDataEvent eventLobbyAskedToLeave = new LobbyDataEvent();

            private static CallResult<LobbyCreated_t> m_LobbyCreated_t;
            private static CallResult<LobbyMatchList_t> m_LobbyMatchList_t;
            private static CallResult<LobbyEnter_t> m_LobbyEnter_t2;

            private static Callback<LobbyEnter_t> m_LobbyEnter_t;
            private static Callback<LobbyDataUpdate_t> m_LobbyDataUpdate_t;
            private static Callback<LobbyChatMsg_t> m_LobbyChatMsg_t;
            private static Callback<FavoritesListChanged_t> m_FavoritesListChanged_t;
            private static Callback<LobbyChatUpdate_t> m_LobbyChatUpdate_t;
            private static Callback<LobbyGameCreated_t> m_LobbyGameCreated_t;
            private static Callback<LobbyInvite_t> m_LobbyInvite_t;

            private static void LobbyEnterHandler(LobbyEnter_t response)
            {
                var responseCode = (EChatRoomEnterResponse)response.m_EChatRoomEnterResponse;

                if (responseCode == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                {
                    if (!memberOfLobbies.Any(p => p == response.m_ulSteamIDLobby))
                        memberOfLobbies.Add(new CSteamID(response.m_ulSteamIDLobby));

                    eventLobbyEnterSuccess.Invoke(response);
                }
                else
                {
                    if (API.App.isDebugging || Application.isEditor)
                    {
                        if (responseCode != EChatRoomEnterResponse.k_EChatRoomEnterResponseLimited)
                        {
                            Debug.LogWarning("This user is limited and cannot fully join a Steam Lobby! metadata and lobby chat will not work for this user though they may appear in the members list.");
                        }
                        else
                        {
                            if (responseCode != EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                                Debug.LogWarning("Detected a Failed lobby enter attempt (" + response.m_ulSteamIDLobby + ":" + responseCode + ")");
                            else
                                Debug.Log("Detected a successful lobby enter attempt (" + response.m_ulSteamIDLobby + ":" + responseCode + ")");
                        }

                        LeaveLobby(response.m_ulSteamIDLobby);
                    }

                    eventLobbyEnterFailed.Invoke(response);
                }

                eventLobbyEnter.Invoke(response);
            }

            public static LobbyData GetCommandLineConnectLobby()
            {
                var args = Environment.GetCommandLineArgs();
                ulong value = 0;
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "+connect_lobby" && i + 1 < args.Length && ulong.TryParse(args[i + 1], out value))
                        return value;
                }

                return value;
            }

            /// <summary>
            /// Adds the game server to the local favorites list or updates the time played of the server if it already exists in the list.
            /// </summary>
            /// <param name="appID">The App ID of the game.</param>
            /// <param name="ipAddress">The IP address of the server in host order, i.e 127.0.0.1 == 0x7f000001.</param>
            /// <param name="port">The port used to connect to the server, in host order.</param>
            /// <param name="queryPort">The port used to query the server, in host order.</param>
            /// <param name="lastPlayedOnServer"></param>
            public static void AddHistoryGame(AppId_t appID, uint ipAddress, ushort port, ushort queryPort, DateTime lastPlayedOnServer) => SteamMatchmaking.AddFavoriteGame(appID, ipAddress, port, queryPort, Constants.k_unFavoriteFlagHistory, Convert.ToUInt32((lastPlayedOnServer - new DateTime(1970, 1, 1)).TotalSeconds));
            /// <summary>
            /// Adds the game server to the local favorites list or updates the time played of the server if it already exists in the list.
            /// </summary>
            /// <param name="appID">The App ID of the game.</param>
            /// <param name="ipAddress">The IP address of the server in host order, i.e 127.0.0.1 == 0x7f000001.</param>
            /// <param name="port">The port used to connect to the server, in host order.</param>
            /// <param name="queryPort">The port used to query the server, in host order.</param>
            /// <param name="lastPlayedOnServer"></param>
            public static void AddFavoriteGame(AppId_t appID, uint ipAddress, ushort port, ushort queryPort, DateTime lastPlayedOnServer) => SteamMatchmaking.AddFavoriteGame(appID, ipAddress, port, queryPort, Constants.k_unFavoriteFlagFavorite, Convert.ToUInt32((lastPlayedOnServer - new DateTime(1970, 1, 1)).TotalSeconds));
            /// <summary>
            /// Adds the game server to the local favorites list or updates the time played of the server if it already exists in the list.
            /// </summary>
            /// <param name="appID">The App ID of the game.</param>
            /// <param name="ipAddress">The IP address of the server.</param>
            /// <param name="port">The port used to connect to the server, in host order.</param>
            /// <param name="queryPort">The port used to query the server, in host order.</param>
            /// <param name="lastPlayedOnServer"></param>
            public static void AddHistoryGame(AppId_t appID, string ipAddress, ushort port, ushort queryPort, DateTime lastPlayedOnServer) => SteamMatchmaking.AddFavoriteGame(appID, Utilities.IPStringToUint(ipAddress), port, queryPort, Constants.k_unFavoriteFlagHistory, Convert.ToUInt32((lastPlayedOnServer - new DateTime(1970, 1, 1)).TotalSeconds));
            /// <summary>
            /// Adds the game server to the local favorites list or updates the time played of the server if it already exists in the list.
            /// </summary>
            /// <param name="appID">The App ID of the game.</param>
            /// <param name="ipAddress">The IP address of the server.</param>
            /// <param name="port">The port used to connect to the server, in host order.</param>
            /// <param name="queryPort">The port used to query the server, in host order.</param>
            /// <param name="lastPlayedOnServer"></param>
            public static void AddFavoriteGame(AppId_t appID, string ipAddress, ushort port, ushort queryPort, DateTime lastPlayedOnServer) => SteamMatchmaking.AddFavoriteGame(appID, Utilities.IPStringToUint(ipAddress), port, queryPort, Constants.k_unFavoriteFlagFavorite, Convert.ToUInt32((lastPlayedOnServer - new DateTime(1970, 1, 1)).TotalSeconds));
            /// <summary>
            /// Sets the physical distance for which we should search for lobbies, this is based on the users IP address and a IP location map on the Steam backed.
            /// </summary>
            /// <param name="distanceFilter">Specifies the maximum distance.</param>
            public static void AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter distanceFilter) => SteamMatchmaking.AddRequestLobbyListDistanceFilter(distanceFilter);
            /// <summary>
            /// Filters to only return lobbies with the specified number of open slots available.
            /// </summary>
            /// <param name="slotsAvailable">The number of open slots that must be open.</param>
            public static void AddRequestLobbyListFilterSlotsAvailable(int slotsAvailable) => SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(slotsAvailable);
            /// <summary>
            /// Sorts the results closest to the specified value.
            /// </summary>
            /// <remarks>
            /// Near filters don't actually filter out values, they just influence how the results are sorted. You can specify multiple near filters, with the first near filter influencing the most, and the last near filter influencing the least.
            /// </remarks>
            /// <param name="key">The filter key name to match. This can not be longer than k_nMaxLobbyKeyLength.</param>
            /// <param name="value">The value that lobbies will be sorted on.</param>
            public static void AddRequestLobbyListNearValueFilter(string key, int value) => SteamMatchmaking.AddRequestLobbyListNearValueFilter(key, value);
            /// <summary>
            /// Adds a numerical comparison filter to the next RequestLobbyList call.
            /// </summary>
            /// <param name="key">The filter key name to match. This can not be longer than k_nMaxLobbyKeyLength.</param>
            /// <param name="value">The number to match.</param>
            /// <param name="comparison">The type of comparison to make.</param>
            public static void AddRequestLobbyListNumericalFilter(string key, int value, ELobbyComparison comparison) => SteamMatchmaking.AddRequestLobbyListNumericalFilter(key, value, comparison);
            /// <summary>
            /// Sets the maximum number of lobbies to return. The lower the count the faster it is to download the lobby results & details to the client.
            /// </summary>
            /// <param name="max"></param>
            public static void AddRequestLobbyListResultCountFilter(int max) => SteamMatchmaking.AddRequestLobbyListResultCountFilter(max);
            /// <summary>
            /// Adds a string comparison filter to the next RequestLobbyList call.
            /// </summary>
            /// <param name="key">The filter key name to match. This can not be longer than k_nMaxLobbyKeyLength.</param>
            /// <param name="value">The string to match.</param>
            /// <param name="comparison">The type of comparison to make.</param>
            public static void AddRequestLobbyListStringFilter(string key, string value, ELobbyComparison comparison) => SteamMatchmaking.AddRequestLobbyListStringFilter(key, value, comparison);
            [Obsolete("Update your callback to take (EResult result, Lobby lobby, bool IOError)")]
            public static void CreateLobby(ELobbyType type, int maxMembers, Action<LobbyData, bool> callback)
            {
                CreateLobby(type, maxMembers, (r, l, e) =>
                    {
                        callback?.Invoke(l, e);
                    });
            }
            /// <summary>
            /// Create a new matchmaking lobby.
            /// </summary>
            /// <param name="type">The type and visibility of this lobby. This can be changed later via SetLobbyType.</param>
            /// <param name="maxMembers">The maximum number of players that can join this lobby. This can not be above 250.</param>
            /// <param name="callback">
            /// An action to be invoked when the creation is completed
            /// <code>
            /// void Callback(EResult result, Lobby lobby, bool ioError)
            /// {
            /// }
            /// </code>
            /// </param>
            public static void CreateLobby(ELobbyType type, int maxMembers, Action<EResult, LobbyData, bool> callback)
            {
                if (type == ELobbyType.k_ELobbyTypePrivateUnique)
                {
                    throw new ArgumentOutOfRangeException("The `k_ELobbyTypePrivateUnique` should not be used and is a legacy feature of Steam API that is not defined for use in the Client API. It is shown in the ELobbyType and editor as a matter of compatibility with the native API. Do Not User It.");
                }

                if (callback == null)
                    return;

                m_LobbyCreated_t ??= CallResult<LobbyCreated_t>.Create();

                var handle = SteamMatchmaking.CreateLobby(type, maxMembers);
                m_LobbyCreated_t.Set(handle, (r, e) =>
                {
                    if (!e && r.m_eResult == EResult.k_EResultOK)
                    {
                        SetLobbyData(new CSteamID(r.m_ulSteamIDLobby), LobbyData.DataType, ((int)type).ToString());
                        memberOfLobbies.Add(new CSteamID(r.m_ulSteamIDLobby));
                    }
                    if (type == ELobbyType.k_ELobbyTypeInvisible)
                        LobbyData.Get(r.m_ulSteamIDLobby).IsGroup = true;
                    else
                        LobbyData.Get(r.m_ulSteamIDLobby).IsSession = true;
                    callback.Invoke(r.m_eResult, new CSteamID(r.m_ulSteamIDLobby), e);
                });
            }
            /// <summary>
            /// Removes a metadata key from the lobby.
            /// </summary>
            /// <remarks>
            /// This can only be done by the owner of the lobby.
            /// </remarks>
            /// <param name="lobby">The Steam ID of the lobby to delete the metadata for.</param>
            /// <param name="key">The key to delete the data for.</param>
            /// <returns></returns>
            public static bool DeleteLobbyData(LobbyData lobby, string key) => SteamMatchmaking.DeleteLobbyData(lobby, key);
            /// <summary>
            /// Gets the details of the favorite game server by index.
            /// </summary>
            /// <param name="index">The index of the favorite game server to get the details of. This must be between 0 and GetFavoriteGameCount</param>
            /// <returns>Null if the index was invalid, otherwise the result contains the details of the server.</returns>
            public static FavoriteGame? GetFavoriteGame(int index)
            {
                if (SteamMatchmaking.GetFavoriteGame(index, out AppId_t app, out uint ip, out ushort connPort, out ushort queryPort, out uint flags, out uint lastPlayed))
                {
                    return new FavoriteGame
                    {
                        appId = app,
                        ipAddress = ip,
                        connectionPort = connPort,
                        queryPort = queryPort,
                        lastPlayedOnServer = new DateTime(1970, 1, 1).AddSeconds(lastPlayed),
                        isHistory = flags == Constants.k_unFavoriteFlagHistory
                    };
                }
                else
                    return null;
            }
            /// <summary>
            /// Returns the collection of favorite game entries
            /// </summary>
            /// <returns></returns>
            public static FavoriteGame[] GetFavoriteGames()
            {
                var count = SteamMatchmaking.GetFavoriteGameCount();
                var results = new FavoriteGame[count];
                for (int i = 0; i < count; i++)
                {
                    SteamMatchmaking.GetFavoriteGame(i, out AppId_t app, out uint ip, out ushort connPort, out ushort queryPort, out uint flags, out uint lastPlayed);
                    results[i] = new FavoriteGame
                    {
                        appId = app,
                        ipAddress = ip,
                        connectionPort = connPort,
                        queryPort = queryPort,
                        lastPlayedOnServer = new DateTime(1970, 1, 1).AddSeconds(lastPlayed),
                        isHistory = flags == Constants.k_unFavoriteFlagHistory
                    };
                }
                return results;
            }
            /// <summary>
            /// Gets the number of favorite and recent game servers the user has stored locally.
            /// </summary>
            /// <returns></returns>
            public static int GetFavoriteGameCount() => SteamMatchmaking.GetFavoriteGameCount();
            /// <summary>
            /// Gets the metadata associated with the specified key from the specified lobby.
            /// </summary>
            /// <param name="lobby">The Steam ID of the lobby to get the metadata from.</param>
            /// <param name="key">The key to get the value of.</param>
            /// <returns></returns>
            public static string GetLobbyData(LobbyData lobby, string key) => SteamMatchmaking.GetLobbyData(lobby, key);
            /// <summary>
            /// Gets a dictionary containing all known metadata values for the indicated lobby
            /// </summary>
            /// <param name="lobby"></param>
            /// <returns></returns>
            public static Dictionary<string, string> GetLobbyData(LobbyData lobby)
            {
                var count = SteamMatchmaking.GetLobbyDataCount(lobby);
                var results = new Dictionary<string, string>();
                for (int i = 0; i < count; i++)
                {
                    if (SteamMatchmaking.GetLobbyDataByIndex(lobby, i, out string key, Constants.k_nMaxLobbyKeyLength, out string value, Constants.k_cubChatMetadataMax))
                    {
                        results.Add(key, value);
                    }
                }
                return results;
            }
            /// <summary>
            /// Gets the details of a game server set in a lobby.
            /// </summary>
            /// <param name="lobby"></param>
            /// <returns></returns>
            public static LobbyGameServer GetLobbyGameServer(LobbyData lobby)
            {
                SteamMatchmaking.GetLobbyGameServer(lobby, out uint ip, out ushort port, out CSteamID serverId);
                return new LobbyGameServer
                {
                    id = serverId,
                    ipAddress = ip,
                    port = port,
                };
            }
            /// <summary>
            /// Returns a list of user IDs for the members of the indicated lobby
            /// </summary>
            /// <remarks>
            /// NOTE: The current user must be in the lobby to retrieve the Steam IDs of other users in that lobby.
            /// </remarks>
            /// <param name="lobby">The lobby to query the list from</param>
            /// <returns></returns>
            public static LobbyMemberData[] GetLobbyMembers(LobbyData lobby)
            {
                var count = SteamMatchmaking.GetNumLobbyMembers(lobby);
                var results = new LobbyMemberData[count];
                for (int i = 0; i < count; i++)
                {
                    results[i] = new LobbyMemberData
                    {
                        lobby = lobby,
                        user = SteamMatchmaking.GetLobbyMemberByIndex(lobby, i)
                    };
                }
                return results;
            }
            /// <summary>
            /// The current limit on the # of users who can join the lobby.
            /// </summary>
            /// <param name="lobby"></param>
            /// <returns></returns>
            public static int GetLobbyMemberLimit(LobbyData lobby) => SteamMatchmaking.GetLobbyMemberLimit(lobby);
            /// <summary>
            /// Returns the current lobby owner.
            /// </summary>
            /// <remarks>
            /// NOTE: You must be a member of the lobby to access this.
            /// </remarks>
            /// <param name="lobby"></param>
            /// <returns></returns>
            public static CSteamID GetLobbyOwner(LobbyData lobby) => SteamMatchmaking.GetLobbyOwner(lobby);
            /// <summary>
            /// Invite another user to the lobby.
            /// </summary>
            /// <param name="lobby">The Steam ID of the lobby to invite the user to.</param>
            /// <param name="user">The Steam ID of the person who will be invited.</param>
            /// <returns></returns>
            public static bool InviteUserToLobby(LobbyData lobby, UserData user) => SteamMatchmaking.InviteUserToLobby(lobby, user);
            /// <summary>
            /// Joins an existing lobby.
            /// </summary>
            /// <remarks>
            /// The lobby Steam ID can be obtained either from a search with RequestLobbyList, joining on a friend, or from an invite.
            /// </remarks>
            /// <param name="lobby">The Steam ID of the lobby to join.</param>
            /// <param name="callback"></param>
            public static void JoinLobby(LobbyData lobby, Action<LobbyEnter, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_LobbyEnter_t2 == null)
                    m_LobbyEnter_t2 = CallResult<LobbyEnter_t>.Create();

                var handle = SteamMatchmaking.JoinLobby(lobby);
                m_LobbyEnter_t2.Set(handle, (r, e) =>
                {
                    var response = (EChatRoomEnterResponse)r.m_EChatRoomEnterResponse;

                    if (!e && response == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                        memberOfLobbies.Add(new CSteamID(r.m_ulSteamIDLobby));
                    else
                    {
                        if (response == EChatRoomEnterResponse.k_EChatRoomEnterResponseLimited)
                            SteamMatchmaking.LeaveLobby(new CSteamID(r.m_ulSteamIDLobby));
                    }

                    callback.Invoke(r, e);
                });
            }
            /// <summary>
            /// Leave a lobby that the user is currently in; this will take effect immediately on the client side, other users in the lobby will be notified by a LobbyChatUpdate_t callback.
            /// </summary>
            /// <param name="lobby"></param>
            public static void LeaveLobby(LobbyData lobby)
            {
                if (API.App.isDebugging)
                {
                    Debug.Log("Detected lobby exit (" + lobby + ")");
                }

                eventLobbyLeave.Invoke(lobby);
                SteamMatchmaking.LeaveLobby(lobby);
                memberOfLobbies.RemoveAll(p => p == lobby);
            }
            /// <summary>
            /// Removes the game server from the local favorites list.
            /// </summary>
            /// <param name="appId">The App ID of the game.</param>
            /// <param name="ip">The IP address of the server in host order, i.e 127.0.0.1 == 0x7f000001.</param>
            /// <param name="connectionPort">The port used to connect to the server, in host order.</param>
            /// <param name="queryPort">The port used to query the server, in host order.</param>
            /// <returns></returns>
            public static bool RemoveFavoriteGame(AppId_t appId, uint ip, ushort connectionPort, ushort queryPort) => SteamMatchmaking.RemoveFavoriteGame(appId, ip, connectionPort, queryPort, Constants.k_unFavoriteFlagFavorite);
            /// <summary>
            /// Removes the game server from the local favorites list.
            /// </summary>
            /// <param name="appId">The App ID of the game.</param>
            /// <param name="ip">The IP address of the server in host order, i.e 127.0.0.1 == 0x7f000001.</param>
            /// <param name="connectionPort">The port used to connect to the server, in host order.</param>
            /// <param name="queryPort">The port used to query the server, in host order.</param>
            /// <returns></returns>
            public static bool RemoveHistoryGame(AppId_t appId, uint ip, ushort connectionPort, ushort queryPort) => SteamMatchmaking.RemoveFavoriteGame(appId, ip, connectionPort, queryPort, Constants.k_unFavoriteFlagHistory);
            /// <summary>
            /// Removes the game server from the local favorites list.
            /// </summary>
            /// <param name="appId">The App ID of the game.</param>
            /// <param name="ip">The IP address of the server in host order.</param>
            /// <param name="connectionPort">The port used to connect to the server, in host order.</param>
            /// <param name="queryPort">The port used to query the server, in host order.</param>
            /// <returns></returns>
            public static bool RemoveFavoriteGame(AppId_t appId, string ip, ushort connectionPort, ushort queryPort) => SteamMatchmaking.RemoveFavoriteGame(appId, Utilities.IPStringToUint(ip), connectionPort, queryPort, Constants.k_unFavoriteFlagFavorite);
            /// <summary>
            /// Removes the game server from the local favorites list.
            /// </summary>
            /// <param name="appId">The App ID of the game.</param>
            /// <param name="ip">The IP address of the server in host order.</param>
            /// <param name="connectionPort">The port used to connect to the server, in host order.</param>
            /// <param name="queryPort">The port used to query the server, in host order.</param>
            /// <returns></returns>
            public static bool RemoveHistoryGame(AppId_t appId, string ip, ushort connectionPort, ushort queryPort) => SteamMatchmaking.RemoveFavoriteGame(appId, Utilities.IPStringToUint(ip), connectionPort, queryPort, Constants.k_unFavoriteFlagHistory);
            /// <summary>
            /// Refreshes all of the metadata for a lobby that you're not in right now.
            /// </summary>
            /// <remarks>
            /// You will never do this for lobbies you're a member of, that data will always be up to date. You can use this to refresh lobbies that you have obtained from RequestLobbyList or that are available via friends.
            /// </remarks>
            /// <param name="lobby">The Steam ID of the lobby to refresh the metadata of.</param>
            /// <returns></returns>
            public static bool RequestLobbyData(LobbyData lobby) => SteamMatchmaking.RequestLobbyData(lobby);
            /// <summary>
            /// Get a filtered list of relevant lobbies.
            /// </summary>
            /// <remarks>
            /// <para>
            /// There can only be one active lobby search at a time. The old request will be canceled if a new one is started. Depending on the users connection to the Steam back-end, this call can take from 300ms to 5 seconds to complete, and has a timeout of 20 seconds.
            /// </para>
            /// <para>
            /// NOTE: To filter the results you MUST call the AddRequestLobbyList* functions before calling this. The filters are cleared on each call to this function.
            /// </para>
            /// <para>
            /// NOTE: If AddRequestLobbyListDistanceFilter is not called, k_ELobbyDistanceFilterDefault will be used, which will only find matches in the same or nearby regions.
            /// </para>
            /// <para>
            /// NOTE: This will only return lobbies that are not full, and only lobbies that are k_ELobbyTypePublic or k_ELobbyTypeInvisible, and are set to joinable with SetLobbyJoinable.
            /// </para>
            /// </remarks>
            /// <param name="callback"></param>
            public static void RequestLobbyList(Action<LobbyData[], bool> callback)
            {
                if (callback == null)
                    return;

                if (m_LobbyMatchList_t == null)
                    m_LobbyMatchList_t = CallResult<LobbyMatchList_t>.Create();

                var handle = SteamMatchmaking.RequestLobbyList();
                m_LobbyMatchList_t.Set(handle, (results, error) =>
                {
                    if (!error && results.m_nLobbiesMatching > 0)
                    {
                        var buffer = new LobbyData[results.m_nLobbiesMatching];
                        for (int i = 0; i < results.m_nLobbiesMatching; i++)
                        {
                            buffer[i] = SteamMatchmaking.GetLobbyByIndex(i);
                        }
                        callback.Invoke(buffer, error);
                    }
                    else
                    {
                        callback.Invoke(new LobbyData[0], error);
                    }
                });
            }
            /// <summary>
            /// Broadcasts a chat (text or binary data) message to the all of the users in the lobby.
            /// </summary>
            /// <param name="lobby">The Steam ID of the lobby to send the chat message to.</param>
            /// <param name="messageBody">This can be text or binary data, up to 4 Kilobytes in size.</param>
            /// <returns></returns>
            public static bool SendLobbyChatMsg(LobbyData lobby, byte[] messageBody) => SteamMatchmaking.SendLobbyChatMsg(lobby, messageBody, messageBody.Length);
            /// <summary>
            /// Sets a key/value pair in the lobby metadata. This can be used to set the the lobby name, current map, game mode, etc.
            /// </summary>
            /// <remarks>
            /// This can only be set by the owner of the lobby. Lobby members should use SetLobbyMemberData instead.
            /// </remarks>
            /// <param name="lobby">The Steam ID of the lobby to set the metadata for.</param>
            /// <param name="key">The key to set the data for. This can not be longer than k_nMaxLobbyKeyLength.</param>
            /// <param name="value">The value to set. This can not be longer than k_cubChatMetadataMax.</param>
            /// <returns></returns>
            public static bool SetLobbyData(LobbyData lobby, string key, string value) => SteamMatchmaking.SetLobbyData(lobby, key, value);
            /// <summary>
            /// Sets the game server associated with the lobby.
            /// </summary>
            /// <remarks>
            /// <para>
            /// This can only be set by the owner of the lobby.
            /// </para>
            /// <para>
            /// Either the IP/Port or the Steam ID of the game server must be valid, depending on how you want the clients to be able to connect.
            /// </para>
            /// <para>
            /// A LobbyGameCreated_t callback will be sent to all players in the lobby, usually at this point, the users will join the specified game server.
            /// </para>
            /// </remarks>
            /// <param name="lobby">The Steam ID of the lobby to set the game server information for.</param>
            /// <param name="ip">Sets the IP address of the game server,</param>
            /// <param name="port">Sets the connection port of the game server, in host order.</param>
            /// <param name="gameServerId">Sets the Steam ID of the game server. Use k_steamIDNil if you're not setting this.</param>
            public static void SetLobbyGameServer(LobbyData lobby, uint ip, ushort port, CSteamID gameServerId) => SteamMatchmaking.SetLobbyGameServer(lobby, ip, port, gameServerId);
            /// <summary>
            /// Sets the game server associated with the lobby.
            /// </summary>
            /// <remarks>
            /// <para>
            /// This can only be set by the owner of the lobby.
            /// </para>
            /// <para>
            /// Either the IP/Port or the Steam ID of the game server must be valid, depending on how you want the clients to be able to connect.
            /// </para>
            /// <para>
            /// A LobbyGameCreated_t callback will be sent to all players in the lobby, usually at this point, the users will join the specified game server.
            /// </para>
            /// </remarks>
            /// <param name="lobby">The Steam ID of the lobby to set the game server information for.</param>
            /// <param name="ip">Sets the IP address of the game server,</param>
            /// <param name="port">Sets the connection port of the game server, in host order.</param>
            /// <param name="gameServerId">Sets the Steam ID of the game server. Use k_steamIDNil if you're not setting this.</param>
            public static void SetLobbyGameServer(LobbyData lobby, string ip, ushort port, CSteamID gameServerId) => SteamMatchmaking.SetLobbyGameServer(lobby, Utilities.IPStringToUint(ip), port, gameServerId);
            /// <summary>
            /// Sets whether or not a lobby is joinable by other players. This always defaults to enabled for a new lobby.
            /// </summary>
            /// <remarks>
            /// If joining is disabled, then no players can join, even if they are a friend or have been invited.
            /// </remarks>
            /// <param name="lobby">The Steam ID of the lobby</param>
            /// <param name="joinable">Enable (true) or disable (false) allowing users to join this lobby?</param>
            /// <returns></returns>
            public static bool SetLobbyJoinable(LobbyData lobby, bool joinable) => SteamMatchmaking.SetLobbyJoinable(lobby, joinable);
            /// <summary>
            /// Gets per-user metadata from another player in the specified lobby.
            /// </summary>
            /// <remarks>
            /// This can only be queried from members in lobbies that you are currently in.
            /// </remarks>
            /// <param name="lobby"></param>
            /// <param name="member"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static string GetLobbyMemberData(LobbyData lobby, CSteamID member, string key) => SteamMatchmaking.GetLobbyMemberData(lobby, member, key);
            /// <summary>
            /// Get the LobbyMember object for a given user
            /// </summary>
            /// <param name="id">The ID of the member to fetch</param>
            /// <param name="member">The member found</param>
            /// <returns>True if the user is a member of the lobby, false if they are not</returns>
            public static bool GetMember(LobbyData lobby, CSteamID id, out LobbyMemberData member)
            {
                var contained = GetLobbyMemberData(lobby, id, "anyKey");
                if (contained == null)
                {
                    member = default;
                    return false;
                }
                else
                {
                    member = new LobbyMemberData { lobby = lobby, user = id };
                    return true;
                }
            }
            /// <summary>
            /// Checks if a user is a member of this lobby
            /// </summary>
            /// <param name="id">The user to check for</param>
            /// <returns>True if they are, false if not</returns>
            public static bool IsAMember(LobbyData lobby, CSteamID id)
            {
                var contained = GetLobbyMemberData(lobby, id, "anyKey");
                return contained != null;
            }
            /// <summary>
            /// Sets per-user metadata for the local user.
            /// </summary>
            /// <remarks>
            /// Each user in the lobby will be receive notification of the lobby data change via a LobbyDataUpdate_t callback, and any new users joining will receive any existing data.
            /// </remarks>
            /// <param name="lobby"></param>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public static void SetLobbyMemberData(LobbyData lobby, string key, string value) => SteamMatchmaking.SetLobbyMemberData(lobby, key, value);
            /// <summary>
            /// Set the maximum number of players that can join the lobby.
            /// </summary>
            /// <param name="lobby"></param>
            /// <param name="maxMembers"></param>
            /// <returns></returns>
            public static bool SetLobbyMemberLimit(LobbyData lobby, int maxMembers) => SteamMatchmaking.SetLobbyMemberLimit(lobby, maxMembers);
            /// <summary>
            /// Changes who the lobby owner is.
            /// </summary>
            /// <remarks>
            /// This can only be set by the owner of the lobby. This will trigger a LobbyDataUpdate_t for all of the users in the lobby, each user should update their local state to reflect the new owner. This is typically accomplished by displaying a crown icon next to the owners name.
            /// </remarks>
            /// <param name="lobby"></param>
            /// <param name="newOwner"></param>
            /// <returns></returns>
            public static bool SetLobbyOwner(LobbyData lobby, CSteamID newOwner) => SteamMatchmaking.SetLobbyOwner(lobby, newOwner);
            /// <summary>
            /// Updates what type of lobby this is.
            /// </summary>
            /// <param name="lobby"></param>
            /// <param name="type"></param>
            /// <returns></returns>
            public static bool SetLobbyType(LobbyData lobby, ELobbyType type)
            {
                SteamMatchmaking.SetLobbyData(lobby, LobbyData.DataType, ((int)type).ToString());
                return SteamMatchmaking.SetLobbyType(lobby, type);
            }
            /// <summary>
            /// Cancel an outstanding server list request.
            /// </summary>
            /// <param name="request">The handle to the server list request.</param>
            public static void CancelQuery(HServerListRequest request) => SteamMatchmakingServers.CancelQuery(request);
            /// <summary>
            /// Cancel an outstanding individual server query.
            /// </summary>
            /// <param name="query">The server query to cancel.</param>
            public static void CancelServerQuery(HServerQuery query) => SteamMatchmakingServers.CancelServerQuery(query);
            /// <summary>
            /// Gets the number of servers in the given list.
            /// </summary>
            /// <param name="request">The handle to the server list request.</param>
            /// <returns></returns>
            public static int GetServerCount(HServerListRequest request) => SteamMatchmakingServers.GetServerCount(request);
            /// <summary>
            /// Get the details of a given server in the list.
            /// </summary>
            /// <param name="request"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public static gameserveritem_t GetServerDetails(HServerListRequest request, int index) => SteamMatchmakingServers.GetServerDetails(request, index);
            /// <summary>
            /// Gets the details of a given server list request
            /// </summary>
            /// <param name="request"></param>
            /// <returns></returns>
            public static gameserveritem_t[] GetServerDetails(HServerListRequest request)
            {
                var count = SteamMatchmakingServers.GetServerCount(request);
                var results = new gameserveritem_t[count];
                for (int i = 0; i < count; i++)
                {
                    results[i] = SteamMatchmakingServers.GetServerDetails(request, i);
                }
                return results;
            }
            /// <summary>
            /// Checks if the server list request is currently refreshing.
            /// </summary>
            /// <param name="request"></param>
            /// <returns></returns>
            public static bool IsRefreshing(HServerListRequest request) => SteamMatchmakingServers.IsRefreshing(request);
            /// <summary>
            /// Queries an individual game servers directly via IP/Port to request an updated ping time and other details from the server.
            /// </summary>
            /// <remarks>
            /// You must inherit from the ISteamMatchmakingPingResponse object to receive this callback.
            /// </remarks>
            /// <param name="ip"></param>
            /// <param name="port"></param>
            /// <param name="response"></param>
            /// <returns></returns>
            public static HServerQuery PingServer(uint ip, ushort port, ISteamMatchmakingPingResponse response) => SteamMatchmakingServers.PingServer(ip, port, response);
            /// <summary>
            /// Queries an individual game servers directly via IP/Port to request an updated ping time and other details from the server.
            /// </summary>
            /// <remarks>
            /// You must inherit from the ISteamMatchmakingPingResponse object to receive this callback.
            /// </remarks>
            /// <param name="ip"></param>
            /// <param name="port"></param>
            /// <param name="response"></param>
            /// <returns></returns>
            public static HServerQuery PingServer(string ip, ushort port, ISteamMatchmakingPingResponse response) => SteamMatchmakingServers.PingServer(Utilities.IPStringToUint(ip), port, response);
            /// <summary>
            /// Queries an individual game servers directly via IP/Port to request the list of players currently playing on the server.
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="port"></param>
            /// <param name="response"></param>
            /// <returns></returns>
            public static HServerQuery PlayerDetails(uint ip, ushort port, ISteamMatchmakingPlayersResponse response) => SteamMatchmakingServers.PlayerDetails(ip, port, response);
            /// <summary>
            /// Queries an individual game servers directly via IP/Port to request the list of players currently playing on the server.
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="port"></param>
            /// <param name="response"></param>
            /// <returns></returns>
            public static HServerQuery PlayerDetails(string ip, ushort port, ISteamMatchmakingPlayersResponse response) => SteamMatchmakingServers.PlayerDetails(Utilities.IPStringToUint(ip), port, response);
            /// <summary>
            /// Ping every server in your list again but don't update the list of servers.
            /// </summary>
            /// <param name="request"></param>
            public static void RefreshQuery(HServerListRequest request) => SteamMatchmakingServers.RefreshQuery(request);
            /// <summary>
            /// Refreshes a single server inside of a query.
            /// </summary>
            /// <param name="request"></param>
            /// <param name="index"></param>
            public static void RefreshServer(HServerListRequest request, int index) => SteamMatchmakingServers.RefreshServer(request, index);
            /// <summary>
            /// Releases the asynchronous server list request object and cancels any pending query on it if there's a pending query in progress.
            /// </summary>
            /// <param name="request"></param>
            public static void ReleaseRequest(HServerListRequest request) => SteamMatchmakingServers.ReleaseRequest(request);
            /// <summary>
            /// Request a new list of game servers from the 'favorites' server list.
            /// </summary>
            /// <param name="appId"></param>
            /// <param name="filters"></param>
            /// <param name="pRequestServersResponse"></param>
            /// <returns></returns>
            public static HServerListRequest RequestFavoritesServerList(AppId_t appId, MatchMakingKeyValuePair_t[] filters, ISteamMatchmakingServerListResponse pRequestServersResponse) => SteamMatchmakingServers.RequestFavoritesServerList(appId, filters, (uint)filters.Length, pRequestServersResponse);
            /// <summary>
            /// Request a new list of game servers from the 'friends' server list.
            /// </summary>
            /// <param name="appId"></param>
            /// <param name="filters"></param>
            /// <param name="pRequestServersResponse"></param>
            /// <returns></returns>
            public static HServerListRequest RequestFriendsServerList(AppId_t appId, MatchMakingKeyValuePair_t[] filters, ISteamMatchmakingServerListResponse pRequestServersResponse) => SteamMatchmakingServers.RequestFriendsServerList(appId, filters, (uint)filters.Length, pRequestServersResponse);
            /// <summary>
            /// Request a new list of game servers from the 'history' server list.
            /// </summary>
            /// <param name="appId"></param>
            /// <param name="filters"></param>
            /// <param name="pRequestServersResponse"></param>
            /// <returns></returns>
            public static HServerListRequest RequestHistoryServerList(AppId_t appId, MatchMakingKeyValuePair_t[] filters, ISteamMatchmakingServerListResponse pRequestServersResponse) => SteamMatchmakingServers.RequestHistoryServerList(appId, filters, (uint)filters.Length, pRequestServersResponse);
            /// <summary>
            /// Request a new list of game servers from the 'internet' server list.
            /// </summary>
            /// <param name="appId"></param>
            /// <param name="filters"></param>
            /// <param name="pRequestServersResponse"></param>
            /// <returns></returns>
            public static HServerListRequest RequestInternetServerList(AppId_t appId, MatchMakingKeyValuePair_t[] filters, ISteamMatchmakingServerListResponse pRequestServersResponse) => SteamMatchmakingServers.RequestInternetServerList(appId, filters, (uint)filters.Length, pRequestServersResponse);
            /// <summary>
            /// Request a new list of game servers from the 'LAN' server list.
            /// </summary>
            /// <param name="appId"></param>
            /// <param name="pRequestServersResponse"></param>
            /// <returns></returns>
            public static HServerListRequest RequestLANServerList(AppId_t appId, ISteamMatchmakingServerListResponse pRequestServersResponse) => SteamMatchmakingServers.RequestLANServerList(appId, pRequestServersResponse);
            /// <summary>
            /// Request a new list of game servers from the 'spectator' server list.
            /// </summary>
            /// <param name="appId"></param>
            /// <param name="filters"></param>
            /// <param name="pRequestServersResponse"></param>
            /// <returns></returns>
            public static HServerListRequest RequestSpectatorServerList(AppId_t appId, MatchMakingKeyValuePair_t[] filters, ISteamMatchmakingServerListResponse pRequestServersResponse) => SteamMatchmakingServers.RequestSpectatorServerList(appId, filters, (uint)filters.Length, pRequestServersResponse);
            /// <summary>
            /// Queries an individual game servers directly via IP/Port to request the list of rules that the server is running. (See ISteamGameServer::SetKeyValue to set the rules on the server side.)
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="port"></param>
            /// <param name="response"></param>
            /// <returns></returns>
            public static HServerQuery ServerRules(uint ip, ushort port, ISteamMatchmakingRulesResponse response) => SteamMatchmakingServers.ServerRules(ip, port, response);
            /// <summary>
            /// Queries an individual game servers directly via IP/Port to request the list of rules that the server is running. (See ISteamGameServer::SetKeyValue to set the rules on the server side.)
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="port"></param>
            /// <param name="response"></param>
            /// <returns></returns>
            public static HServerQuery ServerRules(string ip, ushort port, ISteamMatchmakingRulesResponse response) => SteamMatchmakingServers.ServerRules(Utilities.IPStringToUint(ip), port, response);
            /// <summary>
            /// Leaves the current lobby if any
            /// </summary>
            public static void LeaveAllLobbies()
            {
                var tempList = memberOfLobbies.ToArray();

                foreach (var lobby in tempList)
                    lobby.Leave();

                memberOfLobbies.Clear();
                tempList = null;
            }
        }
    }
}
#endif