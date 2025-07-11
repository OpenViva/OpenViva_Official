//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using UnityEngine;
using UnityEngine.Serialization;

namespace JBooth.FoliageRendering
{
    [HelpURL("https://dicewrenchdesigns.com/category/assets/")]
    [ExecuteInEditMode]
    public class WindController : MonoBehaviour
    {
        private const float _SIZE_SCALAR = 0.2777f;

        [Tooltip("Wind Speed in Kilometers per hour")]
        public float windSpeed = 30;
        [Range(0.0f, 2.0f)]
        [Tooltip("Wind Turbulence in percentage of wind Speed")]
        public float turbulence = 0.25f;

        [Tooltip("Texture used for wind turbulence")]
        public Texture2D noiseTexture;
        [Tooltip("Size of one world tiling patch of the Noise Texture, for bending trees")]
        public float bendingWorldSize = 175.0f;
        [Tooltip("Size of one world tiling patch of the Noise Texture, for leaf shivering")]
        public float leafWorldSize = 10.0f;

        [Tooltip("Texture used for wind gusts")]
        public Texture2D gustTexture;
        [Tooltip("Size of one world tiling patch of the Gust Texture, for leaf shivering")]
        public float gustWorldSize = 600.0f;

        [Tooltip("Wind Gust Speed in Kilometers per hour")]
        [FormerlySerializedAs("gistSpeed")]
        public float gustSpeed = 50;
        [Tooltip("Wind Gust Influence on trees")]
        public float gustScale = 1.0f;

        [Tooltip("Wind Gust Influence on trees")]
        public WindZone point1;
        [Tooltip("Wind Gust Influence on trees")]
        public WindZone point2;
        [Tooltip("Wind Gust Influence on trees")]
        public WindZone point3;
        [Tooltip("Wind Gust Influence on trees")]
        public WindZone point4;
        [Tooltip("Wind Gust Influence on trees")]

        private Vector4 pos1 = new Vector4();
        private Vector4 pos2 = new Vector4();
        private Vector4 pos3 = new Vector4();
        private Vector4 pos4 = new Vector4();
        private Vector4 radius = new Vector4();

        private void Start()
        {
            ApplySettings();
        }

        private void Update()
        {
            ApplySettings();
        }

        private void OnValidate()
        {
            ApplySettings();
        }

        #region ShaderParamIDS
        static int WIND_SETTINGS_TexNoise = Shader.PropertyToID("WIND_SETTINGS_TexNoise");
        static int WIND_SETTINGS_TexGust = Shader.PropertyToID("WIND_SETTINGS_TexGust");
        static int WIND_SETTINGS_WorldDirectionAndSpeed = Shader.PropertyToID("WIND_SETTINGS_WorldDirectionAndSpeed");
        static int WIND_SETTINGS_FlexNoiseScale = Shader.PropertyToID("WIND_SETTINGS_FlexNoiseScale");
        static int WIND_SETTINGS_ShiverNoiseScale = Shader.PropertyToID("WIND_SETTINGS_ShiverNoiseScale");
        static int WIND_SETTINGS_Turbulence = Shader.PropertyToID("WIND_SETTINGS_Turbulence");
        static int WIND_SETTINGS_GustSpeed = Shader.PropertyToID("WIND_SETTINGS_GustSpeed");
        static int WIND_SETTINGS_GustScale = Shader.PropertyToID("WIND_SETTINGS_GustScale");
        static int WIND_SETTINGS_GustWorldScale = Shader.PropertyToID("WIND_SETTINGS_GustWorldScale");
        #endregion


        private Vector3 _workingPos = Vector3.zero;
        private void SetPosAndRadiusForPoint(WindZone point, ref Vector4 pos, int targetIndex)
        {
            if(point != null)
            {
                _workingPos = point.transform.position;
                pos = new Vector4(_workingPos.x, _workingPos.y, _workingPos.z, point.windMain * _SIZE_SCALAR);
                radius[targetIndex] = point.radius;
            }
            else
            {
                pos = Vector4.zero;
                radius[targetIndex] = 0;
            }
        }

        private void ApplySettings()
        {
            Shader.SetGlobalTexture(WIND_SETTINGS_TexNoise, noiseTexture);
            Shader.SetGlobalTexture(WIND_SETTINGS_TexGust, gustTexture);
            Shader.SetGlobalVector(WIND_SETTINGS_WorldDirectionAndSpeed, GetDirectionAndSpeed());
            Shader.SetGlobalFloat(WIND_SETTINGS_FlexNoiseScale, 1.0f / Mathf.Max(0.01f, bendingWorldSize));
            Shader.SetGlobalFloat(WIND_SETTINGS_ShiverNoiseScale, 1.0f / Mathf.Max(0.01f, leafWorldSize));
            Shader.SetGlobalFloat(WIND_SETTINGS_Turbulence, windSpeed * turbulence);
            Shader.SetGlobalFloat(WIND_SETTINGS_GustSpeed, gustSpeed);
            Shader.SetGlobalFloat(WIND_SETTINGS_GustScale, gustScale);
            Shader.SetGlobalFloat(WIND_SETTINGS_GustWorldScale, 1.0f / Mathf.Max(0.01f, gustWorldSize));

            SetPosAndRadiusForPoint(point1, ref pos1, 0);
            SetPosAndRadiusForPoint(point2, ref pos2, 1);
            SetPosAndRadiusForPoint(point3, ref pos3, 2);
            SetPosAndRadiusForPoint(point4, ref  pos4, 3);

            Shader.SetGlobalMatrix("WIND_SETTINGS_Points", new Matrix4x4(pos1, pos2, pos3, pos4));
            Shader.SetGlobalVector("WIND_SETTINGS_Points_Radius", radius);
        }

        private Vector4 GetDirectionAndSpeed()
        {
            Vector3 dir = transform.forward.normalized;
            return new Vector4(dir.x, dir.y, dir.z, windSpeed * _SIZE_SCALAR);
        }

    }
}