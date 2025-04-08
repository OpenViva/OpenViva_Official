#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heathen.SteamworksIntegration.UI
{
    [Obsolete("Features merged into LobbyManager")]
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/lobby-chat-director")]
    [RequireComponent(typeof(LobbyManager))]
    public class LobbyChatDirector : MonoBehaviour
    {
        private LobbyManager manager;

        public LobbyChatMsgEvent evtMessageRecieved;

        public bool HasLobby => manager != null && manager.HasLobby;

        public bool Send(string message) => manager.Lobby.SendChatMessage(message);
        public bool Send(byte[] data) => manager.Lobby.SendChatMessage(data);
        public bool Send(object jsonObject)
        {
            return Send(System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(jsonObject)));
        }

        /// <summary>
        /// The same as Send, this only exsits for use in Unity Inspector
        /// </summary>
        /// <param name="message"></param>
        public void SendString(string message) => Send(message);

        private void Awake()
        {
            manager = GetComponent<LobbyManager>();
        }

        private void Start()
        {
            API.Matchmaking.Client.EventLobbyChatMsg.AddListener(HandleChatMessage);
        }

        private void HandleChatMessage(LobbyChatMsg message)
        {
            if (message.lobby == manager.Lobby)
            {
                evtMessageRecieved.Invoke(message);
            }
        }
    }
}
#endif