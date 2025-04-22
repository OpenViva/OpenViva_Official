#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
#endif

namespace Heathen.SteamworksIntegration.UI
{
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/quick-match-lobby-control")]
    public class QuickMatchLobbyControl : MonoBehaviour
    {
        public enum Status
        {
            Idle,
            Searching,
            WaitingForStart,
            Starting
        }

        [Header("UI Settings")]
        [Tooltip("Enabled when the control is not searching for a lobby or waiting for a lobby to fill")]
        public GameObject idleGroup;
        [Tooltip("Enabled when the control is search for a lobby or waiting for a lobby to fill")]
        public GameObject processingGroup;

        [Header("Lobby Management")]
        public bool updateRichPresenceGroupData = true;
        public EAuthSessionResponse[] kickWhen = new EAuthSessionResponse[]
            {
                EAuthSessionResponse.k_EAuthSessionResponseAuthTicketCanceled,
                EAuthSessionResponse.k_EAuthSessionResponseAuthTicketInvalid,
                EAuthSessionResponse.k_EAuthSessionResponseAuthTicketInvalidAlreadyUsed,
                EAuthSessionResponse.k_EAuthSessionResponseLoggedInElseWhere,
                EAuthSessionResponse.k_EAuthSessionResponseNoLicenseOrExpired,
                EAuthSessionResponse.k_EAuthSessionResponsePublisherIssuedBan,
                EAuthSessionResponse.k_EAuthSessionResponseUserNotConnectedToSteam,
                EAuthSessionResponse.k_EAuthSessionResponseVACBanned,
            };
        public SearchArguments searchArguments = new();
        public CreateArguments createArguments = new();

        /// <summary>
        /// Raised when the lobby is first filled to capacity
        /// </summary>
        [Header("Events")]
        public UnityEvent evtProcessStarted;
        public UnityEvent evtProcessStopped;
        public LobbyDataEvent evtLobbyFull;
        /// <summary>
        /// Occurs when the <see cref="GameServer"/> information is first set on the lobby
        /// </summary>
        public GameServerSetEvent evtGameCreated;
        /// <summary>
        /// Occurs when the local user enters a lobby rather they joined or created it them selves
        /// </summary>
        public LobbyDataEvent evtEnterSuccess;
        /// <summary>
        /// Occurs when the local user tried but failed to enter a lobby
        /// </summary>
        public LobbyResponceEvent evtEnterFailed;
        /// <summary>
        /// Occurs when the local user tried but failed to create a lobby
        /// </summary>
        public EResultEvent evtCreateFailed;
        /// <summary>
        /// Occurs when any state changes on the lobby, this includes people coming and going, succeeding or failing authentication or any other lobby data function.
        /// </summary>
        public UnityEvent evtStateChanged;

        public LobbyData Lobby
        {
            get;
            set;
        }
        public LobbyMemberData Owner => Lobby.Owner;
        public LobbyMemberData Me => Lobby.Me;
        public bool HasLobby => Lobby != CSteamID.Nil.m_SteamID && SteamMatchmaking.GetNumLobbyMembers(Lobby) > 0;
        public bool Searching { get; private set; }
        public bool IsPlayerOwner => HasLobby && Lobby.IsOwner;
        public bool AllPlayersReady => HasLobby && Lobby.AllPlayersReady;
        public bool IsPlayerReady
        {
            get => API.Matchmaking.Client.GetLobbyMemberData(Lobby, API.User.Client.Id, LobbyData.DataReady) == "true";
            set => API.Matchmaking.Client.SetLobbyMemberData(Lobby, LobbyData.DataReady, value.ToString().ToLower());
        }
        public bool Full => HasLobby && Lobby.Full;
        public int Slots => HasLobby ? SteamMatchmaking.GetLobbyMemberLimit(Lobby) : 0;
        public int MemberCount => HasLobby ? SteamMatchmaking.GetNumLobbyMembers(Lobby) : 0;
        public LobbyGameServer GameServer => HasLobby ? Lobby.GameServer : default;
        public Status WorkingStatus
        {
            get
            {
                if (!HasLobby && !Searching)
                    return Status.Idle;
                else if (Searching)
                    return Status.Searching;
                else if (HasLobby && !Lobby.HasServer)
                    return Status.WaitingForStart;
                else
                    return Status.Starting;
            }
        }
        public float Timer => Time.unscaledTime - enterTime;

        private bool cancelRequest = false;
        private float enterTime = 0;
        private ulong filledId = 0;

        private void Start()
        {
            if (LobbyData.SessionLobby(out var lobby))
                Lobby = lobby;

            
            API.Matchmaking.Client.EventLobbyChatMsg.AddListener(HandleChatMessage);
            API.Matchmaking.Client.EventLobbyEnterSuccess.AddListener(HandleLobbyEnterSuccess);
            API.Matchmaking.Client.EventLobbyAskedToLeave.AddListener(HandleLobbyKickRequest);
            API.Matchmaking.Client.EventLobbyDataUpdate.AddListener(HandleLobbyDataUpdated);
            API.Matchmaking.Client.EventLobbyChatUpdate.AddListener(HandleChatUpdate);
            API.Matchmaking.Client.EventLobbyGameCreated.AddListener(HandleGameServerSet);

            RefreshUI();
        }

        private void OnDestroy()
        {
            API.Matchmaking.Client.EventLobbyChatMsg.RemoveListener(HandleChatMessage);
            API.Matchmaking.Client.EventLobbyEnterSuccess.RemoveListener(HandleLobbyEnterSuccess);
            API.Matchmaking.Client.EventLobbyAskedToLeave.RemoveListener(HandleLobbyKickRequest);
            API.Matchmaking.Client.EventLobbyDataUpdate.RemoveListener(HandleLobbyDataUpdated);
            API.Matchmaking.Client.EventLobbyChatUpdate.RemoveListener(HandleChatUpdate);
            API.Matchmaking.Client.EventLobbyGameCreated.RemoveListener(HandleGameServerSet);
        }

        private void Update()
        {
            if(HasLobby 
                && filledId != Lobby
                && Lobby.Full)
            {
                filledId = Lobby;
                evtLobbyFull.Invoke(Lobby);
            }
        }

        private void HandleChatUpdate(LobbyChatUpdate_t arg0)
        {
            if (arg0.m_ulSteamIDLobby == Lobby)
            {
                var state = (EChatMemberStateChange)arg0.m_rgfChatMemberStateChange;
                if (state == EChatMemberStateChange.k_EChatMemberStateChangeEntered)
                {
                    API.Friends.Client.SetPlayedWith(arg0.m_ulSteamIDUserChanged);
                    evtStateChanged.Invoke();
                }
            }
        }

        private void HandleLobbyKickRequest(LobbyData arg0)
        {
            if (arg0 == Lobby)
            {
                Debug.LogWarning($"We have been asked to leave the lobby, this usually happens when we fail authentication with the lobby Owner.");
                Lobby.Leave();
                Lobby = default;
                RefreshUI();
            }
        }

        private void HandleLobbyDataUpdated(LobbyDataUpdateEventData arg0)
        {
            if (arg0.lobby == Lobby)
                RefreshUI();
        }

        [Serializable]
        private struct AuthMessage
        {
            public int valid;
            public byte[] data;
        }

        private void HandleLobbyEnterSuccess(LobbyEnter_t arg0)
        {
            LobbyData nLobby = arg0.m_ulSteamIDLobby;
            if (nLobby.IsSession)
            {
                enterTime = Time.unscaledTime;
                filledId = 0;
                Lobby = nLobby;

                if (LobbyData.GroupLobby(out var lobby))
                    lobby.SendChatMessage("[SessionId]" + Lobby.ToString());

                RefreshUI();

                if(!IsPlayerOwner)
                {
                    API.Authentication.GetAuthSessionTicket(Lobby.Owner.user, (ticket, error) =>
                    {
                        if (!error)
                        {

                            Lobby.SendChatMessage(new AuthMessage
                            {
                                valid = 1,
                                data = ticket.Data
                            });
                        }
                    });
                }
            }
        }

        private void HandleChatMessage(LobbyChatMsg message)
        {
            if (message.lobby == Lobby
                && kickWhen.Length > 0
                && IsPlayerOwner)
            {
                if (message.TryFromJson<AuthMessage>(out var authMessage)
                    && authMessage.valid == 1)
                {
                    API.Authentication.BeginAuthSession(authMessage.data, message.sender, data =>
                    {
                        if (data.User != message.sender
                        || kickWhen.Contains(data.Response))
                        {
                            Debug.LogWarning($"{message.sender.Nickname} failed authentication with state {data.Response} and is being asked to leave.");
                            Lobby.KickMember(data.User);
                        }
                        data.End();
                    });
                }
            }
        }

        private void HandleGameServerSet(LobbyGameCreated_t arg0)
        {
            if (arg0.m_ulSteamIDLobby == Lobby)
            {
                evtGameCreated.Invoke(Lobby.GameServer);
                evtStateChanged.Invoke();
            }
        }

        private void RefreshUI()
        {
            if (!HasLobby)
            {
                if (updateRichPresenceGroupData)
                {
                    UserData.SetRichPresence("steam_player_group", string.Empty);
                    UserData.SetRichPresence("steam_player_group_size", string.Empty);
                }

                if(processingGroup.activeSelf)
                {
                    idleGroup.SetActive(true);
                    processingGroup.SetActive(false);
                    evtProcessStopped.Invoke();
                }
            }
            else
            {
                if (updateRichPresenceGroupData)
                {
                    UserData.SetRichPresence("steam_player_group", Lobby.ToString());
                    UserData.SetRichPresence("steam_player_group_size", (createArguments.slots).ToString());
                }

                if (!processingGroup.activeSelf)
                {
                    idleGroup.SetActive(false);
                    processingGroup.SetActive(true);
                    evtProcessStarted.Invoke();
                }
            }

            evtStateChanged.Invoke();
        }

        public void Cancel()
        {
            if (Searching)
                cancelRequest = true;

            Searching = false;

            if (HasLobby)
            {
                Lobby.Leave();
                Lobby = default;
            }

            idleGroup.SetActive(true);
            processingGroup.SetActive(false);
            evtProcessStopped.Invoke();
            evtStateChanged.Invoke();
        }

        public void RunQuickMatch()
        {
            if (LobbyData.GroupLobby(out LobbyData partyLobby)
               && !partyLobby.IsOwner)
                return;

            if(!HasLobby && !Searching)
            {
                idleGroup.SetActive(false);
                processingGroup.SetActive(true);
                evtProcessStarted.Invoke();

                filledId = 0;
                Searching = true;
                API.Matchmaking.Client.AddRequestLobbyListDistanceFilter(searchArguments.distance);

                if (LobbyData.GroupLobby(out var groupLobby))
                    API.Matchmaking.Client.AddRequestLobbyListFilterSlotsAvailable(SteamMatchmaking.GetNumLobbyMembers(groupLobby));

                foreach (var near in searchArguments.nearValues)
                    API.Matchmaking.Client.AddRequestLobbyListNearValueFilter(near.key, near.value);

                foreach (var numeric in searchArguments.numericFilters)
                    API.Matchmaking.Client.AddRequestLobbyListNumericalFilter(numeric.key, numeric.value, numeric.comparison);

                foreach (var text in searchArguments.stringFilters)
                    API.Matchmaking.Client.AddRequestLobbyListStringFilter(text.key, text.value, text.comparison);

                API.Matchmaking.Client.AddRequestLobbyListResultCountFilter(1);

                API.Matchmaking.Client.RequestLobbyList((r, e) =>
                {
                    if (cancelRequest)
                    {
                        cancelRequest = false;
                        return;
                    }

                    if (!e && r.Length >= 1)
                    {
                        Searching = false;
                        API.Matchmaking.Client.JoinLobby(r[0], (r2, e2) =>
                        {
                            var response = r2.Response;

                            if (!e2 && response == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                            {
                                if(cancelRequest)
                                {
                                    r[0].Leave();
                                    cancelRequest = false;
                                    return;
                                }

                                if (API.App.isDebugging)
                                    Debug.Log("Quick match found, joined lobby: " + r2.Lobby.ToString());

                                Lobby = r2.Lobby;
                                evtEnterSuccess.Invoke(r[0]);
                                evtStateChanged.Invoke();
                            }
                            else
                            {
                                if (cancelRequest)
                                {
                                    cancelRequest = false;
                                    return;
                                }

                                if (response == EChatRoomEnterResponse.k_EChatRoomEnterResponseLimited)
                                {
                                    Debug.LogError("This user is limited and cannot create or join lobbies or chats.");
                                    evtEnterFailed.Invoke(response);
                                }
                                else
                                {
                                    Debug.LogError("Quick match failed, lobbies found but failed to join ... creating lobby.");
                                    evtEnterFailed.Invoke(response);
                                }
                            }
                        });
                    }
                    else
                    {
                        API.Matchmaking.Client.CreateLobby(createArguments.type, createArguments.slots, (result, lobby, ioError) =>
                        {
                            Searching = false;
                            if (!ioError)
                            {
                                if (cancelRequest)
                                {
                                    lobby.Leave();
                                    cancelRequest = false;
                                    return;
                                }

                                if (result == EResult.k_EResultOK)
                                {
                                    if (API.App.isDebugging)
                                        Debug.Log("New lobby created.");

                                    Lobby = lobby;
                                    lobby.IsSession = true;

                                    lobby[LobbyData.DataName] = createArguments.name;
                                    foreach (var data in createArguments.metadata)
                                        lobby[data.key] = data.value;

                                    evtEnterSuccess.Invoke(lobby);
                                    evtStateChanged.Invoke();
                                }
                                else
                                {
                                    Debug.Log($"No lobby created Steam API response code: {result}");
                                    evtCreateFailed?.Invoke(result);
                                    evtStateChanged.Invoke();
                                }
                            }
                            else
                            {
                                if (cancelRequest)
                                {
                                    cancelRequest = false;
                                    return;
                                }

                                Debug.LogError("Lobby creation failed with message: IOFailure\nSteam API responded with a general IO Failure.");
                                evtCreateFailed?.Invoke(EResult.k_EResultIOFailure);
                                evtStateChanged.Invoke();
                            }
                        });
                    }
                });
            }
        }
        public void SetGameServer() => Lobby.SetGameServer();
        public void SetGameServer(string address, ushort port, CSteamID gameServerId) => Lobby.SetGameServer(address, port, gameServerId);
        public void SetGameServer(string address, ushort port) => Lobby.SetGameServer(address, port);
        public void SetGameServer(CSteamID gameServerId) => Lobby.SetGameServer(gameServerId);
    }
}
#endif