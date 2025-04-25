#if !DISABLESTEAMWORKS && STEAMWORKSNET
using Heathen.SteamworksIntegration;
using Heathen.SteamworksIntegration.API;
using Steamworks;
using TMPro;
using UnityEngine;

namespace Heathen.DEMO
{
    [RequireComponent(typeof(LobbyManager))]
    public class LobbyDemoButtonLogic : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField m_InputField;
        [SerializeField]
        private TextMeshProUGUI m_buttonLabel;
        private LobbyManager m_LobbyManager;

        void Start()
        {
            m_LobbyManager = GetComponent<LobbyManager>();
            m_LobbyManager.evtEnterFailed.AddListener(HandleEnterFail);
            m_LobbyManager.evtEnterSuccess.AddListener(HandleEnterSuccess);
            m_LobbyManager.evtCreateFailed.AddListener(HandleCreateFail);
            m_LobbyManager.evtCreated.AddListener(HandleCreated);
            m_LobbyManager.evtLeave.AddListener(HandleLeave);
            m_InputField.onValueChanged.AddListener(HandleInputChange);
        }

        private void OnDestroy()
        {
            if(m_LobbyManager != null)
            {
                m_LobbyManager.evtEnterFailed.RemoveListener(HandleEnterFail);
                m_LobbyManager.evtEnterSuccess.RemoveListener(HandleEnterSuccess);
                m_LobbyManager.evtCreateFailed.RemoveListener(HandleCreateFail);
                m_LobbyManager.evtCreated.RemoveListener(HandleCreated);
                m_LobbyManager.evtLeave.RemoveListener(HandleLeave);
                m_InputField.onValueChanged.RemoveListener(HandleInputChange);
            }
        }

        private void HandleInputChange(string arg0)
        {
            if (m_LobbyManager.HasLobby)
            {
                m_buttonLabel.text = "Leave";
                m_InputField.readOnly = true;
            }
            else
            {
                if (m_InputField.text.Length > 0)
                    m_buttonLabel.text = "Join";
                else
                    m_buttonLabel.text = "Create";

            }
        }

        private void HandleLeave()
        {
            m_InputField.SetTextWithoutNotify(string.Empty);
            m_buttonLabel.text = "Create";
            m_InputField.readOnly = false;
        }

        private void HandleCreated(LobbyData arg0)
        {
            m_InputField.SetTextWithoutNotify(arg0.ToString());
            m_buttonLabel.text = "Leave";
            m_InputField.readOnly = true;
        }

        private void HandleCreateFail(EResult arg0)
        {
            Debug.LogError("Failed to create lobby with result: " + arg0);
            m_InputField.readOnly = false;
        }

        private void HandleEnterSuccess(LobbyData arg0)
        {
            m_InputField.SetTextWithoutNotify(arg0.ToString());
            m_buttonLabel.text = "Leave";
            m_InputField.readOnly = true;
        }

        private void HandleEnterFail(EChatRoomEnterResponse arg0)
        {
            Debug.LogError("Failed to enter lobby with result: " + arg0);
            m_InputField.readOnly = false;
        }

        public void OnJoinCreateClick()
        {
            if (m_LobbyManager.HasLobby)
                m_LobbyManager.Leave();
            else if (m_InputField.text.Length > 0)
            {
                var lobby = LobbyData.Get(m_InputField.text);
                if (lobby.IsValid)
                    m_LobbyManager.Join(lobby);
                else
                    Debug.LogWarning("Invalid Lobby ID was provided, no action taken.");
            }
            else
            {
                m_LobbyManager.Create();
            }
        }

        public void OnInviteFriendClick()
        {
            if (m_LobbyManager.HasLobby)
            {
                Overlay.Client.ActivateInviteDialog(m_LobbyManager.Lobby);
            }
            else
            {
                Debug.LogWarning("Invalid Lobby, no action taken.");
            }
        }
    }
}
#endif