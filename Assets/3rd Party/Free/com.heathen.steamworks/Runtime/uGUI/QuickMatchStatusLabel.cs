#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using TMPro;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
#endif

namespace Heathen.SteamworksIntegration.UI
{
    public class QuickMatchStatusLabel : MonoBehaviour
    {
        public QuickMatchLobbyControl controller;
        public TextMeshProUGUI label;

        [Header("Plural and Singular Token")]
        public string player = "Player";
        public string players = "Players";

        [Header("Messages")]
        public string idleMessage = "Click Play to Start";
        public string searchingMessage = "... Searching [ %timer% sec ] ...";
        public string waitingMessage = "... ( %count% / %max% ) Waiting for %remaining% %player% [ %timer% sec ] ...";
        public string startingMessage = "... Starting [ %timer% sec ] ...";

        private void Update()
        {
            switch(controller.WorkingStatus)
            {
                case QuickMatchLobbyControl.Status.Idle:
                    label.text = ParseString(idleMessage);
                    break;
                case QuickMatchLobbyControl.Status.Searching:
                    label.text = ParseString(searchingMessage);
                    break;
                case QuickMatchLobbyControl.Status.WaitingForStart:
                    label.text = ParseString(waitingMessage);
                    break;
                case QuickMatchLobbyControl.Status.Starting:
                    label.text = ParseString(startingMessage);
                    break;
            }
        }

        private string ParseString(string input)
        {
            return input.Replace("%player%", controller.Slots - controller.MemberCount > 1 ? players : player)
                .Replace("%timer%", ((int)controller.Timer).ToString())
                .Replace("%count%", controller.MemberCount.ToString())
                .Replace("%max%", controller.Slots.ToString())
                .Replace("%remaining%", (controller.Slots - controller.MemberCount).ToString());
        }
    }
}
#endif