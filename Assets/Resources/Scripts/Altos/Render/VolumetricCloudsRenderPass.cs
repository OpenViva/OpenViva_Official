using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OccaSoftware.Altos.Runtime
{
    internal class VolumetricCloudsRenderPass : ScriptableRenderPass
    {
        #region RT Handles
        private RTHandle cloudTarget;
        private RTHandle temporalTarget;
        private RTHandle upscaleHalfRes;
        private RTHandle mergeTarget;
        private RTHandle depthTex;
        private RTHandle reprojectTarget;
        #endregion

        #region Input vars
        private const string profilerTag = "altos_VolumetricCloudRenderPass";
        #endregion

        #region Shader Variable References
        private const string altosCloudTexture = "_altos_CloudTexture";
        private const string previousTaaResults = "_PREVIOUS_TAA_CLOUD_RESULTS";
        private const string depthId = "_DitheredDepthTex";
        #endregion

        #region Texture Ids
        private const string cloudId = "_CloudRenderPass";
        private const string upscaleHalfId = "_CloudUpscaleHalfResTarget";
        private const string taaId = "_CloudTemporalIntegration";
        private const string mergeId = "_CloudSceneMergeTarget";
        #endregion

        #region Materials
        private Material cloudTaa;
        private Material merge;
        private Material upscale;
        private Material ditherDepth;
        private Material reproject;
        #endregion


        // TAA Class
        private TemporalAA taa;

        public VolumetricCloudsRenderPass()
        {
            // Create TAA handler
            taa = new TemporalAA();

            // Setup RT Handles
            cloudTarget = RTHandles.Alloc(Shader.PropertyToID(cloudId), name: cloudId);
            upscaleHalfRes = RTHandles.Alloc(Shader.PropertyToID(upscaleHalfId), name: upscaleHalfId);
            temporalTarget = RTHandles.Alloc(Shader.PropertyToID(taaId), name: taaId);
            mergeTarget = RTHandles.Alloc(Shader.PropertyToID(mergeId), name: mergeId);
            depthTex = RTHandles.Alloc(Shader.PropertyToID(depthId), name: depthId);
            reprojectTarget = RTHandles.Alloc(Shader.PropertyToID("altos_ReprojectTarget"), name: "altos_ReprojectTarget");
        }

        public void Dispose()
        {
            CoreUtils.Destroy(merge);
            CoreUtils.Destroy(cloudTaa);
            CoreUtils.Destroy(upscale);
            CoreUtils.Destroy(ditherDepth);
            CoreUtils.Destroy(reproject);
            merge = null;
            cloudTaa = null;
            upscale = null;
            ditherDepth = null;
            reproject = null;

            reprojectTarget?.Release();
            cloudTarget?.Release();
            upscaleHalfRes?.Release();
            temporalTarget?.Release();
            mergeTarget?.Release();
            depthTex?.Release();

            reprojectTarget = null;
            cloudTarget = null;
            upscaleHalfRes = null;
            temporalTarget = null;
            mergeTarget = null;
            depthTex = null;

            taa?.Dispose();
            taa = null;
        }

        AltosSkyDirector skyDirector;
        Material cloudRenderMaterial;

        public void Setup(AltosSkyDirector skyDirector, Material cloudRenderMaterial)
        {
            this.skyDirector = skyDirector;
            this.cloudRenderMaterial = cloudRenderMaterial;

            // Setup Materials
            if (merge == null)
                merge = CoreUtils.CreateEngineMaterial(skyDirector.data.shaders.mergeClouds);
            if (cloudTaa == null)
                cloudTaa = CoreUtils.CreateEngineMaterial(skyDirector.data.shaders.temporalIntegration);
            if (upscale == null)
                upscale = CoreUtils.CreateEngineMaterial(skyDirector.data.shaders.upscaleClouds);
            if (ditherDepth == null)
                ditherDepth = CoreUtils.CreateEngineMaterial(skyDirector.data.shaders.ditherDepth);
            if (reproject == null)
                reproject = CoreUtils.CreateEngineMaterial(skyDirector.data.shaders.reproject);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            CloudShaderParamHandler.SetCloudMaterialSettings(cmd, skyDirector.cloudDefinition);

            RenderTextureDescriptor rtDescriptor = cameraTextureDescriptor;
            SetupTargets(rtDescriptor);
        }

        RenderTextureDescriptor cloudDescriptor;
        RenderTextureDescriptor reprojectDescriptor;
        RenderTextureDescriptor upscaleDescriptor;

        private void SetupTargets(RenderTextureDescriptor cameraDescriptor)
        {
            cameraDescriptor.msaaSamples = 1;
            cameraDescriptor.depthBufferBits = 0;
            cameraDescriptor.width = Mathf.Max(1, cameraDescriptor.width);
            cameraDescriptor.height = Mathf.Max(1, cameraDescriptor.height);
            cameraDescriptor.useDynamicScale = false;
            cameraDescriptor.colorFormat = RenderTextureFormat.DefaultHDR;
            var cloudScale = Scale.Full;
            if (skyDirector.cloudDefinition.useReprojection)
            {
                cloudScale++;
            }

            if (skyDirector.cloudDefinition.resolutionOptions == ResolutionOptions.Half)
            {
                cloudScale++;
            }

            cloudDescriptor = CreateDescriptor(cameraDescriptor, cloudScale);

            RenderingUtils.ReAllocateIfNeeded(ref cloudTarget, cloudDescriptor, FilterMode.Point, TextureWrapMode.Clamp, name: cloudId);

            if (skyDirector.cloudDefinition.useReprojection)
            {
                var reprojectionScale = cloudScale - 1;
                reprojectDescriptor = CreateDescriptor(cameraDescriptor, reprojectionScale);

                RenderingUtils.ReAllocateIfNeeded(
                    ref reprojectTarget,
                    reprojectDescriptor,
                    FilterMode.Point,
                    TextureWrapMode.Clamp,
                    name: "altos_ReprojectTarget"
                );
            }

            // Need upscale tex when rendering at half resolution.
            if (skyDirector.cloudDefinition.resolutionOptions == ResolutionOptions.Half)
            {
                // Need a downsampled depth tex when rendering at half resolution while respecting geometry.
                if (skyDirector.cloudDefinition.depthOptions == DepthCullOptions.RespectGeometry)
                {
                    RenderingUtils.ReAllocateIfNeeded(
                        ref depthTex,
                        CreateDescriptor(cameraDescriptor, Scale.Half, RenderTextureFormat.RFloat),
                        FilterMode.Point,
                        TextureWrapMode.Clamp,
                        name: depthId
                    );
                }

                upscaleDescriptor = cameraDescriptor;
                RenderingUtils.ReAllocateIfNeeded(
                    ref upscaleHalfRes,
                    upscaleDescriptor,
                    FilterMode.Point,
                    TextureWrapMode.Clamp,
                    name: upscaleHalfId
                );
            }

            if (skyDirector.cloudDefinition.useTemporalDenoising)
            {
                RenderingUtils.ReAllocateIfNeeded(ref temporalTarget, cameraDescriptor, FilterMode.Point, TextureWrapMode.Clamp, name: taaId);
            }

            RenderingUtils.ReAllocateIfNeeded(ref mergeTarget, cameraDescriptor, FilterMode.Point, TextureWrapMode.Clamp, name: mergeId);
        }

        // Simplifies bit shift operation (0 // 1 // 2 shifts correspond to Full / Half / Quarter res).
        private enum Scale
        {
            Full,
            Half,
            Quarter
        }

        private RenderTextureDescriptor CreateDescriptor(
            RenderTextureDescriptor source,
            Scale scale = Scale.Full,
            RenderTextureFormat format = RenderTextureFormat.DefaultHDR
        )
        {
            RenderTextureDescriptor d = source;
            d.width >>= (int)scale;
            d.height >>= (int)scale;
            d.colorFormat = format;
            return d;
        }

        RTHandle source;
        Vector3 cameraPosition;
        bool isTemporalDataValid;
        int frameId;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            TimeManager.Update();
            source = renderingData.cameraData.renderer.cameraColorTargetHandle;
            cameraPosition = renderingData.cameraData.worldSpaceCameraPos;

            TemporalAA.ProjectionMatrices matrices = taa.SetupMatrices(renderingData);
            cmd.SetGlobalMatrix(CloudShaderParamHandler.ShaderParams._ViewProjM, matrices.viewProjection);
            cmd.SetGlobalMatrix(CloudShaderParamHandler.ShaderParams._PrevViewProjM, matrices.prevViewProjection);
            cmd.SetGlobalMatrix("_InverseViewProjM", matrices.inverseViewProjection);
            cmd.SetGlobalVector("_RenderScale", new Vector4(1f, 1f, 0f, 0f));

            cmd.SetGlobalInt("_UseDownscaledDepth", skyDirector.cloudDefinition.resolutionOptions == ResolutionOptions.Half ? 1 : 0);
            cmd.SetGlobalInt("_UseReprojection", skyDirector.cloudDefinition.useReprojection ? 1 : 0);
            cmd.SetGlobalInt("_UseDepth", skyDirector.cloudDefinition.depthOptions == DepthCullOptions.RespectGeometry ? 1 : 0);

            // For both TAA and Reprojection, we need a previous frame (or info about the previous frame).
            if (skyDirector.cloudDefinition.useTemporalDenoising || skyDirector.cloudDefinition.useReprojection)
            {
                int width = renderingData.cameraData.cameraTargetDescriptor.width;
                int height = renderingData.cameraData.cameraTargetDescriptor.height;

                if (!skyDirector.cloudDefinition.useTemporalDenoising && skyDirector.cloudDefinition.resolutionOptions == ResolutionOptions.Half)
                {
                    width >>= 1;
                    height >>= 1;
                }

                isTemporalDataValid = taa.IsTemporalDataValid(renderingData.cameraData.camera, width, height);

                if (!isTemporalDataValid)
                {
                    frameId = 0;
                    taa.SetupTemporalData(renderingData.cameraData.camera, renderingData.cameraData.cameraTargetDescriptor, width, height);
                    CloudShaderParamHandler.IgnoreTAAThisFrame(cmd);
                }
                else
                {
                    CloudShaderParamHandler.ConfigureTAAParams(cmd, skyDirector.cloudDefinition);
                    cmd.SetGlobalTexture("_PreviousFrame", taa.TemporalData[renderingData.cameraData.camera].ColorTexture);
                    taa.TemporalData[renderingData.cameraData.camera].LastFrameUsed = TimeManager.FrameCount;
                }

                cmd.SetGlobalInt("_IsFirstFrame", frameId < 1 ? 1 : 0);
                frameId++;
            }

            SetupGlobalShaderParams(cmd);

            // Downscale Depth
            if (
                skyDirector.cloudDefinition.resolutionOptions == ResolutionOptions.Half
                && skyDirector.cloudDefinition.depthOptions == DepthCullOptions.RespectGeometry
            )
            {
                cmd.SetRenderTarget(depthTex.nameID);
                Blitter.BlitTexture(cmd, new Vector4(1, 1, 0, 0), ditherDepth, 0);
                cmd.SetGlobalTexture(CloudShaderParamHandler.ShaderParams._DitheredDepthTexture, depthTex.nameID);
            }

            RenderTargetIdentifier activeTarget;
            // Render clouds
            activeTarget = RenderClouds(cmd);

            // Reproject
            if (skyDirector.cloudDefinition.useReprojection)
            {
                activeTarget = Reproject(cmd, renderingData.cameraData.camera, activeTarget);
            }

            // Upscale clouds
            if (skyDirector.cloudDefinition.resolutionOptions == ResolutionOptions.Half)
            {
                activeTarget = UpscaleClouds(cmd, activeTarget);
            }

            // Temporal AA
            if (skyDirector.cloudDefinition.useTemporalDenoising)
            {
                activeTarget = TemporalAntiAliasing(cmd, renderingData.cameraData.camera, activeTarget);
            }

            // Merge
            Merge(cmd, activeTarget);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        RenderTargetIdentifier Reproject(CommandBuffer cmd, Camera camera, RenderTargetIdentifier input)
        {
            // Calculate render texture dimensions
            Vector4 rtDimensions = new Vector4(
                1f / reprojectDescriptor.width,
                1f / reprojectDescriptor.height,
                reprojectDescriptor.width,
                reprojectDescriptor.height
            );
            cmd.SetGlobalVector(CloudShaderParamHandler.ShaderParams._RenderTextureDimensions, rtDimensions);

            // Set render scale based on cloud resolution options
            Vector4 renderScale =
                skyDirector.cloudDefinition.resolutionOptions == ResolutionOptions.Half ? new Vector4(0.5f, 2f, 0f, 0f) : new Vector4(1f, 1f, 0f, 0f);
            cmd.SetGlobalVector("_RenderScale", renderScale);

            cmd.SetGlobalFloat("_CloudTextureRenderScale", skyDirector.cloudDefinition.resolutionOptions == ResolutionOptions.Full ? 0.5f : 0.25f);

            // Set global textures for the reproject operation
            cmd.SetGlobalTexture("_PreviousFrame", taa.TemporalData[camera]?.ColorTexture);
            cmd.SetGlobalTexture("_CurrentFrame", input);

            // Perform the blit operation
            Blitter.BlitCameraTexture(cmd, source, reprojectTarget, reproject, 0);

            // Copy texture if not using Temporal Anti-Aliasing
            if (!skyDirector.cloudDefinition.useTemporalDenoising)
            {
                // Ensure that the source and destination textures are valid before copying
                if (taa.TemporalData[camera]?.ColorTexture != null && reprojectTarget != null)
                {
                    cmd.CopyTexture(reprojectTarget, new RenderTargetIdentifier(taa.TemporalData[camera].ColorTexture));
                }
                else
                {
                    // Handle potential errors or log a warning
                    Debug.LogWarning("Texture copy skipped due to null textures.");
                }
            }

            return reprojectTarget;
        }

        void SetupGlobalShaderParams(CommandBuffer cmd)
        {

            cmd.SetGlobalTexture(CloudShaderParamHandler.ShaderParams._Halton_23_Sequence, skyDirector.data.textures.halton);
            cmd.SetGlobalTexture(CloudShaderParamHandler.ShaderParams._BLUE_NOISE, skyDirector.data.textures.blue[Time.renderedFrameCount % 64]);
            cmd.SetGlobalInt(CloudShaderParamHandler.ShaderParams._FrameId, TimeManager.FrameCount);
            cmd.SetGlobalVector(CloudShaderParamHandler.ShaderParams._MainCameraOrigin, cameraPosition);
            cmd.SetGlobalFloat(CloudShaderParamHandler.ShaderParams.Shadows._CloudShadowStrength, skyDirector.cloudDefinition.shadowStrength);
        }

        RenderTargetIdentifier RenderClouds(CommandBuffer cmd)
        {
            cmd.SetGlobalInt(CloudShaderParamHandler.ShaderParams._ShadowPass, 0);

            cmd.SetGlobalVector(
                CloudShaderParamHandler.ShaderParams._RenderTextureDimensions,
                new Vector4(1f / cloudDescriptor.width, 1f / cloudDescriptor.height, cloudDescriptor.width, cloudDescriptor.height)
            );

            cmd.SetRenderTarget(cloudTarget.nameID);

            Blitter.BlitTexture(cmd, new Vector4(1, 1, 0, 0), cloudRenderMaterial, 0);
            cmd.SetGlobalTexture("_CurrentFrame", cloudTarget);
            cmd.SetGlobalTexture(altosCloudTexture, cloudTarget);

            return cloudTarget.nameID;
        }

        RenderTargetIdentifier UpscaleClouds(CommandBuffer cmd, RenderTargetIdentifier input)
        {
            cmd.SetGlobalVector(
                CloudShaderParamHandler.ShaderParams._RenderTextureDimensions,
                new Vector4(1f / upscaleDescriptor.width, 1f / upscaleDescriptor.height, upscaleDescriptor.width, upscaleDescriptor.height)
            );
            cmd.SetGlobalTexture(CloudShaderParamHandler.ShaderParams._ScreenTexture, input);
            Blitter.BlitCameraTexture(cmd, source, upscaleHalfRes, upscale, 0);

            return upscaleHalfRes;
        }

        RenderTargetIdentifier TemporalAntiAliasing(CommandBuffer cmd, Camera camera, RenderTargetIdentifier taaInput)
        {
            cmd.SetGlobalTexture(altosCloudTexture, taaInput);
            cmd.SetGlobalTexture(previousTaaResults, taa.TemporalData[camera].ColorTexture);
            cmd.SetGlobalTexture("_CURRENT_TAA_FRAME", taaInput);

            Blitter.BlitCameraTexture(cmd, source, temporalTarget, cloudTaa, 0);
            cmd.CopyTexture(temporalTarget, new RenderTargetIdentifier(taa.TemporalData[camera].ColorTexture));

            return temporalTarget;
        }

        void Merge(CommandBuffer cmd, RenderTargetIdentifier cloudTexture)
        {
            cmd.SetGlobalTexture(altosCloudTexture, cloudTexture);
            cmd.SetGlobalTexture(CloudShaderParamHandler.ShaderParams._ScreenTexture, source);
            Blitter.BlitCameraTexture(cmd, source, mergeTarget, merge, 0);
            Blitter.BlitCameraTexture(cmd, mergeTarget, source);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            taa.Cleanup();
        }
    }
}
