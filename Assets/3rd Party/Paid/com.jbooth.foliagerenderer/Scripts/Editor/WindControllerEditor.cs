//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEditor;

namespace JBooth.FoliageRendering
{
    [CustomEditor(typeof(WindController))]
    public class WindControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            FoliageGUIUtil.DrawHeaderLogo();
            serializedObject.Update();

            if (FoliageGUIUtil.DrawRollup("General Parameters"))
            {
                using (new EditorGUILayout.VerticalScope(FoliageGUIUtil.boxStyle))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WindController.windSpeed)), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WindController.turbulence)), true);
                    EditorGUI.indentLevel--;
                }
            }

            if (FoliageGUIUtil.DrawRollup("Noise Parameters"))
            {
                using (new EditorGUILayout.VerticalScope(FoliageGUIUtil.boxStyle))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WindController.noiseTexture)), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WindController.bendingWorldSize)), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WindController.leafWorldSize)), true);
                    EditorGUI.indentLevel--;
                }
            }

            if (FoliageGUIUtil.DrawRollup("Gust Settings"))
            {
                using(new EditorGUILayout.VerticalScope(FoliageGUIUtil.boxStyle))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WindController.gustTexture)), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WindController.gustWorldSize)), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WindController.gustSpeed)), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WindController.gustScale)), true);
                    EditorGUI.indentLevel--;
                }
            }

            if(FoliageGUIUtil.DrawRollup("Wind Zones"))
            {
                using (new EditorGUILayout.VerticalScope(FoliageGUIUtil.boxStyle))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WindController.point1)), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WindController.point2)), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WindController.point3)), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(WindController.point4)), true);
                    EditorGUI.indentLevel--;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}