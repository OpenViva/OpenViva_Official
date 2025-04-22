#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Displays the controller button reported for this specific action
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/input-action-name")]
    [RequireComponent(typeof(TMPro.TextMeshProUGUI))]
    public class InputActionName : MonoBehaviour
    {
        /// <summary>
        /// The Steam Input Action Set the action is a member of
        /// </summary>
        public InputActionSet set;
        /// <summary>
        /// The Steam Input Action Set Layer the action is a member of
        /// </summary>
        public InputActionSetLayer layer;
        /// <summary>
        /// The Steam Input Action to be displayed
        /// </summary>
        public InputAction action;

        private TMPro.TextMeshProUGUI label;

        private void Start()
        {
            label = GetComponent<TMPro.TextMeshProUGUI>();
            RefreshName();
        }

        private void OnEnable()
        {
            RefreshName();
        }
        /// <summary>
        /// Refresh the displayed button name
        /// </summary>
        public void RefreshName()
        {
            if (action != null && label != null)
            {
                if (set != null)
                {
                    var controllers = API.Input.Client.Controllers;
                    if (controllers.Length > 0)
                    {
                        var names = action.GetInputNames(controllers[0], set);
                        if (names.Length > 0)
                        {
                            label.text = names[0];
                        }
                    }
                }
                else if (layer != null)
                {
                    var controllers = API.Input.Client.Controllers;
                    if (controllers.Length > 0)
                    {
                        var names = action.GetInputNames(controllers[0], layer);
                        if (names.Length > 0)
                        {
                            label.text = names[0];
                        }
                    }
                }
            }
        }
    }
}
#endif