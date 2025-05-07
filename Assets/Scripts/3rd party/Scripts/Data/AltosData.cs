using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System;
using UnityEngine.Rendering;

namespace OccaSoftware.Altos.Runtime
{
    [Serializable]
    public class AltosData : ScriptableObject
    {
        public static string packagePath = "Assets/3rd Party/Paid/com.occasoftware.altos";

        public ShaderResources shaders;
        public TextureResources textures;
        public MeshResources meshes;

        [Serializable]
        public sealed class ShaderResources
        {
            public Shader atmosphereShader;

            public Shader backgroundShader;

            public Shader skyObjectShader;

            public Shader starShader;

            public Shader ditherDepth;

            public Shader mergeClouds;

            public Shader renderClouds;

            public Shader edgeData;

            public Shader temporalIntegration;

            public Shader reproject;

            public Shader upscaleClouds;

            public Shader renderShadowsToScreen;

            public Shader screenShadows;

            public Shader atmosphereBlending;

            public Shader cloudMap;

            public Shader cloudShadowTaa;
        }

        [Serializable]
        public sealed class TextureResources
        {
            public Texture2D halton;

            public Texture2D[] blue;
        }

        [Serializable]
        public sealed class MeshResources
        {
            public Mesh skyboxMesh;
        }
    }
}
