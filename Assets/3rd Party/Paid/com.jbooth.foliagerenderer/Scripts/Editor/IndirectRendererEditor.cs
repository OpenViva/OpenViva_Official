//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;

namespace JBooth.FoliageRendering
{
    [CustomEditor(typeof(IndirectRenderer))]
    public class IndirectRendererEditor : Editor
    {
        void Stat(string name, string values)
        {
            using (new EditorGUILayout.HorizontalScope())
            { 
                EditorGUILayout.LabelField(name, GUILayout.Width(140));
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(values);
            }
        }

        private void DrawProperties()
        {
            serializedObject.Update();

            SerializedProperty shadow = serializedObject.FindProperty(nameof(IndirectRenderer.shadowFrustumCulling));
            SerializedProperty sun = serializedObject.FindProperty(nameof(IndirectRenderer.sun));
            SerializedProperty hizToggle = serializedObject.FindProperty(nameof(IndirectRenderer.hiZOcclusion));
            SerializedProperty cache = serializedObject.FindProperty(nameof(IndirectRenderer.maxGraphicsBufferCaches));
            SerializedProperty debugCam = serializedObject.FindProperty(nameof(IndirectRenderer.cullingCamera));
            SerializedProperty distCheck = serializedObject.FindProperty(nameof(IndirectRenderer.distanceCheckUpdates));
            SerializedProperty frustCheck = serializedObject.FindProperty(nameof(IndirectRenderer.frustumCheckUpdate));

            EditorGUILayout.PropertyField(shadow, true);
            EditorGUILayout.PropertyField(sun, true);
            EditorGUILayout.PropertyField(hizToggle, true);
            EditorGUILayout.PropertyField(cache, true); 
            EditorGUILayout.PropertyField(debugCam, true);
            EditorGUILayout.PropertyField(distCheck, true);
            EditorGUILayout.PropertyField(frustCheck, true);

            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            FoliageGUIUtil.DrawHeaderLogo();

            EditorGUILayout.Space();
            TryDrawCullingShaderWarning();

            DrawProperties();

            FoliageGUIUtil.DrawSeparator();
            var rend = target as IndirectRenderer;
            if (IndirectRenderer.hasInstance == false)
                rend = IndirectRenderer.instance;
            if (rend != null && IndirectRenderer.GraphicsBufferPool != null)
            {
                using (new EditorGUILayout.VerticalScope(FoliageGUIUtil.boxStyle))
                {
                    var stats = IndirectRenderer.GraphicsBufferPool.GetStatistics();
                    EditorGUILayout.LabelField("Graphics Buffers:");
                    EditorGUI.indentLevel++;

                    float totalMem = (stats.inuseMemory + stats.reservedMemory) / 1024.0f / 1024.0f;
                    totalMem += (CellSystem.graphicsBufferBytes / 1024 / 1024);
                    totalMem *= 10;
                    totalMem = (int)totalMem;
                    totalMem /= 10;
                    Stat("Total Memory", totalMem + " mb");
                    Stat("Used Buffers", stats.currentUsedBuffers.ToString());
                    Stat("Free Buffers", stats.currentFreeBuffers.ToString());
                    Stat("Pool Count", stats.poolCount.ToString());
                    Stat("Used Memory", stats.inuseMemory / 1024 + " kb");
                    Stat("Reserve Memory", stats.reservedMemory / 1024 + " kb");
                    Stat("Resize Events", stats.resizeEvents.ToString());
                    Stat("Cell Buffers", CellSystem.graphicsBufferBytes / 1024 + " kb");
                    EditorGUI.indentLevel--;
                }
            }
            if (rend != null)
            {
                using (new EditorGUILayout.VerticalScope(FoliageGUIUtil.boxStyle))
                {
                    EditorGUILayout.LabelField("Rendering Stats:");
                    EditorGUI.indentLevel++;
                    Stat("Draw Calls", IndirectRenderer.drawCalls.ToString());
                    Stat("Unculled Instances", IndirectRenderer.unculledInstanceCount.ToString());

                    if (GUILayout.Button("Capture Post Culling Data"))
                    {
                        IndirectRenderer.doPostCullingCapture = true;
                    }
                    Stat("Post Culling Instances", IndirectRenderer.postCullingInstanceCount.ToString());
                    Stat("Post Culling Shadows", IndirectRenderer.postCullingShadowCount.ToString());

                    EditorGUI.indentLevel--;
                }
            }
            this.Repaint();

        }

        private void TryDrawCullingShaderWarning()
        {
            if(IndirectRenderer.CullingShader == null)
            {
                EditorGUILayout.HelpBox("Culling Shader error detected! Indirect Rendering will not work.", MessageType.Warning);
                if (GUILayout.Button(new GUIContent("Fix Culling Shader", "Check the Culling Shader state and create or update if necessary.")))
                    CullingAssembler.ValidateCullingComputeShader();
                EditorGUILayout.Space();
            }
        }
    }
}
