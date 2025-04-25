#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// This API can be used to selectively advertise your multiplayer game session in a Steam chat room group. Tell Steam the number of player spots that are available for your party, and a join-game string, and it will show a beacon in the selected group and allow that many users to “follow” the beacon to your party. Adjust the number of open slots if other players join through alternate matchmaking methods.
    /// </summary>
    public static class Parties
    {
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                eventReservationNotificationCallback = new ReservationNotificationCallbackEvent();
                eventActiveBeaconsUpdated = new ActiveBeaconsUpdatedEvent();
                eventAvailableBeaconLocationsUpdated = new AvailableBeaconLocationsUpdatedEvent();
                m_CreateBeaconCallback_t = null;
                m_ChangeNumOpenSlotsCallback_t = null;
                m_JoinPartyCallback_t = null;
                m_ReservationNotificationCallback_t = null;
                m_AvailableBeaconLocationsUpdated_t = null;
                m_ActiveBeaconsUpdated_t = null;
                reservations = null;
                createdBeacons = null;
            }

            /// <summary>
            /// An array of beacons created by this user in this session.
            /// </summary>
            public static PartyBeaconID_t[] MyBeacons => createdBeacons?.ToArray();

            /// <summary>
            /// The array of reservations you have been notified of but have not yet completed.
            /// </summary>
            public static ReservationNotificationCallback_t[] Reservations => reservations?.ToArray();

            /// <summary>
            /// Invoked when a user joins your party, call OnReservationCompleted to notify Steam that the user has joined successfully.
            /// </summary>
            public static ReservationNotificationCallbackEvent EventReservationNotificationCallback
            {
                get
                {
                    if (m_ReservationNotificationCallback_t == null)
                        m_ReservationNotificationCallback_t = Callback<ReservationNotificationCallback_t>.Create(ReservationCallback);

                    return eventReservationNotificationCallback;
                }
            }
            /// <summary>
            /// Notification that the list of active beacons visible to the current user has changed.
            /// </summary>
            public static ActiveBeaconsUpdatedEvent EventActiveBeaconsUpdated
            {
                get
                {
                    if (m_ActiveBeaconsUpdated_t == null)
                        m_ActiveBeaconsUpdated_t = Callback<ActiveBeaconsUpdated_t>.Create(eventActiveBeaconsUpdated.Invoke);

                    return eventActiveBeaconsUpdated;
                }
            }
            /// <summary>
            /// Notification that the list of available locations for posting a beacon has been updated.
            /// </summary>
            public static AvailableBeaconLocationsUpdatedEvent EventAvailableBeaconLocationsUpdated
            {
                get
                {
                    if (m_AvailableBeaconLocationsUpdated_t == null)
                        m_AvailableBeaconLocationsUpdated_t = Callback<AvailableBeaconLocationsUpdated_t>.Create(eventAvailableBeaconLocationsUpdated.Invoke);

                    return eventAvailableBeaconLocationsUpdated;
                }
            }

            private static ReservationNotificationCallbackEvent eventReservationNotificationCallback = new ReservationNotificationCallbackEvent();
            private static ActiveBeaconsUpdatedEvent eventActiveBeaconsUpdated = new ActiveBeaconsUpdatedEvent();
            private static AvailableBeaconLocationsUpdatedEvent eventAvailableBeaconLocationsUpdated = new AvailableBeaconLocationsUpdatedEvent();

            private static CallResult<CreateBeaconCallback_t> m_CreateBeaconCallback_t;
            private static CallResult<ChangeNumOpenSlotsCallback_t> m_ChangeNumOpenSlotsCallback_t;
            private static CallResult<JoinPartyCallback_t> m_JoinPartyCallback_t;

            private static Callback<ReservationNotificationCallback_t> m_ReservationNotificationCallback_t;
            private static Callback<ActiveBeaconsUpdated_t> m_ActiveBeaconsUpdated_t;
            private static Callback<AvailableBeaconLocationsUpdated_t> m_AvailableBeaconLocationsUpdated_t;

            private static List<ReservationNotificationCallback_t> reservations;
            private static List<PartyBeaconID_t> createdBeacons;

            /// <summary>
            /// Get the list of locations in which you can post a party beacon.
            /// </summary>
            /// <returns></returns>
            public static SteamPartyBeaconLocation_t[] GetAvailableBeaconLocations()
            {
                SteamParties.GetNumAvailableBeaconLocations(out uint locations);
                var output = new SteamPartyBeaconLocation_t[locations];
                SteamParties.GetAvailableBeaconLocations(output, locations);
                return output;
            }
            /// <summary>
            /// Create a beacon. You can only create one beacon at a time. Steam will display the beacon in the specified location, and let up to unOpenSlots users "follow" the beacon to your party.
            /// </summary>
            /// <param name="openSlots">Number of reservation slots to create for the beacon. Normally, this is the size of your desired party minus one (for the current user).</param>
            /// <param name="location">Location information for the beacon. Should be one of the locations returned by ISteamParties::GetAvailableBeaconLocations.</param>
            /// <param name="connectionString">Connect string that will be given to the game on launch for a user that follows the beacon.</param>
            /// <param name="metadata">Additional game metadata that can be set on the beacon, and is exposed via ISteamParties::GetBeaconDetails.</param>
            /// <param name="callback"></param>
            public static void CreateBeacon(uint openSlots, ref SteamPartyBeaconLocation_t location, string connectionString, string metadata, Action<CreateBeaconCallback_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_CreateBeaconCallback_t == null)
                    m_CreateBeaconCallback_t = CallResult<CreateBeaconCallback_t>.Create();

                var handle = SteamParties.CreateBeacon(openSlots, ref location, connectionString, metadata);
                m_CreateBeaconCallback_t.Set(handle, (r,e) =>
                {
                    if (!e && r.m_eResult == EResult.k_EResultOK)
                    {
                        if (createdBeacons == null)
                            createdBeacons = new List<PartyBeaconID_t>();

                        createdBeacons.Add(r.m_ulBeaconID);
                    }

                    callback.Invoke(r, e);
                });
            }
            /// <summary>
            /// When a user follows your beacon, Steam will reserve one of the open party slots for them, and send your game a ReservationNotificationCallback_t callback. When that user joins your party, call OnReservationCompleted to notify Steam that the user has joined successfully.
            /// </summary>
            /// <param name="beacon"></param>
            /// <param name="user"></param>
            public static void OnReservationCompleted(PartyBeaconID_t beacon, CSteamID user)
            {
                SteamParties.OnReservationCompleted(beacon, user);

                if (reservations != null)
                    reservations.RemoveAll((p) => p.m_ulBeaconID == beacon && p.m_steamIDJoiner == user);
            }
            public static bool OnReservationCompleted(UserData user)
            {
                if (reservations.Any(p => p.m_steamIDJoiner == user))
                {
                    var beacon = reservations.FirstOrDefault(p => p.m_steamIDJoiner == user);
                    OnReservationCompleted(beacon.m_ulBeaconID, user);

                    return true;
                }
                else
                    return false;
            }
            /// <summary>
            /// If a user joins your party through other matchmaking (perhaps a direct Steam friend, or your own matchmaking system), your game should reduce the number of open slots that Steam is managing through the party beacon. For example, if you created a beacon with five slots, and Steam sent you two ReservationNotificationCallback_t callbacks, and then a third user joined directly, you would want to call ChangeNumOpenSlots with a value of 2 for unOpenSlots. That value represents the total number of new users that you would like Steam to send to your party.
            /// </summary>
            /// <param name="beacon"></param>
            /// <param name="openSlots"></param>
            public static void ChangeNumOpenSlots(PartyBeaconID_t beacon, uint openSlots, Action<ChangeNumOpenSlotsCallback_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_ChangeNumOpenSlotsCallback_t == null)
                    m_ChangeNumOpenSlotsCallback_t = CallResult<ChangeNumOpenSlotsCallback_t>.Create();

                var handle = SteamParties.ChangeNumOpenSlots(beacon, openSlots);
                m_ChangeNumOpenSlotsCallback_t.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Call this method to destroy the Steam party beacon. This will immediately cause Steam to stop showing the beacon in the target location. Note that any users currently in-flight may still arrive at your party expecting to join.
            /// </summary>
            /// <param name="beacon"></param>
            /// <returns></returns>
            public static bool DestroyBeacon(PartyBeaconID_t beacon)
            {
                if (createdBeacons != null)
                    createdBeacons.RemoveAll((p) => p == beacon);

                return SteamParties.DestroyBeacon(beacon);
            }
            /// <summary>
            /// Get the collection of active beacons visible to the current user.
            /// </summary>
            /// <returns></returns>
            public static PartyBeaconID_t[] GetBeacons()
            {
                var count = SteamParties.GetNumActiveBeacons();
                var results = new PartyBeaconID_t[count];
                for (uint i = 0; i < count; i++)
                {
                    results[i] = SteamParties.GetBeaconByIndex(i);
                }

                return results;
            }
            /// <summary>
            /// Get details about the specified beacon.
            /// </summary>
            /// <param name="beacon"></param>
            /// <returns></returns>
            public static PartyBeaconDetails? GetBeaconDetails(PartyBeaconID_t beacon)
            {
                if (SteamParties.GetBeaconDetails(beacon, out CSteamID owner, out SteamPartyBeaconLocation_t location, out string metadata, 8193))
                {
                    return new PartyBeaconDetails
                    {
                        id = beacon,
                        owner = owner,
                        location = location,
                        metadata = metadata
                    };
                }
                else
                    return null;
            }
            /// <summary>
            /// When the user indicates they wish to join the party advertised by a given beacon, call this method. On success, Steam will reserve a slot for this user in the party and return the necessary "join game" string to use to complete the connection.
            /// </summary>
            /// <param name="beacon"></param>
            /// <param name="callback"></param>
            public static void JoinParty(PartyBeaconID_t beacon, Action<JoinPartyCallback_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_JoinPartyCallback_t == null)
                    m_JoinPartyCallback_t = CallResult<JoinPartyCallback_t>.Create();

                var handle = SteamParties.JoinParty(beacon);
                m_JoinPartyCallback_t.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Query general metadata for the given beacon location. For instance the Name, or the URL for an icon if the location type supports icons (for example, the icon for a Steam Chat Room Group).
            /// </summary>
            /// <param name="location">Location to query.</param>
            /// <param name="data">Type of location data you wish to get.</param>
            /// <param name="result">Output buffer for location data string. Will be NULL-terminated on success.</param>
            /// <returns></returns>
            public static bool GetBeaconLocationData(SteamPartyBeaconLocation_t location, ESteamPartyBeaconLocationData data, out string result) => SteamParties.GetBeaconLocationData(location, data, out result, 8193);

            private static void ReservationCallback(ReservationNotificationCallback_t arg)
            {
                if (reservations == null)
                    reservations = new List<ReservationNotificationCallback_t>();

                reservations.Add(arg);
                eventReservationNotificationCallback.Invoke(arg);
            }
        }
    }
}
#endif