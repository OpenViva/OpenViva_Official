#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Heathen.SteamworksIntegration.UI
{
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/party-lobby-control")]
    public class PartyLobbyControl : MonoBehaviour
    {
        [Header("Local User Features")]
        public GameObject userOwnerPip;
        public Button readyButton;
        public Button notReadyButton;
        public Button leaveButton;

        [Header("Configuration")]
        public bool autoJoinOnInvite = false;
        public RectTransform invitePanel;
        public FriendInviteDropDown inviteDropdown;
        public LobbyMemberSlot[] slots;
        public bool updateRichPresenceGroupData = true;

        [Header("Chat")]
        public int maxMessages = 200;
        public GameObject chatPanel;
        public TMP_InputField inputField;
        public ScrollRect scrollView;
        public Transform messageRoot;
        public GameObject myChatTemplate;
        public GameObject theirChatTemplate;

        [Header("Events")]
        public LobbyDataEvent evtSessionLobbyInvite;
        public GameLobbyJoinRequestedEvent evtGroupLobbyInvite;

        public LobbyData Lobby
        {
            get;
            set;
        }
        public bool HasLobby => Lobby != CSteamID.Nil.m_SteamID && SteamMatchmaking.GetNumLobbyMembers(Lobby) > 0;
        public bool IsPlayerOwner => Lobby.IsOwner;
        public bool AllPlayersReady => Lobby.AllPlayersReady;
        public bool IsPlayerReady
        {
            get => API.Matchmaking.Client.GetLobbyMemberData(Lobby, API.User.Client.Id, LobbyData.DataReady) == "true";
            set => API.Matchmaking.Client.SetLobbyMemberData(Lobby, LobbyData.DataReady, value.ToString().ToLower());
        }

        private readonly List<IChatMessage> chatMessages = new List<IChatMessage>();
        private LobbyData inviteLobbyData;
        private LobbyData loadingLobbyData;
        private UserData groupInviteFrom;
        private Canvas canvas;

        private void Start()
        {
            canvas = GetComponentInParent<Canvas>();
            inviteDropdown.Invited.AddListener(InvitedUserToLobby);
            
            if (readyButton != null)
                readyButton.onClick.AddListener(HandleReadyClicked);
            
            if (notReadyButton != null)
                notReadyButton.onClick.AddListener(HandleNotReadyClicked);

            leaveButton.onClick.AddListener(HandleLeaveClicked);

            var group = API.Matchmaking.Client.memberOfLobbies.FirstOrDefault(p => p.IsGroup);
            if (group.IsValid)
                Lobby = group;

            API.Overlay.Client.EventGameLobbyJoinRequested.AddListener(HandleLobbyJoinRequest);
            API.Matchmaking.Client.EventLobbyChatMsg.AddListener(HandleChatMessage);
            API.Matchmaking.Client.EventLobbyEnterSuccess.AddListener(HandleLobbyEnterSuccess);
            API.Matchmaking.Client.EventLobbyAskedToLeave.AddListener(HandleLobbyKickRequest);
            API.Matchmaking.Client.EventLobbyDataUpdate.AddListener(HandleLobbyDataUpdated);
            API.Matchmaking.Client.EventLobbyChatUpdate.AddListener(HandleChatUpdate);

            if (API.App.Initialized)
                RefreshUI();
            else
                API.App.evtSteamInitialized.AddListener(HandleSteamInitialization);
        }

        private void HandleSteamInitialization()
        {
            RefreshUI();
            API.App.evtSteamInitialized.RemoveListener(HandleSteamInitialization);
        }

        private void OnDestroy()
        {
            API.Overlay.Client.EventGameLobbyJoinRequested.RemoveListener(HandleLobbyJoinRequest);
            API.Matchmaking.Client.EventLobbyChatMsg.RemoveListener(HandleChatMessage);
            API.Matchmaking.Client.EventLobbyEnterSuccess.RemoveListener(HandleLobbyEnterSuccess);
            API.Matchmaking.Client.EventLobbyAskedToLeave.RemoveListener(HandleLobbyKickRequest);
            API.Matchmaking.Client.EventLobbyDataUpdate.RemoveListener(HandleLobbyDataUpdated);
            API.Matchmaking.Client.EventLobbyChatUpdate.RemoveListener(HandleChatUpdate);
        }

        private void Update()
        {
            if (invitePanel.gameObject.activeSelf
                && !inviteDropdown.IsExpanded
                && ((
#if ENABLE_INPUT_SYSTEM   
                Mouse.current.leftButton.wasPressedThisFrame
                && !RectTransformUtility.RectangleContainsScreenPoint(invitePanel, Mouse.current.position.ReadValue(), canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null)
#else
                Input.GetMouseButtonDown(0)
                && !RectTransformUtility.RectangleContainsScreenPoint(invitePanel, Input.mousePosition, canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null)
#endif
                )
                ||
#if ENABLE_INPUT_SYSTEM
                Keyboard.current.escapeKey.wasPressedThisFrame
#else
                Input.GetKeyDown(KeyCode.Escape)
#endif
                ))
            {
                //And if so then we hide the panel and clear the to invite text field
                inviteDropdown.gameObject.SetActive(false);
                inviteDropdown.InputText = string.Empty;
            }

            if (EventSystem.current.currentSelectedGameObject == inputField.gameObject
#if ENABLE_INPUT_SYSTEM
                && (Keyboard.current.enterKey.wasPressedThisFrame
                || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
#else
                && (Input.GetKeyDown(KeyCode.Return)
                    || Input.GetKeyDown(KeyCode.KeypadEnter))
#endif
                )
            {
                OnSendChatMessage();
            }
        }

        private void HandleChatUpdate(LobbyChatUpdate_t arg0)
        {
            if (arg0.m_ulSteamIDLobby == Lobby)
            {
                var state = (EChatMemberStateChange)arg0.m_rgfChatMemberStateChange;
                if (state == EChatMemberStateChange.k_EChatMemberStateChangeEntered)
                    API.Friends.Client.SetPlayedWith(arg0.m_ulSteamIDUserChanged);

                RefreshUI();
            }
        }

        private void OnSendChatMessage()
        {
            if(HasLobby
                && !string.IsNullOrEmpty(inputField.text))
            {
                Lobby.SendChatMessage(inputField.text);
                inputField.text = string.Empty;
                StartCoroutine(SelectInputField());
            }
        }

        private void HandleLeaveClicked()
        {
            if (HasLobby)
            {
                Lobby.Leave();
                Lobby = default;
                RefreshUI();
            }
        }

        private void HandleNotReadyClicked()
        {
            IsPlayerReady = false;
            RefreshUI();
        }

        private void HandleReadyClicked()
        {
            IsPlayerReady = true;
            RefreshUI();
        }

        private void HandleLobbyDataUpdated(LobbyDataUpdateEventData arg0)
        {
            if (arg0.lobby == Lobby)
                RefreshUI();
            else if (arg0.lobby == inviteLobbyData
                && inviteLobbyData.IsGroup)
            {
                if (autoJoinOnInvite)
                {
                    if (HasLobby && Lobby != inviteLobbyData)
                    {
                        Lobby.Leave();
                    }

                    Lobby = inviteLobbyData;
                    inviteLobbyData.Join((result, error) =>
                    {
                        if (result.Response == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                            RefreshUI();
                        else
                        {
                            inviteLobbyData.Leave();
                            Lobby = default;
                        }
                    });
                }

                evtGroupLobbyInvite?.Invoke(loadingLobbyData, groupInviteFrom);
            }
            else if (arg0.lobby == loadingLobbyData
                && loadingLobbyData.IsSession)
            {
                if (LobbyData.SessionLobby(out var session))
                {
                    if (session != loadingLobbyData)
                    {
                        session.Leave();

                        loadingLobbyData.Join((result, error) =>
                        {
                            if (result.Response == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                            {
                                RefreshUI();
                                evtSessionLobbyInvite.Invoke(loadingLobbyData);
                            }
                            else
                            {
                                loadingLobbyData.Leave();
                            }
                        });
                    }
                }
                else
                {
                    loadingLobbyData.Join((result, error) =>
                    {
                        if (result.Response == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                        {
                            RefreshUI();
                            evtSessionLobbyInvite.Invoke(loadingLobbyData);
                        }
                        else
                        {
                            loadingLobbyData.Leave();
                        }
                    });
                }
                loadingLobbyData = default;
            }
        }

        public void InvitedUserToLobby(UserData user)
        {
            if(!HasLobby)
            {
                API.Matchmaking.Client.CreateLobby(ELobbyType.k_ELobbyTypeInvisible, slots.Length + 1, (result, lobby, error) =>
                {
                    if(result == EResult.k_EResultOK && !error)
                    {
                        lobby.IsGroup = true;
                        Lobby = lobby;
                        Lobby.InviteUserToLobby(user);
                    }
                });
            }
            else
            {
                Lobby.InviteUserToLobby(user);
            }
        }

        private void HandleLobbyKickRequest(LobbyData arg0)
        {
            if(arg0 == Lobby)
            {
                Lobby.Leave();
                Lobby = default;
                RefreshUI();
            }
        }

        private void HandleLobbyJoinRequest(LobbyData lobby, UserData user)
        {
            inviteLobbyData = lobby;
            groupInviteFrom = user;
            API.Matchmaking.Client.RequestLobbyData(lobby);
        }

        private void HandleLobbyEnterSuccess(LobbyEnter_t arg0)
        {
            LobbyData nLobby = arg0.m_ulSteamIDLobby;
            if (nLobby.IsGroup)
            {
                Lobby = nLobby;
                RefreshUI();
            }
        }

        private void HandleChatMessage(LobbyChatMsg message)
        {
            if (message.lobby == Lobby)
            {
                if (message.Message.StartsWith("[SessionId]"))
                {
                    if(ulong.TryParse(message.Message.Substring(11), out ulong steamID))
                    {
                        loadingLobbyData = steamID;
                        API.Matchmaking.Client.RequestLobbyData(loadingLobbyData);
                    }
                }
                else
                {
                    if (chatMessages.Count == maxMessages)
                    {
                        Destroy(chatMessages[0].GameObject);
                        chatMessages.RemoveAt(0);
                    }

                    if (message.sender == UserData.Me)
                    {
                        var go = Instantiate(myChatTemplate, messageRoot);
                        go.transform.SetAsLastSibling();
                        var cmsg = go.GetComponent<IChatMessage>();
                        if (cmsg != null)
                        {
                            cmsg.Initialize(message);
                            if (chatMessages.Count > 0
                                && chatMessages[chatMessages.Count - 1].User == cmsg.User)
                                cmsg.IsExpanded = false;

                            chatMessages.Add(cmsg);
                        }
                    }
                    else
                    {
                        var go = Instantiate(theirChatTemplate, messageRoot);
                        go.transform.SetAsLastSibling();
                        var cmsg = go.GetComponent<IChatMessage>();
                        if (cmsg != null)
                        {
                            cmsg.Initialize(message);
                            if (chatMessages[chatMessages.Count - 1].User == cmsg.User)
                                cmsg.IsExpanded = false;

                            chatMessages.Add(cmsg);
                        }
                    }

                    StartCoroutine(ForceScrollDown());
                }
            }
        }

        IEnumerator SelectInputField()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            inputField.Select();
        }
        /// <summary>
        /// Called when a new chat message is added to the UI to force the system to scroll to the bottom
        /// </summary>
        /// <returns></returns>
        IEnumerator ForceScrollDown()
        {
            // Wait for end of frame AND force update all canvases before setting to bottom.
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            scrollView.verticalNormalizedPosition = 0f;
        }

        public void RefreshUI()
        {
            if (!HasLobby)
            {
                if (updateRichPresenceGroupData)
                {
                    UserData.SetRichPresence("steam_player_group", string.Empty);
                    UserData.SetRichPresence("steam_player_group_size", string.Empty);
                }

                foreach (var slot in slots)
                {
                    slot.ClearUser();
                    slot.Interactable = true;
                }

                userOwnerPip.SetActive(false);

                if (readyButton != null)
                    readyButton.gameObject.SetActive(false);
                
                if (notReadyButton != null)
                    notReadyButton.gameObject.SetActive(false);
                
                leaveButton.gameObject.SetActive(false);
                chatPanel.SetActive(false);
            }
            else
            {
                if (updateRichPresenceGroupData)
                {
                    UserData.SetRichPresence("steam_player_group", Lobby.ToString());
                    UserData.SetRichPresence("steam_player_group_size", (slots.Length + 1).ToString());
                }

                leaveButton.gameObject.SetActive(true);
                userOwnerPip.SetActive(IsPlayerOwner);
                
                if (readyButton != null)
                    readyButton.gameObject.SetActive(!IsPlayerReady);
                
                if (notReadyButton != null)
                    notReadyButton.gameObject.SetActive(IsPlayerReady);

                var members = Lobby.Members;
                if (members.Length > 1)
                {
                    chatPanel.SetActive(true);
                }
                else
                {
                    chatPanel.SetActive(false);
                }

                members = members.Where(p => p.user != UserData.Me).ToArray();

                for (int i = 0; i < slots.Length; i++)
                {
                    var slot = slots[i];
                    slot.Interactable = Lobby.IsOwner;
                    if (members.Length > i)
                        slot.SetUser(members[i]);
                    else
                        slot.ClearUser();
                }
            }
        }
    }
}
#endif