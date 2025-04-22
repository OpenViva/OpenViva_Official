#if !DISABLESTEAMWORKS && STEAMWORKSNET
using Heathen.SteamworksIntegration;
using Heathen.SteamworksIntegration.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Heathen.DEMO
{
    [RequireComponent(typeof(LobbyManager))]
    public class LobbyChatUILogic : MonoBehaviour
    {
        [SerializeField]
        private int maxMessages = 200;
        [SerializeField]
        private GameObject chatPanel;
        [SerializeField]
        private TMP_InputField inputField;
        [SerializeField]
        private ScrollRect scrollView;
        [SerializeField]
        private Transform messageRoot;
        [SerializeField]
        private GameObject myChatTemplate;
        [SerializeField]
        private GameObject theirChatTemplate;

        private LobbyManager lobbyManager;
        private readonly List<IChatMessage> chatMessages = new List<IChatMessage>();

        private void Start()
        {
            lobbyManager = GetComponent<LobbyManager>();

            lobbyManager.evtChatMsgReceived.AddListener(HandleChatMessage);
        }

        private void OnDestroy()
        {
            lobbyManager.evtChatMsgReceived.RemoveListener(HandleChatMessage);
        }

        private void Update()
        {
            //Show or hide the chat panel based on rather or not we have a lobby
            if(lobbyManager.HasLobby
                && !chatPanel.activeSelf)
                chatPanel.SetActive(true);
            else if (!lobbyManager.HasLobby
                && chatPanel.activeSelf)
                chatPanel.SetActive(false);

            if (EventSystem.current.currentSelectedGameObject == inputField.gameObject
#if ENABLE_INPUT_SYSTEM
                && (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame
                || UnityEngine.InputSystem.Keyboard.current.numpadEnterKey.wasPressedThisFrame)
#else
                && (Input.GetKeyDown(KeyCode.Return)
                    || Input.GetKeyDown(KeyCode.KeypadEnter))
#endif
                )
            {
                OnSendChatMessage();
            }
        }

        private void HandleChatMessage(LobbyChatMsg message)
        {
            if (message.lobby == lobbyManager.Lobby)
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

        private void OnSendChatMessage()
        {
            if (lobbyManager.HasLobby
                && !string.IsNullOrEmpty(inputField.text))
            {
                lobbyManager.Lobby.SendChatMessage(inputField.text);
                inputField.text = string.Empty;
                StartCoroutine(SelectInputField());
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
    }
}
#endif