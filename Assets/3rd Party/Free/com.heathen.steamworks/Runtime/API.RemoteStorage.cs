#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Provides functions for reading, writing, and accessing files which can be stored remotely in the Steam Cloud.
    /// </summary>
    public static class RemoteStorage
    {
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                eventRemoteStorageLocalFileChange = new RemoteStorageLocalFileChangeEvent();
                m_RemoteStorageLocalFileChange_t = null;
                m_RemoteStorageFileReadAsyncComplete_t = null;
                m_RemoteStorageFileShareResult_t = null;
                m_RemoteStorageFileWriteAsyncComplete_t = null;
                m_RemoteStorageDownloadUGCResult_t = null;
            }
            public static bool IsEnabledForAccount => SteamRemoteStorage.IsCloudEnabledForAccount();
            public static bool IsEnabledForApp
            {
                get => SteamRemoteStorage.IsCloudEnabledForApp();
                set => SteamRemoteStorage.SetCloudEnabledForApp(value);
            }
            public static bool IsEnabled => IsEnabledForAccount && IsEnabledForApp;

            /// <summary>
            /// If a Steam app is flagged for supporting dynamic Steam Cloud sync, and a sync occurs, this callback will be posted to the app if any local files changed.
            /// </summary>
            public static RemoteStorageLocalFileChangeEvent EventLocalFileChange
            {
                get
                {
                    if (m_RemoteStorageLocalFileChange_t == null)
                        m_RemoteStorageLocalFileChange_t = Callback<RemoteStorageLocalFileChange_t>.Create(eventRemoteStorageLocalFileChange.Invoke);

                    return eventRemoteStorageLocalFileChange;
                }
            }

            private static RemoteStorageLocalFileChangeEvent eventRemoteStorageLocalFileChange = new RemoteStorageLocalFileChangeEvent();

            private static Callback<RemoteStorageLocalFileChange_t> m_RemoteStorageLocalFileChange_t;

            private static CallResult<RemoteStorageFileReadAsyncComplete_t> m_RemoteStorageFileReadAsyncComplete_t;
            private static CallResult<RemoteStorageFileShareResult_t> m_RemoteStorageFileShareResult_t;
            private static CallResult<RemoteStorageFileWriteAsyncComplete_t> m_RemoteStorageFileWriteAsyncComplete_t;
            private static CallResult<RemoteStorageDownloadUGCResult_t> m_RemoteStorageDownloadUGCResult_t;

            /// <summary>
            /// Deletes a file from the local disk, and propagates that delete to the cloud.
            /// </summary>
            /// <remarks>
            /// This is meant to be used when a user actively deletes a file. Use FileForget if you want to remove a file from the Steam Cloud but retain it on the users local disk.
            /// </remarks>
            /// <param name="file"></param>
            /// <returns></returns>
            public static bool FileDelete(string file) => SteamRemoteStorage.FileDelete(file);
            /// <summary>
            /// Checks whether the specified file exists.
            /// </summary>
            /// <param name="file"></param>
            /// <returns></returns>
            public static bool FileExists(string file) => SteamRemoteStorage.FileExists(file);
            /// <summary>
            /// Deletes the file from remote storage, but leaves it on the local disk and remains accessible from the API.
            /// </summary>
            /// <remarks>
            /// <para>
            /// When you are out of Cloud space, this can be used to allow calls to FileWrite to keep working without needing to make the user delete files.
            /// </para>
            /// <para>
            /// How you decide which files to forget are up to you. It could be a simple Least Recently Used (LRU) queue or something more complicated.
            /// </para>
            /// <para>
            /// Requiring the user to manage their Cloud-ized files for a game, while is possible to do, it is never recommended. For instance, "Which file would you like to delete so that you may store this new one?" removes a significant advantage of using the Cloud in the first place: its transparency.
            /// </para>
            /// <para>
            /// Once a file has been deleted or forgotten, calling FileWrite will resynchronize it in the Cloud. Rewriting a forgotten file is the only way to make it persisted again.
            /// </para>
            /// </remarks>
            /// <param name="file"></param>
            /// <returns></returns>
            public static bool FileForget(string file) => SteamRemoteStorage.FileForget(file);
            /// <summary>
            /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
            /// </summary>
            /// <param name="file"></param>
            /// <returns></returns>
            public static byte[] FileRead(string file)
            {
                var size = SteamRemoteStorage.GetFileSize(file);
                var results = new byte[size];
                SteamRemoteStorage.FileRead(file, results, size);
                return results;
            }
            /// <summary>
            /// Reads the data from the file as text
            /// </summary>
            /// <param name="fileName">The name of the file to load</param>
            /// <param name="encoding">The text encoding of the file ... typically this will be System.TExt.Encoding.UTF8</param>
            /// <returns></returns>
            public static string FileReadString(string fileName, System.Text.Encoding encoding)
            {
                var size = SteamRemoteStorage.GetFileSize(fileName);

                var buffer = new byte[size];
                SteamRemoteStorage.FileRead(fileName, buffer, buffer.Length);
                return encoding.GetString(buffer);
            }
            /// <summary>
            /// Reads the data from the file as a JSON object
            /// </summary>
            /// <typeparam name="T">The object type that should be deserialized from the file's JSON string</typeparam>
            /// <param name="fileName">the name of the file to load</param>
            /// <param name="encoding">the text encoding of the file ... typically this will be System.Text.Encoding.UTF8</param>
            /// <returns></returns>
            public static T FileReadJson<T>(string fileName, System.Text.Encoding encoding)
            {
                var size = SteamRemoteStorage.GetFileSize(fileName);

                if (size <= 0)
                    return default;

                var buffer = new byte[size];
                SteamRemoteStorage.FileRead(fileName, buffer, buffer.Length);
                var JsonString = encoding.GetString(buffer);

                return JsonUtility.FromJson<T>(JsonString);
            }
            /// <summary>
            /// Starts an asynchronous read from a file.
            /// </summary>
            /// <param name="file"></param>
            /// <param name="callback"></param>
            public static void FileReadAsync(string file, Action<byte[], bool> callback)
            {
                if (callback == null)
                    return;

                if (m_RemoteStorageFileReadAsyncComplete_t == null)
                    m_RemoteStorageFileReadAsyncComplete_t = CallResult<RemoteStorageFileReadAsyncComplete_t>.Create();

                var size = SteamRemoteStorage.GetFileSize(file);
                var handle = SteamRemoteStorage.FileReadAsync(file, 0, (uint)size);
                m_RemoteStorageFileReadAsyncComplete_t.Set(handle, (r, e) =>
                {
                    if (!e && r.m_eResult == EResult.k_EResultOK)
                    {
                        var results = new byte[size];
                        SteamRemoteStorage.FileReadAsyncComplete(r.m_hFileReadAsync, results, r.m_cubRead);
                        callback.Invoke(results, e);
                    }
                    else
                    {
                        callback.Invoke(new byte[0], e);
                    }
                });
            }
            public static void FileShare(string file, Action<RemoteStorageFileShareResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_RemoteStorageFileShareResult_t == null)
                    m_RemoteStorageFileShareResult_t = CallResult<RemoteStorageFileShareResult_t>.Create();

                var handle = SteamRemoteStorage.FileShare(file);
                m_RemoteStorageFileShareResult_t.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Creates a new file, writes the bytes to the file, and then closes the file. If the target file already exists, it is overwritten.
            /// </summary>
            /// <param name="file">The name of the file to write to.</param>
            /// <param name="data">The bytes to write to the file.</param>
            /// <returns></returns>
            public static bool FileWrite(string file, byte[] data) => SteamRemoteStorage.FileWrite(file, data, data.Length);
            /// <summary>
            /// Creates a new file, writes the bytes to the file, and then closes the file. If the target file already exists, it is overwritten.
            /// </summary>
            /// <remarks>
            /// May return false under the following conditions:
            /// The file you're trying to write is larger than 100MiB as defined by k_unMaxCloudFileChunkSize.
            /// cubData is less than 0.
            /// pvData is NULL.
            /// You tried to write to an invalid path or filename.Because Steamworks Cloud is cross platform the files need to have valid names on all supported OSes and file systems. See Microsoft's documentation on Naming Files, Paths, and Namespaces.
            /// The current user's Steamworks Cloud storage quota has been exceeded. They may have run out of space, or have too many files.
            /// Steamworks could not write to the disk, the location might be read-only.
            /// </remarks>
            /// <param name="file"></param>
            /// <param name="body">The text to encode and save to the Valve Remote Storage servers</param>
            /// <param name="encoding">The text encoding to use ... usually System.Text.Encoding.UTF8</param>
            /// <returns>true if the write was successful. Otherwise, false.
            /// </returns>
            public static bool FileWrite(string file, string body, System.Text.Encoding encoding)
            {
                var data = encoding.GetBytes(body);
                return FileWrite(file, data);
            }
            public static bool FileWrite(string file, string body)
            {
                var data = System.Text.Encoding.UTF8.GetBytes(body);
                return FileWrite(file, data);
            }
            /// <summary>
            /// Creates a new file, writes the bytes to the file, and then closes the file. If the target file already exists, it is overwritten.
            /// </summary>
            /// <remarks>
            /// May return false under the following conditions:
            /// The file you're trying to write is larger than 100MiB as defined by k_unMaxCloudFileChunkSize.
            /// cubData is less than 0.
            /// pvData is NULL.
            /// You tried to write to an invalid path or filename.Because Steamworks Cloud is cross platform the files need to have valid names on all supported OSes and file systems. See Microsoft's documentation on Naming Files, Paths, and Namespaces.
            /// The current user's Steamworks Cloud storage quota has been exceeded. They may have run out of space, or have too many files.
            /// Steamworks could not write to the disk, the location might be read-only.
            /// </remarks>
            /// <param name="fileName"></param>
            /// <param name="JsonObject">the object to be serialized to a JSON string and saved to the target file. Any type that the UnityEngine.JsonUtility can handle can be used.</param>
            /// <param name="encoding">The text encoding to use ... usually System.Text.Encoding.UTF8</param>
            /// <returns>true if the write was successful. Otherwise, false.
            /// </returns>
            public static bool FileWrite(string fileName, object JsonObject, System.Text.Encoding encoding)
            {
                return FileWrite(fileName, JsonUtility.ToJson(JsonObject), encoding);
            }
            public static bool FileWrite(string fileName, object JsonObject)
            {
                return FileWrite(fileName, JsonUtility.ToJson(JsonObject), System.Text.Encoding.UTF8);
            }
            /// <summary>
            /// Creates a new file and asynchronously writes the raw byte data to the Steam Cloud, and then closes the file. If the target file already exists, it is overwritten.
            /// </summary>
            /// <param name="file"></param>
            /// <param name="data"></param>
            /// <param name="callback"></param>
            public static void FileWriteAsync(string file, byte[] data, Action<RemoteStorageFileWriteAsyncComplete_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_RemoteStorageFileWriteAsyncComplete_t == null)
                    m_RemoteStorageFileWriteAsyncComplete_t = CallResult<RemoteStorageFileWriteAsyncComplete_t>.Create();

                var handle = SteamRemoteStorage.FileWriteAsync(file, data, (uint)data.Length);
                m_RemoteStorageFileWriteAsyncComplete_t.Set(handle, callback.Invoke);
            }
            public static void FileWriteAsync(string file, string body, System.Text.Encoding encoding, Action<RemoteStorageFileWriteAsyncComplete_t, bool> callback)
            {
                var data = encoding.GetBytes(body);
                FileWriteAsync(file, data, callback);
            }
            public static void FileWriteAsync(string fileName, object JsonObject, System.Text.Encoding encoding, Action<RemoteStorageFileWriteAsyncComplete_t, bool> callback)
            {
                FileWriteAsync(fileName, JsonUtility.ToJson(JsonObject), encoding, callback);
            }
            /// <summary>
            /// Cancels a file write stream that was started by FileWriteStreamOpen.
            /// </summary>
            /// <param name="handle"></param>
            /// <returns></returns>
            public static bool FileWriteStreamCancel(UGCFileWriteStreamHandle_t handle) => SteamRemoteStorage.FileWriteStreamCancel(handle);
            /// <summary>
            /// Closes a file write stream that was started by FileWriteStreamOpen.
            /// </summary>
            /// <param name="handle"></param>
            /// <returns></returns>
            public static bool FileWriteStreamClose(UGCFileWriteStreamHandle_t handle) => SteamRemoteStorage.FileWriteStreamClose(handle);
            /// <summary>
            /// Creates a new file output stream allowing you to stream out data to the Steam Cloud file in chunks. If the target file already exists, it is not overwritten until FileWriteStreamClose has been called.
            /// </summary>
            /// <remarks>
            /// To write data out to this stream you can use FileWriteStreamWriteChunk, and then to close or cancel you use FileWriteStreamClose and FileWriteStreamCancel respectively.
            /// </remarks>
            /// <param name="file"></param>
            /// <returns></returns>
            public static UGCFileWriteStreamHandle_t FileWriteStreamOpen(string file) => SteamRemoteStorage.FileWriteStreamOpen(file);
            /// <summary>
            /// Writes a blob of data to the file write stream.
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            public static bool FileWriteStreamWriteChunk(UGCFileWriteStreamHandle_t handle, byte[] data) => SteamRemoteStorage.FileWriteStreamWriteChunk(handle, data, data.Length);
            public static int GetCachedUGCCount() => SteamRemoteStorage.GetCachedUGCCount();
            public static UGCHandle_t GetCachedUGCHandle(int index) => SteamRemoteStorage.GetCachedUGCHandle(index);
            public static UGCHandle_t[] GetCashedUGCHandles()
            {
                var count = SteamRemoteStorage.GetCachedUGCCount();
                var results = new UGCHandle_t[count];
                for (int i = 0; i < count; i++)
                {
                    results[i] = SteamRemoteStorage.GetCachedUGCHandle(i);
                }

                return results;
            }
            /// <summary>
            /// Gets the total number of local files synchronized by Steam Cloud.
            /// </summary>
            /// <returns></returns>
            public static int GetFileCount() => SteamRemoteStorage.GetFileCount();
            /// <summary>
            /// Gets a collection containing information about all of the files stored on the Steam Cloud system
            /// </summary>
            /// <returns></returns>
            public static RemoteStorageFile[] GetFiles()
            {
                var count = SteamRemoteStorage.GetFileCount();
                var results = new RemoteStorageFile[count];
                for (int i = 0; i < count; i++)
                {
                    var name = SteamRemoteStorage.GetFileNameAndSize(i, out int size);
                    var time = new DateTime(1970, 1, 1).AddSeconds(SteamRemoteStorage.GetFileTimestamp(name));

                    results[i] = new RemoteStorageFile
                    {
                        name = name,
                        size = size,
                        timestamp = time
                    };
                }
                return results;
            }
            /// <summary>
            /// Returns the subset of files found on the user's Steam Cloud that end with the specified value
            /// </summary>
            /// <param name="extension"></param>
            /// <returns></returns>
            public static RemoteStorageFile[] GetFiles(string extension)
            {
                var count = SteamRemoteStorage.GetFileCount();
                var results = new RemoteStorageFile[count];
                int found = 0;
                for (int i = 0; i < count; i++)
                {
                    var name = SteamRemoteStorage.GetFileNameAndSize(i, out int size);

                    if (name.ToLower().EndsWith(extension))
                    {
                        var time = new DateTime(1970, 1, 1).AddSeconds(SteamRemoteStorage.GetFileTimestamp(name));

                        results[found] = new RemoteStorageFile
                        {
                            name = name,
                            size = size,
                            timestamp = time
                        };

                        found++;
                    }
                }
                Array.Resize(ref results, found);
                return results;
            }
            /// <summary>
            /// Gets the specified file's last modified timestamp
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public static DateTime GetFileTimestamp(string name) => new DateTime(1970, 1, 1).AddSeconds(SteamRemoteStorage.GetFileTimestamp(name));
            /// <summary>
            /// Note: only applies to applications flagged as supporting dynamic Steam Cloud sync.
            /// </summary>
            /// <returns></returns>
            public static int GetLocalFileChangeCount() => SteamRemoteStorage.GetLocalFileChangeCount();
            /// <summary>
            /// Note: only applies to applications flagged as supporting dynamic Steam Cloud sync.
            /// </summary>
            /// <remarks>
            /// <para>
            /// After calling GetLocalFileChangeCount, use this method to iterate over the changes. The changes described have already been made to local files. Your application should take appropriate action to reload state from disk, and possibly notify the user.
            /// </para>
            /// <para>
            /// For example: The local system had been suspended, during which time the user played elsewhere and uploaded changes to the Steam Cloud. On resume, Steam downloads those changes to the local system before resuming the application. The application receives an RemoteStorageLocalFileChange_t, and uses GetLocalFileChangeCount and GetLocalFileChange to iterate those changes. Depending on the application structure and the nature of the changes, the application could:
            /// </para>
            /// <list type="bullet">
            /// <item>
            /// Re-load game progress to resume at exactly the point where the user was when they exited the game on the other device
            /// </item>
            /// <item>
            /// Notify the user of any synchronized changes that don't require reloading
            /// </item>
            /// <item>
            /// etc
            /// </item>
            /// </list>
            /// </remarks>
            /// <param name="index"></param>
            /// <param name="changeType"></param>
            /// <param name="pathType"></param>
            /// <returns></returns>
            public static string GetLocalFileChange(int index, out ERemoteStorageLocalFileChange changeType, out ERemoteStorageFilePathType pathType) => SteamRemoteStorage.GetLocalFileChange(index, out changeType, out pathType);
            /// <summary>
            /// Gets the number of bytes available, and used on the users Steam Cloud storage.
            /// </summary>
            /// <param name="totalBytes"></param>
            /// <param name="remainingBytes"></param>
            /// <returns></returns>
            public static bool GetQuota(out ulong totalBytes, out ulong remainingBytes) => SteamRemoteStorage.GetQuota(out totalBytes, out remainingBytes);
            public static ERemoteStoragePlatform GetSyncPlatforms(string file) => SteamRemoteStorage.GetSyncPlatforms(file);
            public static bool GetUGCDetails(UGCHandle_t handle, out AppId_t appId, out string name, out int size, out CSteamID owner) => SteamRemoteStorage.GetUGCDetails(handle, out appId, out name, out size, out owner);
            public static bool GetUGCDownloadProgress(UGCHandle_t handle, out int downloaded, out int expected) => SteamRemoteStorage.GetUGCDownloadProgress(handle, out downloaded, out expected);
            public static bool SetSyncPlatforms(string file, ERemoteStoragePlatform platform) => SteamRemoteStorage.SetSyncPlatforms(file, platform);
            public static void UGCDownload(UGCHandle_t handle, uint priority, Action<RemoteStorageDownloadUGCResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_RemoteStorageDownloadUGCResult_t == null)
                    m_RemoteStorageDownloadUGCResult_t = CallResult<RemoteStorageDownloadUGCResult_t>.Create();

                var callbackHandle = SteamRemoteStorage.UGCDownload(handle, priority);
                m_RemoteStorageDownloadUGCResult_t.Set(callbackHandle, callback.Invoke);
            }
            public static void UGCDownloadToLocation(UGCHandle_t handle, string location, uint priority, Action<RemoteStorageDownloadUGCResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_RemoteStorageDownloadUGCResult_t == null)
                    m_RemoteStorageDownloadUGCResult_t = CallResult<RemoteStorageDownloadUGCResult_t>.Create();

                var callbackHandle = SteamRemoteStorage.UGCDownloadToLocation(handle, location, priority);
                m_RemoteStorageDownloadUGCResult_t.Set(callbackHandle, callback.Invoke);
            }
            public static byte[] UGCRead(UGCHandle_t handle)
            {
                SteamRemoteStorage.GetUGCDetails(handle, out _, out _, out int size, out _);
                var results = new byte[size];
                SteamRemoteStorage.UGCRead(handle, results, size, 0, EUGCReadAction.k_EUGCRead_ContinueReadingUntilFinished);
                return results;
            }
        }
    }
}
#endif