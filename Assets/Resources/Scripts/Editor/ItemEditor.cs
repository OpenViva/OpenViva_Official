using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Viva;


[CustomEditor(typeof(Item))]
public class ItemInspector : Editor
{

    public override void OnInspectorGUI()
    {

        SerializedObject sObj = new SerializedObject(target);
        DrawDefaultInspector();

        if (GUILayout.Button("Build Collider list"))
        {
            var newColliders = ((Viva.Item)target).gameObject.GetComponentsInChildren<Collider>();
            var validColliders = new List<Collider>();
            foreach (var c in newColliders)
            {
                if (c.gameObject.layer == WorldUtil.itemsLayer && !c.isTrigger)
                {
                    validColliders.Add(c);
                }
            }
            SerializedProperty colliders = sObj.FindProperty("m_colliders");
            colliders.arraySize = validColliders.Count;
            for (int i = 0; i < colliders.arraySize; i++)
            {
                colliders.GetArrayElementAtIndex(i).objectReferenceValue = validColliders[i];
            }
            sObj.ApplyModifiedProperties();
        }
    }
}