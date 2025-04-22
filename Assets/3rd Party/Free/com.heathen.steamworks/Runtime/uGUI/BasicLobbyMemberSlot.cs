#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
#endif

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// An implementation of <see cref="LobbyMemberSlot"/> used by the <see cref="PartyLobbyControl"/> to represent a user slot in the UI.
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/programming-tools/lobbymemberslot")]
    public class BasicLobbyMemberSlot : LobbyMemberSlot
    {
        /// <summary>
        /// The user's avatar
        /// </summary>
        [Tooltip("The user's avatar")]
        public SetUserAvatar avatar;
        /// <summary>
        /// A button that can be pressed to start the invite process
        /// </summary>
        [Tooltip("A button that can be pressed to start the invite process")]
        public Button inviteButton;
        /// <summary>
        /// A button that can be pressed to ask the user in this slot to leave
        /// </summary>
        [Tooltip("A button that can be pressed to ask the user in this slot to leave")]
        public Button removeButton;
        /// <summary>
        /// An icon to display if this user is the owner of the lobby
        /// </summary>
        [Tooltip("An icon to display if this user is the owner of the lobby")]
        public GameObject ownerPip;
        /// <summary>
        /// An icon to display if the user in this slot has indicated they are ready
        /// </summary>
        [Tooltip("An icon to display if the user in this slot has indicated they are ready")]
        public GameObject readyPip;
        /// <summary>
        /// An icon to display if the user not yet indicated they are ready to play
        /// </summary>
        [Tooltip("An icon to display if the user not yet indicated they are ready to play")]
        public GameObject waitingPip;

        private LobbyMemberData member;
        /// <summary>
        /// Marks the buttons interactable or not based on the input value
        /// </summary>
        public override bool Interactable 
        { 
            get
            {
                if (inviteButton != null)
                    return inviteButton.interactable;
                else if (removeButton != null)
                    return removeButton.interactable;
                else
                    return false;
            }
            set
            {
                if (inviteButton != null)
                    inviteButton.interactable = value;
                
                if (removeButton != null)
                    removeButton.interactable = value;
            }
        }
        /// <summary>
        /// Clears the display of all user data
        /// </summary>
        public override void ClearUser()
        {
            member = default;
            if (avatar != null)
                avatar.gameObject.SetActive(false);
            if (inviteButton != null)
                inviteButton.gameObject.SetActive(true);
            if (removeButton != null)
                removeButton.gameObject.SetActive(false);
            if (ownerPip != null)
                ownerPip.SetActive(false);
            if (readyPip != null)
                readyPip.SetActive(false);
            if (waitingPip != null)
                waitingPip.SetActive(false);
        }
        /// <summary>
        /// Returns the user linked with this slot if any
        /// </summary>
        /// <returns>The member present if any</returns>
        public override LobbyMemberData GetUser() => member;
        /// <summary>
        /// Set the user linked with this slot and update all related UI elements
        /// </summary>
        /// <param name="member">The member to apply</param>
        public override void SetUser(LobbyMemberData member)
        {
            this.member = member;
            if (avatar != null)
            {
                avatar.UserData = member.user;
                avatar.gameObject.SetActive(true);
            }
            if (inviteButton != null)
                inviteButton.gameObject.SetActive(false);
            if (removeButton != null)
                removeButton.gameObject.SetActive(true);
            if (ownerPip != null)
                ownerPip.SetActive(member.IsOwner);
            if (readyPip != null)
                readyPip.SetActive(member.IsReady);
            if (waitingPip != null)
                waitingPip.SetActive(!member.IsReady);
        }
    }
}
#endif