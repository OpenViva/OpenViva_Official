#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Functions that provide information about Steam Remote Play sessions, streaming your game content to another computer or to a Steam Link app or hardware.
    /// </summary>
    public static class RemotePlay
    {
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                eventSteamRemotePlaySessionConnected = new SteamRemotePlaySessionConnectedEvent();
                eventSteamRemotePlaySessionDisconnected = new SteamRemotePlaySessionDisconnectedEvent();
                m_SteamRemotePlaySessionConnected_t = null;
                m_SteamRemotePlaySessionDisconnected_t = null;
            }

            /// <summary>
            /// Invoked when a session connects
            /// </summary>
            public static SteamRemotePlaySessionConnectedEvent EventSessionConnected
            {
                get
                {
                    if (m_SteamRemotePlaySessionConnected_t == null)
                        m_SteamRemotePlaySessionConnected_t = Callback<SteamRemotePlaySessionConnected_t>.Create(eventSteamRemotePlaySessionConnected.Invoke);

                    return eventSteamRemotePlaySessionConnected;
                }
            }
            /// <summary>
            /// Invoked when a session disconnects
            /// </summary>
            public static SteamRemotePlaySessionDisconnectedEvent EventSessionDisconnected
            {
                get
                {
                    if (m_SteamRemotePlaySessionDisconnected_t == null)
                        m_SteamRemotePlaySessionDisconnected_t = Callback<SteamRemotePlaySessionDisconnected_t>.Create(eventSteamRemotePlaySessionDisconnected.Invoke);

                    return eventSteamRemotePlaySessionDisconnected;
                }
            }

            private static SteamRemotePlaySessionConnectedEvent eventSteamRemotePlaySessionConnected = new SteamRemotePlaySessionConnectedEvent();
            private static SteamRemotePlaySessionDisconnectedEvent eventSteamRemotePlaySessionDisconnected = new SteamRemotePlaySessionDisconnectedEvent();

            private static Callback<SteamRemotePlaySessionConnected_t> m_SteamRemotePlaySessionConnected_t;
            private static Callback<SteamRemotePlaySessionDisconnected_t> m_SteamRemotePlaySessionDisconnected_t;

            /// <summary>
            /// Get the number of currently connected Steam Remote Play sessions
            /// </summary>
            /// <returns></returns>
            public static uint GetSessionCount() => SteamRemotePlay.GetSessionCount();
            /// <summary>
            /// Get the currently connected Steam Remote Play session ID at the specified index
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public static RemotePlaySessionID_t GetSessionID(int index) => SteamRemotePlay.GetSessionID(index);
            /// <summary>
            /// Get the collection of current remote play sessions
            /// </summary>
            /// <returns></returns>
            public static RemotePlaySessionID_t[] GetSessions()
            {
                var count = SteamRemotePlay.GetSessionCount();
                var results = new RemotePlaySessionID_t[count];
                for (int i = 0; i < count; i++)
                {
                    results[i] = SteamRemotePlay.GetSessionID(i);
                }
                return results;
            }
            /// <summary>
            /// Get the UserData of the connected user
            /// </summary>
            /// <param name="session"></param>
            /// <returns></returns>
            public static UserData GetSessionUser(RemotePlaySessionID_t session) => SteamRemotePlay.GetSessionSteamID(session);
            /// <summary>
            /// Get the name of the session client device
            /// </summary>
            /// <param name="session"></param>
            /// <returns></returns>
            public static string GetSessionClientName(RemotePlaySessionID_t session) => SteamRemotePlay.GetSessionClientName(session);
            /// <summary>
            /// Get the form factor of the session client device
            /// </summary>
            /// <param name="session"></param>
            /// <returns></returns>
            public static ESteamDeviceFormFactor GetSessionClientFormFactor(RemotePlaySessionID_t session) => SteamRemotePlay.GetSessionClientFormFactor(session);
            /// <summary>
            /// Get the resolution, in pixels, of the session client device. This is set to 0x0 if the resolution is not available.
            /// </summary>
            /// <param name="session"></param>
            /// <returns></returns>
            public static Vector2Int GetSessionClientResolution(RemotePlaySessionID_t session)
            {
                SteamRemotePlay.BGetSessionClientResolution(session, out int x, out int y);
                return new Vector2Int(x, y);
            }
            /// <summary>
            /// Invite a friend to join the game using Remote Play Together
            /// </summary>
            /// <param name="user"></param>
            /// <returns></returns>
            public static bool SendInvite(UserData user) => SteamRemotePlay.BSendRemotePlayTogetherInvite(user);
        }
    }
}
#endif