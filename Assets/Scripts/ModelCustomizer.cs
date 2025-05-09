﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Viva.Util;


namespace Viva
{

    public class ModelBuildSettings
    {
        public readonly Shader modelShader;

        public string[] requiredMaterials;
        public Texture2D modelTexture;
        public Texture2D rightEyeTexture;
        public Texture2D leftEyeTexture;

        public int boneCount = 0;
        public int vertexCount = 0;
        public int shapeKeyCount = 0;
        public int materialCount = 0;
        public string[] boneNameTable = null;
        public Vector3[] boneHeadTable = null;
        public Vector3[] boneTailTable = null;
        public float[] boneRollTable = null;
        public bool[] IsPartOfBaseSkeletonTable = null;
        public int[] boneHierarchyChildCountTable = null;
        public int[] childBoneIndicesTable = null;
        public string[] materialNameTable = null;
        public Vector3[] vertexTable = null;
        public Vector3[] normalTable = null;
        public Vector2[] uvTable = null;
        public int[] triVertexIndicesTable = null;
        public BoneWeight[] boneWeightsTable = null;
        public int[] submeshTriCountTable = null;
        public string[] shapeKeyNamesTable = null;
        public int[] shapeKeyLengthsTable = null;
        public int[] vertexDeltaIndicesTable = null;
        public Vector3[] vertexDeltaOffsetTable = null;
        public Vector4 hatLocalPosAndPitch = Vector4.zero;
        public Vector4 headpatWorldSphere = Vector4.zero;
        public Vector4 headCollisionWorldSphere = Vector4.zero;

        public ModelBuildSettings(Shader _modelShader, string[] _requiredMaterials)
        {
            modelShader = _modelShader;
            requiredMaterials = _requiredMaterials;
        }
    }

    public partial class ModelCustomizer : UITabMenu
    {
        public static ModelCustomizer main;

        public enum Tab
        {
            BROWSE,
            CREATE,
            TWEAK,
            CARD_RESULT,
            NONE
        }

        [Header("UI")]
        [SerializeField]
        private GameObject browseTab;

        [Header("Import Variables")]

        [SerializeField]
        private Shader modelShader;

        [SerializeField]
        private RectTransform tabsButtonsContainer;
        [SerializeField]
        private Transform characterPlaySpawnTransform;
        [SerializeField]
        private Text selectedCharactersText;

        private Coroutine activeCoroutine = null;

        private static bool DEBUG_BOOL = false;
        private void Awake()
        {
            main = this;
            InitializeTabs(new GameObject[] { browseTab, createTab, tweakTab, cardResultTab });
            this.enabled = false;
        }

        private void OnEnable()
        {
            ValidateAllowTweakTab();
        }

        private void Start()
        {
#if UNITY_EDITOR
            if (!DEBUG_BOOL)
            { //This is a Hardcoded Way to Load Viva Files Since you cant drag and drop into unity editor
                DEBUG_BOOL = true;
                GameDirector.input.actions.Keyboard.wave.performed += delegate
                {
                    ClickCreateTab();
                    GameDirector.instance.StartCoroutine(ApplyFilesToModel(new string[]{
                    //"C:/Users/Master-Donut/Documents/viva/Cards/Starter Kit/shinobu/megumin.viva3d",
                    "C:/Users/tabwe/Downloads/body.viva3d",
                    //"C:/Users/tabwe/Documents/viva/Cards/Starter Kit/senko/senko_texture.png",
                    //"C:/Users/tabwe/Documents/viva/Cards/Starter Kit/senko/senko_pupil_r.png",
                    //"C:/Users/tabwe/Documents/viva/Cards/Starter Kit/senko/senko_pupil_l.png",

                    //"C:/Program Files/Blender Foundation/Blender 2.81/Viva Project Pre-Exports/megumin.viva3d",
                    //"C:/Program Files/Blender Foundation/Blender 2.81/Viva Project Pre-Exports/megumin.png"
                    //"C:/Program Files/Blender Foundation//Blender/Viva Project Pre-Exports/shinobu_texture.png"
                }));
                };
            }
#endif
        }

        public void SetAllTabButtonsInteractible(bool interactible)
        {
            for (int i = 0; i < tabsButtonsContainer.childCount; i++)
            {
                Button button = tabsButtonsContainer.GetChild(i).GetComponent<Button>();
                if (button.interactable != interactible)
                {
                    button.interactable = interactible;
                }
            }
        }

        public void ClickBrowseTab()
        {
            SetTab((int)Tab.BROWSE);
        }

        public void ClickCreateTab()
        {
            SetTab((int)Tab.CREATE);
        }

        public void ClickTweakTab()
        {
            SetTab((int)Tab.TWEAK);
        }

        protected override void OnValidTabChange(int newTab)
        {
            modelPreviewer.SetPreviewMode(ModelPreviewViewport.PreviewMode.NONE);
            switch ((Tab)newTab)
            {
                case Tab.BROWSE:
                    InitializeBrowseTab();
                    break;
                case Tab.CREATE:
                    InitializeCreateTab();
                    break;
                case Tab.TWEAK:
                    InitializeTweakTab();
                    break;
            }
        }

        public void clickOpenWebManual()
        {
            Application.OpenURL("https://shinobuproject.itch.io/game/devlog/100331/viva-character-manual-for-v06-and-above");
        }

        public void clickDeselectAll()
        {
            foreach (Companion companion in GameDirector.player.objectFingerPointer.selectedCompanions.ToList())
            {
                companion.OnUnselected();
                GameDirector.player.objectFingerPointer.selectedCompanions.Remove(companion);
            }
            selectedCharactersText.text = GameDirector.player.objectFingerPointer.selectedCompanions.Count + " " + LocalizationManager.GetLocalizedStringFromTable("OtherUI", "CharactersSelected");
        }

        public override void OnBeginUIInput()
        {            
            FileDragAndDrop.EnableDragAndDrop(OnDropFile);
            enabled = true;
            SetTab(lastValidTabIndex);
            selectedCharactersText.text = GameDirector.player.objectFingerPointer.selectedCompanions.Count + " " + LocalizationManager.GetLocalizedStringFromTable("OtherUI", "CharactersSelected");
        }

        public override void OnExitUIInput()
        {
            enabled = false;
            SetTab((int)Tab.NONE);
            FileDragAndDrop.DisableDragAndDrop();
            selectedCharactersText.text = "";
        }

        public void PlaySpawnSound()
        {
            GameDirector.instance.PlayGlobalSound(modelSpawnSound);
        }

        private IEnumerator ApplyFilesToModel(string[] files)
        {

            int textureCount = 0;
            string vivaModelFilePath = null;
            foreach (string file in files)
            {
                string extension = file.ToLower().Split('.').Last();
                if (extension == "png")
                {
                    textureCount++;
                }
                else if (extension == "viva3d")
                {
                    if (vivaModelFilePath != null)
                    {
                        EndActiveCoroutineAction("Cannot upload more than one viva3d file at a time!");
                        yield break;
                    }
                    vivaModelFilePath = file;
                }
            }
            //load model if applicable
            if (vivaModelFilePath != null)
            {
                PlaySpawnSound();
                ModelBuildSettings mbs = new(
                    modelShader,
                    new string[] { "skin", "pupil_r", "pupil_l" }
                );
                VivaModel.CreateCompanionRequest createModelRequest = new(modelDefault, vivaModelFilePath, mbs);
                yield return GameDirector.instance.StartCoroutine(VivaModel.DeserializeVivaModel(createModelRequest));
                if (createModelRequest.result == null)
                {
                    EndActiveCoroutineAction("Could not deserialize Viva model!");
                    yield break;
                }
                var serializedCompanion = new GameDirector.VivaFile.SerializedCompanion(vivaModelFilePath, new GameDirector.VivaFile.SerializedAsset(vivaModelFilePath));
                bool finished = false;
                GameDirector.instance.StartCoroutine(createModelRequest.result.InitializeCompanion(serializedCompanion, delegate
                {
                    finished = true;
                }));
                while (!finished)
                {
                    yield return null;
                }
                createModelRequest.result.spine1RigidBody.isKinematic = true;
                createModelRequest.result.SetOutfit(Outfit.Create(new string[0], false));
                modelPreviewer.SetPreviewCompanion(createModelRequest.result);
            }
            //load textures if applicable
            FileTextureRequest[] requests;
            if (textureCount != 0)
            {
                requests = new FileTextureRequest[textureCount];
                textureCount = 0;
                foreach (string file in files)
                {
                    if (file.ToLower().Split('.').Last() == "png")
                    {
                        requests[textureCount++] = new FileTextureRequest(file);
                    }
                }
            }
            else if (modelPreviewer.modelDefault != null)
            {  //if no texture files dropped, try to load all by model name
                string headModelName = modelPreviewer.modelDefault.headModel.name;
                requests = new FileTextureRequest[3];
                requests[0] = new FileTextureRequest(
                    headModelName + ".png",
                    new Vector2Int[] { new(1024, 1024) },
                    "Character texture must be 1024x1024!"
                );
                requests[1] = new FileTextureRequest(
                    headModelName + "_pupil_r.png",
                    new Vector2Int[] { new(512, 512) },
                    "*_pupil_r.png must be 512x512!"
                );
                requests[2] = new FileTextureRequest(
                    headModelName + "_pupil_l.png",
                    new Vector2Int[] { new(512, 512) },
                    "*_pupil_l.png must be 512x512!"
                );
            }
            else
            {
                requests = new FileTextureRequest[0];
            }
            foreach (FileTextureRequest request in requests)
            {
                yield return GameDirector.instance.StartCoroutine(LoadFileTexture(request));
            }

            if (modelPreviewer.modelDefault != null)
            {
                //apply textures to model
                foreach (FileTextureRequest request in requests)
                {
                    if (request.result == null)
                    {
                        Debug.Log("[VIVA MODEL] Failed texture request " + request.filename);
                        EndActiveCoroutineAction(request.error);
                        continue;
                    }
                    if (request.result.name.EndsWith("_pupil_r.png"))
                    {
                        modelPreviewer.modelDefault.headModel.rightEyeTexture = request.result;
                    }
                    else if (request.result.name.EndsWith("_pupil_l.png"))
                    {
                        modelPreviewer.modelDefault.headModel.leftEyeTexture = request.result;
                    }
                    else
                    {
                        modelPreviewer.modelDefault.headModel.texture = request.result;
                    }
                }
                modelPreviewer.modelDefault.headModel.ApplyHeadModelTextures(modelPreviewer.modelDefault.headSMR);
            }
            EndActiveCoroutineAction(null);
            ValidateAllInfoProperties();
        }

        public void clickDragAndDropFiles()
        {
            GameDirector.instance.SetEnableControls(GameDirector.ControlsAllowed.NONE);
        }

        public void clickPlay()
        {
            GameDirector.instance.StopUIInput();
            if (modelDefault.gameObject.activeSelf)
            {
                if (GameDirector.player.objectFingerPointer.selectedCompanions.Count > 0)
                {
                    foreach (var loli in GameDirector.player.objectFingerPointer.selectedCompanions)
                    {
                        loli.SetHeadModel(modelDefault.headModel, lastMBS);
                    }
                }
                else
                {
                    GameDirector.instance.town.BuildTownLolis(new string[] { modelDefault.headModel.name }, 1, characterPlaySpawnTransform.position);
                }
                modelDefault.gameObject.SetActive(false);
                modelPreviewer.SetPreviewCompanion(null);
            }
        }

        public void OnDropFile(List<string> files, B83.Win32.POINT aDropPoint)
        {

            //avoid calling multiple drag and drop coroutines
            if (activeCoroutine != null)
            {
                return;
            }

            //mouse must hit UI
            RaycastHit hitInfo = new();
            Ray mouseray = GameDirector.instance.mainCamera.ScreenPointToRay(GameDirector.player.mousePosition);
            if (!Physics.Raycast(mouseray.origin, mouseray.direction, out hitInfo, 2.0f, WorldUtil.uiMask))
            {
                return;
            }
            if (files.Count == 0)
            {
                return;
            }

            SetActiveCoroutineAction(ApplyFilesToModel(files.ToArray()), true);
        }

        private void SetActiveCoroutineAction(IEnumerator routine, bool playLoadingCycle)
        {
            if (activeCoroutine != null)
            {
                return;
            }
            if (playLoadingCycle)
            {
                StartLoadingCycle();
            }
            characterCardBrowser.SeAllCardsInteractible(false);
            SetAllTabButtonsInteractible(false);
            activeCoroutine = GameDirector.instance.StartCoroutine(routine);
        }

        private void EndActiveCoroutineAction(string error)
        {
            if (error != null)
            {
                Debug.LogError(error);
                characterCardBrowser.SeAllCardsInteractible(false);
                SetAllTabButtonsInteractible(false);
                DisplayErrorWindow(error);
            }
            else
            {  //if no error, restore UI interaction
                characterCardBrowser.SeAllCardsInteractible(true);
                SetAllTabButtonsInteractible(true);
                activeCoroutine = null;
            }
            StopLoadingCycle();
        }

        public class FileTextureRequest
        {

            public readonly string filename;
            public Texture2D result;
            public string error = null;
            public readonly Vector2Int[] targetSizes;
            public readonly string targetSizeError;
            public int targetSizeIndex = -1;

            public FileTextureRequest(string _filename, Vector2Int[] _targetSizes = null, string _targetSizeError = null)
            {
                filename = _filename;
                targetSizes = _targetSizes;
                targetSizeError = _targetSizeError;
            }
        }


        public static IEnumerator LoadFileTexture(FileTextureRequest request)
        {
            Debug.Log("[FILE TEXTURE] " + request.filename);
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(request.filename))
            {
                yield return uwr.SendWebRequest();
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log("[FILE TEXTURE] Could not load [" + request.filename + "] " + uwr.error);
                    yield break;
                }
                request.result = DownloadHandlerTexture.GetContent(uwr);
                if (request.result == null)
                {
                    Debug.Log("[FILE TEXTURE] Could not read from handle [" + request.filename + "] " + uwr.error);
                    yield break;
                }
                if (request.result.width == 8 && request.result.height == 8)
                {
                    GameDirector.Destroy(request.result);
                    request.result = null;
                    Debug.Log("[FILE TEXTURE] Could not load [" + request.filename + "] " + uwr.error);
                    yield break;
                }
                request.result.name = request.filename.Split('/').Last().Split('\\').Last();
                request.result.wrapMode = TextureWrapMode.Clamp;

                if (request.targetSizes != null)
                {
                    for (int i = 0; i < request.targetSizes.Length; i++)
                    {
                        if (request.targetSizes[i] == new Vector2Int(request.result.width, request.result.height))
                        {
                            request.targetSizeIndex = i;
                            break;
                        }
                    }
                    if (request.targetSizeIndex == -1)
                    {
                        GameDirector.Destroy(request.result);
                        request.result = null;
                        if (request.targetSizeError != null)
                        {
                            request.error = request.targetSizeError;
                        }
                        else
                        {
                            request.error = "Invalid image size!";
                        }
                    }
                }
            }
        }
    }

}