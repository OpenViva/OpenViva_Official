using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OccaSoftware.Altos.Runtime;

namespace OccaSoftware.Altos.Editor
{
    [CustomEditor(typeof(Runtime.SkyDefinition))]
    [CanEditMultipleObjects]
    public class SkyboxDefinitionEditor : UnityEditor.Editor
    {
        private static GUILayoutOption miniButtonWidth = GUILayout.Width(20f);

        SerializedProperty periodsOfDay;
        SerializedProperty initialTime;
        SerializedProperty editorTime;
        SerializedProperty timeSystem;
        SerializedProperty dayNightCycleDuration;
        

        SkyDefinition skyDefinition;

        private bool showTimeSettings = true;
        private bool showPeriodOfDayKeyframes = true;

        private void OnEnable()
        {
            skyDefinition = (SkyDefinition)serializedObject.targetObject;

            periodsOfDay = serializedObject.FindProperty("periodsOfDay");
            initialTime = serializedObject.FindProperty("initialTime");
            timeSystem = serializedObject.FindProperty("timeSystem");
            editorTime = serializedObject.FindProperty("editorTime");
            dayNightCycleDuration = serializedObject.FindProperty("dayNightCycleDuration");
            
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (Event.current.type == EventType.MouseUp)
            {
                skyDefinition.SortPeriodsOfDay();
            }

            Draw();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTimeSettings()
        {
            // Time Settings
            showTimeSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showTimeSettings, new GUIContent("Time Options"));
            if (showTimeSettings)
            {
                EditorGUI.indentLevel++;

                SerializedProperty currentTime = Application.isPlaying ? timeSystem : editorTime;
                EditorGUILayout.PropertyField(currentTime, new GUIContent("Current Time", "Scrub the current game time."));
                EditorGUI.indentLevel++;
                System.TimeSpan timeActive = System.TimeSpan.FromHours(skyDefinition.CurrentTime);
                GUIContent drawDay = new GUIContent(
                    $"Day {skyDefinition.CurrentDay} {timeActive.ToString("hh':'mm':'ss")}",
                    "Represents the in-game day time."
                );
                EditorGUILayout.LabelField(drawDay);

                EditorGUI.indentLevel--;

                EditorGUILayout.PropertyField(initialTime, new GUIContent("Initial Time", "Set the initial in-game time when you hit Play."));

                EditorGUI.indentLevel++;

                timeActive = System.TimeSpan.FromHours(SkyDefinition.SystemTimeToTime024(initialTime.floatValue));
                drawDay = new GUIContent(
                    $"Day {SkyDefinition.SystemTimeToDay(initialTime.floatValue)} {timeActive.ToString("hh':'mm':'ss")}",
                    "Represents the in-game day time."
                );
                EditorGUILayout.LabelField(drawDay);
                EditorGUI.indentLevel--;

                EditorGUILayout.PropertyField(
                    dayNightCycleDuration,
                    new GUIContent(
                        "Day-Night Cycle Duration (m)",
                        "The duration of each full day-night cycle (in minutes). Set to 0 to disable the automatic progression of time."
                    )
                );
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawKeyframeSettings()
        {
            showPeriodOfDayKeyframes = EditorGUILayout.BeginFoldoutHeaderGroup(
                showPeriodOfDayKeyframes,
                new GUIContent(
                    "Periods of Day Key Frames",
                    "Periods of day are treated as keyframes. The sky will linearly interpolate between the current period's colorset and the next period's colorset."
                )
            );
            if (showPeriodOfDayKeyframes)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < periodsOfDay.arraySize; i++)
                {
                    EditorGUILayout.Space(5f);
                    SerializedProperty periodOfDay = periodsOfDay.GetArrayElementAtIndex(i);

                    SerializedProperty description_Prop = periodOfDay.FindPropertyRelative("description");
                    SerializedProperty startTime_Prop = periodOfDay.FindPropertyRelative("startTime");
                    SerializedProperty daySegment_Prop = periodOfDay.FindPropertyRelative("daySegment");
                    SerializedProperty horizonColor_Prop = periodOfDay.FindPropertyRelative("horizonColor");
                    SerializedProperty zenithColor_Prop = periodOfDay.FindPropertyRelative("zenithColor");
                    SerializedProperty groundColor_Prop = periodOfDay.FindPropertyRelative("groundColor");
                    SerializedProperty fogColor_Prop = periodOfDay.FindPropertyRelative("fogColor");
                    SerializedProperty fogDensity_Prop = periodOfDay.FindPropertyRelative("fogDensity");

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(description_Prop, new GUIContent("Name", "(Optional) Descriptive name"));

                    if (GUILayout.Button("-", EditorStyles.miniButtonRight, miniButtonWidth))
                    {
                        periodsOfDay.DeleteArrayElementAtIndex(i);
                    }
                    else
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.indentLevel++;

                        EditorGUILayout.PropertyField(
                            startTime_Prop,
                            new GUIContent(
                                "Start Time",
                                "Defines the start time for the values associated with this key frame. The previous key frame linearly interpolates to these values from the previous key frame."
                            )
                        );
                        EditorGUILayout.PropertyField(daySegment_Prop, new GUIContent("Day Segment"));
                        EditorGUILayout.PropertyField(zenithColor_Prop, new GUIContent("Sky Color"));
                        EditorGUILayout.PropertyField(horizonColor_Prop, new GUIContent("Equator Color"));
                        EditorGUILayout.PropertyField(groundColor_Prop, new GUIContent("Ground Color"));
                        EditorGUILayout.PropertyField(fogColor_Prop, new GUIContent("Fog Color"));
                        EditorGUILayout.PropertyField(fogDensity_Prop, new GUIContent("Fog Density"));
                        EditorGUI.indentLevel--;
                    }
                }
                EditorGUILayout.Space();
                Rect r = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
                if (GUI.Button(r, "+"))
                {
                    periodsOfDay.arraySize += 1;
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        

        private void Draw()
        {
            DrawTimeSettings();
            DrawKeyframeSettings();
        }
    }
}
