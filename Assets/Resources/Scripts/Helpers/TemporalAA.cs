using System.Collections.Generic;

using UnityEngine;

using UnityEngine.Rendering.Universal;
using Viva;

namespace OccaSoftware.Altos.Runtime
{
    internal class TemporalAA
    {
        public TemporalAA()
        {
            // Constructor
        }

        private List<Camera> removeTargets = new List<Camera>();

        private Dictionary<Camera, TAACameraData> temporalData = new Dictionary<Camera, TAACameraData>();
        public Dictionary<Camera, TAACameraData> TemporalData
        {
            get => temporalData;
        }

        public void Cleanup()
        {
            CleanupDictionary();
        }

        public void Dispose()
        {
            foreach (KeyValuePair<Camera, TAACameraData> data in temporalData)
            {
                data.Value.Dispose();
            }
        }

        internal class TAACameraData
        {
            private int lastFrameUsed;
            private RenderTexture colorTexture;
            private string cameraName;
            private Matrix4x4 prevViewProj;

            public void Dispose()
            {
                if (colorTexture != null)
                {
                    colorTexture.Release();
                    colorTexture = null;
                }
            }

            public TAACameraData(int lastFrameUsed, RenderTexture colorTexture, string cameraName)
            {
                LastFrameUsed = lastFrameUsed;
                ColorTexture = colorTexture;
                CameraName = cameraName;
                prevViewProj = Matrix4x4.identity;
            }

            public int LastFrameUsed
            {
                get => lastFrameUsed;
                set => lastFrameUsed = value;
            }

            public RenderTexture ColorTexture
            {
                get => colorTexture;
                set => colorTexture = value;
            }

            public string CameraName
            {
                get => cameraName;
                set => cameraName = value;
            }

            public Matrix4x4 PrevViewProj
            {
                get => prevViewProj;
                set => prevViewProj = value;
            }
        }

        public bool IsTemporalDataValid(Camera camera, int sourceWidth, int sourceHeight)
        {
            if (temporalData.TryGetValue(camera, out TAACameraData cameraData))
            {
                bool isColorTexValid = IsRenderTextureValid(sourceWidth, sourceHeight, cameraData.ColorTexture);

                if (isColorTexValid)
                    return true;
            }

            return false;

            bool IsRenderTextureValid(int sourceWidth, int sourceHeight, RenderTexture rt)
            {
                if (rt == null)
                {
                    return false;
                }

                bool rtWrongSize = (rt.width != sourceWidth || rt.height != sourceHeight) ? true : false;
                if (rtWrongSize)
                {
                    return false;
                }

                return true;
            }
        }

        public void SetupTemporalData(Camera camera, RenderTextureDescriptor descriptor, int width, int height)
        {
            RenderTexture color = SetupColorTexture(camera, descriptor, width, height);

            if (temporalData.ContainsKey(camera))
            {
                if (temporalData[camera].ColorTexture != null)
                {
                    temporalData[camera].ColorTexture.Release();
                    temporalData[camera].ColorTexture = null;
                }

                temporalData[camera].ColorTexture = color;
            }
            else
            {
                temporalData.Add(camera, new TAACameraData(TimeManager.FrameCount, color, camera.name));
            }
        }

        private RenderTexture SetupColorTexture(Camera camera, RenderTextureDescriptor descriptor, int width, int height)
        {
            descriptor.colorFormat = RenderTextureFormat.DefaultHDR;
            descriptor.width = width;
            descriptor.height = height;

            descriptor.depthBufferBits = 24; // Do we need a depth buffer for this texture?
            descriptor.msaaSamples = 1;
            descriptor.useDynamicScale = false;

            RenderTexture renderTexture = new RenderTexture(descriptor);
            renderTexture.name = camera.name + " Color History";
            renderTexture.filterMode = FilterMode.Point;
            renderTexture.wrapMode = TextureWrapMode.Clamp;
            renderTexture.Create();

            RenderTexture activeTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            GL.Clear(true, true, new Color(0.0f, 0.0f, 0.0f, 1.0f));
            RenderTexture.active = activeTexture;
            
            return renderTexture;
        }

        private void CleanupDictionary()
        {
            removeTargets.Clear();
            foreach (KeyValuePair<Camera, TAACameraData> entry in temporalData)
            {
                if (entry.Value.LastFrameUsed < TimeManager.FrameCount - 10)
                {
                    if (entry.Value.ColorTexture != null)
                    {
                        entry.Value.ColorTexture.Release();
                        entry.Value.ColorTexture = null;
                    }

                    removeTargets.Add(entry.Key);
                }
            }

            for (int i = 0; i < removeTargets.Count; i++)
            {
                temporalData.Remove(removeTargets[i]);
            }
        }

        public struct ProjectionMatrices
        {
            public Matrix4x4 viewProjection;
            public Matrix4x4 prevViewProjection;
            public Matrix4x4 projection;
            public Matrix4x4 inverseProjection;
            public Matrix4x4 inverseViewProjection;
        }

        public ProjectionMatrices SetupMatrices(RenderingData renderingData)
        {
            ProjectionMatrices m;

            m.prevViewProjection = GetPreviousViewProjection(renderingData.cameraData.camera);
            m.projection = GL.GetGPUProjectionMatrix(renderingData.cameraData.camera.nonJitteredProjectionMatrix, true);
            //m.projection = renderingData.cameraData.camera.nonJitteredProjectionMatrix;
            m.inverseProjection = m.projection.inverse;

            var view = renderingData.cameraData.camera.worldToCameraMatrix;
            m.viewProjection = m.projection * view;

            m.inverseViewProjection = m.viewProjection.inverse;

            SetPreviousViewProjection(renderingData.cameraData.camera, m.viewProjection);

            return m;
        }

        public Matrix4x4 GetPreviousViewProjection(Camera camera)
        {
            if (temporalData.TryGetValue(camera, out TAACameraData data))
            {
                return data.PrevViewProj;
            }
            else
            {
                return Matrix4x4.identity;
            }
        }

        public void SetPreviousViewProjection(Camera camera, Matrix4x4 currentViewProjection)
        {
            if (temporalData.ContainsKey(camera))
            {
                temporalData[camera].PrevViewProj = currentViewProjection;
            }
        }
    }
}
