#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Rinvos_MissingScriptsRemover : EditorWindow
{
    // Start is called before the first frame update
    private GameObject targetObject;
    private List<GameObject> ObjectsWithMissingScripts = new List<GameObject>();
    private Vector2 scrollPosition = Vector2.zero;
    private GameObject LastTargetObject = null;

    // windows setup
    float minWidthForNames = 120;

    static float minWindowWidth = 500;
    static float minWindowHeight = 400;


    [MenuItem("Tools/Rinvo's/Missing Scripts Remover")]
    private static void ShowWindow()
    {
        Rinvos_MissingScriptsRemover window = GetWindow<Rinvos_MissingScriptsRemover>("Rinvo's Missing Script Remover");
        window.minSize = new Vector2(minWindowWidth, minWindowHeight);
    }

    private void OnObjectChanged(){
        ObjectsWithMissingScripts.Clear();
        FindMissingScripts(targetObject);
    }

    private void OnGUI()
    {
        targetObject = EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true) as GameObject;

        if(targetObject != LastTargetObject){
            OnObjectChanged();
        }

        if (ObjectsWithMissingScripts.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Objects With Missing Scripts:");

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (GameObject obj in ObjectsWithMissingScripts)
            {
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button(obj.name, EditorStyles.objectField, GUILayout.Width(200)))
                {
                    Selection.activeGameObject = obj;
                    EditorGUIUtility.PingObject(obj);
                }
                EditorGUILayout.LabelField("Has " + GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(obj) + " Missing Scipts");
                if (GUILayout.Button("Delete", GUILayout.Width(80)))
                {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
                    ObjectsWithMissingScripts.Remove(obj);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            if (GUILayout.Button("Refresh list")){
                ObjectsWithMissingScripts.Clear();
                FindMissingScripts(targetObject);
            }
            if(GUILayout.Button("Delete All Missing Scripts")){
                foreach (GameObject obj in ObjectsWithMissingScripts){
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
                }
                ObjectsWithMissingScripts.Clear();
                FindMissingScripts(targetObject);
            }
        }
        else{
            if(targetObject == null) EditorGUILayout.LabelField("Select GameObject(Avatar) Above");
            else EditorGUILayout.LabelField("No Missing Scripts Found");
        }
    }

    private void FindMissingScripts(GameObject obj){
        if (HasMissingScripts(obj)) ObjectsWithMissingScripts.Add(obj);
        
        // recursively check children for missing scripts too
        foreach (Transform child in obj.transform){
            FindMissingScripts(child.gameObject);
        }
    }

    private bool HasMissingScripts(GameObject obj){   
        return GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(obj) > 0;
    }


}
#endif