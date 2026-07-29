using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class BuildPostProcessor
{
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        string charactersFolder = null;

        if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
        {
            string buildFolder = Path.GetDirectoryName(pathToBuiltProject);
            charactersFolder = Path.Combine(buildFolder, "Characters");
        }
        else if (target == BuildTarget.Android)
        {
            charactersFolder = Path.Combine(Application.persistentDataPath, "Characters");
        }

        if (!string.IsNullOrEmpty(charactersFolder))
        {
            if (!Directory.Exists(charactersFolder))
            {
                Directory.CreateDirectory(charactersFolder);
                Debug.Log($"[Build Post-Process] Created Characters folder at: {charactersFolder}");
            }

            string readmePath = Path.Combine(charactersFolder, "Place Characters Here!.txt");
            if (!File.Exists(readmePath))
            {
                File.WriteAllText(readmePath, "Place your .viva character files here.");
            }
        }
    }
}
