#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// <para>
    /// The Clan Chat Director is intended to be attached to your Clan/Group chat UI. You will then need to call the Join method to connect the director to a specific clan's chat. Assuming the user is permitted to be in that chat you will now start receiving messages from that chat and send messages to that chat. 
    /// </para>
    /// <para>
    /// For smaller groups/clans you can also iterate over the members of the chat however be advised that most Steam groups / clans get very large and cannot be iterated over by the user.
    /// </para>
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/clan-chat-director")]
    public class ClanChatDirector : MonoBehaviour
    {
        [Header("Events")]
        public GameConnectedChatJoinEvent evtJoin;
        public GameConnectedClanChatMsgEvent evtReceived;
        public GameConnectedChatLeaveEvent evtLeave;
        /// <summary>
        /// The members seen in this chat room, this may not list all members for large chat rooms.
        /// </summary>
        public UserData[] Members
        {
            get
            {
                if (InRoom)
                    return chatRoom.Value.Members;
                else
                    return new UserData[0];
            }
        }
        /// <summary>
        /// Checks if the Steam Group chat room is open in the Steam UI.
        /// </summary>
        public bool IsOpenInSteam
        {
            get
            {
                if (InRoom)
                    return chatRoom.Value.IsOpenInSteam;
                else
                    return false;
            }
        }
        /// <summary>
        /// This will be true if the user is "in" this chat room
        /// </summary>
        public bool InRoom => chatRoom.HasValue && chatRoom.Value.enterResponse == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess;
        /// <summary>
        /// The chat room associated with this director if any
        /// </summary>
        public ChatRoom ChatRoom => chatRoom.HasValue ? chatRoom.Value : default;

        private ChatRoom? chatRoom = null;

        private void OnEnable()
        {
            API.Clans.Client.EventChatMessageReceived.AddListener(HandleNewMessage);
            API.Clans.Client.EventGameConnectedChatJoin.AddListener(HandleJoined);
            API.Clans.Client.EventGameConnectedChatLeave.AddListener(HandleLeave);
        }

        private void OnDisable()
        {
            API.Clans.Client.EventChatMessageReceived.RemoveListener(HandleNewMessage);
            API.Clans.Client.EventGameConnectedChatJoin.RemoveListener(HandleJoined);
            API.Clans.Client.EventGameConnectedChatLeave.RemoveListener(HandleLeave);
        }

        public void Join(ClanData clan)
        {
            API.Clans.Client.JoinChatRoom(clan, (result, error) =>
            {
                if (!error)
                    chatRoom = result;
                else
                    Debug.LogWarning("Steam client responded with an IO error when attempting to join Clan chat for " + clan.ToString());
            });
        }

        public void Leave()
        {
            if (InRoom)
            {
                chatRoom.Value.Leave();
                chatRoom = null;
            }
        }

        public void Send(string message)
        {
            if (InRoom)
            {
                chatRoom.Value.SendMessage(message);
            }
        }

        public void OpenInSteam()
        {
            if (InRoom)
                chatRoom.Value.OpenChatWindowInSteam();
        }

        private void HandleLeave(UserLeaveData arg0)
        {
            if (InRoom && arg0.room.id == chatRoom.Value.id)
                evtLeave.Invoke(arg0);
        }

        private void HandleJoined(ChatRoom arg0, UserData arg1)
        {
            if (InRoom && arg0.id == chatRoom.Value.id)
                evtJoin.Invoke(arg0, arg1);
        }

        private void HandleNewMessage(ClanChatMsg arg0)
        {
            if (InRoom && arg0.room.id == chatRoom.Value.id)
                evtReceived.Invoke(arg0);
        }
    }
}
#endif