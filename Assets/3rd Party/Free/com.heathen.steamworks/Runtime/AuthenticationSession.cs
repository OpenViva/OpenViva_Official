#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents an authentication session and carries unique information about the session request such as the user the session is in relation to and the ticket data of the session.
    /// </summary>
    [Serializable]
    public class AuthenticationSession
    {
        /// <summary>
        /// Indicates that this session is being managed by a client or server
        /// </summary>
        public bool IsClientSession { get; private set; } = true;
        /// <summary>
        /// The user this session is in relation to
        /// </summary>
        public UserData User { get; private set; }
        /// <summary>
        /// The ID of the user that owns the game the user of this session is playing ... e.g. if this differs from the user then this is a barrowed game.
        /// </summary>
        public UserData GameOwner { get; private set; }
        /// <summary>
        /// The session data aka the 'ticket' data.
        /// </summary>
        public byte[] Data { get; private set; }
        /// <summary>
        /// The response received when validating a provided ticket.
        /// </summary>
        public EAuthSessionResponse Response { get; private set; }
        /// <summary>
        /// If true then the game this user is playing is barrowed from another user e.g. this user does not have a license for this game but is barrowing it from another user.
        /// </summary>
        public bool IsBarrowed { get { return User != GameOwner; } }
        /// <summary>
        /// The callback delegate to be called when the authenticate session response returns from the steam backend.
        /// </summary>
        public Action<AuthenticationSession> OnStartCallback { get; private set; }
        /// <summary>
        /// Construct a new <see cref="AuthenticationSession"/>
        /// </summary>
        /// <param name="userId">The user the session relates to</param>
        /// <param name="callback">A delegate of form (<see cref="AuthenticationSession"/> session) to invoke when ready</param>
        /// <param name="isClient">Is this a client or server session</param>
        public AuthenticationSession(CSteamID userId, Action<AuthenticationSession> callback, bool isClient = true)
        {
            IsClientSession = isClient;
            User = userId;
            OnStartCallback = callback;
        }

        internal void Authenticate(ValidateAuthTicketResponse_t response)
        {
            Response = response.m_eAuthSessionResponse;
            GameOwner = response.m_OwnerSteamID;
        }

        /// <summary>
        /// Ends the authentication session.
        /// </summary>
        public void End()
        {
            if (IsClientSession)
                SteamUser.EndAuthSession(User);
            else
                SteamGameServer.EndAuthSession(User);
        }
    }
}
#endif