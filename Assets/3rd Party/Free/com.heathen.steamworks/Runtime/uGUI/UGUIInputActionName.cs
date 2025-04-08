#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    public class UGUIInputActionName : MonoBehaviour
    {
        public InputActionSet set;
        public InputActionSetLayer layer;
        public InputAction action;

        private UnityEngine.UI.Text label;

        private void Start()
        {
            label = GetComponent<UnityEngine.UI.Text>();
            if (!API.App.Initialized)
                API.App.evtSteamInitialized.AddListener(HandleInitalization);
            else
                HandleInitalization();
        }

        private void HandleInitalization()
        {
            API.App.evtSteamInitialized.RemoveListener(HandleInitalization);

            RefreshName();
        }

        private void OnEnable()
        {
            RefreshName();
        }

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