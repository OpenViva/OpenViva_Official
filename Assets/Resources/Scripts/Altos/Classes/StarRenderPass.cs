using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Viva.Util;

namespace OccaSoftware.Altos.Runtime
{
    internal class StarRenderPass
    {
        private AltosSkyDirector skyDirector;
        private Material starMaterial;
        private int initialSeed = 1;

        private Texture2D starTexture;
        private ComputeBuffer meshPropertiesBuffer = null;
        private ComputeBuffer argsBuffer = null;
        private bool initialized = false;

        private Material GetStarMaterial(AltosSkyDirector skyDirector)
        {
            if (starMaterial == null)
            {
                starMaterial = CoreUtils.CreateEngineMaterial(skyDirector.data.shaders.starShader);
            }

            return starMaterial;
        }

        private Mesh mesh = null;
        private Mesh Mesh
        {
            get
            {
                if (mesh == null)
                    mesh = Tools.CreateQuad();

                return mesh;
            }
        }

        private Texture2D white = null;
        private Texture2D White
        {
            get
            {
                if (white == null)
                {
                    white = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    white.SetPixel(0, 0, Color.white);
                    white.Apply();
                }

                return white;
            }
        }

        public StarRenderPass() { }

        public void Setup(AltosSkyDirector skyDirector)
        {
            this.skyDirector = skyDirector;
        }

        private void Init()
        {
            InitializeBuffers();
        }

        public void Cleanup()
        {
            argsBuffer?.Release();
            argsBuffer = null;
            meshPropertiesBuffer?.Release();
            meshPropertiesBuffer = null;

            if (white != null)
            {
                CoreUtils.Destroy(white);
                white = null;
            }

            if (mesh != null)
            {
                CoreUtils.Destroy(mesh);
                mesh = null;
            }

            if (starMaterial != null)
            {
                CoreUtils.Destroy(starMaterial);
                starMaterial = null;
            }
        }

        private void InitializeBuffers()
        {
            if (skyDirector.starDefinition == null)
                return;

            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = Mesh.GetIndexCount(0);
            args[1] = (uint)skyDirector.starDefinition.count;

            if (argsBuffer == null)
            {
                argsBuffer?.Release();
                argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            }

            argsBuffer.SetData(args);

            // Initialize buffer with the given population.
            MeshProperties[] meshPropertiesArray = new MeshProperties[skyDirector.starDefinition.count];
            UnityEngine.Random.InitState(initialSeed);
            for (int i = 0; i < skyDirector.starDefinition.count; i++)
            {
                MeshProperties starProperties = new MeshProperties();
                Vector3 position = UnityEngine.Random.onUnitSphere * 100f;
                Quaternion rotation = Quaternion.LookRotation(Vector3.zero - position, UnityEngine.Random.onUnitSphere);
                Vector3 scale = Vector3.one * UnityEngine.Random.Range(1f, 2f) * 0.1f * skyDirector.starDefinition.size;

                starProperties.mat = Matrix4x4.TRS(position, rotation, scale);
                starProperties.color = new Vector3(1, 1, 1);
                starProperties.brightness = Mathf.Pow(100, Random.Range(0f, 1f)) * 0.01f;


                starProperties.id = UnityEngine.Random.Range(0f, 1f);
                meshPropertiesArray[i] = starProperties;
            }

            if (meshPropertiesBuffer == null || meshPropertiesBuffer.count != skyDirector.starDefinition.count)
            {
                meshPropertiesBuffer?.Release();
                meshPropertiesBuffer = new ComputeBuffer(skyDirector.starDefinition.count, MeshProperties.Size());
            }

            meshPropertiesBuffer.SetData(meshPropertiesArray);
            Shader.SetGlobalBuffer("altos_StarBuffer", meshPropertiesBuffer);
        }

        public void Draw(CommandBuffer cmd, SkyDefinition skyboxDefinition)
        {
            if (!initialized || skyDirector.starDefinition.IsDirty())
            {
                Init();
                initialized = true;
            }

            SetProperties(cmd, skyboxDefinition);

            cmd.DrawMeshInstancedIndirect(Mesh, 0, GetStarMaterial(skyDirector), -1, argsBuffer);
        }

        private void SetProperties(CommandBuffer cmd, SkyDefinition skyboxDefinition)
        {
            cmd.SetGlobalFloat(ShaderParams._EarthTime, skyboxDefinition.CurrentTime);
            cmd.SetGlobalFloat(ShaderParams._Brightness, GetStarBrightness());
            cmd.SetGlobalFloat(ShaderParams._FlickerFrequency, skyDirector.starDefinition.flickerFrequency);
            cmd.SetGlobalFloat(ShaderParams._FlickerStrength, skyDirector.starDefinition.flickerStrength);
            cmd.SetGlobalFloat(ShaderParams._Inclination, -skyDirector.starDefinition.inclination);
            cmd.SetGlobalColor(ShaderParams._StarColor, skyDirector.starDefinition.color);
            starTexture = skyDirector.starDefinition.texture == null ? White : skyDirector.starDefinition.texture;
            cmd.SetGlobalTexture(ShaderParams._MainTex, starTexture);
            //cmd.SetGlobalBuffer(ShaderParams._Properties, meshPropertiesBuffer);
        }

        private float GetStarBrightness()
        {
            float brightness = Mathf.Lerp(skyDirector.starDefinition.brightness, skyDirector.starDefinition.dayBrightness, skyDirector.daytimeFactor);
            return brightness;
        }

        private struct MeshProperties
        {
            public Matrix4x4 mat;
            public Vector3 color;
            public float brightness;
            public float id;

            public static int Size()
            {
                return sizeof(float) * 4 * 4
                    + // matrix
                    sizeof(float) * 3
                    + // color
                    sizeof(float)
                    + // brightness
                    sizeof(float); // id
            }
        }

        private static class ShaderParams
        {
            public static int _EarthTime = Shader.PropertyToID("_EarthTime");
            public static int _Brightness = Shader.PropertyToID("_Brightness");
            public static int _FlickerFrequency = Shader.PropertyToID("_FlickerFrequency");
            public static int _FlickerStrength = Shader.PropertyToID("_FlickerStrength");
            public static int _MainTex = Shader.PropertyToID("_MainTex");
            public static int _Properties = Shader.PropertyToID("_Properties");
            public static int _Inclination = Shader.PropertyToID("_Inclination");
            public static int _StarColor = Shader.PropertyToID("_StarColor");
        }
    }
}
