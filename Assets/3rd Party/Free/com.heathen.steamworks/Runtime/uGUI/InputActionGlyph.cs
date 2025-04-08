#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Displays the controller button reported for this specific action
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/input-action-glyph")]
    [RequireComponent(typeof(UnityEngine.UI.RawImage))]
    public class InputActionGlyph : MonoBehaviour
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
        /// <summary>
        /// The image the icon will be displayed with
        /// </summary>
        private UnityEngine.UI.RawImage image;

        private void Start()
        {
            image = GetComponent<UnityEngine.UI.RawImage>();
            if (!API.App.Initialized)
                API.App.evtSteamInitialized.AddListener(HandleInitialization);
            else
                HandleInitialization();
        }

        private void HandleInitialization()
        {
            API.App.evtSteamInitialized.RemoveListener(HandleInitialization);

            RefreshImage();
        }

        private void OnEnable()
        {
            RefreshImage();
        }
        /// <summary>
        /// Refresh the image
        /// </summary>
        public void RefreshImage()
        {
            if (action != null && image != null)
            {
                if (set != null)
                {
                    var controllers = API.Input.Client.Controllers;
                    if (controllers.Length > 0)
                    {
                        var textures = action.GetInputGlyphs(controllers[0], set);
                        if (textures.Length > 0)
                        {
                            image.texture = textures[0];
                        }
                    }
                }
                else if (layer != null)
                {
                    var controllers = API.Input.Client.Controllers;
                    if (controllers.Length > 0)
                    {
                        var textures = action.GetInputGlyphs(controllers[0], layer);
                        if (textures.Length > 0)
                        {
                            image.texture = textures[0];
                        }
                    }
                }
            }
        }
    }
}
#endif