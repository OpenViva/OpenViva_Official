using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OccaSoftware.Altos.Runtime
{
    internal class SkyRenderPass : ScriptableRenderPass
    {
        private const string profilerTag = "Altos: Render Sky";
        AltosSkyDirector skyDirector;

        private StarRenderPass stars = null;

        private RTHandle skyTarget;

        private const string skyTargetId = "_SkyTexture";

        private Material atmosphereMaterial;
        private Material backgroundMaterial;
        
        public static int _Direction = Shader.PropertyToID("_Direction");
        public static int _Color = Shader.PropertyToID("_Color");
        public static int _Falloff = Shader.PropertyToID("_Falloff");
        public static int _Count = Shader.PropertyToID("_SkyObjectCount");

        public void Dispose()
        {
            skyTarget?.Release();
            skyTarget = null;

            CoreUtils.Destroy(atmosphereMaterial);
            CoreUtils.Destroy(backgroundMaterial);
            atmosphereMaterial = null;
            backgroundMaterial = null;
            stars?.Cleanup();
            stars = null;
        }

        public SkyRenderPass()
        {
            skyTarget = RTHandles.Alloc(Shader.PropertyToID(skyTargetId), name: skyTargetId);
        }

        public void Setup(AltosSkyDirector skyDirector)
        {
            this.skyDirector = skyDirector;
            if (atmosphereMaterial == null)
                atmosphereMaterial = CoreUtils.CreateEngineMaterial(skyDirector.data?.shaders.atmosphereShader);
            if (backgroundMaterial == null)
                backgroundMaterial = CoreUtils.CreateEngineMaterial(skyDirector.data?.shaders.backgroundShader);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderTextureDescriptor skyTargetDescriptor = cameraTextureDescriptor;
            skyTargetDescriptor.msaaSamples = 1;
            skyTargetDescriptor.depthBufferBits = 0;
            skyTargetDescriptor.width = (int)(skyTargetDescriptor.width * 0.125f);
            skyTargetDescriptor.width = Mathf.Max(skyTargetDescriptor.width, 1);
            skyTargetDescriptor.height = (int)(skyTargetDescriptor.height * 0.125f);
            skyTargetDescriptor.width = Mathf.Max(skyTargetDescriptor.height, 1);

            RenderingUtils.ReAllocateIfNeeded(ref skyTarget, skyTargetDescriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: skyTargetId);

            if (skyDirector.starDefinition != null)
            {
                if (stars == null)
                    stars = new StarRenderPass();
            }
        }

        private static int _MAX_SKY_OBJECT_COUNT = 8;
        Vector4[] directions = new Vector4[_MAX_SKY_OBJECT_COUNT];
        Vector4[] colors = new Vector4[_MAX_SKY_OBJECT_COUNT];
        float[] falloffs = new float[_MAX_SKY_OBJECT_COUNT];
        SkyObject skyObject = null;
        List<SkyObject> objectsToRemove = new List<SkyObject>();

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
            RTHandle source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            cmd.SetRenderTarget(source);

            #region Draw Background
            Matrix4x4 m = Matrix4x4.identity;
            m.SetTRS(renderingData.cameraData.worldSpaceCameraPos, Quaternion.identity, Vector3.one * renderingData.cameraData.camera.farClipPlane);
            cmd.DrawMesh(skyDirector.data.meshes.skyboxMesh, m, backgroundMaterial, 0);
            #endregion


            #region Draw Stars
            if (skyDirector.starDefinition != null)
            {
                stars.Setup(skyDirector);
                stars.Draw(cmd, skyDirector.skyDefinition);
            }
            #endregion


            #region Draw Sun and Moon
            int skyObjectCount = Mathf.Min(skyDirector.SkyObjects.Count, _MAX_SKY_OBJECT_COUNT);
            int validSkyObjectCount = 0;
            objectsToRemove.Clear();

            for (int i = 0; i < skyObjectCount; i++)
            {
                skyObject = skyDirector.SkyObjects[i];
                if (skyObject == null || !skyObject.isActiveAndEnabled)
                {
                    objectsToRemove.Add(skyObject);
                    continue;
                }

                m.SetTRS(
                    skyObject.positionRelative + renderingData.cameraData.worldSpaceCameraPos,
                    skyObject.GetRotation(),
                    Vector3.one * skyObject.CalculateSize()
                );

                cmd.DrawMesh(skyObject.Quad, m, skyObject.GetMaterial());
                directions[i] = skyObject.GetDirection();
                colors[i] = skyObject.GetColor();
                falloffs[i] = skyObject.GetFalloff();
                skyObject = null;
                validSkyObjectCount++;
            }

            foreach (SkyObject o in objectsToRemove)
            {
                skyDirector.SkyObjects.Remove(o);
            }

            cmd.SetGlobalVectorArray(_Direction, directions);
            cmd.SetGlobalVectorArray(_Color, colors);
            cmd.SetGlobalFloatArray(_Falloff, falloffs);
            cmd.SetGlobalInteger(_Count, validSkyObjectCount);
            #endregion


            #region Draw Sky
            if (skyDirector.skyDefinition != null)
            {
                m.SetTRS(
                    renderingData.cameraData.camera.transform.position,
                    Quaternion.identity,
                    Vector3.one * renderingData.cameraData.camera.farClipPlane
                );

                cmd.SetGlobalColor(ShaderParams._HorizonColor, skyDirector.skyDefinition.SkyColors.equatorColor);
                cmd.SetGlobalColor(ShaderParams._ZenithColor, skyDirector.skyDefinition.SkyColors.skyColor);
                cmd.DrawMesh(skyDirector.data.meshes.skyboxMesh, m, atmosphereMaterial, 0);

                // Draw Sky Target
                cmd.SetRenderTarget(skyTarget);
                cmd.ClearRenderTarget(true, true, Color.black);

                cmd.SetGlobalInteger(_Count, 0);
                cmd.DrawMesh(skyDirector.data.meshes.skyboxMesh, m, atmosphereMaterial, 0);
                cmd.SetGlobalInt(ShaderParams._HasSkyTexture, 1);
                cmd.SetGlobalTexture("_SkyTexture", skyTarget);

                cmd.SetRenderTarget(source, renderingData.cameraData.renderer.cameraDepthTargetHandle);
            }
            #endregion


            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        private static class ShaderParams
        {
            public static int _MainTex = Shader.PropertyToID("_MainTex");
            public static int _Color = Shader.PropertyToID("_Color");
            public static int _SunFalloff = Shader.PropertyToID("_SunFalloff");
            public static int _SunColor = Shader.PropertyToID("_SunColor");
            public static int _SunForward = Shader.PropertyToID("_SunForward");
            public static int _HorizonColor = Shader.PropertyToID("_HorizonColor");
            public static int _ZenithColor = Shader.PropertyToID("_ZenithColor");
            public static int _HasSkyTexture = Shader.PropertyToID("_HasSkyTexture");
        }
    }
}
