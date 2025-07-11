//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace JBooth.FoliageRendering
{
    [InitializeOnLoad]
    public static class CullingAssembler
    {
        private const string _MENU_VALIDATECULLING = "Window/FoliageRenderer/Validate Culling Shader";

        private const string _ASSETS_LOCATION = "/JBooth/FoliageRenderer/Resources/";
        private const string _BUILT_LOCATION = "Assets" + _ASSETS_LOCATION;
        private static string _FILENAME => IndirectRenderer.CULLING_FILENAME;

        private static string _EXTENSION = ".compute";
        private static string _OUTPUT_PATH => _BUILT_LOCATION + _FILENAME + _EXTENSION;


        [System.Serializable]
        private class Fragment
        {
            public string path;
            public bool hdrpOnly;

            public Fragment(string path, bool hdrp)
            {
                this.path = path;
                this.hdrpOnly = hdrp;
            }
        }

        private static Fragment[] _FRAGMENT_PATHS = new Fragment[]
        {
            new Fragment("ComputeFragments/Culling_header", false),
            new Fragment("ComputeFragments/Culling_hdrp_includes", true),
            new Fragment("ComputeFragments/Culling_body", false)
        };

        static CullingAssembler()
        {
            EditorApplication.delayCall += () => ValidateCullingComputeShader(false);
        }

        [MenuItem(_MENU_VALIDATECULLING)]
        public static void MenuItem()
        {
            BuildCullingShader(true);
        }

        public static void ValidateCullingComputeShader(bool showLog = true)
        {
            ComputeShader culling = IndirectRenderer.CullingShader;
            if (culling == null)
                BuildCullingShader(true);
            else
            {
                if(showLog)
                    Debug.Log("Detected Culling Shader needs rebuilt.  Rebuilding a new Culling Shader.");
                BuildCullingShader(false);
            }
        }

        private static void ReadLinesToList(ref List<string> array, string path)
        {
            TextAsset loadedFrag = Resources.Load(path) as TextAsset;
            if (loadedFrag != null)
            {
                array.Add(loadedFrag.text);
            }
        }

        private static void BuildCullingShader(bool showLog)
        {
            List<string> lines = new List<string>();

            int fragCount = _FRAGMENT_PATHS.Length;
            for(int a = 0; a < fragCount;  a++)
            {
                Fragment frag = _FRAGMENT_PATHS[a];
#if _HDRP
                ReadLinesToList(ref lines, frag.path);
#else
                if (frag.hdrpOnly == false) //goofy that this isn't truthy but oh well
                    ReadLinesToList(ref lines, frag.path);
#endif
            }
            System.IO.Directory.CreateDirectory(Application.dataPath + _ASSETS_LOCATION);
            AssetDatabase.Refresh();

            using(StreamWriter writer = new StreamWriter(_OUTPUT_PATH, append: false))
            {
                int lineCount = lines.Count;
                for(int a = 0; a < lineCount; a++)
                    writer.WriteLine(lines[a]);
            }

            AssetDatabase.Refresh(); 

            //after we rebuild call draw so we set all the keywords and
            //stuff without worrying about getting the cullID ourselves
            if (IndirectRenderer.hasInstance)
                IndirectRenderer.instance.DrawAllIndirect();
            if(showLog)
                Debug.Log("Finished creating Culling Shader!");
        }
    }
}