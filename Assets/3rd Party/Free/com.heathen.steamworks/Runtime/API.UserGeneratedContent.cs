#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Functions to create, consume, and interact with the Steam Workshop.
    /// </summary>
    public static class UserGeneratedContent
    {
        /// <summary>
        /// Checks if the 'checkFlag' value is in the 'value'
        /// </summary>
        /// <param name="value"></param>
        /// <param name="checkflag"></param>
        /// <returns></returns>
        public static bool ItemStateHasFlag(EItemState value, EItemState checkflag)
        {
            return (value & checkflag) == checkflag;
        }
        /// <summary>
        /// Checks if any of the 'checkflags' values are in the 'value'
        /// </summary>
        /// <param name="value"></param>
        /// <param name="checkflags"></param>
        /// <returns></returns>
        public static bool ItemStateHasAllFlags(EItemState value, params EItemState[] checkflags)
        {
            foreach (var checkflag in checkflags)
            {
                if ((value & checkflag) != checkflag)
                    return false;
            }
            return true;
        }

        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                evtItemDownloaded = new WorkshopDownloadedItemResultEvent();
                evtItemInstalled = new WorkshopItemInstalledEvent();

                m_AddAppDependencyResults = null;
                m_AddUGCDependencyResults = null;
                m_UserFavoriteItemsListChanged = null;
                m_CreatedItem = null;
                m_DeleteItem = null;
                m_AppDependenciesResult = null;
                m_GetUserItemVoteResult = null;
                m_RemoveAppDependencyResult = null;
                m_RemoveDependencyResult = null;
                m_SteamUGCRequestUGCDetailsResult = null;
                m_SteamUGCQueryCompleted = null;
                m_SetUserItemVoteResult = null;
                m_StartPlaytimeTrackingResult = null;
                m_StopPlaytimeTrackingResult = null;
                m_SubmitItemUpdateResult = null;
                m_RemoteStorageSubscribePublishedFileResult = null;
                m_RemoteStorageUnsubscribePublishedFileResult = null;
                m_WorkshopEULAStatus = null;
                m_DownloadItem = null;
                m_ItemInstalled = null;
            }

            private static WorkshopDownloadedItemResultEvent evtItemDownloaded = new WorkshopDownloadedItemResultEvent();
            private static WorkshopItemInstalledEvent evtItemInstalled = new WorkshopItemInstalledEvent();

            private static CallResult<AddAppDependencyResult_t> m_AddAppDependencyResults;
            private static CallResult<AddUGCDependencyResult_t> m_AddUGCDependencyResults;
            private static CallResult<UserFavoriteItemsListChanged_t> m_UserFavoriteItemsListChanged;
            private static CallResult<CreateItemResult_t> m_CreatedItem;
            private static CallResult<DeleteItemResult_t> m_DeleteItem;
            private static CallResult<GetAppDependenciesResult_t> m_AppDependenciesResult;
            private static CallResult<GetUserItemVoteResult_t> m_GetUserItemVoteResult;
            private static CallResult<RemoveAppDependencyResult_t> m_RemoveAppDependencyResult;
            private static CallResult<RemoveUGCDependencyResult_t> m_RemoveDependencyResult;
            private static CallResult<SteamUGCRequestUGCDetailsResult_t> m_SteamUGCRequestUGCDetailsResult;
            private static CallResult<SteamUGCQueryCompleted_t> m_SteamUGCQueryCompleted;
            private static CallResult<SetUserItemVoteResult_t> m_SetUserItemVoteResult;
            private static CallResult<StartPlaytimeTrackingResult_t> m_StartPlaytimeTrackingResult;
            private static CallResult<StopPlaytimeTrackingResult_t> m_StopPlaytimeTrackingResult;
            private static CallResult<SubmitItemUpdateResult_t> m_SubmitItemUpdateResult;
            private static CallResult<RemoteStorageSubscribePublishedFileResult_t> m_RemoteStorageSubscribePublishedFileResult;
            private static CallResult<RemoteStorageUnsubscribePublishedFileResult_t> m_RemoteStorageUnsubscribePublishedFileResult;
            private static CallResult<WorkshopEULAStatus_t> m_WorkshopEULAStatus;

            private static Callback<DownloadItemResult_t> m_DownloadItem;
            private static Callback<ItemInstalled_t> m_ItemInstalled;



#region Events
            /// <summary>
            /// Occurs when a UGC item is downloaded
            /// </summary>
            public static WorkshopDownloadedItemResultEvent EventItemDownloaded
            {
                get
                {
                    if (m_DownloadItem == null)
                        m_DownloadItem = Callback<DownloadItemResult_t>.Create(evtItemDownloaded.Invoke);

                    return evtItemDownloaded;
                }
            }

            /// <summary>
            /// Called when a workshop item has been installed or updated.
            /// </summary>
            public static WorkshopItemInstalledEvent EventWorkshopItemInstalled
            {
                get
                {
                    if (m_ItemInstalled == null)
                        m_ItemInstalled = Callback<ItemInstalled_t>.Create(evtItemInstalled.Invoke);

                    return evtItemInstalled;
                }
            }


            
#endregion

#region Workshop System
            public static bool CreateItem(WorkshopItemData item, WorkshopItemPreviewFile[] additionalPreviews, string[] additionalYouTubeIds, WorkshopItemKeyValueTag[] additionalKeyValueTags, Action<WorkshopItemDataCreateStatus> completedCallback = null, Action<UGCUpdateHandle_t> uploadStartedCallback = null, Action<CreateItemResult_t> fileCreatedCallback = null)
            {
                if (m_CreatedItem == null)
                    m_CreatedItem = CallResult<CreateItemResult_t>.Create();

                if (m_SubmitItemUpdateResult == null)
                    m_SubmitItemUpdateResult = CallResult<SubmitItemUpdateResult_t>.Create();

                var call = SteamUGC.CreateItem(item.appId, EWorkshopFileType.k_EWorkshopFileTypeCommunity);
                m_CreatedItem.Set(call, (createResult, createIOError) =>
                {
                    if (createIOError || createResult.m_eResult != EResult.k_EResultOK)
                    { 
                        if (createIOError)
                        {
                            completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                            {
                                hasError = true,
                                errorMessage = "Steamworks Client failed to create UGC item.",
                                createItemResult = createResult,
                            });
                        }
                        else
                        {
                            switch(createResult.m_eResult)
                            {
                                case EResult.k_EResultInsufficientPrivilege:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        hasError = true,
                                        errorMessage = "The user is currently restricted from uploading content due to a hub ban, account lock, or community ban. They would need to contact Steam Support.",
                                        createItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultBanned:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        hasError = true,
                                        errorMessage = "The user doesn't have permission to upload content to this hub because they have an active VAC or Game ban.",
                                        createItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultTimeout:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        hasError = true,
                                        errorMessage = "The operation took longer than expected. Have the user retry the creation process.",
                                        createItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultNotLoggedOn:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        hasError = true,
                                        errorMessage = "The user is not currently logged into Steam.",
                                        createItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultServiceUnavailable:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        hasError = true,
                                        errorMessage = "The workshop server hosting the content is having issues - have the user retry.",
                                        createItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultInvalidParam:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        hasError = true,
                                        errorMessage = "One of the submission fields contains something not being accepted by that field.",
                                        createItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultAccessDenied:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        hasError = true,
                                        errorMessage = "There was a problem trying to save the title and description. Access was denied.",
                                        createItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultLimitExceeded:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        hasError = true,
                                        errorMessage = "The user has exceeded their Steam Cloud quota. Have them remove some items and try again.",
                                        createItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultFileNotFound:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        hasError = true,
                                        errorMessage = "The uploaded file could not be found.",
                                        createItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultDuplicateRequest:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        hasError = true,
                                        errorMessage = "The file was already successfully uploaded. The user just needs to refresh.",
                                        createItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultDuplicateName:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        hasError = true,
                                        errorMessage = "The user already has a Steam Workshop item with that name.",
                                        createItemResult = createResult,
                                    });
                                    break;
                                case EResult.k_EResultServiceReadOnly:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        hasError = true,
                                        errorMessage = "Due to a recent password or email change, the user is not allowed to upload new content. Usually this restriction will expire in 5 days, but can last up to 30 days if the account has been inactive recently.",
                                        createItemResult = createResult,
                                    });
                                    break;
                                default:
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        hasError = true,
                                        errorMessage = "Unexpected result please see the createItemResult.m_eResult status for more information.",
                                        createItemResult = createResult,
                                    });
                                    break;
                            }
                        }
                    }
                    else
                    {
                        fileCreatedCallback?.Invoke(createResult);
                        var updateHandle = SteamUGC.StartItemUpdate(item.appId, createResult.m_nPublishedFileId);
                        var hasError = false;
                        var sb = new System.Text.StringBuilder();

                        if (!string.IsNullOrEmpty(item.title))
                        {
                            if (!SteamUGC.SetItemTitle(updateHandle, item.title))
                            {
                                hasError = true;
                                if (sb.Length > 0)
                                    sb.Append("\n");
                                sb.Append("Failed to update item title.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("The title was not provided and is required; the update might be rejected by Valve");
                        }

                        if (!string.IsNullOrEmpty(item.description))
                        {
                            if (!SteamUGC.SetItemDescription(updateHandle, item.description))
                            {
                                hasError = true;
                                if (sb.Length > 0)
                                    sb.Append("\n");
                                sb.Append("Failed to update item description.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("The description was not provided and is required; the update might be rejected by Valve");
                        }

                        if (!SteamUGC.SetItemVisibility(updateHandle, item.visibility))
                        {
                            hasError = true;
                            if (sb.Length > 0)
                                sb.Append("\n");
                            sb.Append("Failed to update item visibility.");
                        }

                        if (item.tags != null && item.tags.Count() > 0)
                        {
                            if (!SteamUGC.SetItemTags(updateHandle, item.tags.ToList()))
                            {
                                hasError = true;
                                if (sb.Length > 0)
                                    sb.Append("\n");
                                sb.Append("Failed to update item tags.");
                            }
                        }

                        if (item.content != null && item.content.Exists)
                        {
                            if (!SteamUGC.SetItemContent(updateHandle, item.content.FullName))
                            {
                                hasError = true;
                                if (sb.Length > 0)
                                    sb.Append("\n");
                                sb.Append("Failed to update item content location.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("The content folder does not exist and is required; the update might be rejected by Valve");
                        }

                        if (item.preview != null && item.preview.Exists)
                        {
                            if (!SteamUGC.SetItemPreview(updateHandle, item.preview.FullName))
                            {
                                hasError = true;
                                if (sb.Length > 0)
                                    sb.Append("\n");
                                sb.Append("Failed to update item preview.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("The preview image does not exist and is required; the update might be rejected by Valve");
                        }

                        if (additionalPreviews != null && additionalPreviews.Length > 0)
                        {
                            foreach (var previewFile in additionalPreviews)
                            {
                                if (!SteamUGC.AddItemPreviewFile(updateHandle, previewFile.source, previewFile.type))
                                {
                                    hasError = true;
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("Failed to add item preview: " + previewFile.source + ".");
                                }
                            }
                        }

                        if (additionalYouTubeIds != null && additionalYouTubeIds.Length > 0)
                        {
                            foreach (var video in additionalYouTubeIds)
                            {
                                if (!SteamUGC.AddItemPreviewVideo(updateHandle, video))
                                {
                                    hasError = true;
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("Failed to add item video: " + video + ".");
                                }
                            }
                        }

                        if (additionalKeyValueTags != null && additionalKeyValueTags.Length > 0)
                        {
                            foreach (var tag in additionalKeyValueTags)
                            {
                                if (!SteamUGC.AddItemKeyValueTag(updateHandle, tag.key, tag.value))
                                {
                                    hasError = true;
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("Failed to add item key value tag: " + tag.key + ":" + tag.value);
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(item.metadata))
                        {
                            if (!SteamUGC.SetItemMetadata(updateHandle, item.metadata))
                            {
                                hasError = true;
                                if (sb.Length > 0)
                                    sb.Append("\n");
                                sb.Append("Failed to update item metadata.");
                            }
                        }

                        var siu = SteamUGC.SubmitItemUpdate(updateHandle, "Initial Creation");
                        m_SubmitItemUpdateResult.Set(siu, (updateResult, updateIOError) =>
                        {
                            if (updateIOError || updateResult.m_eResult != EResult.k_EResultOK)
                            {
                                hasError = true;

                                if (updateIOError)
                                {
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("Steamworks Client failed to submit item updates.");

                                    item.publishedFileId = createResult.m_nPublishedFileId;
                                    completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                    {
                                        hasError = true,
                                        errorMessage = sb.ToString(),
                                        data = item,
                                        createItemResult = createResult,
                                        submitItemUpdateResult = updateResult,
                                    });
                                }
                                else
                                {
                                    switch(updateResult.m_eResult)
                                    {
                                        case EResult.k_EResultFail:
                                            if (sb.Length > 0)
                                                sb.Append("\n");
                                            sb.Append("Generic failure.");

                                            item.publishedFileId = createResult.m_nPublishedFileId;
                                            completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                            {
                                                hasError = true,
                                                errorMessage = sb.ToString(),
                                                data = item,
                                                createItemResult = createResult,
                                                submitItemUpdateResult = updateResult,
                                            });
                                            break;
                                        case EResult.k_EResultInvalidParam:
                                            if (sb.Length > 0)
                                                sb.Append("\n");
                                            sb.Append("Either the provided app ID is invalid or doesn't match the consumer app ID of the item or, you have not enabled ISteamUGC for the provided app ID on the Steam Workshop Configuration App Admin page.\nThe preview file is smaller than 16 bytes.");

                                            item.publishedFileId = createResult.m_nPublishedFileId;
                                            completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                            {
                                                hasError = true,
                                                errorMessage = sb.ToString(),
                                                data = item,
                                                createItemResult = createResult,
                                                submitItemUpdateResult = updateResult,
                                            });
                                            break;
                                        case EResult.k_EResultAccessDenied:
                                            if (sb.Length > 0)
                                                sb.Append("\n");
                                            sb.Append("The user doesn't own a license for the provided app ID.");

                                            item.publishedFileId = createResult.m_nPublishedFileId;
                                            completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                            {
                                                hasError = true,
                                                errorMessage = sb.ToString(),
                                                data = item,
                                                createItemResult = createResult,
                                                submitItemUpdateResult = updateResult,
                                            });
                                            break;
                                        case EResult.k_EResultFileNotFound:
                                            if (sb.Length > 0)
                                                sb.Append("\n");
                                            sb.Append("Failed to get the workshop info for the item or failed to read the preview file or the content folder is not valid.");

                                            item.publishedFileId = createResult.m_nPublishedFileId;
                                            completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                            {
                                                hasError = true,
                                                errorMessage = sb.ToString(),
                                                data = item,
                                                createItemResult = createResult,
                                                submitItemUpdateResult = updateResult,
                                            });
                                            break;
                                        case EResult.k_EResultLockingFailed:
                                            if (sb.Length > 0)
                                                sb.Append("\n");
                                            sb.Append("Failed to acquire UGC Lock.");

                                            item.publishedFileId = createResult.m_nPublishedFileId;
                                            completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                            {
                                                hasError = true,
                                                errorMessage = sb.ToString(),
                                                data = item,
                                                createItemResult = createResult,
                                                submitItemUpdateResult = updateResult,
                                            });
                                            break;
                                        case EResult.k_EResultLimitExceeded:

                                            if (sb.Length > 0)
                                                sb.Append("\n");
                                            sb.Append("The preview image is too large, it must be less than 1 Megabyte; or there is not enough space available on the users Steam Cloud.");

                                            item.publishedFileId = createResult.m_nPublishedFileId;
                                            completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                            {
                                                hasError = true,
                                                errorMessage = sb.ToString(),
                                                data = item,
                                                createItemResult = createResult,
                                                submitItemUpdateResult = updateResult,
                                            });
                                            break;
                                        default:
                                            if (sb.Length > 0)
                                                sb.Append("\n");
                                            sb.Append("Unexpected status message from Steam client, please see the submitItemUpdateResult.m_eResult status for more information.");

                                            item.publishedFileId = createResult.m_nPublishedFileId;
                                            completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                            {
                                                hasError = true,
                                                errorMessage = sb.ToString(),
                                                data = item,
                                                createItemResult = createResult,
                                                submitItemUpdateResult = updateResult,
                                            });
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                item.publishedFileId = createResult.m_nPublishedFileId;
                                completedCallback?.Invoke(new WorkshopItemDataCreateStatus
                                {
                                    hasError = hasError,
                                    errorMessage = hasError ? sb.ToString() : string.Empty,
                                    data = item,
                                    createItemResult = createResult,
                                    submitItemUpdateResult = updateResult,
                                });
                            }
                        });
                        uploadStartedCallback?.Invoke(updateHandle);
                    }
                });

                return true;
            }

            public static bool UpdateItem(WorkshopItemData item, WorkshopItemPreviewFile[] additionalPreviews, string[] additionalYouTubeIds, WorkshopItemKeyValueTag[] additionalKeyValueTags, Action<WorkshopItemDataUpdateStatus> callback = null, Action<UGCUpdateHandle_t> uploadStartedCallback = null)
            {
                if (m_CreatedItem == null)
                    m_CreatedItem = CallResult<CreateItemResult_t>.Create();

                if (m_SubmitItemUpdateResult == null)
                    m_SubmitItemUpdateResult = CallResult<SubmitItemUpdateResult_t>.Create();

                if (!item.publishedFileId.HasValue)
                    return false;

                var updateHandle = SteamUGC.StartItemUpdate(item.appId, item.publishedFileId.Value);
                var hasError = false;
                var sb = new System.Text.StringBuilder();
                if (!SteamUGC.SetItemTitle(updateHandle, item.title))
                {
                    hasError = true;
                    if (sb.Length > 0)
                        sb.Append("\n");
                    sb.Append("Failed to update item title.");
                }

                if (!string.IsNullOrEmpty(item.description))
                {
                    if (!SteamUGC.SetItemDescription(updateHandle, item.description))
                    {
                        hasError = true;
                        if (sb.Length > 0)
                            sb.Append("\n");
                        sb.Append("Failed to update item description.");
                    }
                }

                if (!SteamUGC.SetItemVisibility(updateHandle, item.visibility))
                {
                    hasError = true;
                    if (sb.Length > 0)
                        sb.Append("\n");
                    sb.Append("Failed to update item visibility.");
                }

                if (item.tags != null && item.tags.Count() > 0)
                {
                    if (!SteamUGC.SetItemTags(updateHandle, item.tags.ToList()))
                    {
                        hasError = true;
                        if (sb.Length > 0)
                            sb.Append("\n");
                        sb.Append("Failed to update item tags.");
                    }
                }

                if (!SteamUGC.SetItemContent(updateHandle, item.content.FullName))
                {
                    hasError = true;
                    if (sb.Length > 0)
                        sb.Append("\n");
                    sb.Append("Failed to update item content location.");
                }

                if (!SteamUGC.SetItemPreview(updateHandle, item.preview.FullName))
                {
                    hasError = true;
                    if (sb.Length > 0)
                        sb.Append("\n");
                    sb.Append("Failed to update item preview.");
                }

                if (additionalPreviews != null && additionalPreviews.Length > 0)
                {
                    foreach (var previewFile in additionalPreviews)
                    {
                        if (!SteamUGC.AddItemPreviewFile(updateHandle, previewFile.source, previewFile.type))
                        {
                            hasError = true;
                            if (sb.Length > 0)
                                sb.Append("\n");
                            sb.Append("Failed to add item preview: " + previewFile.source + ".");
                        }
                    }
                }

                if (additionalYouTubeIds != null && additionalYouTubeIds.Length > 0)
                {
                    foreach (var video in additionalYouTubeIds)
                    {
                        if (!SteamUGC.AddItemPreviewVideo(updateHandle, video))
                        {
                            hasError = true;
                            if (sb.Length > 0)
                                sb.Append("\n");
                            sb.Append("Failed to add item video: " + video + ".");
                        }
                    }
                }

                if (additionalKeyValueTags != null && additionalKeyValueTags.Length > 0)
                {
                    foreach (var tag in additionalKeyValueTags)
                    {
                        if (!SteamUGC.AddItemKeyValueTag(updateHandle, tag.key, tag.value))
                        {
                            hasError = true;
                            if (sb.Length > 0)
                                sb.Append("\n");
                            sb.Append("Failed to add item key value tag: " + tag.key + ":" + tag.value);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(item.metadata))
                {
                    if (!SteamUGC.SetItemMetadata(updateHandle, item.metadata))
                    {
                        hasError = true;
                        if (sb.Length > 0)
                            sb.Append("\n");
                        sb.Append("Failed to update item metadata.");
                    }
                }

                var siu = SteamUGC.SubmitItemUpdate(updateHandle, "Initial Creation");
                m_SubmitItemUpdateResult.Set(siu, (updateResult, updateIOError) =>
                {
                    if (updateIOError || updateResult.m_eResult != EResult.k_EResultOK)
                    {
                        hasError = true;

                        if (updateIOError)
                        {
                            if (sb.Length > 0)
                                sb.Append("\n");
                            sb.Append("Steamworks Client failed to submit item updates.");

                            callback?.Invoke(new WorkshopItemDataUpdateStatus
                            {
                                hasError = true,
                                errorMessage = sb.ToString(),
                                data = item,
                                submitItemUpdateResult = updateResult,
                            });
                        }
                        else
                        {
                            switch (updateResult.m_eResult)
                            {
                                case EResult.k_EResultFail:
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("Generic failure.");

                                    callback?.Invoke(new WorkshopItemDataUpdateStatus
                                    {
                                        hasError = true,
                                        errorMessage = sb.ToString(),
                                        data = item,
                                        submitItemUpdateResult = updateResult,
                                    });
                                    break;
                                case EResult.k_EResultInvalidParam:
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("Either the provided app ID is invalid or doesn't match the consumer app ID of the item or, you have not enabled ISteamUGC for the provided app ID on the Steam Workshop Configuration App Admin page.\nThe preview file is smaller than 16 bytes.");

                                    callback?.Invoke(new WorkshopItemDataUpdateStatus
                                    {
                                        hasError = true,
                                        errorMessage = sb.ToString(),
                                        data = item,
                                        submitItemUpdateResult = updateResult,
                                    });
                                    break;
                                case EResult.k_EResultAccessDenied:
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("The user doesn't own a license for the provided app ID.");

                                    callback?.Invoke(new WorkshopItemDataUpdateStatus
                                    {
                                        hasError = true,
                                        errorMessage = sb.ToString(),
                                        data = item,
                                        submitItemUpdateResult = updateResult,
                                    });
                                    break;
                                case EResult.k_EResultFileNotFound:
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("Failed to get the workshop info for the item or failed to read the preview file or the content folder is not valid.");

                                    callback?.Invoke(new WorkshopItemDataUpdateStatus
                                    {
                                        hasError = true,
                                        errorMessage = sb.ToString(),
                                        data = item,
                                        submitItemUpdateResult = updateResult,
                                    });
                                    break;
                                case EResult.k_EResultLockingFailed:
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("Failed to acquire UGC Lock.");

                                    callback?.Invoke(new WorkshopItemDataUpdateStatus
                                    {
                                        hasError = true,
                                        errorMessage = sb.ToString(),
                                        data = item,
                                        submitItemUpdateResult = updateResult,
                                    });
                                    break;
                                case EResult.k_EResultLimitExceeded:

                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("The preview image is too large, it must be less than 1 Megabyte; or there is not enough space available on the users Steam Cloud.");

                                    callback?.Invoke(new WorkshopItemDataUpdateStatus
                                    {
                                        hasError = true,
                                        errorMessage = sb.ToString(),
                                        data = item,
                                        submitItemUpdateResult = updateResult,
                                    });
                                    break;
                                default:
                                    if (sb.Length > 0)
                                        sb.Append("\n");
                                    sb.Append("Unexpected status message from Steam client, please see the submitItemUpdateResult.m_eResult status for more information.");

                                    callback?.Invoke(new WorkshopItemDataUpdateStatus
                                    {
                                        hasError = true,
                                        errorMessage = sb.ToString(),
                                        data = item,
                                        submitItemUpdateResult = updateResult,
                                    });
                                    break;
                            }
                        }
                    }
                    else
                    {
                        callback?.Invoke(new WorkshopItemDataUpdateStatus
                        {
                            hasError = hasError,
                            errorMessage = hasError ? sb.ToString() : string.Empty,
                            data = item,
                            submitItemUpdateResult = updateResult,
                        });
                    }
                });
                uploadStartedCallback?.Invoke(updateHandle);
                return true;
            }

            /// <summary>
            /// Adds a dependency between the given item and the appid. This list of dependencies can be retrieved by calling GetAppDependencies. This is a soft-dependency that is displayed on the web. It is up to the application to determine whether the item can actually be used or not.
            /// </summary>
            /// <param name="fileId"></param>
            /// <param name="appId"></param>
            public static void AddAppDependency(PublishedFileId_t fileId, AppId_t appId, Action<AddAppDependencyResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_AddAppDependencyResults == null)
                    m_AddAppDependencyResults = CallResult<AddAppDependencyResult_t>.Create();

                var call = SteamUGC.AddAppDependency(fileId, appId);
                m_AddAppDependencyResults.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Adds a workshop item as a dependency to the specified item. If the nParentPublishedFileID item is of type k_EWorkshopFileTypeCollection, than the nChildPublishedFileID is simply added to that collection. Otherwise, the dependency is a soft one that is displayed on the web and can be retrieved via the ISteamUGC API using a combination of the m_unNumChildren member variable of the SteamUGCDetails_t struct and GetQueryUGCChildren.
            /// </summary>
            /// <param name="parentFileId"></param>
            /// <param name="childFileId"></param>
            public static void AddDependency(PublishedFileId_t parentFileId, PublishedFileId_t childFileId, Action<AddUGCDependencyResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_AddUGCDependencyResults == null)
                    m_AddUGCDependencyResults = CallResult<AddUGCDependencyResult_t>.Create();

                var call = SteamUGC.AddDependency(parentFileId, childFileId);
                m_AddUGCDependencyResults.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Adds a excluded tag to a pending UGC Query. This will only return UGC without the specified tag.
            /// </summary>
            /// <param name="handle">The UGC query handle to customize.</param>
            /// <param name="tagName">The tag that must NOT be attached to the UGC to receive it.</param>
            /// <returns>true upon success. false if the UGC query handle is invalid, if the UGC query handle is from CreateQueryUGCDetailsRequest, or tagName was NULL.</returns>
            /// <remarks>This must be set before you send a UGC Query handle using SendQueryUGCRequest.</remarks>
            public static bool AddExcludedTag(UGCQueryHandle_t handle, string tagName) => SteamUGC.AddExcludedTag(handle, tagName);

            /// <summary>
            /// Adds a key-value tag pair to an item. Keys can map to multiple different values (1-to-many relationship).
            /// Key names are restricted to alpha-numeric characters and the '_' character.
            /// Both keys and values cannot exceed 255 characters in length.
            /// Key-value tags are searchable by exact match only.
            /// </summary>
            /// <param name="handle">The workshop item update handle to customize.</param>
            /// <param name="key">The key to set on the item.</param>
            /// <param name="value">A value to map to the key.</param>
            /// <returns></returns>
            public static bool AddItemKeyValueTag(UGCUpdateHandle_t handle, string key, string value) => SteamUGC.AddItemKeyValueTag(handle, key, value);

            /// <summary>
            /// Adds an additional preview file for the item.
            /// Then the format of the image should be one that both the web and the application(if necessary) can render, and must be under 1MB.Suggested formats include JPG, PNG and GIF.
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="previewFile"></param>
            /// <param name="type"></param>
            /// <returns></returns>
            public static bool AddItemPreviewFile(UGCUpdateHandle_t handle, string previewFile, EItemPreviewType type) => SteamUGC.AddItemPreviewFile(handle, previewFile, type);

            /// <summary>
            /// Adds an additional video preview from YouTube for the item.
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="videoId">The YouTube video ID ... e.g jHgZh4GV9G0</param>
            /// <returns></returns>
            public static bool AddItemPreviewVideo(UGCUpdateHandle_t handle, string videoId) => SteamUGC.AddItemPreviewVideo(handle, videoId);

            /// <summary>
            /// Adds workshop item to the users favorite list
            /// </summary>
            /// <param name="appId"></param>
            /// <param name="fileId"></param>
            public static void AddItemToFavorites(AppId_t appId, PublishedFileId_t fileId, Action<UserFavoriteItemsListChanged_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_UserFavoriteItemsListChanged == null)
                    m_UserFavoriteItemsListChanged = CallResult<UserFavoriteItemsListChanged_t>.Create();

                var call = SteamUGC.AddItemToFavorites(appId, fileId);
                m_UserFavoriteItemsListChanged.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Adds a required key-value tag to a pending UGC Query. This will only return workshop items that have a key = pKey and a value = pValue.
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public static bool AddRequiredKeyValueTag(UGCQueryHandle_t handle, string key, string value) => SteamUGC.AddRequiredKeyValueTag(handle, key, value);

            /// <summary>
            /// Adds a required tag to a pending UGC Query. This will only return UGC with the specified tag.
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="tagName"></param>
            /// <returns></returns>
            public static bool AddRequiredTag(UGCQueryHandle_t handle, string tagName) => SteamUGC.AddRequiredTag(handle, tagName);

            /// <summary>
            /// Creates an empty workshop Item
            /// </summary>
            /// <param name="appId"></param>
            /// <param name="type"></param>
            public static void CreateItem(AppId_t appId, EWorkshopFileType type, Action<CreateItemResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_CreatedItem == null)
                    m_CreatedItem = CallResult<CreateItemResult_t>.Create();

                var call = SteamUGC.CreateItem(appId, type);
                m_CreatedItem.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Query for all matching UGC. You can use this to list all of the available UGC for your app.
            /// You must release the handle returned by this function by calling WorkshopReleaseQueryRequest when you are done with it!
            /// </summary>
            /// <param name="queryType"></param>
            /// <param name="matchingFileType"></param>
            /// <param name="creatorAppId"></param>
            /// <param name="consumerAppId"></param>
            /// <param name="page"></param>
            /// <returns></returns>
            public static UGCQueryHandle_t CreateQueryAllRequest(EUGCQuery queryType, EUGCMatchingUGCType matchingFileType, AppId_t creatorAppId, AppId_t consumerAppId, uint page) => SteamUGC.CreateQueryAllUGCRequest(queryType, matchingFileType, creatorAppId, consumerAppId, page);

            /// <summary>
            /// Query for the details of specific workshop items
            /// You must release the handle returned by this function by calling WorkshopReleaseQueryRequest when you are done with it!
            /// </summary>
            /// <param name="fileIds">The list of workshop items to get the details for.</param>
            /// <param name="count">The number of items in the list</param>
            /// <returns></returns>
            public static UGCQueryHandle_t CreateQueryDetailsRequest(PublishedFileId_t[] fileIds) => SteamUGC.CreateQueryUGCDetailsRequest(fileIds, (uint)fileIds.GetLength(0));

            /// <summary>
            /// Query for the details of specific workshop items
            /// You must release the handle returned by this function by calling WorkshopReleaseQueryRequest when you are done with it!
            /// </summary>
            /// <param name="fileIds"></param>
            /// <returns></returns>
            public static UGCQueryHandle_t CreateQueryDetailsRequest(List<PublishedFileId_t> fileIds) => SteamUGC.CreateQueryUGCDetailsRequest(fileIds.ToArray(), (uint)fileIds.Count);

            /// <summary>
            /// Query for the details of specific workshop items
            /// You must release the handle returned by this function by calling WorkshopReleaseQueryRequest when you are done with it!
            /// </summary>
            /// <param name="fileIds"></param>
            /// <returns></returns>
            public static UGCQueryHandle_t CreateQueryDetailsRequest(IEnumerable<PublishedFileId_t> fileIds) => SteamUGC.CreateQueryUGCDetailsRequest(fileIds.ToArray(), (uint)fileIds.Count());

            /// <summary>
            /// Query UGC associated with a user. You can use this to list the UGC the user is subscribed to amongst other things.
            /// You must release the handle returned by this function by calling WorkshopReleaseQueryRequest when you are done with it!
            /// </summary>
            /// <param name="accountId">The Account ID to query the UGC for. You can use CSteamID.GetAccountID to get the Account ID from a Steamworks ID.</param>
            /// <param name="listType">Used to specify the type of list to get.</param>
            /// <param name="matchingType">Used to specify the type of UGC queried for.</param>
            /// <param name="sortOrder">Used to specify the order that the list will be sorted in.</param>
            /// <param name="creatorAppId">This should contain the App ID of the app where the item was created. This may be different than nConsumerAppID if your item creation tool is a separate App ID.</param>
            /// <param name="consumerAppId">This should contain the App ID for the current game or application. Do not pass the App ID of the workshop item creation tool if that is a separate App ID!</param>
            /// <param name="page">The page number of the results to receive. This should start at 1 on the first call.</param>
            /// <returns></returns>
            public static UGCQueryHandle_t CreateQueryUserRequest(AccountID_t accountId, EUserUGCList listType, EUGCMatchingUGCType matchingType, EUserUGCListSortOrder sortOrder, AppId_t creatorAppId, AppId_t consumerAppId, uint page) => SteamUGC.CreateQueryUserUGCRequest(accountId, listType, matchingType, sortOrder, creatorAppId, consumerAppId, page);

            /// <summary>
            /// Frees a UGC query
            /// </summary>
            /// <param name="handle"></param>
            /// <returns></returns>
            public static bool ReleaseQueryRequest(UGCQueryHandle_t handle) => SteamUGC.ReleaseQueryUGCRequest(handle);

            /// <summary>
            /// Requests delete of a UGC item
            /// </summary>
            /// <param name="fileId"></param>
            public static void DeleteItem(PublishedFileId_t fileId, Action<DeleteItemResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_DeleteItem == null)
                    m_DeleteItem = CallResult<DeleteItemResult_t>.Create();

                var call = SteamUGC.DeleteItem(fileId);
                m_DeleteItem.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Request download of a UGC item
            /// </summary>
            /// <param name="fileId"></param>
            /// <param name="setHighPriority"></param>
            /// <returns></returns>
            public static bool DownloadItem(PublishedFileId_t fileId, bool setHighPriority) => SteamUGC.DownloadItem(fileId, setHighPriority);

            /// <summary>
            /// Request the app dependencies of a UGC item
            /// <para><see cref="https://partner.steamgames.com/doc/api/ISteamUGC#GetAppDependencies">https://partner.steamgames.com/doc/api/ISteamUGC#GetAppDependencies</see></para>
            /// </summary>
            /// <param name="fileId">The workshop item to get app dependencies for.</param>
            public static void GetAppDependencies(PublishedFileId_t fileId, Action<GetAppDependenciesResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_AppDependenciesResult == null)
                    m_AppDependenciesResult = CallResult<GetAppDependenciesResult_t>.Create();

                var call = SteamUGC.GetAppDependencies(fileId);
                m_AppDependenciesResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Request the download information of a UGC item
            /// <para><see cref="https://partner.steamgames.com/doc/api/ISteamUGC#GetItemDownloadInfo">https://partner.steamgames.com/doc/api/ISteamUGC#GetItemDownloadInfo</see></para>
            /// </summary>
            /// <param name="fileId">The workshop item to get the download info for.</param>
            /// <param name="completion">The % complete e.g. 0.5 represents 50% complete</param>
            /// <returns>true if the download information was available; otherwise, false.</returns>
            public static bool GetItemDownloadInfo(PublishedFileId_t fileId, out float completion)
            {
                var result = SteamUGC.GetItemDownloadInfo(fileId, out var current, out var total);
                if (result)
                    completion = total > 0 ? Convert.ToSingle(Convert.ToDouble(current) / Convert.ToDouble(total)) : 0;
                else
                    completion = 0;
                return result;
            }

            /// <summary>
            /// Request the installation information of a UGC item
            /// <para><see cref="https://partner.steamgames.com/doc/api/ISteamUGC#GetItemInstallInfo">https://partner.steamgames.com/doc/api/ISteamUGC#GetItemInstallInfo</see></para>
            /// </summary>
            /// <param name="fileId">The item to check</param>
            /// <param name="sizeOnDisk">The size of the item on the disk</param>
            /// <param name="folderPath">The path of the item on the disk</param>
            /// <param name="timeStamp">The date time stamp of the item</param>
            /// <returns>true if the workshop item is already installed.
            /// false in the following cases:
            /// The workshop item has no content.
            /// The workshop item is not installed.</returns>
            public static bool GetItemInstallInfo(PublishedFileId_t fileId, out ulong sizeOnDisk, out string folderPath, out DateTime timeStamp)
            {


                uint iTimeStamp;
                var result = SteamUGC.GetItemInstallInfo(fileId, out sizeOnDisk, out folderPath, 1024, out iTimeStamp);
                timeStamp = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                timeStamp = timeStamp.AddSeconds(iTimeStamp);
                return result;
            }

            /// <summary>
            /// Request the installation information of a UGC item
            /// <para><see cref="https://partner.steamgames.com/doc/api/ISteamUGC#GetItemInstallInfo">https://partner.steamgames.com/doc/api/ISteamUGC#GetItemInstallInfo</see></para>
            /// </summary>
            /// <param name="fileId">The item to check</param>
            /// <param name="sizeOnDisk">The size of the item on the disk</param>
            /// <param name="folderPath">The path of the item on the disk</param>
            /// <param name="folderSize">The size of folder path ... this is the length of the path e.g. 1024 would cover a max length path</param>
            /// <param name="timeStamp">The date time stamp of the item</param>
            /// <returns>true if the workshop item is already installed.
            /// false in the following cases:
            /// folderSize is 0
            /// The workshop item has no content.
            /// The workshop item is not installed.</returns>
            public static bool GetItemInstallInfo(PublishedFileId_t fileId, out ulong sizeOnDisk, out string folderPath, uint folderSize, out DateTime timeStamp)
            {


                uint iTimeStamp;
                var result = SteamUGC.GetItemInstallInfo(fileId, out sizeOnDisk, out folderPath, folderSize, out iTimeStamp);
                timeStamp = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                timeStamp = timeStamp.AddSeconds(iTimeStamp);
                return result;
            }

            /// <summary>
            /// Gets the current state of a workshop item on this client.
            /// <para><see cref="https://partner.steamgames.com/doc/api/ISteamUGC#GetItemState">https://partner.steamgames.com/doc/api/ISteamUGC#GetItemState</see></para>
            /// </summary>
            /// <param name="fileId">The workshop item to get the state for.</param>
            /// <returns>Item State flags, use with WorkshopItemStateHasFlag and WorkshopItemStateHasAllFlags</returns>
            public static EItemState GetItemState(PublishedFileId_t fileId)
            {
                return (EItemState)SteamUGC.GetItemState(fileId);
            }

            /// <summary>
            /// Checks if the 'checkFlag' value is in the 'value'
            /// </summary>
            /// <param name="value">The value to check if a state is contained within</param>
            /// <param name="checkflag">The state to see if it is contained within value</param>
            /// <returns>true if checkflag is contained within value</returns>
            public static bool ItemStateHasFlag(EItemState value, EItemState checkflag)
            {
                return (value & checkflag) == checkflag;
            }

            /// <summary>
            /// Checks if any of the 'checkflags' values are in the 'value'
            /// </summary>
            /// <param name="value">The value to check if a state is contained within</param>
            /// <param name="checkflag">The state to see if it is contained within value</param>
            /// <returns>true if checkflag is contained within value</returns>
            public static bool ItemStateHasAllFlags(EItemState value, params EItemState[] checkflags)
            {
                foreach (var checkflag in checkflags)
                {
                    if ((value & checkflag) != checkflag)
                        return false;
                }
                return true;
            }

            /// <summary>
            /// Gets the progress of an item update.
            /// <para><see cref="https://partner.steamgames.com/doc/api/ISteamUGC#GetItemUpdateProgress"/></para>
            /// </summary>
            /// <param name="handle">The update handle to get the progress for.</param>
            /// <param name="completion">The % completion e.g. 0.5 represents 50% complete</param>
            /// <returns></returns>
            public static EItemUpdateStatus GetItemUpdateProgress(UGCUpdateHandle_t handle, out float completion)
            {
                var result = SteamUGC.GetItemUpdateProgress(handle, out ulong current, out ulong total);
                if (result != EItemUpdateStatus.k_EItemUpdateStatusInvalid)
                    completion = Convert.ToSingle(current / (double)total);
                else
                    completion = 0;
                return result;
            }

            /// <summary>
            /// Returns the number of subscribed UGC items
            /// <para><see cref="https://partner.steamgames.com/doc/api/ISteamUGC#GetNumSubscribedItems"/></para>
            /// </summary>
            /// <returns>Returns 0 if called from a game server. else returns the number of subscribed items</returns>
            public static uint GetNumSubscribedItems() => SteamUGC.GetNumSubscribedItems();

            /// <summary>
            /// Request an additional preview for a UGC item
            /// <para><see cref="https://partner.steamgames.com/doc/api/ISteamUGC#GetQueryUGCAdditionalPreview"/></para>
            /// </summary>
            /// <param name="handle">The UGC query handle to get the results from.</param>
            /// <param name="index">The index of the item to get the details of.</param>
            /// <param name="previewIndex">The index of the additional preview to get the details of.</param>
            /// <param name="urlOrVideoId">Returns a URL or Video ID by copying it into this string.</param>
            /// <param name="urlOrVideoSize">The size of pchURLOrVideoID in bytes.</param>
            /// <param name="fileName">Returns the original file name. May be set to NULL to not receive this.</param>
            /// <param name="fileNameSize">The size of pchOriginalFileName in bytes.</param>
            /// <param name="type">The type of preview that was returned.</param>
            /// <returns>true upon success, indicates that pchURLOrVideoID and pPreviewType have been filled out.
            /// Otherwise, false if the UGC query handle is invalid, the index is out of bounds, or previewIndex is out of bounds.</returns>
            public static bool GetQueryAdditionalPreview(UGCQueryHandle_t handle, uint index, uint previewIndex, out string urlOrVideoId, uint urlOrVideoSize, string fileName, uint fileNameSize, out EItemPreviewType type) => SteamUGC.GetQueryUGCAdditionalPreview(handle, index, previewIndex, out urlOrVideoId, urlOrVideoSize, out fileName, fileNameSize, out type);

            /// <summary>
            /// Request the child items of a given UGC item
            /// <para><see cref="https://partner.steamgames.com/doc/api/ISteamUGC#GetQueryUGCChildren"/></para>
            /// </summary>
            /// <param name="handle">The UGC query handle to get the results from.</param>
            /// <param name="index">The index of the item to get the details of.</param>
            /// <param name="fileIds">Returns the UGC children by setting this array.</param>
            /// <param name="maxEntries">The length of pvecPublishedFileID.</param>
            /// <returns>true upon success, indicates that pvecPublishedFileID has been filled out.
            /// Otherwise, false if the UGC query handle is invalid or the index is out of bounds.</returns>
            public static bool GetQueryChildren(UGCQueryHandle_t handle, uint index, PublishedFileId_t[] fileIds, uint maxEntries) => SteamUGC.GetQueryUGCChildren(handle, index, fileIds, maxEntries);

            /// <summary>
            /// Retrieve the details of a key-value tag associated with an individual workshop item after receiving a querying UGC call result.
            /// <para><see cref="https://partner.steamgames.com/doc/api/ISteamUGC#GetQueryUGCKeyValueTag"/></para>
            /// </summary>
            /// <param name="handle">The UGC query handle to get the results from.</param>
            /// <param name="index">The index of the item to get the details of.</param>
            /// <param name="keyValueTagIndex">The index of the tag to get the details of.</param>
            /// <param name="key">Returns the key by copying it into this string.</param>
            /// <param name="value">Returns the value by copying it into this string.</param>
            /// <returns>true upon success, indicates that pchKey and pchValue have been filled out.
            /// Otherwise, false if the UGC query handle is invalid, the index is out of bounds, or keyValueTagIndex is out of bounds.</returns>
            public static bool GetQueryKeyValueTag(UGCQueryHandle_t handle, uint index, uint keyValueTagIndex, out string key, string value)
            {
                var ret = SteamUGC.GetQueryUGCKeyValueTag(handle, index, keyValueTagIndex, out key, 2048, out value, 2048);
                key = key.Trim();
                value = value.Trim();
                return ret;
            }

            /// <summary>
            /// Retrieve the details of a key-value tag associated with an individual workshop item after receiving a querying UGC call result.
            /// <para><see cref="https://partner.steamgames.com/doc/api/ISteamUGC#GetQueryUGCKeyValueTag"/></para>
            /// </summary>
            /// <param name="handle">The UGC query handle to get the results from.</param>
            /// <param name="index">The index of the item to get the details of.</param>
            /// <param name="keyValueTagIndex">The index of the tag to get the details of.</param>
            /// <param name="key">Returns the key by copying it into this string.</param>
            /// <param name="keySize">The size of key in bytes.</param>
            /// <param name="value">Returns the value by copying it into this string.</param>
            /// <param name="valueSize">The size of value in bytes.</param>
            /// <returns>true upon success, indicates that pchKey and pchValue have been filled out.
            /// Otherwise, false if the UGC query handle is invalid, the index is out of bounds, or keyValueTagIndex is out of bounds.</returns>
            public static bool GetQueryKeyValueTag(UGCQueryHandle_t handle, uint index, uint keyValueTagIndex, out string key, uint keySize, out string value, uint valueSize) => SteamUGC.GetQueryUGCKeyValueTag(handle, index, keyValueTagIndex, out key, keySize, out value, valueSize);

            /// <summary>
            /// Request the metadata of a UGC item
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="index"></param>
            /// <param name="metadata"></param>
            /// <param name="size"></param>
            /// <returns></returns>
            public static bool GetQueryMetadata(UGCQueryHandle_t handle, uint index, out string metadata, uint size) => SteamUGC.GetQueryUGCMetadata(handle, index, out metadata, size);

            /// <summary>
            /// Request the number of previews of a UGC item
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public static uint GetQueryNumAdditionalPreviews(UGCQueryHandle_t handle, uint index) => SteamUGC.GetQueryUGCNumAdditionalPreviews(handle, index);

            /// <summary>
            /// Request the number of key value tags for a UGC item
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public static uint GetQueryNumKeyValueTags(UGCQueryHandle_t handle, uint index) => SteamUGC.GetQueryUGCNumKeyValueTags(handle, index);

            /// <summary>
            /// Get the preview URL of a UGC item
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="index"></param>
            /// <param name="URL"></param>
            /// <param name="urlSize"></param>
            /// <returns></returns>
            public static bool GetQueryPreviewURL(UGCQueryHandle_t handle, uint index, out string URL, uint urlSize) => SteamUGC.GetQueryUGCPreviewURL(handle, index, out URL, urlSize);

            /// <summary>
            /// Fetch the results of a UGC query
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="index"></param>
            /// <param name="details"></param>
            /// <returns></returns>
            public static bool GetQueryResult(UGCQueryHandle_t handle, uint index, out SteamUGCDetails_t details) => SteamUGC.GetQueryUGCResult(handle, index, out details);

            /// <summary>
            /// Fetch the statistics of a UGC query
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="index"></param>
            /// <param name="statType"></param>
            /// <param name="statValue"></param>
            /// <returns></returns>
            public static bool GetQueryStatistic(UGCQueryHandle_t handle, uint index, EItemStatistic statType, out ulong statValue) => SteamUGC.GetQueryUGCStatistic(handle, index, statType, out statValue);

            /// <summary>
            /// Get the file IDs of all subscribed UGC items up to the array size
            /// </summary>
            /// <param name="fileIDs"></param>
            /// <param name="maxEntries"></param>
            /// <returns></returns>
            public static uint GetSubscribedItems(PublishedFileId_t[] fileIDs, uint maxEntries) => SteamUGC.GetSubscribedItems(fileIDs, maxEntries);

            /// <summary>
            /// Returns the IDs of the files this user is subscribed to
            /// </summary>
            /// <returns></returns>
            public static PublishedFileId_t[] GetSubscribedItems()
            {
                var count = GetNumSubscribedItems();
                if (count > 0)
                {
                    var fileIds = new PublishedFileId_t[count];
                    if (GetSubscribedItems(fileIds, count) > 0)
                    {
                        return fileIds;
                    }
                    else
                        return new PublishedFileId_t[0];
                }
                else
                    return null;
            }

            /// <summary>
            /// Invokes a callback after querying the files and details of the items this user is subscribed to
            /// </summary>
            /// <param name="callback"></param>
            public static void GetSubscribedItems(Action<List<WorkshopItem>> callback)
            {
                var query = UgcQuery.GetSubscribed();
                query.Execute(r =>
                {
                    callback?.Invoke(query.ResultsList);
                    query.Dispose();
                });     
            }

            /// <summary>
            /// Invokes a callback after querying the files and details of the items this user is subscribed to
            /// </summary>
            /// <param name="withAdditionalPreviews">Should we request additional preview images or just the main</param>
            /// <param name="withKeyValueTags">Should we request key value tags as well</param>
            /// <param name="withLongDescription">Should we request the full long hand description</param>
            /// <param name="withMetadata">Should we also request metadata</param>
            /// <param name="withPlayTimeStatsInDays">If greater than 0 then we will also request play time stats over this many days</param>
            /// <param name="callback"></param>
            public static void GetSubscribedItems(bool withLongDescription, bool withMetadata, bool withKeyValueTags, bool withAdditionalPreviews, uint withPlayTimeStatsInDays, Action<List<WorkshopItem>> callback)
            {
                var query = UgcQuery.GetSubscribed(withLongDescription, withMetadata, withKeyValueTags, withAdditionalPreviews, withPlayTimeStatsInDays);
                query.Execute(r =>
                {
                    callback?.Invoke(query.ResultsList);
                    query.Dispose();
                });
            }

            /// <summary>
            /// Get the item vote value of a UGC item
            /// </summary>
            /// <param name="fileId"></param>
            public static void GetUserItemVote(PublishedFileId_t fileId, Action<GetUserItemVoteResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_GetUserItemVoteResult == null)
                    m_GetUserItemVoteResult = CallResult<GetUserItemVoteResult_t>.Create();

                var call = SteamUGC.GetUserItemVote(fileId);
                m_GetUserItemVoteResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Asynchronously retrieves data about whether the user accepted the Workshop EULA for the current app.
            /// </summary>
            public static void GetWorkshopEULAStatus(Action<WorkshopEULAStatus_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_WorkshopEULAStatus == null)
                    m_WorkshopEULAStatus = CallResult<WorkshopEULAStatus_t>.Create();

                var handle = SteamUGC.GetWorkshopEULAStatus();
                m_WorkshopEULAStatus.Set(handle, callback.Invoke);
            }

            /// <summary>
            /// Show the app's latest Workshop EULA to the user in an overlay window, where they can accept it or not
            /// </summary>
            /// <returns></returns>
            public static bool ShowWorkshopEULA() => SteamUGC.ShowWorkshopEULA();

            /// <summary>
            /// Request the removal of app dependency from a UGC item
            /// </summary>
            /// <param name="fileId"></param>
            /// <param name="appId"></param>
            public static void RemoveAppDependency(PublishedFileId_t fileId, AppId_t appId, Action<RemoveAppDependencyResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_RemoveAppDependencyResult == null)
                    m_RemoveAppDependencyResult = CallResult<RemoveAppDependencyResult_t>.Create();

                var call = SteamUGC.RemoveAppDependency(fileId, appId);
                m_RemoveAppDependencyResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Request the removal of a dependency from a UGC item
            /// </summary>
            /// <param name="parentFileId"></param>
            /// <param name="childFileId"></param>
            public static void RemoveDependency(PublishedFileId_t parentFileId, PublishedFileId_t childFileId, Action<RemoveUGCDependencyResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_RemoveDependencyResult == null)
                    m_RemoveDependencyResult = CallResult<RemoveUGCDependencyResult_t>.Create();

                var call = SteamUGC.RemoveDependency(parentFileId, childFileId);
                m_RemoveDependencyResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Removes the UGC item from user favorites
            /// </summary>
            /// <param name="appId"></param>
            /// <param name="fileId"></param>
            public static void RemoveItemFromFavorites(AppId_t appId, PublishedFileId_t fileId, Action<UserFavoriteItemsListChanged_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_UserFavoriteItemsListChanged == null)
                    m_UserFavoriteItemsListChanged = CallResult<UserFavoriteItemsListChanged_t>.Create();

                var call = SteamUGC.RemoveItemFromFavorites(appId, fileId);
                m_UserFavoriteItemsListChanged.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Remove UGC item key value tags
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static bool RemoveItemKeyValueTags(UGCUpdateHandle_t handle, string key) => SteamUGC.RemoveItemKeyValueTags(handle, key);

            /// <summary>
            /// Removes UGC item preview
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public static bool RemoveItemPreview(UGCUpdateHandle_t handle, uint index) => SteamUGC.RemoveItemPreview(handle, index);

            /// <summary>
            /// Requests details of a UGC item
            /// </summary>
            /// <param name="fileId"></param>
            /// <param name="maxAgeSeconds"></param>
            public static void RequestDetails(PublishedFileId_t fileId, uint maxAgeSeconds, Action<SteamUGCRequestUGCDetailsResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_SteamUGCRequestUGCDetailsResult == null)
                    m_SteamUGCRequestUGCDetailsResult = CallResult<SteamUGCRequestUGCDetailsResult_t>.Create();

                var call = SteamUGC.RequestUGCDetails(fileId, maxAgeSeconds);
                m_SteamUGCRequestUGCDetailsResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Sends a UGC query
            /// </summary>
            /// <param name="handle"></param>
            public static void SendQueryUGCRequest(UGCQueryHandle_t handle, Action<SteamUGCQueryCompleted_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_SteamUGCQueryCompleted == null)
                    m_SteamUGCQueryCompleted = CallResult<SteamUGCQueryCompleted_t>.Create();

                var call = SteamUGC.SendQueryUGCRequest(handle);
                m_SteamUGCQueryCompleted.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Set allow cached response
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="maxAgeSeconds"></param>
            /// <returns></returns>
            public static bool SetAllowCachedResponse(UGCQueryHandle_t handle, uint maxAgeSeconds) => SteamUGC.SetAllowCachedResponse(handle, maxAgeSeconds);

            /// <summary>
            /// Set cloud file name filter
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="fileName"></param>
            /// <returns></returns>
            public static bool SetCloudFileNameFilter(UGCQueryHandle_t handle, string fileName) => SteamUGC.SetCloudFileNameFilter(handle, fileName);

            /// <summary>
            /// Set item content path
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="folder"></param>
            /// <returns></returns>
            public static bool SetItemContent(UGCUpdateHandle_t handle, string folder) => SteamUGC.SetItemContent(handle, folder);

            /// <summary>
            /// Set item description
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="description"></param>
            /// <returns></returns>
            public static bool SetItemDescription(UGCUpdateHandle_t handle, string description) => SteamUGC.SetItemDescription(handle, description);

            /// <summary>
            /// Set item metadata
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="metadata"></param>
            /// <returns></returns>
            public static bool SetItemMetadata(UGCUpdateHandle_t handle, string metadata) => SteamUGC.SetItemMetadata(handle, metadata);

            /// <summary>
            /// Set item preview
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="previewFile"></param>
            /// <returns></returns>
            public static bool SetItemPreview(UGCUpdateHandle_t handle, string previewFile) => SteamUGC.SetItemPreview(handle, previewFile);

            /// <summary>
            /// Set item tags
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="tags"></param>
            /// <returns></returns>
            public static bool SetItemTags(UGCUpdateHandle_t handle, List<string> tags) => SteamUGC.SetItemTags(handle, tags);

            /// <summary>
            /// Set item title
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="title"></param>
            /// <returns></returns>
            public static bool SetItemTitle(UGCUpdateHandle_t handle, string title) => SteamUGC.SetItemTitle(handle, title);

            /// <summary>
            /// Set item update language
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="language"></param>
            /// <returns></returns>
            public static bool SetItemUpdateLanguage(UGCUpdateHandle_t handle, string language) => SteamUGC.SetItemUpdateLanguage(handle, language);

            /// <summary>
            /// Set item visibility
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="visibility"></param>
            /// <returns></returns>
            public static bool SetItemVisibility(UGCUpdateHandle_t handle, ERemoteStoragePublishedFileVisibility visibility) => SteamUGC.SetItemVisibility(handle, visibility);

            /// <summary>
            /// Set item language
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="language"></param>
            /// <returns></returns>
            public static bool SetLanguage(UGCQueryHandle_t handle, string language) => SteamUGC.SetLanguage(handle, language);

            /// <summary>
            /// Set match any tag
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="anyTag"></param>
            /// <returns></returns>
            public static bool SetMatchAnyTag(UGCQueryHandle_t handle, bool anyTag) => SteamUGC.SetMatchAnyTag(handle, anyTag);

            /// <summary>
            /// Set ranked by trend days
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="days"></param>
            /// <returns></returns>
            public static bool SetRankedByTrendDays(UGCQueryHandle_t handle, uint days) => SteamUGC.SetRankedByTrendDays(handle, days);

            /// <summary>
            /// Set return additional previews
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="additionalPreviews"></param>
            /// <returns></returns>
            public static bool SetReturnAdditionalPreviews(UGCQueryHandle_t handle, bool additionalPreviews) => SteamUGC.SetReturnAdditionalPreviews(handle, additionalPreviews);

            /// <summary>
            /// Set return children
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="returnChildren"></param>
            /// <returns></returns>
            public static bool SetReturnChildren(UGCQueryHandle_t handle, bool returnChildren) => SteamUGC.SetReturnChildren(handle, returnChildren);

            /// <summary>
            /// Set return key value tags
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="tags"></param>
            /// <returns></returns>
            public static bool SetReturnKeyValueTags(UGCQueryHandle_t handle, bool tags) => SteamUGC.SetReturnKeyValueTags(handle, tags);

            /// <summary>
            /// SEt return long description
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="longDescription"></param>
            /// <returns></returns>
            public static bool SetReturnLongDescription(UGCQueryHandle_t handle, bool longDescription) => SteamUGC.SetReturnLongDescription(handle, longDescription);

            /// <summary>
            /// Set return metadata
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="metadata"></param>
            /// <returns></returns>
            public static bool SetReturnMetadata(UGCQueryHandle_t handle, bool metadata) => SteamUGC.SetReturnMetadata(handle, metadata);

            /// <summary>
            /// Set return IDs only
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="onlyIds"></param>
            /// <returns></returns>
            public static bool SetReturnOnlyIDs(UGCQueryHandle_t handle, bool onlyIds) => SteamUGC.SetReturnOnlyIDs(handle, onlyIds);

            /// <summary>
            /// Set return playtime stats
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="days"></param>
            /// <returns></returns>
            public static bool SetReturnPlaytimeStats(UGCQueryHandle_t handle, uint days) => SteamUGC.SetReturnPlaytimeStats(handle, days);

            /// <summary>
            /// Set return total only
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="totalOnly"></param>
            /// <returns></returns>
            public static bool SetReturnTotalOnly(UGCQueryHandle_t handle, bool totalOnly) => SteamUGC.SetReturnTotalOnly(handle, totalOnly);

            /// <summary>
            /// Set search text
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="text"></param>
            /// <returns></returns>
            public static bool SetSearchText(UGCQueryHandle_t handle, string text) => SteamUGC.SetSearchText(handle, text);

            /// <summary>
            /// Set user item vote
            /// </summary>
            /// <param name="fileID"></param>
            /// <param name="voteUp"></param>
            public static void SetUserItemVote(PublishedFileId_t fileID, bool voteUp, Action<SetUserItemVoteResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_SetUserItemVoteResult == null)
                    m_SetUserItemVoteResult = CallResult<SetUserItemVoteResult_t>.Create();

                var call = SteamUGC.SetUserItemVote(fileID, voteUp);
                m_SetUserItemVoteResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Start item update
            /// </summary>
            /// <param name="appId"></param>
            /// <param name="fileID"></param>
            /// <returns></returns>
            public static UGCUpdateHandle_t StartItemUpdate(AppId_t appId, PublishedFileId_t fileID) => SteamUGC.StartItemUpdate(appId, fileID);

            /// <summary>
            /// Start playtime tracking
            /// </summary>
            /// <param name="fileIds"></param>
            public static void StartPlaytimeTracking(PublishedFileId_t[] fileIds, Action<StartPlaytimeTrackingResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_StartPlaytimeTrackingResult == null)
                    m_StartPlaytimeTrackingResult = CallResult<StartPlaytimeTrackingResult_t>.Create();

                var call = SteamUGC.StartPlaytimeTracking(fileIds, (uint)fileIds.Length);
                m_StartPlaytimeTrackingResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Stop playtime tracking
            /// </summary>
            /// <param name="fileIds"></param>
            public static void StopPlaytimeTracking(PublishedFileId_t[] fileIds, Action<StopPlaytimeTrackingResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_StopPlaytimeTrackingResult == null)
                    m_StopPlaytimeTrackingResult = CallResult<StopPlaytimeTrackingResult_t>.Create();

                var call = SteamUGC.StopPlaytimeTracking(fileIds, (uint)fileIds.Length);
                m_StopPlaytimeTrackingResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// stop playtime tracking for all items
            /// </summary>
            public static void StopPlaytimeTrackingForAllItems(Action<StopPlaytimeTrackingResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_StopPlaytimeTrackingResult == null)
                    m_StopPlaytimeTrackingResult = CallResult<StopPlaytimeTrackingResult_t>.Create();

                var call = SteamUGC.StopPlaytimeTrackingForAllItems();
                m_StopPlaytimeTrackingResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Submit item update
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="changeNote"></param>
            public static void SubmitItemUpdate(UGCUpdateHandle_t handle, string changeNote, Action<SubmitItemUpdateResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_SubmitItemUpdateResult == null)
                    m_SubmitItemUpdateResult = CallResult<SubmitItemUpdateResult_t>.Create();

                var call = SteamUGC.SubmitItemUpdate(handle, changeNote);
                m_SubmitItemUpdateResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Subscribe to item
            /// </summary>
            /// <param name="fileId"></param>
            public static void SubscribeItem(PublishedFileId_t fileId, Action<RemoteStorageSubscribePublishedFileResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_RemoteStorageSubscribePublishedFileResult == null)
                    m_RemoteStorageSubscribePublishedFileResult = CallResult<RemoteStorageSubscribePublishedFileResult_t>.Create();

                var call = SteamUGC.SubscribeItem(fileId);
                m_RemoteStorageSubscribePublishedFileResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Suspend downloads
            /// </summary>
            /// <param name="suspend"></param>
            public static void SuspendDownloads(bool suspend) => SteamUGC.SuspendDownloads(suspend);

            /// <summary>
            /// Unsubscribe to item
            /// </summary>
            /// <param name="fileId"></param>
            public static void UnsubscribeItem(PublishedFileId_t fileId, Action<RemoteStorageUnsubscribePublishedFileResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_RemoteStorageUnsubscribePublishedFileResult == null)
                    m_RemoteStorageUnsubscribePublishedFileResult = CallResult<RemoteStorageUnsubscribePublishedFileResult_t>.Create();

                var call = SteamUGC.UnsubscribeItem(fileId);
                m_RemoteStorageUnsubscribePublishedFileResult.Set(call, callback.Invoke);
            }

            /// <summary>
            /// Update item preview file
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="index"></param>
            /// <param name="file"></param>
            /// <returns></returns>
            public static bool UpdateItemPreviewFile(UGCUpdateHandle_t handle, uint index, string file) => SteamUGC.UpdateItemPreviewFile(handle, index, file);

            /// <summary>
            /// Update item preview video
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="index"></param>
            /// <param name="videoId"></param>
            /// <returns></returns>
            public static bool UpdateItemPreviewVideo(UGCUpdateHandle_t handle, uint index, string videoId) => SteamUGC.UpdateItemPreviewVideo(handle, index, videoId);
#endregion
        }
    }
}
#endif