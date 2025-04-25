using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OccaSoftware.Altos.Runtime
{
    internal class CloudShadowsRenderPass : ScriptableRenderPass
    {
        private AltosSkyDirector skyDirector;
        private RTHandle shadowmapTarget;
        private RTHandle screenShadowsTarget;
        private RTHandle mergeTarget;
        private RTHandle cloudShadowTemporalAA;

        private const string shadowmapId = "_CloudShadowmap";
        private const string screenShadowsId = "_CloudScreenShadows";
        private const string mergeId = "_CloudShadowsOnScreen";

        private RenderTexture cloudShadowTaaTexture;

        private Material screenShadows;
        private Material shadowsToScreen;
        private Material cloudShadowTaaMaterial;

        private const string profilerTag = "Altos: Cloud Shadows";

        public CloudShadowsRenderPass()
        {
            shadowmapTarget = RTHandles.Alloc(Shader.PropertyToID(shadowmapId), name: shadowmapId);
            screenShadowsTarget = RTHandles.Alloc(Shader.PropertyToID(screenShadowsId), name: screenShadowsId);
            mergeTarget = RTHandles.Alloc(Shader.PropertyToID(mergeId), name: mergeId);
            cloudShadowTemporalAA = RTHandles.Alloc("_CloudShadowTAAHistory", name: "_CloudShadowTAAHistory");
        }

        public void Dispose()
        {
            shadowmapTarget?.Release();
            screenShadowsTarget?.Release();
            mergeTarget?.Release();
            cloudShadowTaaTexture?.Release();
            cloudShadowTemporalAA?.Release();

            shadowmapTarget = null;
            screenShadowsTarget = null;
            mergeTarget = null;
            cloudShadowTaaTexture = null;
            cloudShadowTemporalAA = null;

            CoreUtils.Destroy(screenShadows);
            CoreUtils.Destroy(shadowsToScreen);

            screenShadows = null;
            shadowsToScreen = null;
        }

        public void Setup(AltosSkyDirector skyDirector, Material cloudRenderMaterial)
        {
            this.skyDirector = skyDirector;
            this.cloudRenderMaterial = cloudRenderMaterial;

            if (screenShadows == null)
                screenShadows = CoreUtils.CreateEngineMaterial(skyDirector.data.shaders.screenShadows);
            if (shadowsToScreen == null)
                shadowsToScreen = CoreUtils.CreateEngineMaterial(skyDirector.data.shaders.renderShadowsToScreen);
            if (cloudShadowTaaMaterial == null)
                cloudShadowTaaMaterial = CoreUtils.CreateEngineMaterial(skyDirector.data.shaders.cloudShadowTaa);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderTextureDescriptor rtDescriptor = cameraTextureDescriptor;
            rtDescriptor.msaaSamples = 1;
            rtDescriptor.depthBufferBits = 0;
            rtDescriptor.width = Mathf.Max(1, rtDescriptor.width);
            rtDescriptor.height = Mathf.Max(1, rtDescriptor.height);
            rtDescriptor.useDynamicScale = false;
            rtDescriptor.colorFormat = RenderTextureFormat.DefaultHDR;

            ConfigureShadows(rtDescriptor);
            ConfigureScreenShadows(rtDescriptor);

            RenderingUtils.ReAllocateIfNeeded(ref mergeTarget, rtDescriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: mergeId);

            void ConfigureShadows(RenderTextureDescriptor descriptor)
            {
                if (skyDirector.cloudDefinition.castShadowsEnabled)
                {
                    descriptor.width = (int)skyDirector.cloudDefinition.shadowResolution;
                    descriptor.height = (int)skyDirector.cloudDefinition.shadowResolution;
                    descriptor.colorFormat = RenderTextureFormat.DefaultHDR;

                    RenderingUtils.ReAllocateIfNeeded(ref shadowmapTarget, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: shadowmapId);

                    if (
                        cloudShadowTaaTexture == null
                        || cloudShadowTaaTexture.width != descriptor.width
                        || cloudShadowTaaTexture.height != descriptor.height
                    )
                    {
                        cloudShadowTaaTexture = new RenderTexture(descriptor);
                        cloudShadowTaaTexture.Create();
                    }

                    RenderingUtils.ReAllocateIfNeeded(ref cloudShadowTemporalAA, descriptor, name: "_CloudShadowTAAHistory");
                }
            }

            void ConfigureScreenShadows(RenderTextureDescriptor descriptor)
            {
                RenderingUtils.ReAllocateIfNeeded(
                    ref screenShadowsTarget,
                    descriptor,
                    FilterMode.Bilinear,
                    TextureWrapMode.Clamp,
                    name: screenShadowsId
                );
            }
        }

        private struct SunData
        {
            public Vector3 forward;
            public Vector3 right;
            public Vector3 up;
        }

        bool GetSunData(out SunData sunData)
        {
            sunData = new SunData();

            if (skyDirector?.Sun != null)
            {
                Transform child = skyDirector.Sun.GetChild();
                sunData.forward = child.forward;
                sunData.up = child.up;
                sunData.right = child.right;
                return true;
            }
            else
            {
                SetMainLightShaderProperties setMainLightShaderProperties = SetMainLightShaderProperties.Instance;
                if (setMainLightShaderProperties != null)
                {
                    sunData.forward = setMainLightShaderProperties.transform.forward;
                    sunData.up = setMainLightShaderProperties.transform.up;
                    sunData.right = setMainLightShaderProperties.transform.right;
                    return true;
                }
            }
            return false;
        }

        Vector3 Div(Vector3 a, float b)
        {
            return new Vector3(a.x / b, a.y / b, a.z / b);
        }

        Vector3 Floor(Vector3 i)
        {
            return new Vector3(Mathf.Floor(i.x), Mathf.Floor(i.y), Mathf.Floor(i.z));
        }

        Matrix4x4 projectionMatrix = new Matrix4x4();
        Matrix4x4 viewMatrix = new Matrix4x4();
        Matrix4x4 worldToShadowMatrix = new Matrix4x4();
        Vector4 shadowCameraPosition = new Vector4();
        private Material cloudRenderMaterial;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            RTHandle source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            if (GetSunData(out SunData sunData))
            {
                RenderShadows(cmd, renderingData, sunData);
                DrawScreenSpaceShadows(cmd);
                if (skyDirector.cloudDefinition.screenShadows)
                {
                    RenderToScreen(cmd);
                }
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
            
            void RenderShadows(CommandBuffer cmd, RenderingData renderingData, SunData sunData)
            {
                float zFar = 60000f;

                float halfWidth = skyDirector.cloudDefinition.shadowDistance;

                Vector3 sourcePosition = Vector3.zero;
                Vector3 shadowCasterCameraPosition = sourcePosition - sunData.forward * zFar * 0.5f;

                // Prevent shimmering when the camera moves
                // Might be unnecessary now that we switched to TAA?
                Vector3 min = shadowCasterCameraPosition - halfWidth * sunData.right - halfWidth * sunData.up;

                Vector3 max = shadowCasterCameraPosition + sunData.forward * zFar + halfWidth * sunData.right + halfWidth * sunData.up;

                float radius = (new Vector2(max.x, max.z) - new Vector2(min.x, min.z)).magnitude / 2f;
                float texelSize = radius / (0.25f * (int)skyDirector.cloudDefinition.shadowResolution);

                sourcePosition = Floor(Div(sourcePosition, texelSize));
                sourcePosition *= texelSize;

                shadowCasterCameraPosition = sourcePosition - sunData.forward * zFar * 0.5f;

                // Setup matrices
                viewMatrix = MatrixHandler.SetupViewMatrix(shadowCasterCameraPosition, sunData.forward, zFar, sunData.up);
                shadowCameraPosition = shadowCasterCameraPosition;
                projectionMatrix = MatrixHandler.SetupProjectionMatrix(halfWidth, zFar);

                cmd.SetGlobalFloat(CloudShaderParamHandler.ShaderParams.Shadows._ShadowRadius, halfWidth);

                cmd.SetGlobalVector(
                    CloudShaderParamHandler.ShaderParams.Shadows._CloudShadowOrthoParams,
                    new Vector4(halfWidth * 2, halfWidth * 2, zFar, 0)
                );

                cmd.SetGlobalVector(CloudShaderParamHandler.ShaderParams.Shadows._ShadowCasterCameraPosition, shadowCameraPosition);
                worldToShadowMatrix = MatrixHandler.ConvertToWorldToShadowMatrix(projectionMatrix, viewMatrix);

                cmd.SetGlobalMatrix(CloudShaderParamHandler.ShaderParams.Shadows._CloudShadow_WorldToShadowMatrix, worldToShadowMatrix);
                cmd.SetGlobalFloat(CloudShaderParamHandler.ShaderParams.Shadows._ShadowRenderStepCount, skyDirector.cloudDefinition.shadowStepCount);
                cmd.SetGlobalFloat(CloudShaderParamHandler.ShaderParams.Shadows._CloudShadowDistance, skyDirector.cloudDefinition.shadowDistance);
                cmd.SetGlobalVector(CloudShaderParamHandler.ShaderParams.Shadows._ShadowCasterCameraForward, sunData.forward);
                cmd.SetGlobalVector(CloudShaderParamHandler.ShaderParams.Shadows._ShadowCasterCameraUp, sunData.up);
                cmd.SetGlobalVector(CloudShaderParamHandler.ShaderParams.Shadows._ShadowCasterCameraRight, sunData.right);
                cmd.SetGlobalInt(CloudShaderParamHandler.ShaderParams._ShadowPass, 1);
                cmd.SetGlobalVector(
                    CloudShaderParamHandler.ShaderParams._RenderTextureDimensions,
                    new Vector4(
                        1f / (int)skyDirector.cloudDefinition.shadowResolution,
                        1f / (int)skyDirector.cloudDefinition.shadowResolution,
                        (int)skyDirector.cloudDefinition.shadowResolution,
                        (int)skyDirector.cloudDefinition.shadowResolution
                    )
                );

                cmd.SetGlobalVector(
                    CloudShaderParamHandler.ShaderParams.Shadows._ShadowmapResolution,
                    new Vector4(
                        (int)skyDirector.cloudDefinition.shadowResolution,
                        (int)skyDirector.cloudDefinition.shadowResolution,
                        1f / (int)skyDirector.cloudDefinition.shadowResolution,
                        1f / (int)skyDirector.cloudDefinition.shadowResolution
                    )
                );

                Blitter.BlitCameraTexture(cmd, source, shadowmapTarget, cloudRenderMaterial, 0);
                cmd.SetGlobalTexture("_PREVIOUS_TAA_CLOUD_RESULTS", cloudShadowTaaTexture);
                cmd.SetGlobalTexture("_CURRENT_TAA_FRAME", shadowmapTarget);
                Blitter.BlitCameraTexture(cmd, source, cloudShadowTemporalAA, cloudShadowTaaMaterial, 0);
                cmd.CopyTexture(cloudShadowTemporalAA, new RenderTargetIdentifier(cloudShadowTaaTexture));
                cmd.SetGlobalTexture(CloudShaderParamHandler.ShaderParams.Shadows._CloudShadowmap, cloudShadowTemporalAA);
            }
            
            void DrawScreenSpaceShadows(CommandBuffer cmd)
            {
                cmd.SetGlobalInt(CloudShaderParamHandler.ShaderParams._CastScreenCloudShadows, skyDirector.cloudDefinition.screenShadows ? 1 : 0);
                cmd.SetGlobalTexture(CloudShaderParamHandler.ShaderParams._ScreenTexture, source);
                Blitter.BlitCameraTexture(cmd, source, screenShadowsTarget, screenShadows, 0);
            }
            void RenderToScreen(CommandBuffer cmd)
            {
                cmd.SetGlobalTexture(CloudShaderParamHandler.ShaderParams._ScreenTexture, source);
                cmd.SetGlobalTexture(CloudShaderParamHandler.ShaderParams.Shadows._CloudScreenShadows, screenShadowsTarget);
                Blitter.BlitCameraTexture(cmd, source, mergeTarget, shadowsToScreen, 0);
                Blitter.BlitCameraTexture(cmd, mergeTarget, source);
            }
        }
    }
}
