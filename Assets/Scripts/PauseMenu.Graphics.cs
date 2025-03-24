using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Viva
{
    [System.Serializable]
    public class Resolution
    {
        public int horizontal, vertical;
    }

    public partial class PauseMenu : UIMenu
    {
        [Header("Graphics")]
        [SerializeField]
        private Text reflectionDistanceText;
        [SerializeField]
        private GameObject FpsLimitContainer;
        [SerializeField]
        private Text resolutionText;
        [SerializeField]
        private Text lodDistanceText;
        public List<Resolution> resolutions = new List<Resolution>();
        private int selectedResolution;

        private void UpdateGraphicsUIText()
        {
            UpdateToggleQualityText();
            UpdateAntiAliasingText();
            UpdateShadowLevelText();
            //UpdateResolutionText();
            UpdateVsyncText();
            UpdateFpsLimitText();
            UpdateLodDistanceText();
        }

        // public void FindAutomaticResolution()
        // {
        //     bool foundRes = false;
        //     for(int i = 0; i < resolutions.Count; i++)
        //     {
        //         if(Screen.width == resolutions[i].horizontal && Screen.height == resolutions[i].vertical)
        //         {
        //             foundRes = true;
        //             selectedResolution = i;
        //             UpdateResolutionText();
        //         }
        //     }
        //     if (!foundRes)
        //     {
        //         Resolution resolution = new Resolution();
        //         resolution.horizontal = Screen.width;
        //         resolution.vertical = Screen.height;
        //
        //         resolutions.Add(resolution);
        //         selectedResolution = resolutions.Count - 1;
        //         UpdateResolutionText();
        //     }
        //     ApplyResolution();
        // }
        //
        // public void ApplyResolution()
        // {
        //     //Screen.SetResolution(resolutions[selectedResolution].horizontal, resolutions[selectedResolution].vertical, GameSettings.main.fullScreen);
        // }
        //
        // public void shiftResLeft()
        // {
        //     selectedResolution--;
        //     if(selectedResolution < 0)
        //     {
        //         selectedResolution = 0;
        //     }
        //     UpdateResolutionText();
        //     ApplyResolution();
        // }
        //
        // public void shiftResRight()
        // {
        //     selectedResolution++;
        //     if (selectedResolution > resolutions.Count - 1)
        //     {
        //         selectedResolution = resolutions.Count - 1;
        //     }
        //     UpdateResolutionText();
        //     ApplyResolution();
        // }

        public void clickShiftFpsLimit(int amount)
        {
            GameSettings.main.AdjustFpsLimit(amount);
            UpdateFpsLimitText();
        }

        public void clickShiftLodDistance(float amount)
        {
            GameSettings.main.AdjustLODDistance(amount);
            UpdateLodDistanceText();
        }

        public void clickCycleAntiAliasing()
        {
            GameSettings.main.CycleAntiAliasing();
            UpdateAntiAliasingText();
        }

        public void clickCycleShadowLevel()
        {
            GameSettings.main.CycleShadowSetting();
            UpdateShadowLevelText();
        }

        public void clickToggleQuality()
        {
            GameSettings.main.CycleQualitySetting();
            UpdateToggleQualityText();
        }

        public void clickToggleScreenMode()
        {
            GameSettings.main.ToggleFullScreen();
            //ApplyResolution();
            Screen.fullScreen = GameSettings.main.fullScreen;
            UpdateScreenModeText();
        }

        public void clickToggleVsync()
        {
            GameSettings.main.ToggleVsync();
            UpdateVsyncText();
        }

        public void ToggleFpsLimitContainer(bool enable)
        {
            Button[] buttons = FpsLimitContainer.GetComponentsInChildren<Button>();
            foreach(Button button in buttons)
            {
                button.interactable = enable;
            }
        }

        public void clickToggleClouds()
        {
            Text text = GetRightPageUIByMenu(Menu.GRAPHICS).transform.Find("Toggle Clouds").GetChild(0).GetComponent(typeof(Text)) as Text;
            GameSettings.main.toggleClouds = !GameSettings.main.toggleClouds;
            text.text = GameSettings.main.toggleClouds ? "Turn Off Clouds" : "Turn On Clouds";
            //GameDirector.instance.RebuildCloudRendering();
        }

        private void UpdateFpsLimitText()
        {
            Text text = FpsLimitContainer.transform.GetChild(3).GetComponent(typeof(Text)) as Text;
            text.text = "" + GameSettings.main.fpsLimit;          
        }
        
        private void UpdateSettingText(string settingPath, int settingValue, Dictionary<int, string> settingLabels)
        {
            Text text = GetRightPageUIByMenu(Menu.GRAPHICS).transform.Find(settingPath).GetChild(0).GetComponent<Text>();
    
            if (settingLabels.TryGetValue(settingValue, out var label))
            {
                text.text = label;
            }
            else
            {
                text.text = "Unknown"; 
                //handle invalid setting values
                Debug.LogError("[ERROR] Invalid value for setting: " + settingPath);
            }
        }

        private void UpdateAntiAliasingText()
        {
            var antiAliasingLabels = new Dictionary<int, string>
            {
                { 1, "None" },
                { 2, "2x" },
                { 4, "4x" },
                { 8, "8x" }
            };

            UpdateSettingText("Toggle Anti Aliasing", GameSettings.main.antiAliasing, antiAliasingLabels);
        }

        private void UpdateShadowLevelText()
        {
            var shadowLabels = new Dictionary<int, string>
            {
                { 0, "Disabled" },
                { 1, "Low" },
                { 2, "Medium" },
                { 3, "High" },
                { 4, "Very High" },
                { 5, "Ultra" }
            };

            UpdateSettingText("Toggle Shadows", GameSettings.main.shadowLevel, shadowLabels);
        }

        private void UpdateToggleQualityText()
        {
            var qualityLevels = QualitySettings.names;
            var qualityLabels = new Dictionary<int, string>();

            for (int i = 0; i < qualityLevels.Length; i++)
            {
                qualityLabels.Add(i, qualityLevels[i]);
            }

            UpdateSettingText("Toggle Quality", QualitySettings.GetQualityLevel(), qualityLabels);
        }


        // public void UpdateResolutionText()
        // {
        //     resolutionText.text = resolutions[selectedResolution].horizontal.ToString() + " x " + resolutions[selectedResolution].vertical.ToString();
        // }
        
        private void UpdateScreenModeText()
        {
            UpdateSettingText("FullScreen", GameSettings.main.fullScreen ? 1 : 0, new Dictionary<int, string>
            {
                { 0, "Windowed" },
                { 1, "FullScreen" }
            });
        }

        private void UpdateVsyncText()
        {
            UpdateSettingText("Vsync", GameSettings.main.vSync ? 1 : 0, new Dictionary<int, string>
            {
                { 0, "Disabled" },
                { 1, "Enabled" }
            });
        }


        private void UpdateLodDistanceText()
        {
            lodDistanceText.text = $"{(GameSettings.main.lodDistance * 100f):00}%";
        }

    }

}