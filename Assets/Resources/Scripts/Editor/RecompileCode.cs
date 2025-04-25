using UnityEditor;

public class RecompileCode
{
    [MenuItem("Tools/Recompile Code")]
    static void Recompile()
    {
        EditorApplication.ExecuteMenuItem("Assets/Refresh");
        AssetDatabase.Refresh();
    }
}