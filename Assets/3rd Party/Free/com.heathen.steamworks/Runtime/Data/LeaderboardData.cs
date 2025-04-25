#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a Steam leaderboard
    /// </summary>
    [Serializable]
    public struct LeaderboardData : IEquatable<SteamLeaderboard_t>, IEquatable<ulong>, IEquatable<string>
    {
        /// <summary>
        /// What is the name of the board ... if this is not to be created at run time then this must match the name as it appears in Steamworks
        /// </summary>
        public string apiName;
        /// <summary>
        /// What is the leaderboard ID ... this is nullable if null then no leaderboard has been connected
        /// </summary>
        public SteamLeaderboard_t id;
        /// <summary>
        /// The display name of the leaderboard if any
        /// </summary>
        public readonly string DisplayName => Steamworks.SteamUserStats.GetLeaderboardName(id);
        /// <summary>
        /// Is the board valid and ready for use
        /// </summary>
        public readonly bool Valid => id.m_SteamLeaderboard > 0;
        /// <summary>
        /// The number of entries on the board
        /// </summary>
        public readonly int EntryCount => API.Leaderboards.Client.GetEntryCount(id);

        /// <summary>
        /// Returns the user entry for the local user
        /// </summary>
        /// <param name="callback">The delegate to invoke when the process is complete</param>
        public readonly void GetUserEntry(int maxDetailEntries, Action<LeaderboardEntry, bool> callback)
        {
            API.Leaderboards.Client.DownloadEntries(id, new CSteamID[] { UserData.Me }, maxDetailEntries, (results, error) =>
            {
                if (error || results.Length == 0)
                    callback.Invoke(null, error);
                else
                    callback.Invoke(results[0], error);
            });
        }
        /// <summary>
        /// Returns the top number of entries on the board
        /// </summary>
        /// <param name="count">How many top entries to return</param>
        /// <param name="maxDetailEntries">How many detail entries should be read</param>
        /// <param name="callback">This will be invoked when the process is completed</param>
        public readonly void GetTopEntries(int count, int maxDetailEntries, Action<LeaderboardEntry[], bool> callback) => GetEntries(ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 0, count, maxDetailEntries, callback);
        /// <summary>
        /// Invokes the callback with the query results
        /// </summary>
        /// <param name="request">The type of range to get from the board</param>
        /// <param name="start">The index to start downloading at</param>
        /// <param name="end">The index to end downloading at</param>
        /// <param name="callback">The delegate to invoke when the process is complete</param>
        public readonly void GetEntries(ELeaderboardDataRequest request, int start, int end, int maxDetailEntries, Action<LeaderboardEntry[], bool> callback) => API.Leaderboards.Client.DownloadEntries(id, request, start, end, maxDetailEntries, callback);
        /// <summary>
        /// Invokes the callback with the query results 
        /// </summary>
        /// <param name="users">The users to get results for</param>
        /// <param name="callback">The delegate to invoke when the process is complete</param>
        public readonly void GetEntries(UserData[] users, int maxDetailEntries, Action<LeaderboardEntry[], bool> callback) => API.Leaderboards.Client.DownloadEntries(id, Array.ConvertAll<UserData, CSteamID>(users, p => p.id), maxDetailEntries, callback);
        /// <summary>
        /// Attempt to return all entries or this board, this is not recommended and may not return all results in that Steam may rate limit the request
        /// </summary>
        /// <param name="maxDetailEntries"></param>
        /// <param name="callback"></param>
        public readonly void GetAllEntries(int maxDetailEntries, Action<LeaderboardEntry[], bool> callback)
        {
            GetEntries(ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 0, int.MaxValue, maxDetailEntries, callback);
        }

        /// <summary>
        /// Invokes the callback with the query results 
        /// </summary>
        /// <param name="users">The users to get results for</param>
        /// <param name="callback">The delegate to invoke when the process is complete</param>
        public readonly void GetEntries(CSteamID[] users, int maxDetailEntries, Action<LeaderboardEntry[], bool> callback) => API.Leaderboards.Client.DownloadEntries(id, users, maxDetailEntries, callback);
        /// <summary>
        /// Get the board that matches the name provided
        /// </summary>
        /// <param name="name">The API name of the board to get</param>
        /// <param name="callback">A delegate of the form (<see cref="LeaderboardData"/> result, <see cref="bool"/> ioError) that will be invoked when the process completes</param>
        public static void Get(string name, Action<LeaderboardData, bool> callback) => API.Leaderboards.Client.Find(name, callback);
        /// <summary>
        /// Get the board based on its ID
        /// </summary>
        /// <param name="id">The id of the board to get</param>
        /// <returns>The resulting <see cref="LeaderboardData"/></returns>
        public static LeaderboardData Get(ulong id) => id;
        /// <summary>
        /// Get the board based on its ID
        /// </summary>
        /// <param name="id">The id of the board to get</param>
        /// <returns>The resulting <see cref="LeaderboardData"/></returns>
        public static LeaderboardData Get(SteamLeaderboard_t id) => id;
        /// <summary>
        /// An internal helper struct used to fetch all leaderboards in sequence on initialization of the Steam API
        /// </summary>
        public struct GetAllRequest
        {
            public bool create;
            public string name;
            public ELeaderboardDisplayType type;
            public ELeaderboardSortMethod sort;
        }
        /// <summary>
        /// Get the details of the leaderboards provided
        /// </summary>
        /// <param name="boards">The boards to get</param>
        /// <param name="callback">A delegate of the form (<see cref="EResult"/> result) that will be invoked when the process completes</param>
        public static void GetAll(LeaderboardObject[] boards, Action<EResult> callback)
        {
            if (boards == null || boards.Length == 0)
            {
                callback?.Invoke(EResult.k_EResultOK);
                return;
            }

            if(SteamSettings.current != null && SteamSettings.current.isDebugging)
            {
                Debug.Log($"Beginning GetAll for {boards.Length} boards.");
            }
            
            if(boards.Any(b => b == null || string.IsNullOrEmpty(b.apiName)))
            {
                Debug.LogError("Errors have been found with the Leaderboard Objects provided. Please review your Leaderboard Objects and try again.");
                callback?.Invoke(EResult.k_EResultUnexpectedError);
                return;
            }

            try
            {
                var commands = new GetAllRequest[boards.Length];
                for (int i = 0; i < boards.Length; i++)
                {
                    commands[i] = new GetAllRequest
                    {
                        create = boards[i].createIfMissing,
                        name = boards[i].apiName,
                        sort = boards[i].sortMethod,
                        type = boards[i].displayType
                    };
                }

                var bgWorker = new BackgroundWorker();
                bgWorker.DoWork += BgWorker_DoWork;
                bgWorker.RunWorkerCompleted += (sender, arguments) =>
                {
                    if (arguments.Cancelled)
                        callback?.Invoke(EResult.k_EResultCancelled);
                    else if (arguments.Error != null)
                        callback?.Invoke(EResult.k_EResultUnexpectedError);
                    else
                    {
                        var results = arguments.Result as LeaderboardData[];
                        for (int i = 0; i < results.Length; i++)
                        {
                            boards[i].data = results[i];
                        }
                        callback?.Invoke(EResult.k_EResultOK);
                    }

                    bgWorker.Dispose();
                };
                bgWorker.RunWorkerAsync(commands);
            }
            catch (Exception ex)
            {
                Debug.LogError("Get All Leaderboards experienced and unhandled exception: " + ex.ToString());
                callback?.Invoke(EResult.k_EResultUnexpectedError);
            }
        }
        /// <summary>
        /// Get all the boards indicated by the input
        /// </summary>
        /// <param name="commands">The set of commands representing each board to be fetched</param>
        /// <param name="callback">A delegate of the form (<see cref="LeaderboardData"/>[] boards, <see cref="EResult"/> result) that will be invoked when completed</param>
        public static void GetAll(GetAllRequest[] commands, Action<LeaderboardData[], EResult> callback)
        {
            if (commands == null || commands.Length == 0)
            {
                callback?.Invoke(null, EResult.k_EResultOK);
                return;
            }

            var boards = new LeaderboardData[commands.Length];

            try
            {
                var bgWorker = new BackgroundWorker();
                bgWorker.DoWork += BgWorker_DoWork;
                bgWorker.RunWorkerCompleted += (sender, arguments) =>
                {
                    if (arguments.Cancelled)
                        callback?.Invoke(null, EResult.k_EResultCancelled);
                    else if (arguments.Error != null)
                        callback?.Invoke(null, EResult.k_EResultUnexpectedError);
                    else
                    {
                        var results = arguments.Result as LeaderboardData[];
                        for (int i = 0; i < results.Length; i++)
                        {
                            boards[i] = results[i];
                        }
                        callback?.Invoke(boards, EResult.k_EResultOK);
                    }

                    bgWorker.Dispose();
                };
                bgWorker.RunWorkerAsync(commands);
            }
            catch (Exception ex)
            {
                Debug.LogError("Get All Leaderboards experienced and unhandled exception: " + ex.ToString());
                callback?.Invoke(null, EResult.k_EResultUnexpectedError);
            }
        }

        private static void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var boards = e.Argument as GetAllRequest[];
            var results = new LeaderboardData[boards.Length];

            for (int i = 0; i < boards.Length; i++)
            {
                try
                {
                    var board = boards[i];
                    bool waiting = true;
                    if (board.create)
                        GetOrCreate(board.name, board.type, board.sort, (result, error) =>
                        {
                            results[i] = result;
                            waiting = false;
                        });
                    else
                        Get(board.name, (result, error) =>
                        {
                            results[i] = result;
                            waiting = false;
                        });

                    while (waiting)
                    {
                        Thread.Sleep(10);
                    }
                }
                catch
                {
                    results[i] = default;
                }
            }

            e.Result = results;
        }
        /// <summary>
        /// Find or if not available create the indicated board
        /// </summary>
        /// <param name="name">The API name of the board</param>
        /// <param name="displayType">How the board should display</param>
        /// <param name="sortMethod">The sorting method of the board</param>
        /// <param name="callback">A delegate of the form (<see cref="LeaderboardData"/> board, <see cref="bool"/> ioError) that will be called when completed</param>
        public static void GetOrCreate(string name, ELeaderboardDisplayType displayType, ELeaderboardSortMethod sortMethod, Action<LeaderboardData, bool> callback) => API.Leaderboards.Client.FindOrCreate(name, sortMethod, displayType, callback);

        /// <summary>
        /// Uploads a score for the player to this board
        /// </summary>
        /// <param name="score"></param>
        /// <param name="method"></param>
        /// <param name="callback">A delegate of the form (<see cref="LeaderboardScoreUploaded"/> results, <see cref="bool"/> ioError) that will be invoked when the process is completed</param>
        public readonly void UploadScore(int score, ELeaderboardUploadScoreMethod method, Action<LeaderboardScoreUploaded, bool> callback = null) => API.Leaderboards.Client.UploadScore(id, method, score, null, callback);

        /// <summary>
        /// Uploads a score for the player to this board
        /// </summary>
        /// <param name="score"></param>
        /// <param name="method"></param>
        /// <param name="callback">A delegate of the form (<see cref="LeaderboardScoreUploaded"/> results, <see cref="bool"/> ioError) that will be invoked when the process is completed</param>
        public readonly void UploadScore(int score, int[] scoreDetails, ELeaderboardUploadScoreMethod method, Action<LeaderboardScoreUploaded, bool> callback = null) => API.Leaderboards.Client.UploadScore(id, method, score, scoreDetails, callback);

        /// <summary>
        /// Attempts to save, share and attach an object to the leaderboard
        /// </summary>
        /// <remarks>
        /// Note that this depends on being able to save the file to the User's Remote Storage which is a limited resource so use this sparingly.
        /// </remarks>
        /// <param name="fileName">The name the file should be saved as. This must be unique on the user's storage</param>
        /// <param name="jsonObject">A JsonUtility serializable object, we will serialize this to UTF8 format and then convert to byte[] for you and upload to Steam Remote Storage</param>
        /// <param name="callback">A delegate of the form (<see cref="LeaderboardUGCSet"/> results, <see cref="bool"/> ioError) that will be invoked when the process is completed</param>
        public readonly void AttachUGC(string fileName, object jsonObject, System.Text.Encoding encoding, Action<LeaderboardUGCSet, bool> callback = null) => API.Leaderboards.Client.AttachUGC(id, fileName, jsonObject, encoding, callback);
        /// <summary>
        /// Attempts to save, share and attach an object to the leaderboard
        /// </summary>
        /// <remarks>
        /// Note that this depends on being able to save the file to the User's Remote Storage which is a limited resource so use this sparingly.
        /// </remarks>
        /// <param name="fileName">The name the file should be saved as. This must be unique on the user's storage</param>
        /// <param name="jsonObject">A JsonUtility serializable object, we will serialize this to UTF8 format and then convert to byte[] for you and upload to Steam Remote Storage</param>
        /// <param name="callback">A delegate of the form (<see cref="LeaderboardUGCSet"/> results, <see cref="bool"/> ioError) that will be invoked when the process is completed</param>
        public readonly void AttachUGC(string fileName, object jsonObject, Action<LeaderboardUGCSet, bool> callback = null) => API.Leaderboards.Client.AttachUGC(id, fileName, jsonObject, System.Text.Encoding.UTF8, callback);
        /// <summary>
        /// Uploads a score using the <see cref="ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate"/> option
        /// </summary>
        /// <param name="score">The score to upload</param>
        public readonly void ForceUploadScore(string score)
        {
            if (int.TryParse(score, out int result))
                UploadScore(result, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate);
        }
        /// <summary>
        /// Uploads a score using the <see cref="ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate"/> option
        /// </summary>
        /// <param name="score">The score to upload</param>
        public readonly void ForceUploadScore(int score) => UploadScore(score, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate);
        /// <summary>
        /// Uploads a score using the <see cref="ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate"/> option
        /// </summary>
        /// <param name="score">The score to upload</param>
        /// <param name="details">additional details to be attached to the board</param>
        public readonly void ForceUploadScore(int score, int[] details) => UploadScore(score, details, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate);
        /// <summary>
        /// Uploads a score using the <see cref="ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest"/> option
        /// </summary>
        /// <param name="score">The score to upload</param>
        public readonly void KeepBestUploadScore(string score)
        {
            if (int.TryParse(score, out int result))
                UploadScore(result, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest);
        }
        /// <summary>
        /// Uploads a score using the <see cref="ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest"/> option
        /// </summary>
        /// <param name="score">The score to upload</param>
        public readonly void KeepBestUploadScore(int score) => UploadScore(score, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest);
        /// <summary>
        /// Uploads a score using the <see cref="ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest"/> option
        /// </summary>
        /// <param name="score">The score to upload</param>
        /// <param name="details">additional details to be attached to the board</param>
        public readonly void KeepBestUploadScore(int score, int[] details) => UploadScore(score, details, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest);

        #region Boilerplate
        public readonly override string ToString()
        {
            return apiName;
        }

        public readonly override int GetHashCode()
        {
            return id.GetHashCode() + apiName.GetHashCode();
        }

        public readonly override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(SteamLeaderboard_t))
                return Equals((SteamLeaderboard_t)obj);
            else if (obj.GetType() == typeof(string))
                return Equals((string)obj);
            else if (obj.GetType() == typeof(ulong))
                return Equals((ulong)obj);
            else
                return id.Equals(obj);
        }

        public readonly bool Equals(SteamLeaderboard_t other)
        {
            return id.Equals(other);
        }

        public readonly bool Equals(ulong other)
        {
            return id.m_SteamLeaderboard.Equals(other);
        }

        public readonly bool Equals(string other)
        {
            return apiName.Equals(other);
        }

        public static bool operator ==(LeaderboardData l, LeaderboardData r) => l.id == r.id;
        public static bool operator ==(LeaderboardData l, ulong r) => l.id.m_SteamLeaderboard == r;
        public static bool operator ==(LeaderboardData l, string r) => l.apiName == r;
        public static bool operator ==(LeaderboardData l, SteamLeaderboard_t r) => l.id == r;
        public static bool operator !=(LeaderboardData l, LeaderboardData r) => l.id != r.id;
        public static bool operator !=(LeaderboardData l, ulong r) => l.id.m_SteamLeaderboard != r;
        public static bool operator !=(LeaderboardData l, string r) => l.apiName != r;
        public static bool operator !=(LeaderboardData l, SteamLeaderboard_t r) => l.id != r;

        public static implicit operator ulong(LeaderboardData c) => c.id.m_SteamLeaderboard;
        public static implicit operator LeaderboardData(ulong id) => new LeaderboardData { id = new SteamLeaderboard_t(id), apiName = API.Leaderboards.Client.GetName(new SteamLeaderboard_t(id)) };
        public static implicit operator SteamLeaderboard_t(LeaderboardData c) => c.id;
        public static implicit operator LeaderboardData(SteamLeaderboard_t id) => new LeaderboardData { id = id };
        public static implicit operator string(LeaderboardData c) => c.apiName;
        #endregion
    }
}
#endif