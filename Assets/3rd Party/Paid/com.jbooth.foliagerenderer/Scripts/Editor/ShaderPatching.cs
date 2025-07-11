//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace JBooth.FoliageRendering
{
    public class ShaderPatching
    {
        public enum ShaderState
        {
            FRCompatible,
            BuiltIn,
            NeedsConversion
        }
        List<Shader> allShaders = new List<Shader>();
        List<ShaderState> patches = new List<ShaderState>();

        Shader FindOriginal(Shader s)
        {
            var path = AssetDatabase.GetAssetPath(s);
            if (path.Contains("_FR."))
            {
                path = path.Replace("_FR.", ".");
                if (File.Exists(path))
                {
                    Shader orig = AssetDatabase.LoadAssetAtPath<Shader>(path);
                    if (orig != null)
                        return orig;
                }
            }
            return s;
        }

        public void DrawPatchingGUI(List<GameObject> allObjects)
        {
            if (!FoliageGUIUtil.DrawRollup("Shader Patching", true, true))
                return;

            EditorGUILayout.HelpBox("Press scan to make sure your shaders are compatible with Foliage Renderer. If not\n" +
                                    "you can automatically patch them here. This will create a separate _FR shader, and\n" +
                                    "will modify the materials to use the new shader", MessageType.Info);

            if (GUILayout.Button("Scan"))
            {
                allShaders.Clear();
                patches.Clear();
                foreach (var go in allObjects)
                {
                    var mrs = go.GetComponentsInChildren<MeshRenderer>();
                    foreach (var m in mrs)
                    {
                        foreach (var mat in m.sharedMaterials)
                        {
                            if (mat.shader != null)
                            {
                                allShaders.Add(mat.shader);
                            }
                        }
                    }
                }
                allShaders = allShaders.Distinct().ToList();
                foreach (var s in allShaders)
                {
                    patches.Add(IsSetupWithFR(s));
                }
            }
            for (int i = 0; i < allShaders.Count; ++i)
            {
                Shader s = allShaders[i];
                var setupWithFR = patches[i];
                if (setupWithFR == ShaderState.FRCompatible)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PrefixLabel(s.name);
                        if (GUILayout.Button("Repatch", GUILayout.Width(120)))
                        {
                            Shader orig = FindOriginal(s);
                            if (orig == null)
                            {
                                Debug.LogError("Original Shader not found, cannot repatch");
                            }
                            else
                            {
                                var newShader = PatchShader(orig);
                                if (newShader != null)
                                {
                                    foreach (var go in allObjects)
                                    {
                                        var mrs = go.GetComponentsInChildren<MeshRenderer>();
                                        foreach (var m in mrs)
                                        {
                                            foreach (var mat in m.sharedMaterials)
                                            {
                                                if (mat.shader == s)
                                                {
                                                    mat.shader = newShader;
                                                    EditorUtility.SetDirty(mat);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (setupWithFR == ShaderState.BuiltIn)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PrefixLabel(s.name);
                        GUILayout.Label("Cannot Convert", GUILayout.Width(120));
                    }
                }
                else
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PrefixLabel(s.name);

                        if (GUILayout.Button("Fix", GUILayout.Width(120)))
                        {
                            var newShader = PatchShader(s);
                            if (newShader != null)
                            {
                                patches[i] = ShaderState.FRCompatible;
                                foreach (var go in allObjects)
                                {
                                    var mrs = go.GetComponentsInChildren<MeshRenderer>();
                                    foreach (var m in mrs)
                                    {
                                        foreach (var mat in m.sharedMaterials)
                                        {
                                            if (mat.shader == s)
                                            {
                                                mat.shader = newShader;
                                                EditorUtility.SetDirty(mat);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


        }

        static bool FindStringInFile(string filePath, string searchString)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains(searchString))
                    {
                        return true; // String found
                    }
                }
            }

            return false; // String not found
        }

        public static ShaderState IsSetupWithFR(Shader shader)
        {
            var path = AssetDatabase.GetAssetPath(shader);
            if (path.Contains("Resources/unity_builtin_extra"))
                return ShaderState.BuiltIn;
            if (!ShaderUtil.HasProceduralInstancing(shader))
                return ShaderState.NeedsConversion;

            if (FindStringInFile(path, "procedural:setupFoliageRenderer"))
                return ShaderState.FRCompatible;
            return ShaderState.NeedsConversion;
        }

        public static Shader PatchShader(Shader shader)
        {
            var injectPath = "Packages/com.jbooth.foliagerenderer/Shaders/FoliageRendererInstancing.cginc";
            if (!System.IO.File.Exists(injectPath))
            {
                Debug.LogError("Could not find injection code at path : " + injectPath);
                return null;
            }

            string proceduralCall = "#pragma instancing_options procedural:setupFoliageRenderer forwardadd";
            var path = AssetDatabase.GetAssetPath(shader);
            var srcLines = System.IO.File.ReadAllLines(path);
            if (ShaderUtil.HasProceduralInstancing(shader))
            {
                // strip existing
                for (int i = 0; i < srcLines.Length; ++i)
                {
                    var line = srcLines[i];
                    // rename line
                    if (line.Contains("Shader \""))
                    {
                        srcLines[i] = line.Substring(0, line.LastIndexOf("\"")) + "_FR\"";
                    }
                    if (line.Contains("#pragma") && line.Contains("instancing_options"))
                    {
                        var trimmed = line.Trim();
                        if (trimmed.StartsWith("#pragma"))
                        {
                            if (trimmed.Contains("procedural:setupFoliageRenderer")) // already setup, return it
                                return shader;
                            srcLines[i] = proceduralCall + System.Environment.NewLine + "#include \"" + injectPath + "\"";
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < srcLines.Length; ++i)
                {
                    var line = srcLines[i];
                    if (line.Contains("Shader \""))
                    {
                        srcLines[i] = line.Substring(0, line.LastIndexOf("\"")) + "_FR\"";
                    }
                    if (line.Contains("#pragma") && line.Contains("vertex"))
                    {
                        var trimmed = line.Trim();
                        if (trimmed.StartsWith("#pragma"))
                        {
                            line += System.Environment.NewLine;
                            line += proceduralCall + System.Environment.NewLine + "#include \"" + injectPath;
                            srcLines[i] = line;
                        }
                    }
                }
            }


            var outputPath = path.Replace(".", "_FR.");
            // in packages or built in, save in assets
            if (!outputPath.StartsWith("Assets"))
            {
                outputPath = "Assets/JBooth/FoliageRenderer/" + Path.GetFileName(outputPath);
            }
            File.WriteAllLines(outputPath, srcLines);
            AssetDatabase.ImportAsset(outputPath);
            return AssetDatabase.LoadAssetAtPath<Shader>(outputPath);

        }
    }
}