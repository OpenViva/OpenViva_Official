#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// A helper tool that allows you to pre-define all of the typical fields of a Steam User Generated Content item (aka a Workshop Item) for quick and easy create or update. see the <see cref="API.UserGeneratedContent.Client.CreateItem(WorkshopItemData, WorkshopItemPreviewFile[], string[], WorkshopItemKeyValueTag[], Action{WorkshopItemDataCreateStatus}, Action{UGCUpdateHandle_t}, Action{CreateItemResult_t})"/> method for details.
    /// </summary>
    public struct WorkshopItemData
    {
        /// <summary>
        /// The published file ID to be updated. This can be null and would be in the case of a create, if you are however updating an item this should be populated with the ID of the file to be updated
        /// </summary>
        public PublishedFileId_t? publishedFileId;
        /// <summary>
        /// The consume and creating app ID of the item
        /// </summary>
        public AppData appId;
        /// <summary>
        /// The title of the item
        /// </summary>
        public string title;
        /// <summary>
        /// The description of the item
        /// </summary>
        public string description;
        /// <summary>
        /// The local folder where the item's content is located
        /// </summary>
        public DirectoryInfo content;
        /// <summary>
        /// The local file that is the item's main preview image, this must be smaller than the max size allowed in the app's Cloud Storage
        /// </summary>
        public FileInfo preview;
        /// <summary>
        /// Any metadata associated with the item, this is optional
        /// </summary>
        public string metadata;
        /// <summary>
        /// Any tags associated with the item, this is optional
        /// </summary>
        public string[] tags;
        /// <summary>
        /// The visibility settings for this item, in general its wise to start with <see cref="ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate"/> the user can change this in Steam once they are happy with the results.
        /// </summary>
        public ERemoteStoragePublishedFileVisibility visibility;

        /// <summary>
        /// To be valid the following must be true
        /// <list type="bullet">
        /// <item><see cref="appId"/> must be valid</item>
        /// <item><see cref="title"/> must be populated with a value whose length is less than <see cref="Constants.k_cchPublishedDocumentTitleMax"/></item>
        /// <item><see cref="description"/> must be populated with a value whose length is less than <see cref="Constants.k_cchPublishedDocumentDescriptionMax"/></item>
        /// <item><see cref="metadata"/> is option and can be an empty string, if populated its length must be less than <see cref="Constants.k_cchDeveloperMetadataMax"/></item>
        /// <item><see cref="content"/> must be the full path of a valid directory (aka folder path)</item>
        /// <item><see cref="preview"/> must be the full path of a valid JPG, PNG or GIF file whose total size is less than 1mb</item>
        /// <item><see cref="imageFiles"/> is optional and can be null, if populated each path must be a valid JPG, PNG or GIF and each image must have a size less than 1mb</item>
        /// <item><see cref="youTubeVideoIds"/> is optional and can be null, if populated each must be a valid YouTube video ID</item>
        /// <item><see cref="tags"/> is optional and can be null, if populated each tag must have a length less than 255</item>
        /// <item><see cref="keyValueTags"/> is optional and can be null, if populated the length of the key + the length of the value for each entry must be less than 255</item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// It is not required to call this.
        /// If you have have assured that the input is correct you can avoid calling this feature.
        /// This feature can be processor heavy and should only be called if you are unshure if the input values are valid.
        /// This does not catch every possible invalid case but catches the most common
        /// </remarks>
        public bool IsValid
        {
            get
            {
                return appId != AppId_t.Invalid
                    && !string.IsNullOrEmpty(title)
                    && title.Length < Constants.k_cchPublishedDocumentTitleMax
                    && !string.IsNullOrEmpty(description)
                    && description.Length < Constants.k_cchPublishedDocumentDescriptionMax
                    && (string.IsNullOrEmpty(metadata) || metadata.Length < Constants.k_cchDeveloperMetadataMax)
                    && preview != null
                    && content != null
                    && preview.Exists
                    && content.Exists
                    && (tags == null || tags.Length == 0 || !tags.Any(p => p.Length > 255));
            }
        }
        /// <summary>
        /// Request the item be created as a new item
        /// </summary>
        /// <param name="completedCallback">A delegate of the form (<see cref="WorkshopItemDataCreateStatus"/> results) that will be invoked when the process is completed rather that completion is a pass or fail</param>
        /// <param name="uploadStartedCallback">A delegate of the form (<see cref="UGCUpdateHandle_t"/> handle) that will be invoked when the process has completed its upload phase</param>
        /// <param name="fileCreatedCallback">A delegate of the form (<see cref="CreateItemResult_t"/> result) that will be invoked when the process has completed its created phase</param>
        /// <returns>True if the request was accepted</returns>
        public bool Create(Action<WorkshopItemDataCreateStatus> completedCallback = null, Action<UGCUpdateHandle_t> uploadStartedCallback = null, Action<CreateItemResult_t> fileCreatedCallback = null) => API.UserGeneratedContent.Client.CreateItem(this, null, null, null, completedCallback, uploadStartedCallback, fileCreatedCallback);
        /// <summary>
        /// Request the item be created as a new item
        /// </summary>
        /// <param name="additionalPreviews">A collection of <see cref="WorkshopItemPreviewFile"/> for additional preview images</param>
        /// <param name="additionalKeyValueTags">A collection of strings repressing the YouTube video IDs of additional videos you would like to associate with the item</param>
        /// <param name="additionalYouTubeIds">A collection of key value tags to be added to the item</param>
        /// <param name="completedCallback">A delegate of the form (<see cref="WorkshopItemDataCreateStatus"/> results) that will be invoked when the process is completed rather that completion is a pass or fail</param>
        /// <param name="uploadStartedCallback">A delegate of the form (<see cref="UGCUpdateHandle_t"/> handle) that will be invoked when the process has completed its upload phase</param>
        /// <param name="fileCreatedCallback">A delegate of the form (<see cref="CreateItemResult_t"/> result) that will be invoked when the process has completed its created phase</param>
        /// <returns>True if the request was accepted</returns>
        public bool Create(WorkshopItemPreviewFile[] additionalPreviews, string[] additionalYouTubeIds, WorkshopItemKeyValueTag[] additionalKeyValueTags, Action<WorkshopItemDataCreateStatus> completedCallback = null, Action<UGCUpdateHandle_t> uploadStartedCallback = null, Action<CreateItemResult_t> fileCreatedCallback = null) => API.UserGeneratedContent.Client.CreateItem(this, additionalPreviews, additionalYouTubeIds, additionalKeyValueTags, completedCallback, uploadStartedCallback, fileCreatedCallback);
        /// <summary>
        /// Request the item be update as a new item
        /// </summary>
        /// <param name="completedCallback">A delegate of the form (<see cref="WorkshopItemDataCreateStatus"/> results) that will be invoked when the process is completed rather that completion is a pass or fail</param>
        /// <param name="uploadStartedCallback">A delegate of the form (<see cref="UGCUpdateHandle_t"/> handle) that will be invoked when the process has completed its upload phase</param>
        /// <returns>True if the request was accepted</returns>
        public bool Update(Action<WorkshopItemDataUpdateStatus> completedCallback = null, Action<UGCUpdateHandle_t> uploadStartedCallback = null) => API.UserGeneratedContent.Client.UpdateItem(this, null, null, null, completedCallback, uploadStartedCallback);
        /// <summary>
        /// Request the item be updated as a new item
        /// </summary>
        /// <param name="additionalPreviews">A collection of <see cref="WorkshopItemPreviewFile"/> for additional preview images</param>
        /// <param name="additionalKeyValueTags">A collection of strings repressing the YouTube video IDs of additional videos you would like to associate with the item</param>
        /// <param name="additionalYouTubeIds">A collection of key value tags to be added to the item</param>
        /// <param name="completedCallback">A delegate of the form (<see cref="WorkshopItemDataCreateStatus"/> results) that will be invoked when the process is completed rather that completion is a pass or fail</param>
        /// <param name="uploadStartedCallback">A delegate of the form (<see cref="UGCUpdateHandle_t"/> handle) that will be invoked when the process has completed its upload phase</param>
        /// <returns>True if the request was accepted</returns>
        public bool Update(WorkshopItemPreviewFile[] additionalPreviews, string[] additionalYouTubeIds, WorkshopItemKeyValueTag[] additionalKeyValueTags, Action<WorkshopItemDataUpdateStatus> completedCallback = null, Action<UGCUpdateHandle_t> uploadStartedCallback = null) => API.UserGeneratedContent.Client.UpdateItem(this, additionalPreviews, additionalYouTubeIds, additionalKeyValueTags, completedCallback, uploadStartedCallback);
        /// <summary>
        /// Get a specific <see cref="WorkshopItem"/> based on a file ID
        /// </summary>
        /// <param name="file">The file to get</param>
        /// <param name="callback">A delegate of the form (<see cref="WorkshopItem"/> item) that is invoked on completion</param>
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
        /// Gets the subscribed items with the additional arguments requested
        /// </summary>
        /// <param name="withLongDescription">Should the long description be included</param>
        /// <param name="withMetadata">Should metadata be included</param>
        /// <param name="withKeyValueTags">Should key value tags be included</param>
        /// <param name="withAdditionalPreviews">Should additional preview be included</param>
        /// <param name="withPlayTimeStatsInDays">Should play time stats be included</param>
        /// <param name="callback">A delegate of the form  (List&lt;<see cref="WorkshopItem"/>&gt; items) that will be invoked when the results are found</param>
        public static void GetSubscribed(bool withLongDescription, bool withMetadata, bool withKeyValueTags, bool withAdditionalPreviews, uint withPlayTimeStatsInDays, Action<List<WorkshopItem>> callback) => API.UserGeneratedContent.Client.GetSubscribedItems(withLongDescription, withMetadata, withKeyValueTags, withAdditionalPreviews, withPlayTimeStatsInDays, callback);
    }
}
#endif