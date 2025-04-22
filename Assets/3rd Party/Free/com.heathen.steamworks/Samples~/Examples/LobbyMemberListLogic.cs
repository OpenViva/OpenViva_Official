#if !DISABLESTEAMWORKS && STEAMWORKSNET
using Heathen.SteamworksIntegration;
using Heathen.SteamworksIntegration.UI;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace Heathen.DEMO
{
    [RequireComponent(typeof(LobbyManager))]
    public class LobbyMemberListLogic : MonoBehaviour
    {
        private LobbyManager lobbyManager;
        [SerializeField]
        private GameObject memberPrefab;
        [SerializeField]
        private Transform contentRoot;

        private Dictionary<UserData, GameObject> memberToUiMapping = new();

        private void Start()
        {
            lobbyManager = GetComponent<LobbyManager>();
            lobbyManager.evtEnterSuccess.AddListener(HandleLobbyEnter);
            lobbyManager.evtEnterFailed.AddListener(HandleFailedToEnter);
            lobbyManager.evtUserJoined.AddListener(HandleUserJoined);
            lobbyManager.evtUserLeft.AddListener(HandleUserLeft);
        }

        private void OnDestroy()
        {
            lobbyManager.evtEnterSuccess.RemoveListener(HandleLobbyEnter);
            lobbyManager.evtEnterFailed.RemoveListener(HandleFailedToEnter);
            lobbyManager.evtUserJoined.RemoveListener(HandleUserJoined);
            lobbyManager.evtUserLeft.RemoveListener(HandleUserLeft);
        }

        private void HandleLobbyEnter(LobbyData lobby)
        {
            //We entered someone else's lobby, list all the members (except us)
            foreach (var member in lobby.Members)
            {
                //Don't bother listing our selves, we already know us
                if (!member.user.IsMe)
                {
                    var go = Instantiate(memberPrefab, contentRoot);
                    if (memberToUiMapping.TryAdd(member.user, go))
                    {
                        var comp = go.GetComponent<FriendProfile>();
                        comp.UserData = member.user;
                    }
                    else
                    {
                        Destroy(go);
                        Debug.LogWarning($"Attempted to add a UI element for member {member.user.Name}, the attempted failed suggesting the user's already listed in the UI.");
                    }
                }
            }
        }

        private void HandleFailedToEnter(EChatRoomEnterResponse response)
        {
            //Should never happen but handle it if it does
            Debug.LogError($"Failed to enter the requested lobby: {response}");
        }

        private void HandleUserJoined(UserData user)
        {
            //A new member joined a lobby we are in ... give them a UI element
            var go = Instantiate(memberPrefab, contentRoot);
            if (memberToUiMapping.TryAdd(user, go))
            {
                var comp = go.GetComponent<FriendProfile>();
                comp.UserData = user;
            }
            else
            {
                Destroy(go);
                Debug.LogWarning($"Attempted to add a UI element for member {user.Name}, the attempted failed suggesting the user's already listed in the UI.");
            }
        }

        private void HandleUserLeft(UserLobbyLeaveData data)
        {
            //A member left a lobby we are in, remove the UI element
            if (memberToUiMapping.TryGetValue(data.user, out var uiObject))
                Destroy(uiObject);
        }
    }
}
#endif