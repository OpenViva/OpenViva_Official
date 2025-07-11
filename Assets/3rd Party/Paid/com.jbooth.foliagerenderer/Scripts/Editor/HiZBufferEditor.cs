//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
#if _HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace JBooth.FoliageRendering
{
    [CustomEditor(typeof(HiZBuffer))]
    public class HiZBufferEditor : Editor
    {
        bool preview;
        Texture tex;

        public override void OnInspectorGUI()
        {
            FoliageGUIUtil.DrawHeaderLogo();
            serializedObject.Update();

            preview = EditorGUILayout.Foldout(preview, "HiZ Buffer Preview");
            if (preview)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
#if _HDRP
                    if (tex == null)
                        tex = Shader.GetGlobalTexture("_CameraDepthTexture");
#else
                    HiZBuffer buffer = (HiZBuffer)target;
                    tex = buffer.texture;
#endif
                    if (tex != null)
                    {
                        float aspect = (float)tex.height / (float)tex.width;
                        EditorGUILayout.LabelField("", GUILayout.Width(32));
                        float row = EditorGUIUtility.currentViewWidth - 58.0f;
                        var maskPreviewRect = EditorGUILayout.GetControlRect(GUILayout.Width(row), GUILayout.Height(row * aspect));
                        EditorGUI.DrawPreviewTexture(maskPreviewRect, tex);
                    }
                    else
                    {
                        tex = Shader.GetGlobalTexture("_CameraDepthTexture");
                        if (tex != null)
                            EditorGUILayout.LabelField("No active Buffer Texture yet.", EditorStyles.centeredGreyMiniLabel);
                    }
                }
            }
        }
    }
}