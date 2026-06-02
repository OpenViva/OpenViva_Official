using System.IO;
using UnityEngine;

public class ReportCharactersFolderPath : MonoBehaviour
{
    public static string CharactersFolderPath {  get; private set; }

    private void Awake()
    {
        CharactersFolderPath = GetCharactersFolderPath();

        if (!Directory.Exists(CharactersFolderPath))
        {
            Directory.CreateDirectory(CharactersFolderPath);
            Debug.LogError("Created Characters folder at: " + CharactersFolderPath);
        }
        else
        {
            Debug.LogError("Characters folder found at: " + CharactersFolderPath);
        }
    }

    private string GetCharactersFolderPath()
    {
        string exeFolder;

        if (Application.isEditor)
        {
            exeFolder = Directory.GetParent(Application.dataPath).FullName;
        }
        else
        {
            exeFolder = Path.GetDirectoryName(Application.dataPath);
        }

        return Path.Combine(exeFolder, "Characters");
    }
}
