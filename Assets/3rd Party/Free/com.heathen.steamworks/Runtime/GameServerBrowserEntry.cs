#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents the data from the Game Server Browser
    /// </summary>
    /// <remarks>
    /// This is an extension of Valve's <see cref="gameserveritem_t"/>
    /// </remarks>
    [Serializable]
    public class GameServerBrowserEntry : gameserveritem_t
    {
        /// <summary>
        /// Returns the IP address as a string via the <see cref="API.Utilities.IPUintToString(uint)"/> feature
        /// </summary>
        public string IpAddress => API.Utilities.IPUintToString(m_NetAdr.GetIP());
        /// <summary>
        /// Returns the query port registered for the server
        /// </summary>
        public ushort QueryPort => m_NetAdr.GetQueryPort();
        /// <summary>
        /// Returns the connection port registered for the server
        /// </summary>
        public ushort ConnectionPort => m_NetAdr.GetConnectionPort();
        /// <summary>
        /// Returns the steam ID registered to the server
        /// </summary>
        public CSteamID SteamId => m_steamID;
        /// <summary>
        /// Returns the app id registered on the server
        /// </summary>
        public AppId_t AppId => new AppId_t(m_nAppID);
        /// <summary>
        /// Indicates rather or not the server uses a password
        /// </summary>
        public bool UsesPassword => m_bPassword;
        /// <summary>
        /// Indicates rather or not the server is VAC secured
        /// </summary>
        public bool IsSecured => m_bSecure;
        /// <summary>
        /// Indicates the number of player's currently authenticated to the server
        /// </summary>
        public int PlayerCount => m_nPlayers;
        /// <summary>
        /// Indicates the number of bots on the server
        /// </summary>
        public int BotPlayerCount => m_nBotPlayers;
        /// <summary>
        /// Indicates the max number of players permitted on the server
        /// </summary>
        public int MaxPlayerCount => m_nMaxPlayers;
        /// <summary>
        /// Returns the last known ping time
        /// </summary>
        public int Ping => m_nPing;
        /// <summary>
        /// Returns the server's version id
        /// </summary>
        public int Version => m_nServerVersion;
        /// <summary>
        /// Returns the last played value as a date time
        /// </summary>
        public DateTime LastPlayed => new DateTime(1970, 1, 1).AddSeconds(m_ulTimeLastPlayed);
        /// <summary>
        /// The description listed on the server
        /// </summary>
        public string Description { get => GetGameDescription(); set => SetGameDescription(value); }
        /// <summary>
        /// The tags listed on the server
        /// </summary>
        public string Tags { get => GetGameTags(); set => SetGameTags(value); }
        /// <summary>
        /// The name of the server
        /// </summary>
        public string Name { get => GetServerName(); set => SetServerName(value); }
        /// <summary>
        /// The map registered to the server
        /// </summary>
        public string Map { get => GetMap(); set => SetMap(value); }
        /// <summary>
        /// The directory used by the server
        /// </summary>
        public string Directory { get => GetGameDir(); set => SetGameDir(value); }
        /// <summary>
        /// The known rules registered on the server
        /// </summary>
        public List<StringKeyValuePair> rules;
        /// <summary>
        /// The players listed on the server
        /// </summary>
        public List<ServerPlayerEntry> players;
        /// <summary>
        /// event invoked when the server's data is updated
        /// </summary>
        public UnityEvent evtDataUpdated = new();

        public GameServerBrowserEntry(gameserveritem_t item)
        {
            evtDataUpdated = new();
            m_bDoNotRefresh = item.m_bDoNotRefresh;
            m_bHadSuccessfulResponse = item.m_bHadSuccessfulResponse;
            m_bPassword = item.m_bPassword;
            m_bSecure = item.m_bSecure;
            m_nAppID = item.m_nAppID;
            m_nBotPlayers = item.m_nBotPlayers;
            m_NetAdr = item.m_NetAdr;
            m_nMaxPlayers = item.m_nMaxPlayers;
            m_nPing = item.m_nPing;
            m_nPlayers = item.m_nPlayers;
            m_nServerVersion = item.m_nServerVersion;
            m_steamID = item.m_steamID;
            m_ulTimeLastPlayed = item.m_ulTimeLastPlayed;
            SetGameDescription(item.GetGameDescription());
            SetGameDir(item.GetGameDir());
            SetGameTags(item.GetGameTags());
            SetMap(item.GetMap());
            SetServerName(item.GetServerName());
            this.players = new List<ServerPlayerEntry>();
            this.rules = new List<StringKeyValuePair>();
        }

        /// <summary>
        /// Updates the data for the entry
        /// </summary>
        /// <param name="item"></param>
        public void Update(gameserveritem_t item)
        {
            m_bDoNotRefresh = item.m_bDoNotRefresh;
            m_bHadSuccessfulResponse = item.m_bHadSuccessfulResponse;
            m_bPassword = item.m_bPassword;
            m_bSecure = item.m_bSecure;
            m_nAppID = item.m_nAppID;
            m_nBotPlayers = item.m_nBotPlayers;
            m_NetAdr = item.m_NetAdr;
            m_nMaxPlayers = item.m_nMaxPlayers;
            m_nPing = item.m_nPing;
            m_nPlayers = item.m_nPlayers;
            m_nServerVersion = item.m_nServerVersion;
            m_steamID = item.m_steamID;
            m_ulTimeLastPlayed = item.m_ulTimeLastPlayed;
            SetGameDescription(item.GetGameDescription());
            SetGameDir(item.GetGameDir());
            SetGameTags(item.GetGameTags());
            SetMap(item.GetMap());
            SetServerName(item.GetServerName());

            evtDataUpdated.Invoke();
        }
    }
}
#endif