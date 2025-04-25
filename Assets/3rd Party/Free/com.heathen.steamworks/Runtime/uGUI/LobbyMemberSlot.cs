#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;
using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
#endif

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// An abstract class used by the <see cref="PartyLobbyControl"/> to represent a user slot in the UI.
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/programming-tools/lobbymemberslot")]
    public abstract class LobbyMemberSlot : MonoBehaviour
    {
        /// <summary>
        /// A <see cref="UnityEvent"/> to be invoked when the player has indicated that they would like to invite a user to join the indicated lobby. This allows the slot to act like a button that when pressed will start the process of selecting and inviting a user to join the lobby.
        /// </summary>
        public UnityEvent InviteUserRequest;
        /// <summary>
        /// A <see cref="UnityEvent"/> to be invoked when the player has indicated that they would like to ask the user that is represented by this slot to leave the lobby.
        /// </summary>
        public UnityEvent RemoveUserRequest;
        /// <summary>
        /// Controls rather the control is interactable or not
        /// </summary>
        public abstract bool Interactable { get; set; }
        /// <summary>
        /// Set the user this slot represents if any
        /// </summary>
        /// <param name="user">The user to apply</param>
        public abstract void SetUser(LobbyMemberData user);
        /// <summary>
        /// Get the user that is set to this slot if any
        /// </summary>
        /// <returns>The user found</returns>
        public abstract LobbyMemberData GetUser();
        /// <summary>
        /// Clear the user from this slot
        /// </summary>
        public abstract void ClearUser();
    }
}
#endif