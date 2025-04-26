using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OccaSoftware.Altos.Runtime
{
    internal class AtmosphereBlendingPass : ScriptableRenderPass
    {
        AltosSkyDirector skyDirector;
        private Material blendingMaterial;
        private RTHandle blendingTarget;

        private const string blendingTargetId = "_AltosFogTarget";
        private const string profilerTag = "Altos: Atmosphere Blending";

        public AtmosphereBlendingPass()
        {
            blendingTarget = RTHandles.Alloc(Shader.PropertyToID(blendingTargetId), name: blendingTargetId);
        }

        public void Dispose()
        {
            blendingTarget?.Release();
            blendingTarget = null;
            CoreUtils.Destroy(blendingMaterial);
            blendingMaterial = null;
        }

        public void Setup(AltosSkyDirector skyDirector)
        {
            this.skyDirector = skyDirector;

            if (blendingMaterial == null)
                blendingMaterial = CoreUtils.CreateEngineMaterial(skyDirector.data.shaders.atmosphereBlending);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderTextureDescriptor rtDescriptor = cameraTextureDescriptor;
            rtDescriptor.msaaSamples = 1;
            rtDescriptor.depthBufferBits = 0;

            RenderingUtils.ReAllocateIfNeeded(ref blendingTarget, rtDescriptor, FilterMode.Point, TextureWrapMode.Clamp, name: blendingTargetId);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            RTHandle source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            cmd.SetGlobalFloat(ShaderParams._Density, skyDirector.atmosphereDefinition.GetDensity());
            cmd.SetGlobalFloat(ShaderParams._BlendStart, skyDirector.atmosphereDefinition.start);
            cmd.SetGlobalTexture(CloudShaderParamHandler.ShaderParams._ScreenTexture, source);
            Blitter.BlitCameraTexture(cmd, source, blendingTarget, blendingMaterial, 0);

            Blitter.BlitCameraTexture(cmd, blendingTarget, source);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public static class ShaderParams
        {
            public static int _Density = Shader.PropertyToID("_Density");
            public static int _BlendStart = Shader.PropertyToID("_BlendStart");
        }
    }
}
