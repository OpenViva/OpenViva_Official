//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


#if _URP
using JBooth.FoliageRendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HiZURP : ScriptableRendererFeature
{
    class HizPass : ScriptableRenderPass
    {
        private string profilerTag;
        
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);
            var camera = renderingData.cameraData.camera;
            var hiz = camera.GetComponent<HiZBuffer>();
            if (hiz != null)
            {
                int sizeX = camera.pixelWidth;
                int sizeY = camera.pixelHeight;
                hiz.AllocTextureIfNeeded(sizeX, sizeY);
            }
            ConfigureInput(ScriptableRenderPassInput.Depth);
        }

        public HizPass(string tag)
        {
            profilerTag = tag;
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            var hiz = camera.GetComponent<HiZBuffer>();
            if (hiz != null)
            {
                CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
                using (new ProfilingScope(cmd, new ProfilingSampler(profilerTag)))
                {
                    // Set the depth texture globally so all shaders can access it
#if UNITY_2022_1_OR_NEWER

                    RenderTargetIdentifier depthTexture = renderingData.cameraData.renderer.cameraDepthTargetHandle;
#else
                    RenderTargetIdentifier depthTexture = renderingData.cameraData.renderer.cameraDepthTarget;
#endif
                    int sizeX = camera.pixelWidth;
                    int sizeY = camera.pixelHeight;
                    int lodCount = (int)Mathf.Floor(Mathf.Log(Mathf.Max(sizeX, sizeY), 2f));
                    HiZBuffer.InitCommandBuffer(lodCount, sizeX, sizeY, cmd, hiz.texture, HiZBuffer.material);

                }
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
            
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);
        }
    }

    HizPass hizPass;

    const string hizPassTag = "Hi-Z pass";

    public override void Create()
    {
        hizPass = new HizPass(hizPassTag);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(hizPass);
    }
}
#endif
