using UnityEngine;
using UnityEditor;
using OccaSoftware.Altos.Runtime;

namespace OccaSoftware.Altos.Editor
{
    [CustomEditor(typeof(SkyObject))]
    [CanEditMultipleObjects]
    public class SkyObjectEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            Tools.hidden = true;
        }

        private void OnDisable()
        {
            Tools.hidden = false;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        private void OnSceneGUI()
        {
            var o = target as SkyObject;
            Handles.DrawWireCube(o.GetChild().position, Vector3.one);
        }
    }
}
