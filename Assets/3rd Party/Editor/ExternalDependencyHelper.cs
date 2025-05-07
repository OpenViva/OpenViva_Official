using UnityEditor;

using UnityEngine;

namespace OccaSoftware.Altos.Editor
{
    [InitializeOnLoad]
    public static class ExternalDependencyHelper
    {
        static ExternalDependencyHelper()
        {
            CheckForButo();
        }

        private static void CheckForButo()
        {
            const string butoCompatibilityKeyword = "ALTOS_BUTO_COMPATIBILITY_ENABLED";
            const string filePath = "Packages/com.occasoftware.buto/Shaders/Resources/Buto.hlsl";

            if (System.IO.File.Exists(filePath))
            {
                Shader.EnableKeyword(butoCompatibilityKeyword);
            }
            else
            {
                Shader.DisableKeyword(butoCompatibilityKeyword);
            }
        }
    }
}
