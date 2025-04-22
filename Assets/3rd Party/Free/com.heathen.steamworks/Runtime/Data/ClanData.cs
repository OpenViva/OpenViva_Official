#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Linq;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Steam Groups aka Steam Clans structure
    /// </summary>
    /// <remarks>
    /// This is interchangeable with CSteamID and simply extends it with Clan related accessor features
    /// <para>
    /// You can fetch the list of available clans via API.Clans.Client.GetClans()
    /// </para>
    /// </remarks>
    [Serializable]
    public struct ClanData : IEquatable<CSteamID>, IEquatable<ClanData>, IEquatable<ulong>, IComparable<CSteamID>, IComparable<ClanData>, IComparable<ulong>
    {
        [SerializeField]
        private ulong id;
        /// <summary>
        /// The native <see cref="Steamworks.CSteamID"/> representation of this Clan
        /// </summary>
        public readonly CSteamID SteamId
        {
            get => new CSteamID(id);
        }
        /// <summary>
        /// The <see cref="AccountID_t"/> portion of the native <see cref="CSteamID"/> representation of this Clan
        /// </summary>
        public readonly AccountID_t AccountId
        {
            get => SteamId.GetAccountID();
        }
        /// <summary>
        /// The primitive <see cref="uint"/> value of the <see cref="AccountId"/>
        /// </summary>
        public readonly uint FriendId
        {
            get => SteamId.GetAccountID().m_AccountID;
        }
        /// <summary>
        /// Is this Clan value a valid value.
        /// This does not indicate it is a clan simply that structurally the data is possibly a clan
        /// </summary>
        public readonly bool IsValid
        {
            get
            {
                var sID = SteamId;

                if (sID == CSteamID.Nil
                    || sID.GetEAccountType() != EAccountType.k_EAccountTypeClan
                    || sID.GetEUniverse() != EUniverse.k_EUniversePublic)
                    return false;
                else
                    return true;
            }
        }
        /// <summary>
        /// The icon of this clan if loaded
        /// </summary>
        public readonly Texture2D Icon => API.Friends.Client.GetLoadedAvatar(this);
        /// <summary>
        /// The name of this clan as seen by this user, this will account for language specific names if any
        /// </summary>
        public readonly string Name => API.Clans.Client.GetName(this);
        /// <summary>
        /// The tags for this clan if any
        /// </summary>
        public readonly string Tag => API.Clans.Client.GetTag(this);
        /// <summary>
        /// The owner of this clan
        /// </summary>
        public readonly UserData Owner => API.Clans.Client.GetOwner(this);
        /// <summary>
        /// Lists the officers of the clan
        /// </summary>
        public readonly UserData[] Officers => API.Clans.Client.GetOfficers(this);
        /// <summary>
        /// The number of members in chat at the moment
        /// </summary>
        public readonly int NumberOfMembersInChat => API.Clans.Client.GetChatMemberCount(this);
        /// <summary>
        /// The collection of members in chat at current
        /// </summary>
        public readonly UserData[] MembersInChat => API.Clans.Client.GetChatMembers(this);
        /// <summary>
        /// Is this clan an official game group
        /// </summary>
        public readonly bool IsOfficialGameGroup => API.Clans.Client.IsClanOfficialGameGroup(this);
        /// <summary>
        /// Is this clan a public group
        /// </summary>
        public readonly bool IsPublic => API.Clans.Client.IsClanPublic(this);
        /// <summary>
        /// Is the user the owner of this clan
        /// </summary>
        public readonly bool IsUserOwner => Owner == UserData.Me;
        /// <summary>
        /// Is the user an officer of this clan
        /// </summary>
        public readonly bool IsUserOfficer => Officers.Any(p => p == UserData.Me);
        /// <summary>
        /// Get all the clans this user is a member of
        /// </summary>
        /// <returns></returns>
        public static ClanData[] Get() => API.Clans.Client.GetClans();
        /// <summary>
        /// Get the clan represented by this account ID
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public static ClanData Get(uint accountId) => new CSteamID(new AccountID_t(accountId), EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeClan);
        /// <summary>
        /// Get the clan represented by this account ID
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public static ClanData Get(AccountID_t accountId) => new CSteamID(accountId, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeClan);
        /// <summary>
        /// Get the clan represented by this CSteamID value
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ClanData Get(ulong id) => new ClanData { id = id };
        /// <summary>
        /// Get the clan represented by this CSteamID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ClanData Get(CSteamID id) => new ClanData { id = id.m_SteamID };
        /// <summary>
        /// Allows the user to join Steam group (clan) chats right within the game.
        /// </summary>
        /// <remarks>
        /// The joins the "old" group chat used in Steam not the new channel based chat system introduced 2018-2019.
        /// It is still usable but will not reflect the chat seen in the Steam client. 
        /// It can be used as a global chat for in-game players as connecting via the Steam API is the only known way to connect to this legacy chat system.
        /// </remarks>
        /// <param name="callback"></param>
        public readonly void JoinChat(Action<ChatRoom, bool> callback) => API.Clans.Client.JoinChatRoom(id, callback);
        /// <summary>
        /// Request the system to locate and load the clan's icon
        /// </summary>
        /// <param name="callback"></param>
        public readonly void LoadIcon(Action<Texture2D> callback) => API.Friends.Client.GetFriendAvatar(SteamId, callback);

    #region Boilerplate
        public readonly int CompareTo(CSteamID other)
        {
            return id.CompareTo(other.m_SteamID);
        }

        public readonly int CompareTo(ClanData other)
        {
            return id.CompareTo(other.id);
        }

        public readonly int CompareTo(ulong other)
        {
            return id.CompareTo(other);
        }

        public readonly override string ToString()
        {
            return id.ToString();
        }

        public readonly bool Equals(CSteamID other)
        {
            return id.Equals(other);
        }

        public readonly bool Equals(ClanData other)
        {
            return id.Equals(other.id);
        }

        public readonly bool Equals(ulong other)
        {
            return id.Equals(other);
        }

        public readonly override bool Equals(object obj)
        {
            return id.Equals(obj);
        }

        public readonly override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public static bool operator ==(ClanData l, ClanData r) => l.id == r.id;
        public static bool operator !=(ClanData l, ClanData r) => l.id != r.id;
        public static bool operator ==(ClanData l, CSteamID r) => l.id == r.m_SteamID;
        public static bool operator !=(ClanData l, CSteamID r) => l.id != r.m_SteamID;
        public static bool operator ==(CSteamID l, ClanData r) => l.m_SteamID == r.id;
        public static bool operator !=(CSteamID l, ClanData r) => l.m_SteamID != r.id;
        public static bool operator <(ClanData l, ClanData r) => l.id < r.id;
        public static bool operator >(ClanData l, ClanData r) => l.id > r.id;
        public static bool operator <=(ClanData l, ClanData r) => l.id <= r.id;
        public static bool operator >=(ClanData l, ClanData r) => l.id >= r.id;
        public static implicit operator CSteamID(ClanData c) => c.SteamId;
        public static implicit operator ClanData(CSteamID id) => new ClanData { id = id.m_SteamID };
        public static implicit operator ulong(ClanData c) => c.id;
        public static implicit operator ClanData(ulong id) => new ClanData { id = id };
    #endregion
    }
}
#endif