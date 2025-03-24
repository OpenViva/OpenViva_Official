using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Viva.Util;
using ShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution;

namespace Viva
{

    public delegate void VolumeCallback(float volume);
    
    //TODO: Cleanup all this cause its kinda messy
    [System.Serializable]
    public class GameSettings
    {
        public static GameSettings main { get; private set; } = new GameSettings();

        public float mouseSensitivity = 240.0f;
        public float masterVolume = 1f;
        public float musicVolume = 0.5f;
        public float sfxVolume = 1f;
        public float voiceVolume = 0.5f;
        public int dayNightCycleSpeedIndex = 2;
        public bool disableGrabToggle = true;
        public bool pressToTurn = false;
        public Player.VRControlType vrControls = Player.VRControlType.TRACKPAD;
        public bool trackpadMovementUseRight = false;
        //Save current Calibration Position/Rotation
        public Vector3 CalibratePosition = new Vector3(0.003261785f, 0.086780190f, 0.05201015f);
        public Vector3 CalibrateEuler = new Vector3(-350.4226f, -101.9745f, -152.1913f);
        public int qualityLevel = 3;
        public int shadowLevel = 4;
        public int antiAliasing = 2;
        public float lodDistance = 1.0f;
        public int fpsLimit = 90;
        public bool fullScreen = false;
        public bool vSync = false;       
        public bool toggleTooltips = true;
        public bool toggleClouds = false;

        private string[] dayNightCycleSpeedDesc = new string[]
        { 
            "Never Change",
            "5 minutes",
            "12 minutes",
            "24 minutes",
            "45 minutes",
            "1 hour",
            "2 hour"
        };

        public void Apply()
        {
            QualitySettings.SetQualityLevel(qualityLevel, false);
            // QualitySettings.antiAliasing = antiAliasing;
            QualitySettings.vSyncCount = vSync ? 1 : 0;     
            QualitySettings.lodBias = lodDistance;
            Application.targetFrameRate = vSync ? -1 : fpsLimit;
            Screen.fullScreen = fullScreen;
            GameDirector.player.pauseMenu.ToggleFpsLimitContainer(!vSync);
            
            
            ApplyURPSpecificSettings();
        }
        
        public void Copy(GameSettings copy)
        {
            if (copy == null) return;
            mouseSensitivity = copy.mouseSensitivity;
            masterVolume = copy.masterVolume;
            musicVolume = copy.musicVolume;
            sfxVolume = copy.sfxVolume;
            voiceVolume = copy.voiceVolume;
            dayNightCycleSpeedIndex = copy.dayNightCycleSpeedIndex;
            disableGrabToggle = copy.disableGrabToggle;
            pressToTurn = copy.pressToTurn;
            vrControls = copy.vrControls;
            trackpadMovementUseRight = copy.trackpadMovementUseRight;
            CalibratePosition = copy.CalibratePosition;
            CalibrateEuler = copy.CalibrateEuler;
            qualityLevel = copy.qualityLevel;
            shadowLevel = copy.shadowLevel;
            antiAliasing = copy.antiAliasing;

            lodDistance = copy.lodDistance;
            fpsLimit = copy.fpsLimit;
            vSync = copy.vSync;
            fullScreen = copy.fullScreen;
            toggleTooltips = copy.toggleTooltips;
            toggleClouds = copy.toggleClouds;
        }
        
        public void ApplyVolumeSetting(float volume, string audioMixerValue, VolumeCallback setVolumeAction = null)
        {
            var valueToSet = Mathf.Log10(volume) * 21.0f; 
            Debug.Log(valueToSet);
            valueToSet = Mathf.Clamp(valueToSet, -80f, 20);
            setVolumeAction?.Invoke(volume);
            GameDirector.instance.audioMixer.SetFloat(audioMixerValue, valueToSet);
        }

        public void SetAllAudioMixers()
        {
            ApplyVolumeSetting(masterVolume, "MasterVolume");
            ApplyVolumeSetting(musicVolume, "MusicVolume");
            ApplyVolumeSetting(voiceVolume, "VoiceVolume");
            ApplyVolumeSetting(sfxVolume, "SfxVolume");
        }

        public void ApplyURPSpecificSettings()
        {
            var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset == null)
            {
                Debug.LogError("URP Asset not found");
                return;
            }

            urpAsset.msaaSampleCount = antiAliasing;
            
            switch (shadowLevel)
            {
                default:
                    urpAsset.shadowCascadeCount = 1;
                    urpAsset.shadowDistance = 0f;
                    UnityGraphicsBullshit.MainLightShadowResolution = ShadowResolution._256;
                    UnityGraphicsBullshit.AdditionalLightShadowResolution = ShadowResolution._256;
                    break;
                case 1:
                    urpAsset.shadowDistance = 50f;
                    break;
                case 2:
                    urpAsset.shadowDistance = 75f;
                    urpAsset.shadowCascadeCount = 2;
                    UnityGraphicsBullshit.MainLightShadowResolution = ShadowResolution._512;
                    UnityGraphicsBullshit.AdditionalLightShadowResolution = ShadowResolution._512;
                    break;
                case 3:
                    urpAsset.shadowDistance = 100f;
                    urpAsset.shadowCascadeCount = 3;
                    UnityGraphicsBullshit.MainLightShadowResolution = ShadowResolution._1024;
                    UnityGraphicsBullshit.AdditionalLightShadowResolution = ShadowResolution._1024;
                    break;
                case 4:
                    urpAsset.shadowDistance = 150f;
                    UnityGraphicsBullshit.MainLightShadowResolution = ShadowResolution._2048;
                    UnityGraphicsBullshit.AdditionalLightShadowResolution = ShadowResolution._2048;
                    break;
                case 5:
                    urpAsset.shadowDistance = 200f;
                    urpAsset.shadowCascadeCount = 4;
                    UnityGraphicsBullshit.MainLightShadowResolution = ShadowResolution._4096;
                    UnityGraphicsBullshit.AdditionalLightShadowResolution = ShadowResolution._4096;
                    break;
            }
        }

        public void AdjustMouseSensitivity(float direction)
        {
            SetMouseSensitivity(mouseSensitivity + direction);
        }
        public void SetMouseSensitivity(float amount)
        {
            mouseSensitivity = Mathf.Clamp(amount, 10.0f, 250.0f);
        }
        public void AdjustFpsLimit(int direction)
        {
            SetFpsLimit(fpsLimit + direction);
            Apply();
        }
        public void SetFpsLimit(int amount)
        {
            fpsLimit = Mathf.Clamp(amount, 30, 250);
        }
        public void AdjustLODDistance(float direction)
        {
            SetLODDistance(lodDistance + direction);
            Apply();
        }
        public void SetLODDistance(float amount)
        {
            lodDistance = Mathf.Clamp(amount, 0.1f, 2.0f);
        }
        public void ShiftWorldTime(float timeAmount)
        {
            GameDirector.newSkyDirector.skyDefinition.timeSystem += timeAmount;
        }
        public void SetWorldTime(float newTime)
        {
            GameDirector.newSkyDirector.skyDefinition.timeSystem = newTime;
        }
        public string AdjustDayTimeSpeedIndex(int direction)
        {
            SetDayNightCycleSpeedIndex(dayNightCycleSpeedIndex + direction);
            GameDirector.newSkyDirector.skyDefinition.ApplyDaySpeed();
            return dayNightCycleSpeedDesc[dayNightCycleSpeedIndex];
        }
        public void SetDayNightCycleSpeedIndex(int index)
        {
            dayNightCycleSpeedIndex = Mathf.Clamp(index, 0, dayNightCycleSpeedDesc.Length - 1);
        }
        public void ToggleDisableGrabToggle()
        {
            disableGrabToggle = !disableGrabToggle;
        }
        public void TogglePresstoTurn()
        {
            pressToTurn = !pressToTurn;
        }
        public void ToggleFullScreen()
        {
            fullScreen = !fullScreen;
        }
        public void ToggleVsync()
        {
            vSync = !vSync;
            Apply();
        }
        public void SetVRControls(Player.VRControlType newVRControls)
        {
            vrControls = newVRControls;
        }
        public void ToggleTrackpadMovementUseRight()
        {
            trackpadMovementUseRight = !trackpadMovementUseRight;
        }
        public void CycleAntiAliasing()
        {
            switch (antiAliasing)
            {
                case 1:
                    antiAliasing = 2;
                    break;
                case 2:
                    antiAliasing = 4;
                    break;
                case 4:
                    antiAliasing = 8;
                    break;
                default:
                    antiAliasing = 1;
                    break;
            }
            Apply();
        }

        public void CycleQualitySetting()
        {
            qualityLevel = (int)QualitySettings.GetQualityLevel();
            qualityLevel = (qualityLevel + 1) % 5;
            Apply();
        }

        public void CycleShadowSetting()
        {
            shadowLevel = (shadowLevel + 1) % 6;
            Apply();
        }

        public void SetDefaultCalibration()
        {
            if (GameDirector.player)
            {
                GameDirector.player.rightPlayerHandState.SetAbsoluteVROffsets(CalibratePosition, CalibrateEuler, true);
                GameDirector.player.leftPlayerHandState.SetAbsoluteVROffsets(CalibratePosition, CalibrateEuler, true);
            }
        }
    }

    public partial class GameDirector : MonoBehaviour
    {

        [Header("Persistence")]
        [SerializeField]
        private GameObject[] itemPrefabManifest;
        public FileLoadStatus fileLoadStatus;


        public GameObject FindItemPrefabByName(string name)
        {
            foreach (var item in itemPrefabManifest)
            {
                if (item.name == name)
                {
                    return item;
                }
            }
            Debug.LogError("Could not find item in manifest: " + name);
            return null;
        }

        [System.Serializable]
        public class TransformSave
        {
            public Vector3 position;
            public Quaternion rotation;

            public TransformSave(Transform target)
            {
                position = target.position;
                rotation = target.rotation;
            }
            public TransformSave()
            {
                position = Vector3.zero;
                rotation = Quaternion.identity;
            }
        }

        [System.Serializable]
        public class VivaFile
        {

            public string languageName;

            [System.Serializable]
            public class SerializedCompanion
            {

                //defines assigned active task session data
                [System.Serializable]
                public class SerializedTaskData
                {
                    public int taskIndex;
                    public List<SerializedVivaProperty> properties;

                    public SerializedTaskData(ActiveBehaviors.ActiveTask task)
                    {
                        taskIndex = (int)task.type;
                        if (task.session != null)
                        {
                            properties = SerializedVivaProperty.Serialize(task.session);
                        }
                    }
                    public SerializedTaskData(ActiveBehaviors.Behavior taskType)
                    {
                        taskIndex = (int)taskType;
                    }
                }

                public string sourceCardFilename;
                public SerializedAsset propertiesAsset;
                public SerializedTaskData activeTaskSession;
                public int serviceIndex = -1;


                public SerializedCompanion(string _sourceCardFilename, SerializedAsset _propertiesAsset)
                {
                    sourceCardFilename = _sourceCardFilename;
                    propertiesAsset = _propertiesAsset;
                    activeTaskSession = new SerializedTaskData(ActiveBehaviors.Behavior.IDLE);
                }
            }

            //defines a prefab gameObject and a single component
            [System.Serializable]
            public class SerializedAsset
            {
                public bool targetsSceneAsset;
                public string assetName;
                public string sessionReferenceName;
                public List<SerializedVivaProperty> properties;
                public TransformSave transform;
                public string uniqueID;

                public SerializedAsset(VivaSessionAsset target)
                {
                    targetsSceneAsset = target.targetsSceneAsset;
                    assetName = target.assetName;
                    transform = new TransformSave(target.transform);
                    sessionReferenceName = target.sessionReferenceName;
                    uniqueID = target.uniqueID;
                    
                    properties = SerializedVivaProperty.Serialize(target);
                }

                public SerializedAsset(string _sessionReferenceName)
                {
                    targetsSceneAsset = false;
                    assetName = "";
                    transform = new TransformSave();
                    sessionReferenceName = _sessionReferenceName;
                    uniqueID = Guid.NewGuid().ToString();
                }
            }

            public List<SerializedAsset> serializedAssets = new List<SerializedAsset>();
            [FormerlySerializedAs("loliAssets")] public List<SerializedCompanion> companionAssets = new List<SerializedCompanion>();
        }

        public void Save()
        {
            VivaFile vivaFile = new VivaFile();
            List<GameObject> rootObjects = new List<GameObject>(SceneManager.GetActiveScene().rootCount);
            SceneManager.GetActiveScene().GetRootGameObjects(rootObjects);
            List<VivaSessionAsset> assets = new List<VivaSessionAsset>();
            foreach (GameObject rootObj in rootObjects)
            {
                assets.AddRange(rootObj.GetComponentsInChildren<VivaSessionAsset>(true));
            }

            vivaFile.languageName = languageName;
            foreach (VivaSessionAsset asset in assets)
            {
                if (asset.IgnorePersistance())
                {
                    continue;
                }
                asset.Save(vivaFile);
            }
            Steganography.EnsureFolderExistence("Saves");
            //combine and save byte buffers
            string json = JsonUtility.ToJson(vivaFile, true);
            using (var stream = new FileStream("Saves/save.viva", FileMode.Create))
            {

                byte[] data = Tools.UTF8ToByteArray(json);
                stream.Write(data, 0, data.Length);
                stream.Close();
            }
            Debug.Log("[PERSISTANCE] Saved File!");
        }

        protected void AttemptLoadVivaFile()
        {

            string path = "Saves/save.viva";
            VivaFile file = null;
            if (File.Exists(path))
            {
                string data = File.ReadAllText(path);
                file = JsonUtility.FromJson(data, typeof(VivaFile)) as VivaFile;
                if (file == null)
                {
                    Debug.LogError("[PERSISTANCE] ERROR Could not load VivaFile!");
                }
                else
                {
                    languageName = file.languageName;
                    if (languageName == null)
                    {
                        languageName = "english";
                    }
                }
            }
            StartCoroutine(LoadVivaFile(file));
        }

        private IEnumerator LoadVivaFile(VivaFile file)
        {
            int oldMask = mainCamera.cullingMask;
            var cardsAvailable = ModelCustomizer.main.characterCardBrowser.FindAllExistingCardsInFolders();
            mainCamera.cullingMask = WorldUtil.uiMask;
            fileLoadStatus.gameObject.SetActive(true);

            if (file == null)
            {
                //defaults if no file present
                GameSettings.main.SetWorldTime(GameDirector.newSkyDirector.skyDefinition.initialTime);
                StartCoroutine(FirstLoadTutorial());

                yield return null;
            }
            else
            {
                afterFirstLoadHints.SetActive(true);

                // Load Companions
                var cdm = new CoroutineDeserializeManager();

                var toLoad = new List<Util.Tuple<Companion, VivaFile.SerializedCompanion>>();
                foreach (var serializedCompanion in file.companionAssets)
                {
                    var targetCompanion = GameDirector.instance.GetCompanionFromPool();
                    cdm.waiting++;
                    StartCoroutine(Companion.LoadCompanionFromSerializedCompanion(serializedCompanion.sourceCardFilename, targetCompanion, delegate
                    {
                        toLoad.Add(new Util.Tuple<Companion, VivaFile.SerializedCompanion>(targetCompanion, serializedCompanion));
                        cdm.waiting--;
                    }
                    ));
                }

                //Load all serialized storage variables first
                StartCoroutine(VivaSessionAsset.LoadFileSessionAssets(file, cdm));

                while (!cdm.finished)
                {
                    if(cardsAvailable.Length > 0)
                    {
                        fileLoadStatus.SetText("Processing Assets: " + cdm.waiting);
                    }
                    else
                    {
                        fileLoadStatus.SetText("No Cards Found!");
                    }                   
                    yield return null;
                }
                Debug.Log("[PERSISTANCE] Loaded assets...");

                foreach (var entry in toLoad)
                {
                    cdm.waiting++;
                    StartCoroutine(entry._1.InitializeCompanion(entry._2, delegate
                    {
                        cdm.waiting--;
                    }));
                }

                while (!cdm.finished)
                {
                    if (cardsAvailable.Length > 0)
                    {
                        fileLoadStatus.SetText("Processing Companions: " + cdm.waiting);
                    }
                    else
                    {
                        fileLoadStatus.SetText("No Cards Found!");
                    }
                    yield return null;
                }
                Debug.Log("[PERSISTANCE] Loaded file!");
            }
            onFinishLoadingVivaFile();

            fileLoadStatus.gameObject.SetActive(false);

            mainCamera.cullingMask = oldMask;
        }

        private void OnPostLoadVivaFile()
        {
            LoadLanguage();
            GameDirector.skyDirector.enabled = true;
            //GameDirector.newSkyDirector.enabled = true;
            InitMusic();
        }
    }

}