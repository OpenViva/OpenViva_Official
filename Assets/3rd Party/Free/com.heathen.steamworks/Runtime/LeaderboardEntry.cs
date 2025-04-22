#if !DISABLESTEAMWORKS && STEAMWORKSNET
using Steamworks;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    public class LeaderboardEntry
    {
        public LeaderboardEntry_t entry;
        public int[] details;
        public UserData User => entry.m_steamIDUser;
        public int Rank => entry.m_nGlobalRank;
        public int Score => entry.m_nScore;
        public UGCHandle_t UgcHandle => entry.m_hUGC;
        public int this[int index] => details[index];
        public bool HasCashedUgcFileName => !string.IsNullOrEmpty(cashedUgcFileName);
        public string cashedUgcFileName = string.Empty;
        public UnityEvent<string> evtUgcDownloaded = new();

        /// <summary>
        /// Returns the object attached to the record if any
        /// </summary>
        /// <typeparam name="T">The type of the object attached, this should be a JsonUtility serializable type.</typeparam>
        /// <param name="callback">{ T result, bool failure } if failure = true then some error occurred and the file cannot be read.
        /// This may be due to an invalid <see cref="UgcHandle"/> or an IO Failure on Valve's part.
        /// <see cref="UgcHandle"/> will be invalid if there is no entry.</param>
        public void GetAttachedUgc<T>(Action<T, bool> callback = null)
        {
            if (UgcHandle == UGCHandle_t.Invalid)
            {
                callback?.Invoke(default, true);
            }
            else
            {
                API.RemoteStorage.Client.UGCDownload(UgcHandle, 0, (dr, de) =>
                {
                    if (!de && dr.m_eResult == EResult.k_EResultOK)
                    {
                        cashedUgcFileName = dr.m_pchFileName;
                        evtUgcDownloaded.Invoke(dr.m_pchFileName);

                        if (callback != null)
                        {
                            var buffer = API.RemoteStorage.Client.UGCRead(dr.m_hFile);
                            var jsonString = System.Text.Encoding.UTF8.GetString(buffer);
                            var result = JsonUtility.FromJson<T>(jsonString);
                            callback.Invoke(result, false);
                        }
                    }
                    else
                    {
                        cashedUgcFileName = string.Empty;
                        evtUgcDownloaded.Invoke(null);

                        callback?.Invoke(default, true);
                    }
                });
            }
        }

        /// <summary>
        /// Starts the process of downloading the UGC file.
        /// </summary>
        /// <remarks>
        /// Invokes the <see cref="evtUgcDownloaded"/> when completed
        /// </remarks>
        /// <param name="priority"></param>
        /// <returns></returns>
        public bool StartUgcDownload(uint priority = 0)
        {
            if (UgcHandle != UGCHandle_t.Invalid)
            {
                API.RemoteStorage.Client.UGCDownload(UgcHandle, priority, HandleUgcDownloadResult);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Starts the process of downloading the UGC file.
        /// </summary>
        /// <remarks>
        /// Invokes the <see cref="evtUgcDownloaded"/> and <paramref name="callback"/> when completed
        /// </remarks>
        /// <param name="priority"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool StartUgcDownload(uint priority, Action<RemoteStorageDownloadUGCResult_t, bool> callback)
        {
            if (UgcHandle != UGCHandle_t.Invalid)
            {
                API.RemoteStorage.Client.UGCDownload(UgcHandle, priority, (p, e) =>
                {
                    HandleUgcDownloadResult(p, e);
                    if (callback != null)
                        callback.Invoke(p, e);
                });

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Returns the % complete for the download where 0 = 0% and 1 = 100%
        /// </summary>
        /// <returns></returns>
        public float UgcDownloadProgress()
        {
            SteamRemoteStorage.GetUGCDownloadProgress(UgcHandle, out int downloaded, out int expected);

            return downloaded / (float)expected;
        }

        private void HandleUgcDownloadResult(RemoteStorageDownloadUGCResult_t param, bool bIOFailure)
        {
            if (!bIOFailure && param.m_eResult == EResult.k_EResultOK)
            {
                cashedUgcFileName = param.m_pchFileName;
                evtUgcDownloaded.Invoke(param.m_pchFileName);
            }
            else
            {
                cashedUgcFileName = string.Empty;
                evtUgcDownloaded.Invoke(null);
            }
        }
    }

}
#endif