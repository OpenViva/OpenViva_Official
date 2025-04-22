#if !DISABLESTEAMWORKS  && STEAMWORKSNET

using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Tools for working with the Steam Game Server Browser
    /// </summary>
    public class GameServerBrowserManager : MonoBehaviour
    {
        /// <summary>
        /// Simple serializable wrapper around UnityEvent { <see cref="GameServerSearchType"/>, List { <see cref="GameServerBrowserEntry"/> } }
        /// </summary>
        [Serializable]
        public class ResultsEvent : UnityEvent<ResultData>
        { }

        [Serializable]
        public class ResultData
        {
            public GameServerSearchType type;
            public List<GameServerBrowserEntry> entries;
            public bool hasIOFailure;

            public ResultData(GameServerSearchType type, List<GameServerBrowserEntry> entries, bool IOFailure)
            {
                this.type = type;
                this.entries = entries;
                this.hasIOFailure = IOFailure;
            }
        }

        private class Search
        {
            public HServerListRequest hRequest;
            /// <summary>
            /// <para>
            /// param 1:
            /// <code>List { <see cref="GameServerBrowserEntry"/> }</code>
            /// This is the list of servers found.
            /// </para>
            /// <para>
            /// Param 2:
            /// <code>bool</code>
            /// This indicates a failure e.g. true = failure; while false = no failure 
            /// </para>
            /// </summary>
            public Action<List<GameServerBrowserEntry>, bool> callback;
            public Action clear;
            public ISteamMatchmakingServerListResponse m_ServerListResponse;

            public Search()
            {
                m_ServerListResponse = new ISteamMatchmakingServerListResponse(OnServerResponded, OnServerFailedToRespond, OnRefreshComplete);
            }

            private void OnServerResponded(HServerListRequest hRequest, int iServer)
            {
                Debug.Log("OnServerResponded: " + hRequest + " - " + iServer);
            }

            private void OnServerFailedToRespond(HServerListRequest hRequest, int iServer)
            {
                Debug.Log("OnServerFailedToRespond: " + hRequest + " - " + iServer);
                if (callback != null)
                    callback.Invoke(null, true);
            }

            private void OnRefreshComplete(HServerListRequest hRequest, EMatchMakingServerResponse response)
            {
                Debug.Log("OnRefreshComplete: " + hRequest + " - " + response);
                List<GameServerBrowserEntry> serverResults = new();
                var count = SteamMatchmakingServers.GetServerCount(hRequest);

                for (int i = 0; i < count; i++)
                {
                    var serverItem = SteamMatchmakingServers.GetServerDetails(hRequest, i);

                    if (serverItem.m_steamID.m_SteamID != 0 && serverItem.m_nAppID == API.App.Id)
                    {
                        GameServerBrowserEntry entry = new GameServerBrowserEntry(serverItem);
                        serverResults.Add(entry);
                    }
                }

                if (hRequest != HServerListRequest.Invalid)
                {
                    SteamMatchmakingServers.ReleaseRequest(hRequest);
                }

                if (callback != null)
                    callback(serverResults, false);

                clear();
            }
        }

        private class PingQuery
        {
            public HServerQuery hQuery;
            public ISteamMatchmakingPingResponse m_PingResponse;
            public GameServerBrowserEntry target;
            public Action<GameServerBrowserEntry, bool> callback;
            public Action clear;

            public PingQuery()
            {
                m_PingResponse = new ISteamMatchmakingPingResponse(OnServerRespondedPing, OnServerFailedToRespondPing);
            }

            private void OnServerFailedToRespondPing()
            {
                if (hQuery != HServerQuery.Invalid)
                {
                    SteamMatchmakingServers.CancelServerQuery(hQuery);
                }

                callback?.Invoke(target, true);

                clear?.Invoke();
            }

            private void OnServerRespondedPing(gameserveritem_t server)
            {
                if (hQuery != HServerQuery.Invalid)
                {
                    SteamMatchmakingServers.CancelServerQuery(hQuery);
                }

                if (target != null)
                {
                    target.Update(server);
                    target.evtDataUpdated.Invoke();
                    callback?.Invoke(target, false);
                }
                else
                {
                    callback?.Invoke(new GameServerBrowserEntry(server), false);
                }

                clear?.Invoke();
            }
        }

        private class PlayerQuery
        {
            public HServerQuery hQuery;
            public ISteamMatchmakingPlayersResponse m_PlayersResponse;
            public GameServerBrowserEntry target;
            public Action<GameServerBrowserEntry, bool> callback;
            public Action clear;

            public PlayerQuery()
            {
                m_PlayersResponse = new ISteamMatchmakingPlayersResponse(OnAddPlayerToList, OnPlayersFailedToRespond, OnPlayersRefreshComplete);
            }

            private void OnPlayersRefreshComplete()
            {
                if (hQuery != HServerQuery.Invalid)
                {
                    SteamMatchmakingServers.CancelServerQuery(hQuery);
                }

                target.evtDataUpdated.Invoke();

                callback?.Invoke(target, false);

                clear?.Invoke();
            }

            private void OnPlayersFailedToRespond()
            {
                if (hQuery != HServerQuery.Invalid)
                {
                    SteamMatchmakingServers.CancelServerQuery(hQuery);
                }

                callback?.Invoke(target, true);

                clear?.Invoke();
            }

            private void OnAddPlayerToList(string pchName, int nScore, float flTimePlayed)
            {
                target.players.Add(new ServerPlayerEntry() { name = pchName, score = nScore, timePlayed = new TimeSpan(0, 0, 0, (int)flTimePlayed, 0) });
            }
        }

        private class RulesQuery
        {
            public HServerQuery hQuery;
            public ISteamMatchmakingRulesResponse m_RulesResponse;
            public GameServerBrowserEntry target;
            /// <summary>
            /// <para>
            /// param 1:
            /// <code><see cref="GameServerBrowserEntry"/></code>
            /// This is the server to work against.
            /// </para>
            /// <para>
            /// Param 2:
            /// <code>bool</code>
            /// This indicates a falure e.g. true = failure; while false = no failure 
            /// </para>
            /// </summary>
            public Action<GameServerBrowserEntry, bool> callback;
            public Action clear;

            public RulesQuery()
            {
                m_RulesResponse = new ISteamMatchmakingRulesResponse(OnAddRuleToList, OnRulesFailedToRespond, OnRulesRefreshComplete);
            }

            private void OnAddRuleToList(string pchRule, string pchValue)
            {
                target.rules.Add(new StringKeyValuePair { key = pchRule, value = pchValue });
            }

            private void OnRulesRefreshComplete()
            {
                if (hQuery != HServerQuery.Invalid)
                {
                    SteamMatchmakingServers.CancelServerQuery(hQuery);
                }

                target.evtDataUpdated.Invoke();

                if (callback != null)
                    callback(target, false);

                if (clear != null)
                    clear();
            }

            private void OnRulesFailedToRespond()
            {
                if (hQuery != HServerQuery.Invalid)
                {
                    SteamMatchmakingServers.CancelServerQuery(hQuery);
                }

                callback?.Invoke(target, true);

                clear?.Invoke();
            }
        }

        /// <summary>
        /// Simple dictionary wrapper to simplify the filter parameter of search methods.
        /// </summary>
        /// <remarks>
        /// Any parameter of <see cref="Filter"/> can be passed in simply via:
        /// <code>new GameServerBrowser.Filter{ {"key1", "value1"}, {"key2", "value2"} }</code>
        /// This is a simple Dictionary{string, string} so you can construct it in traditional ways as well e.g.
        /// <code>var filter = new GameServerBrowser.Filter();</code>
        /// <code>filter.Add("key1", "value1");</code>
        /// <code>filter.Add("key2", "value2");</code>
        /// </remarks>
        public class Filter : Dictionary<string, string>
        {
            public MatchMakingKeyValuePair_t[] Array
            {
                get
                {
                    var array = new MatchMakingKeyValuePair_t[Count];
                    int index = 0;
                    foreach (var pair in this)
                    {
                        array[index] = new MatchMakingKeyValuePair_t() { m_szKey = pair.Key, m_szValue = pair.Value };
                        index++;
                    }

                    return array;
                }
            }
        }

        private readonly List<Search> searchList = new List<Search>();
        private readonly List<PingQuery> pingList = new List<PingQuery>();
        private readonly List<PlayerQuery> playerList = new List<PlayerQuery>();
        private readonly List<RulesQuery> ruleList = new List<RulesQuery>();
        public ResultsEvent evtSearchCompleted = new ResultsEvent();

        public void GetAllFavorites() => GetServerList(API.App.Client.Id, GameServerSearchType.Favorites, null, null);
        public void GetAllFriends() => GetServerList(API.App.Client.Id, GameServerSearchType.Friends, null, null);
        public void GetAllHistory() => GetServerList(API.App.Client.Id, GameServerSearchType.History, null, null);
        public void GetAllInternet() => GetServerList(API.App.Client.Id, GameServerSearchType.Internet, null, null);
        public void GetAllLAN() => GetServerList(API.App.Client.Id, GameServerSearchType.LAN, null, null);
        public void GetAllSpectator() => GetServerList(API.App.Client.Id, GameServerSearchType.Spectator, null, null);
        public void GetFavorites(Filter filter) => GetServerList(API.App.Client.Id, GameServerSearchType.Favorites, null, filter);
        public void GetFriends(Filter filter) => GetServerList(API.App.Client.Id, GameServerSearchType.Friends, null, filter);
        public void GetHistory(Filter filter) => GetServerList(API.App.Client.Id, GameServerSearchType.History, null, filter);
        public void GetInternet(Filter filter) => GetServerList(API.App.Client.Id, GameServerSearchType.Internet, null, filter);
        public void GetLAN(Filter filter) => GetServerList(API.App.Client.Id, GameServerSearchType.LAN, null, filter);
        public void GetSpectator(Filter filter) => GetServerList(API.App.Client.Id, GameServerSearchType.Spectator, null, filter);
        /// <summary>
        /// Fetch a list of server data from Valve for this app id
        /// </summary>
        /// <param name="type">The type of servers to return</param>
        /// <param name="callback">The action to be called when the process is complete.
        /// param 1 of type <see cref="bool"/> indicates success or failure while param 2 is a list of <see cref="GameServerBrowserEntry"/> representing each server found</param>
        /// <param name="filter">a set of key value pairs representing the search filter</param>
        public void GetServerList(GameServerSearchType type, Action<List<GameServerBrowserEntry>, bool> callback = null, Filter filter = null)
        {
            GetServerList(API.App.Client.Id, type, callback, filter);
        }
        /// <summary>
        /// Fetch a list of server data from Valve for the app id indicated by <paramref name="appId"/>
        /// </summary>
        /// <param name="appId">The app ID to search for</param>
        /// <param name="type">The type of servers to return</param>
        /// <param name="callback">The action to be called when the process is complete.
        /// param 1 of type <see cref="bool"/> indicates success or failure while param 2 is a list of <see cref="GameServerBrowserEntry"/> representing each server found</param>
        /// <param name="filter">a set of key value pairs representing the search filter</param>
        public void GetServerList(AppId_t appId, GameServerSearchType type, Action<List<GameServerBrowserEntry>, bool> callback = null, Filter filter = null)
        {
            var nSearch = new Search();
            nSearch.clear = () => searchList.Remove(nSearch);

            MatchMakingKeyValuePair_t[] filters = new MatchMakingKeyValuePair_t[0];

            if (filter != null)
                filters = filter.Array;

            switch (type)
            {
                case GameServerSearchType.Favorites:
                    nSearch.callback = (r, e) =>
                        {
                            callback?.Invoke(r, e);
                            evtSearchCompleted.Invoke(new ResultData(GameServerSearchType.Favorites, r, e));
                        };
                    API.Matchmaking.Client.RequestFavoritesServerList(appId, filters, nSearch.m_ServerListResponse);
                    break;
                case GameServerSearchType.Friends:
                    nSearch.callback = (r, e) =>
                    {
                        callback?.Invoke(r, e);
                        evtSearchCompleted.Invoke(new ResultData(GameServerSearchType.Friends, r, e));
                    };
                    API.Matchmaking.Client.RequestFriendsServerList(appId, filters, nSearch.m_ServerListResponse);
                    break;
                case GameServerSearchType.History:
                    nSearch.callback = (r, e) =>
                    {
                        callback?.Invoke(r, e);
                        evtSearchCompleted.Invoke(new ResultData(GameServerSearchType.History, r, e));
                    };
                    API.Matchmaking.Client.RequestHistoryServerList(appId, filters, nSearch.m_ServerListResponse);
                    break;
                case GameServerSearchType.Internet:
                    nSearch.callback = (r, e) =>
                    {
                        callback?.Invoke(r, e);
                        evtSearchCompleted.Invoke(new ResultData(GameServerSearchType.Internet, r, e));
                    };
                    API.Matchmaking.Client.RequestInternetServerList(appId, filters, nSearch.m_ServerListResponse);
                    break;
                case GameServerSearchType.LAN:
                    nSearch.callback = (r, e) =>
                    {
                        callback?.Invoke(r, e);
                        evtSearchCompleted.Invoke(new ResultData(GameServerSearchType.LAN, r, e));
                    };
                    API.Matchmaking.Client.RequestLANServerList(appId, nSearch.m_ServerListResponse);
                    break;
                case GameServerSearchType.Spectator:
                    nSearch.callback = (r, e) =>
                    {
                        callback?.Invoke(r, e);
                        evtSearchCompleted.Invoke(new ResultData(GameServerSearchType.Spectator, r, e));
                    };
                    API.Matchmaking.Client.RequestSpectatorServerList(appId, filters, nSearch.m_ServerListResponse);
                    break;
            }

            searchList.Add(nSearch);
        }

        /// <summary>
        /// Ping the target server fetching updated data for it
        /// </summary>
        /// <param name="ipAddress">The address of the server to ping</param>
        /// <param name="port">The port of the server to ping</param>
        /// <param name="callback">The action to call when the process is complete</param>
        public void PingServer(string ipAddress, ushort port, Action<GameServerBrowserEntry, bool> callback)
        {
            PingServer(API.Utilities.IPStringToUint(ipAddress), port, callback);
        }

        /// <summary>
        /// Ping the target server fetching updated data for it
        /// </summary>
        /// <param name="ipAddress">The address of the server to ping</param>
        /// <param name="port">The port of the server to ping</param>
        /// <param name="callback">The action to call when the process is complete</param>
        public void PingServer(uint ipAddress, ushort port, Action<GameServerBrowserEntry, bool> callback)
        {
            var nQuery = new PingQuery();
            nQuery.callback = callback;
            nQuery.hQuery = API.Matchmaking.Client.PingServer(ipAddress, port, nQuery.m_PingResponse);
            nQuery.clear = () => pingList.Remove(nQuery);

            pingList.Add(nQuery);
        }

        /// <summary>
        /// Ping the target server fetching updated data for it
        /// </summary>
        /// <param name="address">the server net address to ping</param>
        /// <param name="callback">The action to call when the process is complete</param>
        public void PingServer(servernetadr_t address, Action<GameServerBrowserEntry, bool> callback)
        {
            PingServer(address.GetIP(), address.GetQueryPort(), callback);
        }

        /// <summary>
        /// Ping the target server fetching updated data for it
        /// </summary>
        /// <param name="entry">the server to ping</param>
        /// <param name="callback">The action to call when the process is complete</param>
        public void PingServer(GameServerBrowserEntry entry, Action<GameServerBrowserEntry, bool> callback)
        {
            var nQuery = new PingQuery();
            nQuery.callback = callback;
            nQuery.target = entry;
            nQuery.hQuery = API.Matchmaking.Client.PingServer(entry.m_NetAdr.GetIP(), entry.m_NetAdr.GetQueryPort(), nQuery.m_PingResponse);
            nQuery.clear = () => pingList.Remove(nQuery);

            pingList.Add(nQuery);
        }

        /// <summary>
        /// Clears the player list then requests fresh player data from Valve
        /// </summary>
        /// <param name="entry">The server target to request the data for</param>
        /// <param name="callback"></param>
        public void PlayerDetails(GameServerBrowserEntry entry, Action<GameServerBrowserEntry, bool> callback)
        {
            var nQuery = new PlayerQuery();
            nQuery.callback = callback;
            entry.players.Clear();
            nQuery.target = entry;
            nQuery.hQuery = API.Matchmaking.Client.PlayerDetails(entry.m_NetAdr.GetIP(), entry.m_NetAdr.GetQueryPort(), nQuery.m_PlayersResponse);
            nQuery.clear = () => playerList.Remove(nQuery);

            playerList.Add(nQuery);
        }

        /// <summary>
        /// Clears the rules list then requests fresh rule data from Valve
        /// </summary>
        /// <param name="entry">The server target to request the data for</param>
        /// <param name="callback"></param>
        public void ServerRules(GameServerBrowserEntry entry, Action<GameServerBrowserEntry, bool> callback)
        {
            var nQuery = new RulesQuery();
            nQuery.callback = callback;
            entry.rules.Clear();
            nQuery.target = entry;
            nQuery.hQuery = API.Matchmaking.Client.ServerRules(entry.m_NetAdr.GetIP(), entry.m_NetAdr.GetQueryPort(), nQuery.m_RulesResponse);
            nQuery.clear = () => ruleList.Remove(nQuery);

            ruleList.Add(nQuery);
        }

        private void OnDestroy()
        {
            if (searchList != null)
            {
                foreach (var search in searchList)
                {
                    try
                    {
                        if (search.hRequest != HServerListRequest.Invalid)
                            SteamMatchmakingServers.ReleaseRequest(search.hRequest);
                    }
                    catch { }
                }
            }

            if(pingList != null)
            {
                foreach(var ping in pingList)
                {
                    try
                    {
                        if (ping.hQuery != HServerQuery.Invalid)
                            SteamMatchmakingServers.CancelServerQuery(ping.hQuery);
                    }
                    catch { }
                }
            }

            if(playerList != null)
            {
                foreach (var player in playerList)
                {
                    try
                    {
                        if (player.hQuery != HServerQuery.Invalid)
                            SteamMatchmakingServers.CancelServerQuery(player.hQuery);
                    }
                    catch { }
                }
            }

            if (ruleList != null)
            {
                foreach (var rule in ruleList)
                {
                    try
                    {
                        if (rule.hQuery != HServerQuery.Invalid)
                            SteamMatchmakingServers.CancelServerQuery(rule.hQuery);
                    }
                    catch { }
                }
            }
        }
    }
}
#endif