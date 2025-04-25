#if !DISABLESTEAMWORKS && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Handles Steam's Authentication interface for both Client and Server interfaces
    /// </summary>
    public static class Authentication
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#pragma warning disable IDE0051 // Remove unused private members
        static void Init()
#pragma warning restore IDE0051 // Remove unused private members
        {
            ActiveTickets = new List<AuthenticationTicket>();
            ActiveSessions = new List<AuthenticationSession>();
            m_GetAuthSessionTicketResponse = null;
            m_GetAuthSessionTicketResponseServer = null;
            m_ValidateAuthSessionTicketResponse = null;
            m_ValidateAuthSessionTicketResponseServer = null;
            m_GetTicketForWebApiResponse = null;
            m_EncryptedAppTicketResponse = null;
            m_StoreAuthURLResponse = null;
        }

        /// <summary>
        /// Tickets this player has sent out
        /// </summary>
        public static List<AuthenticationTicket> ActiveTickets = new List<AuthenticationTicket>();
        /// <summary>
        /// Sessions this player has started
        /// </summary>
        public static List<AuthenticationSession> ActiveSessions = new List<AuthenticationSession>();

#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable CS0414
        private static Callback<GetAuthSessionTicketResponse_t> m_GetAuthSessionTicketResponse;
        private static Callback<GetAuthSessionTicketResponse_t> m_GetAuthSessionTicketResponseServer;
        private static Callback<GetTicketForWebApiResponse_t> m_GetTicketForWebApiResponse;
        private static Callback<ValidateAuthTicketResponse_t> m_ValidateAuthSessionTicketResponse;
        private static Callback<ValidateAuthTicketResponse_t> m_ValidateAuthSessionTicketResponseServer;
        internal static CallResult<EncryptedAppTicketResponse_t> m_EncryptedAppTicketResponse;
        private static CallResult<StoreAuthURLResponse_t> m_StoreAuthURLResponse;
#pragma warning restore CS0414
#pragma warning restore IDE0052 // Remove unread private members

        /// <summary>
        /// Determines if the provided ticket handle is valid
        /// </summary>
        /// <param name="ticket">the ticket to test for validity</param>
        /// <returns></returns>
        public static bool IsAuthTicketValid(AuthenticationTicket ticket)
        {
            if (ticket.Handle == default || ticket.Handle == HAuthTicket.Invalid)
                return false;
            else
                return true;
        }

        /// <summary>
        /// <para>Encodes a ticket to hex string format</para>
        /// This is most commonly used with web calls such as <a href="https://partner.steamgames.com/doc/webapi/ISteamUserAuth#AuthenticateUserTicket">https://partner.steamgames.com/doc/webapi/ISteamUserAuth#AuthenticateUserTicket</a>
        /// </summary>
        /// <param name="ticket">The ticket to be encoded</param>
        /// <returns>Returns the hex encoded string representation of the ticket data array.</returns>
        public static string EncodedAuthTicket(AuthenticationTicket ticket)
        {
            if (!IsAuthTicketValid(ticket))
                return "";
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (byte b in ticket.Data)
                    sb.AppendFormat("{0:X2}", b);

                return sb.ToString();
            }
        }
        /// <summary>
        /// Get an authentication session ticket for the specified entity
        /// </summary>
        /// <param name="authenticatingIdentity">The entity that will be authenticating the ticket</param>
        /// <param name="callback">A delegate of the form (<see cref="AuthenticationTicket"/> result, <see cref="bool"/> ioError) that will be invoked when completed</param>
        public static void GetAuthSessionTicket(CSteamID authenticatingIdentity, Action<AuthenticationTicket, bool> callback)
        {
            var nId = new SteamNetworkingIdentity()
            { 
                m_eType = ESteamNetworkingIdentityType.k_ESteamNetworkingIdentityType_SteamID 
            };
            nId.SetSteamID(authenticatingIdentity);
            GetAuthSessionTicket(nId, callback);
        }

        /// <summary>
        /// Requests a new Auth Session Ticket
        /// </summary>
        /// <param name="callback">A delegate of the form (<see cref="AuthenticationTicket"/> result, <see cref="bool"/> ioError) that will be invoked when completed</param>
        public static void GetAuthSessionTicket(SteamNetworkingIdentity forIdentity, Action<AuthenticationTicket, bool> callback)
        {
#if !UNITY_SERVER
            m_GetAuthSessionTicketResponse ??= Callback<GetAuthSessionTicketResponse_t>.Create(HandleGetAuthSessionTicketResponse);

            var ticket = new AuthenticationTicket(forIdentity, callback, true);
            ActiveTickets ??= new List<AuthenticationTicket>();
            ActiveTickets.Add(ticket);
#else
            if (m_GetAuthSessionTicketResponseServer == null)
                m_GetAuthSessionTicketResponseServer = Callback<GetAuthSessionTicketResponse_t>.CreateGameServer(HandleGetAuthSessionTicketResponse);

            var ticket = new AuthenticationTicket(forIdentity, callback, false);

            if (ActiveTickets == null)
                ActiveTickets = new List<AuthenticationTicket>();

            ActiveTickets.Add(ticket);
#endif
        }
        /// <summary>
        /// Get an auth session ticket using the Encrypted Auth Ticket solution. This requires you to configure teh Steam API in your security settings for the app
        /// </summary>
        /// <param name="dataToInclude"></param>
        /// <param name="callback">A delegate of the form (<see cref="AuthenticationTicket"/> url, <see cref="bool"/> ioError) that will be invoked when completed</param>
        public static void GetEncryptedAuthSessionTicket(byte[] dataToInclude, Action<AuthenticationTicket, bool> callback)
        {
#if !UNITY_SERVER
            m_EncryptedAppTicketResponse ??= CallResult<EncryptedAppTicketResponse_t>.Create();

            var ticket = new AuthenticationTicket(dataToInclude, callback);
            ActiveTickets ??= new List<AuthenticationTicket>();
            ActiveTickets.Add(ticket);
#else
#endif
        }
        /// <summary>
        /// Get an auth session ticket for use with the web API
        /// </summary>
        /// <param name="webIdentity"></param>
        /// <param name="callback">A delegate of the form (<see cref="AuthenticationTicket"/> url, <see cref="bool"/> ioError) that will be invoked when completed</param>
        public static void GetWebAuthSessionTicket(string webIdentity, Action<AuthenticationTicket, bool> callback)
        {
#if !UNITY_SERVER
            m_GetTicketForWebApiResponse ??= Callback<GetTicketForWebApiResponse_t>.Create(HandleGetTicketForWebApiResponse);

            var ticket = new AuthenticationTicket(webIdentity, callback);
            ActiveTickets ??= new List<AuthenticationTicket>();
            ActiveTickets.Add(ticket);
#else
            
#endif
        }

        /// <summary>
        /// Requests a URL which authenticates an in-game browser for store check-out, and then redirects to the specified URL.
        /// <para>As long as the in-game browser accepts and handles session cookies, Steam microtransaction checkout pages will automatically recognize the user instead of presenting a login page.</para>
        /// </summary>
        /// <param name="redirectUrl">The redirect URL to be passed in</param>
        /// <param name="callback">A delegate of the form (<see cref="string"/> url, <see cref="bool"/> ioError) that will be invoked when completed</param>
        public static void GetStoreAuthURL(string redirectUrl, Action<string, bool> callback)
        {
            m_StoreAuthURLResponse ??= CallResult<StoreAuthURLResponse_t>.Create();

            var callResult = SteamUser.RequestStoreAuthURL(redirectUrl);
            m_StoreAuthURLResponse.Set(callResult, (result, error) =>
            {
                callback?.Invoke(result.m_szURL, error);
            });
        }

        /// <summary>
        /// Cancels the auth ticket rather its client or server based.
        /// </summary>
        /// <param name="ticket">The ticket to cancel</param>
        public static void CancelAuthTicket(AuthenticationTicket ticket)
        {
            ticket.Cancel();

            ActiveTickets.Remove(ticket);
        }

        /// <summary>
        /// Starts an authorization session with the indicated user given the applied auth ticket
        /// </summary>
        /// <param name="authTicket">The ticket data to validate</param>
        /// <param name="user">The user the session will relate to</param>
        /// <param name="callback">A delegate of the form (<see cref="AuthenticationSession"/> result, <see cref="bool"/> ioError) that will be invoked when completed</param>
        public static EBeginAuthSessionResult BeginAuthSession(byte[] authTicket, UserData user, Action<AuthenticationSession> callback)
        {
#if !UNITY_SERVER
            if (m_ValidateAuthSessionTicketResponse == null)
                m_ValidateAuthSessionTicketResponse = Callback<ValidateAuthTicketResponse_t>.Create(HandleValidateAuthTicketResponse);

            var session = new AuthenticationSession(user, callback, true);

            if (ActiveSessions == null)
            {
                ActiveSessions = new List<AuthenticationSession>();
            }
            ActiveSessions.Add(session);

            return SteamUser.BeginAuthSession(authTicket, authTicket.Length, user);
#else
            if (m_ValidateAuthSessionTicketResponseServer == null)
                m_ValidateAuthSessionTicketResponseServer = Callback<ValidateAuthTicketResponse_t>.CreateGameServer(HandleValidateAuthTicketResponse);

            var session = new AuthenticationSession(user, callback, false);

            if (ActiveSessions == null)
            {
                ActiveSessions = new List<AuthenticationSession>();
            }
            ActiveSessions.Add(session);

            return SteamGameServer.BeginAuthSession(authTicket, authTicket.Length, user);
#endif
        }

        /// <summary>
        /// Ends the auth session with the indicated user if any
        /// </summary>
        /// <param name="user">The user to end the session with</param>
        public static void EndAuthSession(UserData user)
        { 
#if !UNITY_SERVER
            SteamUser.EndAuthSession(user);
#else
            SteamGameServer.EndAuthSession(user);
#endif

            ActiveSessions.RemoveAll(p => p.User == user);
        }
        /// <summary>
        /// Checks if the user owns a specific piece of Downloadable Content (DLC).
        /// </summary>
        /// <remarks>
        /// This can only be used after BeginAuthSession has been ran on the user's ticket
        /// </remarks>
        /// <param name="user">The authenticated user to check</param>
        /// <param name="appId">The app Id of the app to check for</param>
        /// <returns></returns>
        public static EUserHasLicenseForAppResult UserHasLicenseForApp(UserData user, AppData appId)
        {
#if !UNITY_SERVER
            return SteamUser.UserHasLicenseForApp(user, appId);
#else
            return SteamGameServer.UserHasLicenseForApp(user, appId);
#endif
        }

        private static void HandleGetAuthSessionTicketResponse(GetAuthSessionTicketResponse_t pCallback)
        {
            if (ActiveTickets != null && ActiveTickets.Any(p => p.Handle == pCallback.m_hAuthTicket))
            {
                var ticket = ActiveTickets.First(p => p.Handle == pCallback.m_hAuthTicket);
                ticket.Authenticate(pCallback);
            }
        }

        private static void HandleGetTicketForWebApiResponse(GetTicketForWebApiResponse_t pCallback)
        {
            if (ActiveTickets != null && ActiveTickets.Any(p => p.Handle == pCallback.m_hAuthTicket))
            {
                var ticket = ActiveTickets.First(p => p.Handle == pCallback.m_hAuthTicket);
                ticket.Authenticate(pCallback);
            }
        }

        private static void HandleValidateAuthTicketResponse(ValidateAuthTicketResponse_t param)
        {
            if (ActiveSessions != null && ActiveSessions.Any(p => p.User == param.m_SteamID))
            {
                var session = ActiveSessions.First(p => p.User == param.m_SteamID);
                session.Authenticate(param);

                if (App.isDebugging)
                    Debug.Log("Processing session request data for " + param.m_SteamID.m_SteamID.ToString() + " status = " + param.m_eAuthSessionResponse);

                if (param.m_eAuthSessionResponse != EAuthSessionResponse.k_EAuthSessionResponseOK)
                    ActiveSessions.Remove(session);

                if (session.OnStartCallback != null)
                    session.OnStartCallback.Invoke(session);
            }
            else
            {
                if (App.isDebugging)
                    Debug.LogWarning("Received an authentication ticket response for user " + param.m_SteamID.m_SteamID + " no matching session was found for this user.");
            }
        }

        /// <summary>
        /// Ends all tracked sessions
        /// </summary>
        public static void EndAllSessions()
        {
            foreach (var session in ActiveSessions)
                session.End();

            ActiveSessions.Clear();
        }

        /// <summary>
        /// Cancels all tracked tickets
        /// </summary>
        public static void CancelAllTickets()
        {
            foreach (var ticket in ActiveTickets)
                ticket.Cancel();

            ActiveTickets.Clear();
        }
    }
}
#endif