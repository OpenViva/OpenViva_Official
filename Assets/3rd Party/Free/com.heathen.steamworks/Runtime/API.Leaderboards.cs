#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Heathen.SteamworksIntegration.API
{
    public static class Leaderboards
    {
        private struct AttachUGCRequest
        {
            public LeaderboardData leaderboard;
            public UGCHandle_t ugc;
            public Action<LeaderboardUGCSet, bool> callback;
        }

        private struct DownloadScoreRequest
        {
            public bool userRequest;
            public CSteamID[] users;
            public SteamLeaderboard_t leaderboard;
            public ELeaderboardDataRequest request;
            public int start;
            public int end;
            public int maxDetailsPerEntry;
            public Action<LeaderboardEntry[], bool> callback;
        }

        private struct UploadScoreRequest
        {
            public LeaderboardData leaderboard;
            public ELeaderboardUploadScoreMethod method;
            public int score;
            public int[] details;
            public Action<LeaderboardScoreUploaded, bool> callback;
        }

        private struct FindOrCreateRequest
        {
            public string apiName;
            public bool createIfMissing;
            public ELeaderboardDisplayType displayType;
            public ELeaderboardSortMethod sortMethod;
            public Action<LeaderboardData, bool> callback;
        }

        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                m_LeaderboardUGCSet_t = null;
                m_LeaderboardScoresDownloaded_t = null;
                m_LeaderboardFindResult_t = null;
                m_LeaderboardScoreUploaded_t = null;
                ugcQueue = null;
                downloadQueue = null;
                uploadQueue = null;
                findOrCreateQueue = null;
                RequestTimeout = 30f;
            }

            private static CallResult<LeaderboardUGCSet_t> m_LeaderboardUGCSet_t;
            private static CallResult<LeaderboardScoresDownloaded_t> m_LeaderboardScoresDownloaded_t;
            private static CallResult<LeaderboardFindResult_t> m_LeaderboardFindResult_t;
            private static CallResult<LeaderboardScoreUploaded_t> m_LeaderboardScoreUploaded_t;

            private static Queue<AttachUGCRequest> ugcQueue;
            private static Queue<DownloadScoreRequest> downloadQueue;
            private static Queue<UploadScoreRequest> uploadQueue;
            private static Queue<FindOrCreateRequest> findOrCreateQueue;

            private static void ExecuteUgcRequest()
            {
                var bgWorker = new BackgroundWorker();
                bgWorker.DoWork += (sender, e) =>
                {
                    bool waiting = false;

                    while (ugcQueue.Count > 0)
                    {
                        //Check on the next request
                        var request = ugcQueue.Peek();

                        //If the request is still valid
                        if (request.callback != null)
                        {
                            //Initiate the request with Valve
                            var handle = SteamUserStats.AttachLeaderboardUGC(request.leaderboard, request.ugc);

                            //Set our waiting and time out
                            waiting = true;

                            //Set our call result handler
                            m_LeaderboardUGCSet_t.Set(handle, (r, e) =>
                            {
                                request.callback.Invoke(r, e);
                                waiting = false;
                            });

                            //Wait until we get a result or until we timeout
                            while (waiting)
                                Thread.Sleep(100);
                        }

                        ugcQueue.Dequeue();
                    }
                };
                bgWorker.RunWorkerCompleted += (sender, e) =>
                {
                    bgWorker.Dispose();
                };
                bgWorker.RunWorkerAsync();
            }

            private static void ExecuteDownloadRequest()
            {
                var bgWorker = new BackgroundWorker();
                bgWorker.DoWork += (sender, e) =>
                {
                    bool waiting = false;

                    while (downloadQueue.Count > 0)
                    {
                        //Check on the next request
                        var request = downloadQueue.Peek();

                        //If the request is still valid
                        if (request.callback != null)
                        {
                            //Initiate the request with Valve
                            if (request.userRequest)
                            {
                                var handle = SteamUserStats.DownloadLeaderboardEntriesForUsers(request.leaderboard, request.users, request.users.Length);

                                //Set our waiting and time out
                                waiting = true;

                                m_LeaderboardScoresDownloaded_t.Set(handle, (results, error) =>
                                {
                                    request.callback.Invoke(ProcessScoresDownloaded(results, error, request.maxDetailsPerEntry), error);
                                    waiting = false;
                                });

                                //Wait until we get a result
                                while (waiting)
                                    Thread.Sleep(100);
                            }
                            else
                            {
                                var handle = SteamUserStats.DownloadLeaderboardEntries(request.leaderboard, request.request, request.start, request.end);

                                //Set our waiting and time out
                                waiting = true;

                                //Set our call result handler
                                m_LeaderboardScoresDownloaded_t.Set(handle, (results, error) =>
                                {
                                    request.callback.Invoke(ProcessScoresDownloaded(results, error, request.maxDetailsPerEntry), error);
                                    waiting = false;
                                });

                                //Wait until we get a result
                                while (waiting)
                                    Thread.Sleep(100);
                            }
                        }

                        downloadQueue.Dequeue();
                    }
                };
                bgWorker.RunWorkerCompleted += (sender, e) =>
                {
                    bgWorker.Dispose();
                };
                bgWorker.RunWorkerAsync();
            }

            private static void ExecuteUploadRequest()
            {
                var bgWorker = new BackgroundWorker();
                bgWorker.DoWork += (sender, e) =>
                {
                    bool waiting = false;

                    while (uploadQueue.Count > 0)
                    {
                        //Check on the next request
                        var request = uploadQueue.Peek();

                        //If the request is still valid

                        //Initiate the request with Valve
                        var handle = SteamUserStats.UploadLeaderboardScore(request.leaderboard, request.method, request.score, request.details, request.details == null ? 0 : request.details.Length);

                        waiting = true;

                        m_LeaderboardScoreUploaded_t.Set(handle, (r, e) =>
                        {
                            request.callback?.Invoke(r, e);

                            if (SteamSettings.current != null
                                && !e
                                && (r.m_bScoreChanged == 1 || r.m_nGlobalRankNew != r.m_nGlobalRankPrevious))
                            {
                                var leaderboard = SteamSettings.Leaderboards.FirstOrDefault(p => p != null && p.data.id == r.m_hSteamLeaderboard);
                                if (leaderboard != null)
                                    UserRankOrScoreUpdated(leaderboard);
                            }

                            waiting = false;
                        });

                        //Wait until we get a result
                        while (waiting)
                            Thread.Sleep(100);

                        if (waiting)
                        {
                            request.callback?.Invoke(default, true);
                            Debug.LogWarning("Leaderboard upload request exceeded the timeout of " + RequestTimeout + ", the callback will be called as a failure and next request serviced. The request may still come in at a later time.");
                        }

                        uploadQueue.Dequeue();
                    }
                };
                bgWorker.RunWorkerCompleted += (sender, e) =>
                    {
                        bgWorker.Dispose();
                    };
                bgWorker.RunWorkerAsync();
            }

            private static void ExecuteFindOrCreateRequest()
            {
                var bgWorker = new BackgroundWorker();
                bgWorker.DoWork += (sender, e) =>
                {
                    bool waiting = false;

                    while (findOrCreateQueue.Count > 0)
                    {
                        //Check on the next request
                        var request = findOrCreateQueue.Peek();

                        //If the request is still valid
                        if (request.callback != null)
                        {
                            //Initiate the request with Valve
                            if (request.createIfMissing)
                            {
                                var handle = SteamUserStats.FindOrCreateLeaderboard(request.apiName, request.sortMethod, request.displayType);

                                //Set our waiting and time out
                                waiting = true;

                                m_LeaderboardFindResult_t.Set(handle, (results, error) =>
                                {
                                    request.callback?.Invoke(new LeaderboardData
                                    {
                                        apiName = request.apiName,
                                        id = results.m_hSteamLeaderboard
                                    },
                                    error);
                                    waiting = false;
                                });

                                //Wait until we get a result
                                while (waiting)
                                    Thread.Sleep(100);
                            }
                            else
                            {
                                var handle = SteamUserStats.FindLeaderboard(request.apiName);

                                //Set our waiting and time out
                                waiting = true;

                                //Set our call result handler
                                m_LeaderboardFindResult_t.Set(handle, (results, error) =>
                                {
                                    request.callback?.Invoke(new LeaderboardData
                                    {
                                        apiName = request.apiName,
                                        id = results.m_hSteamLeaderboard
                                    },
                                    error);
                                    waiting = false;
                                });

                                //Wait until we get a result
                                while (waiting)
                                    Thread.Sleep(100);
                            }
                        }

                        findOrCreateQueue.Dequeue();
                    }
                };
                bgWorker.RunWorkerCompleted += (sender, e) =>
                {
                    bgWorker.Dispose();
                };
                bgWorker.RunWorkerAsync();
            }
            private static void UserRankOrScoreUpdated(LeaderboardObject leaderboard)
            {
                leaderboard.GetUserEntry((r, e) =>
                {
                    if (!e && r != null)
                        leaderboard.UserEntryUpdated?.Invoke(r);
                });
            }
            /// <summary>
            /// The amount of time to wait for Valve to respond to Leaderboard requests.
            /// </summary>
            public static float RequestTimeout { get; set; } = 30f;
            /// <summary>
            /// The number of pending requests to attach a UGC item to the user's leaderboard entry
            /// </summary>
            public static int PendingSetUgcRequests => ugcQueue == null ? 0 : ugcQueue.Count;
            /// <summary>
            /// The number of pending requests to download scores from a leaderboard
            /// </summary>
            public static int PendingDownloadScoreRequests => downloadQueue == null ? 0 : downloadQueue.Count;
            /// <summary>
            /// The number of pending requests to upload scores to a leaderboard for the local user
            /// </summary>
            public static int PendingUploadScoreRequests => uploadQueue == null ? 0 : uploadQueue.Count;

            /// <summary>
            /// Attaches a piece of user generated content the current user's entry on a leaderboard.
            /// </summary>
            /// <remarks>
            /// This content could be a replay of the user achieving the score or a ghost to race against. The attached handle will be available when the entry is retrieved and can be accessed by other users using GetDownloadedLeaderboardEntry which contains LeaderboardEntry_t.m_hUGC. To create and download user generated content see the documentation for the Steam Workshop.
            /// </remarks>
            /// <param name="leaderboard">A leaderboard handle obtained from FindLeaderboard or FindOrCreateLeaderboard.</param>
            /// <param name="ugc">Handle to a piece of user generated content that was shared using RemoteStorage.FileShare.</param>
            /// <param name="callback"></param>
            public static void AttachUGC(LeaderboardData leaderboard, UGCHandle_t ugc, Action<LeaderboardUGCSet, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_LeaderboardUGCSet_t == null)
                    m_LeaderboardUGCSet_t = CallResult<LeaderboardUGCSet_t>.Create();

                if (ugcQueue == null)
                    ugcQueue = new Queue<AttachUGCRequest>();

                var request = new AttachUGCRequest
                {
                    leaderboard = leaderboard,
                    ugc = ugc,
                    callback = callback
                };

                ugcQueue.Enqueue(request);

                //If we only have 1 enqueued then we need to start the execute, if we have more then its already running
                if (ugcQueue.Count == 1)
                    ExecuteUgcRequest();
            }
            /// <summary>
            /// Attaches a piece of user generated content the current user's entry on a leaderboard.
            /// </summary>
            /// <remarks>
            /// This content could be a replay of the user achieving the score or a ghost to race against. The attached handle will be available when the entry is retrieved and can be accessed by other users using GetDownloadedLeaderboardEntry which contains LeaderboardEntry_t.m_hUGC. To create and download user generated content see the documentation for the Steam Workshop.
            /// </remarks>
            public static void AttachUGC(LeaderboardData leaderboard, string fileName, byte[] data, Action<LeaderboardUGCSet, bool> callback)
            {
                if (callback == null)
                    return;

                API.RemoteStorage.Client.FileWriteAsync(fileName, data, (writeResult, writeError) =>
                {
                    if(!writeError)
                    {
                        API.RemoteStorage.Client.FileShare(fileName, (shareResult, shareError) =>
                        {
                            if (!shareError)
                            {
                                AttachUGC(leaderboard, shareResult.m_hFile, callback);
                            }
                            else
                            {
                                callback.Invoke(new LeaderboardUGCSet_t
                                {
                                    m_eResult = shareResult.m_eResult,
                                    m_hSteamLeaderboard = leaderboard
                                },
                                true);
                            }
                        });
                    }
                    else
                    {
                        callback.Invoke(new LeaderboardUGCSet_t
                        {
                            m_eResult = writeResult.m_eResult,
                            m_hSteamLeaderboard = leaderboard
                        },
                        true);
                    }
                });
            }
            public static void AttachUGC(LeaderboardData leaderboard, string fileName, object jsonObject, System.Text.Encoding encoding, Action<LeaderboardUGCSet, bool> callback)
            {
                if (callback == null)
                    return;

                API.RemoteStorage.Client.FileWriteAsync(fileName, jsonObject, encoding, (writeResult, writeError) =>
                {
                    if (!writeError)
                    {
                        API.RemoteStorage.Client.FileShare(fileName, (shareResult, shareError) =>
                        {
                            if (!shareError)
                            {
                                AttachUGC(leaderboard, shareResult.m_hFile, callback);
                            }
                            else
                            {
                                callback.Invoke(new LeaderboardUGCSet_t
                                {
                                    m_eResult = shareResult.m_eResult,
                                    m_hSteamLeaderboard = leaderboard
                                },
                                true);
                            }
                        });
                    }
                    else
                    {
                        callback.Invoke(new LeaderboardUGCSet_t
                        {
                            m_eResult = writeResult.m_eResult,
                            m_hSteamLeaderboard = leaderboard
                        },
                        true);
                    }
                });
            }
            public static void AttachUGC(LeaderboardData leaderboard, string fileName, string content, System.Text.Encoding encoding, Action<LeaderboardUGCSet, bool> callback)
            {
                if (callback == null)
                    return;

                API.RemoteStorage.Client.FileWriteAsync(fileName, content, encoding, (writeResult, writeError) =>
                {
                    if (!writeError)
                    {
                        API.RemoteStorage.Client.FileShare(fileName, (shareResult, shareError) =>
                        {
                            if (!shareError)
                            {
                                AttachUGC(leaderboard, shareResult.m_hFile, callback);
                            }
                            else
                            {
                                callback.Invoke(new LeaderboardUGCSet_t
                                {
                                    m_eResult = shareResult.m_eResult,
                                    m_hSteamLeaderboard = leaderboard
                                },
                                true);
                            }
                        });
                    }
                    else
                    {
                        callback.Invoke(new LeaderboardUGCSet_t
                        {
                            m_eResult = writeResult.m_eResult,
                            m_hSteamLeaderboard = leaderboard
                        },
                        true);
                    }
                });
            }
            /// <summary>
            /// Fetches a series of leaderboard entries for a specified leaderboard.
            /// </summary>
            /// <param name="leaderboard">A leaderboard handle obtained from FindLeaderboard or FindOrCreateLeaderboard.</param>
            /// <param name="request">The type of data request to make.</param>
            /// <param name="start">The index to start downloading entries relative to eLeaderboardDataRequest.</param>
            /// <param name="end">The last index to retrieve entries for relative to eLeaderboardDataRequest.</param>
            public static void DownloadEntries(LeaderboardData leaderboard, ELeaderboardDataRequest request, int start, int end, int maxDetailsPerEntry, Action<LeaderboardEntry[], bool> callback)
            {
                if (callback == null)
                    return;

                if (m_LeaderboardScoresDownloaded_t == null)
                    m_LeaderboardScoresDownloaded_t = CallResult<LeaderboardScoresDownloaded_t>.Create();


                if (downloadQueue == null)
                    downloadQueue = new Queue<DownloadScoreRequest>();

                var nRequest = new DownloadScoreRequest
                {
                    userRequest = false,
                    leaderboard = leaderboard,
                    request = request,
                    start = start,
                    end = end,
                    maxDetailsPerEntry = maxDetailsPerEntry,
                    callback = callback
                };

                downloadQueue.Enqueue(nRequest);

                //If we only have 1 enqueued then we need to start the execute, if we have more then its already running
                if (downloadQueue.Count == 1)
                    ExecuteDownloadRequest();
            }
            /// <summary>
            /// Fetches leaderboard entries for an arbitrary set of users on a specified leaderboard.
            /// </summary>
            /// <remarks>
            /// A maximum of 100 users can be downloaded at a time, with only one outstanding call at a time. If a user doesn't have an entry on the specified leaderboard, they won't be included in the result.
            /// </remarks>
            /// <param name="leaderboard">A leaderboard handle obtained from FindLeaderboard or FindOrCreateLeaderboard.</param>
            /// <param name="users">An array of Steam IDs to get the leaderboard entries for.</param>
            /// <param name="callback"></param>
            public static void DownloadEntries(LeaderboardData leaderboard, CSteamID[] users, int maxDetailsPerEntry, Action<LeaderboardEntry[], bool> callback)
            {
                if (callback == null)
                    return;

                if (m_LeaderboardScoresDownloaded_t == null)
                    m_LeaderboardScoresDownloaded_t = CallResult<LeaderboardScoresDownloaded_t>.Create();

                if (downloadQueue == null)
                    downloadQueue = new Queue<DownloadScoreRequest>();

                var nRequest = new DownloadScoreRequest
                {
                    userRequest = true,
                    leaderboard = leaderboard,
                    users = users,
                    maxDetailsPerEntry = maxDetailsPerEntry,
                    callback = callback
                };

                downloadQueue.Enqueue(nRequest);

                //If we only have 1 enqueued then we need to start the execute, if we have more then its already running
                if (downloadQueue.Count == 1)
                    ExecuteDownloadRequest();
            }
            public static void DownloadEntries(LeaderboardData leaderboard, UserData[] users, int maxDetailsPerEntry, Action<LeaderboardEntry[], bool> callback) => DownloadEntries(leaderboard, Array.ConvertAll(users, (i) => i.id), maxDetailsPerEntry, callback);
            /// <summary>
            /// Gets a leaderboard by name.
            /// </summary>
            /// <param name="leaderboardName">The name of the leaderboard to find. Must not be longer than <see cref="Constants.k_cchLeaderboardNameMax"/>.</param>
            /// <param name="callback"></param>
            public static void Find(string leaderboardName, Action<LeaderboardData, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_LeaderboardFindResult_t == null)
                    m_LeaderboardFindResult_t = CallResult<LeaderboardFindResult_t>.Create();

                if (findOrCreateQueue == null)
                    findOrCreateQueue = new Queue<FindOrCreateRequest>();

                findOrCreateQueue.Enqueue(new FindOrCreateRequest
                {
                    apiName = leaderboardName,
                    callback = callback,
                    createIfMissing = false
                });

                if (findOrCreateQueue.Count == 1)
                    ExecuteFindOrCreateRequest();
            }
            /// <summary>
            /// Gets a leaderboard by name, it will create it if it's not yet created.
            /// </summary>
            /// <remarks>
            /// Leaderboards created with this function will not automatically show up in the Steam Community. You must manually set the Community Name field in the App Admin panel of the Steamworks website. As such it's generally recommended to prefer creating the leaderboards in the App Admin panel on the Steamworks website and using FindLeaderboard unless you're expected to have a large amount of dynamically created leaderboards.
            /// </remarks>
            /// <param name="leaderboardName">The name of the leaderboard to find. Must not be longer than <see cref="Constants.k_cchLeaderboardNameMax"/>.</param>
            /// <param name="sortingMethod">The sort order of the new leaderboard if it's created.</param>
            /// <param name="displayType">The display type (used by the Steam Community web site) of the new leaderboard if it's created.</param>
            /// <param name="callback"></param>
            public static void FindOrCreate(string leaderboardName, ELeaderboardSortMethod sortingMethod, ELeaderboardDisplayType displayType, Action<LeaderboardData, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_LeaderboardFindResult_t == null)
                    m_LeaderboardFindResult_t = CallResult<LeaderboardFindResult_t>.Create();

                if (sortingMethod == ELeaderboardSortMethod.k_ELeaderboardSortMethodNone)
                {
                    Debug.LogError("You should never pass ELeaderboardSortMethod.k_ELeaderboardSortMethodNone for the sorting method as this is undefined behaviour.");
                    callback?.Invoke(default, true);
                    return;
                }

                if (displayType == ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNone)
                {
                    Debug.LogError("You should never pass ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNone for the display type as this is undefined behaviour.");
                    callback?.Invoke(default, true);
                    return;
                }

                if (findOrCreateQueue == null)
                    findOrCreateQueue = new Queue<FindOrCreateRequest>();

                findOrCreateQueue.Enqueue(new FindOrCreateRequest
                {
                    apiName = leaderboardName,
                    callback = callback,
                    createIfMissing = true,
                    sortMethod = sortingMethod,
                    displayType = displayType
                });

                if (findOrCreateQueue.Count == 1)
                    ExecuteFindOrCreateRequest();
            }
            /// <summary>
            /// Returns the display type of a leaderboard handle.
            /// </summary>
            /// <param name="leaderboard"></param>
            /// <returns></returns>
            public static ELeaderboardDisplayType GetDisplayType(LeaderboardData leaderboard) => SteamUserStats.GetLeaderboardDisplayType(leaderboard);
            /// <summary>
            /// Returns the total number of entries in a leaderboard.
            /// </summary>
            /// <param name="leaderboard"></param>
            /// <returns></returns>
            public static int GetEntryCount(LeaderboardData leaderboard) => SteamUserStats.GetLeaderboardEntryCount(leaderboard);
            /// <summary>
            /// Returns the name of a leaderboard handle.
            /// </summary>
            /// <param name="leaderboard"></param>
            /// <returns></returns>
            public static string GetName(LeaderboardData leaderboard) => SteamUserStats.GetLeaderboardName(leaderboard);
            /// <summary>
            /// Returns the sort order of a leaderboard handle.
            /// </summary>
            /// <param name="leaderboard"></param>
            /// <returns></returns>
            public static ELeaderboardSortMethod GetSortMethod(LeaderboardData leaderboard) => SteamUserStats.GetLeaderboardSortMethod(leaderboard);
            /// <summary>
            /// Uploads a user score to a specified leaderboard.
            /// </summary>
            /// <remarks>
            /// Details are optional game-defined information which outlines how the user got that score. For example if it's a racing style time based leaderboard you could store the timestamps when the player hits each checkpoint. If you have collectibles along the way you could use bit fields as Booleans to store the items the player picked up in the playthrough.
            /// <para>
            /// Uploading scores to Steam is rate limited to 10 uploads per 10 minutes and you may only have one outstanding call to this function at a time.
            /// </para>
            /// </remarks>
            /// <param name="leaderboard">A leaderboard handle obtained from FindLeaderboard or FindOrCreateLeaderboard.</param>
            /// <param name="method">Do you want to force the score to change, or keep the previous score if it was better?</param>
            /// <param name="score">The score to upload.</param>
            /// <param name="details"></param>
            public static void UploadScore(LeaderboardData leaderboard, ELeaderboardUploadScoreMethod method, int score, int[] details, Action<LeaderboardScoreUploaded, bool> callback = null)
            {
                if (m_LeaderboardScoreUploaded_t == null)
                    m_LeaderboardScoreUploaded_t = CallResult<LeaderboardScoreUploaded_t>.Create();

                if (uploadQueue == null)
                    uploadQueue = new Queue<UploadScoreRequest>();

                var nRequest = new UploadScoreRequest
                {
                    leaderboard = leaderboard,
                    method = method,
                    score = score,
                    details = details,
                    callback = callback,
                };

                uploadQueue.Enqueue(nRequest);

                //If we only have 1 enqueued then we need to start the execute, if we have more then its already running
                if (uploadQueue.Count == 1)
                    ExecuteUploadRequest();
            }

            private static LeaderboardEntry[] ProcessScoresDownloaded(LeaderboardScoresDownloaded_t param, bool bIOFailure, int maxDetailEntries)
            {
                ///Check for the current users data in the record set and update accordingly
                if (!bIOFailure)
                {
                    var userId = SteamUser.GetSteamID();
                    var entries = new LeaderboardEntry[param.m_cEntryCount];

                    for (int i = 0; i < param.m_cEntryCount; i++)
                    {
                        LeaderboardEntry_t buffer;
                        int[] details = null;

                        if (maxDetailEntries < 1)
                            SteamUserStats.GetDownloadedLeaderboardEntry(param.m_hSteamLeaderboardEntries, i, out buffer, details, maxDetailEntries);
                        else
                        {
                            details = new int[maxDetailEntries];
                            SteamUserStats.GetDownloadedLeaderboardEntry(param.m_hSteamLeaderboardEntries, i, out buffer, details, maxDetailEntries);
                        }
                        
                        LeaderboardEntry record = new LeaderboardEntry();
                        record.entry = buffer;
                        record.details = details;

                        entries[i] = record;
                    }

                    return entries;
                }
                else
                    return new LeaderboardEntry[0];
            }

        }
    }
}
#endif