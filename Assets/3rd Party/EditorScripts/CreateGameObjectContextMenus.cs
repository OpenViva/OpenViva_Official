using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OccaSoftware.Altos.Runtime;
using System.Linq;

namespace OccaSoftware.Altos.Editor
{
    public class CreateGameObjectContextMenus : EditorWindow
    {
        /// <summary>
        /// Sets up a Skybox Director object stack in the current scene's hierarchy.
        /// </summary>
        [MenuItem("GameObject/Altos/Sky Director", false, 15)]
        private static void CreateDirectorWithChildren()
        {
            if (FindObjectOfType<AltosSkyDirector>() != null)
            {
                Debug.Log("Altos Sky Director already exists in scene.");
                return;
            }
            GameObject director = CreateSkyboxDirector();
            CreateSkyObject("Sun", 0f, SkyObject.ObjectType.Sun, true);
            CreateSkyObject("Moon", 180f, SkyObject.ObjectType.Other, true);
            Selection.activeObject = director;
        }

        public static GameObject CreateSkyboxDirector()
        {
            GameObject skyDirector = new GameObject("Sky Director");
            skyDirector.SetActive(false);
            AltosSkyDirector altosSkyDirector = skyDirector.AddComponent<AltosSkyDirector>();

            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            string path = scene.path;
            string directory = "Assets";
            if (path != "")
            {
                directory = System.IO.Path.GetDirectoryName(path);
            }
            string guid = AssetDatabase.CreateFolder(directory, scene.name + "_Altos");
            string folderPath = AssetDatabase.GUIDToAssetPath(guid);

            altosSkyDirector.skyDefinition = CreateAsset<SkyDefinition>(folderPath, "Sky");
            altosSkyDirector.atmosphereDefinition = CreateAsset<AtmosphereDefinition>(folderPath, "Atmosphere");
            altosSkyDirector.cloudDefinition = CreateAsset<CloudDefinition>(folderPath, "Clouds");
            altosSkyDirector.starDefinition = CreateAsset<StarDefinition>(folderPath, "Stars");
            altosSkyDirector.precipitationDefinition = CreateAsset<PrecipitationDefinition>(folderPath, "Precipitation");
            skyDirector.SetActive(true);
            return skyDirector;
        }

        private static T CreateAsset<T>(string path, string name)
            where T : ScriptableObject
        {
            string newAssetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + name + ".asset");
            T a = CreateInstance<T>();
            AssetDatabase.CreateAsset(a, newAssetPath);
            AssetDatabase.SaveAssets();
            return a;
        }

        [MenuItem("GameObject/Altos/Sky Object", false, 15)]
        public static void CreateSkyObject()
        {
            CreateSkyObject("New Sky Object", Random.Range(0, 360), SkyObject.ObjectType.Other, false);
        }

        private static void CreateSkyObject(string name, float offset, SkyObject.ObjectType objectType, bool addLightComponent)
        {
            AltosSkyDirector skyDirector = FindObjectOfType<AltosSkyDirector>();
            if (skyDirector == null)
            {
                CreateDirectorWithChildren();
                return;
            }

            List<System.Type> types = new List<System.Type>() { typeof(SkyObject) };

            GameObject newSkyObject = new GameObject(name, types.ToArray());
            newSkyObject.transform.SetParent(skyDirector.transform);
            newSkyObject.GetComponent<SkyObject>().type = objectType;
            newSkyObject.GetComponent<SkyObject>().orbitOffset = offset;
            newSkyObject.GetComponent<SkyObject>().SetIcon();
            if (addLightComponent)
            {
                Light l = newSkyObject.AddComponent<Light>();
                l.type = LightType.Directional;
            }

            Selection.activeObject = newSkyObject;
        }

        private static bool CheckIfAnySuns(List<SkyObject> skyObjects)
        {
            foreach (SkyObject skyObject in skyObjects)
            {
                if (skyObject.type == SkyObject.ObjectType.Sun)
                    return true;
            }
            return false;
        }
    }
}
