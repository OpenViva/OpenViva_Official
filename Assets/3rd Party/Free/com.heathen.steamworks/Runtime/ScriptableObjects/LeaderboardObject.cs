#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// <para>Represents a Steamworks Leaderboard and manages its entries and quires</para>
    /// <para>To create a new <see cref="LeaderboardObject"/> object in your project right click in a folder in your project and select</para>
    /// <para>Create >> Steamworks >> Player Services >> Leaderboard Data</para>
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/leaderboard-object")]
    [CreateAssetMenu(menuName = "Steamworks/Leaderboard Object")]
    public class LeaderboardObject : ScriptableObject
    {
        /// <summary>
        /// Should the board be created if missing on the target app
        /// </summary>
        public bool createIfMissing;
        /// <summary>
        /// If creating a board what sort method should be applied
        /// </summary>
        public ELeaderboardSortMethod sortMethod = ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending;
        /// <summary>
        /// If creating a board what display type is it
        /// </summary>
        public ELeaderboardDisplayType displayType = ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric;
        /// <summary>
        /// What is the name of the board ... if this is not to be created at run time then this must match the name as it appears in Steamworks
        /// </summary>
        [HideInInspector]
        public string apiName;
        /// <summary>
        /// The board's display name if any
        /// </summary>
        public string DisplayName => data.DisplayName;
        /// <summary>
        /// How many detail entries should be allowed on entries from this board
        /// </summary>
        [HideInInspector]
        public int maxDetailEntries = 0;
        /// <summary>
        /// What is the leaderboard ID ... this is nullable if null then no leaderboard has been connected
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public LeaderboardData data;
        /// <summary>
        /// Is the board valid and ready for use
        /// </summary>
        public bool Valid => data.Valid;
        /// <summary>
        /// How many entries does this board have
        /// </summary>
        public int EntryCount => data.EntryCount;
        /// <summary>
        /// An event that is invoked when the local user's entry on this board is updated
        /// </summary>
        public UnityLeaderboardRankUpdateEvent UserEntryUpdated = new UnityLeaderboardRankUpdateEvent();

        /// <summary>
        /// Returns the user entry for the local user
        /// </summary>
        /// <param name="callback">The delegate to invoke when the process is complete</param>
        public void GetUserEntry(Action<LeaderboardEntry, bool> callback) => data.GetUserEntry(maxDetailEntries, callback);
        /// <summary>
        /// Invokes the callback with the query results
        /// </summary>
        /// <param name="request">The type of range to get from the board</param>
        /// <param name="start">The index to start downloading at</param>
        /// <param name="end">The index to end downloading at</param>
        /// <param name="callback">The delegate to invoke when the process is complete</param>
        public void GetEntries(ELeaderboardDataRequest request, int start, int end, Action<LeaderboardEntry[], bool> callback) => data.GetEntries(request, start, end, maxDetailEntries, callback);
        /// <summary>
        /// Invokes the callback with the query results 
        /// </summary>
        /// <param name="users">The users to get results for</param>
        /// <param name="callback">The delegate to invoke when the process is complete</param>
        public void GetEntries(UserData[] users, Action<LeaderboardEntry[], bool> callback) => data.GetEntries(users, maxDetailEntries, callback);
        /// <summary>
        /// Request all entries on the board, this should generally not be used
        /// </summary>
        /// <param name="maxDetailEntries"></param>
        /// <param name="callback"></param>
        public void GetAllEntries(int maxDetailEntries, Action<LeaderboardEntry[], bool> callback) => data.GetAllEntries(maxDetailEntries, callback);
        /// <summary>
        /// Registers the board on Steamworks creating if configured to do so or locating if not.
        /// </summary>
        public void Register()
        {
            if (createIfMissing)
                LeaderboardData.GetOrCreate(apiName, displayType, sortMethod, (result, error) =>
                {
                    if(error)
                        Debug.LogError($"Failed to create or find leaderboard {apiName}");
                    data = result;
                });

            else
                LeaderboardData.Get(apiName, (result, error) =>
                {
                    if (error)
                        Debug.LogError($"Failed to find leaderboard {apiName}");
                    data = result;
                });
        }

        /// <summary>
        /// Uploads a score for the player to this board
        /// </summary>
        /// <param name="score"></param>
        /// <param name="method"></param>
        /// <param name="callback">{ LeaderboardScoreUploaded_t result, bool error } optional callback that will pass the results to you, if error is true it indicates a failure.</param>
        public void UploadScore(int score, ELeaderboardUploadScoreMethod method, Action<LeaderboardScoreUploaded, bool> callback = null)
        {
            if (data.Valid)
                data.UploadScore(score, method, callback);
            else
            {
                if (createIfMissing)
                    LeaderboardData.GetOrCreate(apiName, displayType, sortMethod, (result, error) =>
                    {
                        data = result;

                        if (error)
                            Debug.LogError($"Failed to create or find leaderboard {apiName}");
                        else
                            data.UploadScore(score, method, callback);
                    });

                else
                    LeaderboardData.Get(apiName, (result, error) =>
                    {
                        data = result;

                        if (error)
                            Debug.LogError($"Failed to find leaderboard {apiName}");
                        else
                            data.UploadScore(score, method, callback);
                    });
            }
        }

        /// <summary>
        /// Uploads a score for the player to this board
        /// </summary>
        /// <param name="score"></param>
        /// <param name="method"></param>
        /// <param name="callback">{ LeaderboardScoreUploaded_t result, bool error } optional callback that will pass the results to you, if error is true it indicates a failure.</param>
        public void UploadScore(int score, int[] scoreDetails, ELeaderboardUploadScoreMethod method, Action<LeaderboardScoreUploaded, bool> callback = null)
        {
            if (data.Valid)
                data.UploadScore(score, scoreDetails, method, callback);
            else
            {
                if (createIfMissing)
                    LeaderboardData.GetOrCreate(apiName, displayType, sortMethod, (result, error) =>
                    {
                        data = result;

                        if (error)
                            Debug.LogError($"Failed to create or find leaderboard {apiName}");
                        else
                            data.UploadScore(score, scoreDetails, method, callback);
                    });

                else
                    LeaderboardData.Get(apiName, (result, error) =>
                    {
                        data = result;

                        if (error)
                            Debug.LogError($"Failed to find leaderboard {apiName}");
                        else
                            data.UploadScore(score, scoreDetails, method, callback);
                    });
            }
        }

        /// <summary>
        /// Attempts to save, share and attach an object to the leaderboard
        /// </summary>
        /// <remarks>
        /// Note that this depends on being able to save the file to the User's Remote Storage which is a limited resource so use this sparingly.
        /// </remarks>
        /// <param name="fileName">The name the file should be saved as. This must be unique on the user's storage</param>
        /// <param name="JsonObject">A JsonUtility serializable object, we will serialize this to UTF8 format and then convert to byte[] for you and upload to Steam Remote Storage</param>
        /// <param name="callback">{ LeaderboardUGCSet_t result, bool error } optional callback that will pass the results to you, if error is true it indicates a failure from Valve.</param>
        public void AttachUGC(string fileName, object jsonObject, System.Text.Encoding encoding, Action<LeaderboardUGCSet, bool> callback = null)
        {
            if (data.Valid)
                data.AttachUGC(fileName, jsonObject, encoding, callback);
            else
            {
                if (createIfMissing)
                    LeaderboardData.GetOrCreate(apiName, displayType, sortMethod, (result, error) =>
                    {
                        data = result;

                        if (error)
                            Debug.LogError($"Failed to create or find leaderboard {apiName}");
                        else
                            data.AttachUGC(fileName, jsonObject, encoding, callback);
                    });

                else
                    LeaderboardData.Get(apiName, (result, error) =>
                    {
                        data = result;

                        if (error)
                            Debug.LogError($"Failed to find leaderboard {apiName}");
                        else
                            data.AttachUGC(fileName, jsonObject, encoding, callback);
                    });
            }
        }
        /// <summary>
        /// Upload a score using the Force Upload option
        /// </summary>
        /// <param name="score">the score to upload</param>
        public void ForceUploadScore(string score)
        {
            if (data.Valid)
                data.ForceUploadScore(score);
            else
            {
                if (createIfMissing)
                    LeaderboardData.GetOrCreate(apiName, displayType, sortMethod, (result, error) =>
                    {
                        data = result;

                        if (error)
                            Debug.LogError($"Failed to create or find leaderboard {apiName}");
                        else
                            data.ForceUploadScore(score);
                    });

                else
                    LeaderboardData.Get(apiName, (result, error) =>
                    {
                        data = result;

                        if (error)
                            Debug.LogError($"Failed to find leaderboard {apiName}");
                        else
                            data.ForceUploadScore(score);
                    });
            }
        }
        /// <summary>
        /// Upload a score using the Force Upload option
        /// </summary>
        /// <param name="score">the score to upload</param>
        public void ForceUploadScore(int score)
        {
            if (data.Valid)
                data.ForceUploadScore(score);
            else
            {
                if (createIfMissing)
                    LeaderboardData.GetOrCreate(apiName, displayType, sortMethod, (result, error) =>
                    {
                        data = result;

                        if (error)
                            Debug.LogError($"Failed to create or find leaderboard {apiName}");
                        else
                            data.ForceUploadScore(score);
                    });

                else
                    LeaderboardData.Get(apiName, (result, error) =>
                    {
                        data = result;

                        if (error)
                            Debug.LogError($"Failed to find leaderboard {apiName}");
                        else
                            data.ForceUploadScore(score);
                    });
            }
        }
        /// <summary>
        /// Upload a score using the Keep Best option
        /// </summary>
        /// <param name="score">the score to upload</param>
        public void KeepBestUploadScore(string score)
        {
            if (data.Valid)
                data.KeepBestUploadScore(score);
            else
            {
                if (createIfMissing)
                    LeaderboardData.GetOrCreate(apiName, displayType, sortMethod, (result, error) =>
                    {
                        data = result;

                        if (error)
                            Debug.LogError($"Failed to create or find leaderboard {apiName}");
                        else
                            data.KeepBestUploadScore(score);
                    });

                else
                    LeaderboardData.Get(apiName, (result, error) =>
                    {
                        data = result;

                        if (error)
                            Debug.LogError($"Failed to find leaderboard {apiName}");
                        else
                            data.KeepBestUploadScore(score);
                    });
            }
        }
        /// <summary>
        /// Upload a score using the Keep Best option
        /// </summary>
        /// <param name="score">the score to upload</param>
        public void KeepBestUploadScore(int score)
        {
            if (data.Valid)
                data.KeepBestUploadScore(score);
            else
            {
                if (createIfMissing)
                    LeaderboardData.GetOrCreate(apiName, displayType, sortMethod, (result, error) =>
                    {
                        data = result;

                        if (error)
                            Debug.LogError($"Failed to create or find leaderboard {apiName}");
                        else
                            data.KeepBestUploadScore(score);
                    });

                else
                    LeaderboardData.Get(apiName, (result, error) =>
                    {
                        data = result;

                        if (error)
                            Debug.LogError($"Failed to find leaderboard {apiName}");
                        else
                            data.KeepBestUploadScore(score);
                    });
            }
        }
    }
}
#endif