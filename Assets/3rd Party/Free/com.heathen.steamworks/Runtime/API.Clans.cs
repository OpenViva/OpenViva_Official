#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Access to the Steam Clan aka Steam Group system
    /// </summary>
    public static class Clans
    {
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                eventGameConnectedClanChatMsg = new();
                eventGameConnectedChatJoin = new();
                eventGameConnectedChatLeave = new();
                m_DownloadClanActivityCountsResult_t = null;
                m_ClanOfficerListResponse_t = null;
                m_JoinClanChatRoomCompletionResult_t = null;
                m_GameConnectedClanChatMsg_t = null;
                m_GameConnectedChatJoin_t = null;
                m_GameConnectedChatLeave_t = null;
                joinedRooms.Clear();
            }

            /// <summary>
            /// This is provided for debugging purposes and generally shouldn't be used
            /// </summary>
            /// <remarks>
            /// The JoinChatRoom callback provides the ClanChatRoom you just entered you should cashed and use that as opposed to reading from this list.
            /// The chat related events also send the ClanChatRoom meaning there is little reason to read the list of rooms save for debugging.
            /// </remarks>
            public static readonly List<ChatRoom> joinedRooms = new List<ChatRoom>();

            /// <summary>
            /// Called when a chat message has been received in a Steam group chat that we are in.
            /// </summary>
            public static GameConnectedClanChatMsgEvent EventChatMessageReceived
            {
                get
                {
                    if (m_GameConnectedClanChatMsg_t == null)
                        m_GameConnectedClanChatMsg_t = Callback<GameConnectedClanChatMsg_t>.Create((result) =>
                        {
                            var room = joinedRooms.FirstOrDefault(p => p.id == result.m_steamIDClanChat);
                            
                            if(room.clan == default(ClanData))
                            {
                                room.id = result.m_steamIDClanChat;
                                room.enterResponse = EChatRoomEnterResponse.k_EChatRoomEnterResponseError;

                                if (App.isDebugging)
                                    Debug.LogWarning("Received a message from chat room: " + room.id + ", no such room is known!");
                            }

                            var message = GetChatMessage(result.m_steamIDClanChat, result.m_iMessageID, out EChatEntryType type, out CSteamID chatter);
                            var output = new ClanChatMsg
                            {
                                room = room,
                                message = message,
                                type = type,
                                user = chatter
                            };

                            eventGameConnectedClanChatMsg.Invoke(output);
                        });

                    return eventGameConnectedClanChatMsg;
                }
            }
            /// <summary>
            /// Called when a user has joined a Steam group chat that the we are in.
            /// </summary>
            public static GameConnectedChatJoinEvent EventGameConnectedChatJoin
            {
                get
                {
                    if (m_GameConnectedChatJoin_t == null)
                        m_GameConnectedChatJoin_t = Callback<GameConnectedChatJoin_t>.Create((result) =>
                        {
                            var room = joinedRooms.FirstOrDefault(p => p.id == result.m_steamIDClanChat);

                            if (room.clan == default(ClanData))
                            {
                                room.id = result.m_steamIDClanChat;
                                room.enterResponse = EChatRoomEnterResponse.k_EChatRoomEnterResponseError;

                                if (App.isDebugging)
                                    Debug.LogWarning("Received a chat join event from chat room: " + room.id + ", no such room is known!");
                            }

                            eventGameConnectedChatJoin.Invoke(room, result.m_steamIDUser);
                        });

                    return eventGameConnectedChatJoin;
                }
            }
            /// <summary>
            /// Called when a user has left a Steam group chat that the we are in.
            /// </summary>
            public static GameConnectedChatLeaveEvent EventGameConnectedChatLeave
            {
                get
                {
                    if (m_GameConnectedChatLeave_t == null)
                        m_GameConnectedChatLeave_t = Callback<GameConnectedChatLeave_t>.Create((result) =>
                        {
                            var room = joinedRooms.FirstOrDefault(p => p.id == result.m_steamIDClanChat);

                            if (room.clan == default(ClanData))
                            {
                                room.id = result.m_steamIDClanChat;
                                room.enterResponse = EChatRoomEnterResponse.k_EChatRoomEnterResponseError;

                                if (App.isDebugging)
                                    Debug.LogWarning("Received a chat leave event from chat room: " + room.id + ", no such room is known!");
                            }

                            eventGameConnectedChatLeave.Invoke(new UserLeaveData
                            {
                                room = room,
                                user = result.m_steamIDUser,
                                dropped = result.m_bDropped,
                                kicked = result.m_bKicked
                            });
                        });

                    return eventGameConnectedChatLeave;
                }
            }

            private static GameConnectedClanChatMsgEvent eventGameConnectedClanChatMsg = new();
            private static GameConnectedChatJoinEvent eventGameConnectedChatJoin = new();
            private static GameConnectedChatLeaveEvent eventGameConnectedChatLeave = new();

            private static CallResult<DownloadClanActivityCountsResult_t> m_DownloadClanActivityCountsResult_t;
            private static CallResult<ClanOfficerListResponse_t> m_ClanOfficerListResponse_t;
            private static CallResult<JoinClanChatRoomCompletionResult_t> m_JoinClanChatRoomCompletionResult_t;

            private static Callback<GameConnectedClanChatMsg_t> m_GameConnectedClanChatMsg_t;
            private static Callback<GameConnectedChatJoin_t> m_GameConnectedChatJoin_t;
            private static Callback<GameConnectedChatLeave_t> m_GameConnectedChatLeave_t;

            /// <summary>
            /// Allows the user to join Steam group (clan) chats right within the game.
            /// </summary>
            /// <remarks>
            /// The behavior is somewhat complicated, because the user may or may not be already in the group chat from outside the game or in the overlay. You can use ActivateGameOverlayToUser to open the in-game overlay version of the chat.
            /// </remarks>
            /// <param name="clan"></param>
            /// <param name="callback"></param>
            public static void JoinChatRoom(ClanData clan, Action<ChatRoom, bool> callback)
            {
                if (callback == null)
                    return;

                m_JoinClanChatRoomCompletionResult_t ??= CallResult<JoinClanChatRoomCompletionResult_t>.Create();

                var handle = SteamFriends.JoinClanChatRoom(clan);
                m_JoinClanChatRoomCompletionResult_t.Set(handle, (r, e) =>
                {
                    if (!e)
                    {
                        var cc = new ChatRoom
                        {
                            clan = clan,
                            id = r.m_steamIDClanChat,
                            enterResponse = r.m_eChatRoomEnterResponse
                        };

                        if (r.m_eChatRoomEnterResponse == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                            joinedRooms.Add(cc);

                        callback.Invoke(cc, e);
                    }
                    else
                        callback.Invoke(default, e);
                    
                });
            }
            /// <summary>
            /// Leaves a Steam group chat that the user has previously entered with JoinClanChatRoom.
            /// </summary>
            /// <param name="clanChatId">This can be the ChatRoom ID or the Clan ID both will work</param>
            public static bool LeaveChatRoom(CSteamID clanChatId)
            {
                joinedRooms.RemoveAll(p => p.id == clanChatId);

                return SteamFriends.LeaveClanChatRoom(clanChatId);
            }
            /// <summary>
            /// Leaves a Steam group chat that the user has previously entered with JoinClanChatRoom.
            /// </summary>
            public static bool LeaveChatRoom(ChatRoom clanChat)
            {
                joinedRooms.Remove(clanChat);

                return SteamFriends.LeaveClanChatRoom(clanChat.id);
            }
            /// <summary>
            /// Gets the Steam ID at the given index in a Steam group chat.
            /// </summary>
            /// <param name="clan">This MUST be the same source used in the previous call to GetClanChatMemberCount!</param>
            /// <param name="index">An index between 0 and GetClanChatMemberCount.</param>
            /// <returns></returns>
            public static UserData GetChatMemberByIndex(CSteamID clan, int index) => SteamFriends.GetChatMemberByIndex(clan, index);
            /// <summary>
            /// Gets the most recent information we have about what the users in a Steam Group are doing.
            /// </summary>
            /// <param name="clan">The Steam group to get the activity of.</param>
            /// <param name="online">Returns the number of members that are online.</param>
            /// <param name="inGame">Returns the number members that are in game (excluding those with their status set to offline).</param>
            /// <param name="chatting">Returns the number of members in the group chat room.</param>
            /// <returns></returns>
            public static bool GetActivityCounts(CSteamID clan, out int online, out int inGame, out int chatting) => SteamFriends.GetClanActivityCounts(clan, out online, out inGame, out chatting);
            /// <summary>
            /// Gets the Steam group's Steam ID at the given index.
            /// </summary>
            /// <param name="clanIndex">An index between 0 and GetClanCount.</param>
            /// <returns></returns>
            public static ClanData GetClanByIndex(int clanIndex) => SteamFriends.GetClanByIndex(clanIndex);
            /// <summary>
            /// Gets the list of clans the current user is a member of
            /// </summary>
            /// <returns></returns>
            public static ClanData[] GetClans()
            {
                var count = SteamFriends.GetClanCount();
                var results = new ClanData[count];
                for (int i = 0; i < count; i++)
                {
                    results[i] = SteamFriends.GetClanByIndex(i);
                }
                return results;
            }
            /// <summary>
            /// Get the number of users in a Steam group chat.
            /// </summary>
            /// <remarks>
            /// Get the number of users in a Steam group chat. NOTE: Large steam groups cannot be iterated by the local user. NOTE: The current user must be in a lobby to retrieve the Steam IDs of other users in that lobby. This is used for iteration, after calling this then GetChatMemberByIndex can be used to get the Steam ID of each person in the chat.
            /// </remarks>
            /// <param name="clanId">The Steam group to get the chat count of.</param>
            /// <returns></returns>
            public static int GetChatMemberCount(ClanData clanId) => SteamFriends.GetClanChatMemberCount(clanId);
            /// <summary>
            /// Returns a list of the members of the given clans chat
            /// </summary>
            /// <remarks>
            /// Get the number of users in a Steam group chat. NOTE: Large steam groups cannot be iterated by the local user. NOTE: The current user must be in a lobby to retrieve the Steam IDs of other users in that lobby. This is used for iteration, after calling this then GetChatMemberByIndex can be used to get the Steam ID of each person in the chat.
            /// </remarks>
            /// <param name="clanId">The Steam group to get the chat count of.</param>
            /// <returns></returns>
            public static UserData[] GetChatMembers(ClanData clanId)
            {
                var count = SteamFriends.GetClanChatMemberCount(clanId);

                if (count > 0)
                {
                    var results = new UserData[count];
                    for (int i = 0; i < count; i++)
                    {
                        results[i] = SteamFriends.GetChatMemberByIndex(clanId, i);
                    }

                    return results;
                }
                else
                    return new UserData[0];
            }
            /// <summary>
            /// Gets the data from a Steam group chat room message.
            /// </summary>
            /// <param name="clanChatId">The Steam ID of the Steam group chat room.</param>
            /// <param name="index">The index of the message. This should be the m_iMessageID field of GameConnectedClanChatMsg_t.</param>
            /// <param name="type">Returns the type of chat entry that was received.</param>
            /// <param name="chatter">Returns the Steam ID of the user that sent the message.</param>
            /// <returns></returns>
            public static string GetChatMessage(CSteamID clanChatId, int index, out EChatEntryType type, out CSteamID chatter)
            {
                if (SteamFriends.GetClanChatMessage(clanChatId, index, out string result, 8193, out type, out chatter) > 0)
                {
                    return result;
                }
                else
                    return string.Empty;
            }
            /// <summary>
            /// Gets the data from a Steam group chat room message.
            /// </summary>
            /// <param name="clanChat">The Steam ID of the Steam group chat room.</param>
            /// <param name="index">The index of the message. This should be the m_iMessageID field of GameConnectedClanChatMsg_t.</param>
            /// <param name="type">Returns the type of chat entry that was received.</param>
            /// <param name="chatter">Returns the Steam ID of the user that sent the message.</param>
            /// <returns></returns>
            public static string GetChatMessage(ChatRoom clanChat, int index, out EChatEntryType type, out CSteamID chatter)
            {
                if (SteamFriends.GetClanChatMessage(clanChat.id, index, out string result, 8193, out type, out chatter) > 0)
                {
                    return result;
                }
                else
                    return string.Empty;
            }
            /// <summary>
            /// Gets the number of Steam groups that the current user is a member of.
            /// </summary>
            /// <returns></returns>
            public static int GetClanCount() => SteamFriends.GetClanCount();
            /// <summary>
            /// Gets the display name for the specified Steam group; if the local client knows about it.
            /// </summary>
            /// <param name="clanId">The Steam group to get the name of.</param>
            /// <returns></returns>
            public static string GetName(ClanData clanId) => SteamFriends.GetClanName(clanId);
            /// <summary>
            /// Gets the Steam ID of the officer at the given index in a Steam group.
            /// </summary>
            /// <remarks>
            /// NOTE: You must call GetClanOfficerCount before calling this.
            /// </remarks>
            /// <param name="clanId"></param>
            /// <param name="officerIndex"></param>
            /// <returns></returns>
            public static UserData GetOfficerByIndex(ClanData clanId, int officerIndex) => SteamFriends.GetClanOfficerByIndex(clanId, officerIndex);
            /// <summary>
            /// Gets the list of officers for the given clan
            /// </summary>
            /// <param name="clanId">The Steam ID of the group to query for</param>
            /// <returns></returns>
            public static UserData[] GetOfficers(ClanData clanId)
            {
                SteamFriends.RequestClanOfficerList(clanId);

                var count = SteamFriends.GetClanOfficerCount(clanId);
                if (count > 0)
                {
                    var results = new UserData[count];
                    for (int i = 0; i < count; i++)
                    {
                        results[i] = SteamFriends.GetClanOfficerByIndex(clanId, i);
                    }

                    return results;
                }
                else
                    return new UserData[0];
            }
            /// <summary>
            /// Gets the number of officers (administrators and moderators) in a specified Steam group. This also includes the owner of the Steam group. This is used for iteration, after calling this then GetClanOfficerByIndex can be used to get the Steam ID of each officer.
            /// </summary>
            /// <remarks>
            /// NOTE: You must call RequestClanOfficerList before this to get the required data!
            /// </remarks>
            /// <param name="clanId">The clan to query</param>
            /// <returns></returns>
            public static int GetOfficerCount(ClanData clanId) => SteamFriends.GetClanOfficerCount(clanId);
            /// <summary>
            /// Gets the owner of a Steam Group.
            /// </summary>
            /// <remarks>
            /// NOTE: You must call RequestClanOfficerList before this to get the required data!
            /// </remarks>
            /// <param name="clanId"></param>
            /// <returns></returns>
            public static UserData GetOwner(ClanData clanId) => SteamFriends.GetClanOwner(clanId);
            /// <summary>
            /// Gets the unique tag (abbreviation) for the specified Steam group; If the local client knows about it. The Steam group abbreviation is a unique way for people to identify the group and is limited to 12 characters.In some games this will appear next to the name of group members.
            /// </summary>
            /// <param name="clanId">The Steam group to get the tag of.</param>
            /// <returns></returns>
            public static string GetTag(ClanData clanId) => SteamFriends.GetClanTag(clanId);
            /// <summary>
            /// Opens the specified Steam group chat room in the Steam UI.
            /// </summary>
            /// <param name="clanChatRoomId"></param>
            /// <returns>
            /// true if the user successfully entered the Steam group chat room. false in one of the following situations:
            /// <para>
            /// The provided Steam group chat room does not exist or the user does not have access to join it.
            /// </para>
            /// <para>
            /// The current user is currently rate limited.
            /// </para>
            /// <para>
            /// The current user is chat restricted.
            /// </para>
            /// </returns>
            public static bool OpenChatWindowInSteam(CSteamID clanChatRoomId) => SteamFriends.OpenClanChatWindowInSteam(clanChatRoomId);
            /// <summary>
            /// Opens the specified Steam group chat room in the Steam UI.
            /// </summary>
            /// <param name="clanChatRoomId"></param>
            /// <returns>
            /// true if the user successfully entered the Steam group chat room. false in one of the following situations:
            /// <para>
            /// The provided Steam group chat room does not exist or the user does not have access to join it.
            /// </para>
            /// <para>
            /// The current user is currently rate limited.
            /// </para>
            /// <para>
            /// The current user is chat restricted.
            /// </para>
            /// </returns>
            public static bool OpenChatWindowInSteam(ChatRoom clanChat) => SteamFriends.OpenClanChatWindowInSteam(clanChat.id);
            /// <summary>
            /// Sends a message to a Steam group chat room.
            /// </summary>
            /// <param name="clanChatId"></param>
            /// <param name="message"></param>
            /// <returns>
            /// <para>
            /// true if the message was successfully sent.
            /// </para>
            /// <para>
            /// false under one of the following circumstances:
            /// </para>
            /// <list type="bullet">
            /// <item>The current user is not in the specified group chat.</item>
            /// <item>The current user is not connected to Steam.</item>
            /// <item>The current user is rate limited.</item>
            /// <item>The current user is chat restricted.</item>
            /// <item>The message exceeds 2048 characters.</item>
            /// </list>
            /// </returns>
            public static bool SendChatMessage(CSteamID clanChatId, string message) => SteamFriends.SendClanChatMessage(clanChatId, message);
            /// <summary>
            /// Sends a message to a Steam group chat room.
            /// </summary>
            /// <param name="clanChat"></param>
            /// <param name="message"></param>
            /// <returns>
            /// <para>
            /// true if the message was successfully sent.
            /// </para>
            /// <para>
            /// false under one of the following circumstances:
            /// </para>
            /// <list type="bullet">
            /// <item>The current user is not in the specified group chat.</item>
            /// <item>The current user is not connected to Steam.</item>
            /// <item>The current user is rate limited.</item>
            /// <item>The current user is chat restricted.</item>
            /// <item>The message exceeds 2048 characters.</item>
            /// </list>
            /// </returns>
            public static bool SendChatMessage(ChatRoom clanChat, string message) => SteamFriends.SendClanChatMessage(clanChat.id, message);
            public static bool IsClanChatAdmin(CSteamID clanChatId, CSteamID userId) => SteamFriends.IsClanChatAdmin(clanChatId, userId);
            public static bool IsClanChatAdmin(ChatRoom clanChat, CSteamID userId) => SteamFriends.IsClanChatAdmin(clanChat.id, userId);
            /// <summary>
            /// Checks if the Steam group is public.
            /// </summary>
            /// <param name="clanId"></param>
            /// <returns></returns>
            public static bool IsClanPublic(ClanData clanId) => SteamFriends.IsClanPublic(clanId);
            /// <summary>
            /// Checks if the Steam group is an official game group/community hub.
            /// </summary>
            /// <param name="clanId"></param>
            /// <returns></returns>
            public static bool IsClanOfficialGameGroup(ClanData clanId) => SteamFriends.IsClanOfficialGameGroup(clanId);
            /// <summary>
            /// Checks if the Steam Group chat room is open in the Steam UI.
            /// </summary>
            /// <param name="clanChatId"></param>
            /// <returns></returns>
            public static bool IsClanChatWindowOpenInSteam(CSteamID clanChatId) => SteamFriends.IsClanChatWindowOpenInSteam(clanChatId);
            /// <summary>
            /// Requests information about a Steam group officers (administrators and moderators).
            /// </summary>
            /// <remarks>
            /// NOTE: You can only ask about Steam groups that a user is a member of. NOTE: This won't download avatars for the officers automatically. If no avatar image is available for an officer, then call RequestUserInformation to download the avatar.
            /// </remarks>
            /// <param name="clanId"></param>
            /// <param name="callback"></param>
            public static void RequestClanOfficerList(CSteamID clanId, Action<ClanOfficerListResponse_t, bool> callback)
            {
                if (callback == null)
                    return;

                m_ClanOfficerListResponse_t ??= CallResult<ClanOfficerListResponse_t>.Create();

                var handle = SteamFriends.RequestClanOfficerList(clanId);
                m_ClanOfficerListResponse_t.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Closes the specified Steam group chat room in the Steam UI.
            /// </summary>
            /// <param name="clanChatId">The Steam ID of the Steam group chat room to close.</param>
            /// <returns></returns>
            public static bool CloseClanChatWindowInSteam(CSteamID clanChatId) => SteamFriends.CloseClanChatWindowInSteam(clanChatId);
            /// <summary>
            /// Closes the specified Steam group chat room in the Steam UI.
            /// </summary>
            /// <param name="clanChatId">The Steam ID of the Steam group chat room to close.</param>
            /// <returns></returns>
            public static bool CloseClanChatWindowInSteam(ChatRoom clanChat) => SteamFriends.CloseClanChatWindowInSteam(clanChat.id);
            /// <summary>
            /// Refresh the Steam Group activity data or get the data from groups other than one that the current user is a member.
            /// </summary>
            /// <param name="clans">A list of steam groups to get the updated data for.</param>
            /// <param name="callback">Invoked when Steam API responds with the results</param>
            public static void DownloadClanActivityCounts(CSteamID[] clans, Action<DownloadClanActivityCountsResult_t, bool> callback)
            {
                if (callback == null)
                    return;

                m_DownloadClanActivityCountsResult_t ??= CallResult<DownloadClanActivityCountsResult_t>.Create();

                var handle = SteamFriends.DownloadClanActivityCounts(clans, clans.Length);
                m_DownloadClanActivityCountsResult_t.Set(handle, callback.Invoke);
            }
        }
    }
}
#endif