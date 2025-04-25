#if !DISABLESTEAMWORKS  && STEAMWORKSNET
#if UNITY_EDITOR
using UnityEditor;
#endif
using Heathen.SteamworksIntegration.API;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Helps you find or create a lobby.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is meant to be attached to your lobby UI, party UI or similar and manages 1 single lobby.
    /// It can be used to search for a matching lobby and automatically join it, 
    /// to create a lobby or to browse for lobby that match its <see cref="searchArguments"/>.
    /// </para>
    /// <para>
    /// When you create or join a lobby using this tool it will link that joined or created lobby and provide access to that lobbies events. methods and members.
    /// You can then use this object as an interface between your UI and a specific lobby to create lobby windows, party windows, lobby chats and more.
    /// </para>
    /// </remarks>
    [HelpURL("https://kb.heathen.group/toolkit-for-steamworks-sdk/unity/ui-components/lobby-manager")]
    public class LobbyManager : MonoBehaviour
    {
        public enum ManagedLobbyEvents
        {
            AuthenticationSessionResults,
            LobbyChatMessageReceived,
            LobbyCreationFailed,
            LobbyCreationSuccess,
            LobbyInviteReceived,
            LobbyJoinFailure,
            LobbyJoinSuccess,
            LobbyLeave,
            MetadataUpdated,
            OtherUserLeft,
            OtherUserJoined,
            QuickMatchFailed,
            SearchResultsReady,
            SessionConnectionUpdated,
            YouAreAskedToLeave,
        }

        [SerializeField]
        private List<ManagedLobbyEvents> m_Delegates;

        [Tooltip("The values to be used when searching for a lobby")]
        public SearchArguments searchArguments = new();
        [Tooltip("The values to be used when creating a new lobby")]
        public CreateArguments createArguments = new();
        /// <summary>
        /// The Rich Presence fields to be set when a lobby is created.
        /// </summary>
        [Tooltip("The Rich Presence fields to be set when joining a lobby.\nDoes not apply to creating a lobby, use the Create Arguments for that.")]
        public List<StringKeyValuePair> richPresenceFields = new();
        [Header("Events")]
        public LobbyInviteEvent evtLobbyInvite;
        public LobbyChatMsgEvent evtChatMsgReceived;
        public LobbyDataListEvent evtFound;
        /// <summary>
        /// Occurs when the local user enters a lobby as a response to a join
        /// </summary>
        public LobbyDataEvent evtEnterSuccess;
        /// <summary>
        /// Occurs when the local user tried but failed to enter a lobby
        /// </summary>
        public LobbyResponceEvent evtEnterFailed;
        /// <summary>
        /// Occurs when the local user creates a lobby
        /// </summary>
        public LobbyDataEvent evtCreated;
        /// <summary>
        /// Occurs when the local user tried but failed to create a lobby
        /// </summary>
        public EResultEvent evtCreateFailed;
        /// <summary>
        /// Occurs when the local user attempts to quick match but fails to find a match or resolve the quick match
        /// </summary>
        public UnityEvent evtQuickMatchFailed;
        /// <summary>
        /// Occurs when any data is updated on the lobby be that lobby metadata or a members metadata
        /// </summary>
        public LobbyDataUpdateEvent evtDataUpdated;
        /// <summary>
        /// Occurs when the local user leaves the managed lobby
        /// </summary>
        public UnityEvent evtLeave;
        /// <summary>
        /// Occurs when the local user is asked to leave the lobby via Heathen's Kick system
        /// </summary>
        public UnityEvent evtAskedToLeave;
        /// <summary>
        /// Occurs when the <see cref="GameServer"/> information is first set on the lobby
        /// </summary>
        public GameServerSetEvent evtGameCreated;
        /// <summary>
        /// Occurs when the local user is a member of a lobby and a new member joins that lobby
        /// </summary>
        public UserDataEvent evtUserJoined;
        /// <summary>
        /// Occurs when the local user is a member of a lobby and another fellow member leaves the lobby
        /// </summary>
        public UserLeaveEvent evtUserLeft;
        /// <summary>
        /// Occurs when the local user who is the owner of the lobby receives and starts an auth session request for a user
        /// </summary>
        public LobbyAuthenticaitonSessionEvent evtAuthenticationSessionResult;
        /// <summary>
        /// The lobby this manager is currently managing
        /// </summary>
        /// <remarks>
        /// This will automatically be updated when you use the Lobby Manager to create, join or leave a lobby.
        /// If you manually create, join or leave a lobby you must update this field your self.
        /// To clear the value assign <see cref="CSteamID.Nil"/>
        /// </remarks>
        public LobbyData Lobby
        {
            get;
            set;
        }
        /// <summary>
        /// Returns true if the <see cref="Lobby"/> value is populated and resolves to a non-empty lobby
        /// </summary>
        public bool HasLobby => Lobby != CSteamID.Nil.m_SteamID && SteamMatchmaking.GetNumLobbyMembers(Lobby) > 0;
        /// <summary>
        /// Returns true if the local user is the owner of the managed lobby
        /// </summary>
        public bool IsPlayerOwner => HasLobby && Lobby.IsOwner;
        /// <summary>
        /// Returns true if all members in the lobby have indicated that they are Read via the Heathen Ready Check system
        /// </summary>
        public bool AllPlayersReady => HasLobby && Lobby.AllPlayersReady;
        /// <summary>
        /// Is the local player ready
        /// </summary>
        /// <remarks>
        /// You can assign this value to update the local player's LobbyMember accordingly for the Heathen Ready Check system
        /// </remarks>
        public bool IsPlayerReady
        {
            get => HasLobby && API.Matchmaking.Client.GetLobbyMemberData(Lobby, API.User.Client.Id, LobbyData.DataReady) == "true";
            set => API.Matchmaking.Client.SetLobbyMemberData(Lobby, LobbyData.DataReady, value.ToString().ToLower());
        }
        /// <summary>
        /// Returns true when the managed lobby is full e.g. unable to take more members
        /// </summary>
        public bool Full => HasLobby && Lobby.Full;
        [Obsolete("Use " + nameof(MaxMembers) + " instead.")]
        public int Slots => HasLobby ? SteamMatchmaking.GetLobbyMemberLimit(Lobby) : 0;
        public int MemberCount => HasLobby ? SteamMatchmaking.GetNumLobbyMembers(Lobby) : 0;
        /// <summary>
        /// Returns true if the Heathen TypeSet feature has been populated to indicate the type of lobby this is
        /// </summary>
        public bool IsTypeSet => HasLobby && Lobby.IsTypeSet;
        /// <summary>
        /// Returns the type of lobby this is, this is a feature of Heathen's Lobby tools. Valve does not actually expose this so this will only work for lobbies
        /// created by Heathen's tools such as <see cref="LobbyManager"/> and of course <see cref="API.Matchmaking.Client"/>
        /// </summary>
        public ELobbyType Type
        {
            get => Lobby.Type;
            set
            {
                var l = Lobby;
                l.Type = value;
            }
        }
        /// <summary>
        /// The max number of members this lobby supports
        /// </summary>
        /// <remarks>
        /// The owner of the lobby can set this value to update the max allowed
        /// </remarks>
        public int MaxMembers
        {
            get => HasLobby ? API.Matchmaking.Client.GetLobbyMemberLimit(new CSteamID(Lobby)) : 0;
            set => API.Matchmaking.Client.SetLobbyMemberLimit(new CSteamID(Lobby), value);
        }
        /// <summary>
        /// Does the managed lobby have game server data set on it?
        /// </summary>
        /// <remarks>
        /// <see cref="LobbyData.SetGameServer"/> for more information
        /// </remarks>
        public bool HasServer => SteamMatchmaking.GetLobbyGameServer(Lobby, out _, out _, out _);
        /// <summary>
        /// The game server information set on the managed lobby if any
        /// </summary>
        public LobbyGameServer GameServer => API.Matchmaking.Client.GetLobbyGameServer(Lobby);

        public string this[string key]
        {
            get => Lobby[key];
            set
            {
                var lobby = Lobby;
                lobby[key] = value;
            }
        }

        public LobbyMemberData this[UserData user] => Lobby[user];

        private void OnEnable()
        {
            API.Matchmaking.Client.EventLobbyAskedToLeave.AddListener(HandleAskedToLeave);
            API.Matchmaking.Client.EventLobbyDataUpdate.AddListener(HandleLobbyDataUpdate);
            API.Matchmaking.Client.EventLobbyLeave.AddListener(HandleLobbyLeave);
            API.Matchmaking.Client.EventLobbyGameCreated.AddListener(HandleGameServerSet);
            API.Matchmaking.Client.EventLobbyChatUpdate.AddListener(HandleChatUpdate);
            API.Matchmaking.Client.EventLobbyAuthenticationRequest.AddListener(HandleAuthRequest);
            API.Matchmaking.Client.EventLobbyChatMsg.AddListener(HandleChatMessage);
            API.Matchmaking.Client.EventLobbyInvite.AddListener(evtLobbyInvite.Invoke);
        }

        private void OnDisable()
        {
            API.Matchmaking.Client.EventLobbyAskedToLeave.RemoveListener(HandleAskedToLeave);
            API.Matchmaking.Client.EventLobbyDataUpdate.RemoveListener(HandleLobbyDataUpdate);
            API.Matchmaking.Client.EventLobbyLeave.RemoveListener(HandleLobbyLeave);
            API.Matchmaking.Client.EventLobbyGameCreated.RemoveListener(HandleGameServerSet);
            API.Matchmaking.Client.EventLobbyChatUpdate.RemoveListener(HandleChatUpdate);
            API.Matchmaking.Client.EventLobbyAuthenticationRequest.RemoveListener(HandleAuthRequest);
            API.Matchmaking.Client.EventLobbyChatMsg.RemoveListener(HandleChatMessage);
            API.Matchmaking.Client.EventLobbyInvite.RemoveListener(evtLobbyInvite.Invoke);
        }

        private void HandleChatMessage(LobbyChatMsg message)
        {
            if (message.lobby == Lobby)
            {
                evtChatMsgReceived.Invoke(message);
            }
        }

        private void HandleAuthRequest(LobbyData lobby, UserData sender, byte[] ticket, byte[] inventory)
        {
            if(lobby == Lobby)
            {
                Authentication.BeginAuthSession(ticket, sender, (session) =>
                {
                    evtAuthenticationSessionResult.Invoke(session, inventory);
                });
            }
        }

        private void HandleChatUpdate(LobbyChatUpdate_t arg0)
        {
            if(arg0.m_ulSteamIDLobby == Lobby)
            {
                var state = (EChatMemberStateChange)arg0.m_rgfChatMemberStateChange;
                if (state == EChatMemberStateChange.k_EChatMemberStateChangeEntered)
                    evtUserJoined?.Invoke(arg0.m_ulSteamIDUserChanged);
                else
                    evtUserLeft?.Invoke(new UserLobbyLeaveData { user = arg0.m_ulSteamIDUserChanged, state = state });
            }
        }

        private void HandleGameServerSet(LobbyGameCreated_t arg0)
        {
            if (arg0.m_ulSteamIDLobby == Lobby)
                evtGameCreated.Invoke(GameServer);
        }

        private void HandleLobbyLeave(LobbyData arg0)
        {
            if (arg0 == Lobby)
            {
                Lobby = default;
                evtLeave.Invoke();
            }
        }

        private void HandleAskedToLeave(LobbyData arg0)
        {
            if (arg0 == Lobby)
                evtAskedToLeave.Invoke();
        }

        private void HandleLobbyDataUpdate(LobbyDataUpdateEventData arg0)
        {
            if (arg0.lobby == Lobby)
                evtDataUpdated.Invoke(arg0);
        }

        /// <summary>
        /// Changes the type of the current lobby if any
        /// </summary>
        /// <remarks>
        /// This will also update the type in the <see cref="createArguments"/> record
        /// </remarks>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool SetType(ELobbyType type)
        {
            createArguments.type = type;
            return API.Matchmaking.Client.SetLobbyType(Lobby, type);
        }
        /// <summary>
        /// Sets the lobby joinable or not
        /// </summary>
        /// <param name="makeJoinable"></param>
        /// <returns></returns>
        public bool SetJoinable(bool makeJoinable) => API.Matchmaking.Client.SetLobbyJoinable(Lobby, makeJoinable);
        /// <summary>
        /// Searches for a match based on <see cref="searchArguments"/>, if none is found it will create a lobby matching the <see cref="createArguments"/>
        /// </summary>
        public void QuickMatch(bool createOnFail = true)
        {
            if (HasLobby)
            {
                Lobby.Leave();
                Lobby = CSteamID.Nil.m_SteamID;
            }

            API.Matchmaking.Client.AddRequestLobbyListDistanceFilter(searchArguments.distance);

            if (searchArguments.slots > 0)
                API.Matchmaking.Client.AddRequestLobbyListFilterSlotsAvailable(searchArguments.slots);

            foreach (var near in searchArguments.nearValues)
                API.Matchmaking.Client.AddRequestLobbyListNearValueFilter(near.key, near.value);

            foreach (var numeric in searchArguments.numericFilters)
                API.Matchmaking.Client.AddRequestLobbyListNumericalFilter(numeric.key, numeric.value, numeric.comparison);

            foreach (var text in searchArguments.stringFilters)
                API.Matchmaking.Client.AddRequestLobbyListStringFilter(text.key, text.value, text.comparison);

            API.Matchmaking.Client.AddRequestLobbyListResultCountFilter(1);

            API.Matchmaking.Client.RequestLobbyList((r, e) =>
            {
                if (!e && r.Length >= 1)
                {
                    API.Matchmaking.Client.JoinLobby(r[0], (r2, e2) =>
                    {
                        var response = r2.Response;

                        if (!e2 && response == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                        {
                            if (API.App.isDebugging)
                                Debug.Log("Quick match found, joined lobby: " + r2.Lobby.ToString());

                            Lobby = r2.Lobby;
                            evtFound?.Invoke(r);
                            evtEnterSuccess.Invoke(r[0]);
                        }
                        else
                        {
                            if (response == EChatRoomEnterResponse.k_EChatRoomEnterResponseLimited)
                            {
                                Debug.LogError("This user is limited and cannot create or join lobbies or chats.");
                                evtEnterFailed.Invoke(response);
                            }
                            else
                            {
                                if (createOnFail)
                                {
                                    if (API.App.isDebugging)
                                        Debug.LogError("Quick match failed, lobbies found but failed to join ... creating lobby.");

                                    Create();
                                }
                                else
                                    evtQuickMatchFailed.Invoke();
                            }
                        }
                    });
                }
                else
                {
                    if (createOnFail)
                    {
                        if (API.App.isDebugging)
                            Debug.Log("Quick match failed, no lobbies found ... creating lobby.");

                        Create();
                    }
                    else
                        evtQuickMatchFailed.Invoke();
                }
            });
        }
        /// <summary>
        /// Creates a new lobby with the data found in <see cref="createArguments"/>
        /// </summary>
        public void Create() => Create(null);
        public void Create(string name, ELobbyType type, int slots, params MetadataTemplate[] metadata)
        {
            createArguments.name = name;
            createArguments.type = type;
            createArguments.slots = slots;
            createArguments.metadata.Clear();
            createArguments.metadata.AddRange(metadata);
            createArguments.richPresenceFields.Clear();
            Create(null);
        }
        public void Create(ELobbyType type, int slots, params MetadataTemplate[] metadata)
        {
            createArguments.name = string.Empty;
            createArguments.type = type;
            createArguments.slots = slots;
            createArguments.metadata.Clear();
            createArguments.metadata.AddRange(metadata);
            createArguments.richPresenceFields.Clear();
            Create(null);
        }
        /// <summary>
        /// Creates a new lobby with the data found in <see cref="createArguments"/> and invokes the callback when complete
        /// </summary>
        public void Create(Action<EResult, LobbyData, bool> callback)
        {
            if (HasLobby)
            {
                Lobby.Leave();
                Lobby = CSteamID.Nil.m_SteamID;
            }

            API.Matchmaking.Client.CreateLobby(createArguments.type, createArguments.slots, (result, lobby, ioError) =>
            {
                if (!ioError)
                {
                    if (result == EResult.k_EResultOK)
                    {
                        if (API.App.isDebugging)
                            Debug.Log("New lobby created.");

                        Lobby = lobby;

                        if (createArguments.usageHint == CreateArguments.UseHintOptions.Group)
                            lobby.IsGroup = true;
                        else if (createArguments.usageHint == CreateArguments.UseHintOptions.Session)
                            lobby.IsSession = true;

                        lobby[LobbyData.DataName] = createArguments.name;
                        foreach (var data in createArguments.metadata)
                        {
                            if (data.value.Contains("@[") && data.value.Contains("]"))
                            {
                                var workingString = data.value;
                                while (API.Utilities.FindToken("@[", "]", workingString, out var resultString))
                                {
                                    switch(resultString) 
                                    {
                                        case "@[userName]":
                                            workingString.Replace(resultString, UserData.Me.Name);
                                            break;
                                        case "@[userLevel]":
                                            workingString.Replace(resultString, UserData.Me.Level.ToString());
                                            break;
                                        default:
                                            workingString.Replace(resultString, "");
                                            break;
                                    }
                                }
                            }
                            else
                                lobby[data.key] = data.value;
                        }

                        if (createArguments != null
                        && createArguments.richPresenceFields != null)
                            foreach (var kvp in createArguments.richPresenceFields)
                            {
                                if (kvp.value.Contains("@[") && kvp.value.Contains("]"))
                                {
                                    var workingString = kvp.value;
                                    while (API.Utilities.FindToken("@[", "]", workingString, out var resultString))
                                    {
                                        switch (resultString)
                                        {
                                            case "@[lobbyId]":
                                                workingString.Replace(resultString, lobby.ToString());
                                                break;
                                            default:
                                                workingString.Replace(resultString, lobby[resultString[2..^1]]);
                                                break;
                                        }
                                    }
                                }
                                else
                                    Friends.Client.SetRichPresence(kvp.key, kvp.value);
                            }

                        evtCreated?.Invoke(lobby);
                    }
                    else
                    {
                        Debug.Log($"No lobby created Steam API response code: {result}");
                        evtCreateFailed?.Invoke(result);
                    }
                }
                else
                {
                    Debug.LogError("Lobby creation failed with message: IOFailure\nSteam API responded with a general IO Failure.");
                    evtCreateFailed?.Invoke(EResult.k_EResultIOFailure);
                }

                callback?.Invoke(result, lobby, ioError);
            });
        }
        /// <summary>
        /// Searches for lobbies that match the <see cref="searchArguments"/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Remember Lobbies are a matchmaking feature, the first lobby returned is generally he best, lobby search is not intended to return all possible results simply the best matching options.
        /// </para>
        /// </remarks>
        /// <param name="maxResults">The maximum number of lobbies to return. lower values are better.</param>
        public void Search(int maxResults)
        {
            if (maxResults <= 0)
                return;

            API.Matchmaking.Client.AddRequestLobbyListDistanceFilter(searchArguments.distance);

            if (searchArguments.slots > 0)
                API.Matchmaking.Client.AddRequestLobbyListFilterSlotsAvailable(searchArguments.slots);

            foreach (var near in searchArguments.nearValues)
                API.Matchmaking.Client.AddRequestLobbyListNearValueFilter(near.key, near.value);

            foreach (var numeric in searchArguments.numericFilters)
                API.Matchmaking.Client.AddRequestLobbyListNumericalFilter(numeric.key, numeric.value, numeric.comparison);

            foreach (var text in searchArguments.stringFilters)
                API.Matchmaking.Client.AddRequestLobbyListStringFilter(text.key, text.value, text.comparison);

            API.Matchmaking.Client.AddRequestLobbyListResultCountFilter(maxResults);

            API.Matchmaking.Client.RequestLobbyList((r, e) =>
            {
                if (!e)
                {
                    evtFound?.Invoke(r);
                }
                else
                {
                    evtFound?.Invoke(new LobbyData[0]);
                }
            });
        }
        /// <summary>
        /// Joins the indicated steam lobby
        /// </summary>
        /// <param name="lobby"></param>
        public void Join(LobbyData lobby)
        {
            if (HasLobby)
            {
                Lobby.Leave();
                Lobby = CSteamID.Nil.m_SteamID;
            }

            API.Matchmaking.Client.JoinLobby(lobby, (r, e) =>
            {
                if(!e)
                {
                    if (r.Response == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                    {
                        if (API.App.isDebugging)
                            Debug.Log("Joined lobby: " + lobby.ToString());

                        Lobby = r.Lobby;
                        evtEnterSuccess.Invoke(lobby);
                    }
                    else
                        evtEnterFailed.Invoke(r.Response);
                }
                else
                    evtEnterFailed.Invoke(EChatRoomEnterResponse.k_EChatRoomEnterResponseError);
            });
        }

        public void Join(ulong lobby)
        {
            if (HasLobby)
            {
                Lobby.Leave();
                Lobby = CSteamID.Nil.m_SteamID;
            }

            API.Matchmaking.Client.JoinLobby(lobby, (r, e) =>
            {
                if (!e)
                {
                    if (r.Response == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                    {
                        if (API.App.isDebugging)
                            Debug.Log("Joined lobby: " + lobby.ToString());

                        Lobby = r.Lobby;

                        if (richPresenceFields != null)
                            foreach (var kvp in richPresenceFields)
                            {
                                if (kvp.value.Contains("@[") && kvp.value.Contains("]"))
                                {
                                    var workingString = kvp.value;
                                    while (API.Utilities.FindToken("@[", "]", workingString, out var resultString))
                                    {
                                        switch (resultString)
                                        {
                                            case "@[lobbyId]":
                                                workingString.Replace(resultString, r.Lobby.ToString());
                                                break;
                                            default:
                                                workingString.Replace(resultString, r.Lobby[resultString[2..^1]]);
                                                break;
                                        }
                                    }
                                }
                                else
                                    Friends.Client.SetRichPresence(kvp.key, kvp.value);
                            }

                        evtEnterSuccess.Invoke(lobby);
                    }
                    else
                        evtEnterFailed.Invoke(r.Response);
                }
                else
                    evtEnterFailed.Invoke(EChatRoomEnterResponse.k_EChatRoomEnterResponseError);
            });
        }
        public void Join(string lobbyIdAsString)
        {
            var tLobby = LobbyData.Get(lobbyIdAsString);
            if (tLobby.IsValid)
            {
                if (HasLobby)
                {
                    Lobby.Leave();
                    Lobby = CSteamID.Nil.m_SteamID;
                }
                Join(tLobby);
            }
            else
                Debug.LogWarning(lobbyIdAsString + " is not a valid Lobby ID");
        }
        public void Leave()
        {
            Lobby.Leave();
            Lobby = CSteamID.Nil.m_SteamID;
        }
        public bool SetLobbyData(string key, string value) => API.Matchmaking.Client.SetLobbyData(Lobby, key, value);
        public void SetMemberData(string key, string value) => API.Matchmaking.Client.SetLobbyMemberData(Lobby, key, value);
        public LobbyMemberData GetLobbyMember(UserData member) => new LobbyMemberData { lobby = Lobby, user = member };
        public string GetLobbyData(string key) => API.Matchmaking.Client.GetLobbyData(Lobby, key);
        public string GetMemberData(UserData member, string key) => API.Matchmaking.Client.GetLobbyMemberData(Lobby, member, key);
        public bool IsMemberReady(UserData member) => API.Matchmaking.Client.GetLobbyMemberData(Lobby, member, LobbyData.DataReady) == "true";
        public void KickMember(UserData member) => Lobby.KickMember(member);
        public void Invite(UserData user) => API.Matchmaking.Client.InviteUserToLobby(Lobby, user);
        public void Invite(uint FriendId) => API.Matchmaking.Client.InviteUserToLobby(Lobby, UserData.Get(FriendId));
        public LobbyMemberData[] Members => API.Matchmaking.Client.GetLobbyMembers(Lobby);
        public void Authenticate(Action<AuthenticationTicket, bool> callback) => Lobby.Authenticate(callback);
        public bool Authenticate(LobbyAuthenticationData data) => Lobby.Authenticate(data);
        public bool SendChatMessage(string message) => Lobby.SendChatMessage(message);
        public bool SendChatMessage(byte[] data) => Lobby.SendChatMessage(data);
        public bool SendChatMessage(object jsonObject)
        {
            return SendChatMessage(System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(jsonObject)));
        }

        /// <summary>
        /// The same as SendChatMessage, this only exists for use in Unity Inspector where overloads don't play nice with some editor features
        /// </summary>
        /// <param name="message"></param>
        public void SendChatMessageString(string message) => SendChatMessage(message);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LobbyManager), true)]
    public class LobbyManagerEditor : Editor
    {
        SerializedProperty m_DelegatesProperty;
        SerializedProperty m_searchProperty;
        SerializedProperty m_createProperty;
        SerializedProperty m_richPresenceProperty;

        GUIContent m_IconToolbarMinus;
        GUIContent m_EventIDName;
        GUIContent[] m_EventTypes;
        GUIContent m_AddButtonContent;

        protected virtual void OnEnable()
        {
            m_DelegatesProperty = serializedObject.FindProperty("m_Delegates");
            m_searchProperty = serializedObject.FindProperty(nameof(LobbyManager.searchArguments));
            m_createProperty = serializedObject.FindProperty (nameof(LobbyManager.createArguments));
            m_richPresenceProperty = serializedObject.FindProperty(nameof(LobbyManager.richPresenceFields));
            m_AddButtonContent = new GUIContent("Add New Event Type");
            m_EventIDName = new GUIContent("");
            // Have to create a copy since otherwise the tooltip will be overwritten.
            m_IconToolbarMinus = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus"));
            m_IconToolbarMinus.tooltip = "Remove all events in this list.";

            string[] eventNames = Enum.GetNames(typeof(LobbyManager.ManagedLobbyEvents));
            m_EventTypes = new GUIContent[eventNames.Length];
            for (int i = 0; i < eventNames.Length; ++i)
            {
                m_EventTypes[i] = new GUIContent(eventNames[i]);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_searchProperty, true);
            EditorGUILayout.PropertyField(m_createProperty, true);
            EditorGUILayout.PropertyField(m_richPresenceProperty, true);

            int toBeRemovedEntry = -1;

            EditorGUILayout.Space();

            Vector2 removeButtonSize = GUIStyle.none.CalcSize(m_IconToolbarMinus);

            for (int i = 0; i < m_DelegatesProperty.arraySize; ++i)
            {
                SerializedProperty delegateProperty = m_DelegatesProperty.GetArrayElementAtIndex(i);
                m_EventIDName.text = delegateProperty.enumDisplayNames[delegateProperty.enumValueIndex];

                switch ((LobbyManager.ManagedLobbyEvents)delegateProperty.enumValueIndex)
                {
                    case LobbyManager.ManagedLobbyEvents.AuthenticationSessionResults:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtAuthenticationSessionResult)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.LobbyJoinFailure:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtEnterFailed)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.LobbyJoinSuccess:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtEnterSuccess)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.LobbyChatMessageReceived:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtChatMsgReceived)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.LobbyCreationFailed:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtCreateFailed)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.MetadataUpdated:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtDataUpdated)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.LobbyCreationSuccess:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtCreated)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.OtherUserJoined:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtUserJoined)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.OtherUserLeft:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtUserLeft)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.QuickMatchFailed:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtQuickMatchFailed)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.SearchResultsReady:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtFound)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.SessionConnectionUpdated:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtGameCreated)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.YouAreAskedToLeave:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtAskedToLeave)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.LobbyInviteReceived:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtLobbyInvite)), m_EventIDName);
                        break;
                    case LobbyManager.ManagedLobbyEvents.LobbyLeave:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LobbyManager.evtLeave)), m_EventIDName);
                        break;
                }

                Rect callbackRect = GUILayoutUtility.GetLastRect();

                Rect removeButtonPos = new Rect(callbackRect.xMax - removeButtonSize.x - 8, callbackRect.y + 1, removeButtonSize.x, removeButtonSize.y);
                if (GUI.Button(removeButtonPos, m_IconToolbarMinus, GUIStyle.none))
                {
                    toBeRemovedEntry = i;
                }

                EditorGUILayout.Space();
            }

            if (toBeRemovedEntry > -1)
            {
                RemoveEntry(toBeRemovedEntry);
            }

            Rect btPosition = GUILayoutUtility.GetRect(m_AddButtonContent, GUI.skin.button);
            const float addButtonWidth = 200f;
            btPosition.x = btPosition.x + (btPosition.width - addButtonWidth) / 2;
            btPosition.width = addButtonWidth;
            if (GUI.Button(btPosition, m_AddButtonContent))
            {
                ShowAddTriggerMenu();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveEntry(int toBeRemovedEntry)
        {
            m_DelegatesProperty.DeleteArrayElementAtIndex(toBeRemovedEntry);
        }

        void ShowAddTriggerMenu()
        {
            // Now create the menu, add items and show it
            GenericMenu menu = new GenericMenu();
            for (int i = 0; i < m_EventTypes.Length; ++i)
            {
                bool active = true;

                // Check if we already have a Entry for the current eventType, if so, disable it
                for (int p = 0; p < m_DelegatesProperty.arraySize; ++p)
                {
                    SerializedProperty delegateEntry = m_DelegatesProperty.GetArrayElementAtIndex(p);
                    if (delegateEntry.enumValueIndex == i)
                    {
                        active = false;
                    }
                }
                if (active)
                    menu.AddItem(m_EventTypes[i], false, OnAddNewSelected, i);
                else
                    menu.AddDisabledItem(m_EventTypes[i]);
            }
            menu.ShowAsContext();
            Event.current.Use();
        }

        private void OnAddNewSelected(object index)
        {
            int selected = (int)index;

            m_DelegatesProperty.arraySize += 1;
            SerializedProperty delegateEntry = m_DelegatesProperty.GetArrayElementAtIndex(m_DelegatesProperty.arraySize - 1);
            delegateEntry.enumValueIndex = selected;
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif