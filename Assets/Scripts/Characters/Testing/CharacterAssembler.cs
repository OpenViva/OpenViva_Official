using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CharacterAssembler : MonoBehaviour
{
    #region Fields
    [SerializeField] private List<CharacterModel> loadedCharacters = new();
    private bool isLoading = false;

    private string charactersFolder = null;

    // Cached invocations
    private static readonly WaitForSecondsRealtime _waitForSecondsRealtime3 = new(3f);
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
        yield return _waitForSecondsRealtime3;

        foreach (string vivaFile in vivaFiles)
        {
            yield return null;

            string bundleName = Path.GetFileNameWithoutExtension(vivaFile);
            ReadSingleCharacter(vivaFile, bundleName);
        }

        Debug.Log($"[Chara Loader] Loaded {loadedCharacters.Count} characters.");
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

                byte[] characterDataBytes = reader.ReadBytes(header.CharacterDataSize);
                byte[] bundleData = reader.ReadBytes(header.BundleSize);

                // Load Character data
                VivaCharacterData charData = JsonUtility.FromJson<VivaCharacterData>(
                System.Text.Encoding.UTF8.GetString(characterDataBytes));

                // Load AssetBundle from memory
                var assetBundle = AssetBundle.LoadFromMemory(bundleData);
                if (assetBundle == null)
                {
                    Debug.LogError("[Chara Loader] Failed to load AssetBundle from memory.");
                    return;
                }

                GameObject prefab = null;

                if (!string.IsNullOrEmpty(charData.PrefabName))
                {
                    prefab = assetBundle.LoadAsset<GameObject>(charData.PrefabName);
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

                VivaCharacter newData = prefab.AddComponent<VivaCharacter>();
                newData.characterData = charData;

                CharacterModel newCharacterModel = new()
                {
                    bundleName = bundleName,
                    prefab = prefab,
                    vivaCharacterData = charData
                };

                // Add character to the list
                loadedCharacters.Add(newCharacterModel);

                // Cleanup AssetBundle
                assetBundle.Unload(false);
                Debug.Log($"[Chara Loader] Character loaded with prefab name: {prefab.name}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Chara Loader] Failed to load character: {ex.Message}\n{ex.StackTrace}");
        }
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

        int startIndex = 0;
        if (pathParts.Length > 1 || pathParts[0] == root.name) startIndex = 1;

        // If path only contains the root name then give that instead
        if (pathParts.Length == 1)
        {
            return root;
        }

        Transform current = root.transform;

        for (int i = startIndex; i < pathParts.Length; i++)
        {
            string part = pathParts[i].Trim();

            if (string.IsNullOrEmpty(part)) continue;

            Transform found = current.Find(part);

            if (found == null)
            {
                Debug.LogWarning($"[Chara Loader] Failed to find {part} in path: {part}");
                return null;
            }

            current = found;
        }

        return current.gameObject;
    }

    #region Data Classes
    public class CharacterModel
    {
        public string bundleName;
        public GameObject prefab;
        public VivaCharacterData vivaCharacterData;
    }
    #endregion
}
