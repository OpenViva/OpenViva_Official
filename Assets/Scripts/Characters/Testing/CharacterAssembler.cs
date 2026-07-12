using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CharacterAssembler : MonoBehaviour
{
    #region Fields
    [SerializeField] private List<CharacterModel> loadedCharacters = new();
    private bool isLoading = false;

    private string charactersFolder = null;

    // Cached invocations
    private static WaitForSecondsRealtime _waitForSecondsRealtime3 = new(3f);
    #endregion

    private void GetCharactersFolder()
    {

#if UNITY_EDITOR
        if (Application.isEditor)
        {
            charactersFolder = Directory.GetParent(Application.dataPath).FullName;
            charactersFolder = Path.Combine(charactersFolder, "Characters");
            Debug.LogError($"[Chara Loader] Found Characters path at {charactersFolder}");
        }
#else
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            string buildFolder = Directory.GetParent(Application.dataPath).FullName;
            charactersFolder = Path.Combine(buildFolder, "Characters");
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            charactersFolder = Path.Combine(Application.persistentDataPath, "Characters");
        }
#endif
    }

    private void Start()
    {
        GetCharactersFolder();
    }

    #region Public Access
    /// <summary>
    /// Returns all loaded character models
    /// </summary>
    public List<CharacterModel> GetAllModels()
    {
        return loadedCharacters;
    }

    /// <summary>
    /// Returns a character model by its name
    /// </summary>
    public CharacterModel GetModelByName(string name)
    {
        foreach (var model in loadedCharacters)
        {
            if (model.bundleName == name)
            {
                return model;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns the first loaded character model (if any exists)
    /// </summary>
    public CharacterModel GetFirstModel()
    {
        if (loadedCharacters.Count > 0)
        {
            return loadedCharacters[0];
        }
        return null;
    }
    #endregion

    public void ReadAllCharacters()
    {
        if (isLoading)
        {
            Debug.LogError("[Chara Loader] Already reading characters. Please wait.");
            return;
        }

        isLoading = true;

        if (!Directory.Exists(charactersFolder))
        {
            Debug.LogError("[Chara Loader] Characters folder not found: " + charactersFolder);
            isLoading = false;
            return;
        }

        string[] vivaFiles = Directory.GetFiles(charactersFolder, "*.viva");

        if (vivaFiles.Length == 0)
        {
            Debug.LogError("[Chara Loader] No .viva files have been found in Characters folder!");
            isLoading = false;
            return;
        }

        Debug.Log($"[Chara Loader] Found {vivaFiles.Length} characters to read.");

        StartCoroutine(ReadCharactersAsync(vivaFiles));
    }

    private IEnumerator ReadCharactersAsync(string[] vivaFiles)
    {
        yield return _waitForSecondsRealtime3;

        foreach (string vivaFile in vivaFiles)
        {
            yield return null;

            string bundleName = Path.GetFileNameWithoutExtension(vivaFile);
            ReadSingleCharacter(vivaFile, bundleName);
        }

        Debug.Log($"[Chara Loader] Total characters read: {loadedCharacters.Count}");
        isLoading = false;
    }

    private void ReadSingleCharacter(string characterPath, string bundleName)
    {
        if (!File.Exists(characterPath))
        {
            Debug.LogError($"[Chara Loader] Package not found: {characterPath}");
            return;
        }

        try
        {
            using (FileStream fs = new FileStream(characterPath, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                var header = VivaFormat.ReadHeader(reader);

                // Check format key
                if (header.VivaKey != VivaFormat.VivaBytes)
                {
                    Debug.LogError($"[Chara Loader] Invalid package file format: {characterPath} (Key: {header.VivaKey})");
                    return;
                }

                // Check format version against the current version
                if (header.Version > VivaFormat.CurrentVersion)
                {
                    Debug.LogError($"[Chara Loader] Unsupported version: {header.Version}. Max supported: {VivaFormat.CurrentVersion}.");
                    return;
                }

                byte[] bundleData = reader.ReadBytes((int)header.BundleSize);
                byte[] metadataData = reader.ReadBytes((int)header.MetadataSize);

                // Load AssetBundle from memory
                var assetBundle = AssetBundle.LoadFromMemory(bundleData);
                if (assetBundle == null)
                {
                    Debug.LogError("[Chara Loader] Failed to load AssetBundle from memory.");
                    return;
                }

                // Read metadata
                VivaMetadata metadata = new();
                metadata.Deserialize(metadataData);

                GameObject prefab = null;

                if (!string.IsNullOrEmpty(metadata.PrefabName))
                {
                    prefab = assetBundle.LoadAsset<GameObject>(metadata.PrefabName);
                }

                // Fallback, load all assets and read first one
                if (prefab == null)
                {
                    var allAssets = assetBundle.LoadAllAssets<GameObject>();
                    if (allAssets.Length > 0)
                    {
                        prefab = allAssets[0];
                        Debug.LogWarning("[Chara Loader] Prefab name not found, using first asset.");
                    }
                }

                if (prefab == null)
                {
                    Debug.LogError("[Chara Loader] Failed to load prefab from AssetBundle.");
                    assetBundle.Unload(false);
                    return;
                }

                ApplyScriptMetadata(prefab, metadataData);

                // Store prefab reference
                string fileName = Path.GetFileNameWithoutExtension(characterPath);

                // Add character to the list
                CharacterModel characterModel = new()
                {
                    bundleName = fileName,
                    prefab = prefab,
                };

                loadedCharacters.Add(characterModel);

                // Cleanup AssetBundle
                assetBundle.Unload(false);

                Debug.Log($"[Chara Loader] Character loaded with prefab name: {fileName}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Chara Loader] Failed to load character: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void ApplyScriptMetadata(GameObject prefab, byte[] metadataData)
    {
        VivaMetadata metadata = new();
        metadata.Deserialize(metadataData);

        // Validate checksum
        if (!metadata.ValidateChecksums())
        {
            Debug.LogError("[Chara Loader] Metadata checksum validation failed!");
        }

        // Apply each script's data to the prefab
        foreach (var scriptData in metadata.Scripts)
        {
            // Find matching component type
            Type scriptType = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                scriptType = assembly.GetType(scriptData.TypeName);
                if (scriptType != null) break;
            }

            if (scriptType == null)
            {
                Debug.LogError("[Chara Loader] Script type not found!");
            }

            // Find the correct GameObject by the given path
            GameObject targetGameObject = FindGameObjectByPath(prefab, scriptData.GameObjectPath);
            if (targetGameObject == null)
            {
                Debug.LogError($"[Chara Loader] GameObject not found for path: {scriptData.GameObjectPath}");
                continue;
            }

            // Find or add component on the correct GameObject
            Component component = targetGameObject.GetComponent(scriptType);
            if (component == null)
            {
                component = targetGameObject.AddComponent(scriptType);
            }

            // Deserialize component data
            DeserializeComponent(component, scriptData.SerializedData);
        }
    }

    private void LoadAssetBundleWithAddressables(string bundlePath, string bundleName, CharacterPackageConfig config)
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(bundleName);

        handle.Completed += (operation) =>
        {
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject loadedPrefab = operation.Result;

                CharacterModel currentModel = new()
                {
                    bundleName = bundleName,
                    prefab = loadedPrefab,
                    boneData = config.boneData,
                };

                // Store the loaded character
                loadedCharacters.Add(currentModel);

                Debug.Log($"[Chara Loader] Character loaded: {bundleName}");
            }
            else
            {
                Debug.LogError($"[Chara Loader] Failed to load bundle: {bundleName}");
            }
        };
    }

    private GameObject FindGameObjectByPath(GameObject root, string path)
    {
        if (root == null)
        {
            Debug.LogError("[Chara Loader] Root GameObject is null!");
            return null;
        }

        if (string.IsNullOrEmpty(path))
        {
            return root;
        }

        string[] pathParts = path.Split('/');
        Transform current = root.transform;

        for (int i = 0; i < pathParts.Length; i++)
        {
            string part = pathParts[i];
            Transform found = current.Find(part);

            if (found == null)
            {
                Debug.LogWarning($"[Chara Loader] Failed to find GameObject in path: {part}");
                return null;
            }

            current = found;
        }

        return current.gameObject;
    }

    private void DeserializeComponent(Component component, byte[] data)
    {
        try
        {
            string json = System.Text.Encoding.UTF8.GetString(data);
            JsonUtility.FromJsonOverwrite(json, component);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[Chara Loader] Failed to deserialize component: {ex.Message}");
        }
    }

    #region Data Classes
    public class CharacterModel
    {
        public string bundleName;
        public GameObject prefab;
        public List<PhysicsBoneData> boneData;
    }

    public class CharacterPackageConfig
    {
        public string bundleName;
        public List<PhysicsBoneData> boneData;
    }

    public enum BonePreset
    {
        Skirt,
        ShortHair,
        LongHair,
        AnimalTail,
        AnimalEars
    }
    #endregion
}
