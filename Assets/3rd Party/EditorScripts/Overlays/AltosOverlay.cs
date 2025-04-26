using OccaSoftware.Altos.Runtime;

using System.Drawing;

using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace OccaSoftware.Altos.Editor
{
    [Overlay(typeof(SceneView), "Altos", true)]
    public class AltosOverlay : Overlay
    {
        VisualElement root;
        FloatField time;

        public override VisualElement CreatePanelContent()
        {
            root = new VisualElement() { name = "Altos Overlay Root" };

            AltosSkyDirector skyDirector = UnityEngine.GameObject.FindObjectOfType<AltosSkyDirector>();

            if (skyDirector != null)
            {
                time = new FloatField("Time of day");

                SerializedObject so = new SerializedObject(skyDirector);
                SerializedProperty sky = so.FindProperty("skyDefinition");
                SerializedObject skyObject = new SerializedObject(sky.objectReferenceValue);

                if (Application.isPlaying)
                {
                    time.value = skyObject.FindProperty("timeSystem").floatValue;
                    time.BindProperty(skyObject.FindProperty("timeSystem"));
                }
                else
                {
                    time.value = skyObject.FindProperty("editorTime").floatValue;
                    time.BindProperty(skyObject.FindProperty("editorTime"));
                }

                root.Add(time);
            }
            else
            {
                root.Add(new Label("No Altos Sky Director found."));
            }

            return root;
        }
    }
}
