using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class CharacterAssembler : MonoBehaviour
{
    #region Fields
    [SerializeField] private List<CharacterModel> loadedCharacters = new();
    private bool isLoading = false;

    private string charactersFolder = null;
    #endregion

    private void Start()
    {
        GetCharactersFolder();
    }

    private void GetCharactersFolder()
    {
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            charactersFolder = Directory.GetParent(Application.dataPath).FullName;
            charactersFolder = Path.Combine(charactersFolder, "Characters");
            Debug.Log($"[Chara Loader] Found Characters path at {charactersFolder}");
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

    #region Public Access
    /// <summary>
    /// Returns all loaded character models.
    /// </summary>
    public List<CharacterModel> GetAllModels() => loadedCharacters;

    /// <summary>
    /// Returns a character model by its name.
    /// </summary>
    public CharacterModel GetModelByName(string name) => loadedCharacters.Find(m => m.bundleName == name);

    /// <summary>
    /// Returns the first loaded character model (if any exists).
    /// </summary>
    public CharacterModel GetFirstModel() => loadedCharacters.Count > 0 ? loadedCharacters[0] : null;
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
        foreach (string vivaFile in vivaFiles)
        {
            string bundleName = Path.GetFileNameWithoutExtension(vivaFile);

            // 1. Offload file reading and JSON parsing to a background thread
            Task<ParsedFileData> parseTask = Task.Run(() => ReadFileOnBackgroundThread(vivaFile));
            while (!parseTask.IsCompleted)
            {
                yield return null; // Wait for thread without blocking main frame
            }

            ParsedFileData fileData = parseTask.Result;
            if (fileData.HasError)
            {
                Debug.LogError($"[Chara Loader] {fileData.ErrorMessage}");
                continue;
            }

            // 2. Load AssetBundle asynchronously on the Main Thread
            AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromMemoryAsync(fileData.BundleData);
            yield return bundleRequest;

            AssetBundle assetBundle = bundleRequest.assetBundle;
            if (assetBundle == null)
            {
                Debug.LogError("[Chara Loader] Failed to load AssetBundle from memory.");
                continue;
            }

            // 3. Load Prefab asynchronously on the Main Thread
            GameObject prefab = null;
            if (!string.IsNullOrEmpty(fileData.CharData.PrefabName))
            {
                AssetBundleRequest assetRequest = assetBundle.LoadAssetAsync<GameObject>(fileData.CharData.PrefabName);
                yield return assetRequest;
                prefab = assetRequest.asset as GameObject;
            }

            // Load the first asset if prefab is empty for some reason
            if (prefab == null)
            {
                AssetBundleRequest allAssetsRequest = assetBundle.LoadAllAssetsAsync<GameObject>();
                yield return allAssetsRequest;

                if (allAssetsRequest.allAssets.Length > 0)
                {
                    prefab = allAssetsRequest.allAssets[0] as GameObject;
                    Debug.LogWarning("[Chara Loader] Prefab name not found, using first asset.");
                }
            }

            if (prefab == null)
            {
                Debug.LogError("[Chara Loader] Failed to load prefab from AssetBundle.");
                assetBundle.Unload(false);
                continue;
            }

            // 4. Final setup on Main Thread
            VivaCharacter newData = prefab.AddComponent<VivaCharacter>();
            newData.characterData = fileData.CharData;

            CharacterModel newCharacterModel = new()
            {
                bundleName = bundleName,
                prefab = prefab,
                vivaCharacterData = fileData.CharData
            };

            loadedCharacters.Add(newCharacterModel);

            // Cleanup AssetBundle
            assetBundle.Unload(false);
            Debug.Log($"[Chara Loader] Character loaded with prefab name: {prefab.name}");
        }

        Debug.Log($"[Chara Loader] Loaded {loadedCharacters.Count} characters.");
        isLoading = false;
    }

    /// <summary>
    /// Thread-safe helper method to read binary and JSON off the main thread.
    /// </summary>
    private ParsedFileData ReadFileOnBackgroundThread(string characterPath)
    {
        if (!File.Exists(characterPath))
        {
            return new ParsedFileData { ErrorMessage = $"Package not found: {characterPath}" };
        }

        try
        {
            using FileStream fs = new(characterPath, FileMode.Open, FileAccess.Read);
            using BinaryReader reader = new(fs);
            var header = VivaFormat.ReadHeader(reader);

            if (header.VivaKey != VivaFormat.VivaBytes)
            {
                return new ParsedFileData { ErrorMessage = $"Invalid package file format: {characterPath} (Key: {header.VivaKey})" };
            }

            if (header.Version > VivaFormat.CurrentVersion)
            {
                return new ParsedFileData { ErrorMessage = $"Unsupported version: {header.Version}. Max supported: {VivaFormat.CurrentVersion}." };
            }

            byte[] characterDataBytes = reader.ReadBytes(header.CharacterDataSize);
            byte[] bundleData = reader.ReadBytes(header.BundleSize);

            VivaCharacterData charData = JsonUtility.FromJson<VivaCharacterData>(
                System.Text.Encoding.UTF8.GetString(characterDataBytes));

            return new ParsedFileData
            {
                CharData = charData,
                BundleData = bundleData
            };
        }
        catch (Exception ex)
        {
            return new ParsedFileData { ErrorMessage = $"Failed to load character: {ex.Message}" };
        }
    }

    #region Helper Classes
    private class ParsedFileData
    {
        public VivaCharacterData CharData;
        public byte[] BundleData;
        public string ErrorMessage;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    }

    public class CharacterModel
    {
        public string bundleName;
        public GameObject prefab;
        public VivaCharacterData vivaCharacterData;
    }
    #endregion
}