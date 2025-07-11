//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

#if _URP
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace JBooth.FoliageRendering
{
    [InitializeOnLoad]
    public static class URPAssembler
    {
        private const string _MENU_VALIDATEURP = "Window/FoliageRenderer/Validate URP Data Assets";

        static URPAssembler()
        {
            EditorApplication.delayCall += () => ValidateURPData();
        }

        [MenuItem(_MENU_VALIDATEURP)]
        public static void ValidateURPData()
        {
            List<ScriptableRendererData> dataObjects = new List<ScriptableRendererData>();
            int qualCount = QualitySettings.names.Length;
            for(int a = 0; a < qualCount; a++)
            {
                UniversalRenderPipelineAsset asset = QualitySettings.GetRenderPipelineAssetAt(a) as UniversalRenderPipelineAsset;
                ScriptableRendererData data = GetRendererDataForAsset(asset);
                if (dataObjects.Contains(data) == false)
                {
                    dataObjects.Add(data);
                    TryAddFeatureToData(data);
                }
            }
        }

        private static void TryAddFeatureToData(ScriptableRendererData data)
        {
            if(data == null || data.rendererFeatures == null || data.rendererFeatures.Count == 0)
            {
                Debug.LogWarning("Unable to automatically attach Hi Z URP Feature.  " +
                    "Make sure you have a ScriptableRenderer Data present, and Validate URP Assets from the " + _MENU_VALIDATEURP + " Menu Item. " +
                    "Or, manually add the Hi Z URP Render Feature to your ScriptableRenderer Data Objects as needed.");
                return;
            }
            List <ScriptableRendererFeature> features = data.rendererFeatures;
            int count = features.Count;
            for(int a = 0; a < count; a++)
            {
                if (features[a] is HiZURP)
                    return;
            }
            Debug.Log("Adding HiZ URP Feature to " + data.name);
            ScriptableRendererFeature newFeature = ScriptableObject.CreateInstance<HiZURP>();
            newFeature.name = typeof(HiZURP).Name;

            SerializedObject so = new SerializedObject(data);
            so.Update();
            SerializedProperty featuresProp = so.FindProperty("m_RendererFeatures");
            SerializedProperty mapProp = so.FindProperty("m_RendererFeatureMap");
            AssetDatabase.AddObjectToAsset(newFeature, data);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(newFeature, out var guid, out long localId);

            int arraySize = featuresProp.arraySize;
            featuresProp.InsertArrayElementAtIndex(arraySize);
            SerializedProperty newFeatureEntry = featuresProp.GetArrayElementAtIndex(arraySize);
            newFeatureEntry.objectReferenceValue = newFeature;

            mapProp.InsertArrayElementAtIndex(arraySize);
            SerializedProperty newMapEntry = mapProp.GetArrayElementAtIndex(arraySize);
            newMapEntry.longValue = localId;

            AssetDatabase.SaveAssetIfDirty(data);

            so.ApplyModifiedProperties();
        }

        private static int AssetDefaultRendererIndex(UniversalRenderPipelineAsset asset)
        {
            return (int)typeof(UniversalRenderPipelineAsset)
                .GetField("m_DefaultRendererIndex", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(asset);
        }

        private static ScriptableRendererData GetRendererDataForAsset(UniversalRenderPipelineAsset asset)
        {
            if (asset == null)
                return null;

            ScriptableRendererData[] dataArray = (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset)
                        .GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(asset);
            return dataArray[AssetDefaultRendererIndex(asset)];
        }
    }
}

#endif