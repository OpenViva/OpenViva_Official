//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.Rendering;
#if _HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace JBooth.FoliageRendering
{
    [HelpURL("https://dicewrenchdesigns.com/category/assets/")]
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    public class HiZBuffer : MonoBehaviour
    {
        public enum Pass
        {
            Blit = 0,
            Reduce
        }

        public Shader hizShader;

        private static Shader shader;

        private static Material _material;
        public static Material material
        {
            get
            {
                if (_material == null)
                {
                    if (shader == null)
                    {
                        shader = Shader.Find("Hidden/FoliageRenderer/Hi-Z Buffer");
                    }
                    if (shader == null)
                    {
                        shader = Resources.Load<Shader>("FoliageRendererHiZBuffer");
                    }
                    if (shader == null)
                    {
                        Debug.Log("Hi-Z shader is not found");
                        return null;
                    }
                    if (shader.isSupported == false)
                    {
                        Debug.Log("Shader is found, but not supported");
                        return null;
                    }
                    _material = new Material(shader);
                }

                return _material;
            }
        }

        private Camera _camera;
        public Camera activeCamera
        {
            get
            {
                if (_camera == null)
                    _camera = GetComponent<Camera>();
                if (IndirectRenderer.hasInstance && IndirectRenderer.instance.cullingCamera != null)
                    return IndirectRenderer.instance.cullingCamera;
                return _camera;
            }
        }

#if _HDRP
        private HDCamera _hdCamera;
        public HDCamera HDCamera
        {
            get
            {
                if (_hdCamera == null)
                    _hdCamera = activeCamera.GetComponent<HDCamera>();
                return _hdCamera;
            }
        }
#endif

        [HideInInspector]
        public RenderTexture hizBuffer;
#if UNITY_EDITOR || _URP
        public RenderTexture texture
        {
            get
            {
                return hizBuffer;
            }
        }
#endif

        private CommandBuffer commandBuffer;

        private CameraEvent cameraEvent
        {
            get
            {
                if (Camera.main == null)
                    return CameraEvent.AfterDepthTexture;

                var renderingPath = Camera.main.actualRenderingPath;
                
                if (renderingPath == RenderingPath.Forward)
                {
                    if (activeCamera.depthTextureMode == DepthTextureMode.Depth)
                    {
                        return CameraEvent.AfterDepthTexture;
                    }
                    else if (activeCamera.depthTextureMode == DepthTextureMode.DepthNormals)
                    {
                        return CameraEvent.AfterDepthNormalsTexture;
                    }
                }
                else if (renderingPath == RenderingPath.DeferredShading)
                {
                    return CameraEvent.BeforeReflections;
                }
                
                return CameraEvent.BeforeReflections;
            }
        }

        void OnEnable()
        {
            if (activeCamera.depthTextureMode == DepthTextureMode.None)
               activeCamera.depthTextureMode = DepthTextureMode.Depth;
        }

        void OnDisable()
        {
            if (activeCamera != null)
            {
                if (commandBuffer != null)
                {
                    activeCamera.RemoveCommandBuffer(cameraEvent, commandBuffer);
                    commandBuffer = null;
                }
            }

            if (hizBuffer != null)
            {
                hizBuffer.Release();
                hizBuffer = null;
            }
        }

        public bool AllocTextureIfNeeded(int sizeX, int sizeY)
        {            
            if (hizBuffer == null || (hizBuffer.width != sizeX || hizBuffer.height != sizeY))
            {
                if (hizBuffer != null)
                    hizBuffer.Release();

                hizBuffer = new RenderTexture(sizeX, sizeY, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
                hizBuffer.name = "Hi-Z buffer";
                hizBuffer.filterMode = FilterMode.Point;

                hizBuffer.useMipMap = true;
                hizBuffer.autoGenerateMips = false;

                hizBuffer.Create();

                hizBuffer.hideFlags = HideFlags.HideAndDontSave;
                return true;
            }
            return false;
        }

#if !_HDRP
        private static int[] temporaries;
#endif
        public static void InitCommandBuffer(int lodCount, int sizeX, int sizeY,
            CommandBuffer commandBuffer, RenderTexture hizBuffer, Material material )
        {
            UnityEngine.Profiling.Profiler.BeginSample("HiZ Init");
#if !_HDRP
            if (temporaries == null || temporaries.Length != lodCount)
            {
                temporaries = new int[lodCount];
                for (int i = 0; i < lodCount; ++i)
                {
                    temporaries[i] = Shader.PropertyToID("_HIZ_Temporaries" + i.ToString());
                }
            }
            commandBuffer.name = "Hi-Z Buffer";
            commandBuffer.BeginSample("Hi-Z");
            RenderTargetIdentifier id = new RenderTargetIdentifier(hizBuffer);
            commandBuffer.Blit(null, id, material, (int)Pass.Blit);

            for (int i = 0; i < lodCount; ++i)
            {
                sizeX /= 2;
                sizeY /= 2;
                if (sizeX == 0)
                    sizeX = 1;
                if (sizeY == 0)
                    sizeY = 1;

                commandBuffer.GetTemporaryRT(temporaries[i], sizeX, sizeY, 0, FilterMode.Point, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);

                if (i == 0)
                    commandBuffer.Blit(id, temporaries[0], material, (int)Pass.Reduce);
                else
                    commandBuffer.Blit(temporaries[i - 1], temporaries[i], material, (int)Pass.Reduce);

                commandBuffer.CopyTexture(temporaries[i], 0, 0, id, 0, i + 1);

                if (i >= 1)
                    commandBuffer.ReleaseTemporaryRT(temporaries[i - 1]);
            }

            commandBuffer.ReleaseTemporaryRT(temporaries[lodCount - 1]);
            commandBuffer.SetGlobalTexture("_HiZTexture", hizBuffer);           
            commandBuffer.SetGlobalVector("_HiZTextureSize", new Vector4(hizBuffer.width, hizBuffer.height, 1.0f / hizBuffer.width, 1.0f / hizBuffer.height));
            commandBuffer.EndSample("Hi-Z");
#endif
            UnityEngine.Profiling.Profiler.EndSample();
        }

        //suppress "assigned but not used" warning 
        //since we don't need this in every RP
#pragma warning disable CS0414
        bool reInit = false;
        public void ReInit()
        {
            reInit = true;
        }
#pragma warning restore CS0414
        public void Render()
        {
            if (Camera.main == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Skipping HiZBuffer Render, no MainCamera Active.", this);
#endif
                return;
            }
#if !_HDRP
            int sizeX = activeCamera.pixelWidth;
            int sizeY = activeCamera.pixelHeight;
            int lodCount = (int)Mathf.Floor(Mathf.Log(Mathf.Max(sizeX, sizeY), 2f));
            if (lodCount == 0)
                return;

            bool isCommandBufferInvalid = AllocTextureIfNeeded(sizeX, sizeY);
            if (isCommandBufferInvalid || reInit)
            {
                reInit = false;
                if (commandBuffer != null)
                    activeCamera.RemoveCommandBuffer(cameraEvent, commandBuffer);
                commandBuffer = new CommandBuffer();
                InitCommandBuffer(lodCount, sizeX, sizeY, commandBuffer, hizBuffer, material);
                activeCamera.AddCommandBuffer(cameraEvent, commandBuffer);
            }
#endif
        }
    }
}
