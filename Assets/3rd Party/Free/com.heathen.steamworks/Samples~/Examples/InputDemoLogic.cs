#if !DISABLESTEAMWORKS && STEAMWORKSNET
using Heathen.SteamworksIntegration;
using System.Collections.Generic;
using UnityEngine;

namespace Heathen.DEMO
{
    public class InputDemoLogic : MonoBehaviour
    {
        [Header("UI References")]
        public TMPro.TextMeshProUGUI label;

        [Header("Input Action Sets")]
        public InputActionSet menuActionSet;
        public InputActionSet shipActionSet;

        [Header("Input Action Set Layers")]
        public InputActionSetLayer thrustLayer;

        [Header("Input Actions")]
        public InputAction analogAction;
        public InputAction leftAction;
        public InputAction rightAction;
        public InputAction forwardAction;
        public InputAction backwardAction;
        public InputAction fireAction;
        public InputAction pauseAction;
        public InputAction menuUpAction;
        public InputAction menuDownAction;
        public InputAction menuLeftAction;
        public InputAction menuRightAction;
        public InputAction selectAction;
        public InputAction cancelAction;

        private InputActionData analogData;
        private InputActionData leftData;
        private InputActionData rightData;
        private InputActionData forwardData;
        private InputActionData backwardData;
        private InputActionData fireData;
        private InputActionData pauseData;
        private InputActionData menuUpData;
        private InputActionData menuDownData;
        private InputActionData menuLeftData;
        private InputActionData menuRightData;
        private InputActionData selectData;
        private InputActionData cancelData;

        [Header("Glyph Data")]
        public List<InputActionGlyph> glyphs = new List<InputActionGlyph>();
        public List<UGUIInputActionName> names = new List<UGUIInputActionName>();

        private bool hackRefresh = false;

        private void Update()
        {
            if (SteamInputManager.Controllers != null && SteamInputManager.Controllers.Count > 0)
            {
                if (!hackRefresh)
                {
                    hackRefresh = true;
                    Invoke(nameof(DelayActivate), 1);
                }
                analogData = analogAction.GetActionData();

                leftData = leftAction.GetActionData();
                rightData = rightAction.GetActionData();
                forwardData = forwardAction.GetActionData();
                backwardData = backwardAction.GetActionData();

                fireData = fireAction.GetActionData();
                pauseData = pauseAction.GetActionData();

                menuUpData = menuUpAction.GetActionData();
                menuDownData = menuDownAction.GetActionData();
                menuLeftData = menuLeftAction.GetActionData();
                menuRightData = menuRightAction.GetActionData();
                selectData = selectAction.GetActionData();
                cancelData = cancelAction.GetActionData();

                label.text = "Analog Action: " + analogData.ToString() + "\nLeft Action: " + leftData.ToString() + "\nRight Action: " + rightData.ToString() + "\nForward Action: " + forwardData.ToString() + "\nBackward Action: " + backwardData.ToString() + "\nFire Action: " + fireData.ToString() + "\nPause Action: " + pauseData.ToString() + "\nMenu Up Action: " + menuUpData.ToString() + "\nMenu Down Action: " + menuDownData.ToString() + "\nMenu Right Action: " + menuRightData.ToString() + "\nMenu Left Action: " + menuLeftData.ToString() + "\nMenu Select Action: " + selectData.ToString() + "\nCancel Action: " + cancelData.ToString();
            }
            else
                label.text = "No Controllers found";
        }

        private void DelayActivate()
        {
            shipActionSet.Activate(SteamInputManager.Controllers[0].handle);
            thrustLayer.Activate(SteamInputManager.Controllers[0].handle);

            //Because we have to force the App ID in Unity Editor we need to force a refresh after that
            foreach (var glyph in glyphs)
                glyph.RefreshImage();
            foreach (var iName in names)
                iName.RefreshName();
        }

        public void ActivateMenuControls()
        {
            menuActionSet.Activate();
        }

        public void ActivateShipControls()
        {
            shipActionSet.Activate();
            thrustLayer.Activate();
        }

        public void OpenKnowledgeBaseUserData()
        {
            Application.OpenURL("https://kb.heathen.group/steam/input");
        }
    }
}
#endif