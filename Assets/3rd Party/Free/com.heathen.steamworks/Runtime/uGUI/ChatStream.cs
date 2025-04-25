#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEngine;
using Steamworks;
using System.Collections.Generic;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// Displays a flow of chat messages such as those received from the <see cref="ClanChatDirector"/> or <see cref="LobbyChatDirector"/>.
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/chat-stream")]
    public class ChatStream : MonoBehaviour
    {
        /// <summary>
        /// How many chat entries should the tool maintain. When the count exceeds this number the oldest message will be destroyed. This is important for managing total memory use and keeping Unity UI from getting out of hand with long or fast running chats.
        /// </summary>
        [Tooltip("How many message entries will be retained at a time.\nOldest entries are removed as this count is exceeded")]
        [SerializeField]
        private uint historyLength = 200;
        /// <summary>
        /// The root where chat messages will be spawned as they come in
        /// </summary>
        [Tooltip("The root object under which chat messages will be listed.")]
        [SerializeField]
        private Transform content;
        /// <summary>
        /// The object that will be cloned / instantiated for each received message. This should contain a component in the root of the template that implements the <see cref="IChatMessage"/> interface.
        /// </summary>
        [SerializeField]
        private GameObject messageTemplate;

        private UnityEngine.UI.ScrollRect scrollRect;
        private Queue<GameObject> messages = new Queue<GameObject>();

        private void OnEnable()
        {
            scrollRect = GetComponentInChildren<UnityEngine.UI.ScrollRect>();
        }
        /// <summary>
        /// Used by the <see cref="ClanChatDirector.evtReceived"/> to apply a clan chat message
        /// </summary>
        /// <param name="message"></param>
        public void HandleClanMessage(ClanChatMsg message)
        {
            var go = Instantiate(messageTemplate, content);
            var comp = go.GetComponent<IChatMessage>();
            comp.Initialize(message);

            messages.Enqueue(go);
            if(messages.Count > historyLength)
            {
                var target = messages.Dequeue();
                Destroy(target);
            }

            scrollRect.verticalNormalizedPosition = 0;
        }
        /// <summary>
        /// Used by the <see cref="LobbyManager.evtChatMsgReceived"/> to apply a lobby chat message
        /// </summary>
        /// <param name="message"></param>
        public void HandleLobbyMessage(LobbyChatMsg message)
        {
            var go = Instantiate(messageTemplate, content);
            var comp = go.GetComponent<IChatMessage>();
            comp.Initialize(message);

            messages.Enqueue(go);
            if (messages.Count > historyLength)
            {
                var target = messages.Dequeue();
                Destroy(target);
            }

            scrollRect.verticalNormalizedPosition = 0;
        }
        /// <summary>
        /// Generic message handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public void HandleMessage(UserData sender, string message, EChatEntryType type)
        {
            var go = Instantiate(messageTemplate, content);
            var comp = go.GetComponent<IChatMessage>();
            comp.Initialize(sender, message, type);

            messages.Enqueue(go);
            if (messages.Count > historyLength)
            {
                var target = messages.Dequeue();
                Destroy(target);
            }

            scrollRect.verticalNormalizedPosition = 0;
        }
    }
}
#endif