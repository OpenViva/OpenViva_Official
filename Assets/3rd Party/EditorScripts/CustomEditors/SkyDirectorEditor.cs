using UnityEngine;
using UnityEditor;
using OccaSoftware.Altos.Runtime;
using UnityEditor.UIElements;

namespace OccaSoftware.Altos.Editor
{
    [CustomEditor(typeof(AltosSkyDirector))]
    [CanEditMultipleObjects]
    public class SkyDirectorEditor : UnityEditor.Editor
    {
        SerializedProperty skyDefinition;
        SerializedProperty environmentLightingExposure;
        SerializedProperty environmentLightingSaturation;
        SerializedProperty atmosphereDefinition;
        SerializedProperty starDefinition;
        SerializedProperty cloudDefinition;
        SerializedProperty precipitationDefinition;

        SerializedProperty data;

        SerializedObject sky;
        SerializedProperty editorTime;
        SerializedProperty timeSystem;
        
        private bool showEnvironmentLighting = true;

        private void OnEnable()
        {
            skyDefinition = serializedObject.FindProperty("skyDefinition");
            environmentLightingExposure = serializedObject.FindProperty("environmentLightingExposure");
            environmentLightingSaturation = serializedObject.FindProperty("environmentLightingSaturation");
            atmosphereDefinition = serializedObject.FindProperty("atmosphereDefinition");
            starDefinition = serializedObject.FindProperty("starDefinition");
            cloudDefinition = serializedObject.FindProperty("cloudDefinition");
            precipitationDefinition = serializedObject.FindProperty("precipitationDefinition");
            data = serializedObject.FindProperty("data");

            sky = new SerializedObject(skyDefinition.objectReferenceValue);
            editorTime = sky.FindProperty("editorTime");
            timeSystem = sky.FindProperty("timeSystem");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(skyDefinition);
            if (skyDefinition.objectReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                sky.Update();
                SerializedProperty currentTime = editorTime;
                if (Application.isPlaying)
                {
                    currentTime = timeSystem;
                }
                EditorGUILayout.PropertyField(
                    currentTime,
                    new GUIContent("Current Time", "This option only changes the current time in-editor, not the initial time on game start.")
                );
                EditorGUILayout.PropertyField(environmentLightingExposure, new GUIContent("Exposure"));
                EditorGUILayout.PropertyField(environmentLightingSaturation, new GUIContent("Saturation"));

                EditorGUI.indentLevel--;
                sky.ApplyModifiedProperties();
            }

            EditorGUILayout.PropertyField(atmosphereDefinition);
            EditorGUILayout.PropertyField(starDefinition);
            EditorGUILayout.PropertyField(cloudDefinition);
            EditorGUILayout.PropertyField(precipitationDefinition);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Overrides", EditorStyles.boldLabel);
            GUILayoutOption[] option = new GUILayoutOption[] { GUILayout.Width(75) };
            if (AltosSkyDirector.Instance)
            {
                if (AltosSkyDirector.Instance.GetWind(out AltosWindZone windZone))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("WindZone", option);
                    EditorGUILayout.LabelField("Active", option);
                    if (GUILayout.Button("Open", option))
                    {
                        Selection.activeObject = windZone.gameObject;
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.LabelField("A WindZone is actively overriding the wind speed and direction in your scene.", EditorStyles.helpBox);
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("WindZone", option);
                    EditorGUILayout.LabelField("Inactive", option);
                    if (GUILayout.Button("Add", option))
                    {
                        GameObject go = new GameObject("Altos Wind Zone");
                        go.AddComponent<AltosWindZone>();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Resources", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(data);

            if (data.serializedObject == null)
            {
                EditorGUILayout.HelpBox("Altos requires an AltosDataAsset, which contains shader and texture data.", MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
