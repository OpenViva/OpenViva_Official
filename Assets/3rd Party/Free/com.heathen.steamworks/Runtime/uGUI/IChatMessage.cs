#if !DISABLESTEAMWORKS  && STEAMWORKSNET

using Steamworks;
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// A simple interface that standardizes the concept of a chat message making it easier to create UI elements for the presentation and collection of chat messages originating from Steam Lobby, Clan and Friends.
    /// </summary>
    public interface IChatMessage
    {
        /// <summary>
        /// This should return the user that sent the chat message
        /// </summary>
        UserData User { get; }
        /// <summary>
        /// This should return the raw data of the message
        /// </summary>
        byte[] Data { get; }
        /// <summary>
        /// The should return the string formatted data of the message
        /// </summary>
        string Message { get; }
        /// <summary>
        /// This should indicate the date and time the message was received
        /// </summary>
        DateTime ReceivedAt { get; }
        /// <summary>
        /// This should indicate the type of chat message
        /// </summary>
        EChatEntryType Type { get; }
        /// <summary>
        /// This can be read or set to indicate the expansion state of the message
        /// </summary>
        bool IsExpanded { get; set; }
        /// <summary>
        /// A simple shortcut to returning the root GameObject of the message
        /// </summary>
        GameObject GameObject { get; }
        /// <summary>
        /// This should configure the message as a Clan Chat Message
        /// </summary>
        /// <param name="message">The source message</param>
        void Initialize(ClanChatMsg message);
        /// <summary>
        /// This should configure the message as a Lobby Chat Message
        /// </summary>
        /// <param name="message">The source message</param>
        void Initialize(LobbyChatMsg message);
        /// <summary>
        /// This should configure the message in a generic manner 
        /// </summary>
        /// <param name="sender">The user if any this message was sent from</param>
        /// <param name="message">The message if any to be displayed</param>
        /// <param name="type">The type of message to be presented</param>
        void Initialize(UserData sender, string message, EChatEntryType type);
    }
}
#endif