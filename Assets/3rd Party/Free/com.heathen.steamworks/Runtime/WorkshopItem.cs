#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a Steam Workshop Item
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/objects/workshop-item")]
    [Serializable]
    public class WorkshopItem
    {
        /// <summary>
        /// The title of the item
        /// </summary>
        public string Title => itemDetails.m_rgchTitle;
        /// <summary>
        /// The description of the item
        /// </summary>
        public string Description => itemDetails.m_rgchDescription;
        /// <summary>
        /// The app this item was created for
        /// </summary>
        public AppData ConsumerApp => itemDetails.m_nConsumerAppID;
        /// <summary>
        /// The native <see cref="PublishedFileId_t"/> of the item
        /// </summary>
        public PublishedFileId_t FileId => itemDetails.m_nPublishedFileId;
        /// <summary>
        /// The <see cref="UserData"/> that owns the item
        /// </summary>
        public UserData Owner => new CSteamID(itemDetails.m_ulSteamIDOwner);
        /// <summary>
        /// The time stamp the item was created
        /// </summary>
        public DateTime TimeCreated => new DateTime(1970, 1, 1).AddSeconds(itemDetails.m_rtimeCreated);
        /// <summary>
        /// THe time stamp the item was last updated
        /// </summary>
        public DateTime TimeUpdated => new DateTime(1970, 1, 1).AddSeconds(itemDetails.m_rtimeUpdated);
        /// <summary>
        /// The count of UP votes
        /// </summary>
        public uint UpVotes => itemDetails.m_unVotesUp;
        /// <summary>
        /// The count of DOWN votes
        /// </summary>
        public uint DownVotes => itemDetails.m_unVotesDown;
        /// <summary>
        /// The averaged vote score
        /// </summary>
        public float VoteScore => itemDetails.m_flScore;
        /// <summary>
        /// Is the item banned
        /// </summary>
        public bool IsBanned => itemDetails.m_bBanned;
        /// <summary>
        /// Has the tags array been truncated due to length
        /// </summary>
        public bool IsTagsTruncated => itemDetails.m_bTagsTruncated;
        /// <summary>
        /// Is the local user subscribed to this item
        /// </summary>
        public bool IsSubscribed => API.UserGeneratedContent.ItemStateHasFlag(StateFlags, EItemState.k_EItemStateSubscribed);
        /// <summary>
        /// Does the item require an update
        /// </summary>
        public bool IsNeedsUpdate => API.UserGeneratedContent.ItemStateHasFlag(StateFlags, EItemState.k_EItemStateNeedsUpdate);
        /// <summary>
        /// Is the item installed
        /// </summary>
        public bool IsInstalled => API.UserGeneratedContent.ItemStateHasFlag(StateFlags, EItemState.k_EItemStateInstalled);
        /// <summary>
        /// Is the item being downloaded
        /// </summary>
        public bool IsDownloading => API.UserGeneratedContent.ItemStateHasFlag(StateFlags, EItemState.k_EItemStateDownloading);
        /// <summary>
        /// Is the item waiting to be downloaded
        /// </summary>
        public bool IsDownloadPending => API.UserGeneratedContent.ItemStateHasFlag(StateFlags, EItemState.k_EItemStateDownloadPending);
        /// <summary>
        /// If the item is being downloaded what is its progress
        /// </summary>
        public float DownloadCompletion
        {
            get
            {
                API.UserGeneratedContent.Client.GetItemDownloadInfo(FileId, out float value);
                return value;
            }
        }
        /// <summary>
        /// What is the size in bytes of the file
        /// </summary>
        public int FileSize => itemDetails.m_nFileSize;
        /// <summary>
        /// The folder path where the items content is located
        /// </summary>
        public DirectoryInfo FolderPath
        {
            get
            {
                API.UserGeneratedContent.Client.GetItemInstallInfo(FileId, out ulong _, out string path, out DateTime _);
                return new DirectoryInfo(path);
            }
        }
        /// <summary>
        /// The set of <see cref="EItemState"/> flags related to the item
        /// </summary>
        public EItemState StateFlags => (EItemState)SteamUGC.GetItemState(itemDetails.m_nPublishedFileId);
        /// <summary>
        /// The visibility state of the item
        /// </summary>
        public ERemoteStoragePublishedFileVisibility Visibility => itemDetails.m_eVisibility;
        /// <summary>
        /// The list of tags related to the item
        /// </summary>
        public string[] Tags => itemDetails.m_rgchTags?.Split(',');
        /// <summary>
        /// The preview image for the item if it has been downloaded and imported
        /// </summary>
        public Texture2D previewImage;
        /// <summary>
        /// The location on disk where the item should be located
        /// </summary>
        public string previewImageLocation;
        /// <summary>
        /// The native <see cref="SteamUGCDetails_t"/> related to the item
        /// </summary>
        public SteamUGCDetails_t SourceItemDetails => itemDetails;
        protected SteamUGCDetails_t itemDetails;
        /// <summary>
        /// The metadata related to the item if read
        /// </summary>
        public string metadata;
        /// <summary>
        /// THe key value pair tags related to the item if read
        /// </summary>
        public StringKeyValuePair[] keyValueTags;
        /// <summary>
        /// An event that is invoked when the preview image is imported
        /// </summary>
        public UnityEvent previewImageUpdated = new UnityEvent();
        /// <summary>
        /// Do Not Use, this is for internal use only
        /// </summary>
        public CallResult<RemoteStorageDownloadUGCResult_t> m_RemoteStorageDownloadUGCResult;
        /// <summary>
        /// Construct a new item based on existing details
        /// </summary>
        /// <param name="itemDetails"></param>
        public WorkshopItem(SteamUGCDetails_t itemDetails)
        {
            this.itemDetails = itemDetails;

            if (itemDetails.m_eFileType != EWorkshopFileType.k_EWorkshopFileTypeCommunity)
            {
                Debug.LogWarning("HeathenWorkshopReadItem is designed to display File Type = Community Item, this item is not a community item and may not load correctly.");
            }

            m_RemoteStorageDownloadUGCResult = CallResult<RemoteStorageDownloadUGCResult_t>.Create(HandleUGCDownload);

            if (itemDetails.m_nPreviewFileSize > 0)
            {
                var previewCall = SteamRemoteStorage.UGCDownload(itemDetails.m_hPreviewFile, 1);
                m_RemoteStorageDownloadUGCResult.Set(previewCall, HandleUGCDownloadPreviewFile);
            }
            else
            {
                Debug.LogWarning("Item [" + Title + "] has no preview file!");
            }
        }

        /// <summary>
        /// Get the WorkshopItem for the indicated file
        /// </summary>
        /// <param name="file">The file to get</param>
        /// <param name="callback">Will respond when the query comletes with the file in question</param>
        public static void Get(PublishedFileId_t file, Action<WorkshopItem> callback)
        {
            var query = UgcQuery.Get(file);
            query.SetReturnLongDescription(true);
            query.SetReturnMetadata(true);
            query.Execute(r =>
            {
                callback?.Invoke(r.ResultsList != null && r.ResultsList.Count > 0 ? r.ResultsList[0] : null);
                query.Dispose();
            });
        }
        /// <summary>
        /// Get a <see cref="UgcQuery"/> to fetch a specific set of items
        /// </summary>
        /// <param name="files">The items to query</param>
        /// <returns>The <see cref="UgcQuery"/> ready to be executed</returns>
        public static UgcQuery Get(IEnumerable<PublishedFileId_t> files) => UgcQuery.Get(files);
        /// <summary>
        /// Get a <see cref="UgcQuery"/> of all the items this player has published
        /// </summary>
        /// <returns>The <see cref="UgcQuery"/> ready to be executed</returns>
        public static UgcQuery GetMyPublished() => UgcQuery.GetMyPublished();
        /// <summary>
        /// Get a <see cref="UgcQuery"/> of all the items this player has published
        /// </summary>
        /// <param name="creatorApp">The App ID of the APP that created the items</param>
        /// <param name="consumerApp">The App ID of the APP that is meant to consume the items</param>
        /// <returns>The <see cref="UgcQuery"/> ready to be executed</returns>
        public static UgcQuery GetMyPublished(AppData creatorApp, AppData consumerApp) => UgcQuery.GetMyPublished(creatorApp, consumerApp);
        /// <summary>
        /// Get a <see cref="UgcQuery"/> of all the items this player is subscribed to e.g. the ones they have ticket to include in there game
        /// </summary>
        /// <returns>The <see cref="UgcQuery"/> ready to be executed</returns>
        public static UgcQuery GetSubscribed() => UgcQuery.GetSubscribed();
        /// <summary>
        /// Get a <see cref="UgcQuery"/> of all the items this player has played
        /// </summary>
        /// <returns>The <see cref="UgcQuery"/> ready to be executed</returns>
        public static UgcQuery GetPlayed() => UgcQuery.GetPlayed();
        /// <summary>
        /// Get a <see cref="UgcQuery"/> of all the items this player has played
        /// </summary>
        /// <returns>The <see cref="UgcQuery"/> ready to be executed</returns>
        public static UgcQuery GetPlayed(AppData creatorApp, AppData consumerApp) => UgcQuery.GetPlayed(creatorApp, consumerApp);
        /// <summary>
        /// Update the title of this item, only works if this player is the owner
        /// </summary>
        /// <param name="value">The new title to apply</param>
        /// <param name="changeNote">The change note to include with the update</param>
        /// <param name="callback">A delegate of the form (<see cref="SubmitItemUpdateResult_t"/> result, <see cref="bool"/> ioError) that will be invoked when completed</param>
        public void UpdateTitle(string value, string changeNote, Action<SubmitItemUpdateResult_t, bool> callback)
        {
            var handle = API.UserGeneratedContent.Client.StartItemUpdate(ConsumerApp, FileId);

            if (SteamUGC.SetItemTitle(handle, value))
                API.UserGeneratedContent.Client.SubmitItemUpdate(handle, changeNote, callback);
            else
                callback?.Invoke(new SubmitItemUpdateResult_t { m_eResult = EResult.k_EResultInvalidParam }, true);
        }
        /// <summary>
        /// Update the title of this item, only works if this player is the owner
        /// </summary>
        /// <param name="value">The new title to apply</param>
        /// <param name="language">The language this title applies to</param>
        /// <param name="changeNote">The change note to include with the update</param>
        /// <param name="callback">A delegate of the form (<see cref="SubmitItemUpdateResult_t"/> result, <see cref="bool"/> ioError) that will be invoked when completed</param>
        public void UpdateTitle(string value, LanguageCodes language, string changeNote, Action<SubmitItemUpdateResult_t, bool> callback)
        {
            var handle = API.UserGeneratedContent.Client.StartItemUpdate(ConsumerApp, FileId);

            if (SteamUGC.SetItemUpdateLanguage(handle, language.ToString()))
                if (!SteamUGC.SetItemTitle(handle, value))
                    API.UserGeneratedContent.Client.SubmitItemUpdate(handle, changeNote, callback);
                else
                    callback?.Invoke(new SubmitItemUpdateResult_t { m_eResult = EResult.k_EResultInvalidParam }, true);
            else
                callback?.Invoke(new SubmitItemUpdateResult_t { m_eResult = EResult.k_EResultInvalidParam }, true);
        }
        /// <summary>
        /// Update the description of this item, only works if this player is the owner
        /// </summary>
        /// <param name="value">The new description to apply</param>
        /// <param name="changeNote">The change note to include with the update</param>
        /// <param name="callback">A delegate of the form (<see cref="SubmitItemUpdateResult_t"/> result, <see cref="bool"/> ioError) that will be invoked when completed</param>
        public void UpdateDescription(string value, string changeNote, Action<SubmitItemUpdateResult_t, bool> callback)
        {
            var handle = API.UserGeneratedContent.Client.StartItemUpdate(ConsumerApp, FileId);

            if (SteamUGC.SetItemDescription(handle, value))
                API.UserGeneratedContent.Client.SubmitItemUpdate(handle, changeNote, callback);
            else
                callback?.Invoke(new SubmitItemUpdateResult_t { m_eResult = EResult.k_EResultInvalidParam }, true);
        }
        /// <summary>
        /// Update the description of this item, only works if this player is the owner
        /// </summary>
        /// <param name="value">The new description to apply</param>
        /// <param name="language">The language this title applies to</param>
        /// <param name="changeNote">The change note to include with the update</param>
        /// <param name="callback">A delegate of the form (<see cref="SubmitItemUpdateResult_t"/> result, <see cref="bool"/> ioError) that will be invoked when completed</param>
        public void UpdateDescription(string value, LanguageCodes language, string changeNote, Action<SubmitItemUpdateResult_t, bool> callback)
        {
            var handle = API.UserGeneratedContent.Client.StartItemUpdate(ConsumerApp, FileId);

            if (SteamUGC.SetItemUpdateLanguage(handle, language.ToString()))
                if (SteamUGC.SetItemDescription(handle, value))
                    API.UserGeneratedContent.Client.SubmitItemUpdate(handle, changeNote, callback);
                else
                    callback?.Invoke(new SubmitItemUpdateResult_t { m_eResult = EResult.k_EResultInvalidParam }, true);
            else
                callback?.Invoke(new SubmitItemUpdateResult_t { m_eResult = EResult.k_EResultInvalidParam }, true);
        }

        /// <summary>
        /// Updates the content of the item
        /// </summary>
        /// <param name="value">The content folder where the content of this item is located</param>
        /// <param name="callback">A delegate of the form (<see cref="SubmitItemUpdateResult_t"/> result, <see cref="bool"/> ioError) that will be invoked when completed</param>
        public void UpdateContent(DirectoryInfo value, string changeNote, Action<SubmitItemUpdateResult_t, bool> callback)
        {
            var handle = API.UserGeneratedContent.Client.StartItemUpdate(ConsumerApp, FileId);

            if (SteamUGC.SetItemContent(handle, value.FullName))
                API.UserGeneratedContent.Client.SubmitItemUpdate(handle, changeNote, callback);
            else
                callback?.Invoke(new SubmitItemUpdateResult_t { m_eResult = EResult.k_EResultInvalidParam }, true);
        }

        /// <summary>
        /// Updates the preview image for the item
        /// </summary>
        /// <param name="value">The file to be uploaded as the preview image</param>
        /// <param name="callback">A delegate of the form (<see cref="SubmitItemUpdateResult_t"/> result, <see cref="bool"/> ioError) that will be invoked when completed</param>
        public void UpdateContent(FileInfo value, string changeNote, Action<SubmitItemUpdateResult_t, bool> callback)
        {
            var handle = API.UserGeneratedContent.Client.StartItemUpdate(ConsumerApp, FileId);

            if (SteamUGC.SetItemPreview(handle, value.FullName))
                API.UserGeneratedContent.Client.SubmitItemUpdate(handle, changeNote, callback);
            else
                callback?.Invoke(new SubmitItemUpdateResult_t { m_eResult = EResult.k_EResultInvalidParam }, true);
        }
        /// <summary>
        /// Update the metadata for this item, this only works if the local user is the owner of the item
        /// </summary>
        /// <param name="value"></param>
        /// <param name="changeNote"></param>
        /// <param name="callback">A delegate of the form (<see cref="SubmitItemUpdateResult_t"/> result, <see cref="bool"/> ioError) that will be invoked when completed</param>
        public void UpdateMetadata(string value, string changeNote, Action<SubmitItemUpdateResult_t, bool> callback)
        {
            var handle = API.UserGeneratedContent.Client.StartItemUpdate(ConsumerApp, FileId);

            if (SteamUGC.SetItemMetadata(handle, value))
                API.UserGeneratedContent.Client.SubmitItemUpdate(handle, changeNote, callback);
            else
                callback?.Invoke(new SubmitItemUpdateResult_t { m_eResult = EResult.k_EResultInvalidParam }, true);
        }
        /// <summary>
        /// Update the tags for this item, this only works if the local user is the owner of the item
        /// </summary>
        /// <param name="value"></param>
        /// <param name="changeNote"></param>
        /// <param name="callback">A delegate of the form (<see cref="SubmitItemUpdateResult_t"/> result, <see cref="bool"/> ioError) that will be invoked when completed</param>
        public void UpdateTags(string[] value, string changeNote, Action<SubmitItemUpdateResult_t, bool> callback)
        {
            var handle = API.UserGeneratedContent.Client.StartItemUpdate(ConsumerApp, FileId);

            if (SteamUGC.SetItemTags(handle, value))
                API.UserGeneratedContent.Client.SubmitItemUpdate(handle, changeNote, callback);
            else
                callback?.Invoke(new SubmitItemUpdateResult_t { m_eResult = EResult.k_EResultInvalidParam }, true);
        }
        /// <summary>
        /// Download the preview image for this item
        /// </summary>
        public void DownloadPreviewImage()
        {
            if (previewImage == null)
            {
                if (itemDetails.m_nPreviewFileSize > 0)
                {
                    var previewCall = SteamRemoteStorage.UGCDownload(itemDetails.m_hPreviewFile, 1);
                    m_RemoteStorageDownloadUGCResult.Set(previewCall, HandleUGCDownloadPreviewFile);
                }
                else
                {
                    Debug.LogWarning("Item [" + Title + "] has no preview file!");
                }
            }
        }

        /// <summary>
        /// Request delete of this item
        /// </summary>
        /// <param name="callback"></param>
        public void DeleteItem(Action<DeleteItemResult_t, bool> callback) => API.UserGeneratedContent.Client.DeleteItem(FileId, callback);
        /// <summary>
        /// Request download of this item
        /// </summary>
        /// <param name="highPriority"></param>
        /// <returns></returns>
        public bool DownloadItem(bool highPriority) => API.UserGeneratedContent.Client.DownloadItem(FileId, highPriority);
        /// <summary>
        /// Subscribe to the item
        /// </summary>
        /// <param name="callback"></param>
        public void Subscribe(Action<RemoteStorageSubscribePublishedFileResult_t, bool> callback) => API.UserGeneratedContent.Client.SubscribeItem(FileId, callback);
        /// <summary>
        /// Unsubscribe to the item
        /// </summary>
        /// <param name="callback"></param>
        public void Unsubscribe(Action<RemoteStorageUnsubscribePublishedFileResult_t, bool> callback) => API.UserGeneratedContent.Client.UnsubscribeItem(FileId, callback);
        /// <summary>
        /// Set the user's vote for this item
        /// </summary>
        /// <param name="voteUp"></param>
        /// <param name="callback"></param>
        public void SetVote(bool voteUp, Action<SetUserItemVoteResult_t, bool> callback) => API.UserGeneratedContent.Client.SetUserItemVote(FileId, voteUp, callback);
        /// <summary>
        /// Start playtime tracking
        /// </summary>
        /// <param name="callback">A delegate of the form (<see cref="StartPlaytimeTrackingResult_t"/> result, <see cref="bool"/> ioError) that will be invoked when the request is complete</param>
        public void StartPlayTime(Action<StartPlaytimeTrackingResult_t, bool> callback) => API.UserGeneratedContent.Client.StartPlaytimeTracking(new PublishedFileId_t[] { FileId }, callback);
        /// <summary>
        /// Stop playtime tracking
        /// </summary>
        /// <param name="callback">A delegate of the form (<see cref="StartPlaytimeTrackingResult_t"/> result, <see cref="bool"/> ioError) that will be invoked when the request is complete</param>
        public void StopPlayTime(Action<StopPlaytimeTrackingResult_t, bool> callback) => API.UserGeneratedContent.Client.StopPlaytimeTracking(new PublishedFileId_t[] { FileId }, callback);
        /// <summary>
        /// Generic handler useful for testing and debugging
        /// </summary>
        /// <param name="param"></param>
        /// <param name="bIOFailure"></param>
        private void HandleUGCDownload(RemoteStorageDownloadUGCResult_t param, bool bIOFailure)
        {
            if (!bIOFailure)
            {
                Debug.LogError("UGC Download generic handler loaded without failure.");
            }
            else
            {
                Debug.LogError("UGC Download request failed.");
            }
        }

        private void HandleUGCDownloadPreviewFile(RemoteStorageDownloadUGCResult_t param, bool bIOFailure)
        { 
            if (!bIOFailure)
            {
                if (param.m_eResult == EResult.k_EResultOK)
                {
                    byte[] imageBuffer = new byte[param.m_nSizeInBytes];
                    var count = SteamRemoteStorage.UGCRead(param.m_hFile, imageBuffer, param.m_nSizeInBytes, 0, EUGCReadAction.k_EUGCRead_ContinueReadingUntilFinished);
                    //Initialize the image, the LoadImage call will resize as required
                    previewImage = new Texture2D(2, 2);
                    previewImage.LoadImage(imageBuffer);
                    previewImageLocation = param.m_pchFileName;
                    previewImageUpdated.Invoke();

                }
                else
                {
                    Debug.LogError("UGC Download: unexpected result state: " + param.m_eResult.ToString() + "\nImage will not be loaded.");
                }
            }
            else
            {
                Debug.LogError("UGC Download request failed.");
            }
        }

        ~WorkshopItem()
        {
            GameObject.Destroy(previewImage);
        }
    }
}
#endif