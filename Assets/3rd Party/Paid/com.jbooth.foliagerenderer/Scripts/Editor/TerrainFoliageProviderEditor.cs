//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;

namespace JBooth.FoliageRendering
{
    [CustomEditor(typeof(TerrainFoliageProvider))]
    public class TerrainFoliageProviderEditor : Editor
    {
        bool preview;
        public override void OnInspectorGUI()
        {
            FoliageGUIUtil.DrawHeaderLogo();
            serializedObject.Update();
            TerrainFoliageProvider tfp = (TerrainFoliageProvider)target;
            if (tfp.albedoTexture != null)
            {
                preview = EditorGUILayout.Foldout(preview, "Albedo Rendering");
                if (preview)
                {
                    using(new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("", GUILayout.Width(32));
                        EditorGUILayout.Space();
                        var maskPreviewRect = EditorGUILayout.GetControlRect(GUILayout.Width(256), GUILayout.Height(256));
                        EditorGUILayout.Space();
                        EditorGUI.DrawPreviewTexture(maskPreviewRect, tfp.albedoTexture);
                    }
                }
            }
            
        }

    }
}
