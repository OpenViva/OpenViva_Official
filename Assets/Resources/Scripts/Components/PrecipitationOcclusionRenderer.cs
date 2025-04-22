using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OccaSoftware.Altos.Runtime
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class PrecipitationOcclusionRenderer : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset;
        public ScriptableRendererData data;

        [Min(0)]
        public int renderTextureResolution = 128;

        private RenderTexture renderTexture;

        private Camera cam;

        private void OnEnable()
        {
            cam = GetComponent<Camera>();
            cam.enabled = true;
            SetupRendererData();
            CreateRenderTexture();
            RenderPipelineManager.beginCameraRendering += OnBeginCamera;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
            CleanupRenderTexture();
            TeardownRendererData();
            cam.enabled = false;
        }

        private void OnBeginCamera(ScriptableRenderContext context, Camera cam)
        {
            if (cam == this.cam || target == null)
                return;

            // Update the position of the object
            transform.position = target.position + offset;
        }

        private void SetupRendererData()
        {
            cam.enabled = data != null;
            if (data == null)
                return;

            UniversalRenderPipelineAsset asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

            ScriptableRendererData[] rendererDataList = GetRendererDataList(asset);

            int dataIndex = FindRendererDataIndex(rendererDataList, data);

            if (dataIndex == -1)
            {
                dataIndex = AddRendererDataToList(asset, rendererDataList, data);
            }

            if (dataIndex != -1)
            {
                cam.GetUniversalAdditionalCameraData().SetRenderer(dataIndex);
            }
        }

        private void TeardownRendererData()
        {
            if (data == null)
                return;

            UniversalRenderPipelineAsset asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

            ScriptableRendererData[] rendererDataList = GetRendererDataList(asset);

            int dataIndex = FindRendererDataIndex(rendererDataList, data);

            if (dataIndex != -1)
            {
                // Remove the added renderer data from the list
                List<ScriptableRendererData> datas = new List<ScriptableRendererData>(rendererDataList);
                datas.RemoveAt(dataIndex);

                // Set the modified list back to the asset
                typeof(UniversalRenderPipelineAsset)
                    .GetField("m_RendererDataList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(asset, datas.ToArray());
            }

            cam.GetUniversalAdditionalCameraData().SetRenderer(0);
        }

        private ScriptableRendererData[] GetRendererDataList(UniversalRenderPipelineAsset asset)
        {
            return (ScriptableRendererData[])
                typeof(UniversalRenderPipelineAsset)
                    .GetField("m_RendererDataList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .GetValue(asset);
        }

        private int FindRendererDataIndex(ScriptableRendererData[] rendererDataList, ScriptableRendererData targetData)
        {
            for (int i = 0; i < rendererDataList.Length; i++)
            {
                if (rendererDataList[i] == targetData)
                {
                    return i;
                }
            }
            return -1;
        }

        private int AddRendererDataToList(
            UniversalRenderPipelineAsset asset,
            ScriptableRendererData[] rendererDataList,
            ScriptableRendererData newData
        )
        {
            List<ScriptableRendererData> datas = new List<ScriptableRendererData>(rendererDataList);
            datas.Add(newData);

            typeof(UniversalRenderPipelineAsset)
                .GetField("m_RendererDataList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(asset, datas.ToArray());

            return datas.Count - 1; // Index of the added data
        }

        private void CreateRenderTexture()
        {
            CleanupRenderTexture();

            renderTexture = new RenderTexture(renderTextureResolution, renderTextureResolution, 0, RenderTextureFormat.RHalf)
            {
                name = "PrecipitationCollisionTexture"
            };
            renderTexture.Create();
            cam.targetTexture = renderTexture;
        }

        private void CleanupRenderTexture()
        {
            if (renderTexture != null)
            {
                cam.targetTexture = null;
                renderTexture.Release();
                if (Application.isPlaying)
                {
                    // Destroy during Play Mode
                    Destroy(renderTexture);
                }
            }
        }
    }
}
