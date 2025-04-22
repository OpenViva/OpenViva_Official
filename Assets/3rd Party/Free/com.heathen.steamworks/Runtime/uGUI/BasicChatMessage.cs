#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEngine;
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// A simple chat message UI behaviour
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/for-unity-game-engine/interfaces/ichatmessage")]
    public class BasicChatMessage : MonoBehaviour, IChatMessage
    {
        [SerializeField]
        private SetUserAvatar avatar;
        [SerializeField]
        private SetUserName username;
        [SerializeField]
        private string dateTimeFormat = "HH:mm:ss";
        [SerializeField]
        private TMPro.TextMeshProUGUI datetime;
        [SerializeField]
        private TMPro.TextMeshProUGUI message;
        
        /// <summary>
        /// The user this message is from
        /// </summary>
        public UserData User { get; private set; }
        /// <summary>
        /// The source chat data for this message
        /// </summary>
        public byte[] Data { get; private set; }
        /// <summary>
        /// The string parse of this message
        /// </summary>
        public string Message { get; private set; }
        /// <summary>
        /// The time stamp this message was received at
        /// </summary>
        public DateTime ReceivedAt { get; private set; }
        /// <summary>
        /// The type of message this is
        /// </summary>
        public EChatEntryType Type { get; private set; }
        /// <summary>
        /// Is this message expanded
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                if (avatar != null)
                    return avatar.gameObject.activeSelf;
                else if (datetime != null)
                    return datetime.gameObject.activeSelf;
                else
                    return false;
            }
            set
            {
                if (avatar != null)
                    avatar.gameObject.SetActive(value);
                if (datetime != null)
                    datetime.gameObject.SetActive(value);
            }
        }
        /// <summary>
        /// The parent game object of this message
        /// </summary>
        public GameObject GameObject => gameObject;
        /// <summary>
        /// Initialize the message given a source <see cref="ClanChatMsg"/>
        /// </summary>
        /// <param name="message">The message to initialize</param>
        public void Initialize(ClanChatMsg message)
        {
            User = message.user;
            Type = message.type;
            Message = message.message;
            Data = System.Text.Encoding.UTF8.GetBytes(message.message);
            ReceivedAt = DateTime.Now;

            if (avatar != null)
                avatar.UserData = User;

            if (username != null)
                username.UserData = User;

            this.message.text = Message;

            if (datetime != null)
                datetime.text = ReceivedAt.ToString(dateTimeFormat);
        }
        /// <summary>
        /// Initialize the message given a source <see cref="LobbyChatMsg" />
        /// </summary>
        /// <param name="message">The message to initialize</param>
        public void Initialize(LobbyChatMsg message)
        {
            User = message.sender;
            Type = message.type;
            Message = message.Message;
            Data = message.data;
            ReceivedAt = DateTime.Now;

            if (avatar != null)
                avatar.UserData = User;

            if (username != null)
                username.UserData = User;

            this.message.text = Message;

            if (datetime != null)
                datetime.text = ReceivedAt.ToString(dateTimeFormat);
        }
        /// <summary>
        /// Initialize the message given specific values
        /// </summary>
        /// <param name="sender">The user that sent the message</param>
        /// <param name="message">The message its self</param>
        /// <param name="type">The type of message</param>
        public void Initialize(UserData sender, string message, EChatEntryType type)
        {
            User = sender;
            Type = type;
            Message = message;
            Data = System.Text.Encoding.UTF8.GetBytes(message);
            ReceivedAt = DateTime.Now;

            if (avatar != null)
                avatar.UserData = User;

            if (username != null)
                username.UserData = User;

            this.message.text = Message;

            if (datetime != null)
                datetime.text = ReceivedAt.ToString(dateTimeFormat);
        }
    }
}
#endif