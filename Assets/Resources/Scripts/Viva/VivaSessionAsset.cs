using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Viva
{
    public abstract class VivaSessionAsset : SealedMonoBehavior
    {
        
        private static Dictionary<string, VivaSessionAsset> assetRegistry = new Dictionary<string, VivaSessionAsset>();

        
        [SerializeField]
        private bool disablePersistance = false;
        [SerializeField]
        public bool targetsSceneAsset = false;
        [SerializeField]
        public string assetName;
        [SerializeField]
        public string uniqueID;

        public string sessionReferenceName { get; protected set; } = null;
        protected static int sessionReferenceCounter = 1;
        private static bool autoAwake = true;


        public virtual bool IgnorePersistance()
        {
            return disablePersistance;
        }

        public static IEnumerator LoadFileSessionData(GameDirector.VivaFile file)
        {
            if (file == null)
            {
                Debug.LogError("[PERSISTANCE] ERROR Cannot load null VivaFile!");
                yield break;
            }

        }

        public static IEnumerator LoadFileSessionAssets(GameDirector.VivaFile file, CoroutineDeserializeManager cdm)
        {
            if (file == null)
            {
                Debug.LogError("[PERSISTANCE] ERROR Cannot load null VivaFile!");
                yield break;
            }

            assetRegistry.Clear(); //ensure the registry is cleared before loading new assets

            //Load all serialized assets
            VivaSessionAsset[] toBeAwakened = new VivaSessionAsset[file.serializedAssets.Count];
            for (int i = 0; i < file.serializedAssets.Count; i++)
            {

                GameDirector.VivaFile.SerializedAsset serializedAsset = file.serializedAssets[i];
                try
                {
                    toBeAwakened[i] = InitializeVivaSessionAsset(serializedAsset);
                }
                catch (Exception e)
                {
                    Debug.LogError("[PERSISTANCE] Error calling load invoke handler for " + serializedAsset.assetName);
                    Debug.LogError(e.ToString());
                }
            }

            //awaken after all prefabs are instantiated to ensure existing references upon awaken
            for (int i = 0; i < toBeAwakened.Length; i++)
            {
                var asset = toBeAwakened[i];
                if (asset != null)
                {
                    var serializedAsset = file.serializedAssets[i];
                    GameDirector.instance.StartCoroutine(SerializedVivaProperty.Deserialize(serializedAsset.properties, asset, cdm));
                    asset.transform.position = serializedAsset.transform.position;
                    asset.transform.rotation = serializedAsset.transform.rotation;

                    asset.OnAwake();
                }
            }
            while (!cdm.finished)
            {
                if (cdm.failed)
                {
                    yield break;
                }
                yield return null;
            }
        }

        protected static VivaSessionAsset InitializeVivaSessionAsset(GameDirector.VivaFile.SerializedAsset serializedAsset)
        {
            GameObject targetAsset = null;
            if (serializedAsset.targetsSceneAsset)
            {
                targetAsset = GameObject.Find(serializedAsset.assetName);
                if (targetAsset == null)
                {
                    Debug.LogError("[ITEM] Could not find scene asset named " + serializedAsset.assetName);
                    return null;
                }
            }
            else
            {
                if (serializedAsset.assetName == "")
                {
                    return null;
                }
                autoAwake = false;
                GameObject prefab = GameDirector.instance.FindItemPrefabByName(serializedAsset.assetName);
                if (prefab != null)
                {
                    targetAsset = GameObject.Instantiate(prefab, serializedAsset.transform.position, serializedAsset.transform.rotation);
                    targetAsset.name = serializedAsset.sessionReferenceName;
                }
                autoAwake = true;
            }
            if (targetAsset == null)
            {
                Debug.LogError("[ITEM] Could not find prefab named " + serializedAsset.assetName);
                return null;
            }

            var vivaSessionAsset = targetAsset.GetComponent<VivaSessionAsset>();
            if (vivaSessionAsset != null)
            {
                vivaSessionAsset.uniqueID = serializedAsset.uniqueID;
                vivaSessionAsset.sessionReferenceName = serializedAsset.sessionReferenceName;
                RegisterAsset(vivaSessionAsset);
            }
            return vivaSessionAsset;
        }

        protected override sealed void Awake()
        {
            if (string.IsNullOrEmpty(uniqueID))
            {
                uniqueID = Guid.NewGuid().ToString();
            }

            if (string.IsNullOrEmpty(sessionReferenceName))
            {
                sessionReferenceName = uniqueID;
            }

            RegisterAsset(this);

            if (targetsSceneAsset || disablePersistance)
            {
                assetName = name;
            }

            if (autoAwake)
            {
                OnAwake();
            }
        }
        
        protected override sealed void OnDestroy()
        {
            DeregisterAsset(this);
            OnStartDestroy();
            base.OnDestroy();
        }

        private static void RegisterAsset(VivaSessionAsset asset)
        {
            if (!string.IsNullOrEmpty(asset.uniqueID))
            {
                assetRegistry[asset.uniqueID] = asset;
            }
        }

        private static void DeregisterAsset(VivaSessionAsset asset)
        {
            if (!string.IsNullOrEmpty(asset.uniqueID))
            {
                assetRegistry.Remove(asset.uniqueID);
            }
        }

        public static VivaSessionAsset FindAssetByID(string uniqueID)
        {
            assetRegistry.TryGetValue(uniqueID, out VivaSessionAsset asset);
            return asset;
        }


        protected override sealed void Start()
        {
            //disable
        }

        protected virtual void OnAwake() { }
        
        protected virtual void OnStartDestroy() { }
        public virtual void Save(GameDirector.VivaFile vivaFile)
        {
            var serializedAsset = new GameDirector.VivaFile.SerializedAsset(this);
            if (serializedAsset == null)
            {
                Debug.LogError("[PERSISTANCE] Could not save " + name);
                return;
            }
            serializedAsset.uniqueID = uniqueID;
            serializedAsset.sessionReferenceName = sessionReferenceName;
            vivaFile.serializedAssets.Add(serializedAsset);
        }
    }

}