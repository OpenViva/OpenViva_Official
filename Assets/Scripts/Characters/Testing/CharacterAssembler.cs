using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CharacterAssembler : MonoBehaviour
{
    #region Fields
    private List<CharacterPackage> loadedCharacters = new();
    private bool isReading = false;

    private string charactersFolder = null;
    #endregion

    private void GetCharactersFolder()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            string buildFolder = Directory.GetParent(Application.dataPath).FullName;
            charactersFolder = Path.Combine(buildFolder, "Characters");
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            charactersFolder = Path.Combine(Application.persistentDataPath, "Characters");
        }
    }

    private void Start()
    {
        GetCharactersFolder();
    }

    public void ReadAllCharacters()
    {
        if (isReading)
        {
            Debug.LogWarning("[Char Reader] Already reading characters. Please wait.");
            return;
        }

        isReading = true;

        if (!Directory.Exists(charactersFolder))
        {
            Debug.LogError("[Char Reader] Characters folder not found: " + charactersFolder);
            isReading = false;
            return;
        }

        string[] vivaFiles = Directory.GetFiles(charactersFolder, "*.viva");

        if (vivaFiles.Length == 0)
        {
            Debug.LogWarning("[Char Reader] No .viva files have been found in Characters folder!");
            isReading = false;
            return;
        }

        Debug.Log($"[Char Reader] Found {vivaFiles.Length} characters to read.");

        StartCoroutine(ReadCharactersAsync(vivaFiles));
    }

    private IEnumerator ReadCharactersAsync(string[] vivaFiles)
    {
        foreach (string vivaFile in vivaFiles)
        {
            yield return null;

            string bundleName = Path.GetFileNameWithoutExtension(vivaFile);
            ReadSingleCharacter(vivaFile, bundleName);
        }

        Debug.Log($"[Char Reader] Total characters read: {loadedCharacters.Count}");
        isReading = false;
    }

    private void ReadSingleCharacter(string characterPath, string bundleName)
    {
        if (!File.Exists(characterPath))
        {
            Debug.LogError($"[Char Reader] Character not found: {characterPath}");
            return;
        }

        using (FileStream fs = new FileStream(characterPath, FileMode.Open))
        using (BinaryReader reader = new BinaryReader(fs))
        {
            byte b1 = reader.ReadByte();
            byte b2 = reader.ReadByte();
            byte b3 = reader.ReadByte();
            byte b4 = reader.ReadByte();

            if (b1 != 0x56 || b2 != 0x49 || b3 != 0x56 || b4 != 0x41)
            {
                Debug.LogError($"[Char Reader] Invalid package file format: {characterPath}");
                return;
            }

            // Read bundle name
            string loadedBundleName = reader.ReadString();

            // Read JSON config
            int configLength = reader.ReadInt32();
            byte[] configBytes = reader.ReadBytes(configLength);
            string configJson = System.Text.Encoding.UTF8.GetString(configBytes);

            // Parse config
            var config = JsonConvert.DeserializeObject<CharacterPackageConfig>(configJson);

            // TODO: Read thumbnails

            // Read and discard bundle data
            int bundleLength = reader.ReadInt32();
            reader.ReadBytes(bundleLength);

            CharacterPackage characterPackage = new()
            {
                bundleName = bundleName,
                boneData = config.boneData
            };

            loadedCharacters.Add(characterPackage);
        }
    }

    public class CharacterPackage
    {
        public string bundleName;
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
}
