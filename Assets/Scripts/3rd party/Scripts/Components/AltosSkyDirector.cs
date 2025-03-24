using System;

using System.Linq;
using System.Collections.Generic;

using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Profiling;
using Viva;

namespace OccaSoftware.Altos.Runtime
{
    public class AltosSkyDirector : MonoBehaviour
    {
        private static AltosSkyDirector instance;

        public static AltosSkyDirector Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<AltosSkyDirector>();

                return instance;
            }
        }
        
        [System.Serializable]
        public class FogOverride
        {
            public Color color = Color.blue;
            public float density = 0.5f;
        }

        public SkyDefinition skyDefinition;
        
        /// <summary>
        /// Represents the strength of the environmental lighting.
        /// </summary>
        [Min(0)]
        public float environmentLightingExposure = 1f;

        /// <summary>
        /// Represents the saturation of the environmental lighting.
        /// </summary>
        [Min(0)]
        public float environmentLightingSaturation = 1f;


        public AtmosphereDefinition atmosphereDefinition;

        public StarDefinition starDefinition;

        public CloudDefinition cloudDefinition;

        public PrecipitationDefinition precipitationDefinition;
        
        public float[] dayCycleSpeeds = new float[]
        {
            0,
            5,
            12,
            24,
            45,
            60,
            120,
        };

        public static int sWorldPositionOffset = Shader.PropertyToID("altos_WorldPositionOffset");
        public static int sDaytimeFactor = Shader.PropertyToID("altos_DaytimeFactor");
        
        private PeriodOfDay _skyOverride = null;

        public PeriodOfDay skyOverride => Instance._skyOverride;

        public void SetSkyOverride(PeriodOfDay overridePeriod)
        {
            _skyOverride = overridePeriod;
        }

        public float GetCurrentPrecipitation()
        {
            return precipitationDefinition == null ? 0f : precipitationDefinition.currentPrecipitation;
        }

        public float GetCurrentCloudiness()
        {
            return cloudDefinition ? cloudDefinition.currentCloudiness : 0f;
        }

        [Reload("Runtime/Data/AltosDataAsset.asset")]
        public AltosData data;

        [HideInInspector] public AltosWindZone windZone;

        public bool GetWind(out AltosWindZone windZone)
        {
            windZone = null;
            if (this.windZone != null)
            {
                windZone = this.windZone;
                return true;
            }

            return false;
        }

        public void SetWindZone(AltosWindZone windZone)
        {
            this.windZone = windZone;
        }

        public void ClearWindZone()
        {
            windZone = null;
        }

        private const float maxDistance = 10f;
        public static float _HOURS_TO_DEGREES = 15f;
        private bool isValidSetup = false;
        public CloudState cloudState = null;
        public Weathermap weathermap = null;

        private List<SkyObject> _SkyObjects = new List<SkyObject>();

        public List<SkyObject> SkyObjects
        {
            get => _SkyObjects;
        }

        private List<SkyObject> _Sun = new List<SkyObject>();

        public SkyObject Sun
        {
            get => _Sun.Count > 0 ? _Sun[0] : null;
        }

        internal void RegisterSkyObject(SkyObject skyObject)
        {
            if (!_SkyObjects.Contains(skyObject))
            {
                _SkyObjects.Add(skyObject);
            }

            if (skyObject.type == SkyObject.ObjectType.Sun && !_Sun.Contains(skyObject))
            {
                _Sun.Add(skyObject);
            }

            _SkyObjects = _SkyObjects.OrderByDescending(o => o.sortOrder).ToList();
        }

        internal void DeregisterSkyObject(SkyObject skyObject)
        {
            _SkyObjects.Remove(skyObject);
            _Sun.Remove(skyObject);
        }

        private void OnEnable()
        {
            instance = this;
            cloudState = new CloudState(this);
            weathermap = new Weathermap(this);
            Initialize();
            GameDirector.instance.SetMusic(GameDirector.instance.GetDefaultMusic());
        }

        private void OnDisable()
        {
            weathermap.Dispose();
        }

        public void Initialize()
        {
            isValidSetup = ValidateSetup();

            if (!isValidSetup)
                return;

            skyDefinition.Initialize();
        }

        private bool ValidateSetup()
        {
            if (skyDefinition == null)
                return false;

            return true;
        }

        private void Update()
        {
            if (!isValidSetup)
                return;

            if (skyDefinition != null)
            {
                skyDefinition.Update();
                daytimeFactor = skyDefinition.GetDaytimeFactor();
            }

            if (precipitationDefinition != null)
            {
                precipitationDefinition.UpdatePrecipitation(skyDefinition.SystemTime);
            }

            if (cloudDefinition != null)
            {
                cloudDefinition.UpdateCloudiness(skyDefinition.SystemTime);
            }

            if (cloudState != null)
            {
                cloudState.Update();
            }

            if (weathermap != null)
            {
                weathermap.Update();
            }

            ResetScaleAndRotation();

            Shader.SetGlobalFloat(sDaytimeFactor, daytimeFactor);
            Shader.SetGlobalVector(sWorldPositionOffset, (Vector4)origin);
        }

        [HideInInspector] public float daytimeFactor = 1f;

        private void ResetScaleAndRotation()
        {
            if (transform.localScale != Vector3.one || transform.rotation != Quaternion.identity)
            {
                transform.localScale = Vector3.one;
                transform.rotation = Quaternion.identity;
            }
        }

        private Vector3 origin = Vector3.zero;



        public Vector3 GetOrigin()
        {
            return origin;
        }

        /// <summary>
        /// Transform an altitude from offset space to non-offset world space.
        /// </summary>
        /// <param name="altitude"></param>
        /// <returns></returns>
        public float TransformOffsetToWorldPosition(float altitude)
        {
            return altitude;
        }

        public class Weathermap : IDisposable
        {
            public RenderTexture cloudMap;
            public Material cloudMapRenderer;
            AltosSkyDirector skyDirector;
            private Action<AsyncGPUReadbackRequest> gpuReadbackAction = null;
            private Texture2D globalCloudMap;

            public Texture2D GlobalCloudMap
            {
                get => globalCloudMap;
            }

            private struct UV
            {
                public float x;
                public float y;

                public UV(float x, float y)
                {
                    this.x = x;
                    this.y = y;
                }
            }

            private UV PositionToUV(float positionX, float positionZ)
            {
                float x = ((positionX * 0.0001f) + 1.0f) * 0.5f;
                float z = ((positionZ * 0.0001f) + 1.0f) * 0.5f;
                return new UV(x, z);
            }

            public float GetIntensity(Vector3 position)
            {
                UV uv = PositionToUV(position.x, position.z);
                return skyDirector.weathermap.GlobalCloudMap.GetPixelBilinear(uv.x, uv.y).g;
            }

            public Weathermap(AltosSkyDirector skyDirector)
            {
                cloudMap = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                cloudMap.useMipMap = false;
                cloudMap.Create();

                globalCloudMap = new Texture2D(512, 512, TextureFormat.RGBA32, false, true);
                globalCloudMap.wrapMode = TextureWrapMode.Repeat;
                Color[] colors = new Color[globalCloudMap.width * globalCloudMap.height];
                globalCloudMap.SetPixels(colors);
                globalCloudMap.Apply();

                this.skyDirector = skyDirector;
                cloudMapRenderer = CoreUtils.CreateEngineMaterial(skyDirector.data.shaders.cloudMap);

                gpuReadbackAction = OnGPUReadback;
            }

            Vector4[] prec = new Vector4[8]; // xy = position.xz, z = radius, w = precipitation amount

            public void Update()
            {
                cloudMapRenderer.SetFloat("_Gain", skyDirector.cloudDefinition.weathermapGain);
                cloudMapRenderer.SetFloat("_Lacunarity", skyDirector.cloudDefinition.weathermapLacunarity);
                cloudMapRenderer.SetFloat("_Octaves", skyDirector.cloudDefinition.weathermapOctaves);
                cloudMapRenderer.SetFloat("_Scale", skyDirector.cloudDefinition.weathermapScale);
                cloudMapRenderer.SetVector("_Speed", (Vector4)skyDirector.cloudDefinition.weathermapVelocity);
                cloudMapRenderer.SetVector("_Weather", Vector4.zero);
                cloudMapRenderer.SetFloat("_PrecipitationGlobal", skyDirector.GetCurrentPrecipitation());

                int i = 0;
                foreach (WeatherManager m in WeatherManager.WeatherManagers)
                {
                    prec[i].x = m.transform.position.x;
                    prec[i].y = m.transform.position.z;
                    prec[i].z = m.radius;
                    prec[i].w = m.precipitationIntensity;
                    i++;
                }

                cloudMapRenderer.SetFloat("_PrecipitationDataCount", i);
                cloudMapRenderer.SetVectorArray("_PrecipitationData", prec);
                RenderTexture temp = RenderTexture.active;
                RenderTexture.active = cloudMap;
                Graphics.Blit(null, cloudMap, cloudMapRenderer, 0);
                RenderTexture.active = temp;
                AsyncGPUReadback.Request(cloudMap, 0, TextureFormat.RGBA32, gpuReadbackAction);

                Shader.SetGlobalTexture("altos_cloud_map", cloudMap);
            }

            void OnGPUReadback(AsyncGPUReadbackRequest request)
            {
                if (request.hasError)
                    return;

                if (globalCloudMap == null)
                    return;

                globalCloudMap.LoadRawTextureData(request.GetData<uint>());
                globalCloudMap.Apply(false, false);
            }

            public void Dispose()
            {
                if (cloudMap != null)
                {
                    cloudMap.Release();
                    cloudMap = null;
                }

                globalCloudMap = null;

                if (cloudMapRenderer != null)
                {
                    CoreUtils.Destroy(cloudMapRenderer);
                    cloudMapRenderer = null;
                }
            }
        }

        public class CloudState
        {
            private Vector2 positionWeather = Vector2.zero;
            private Vector3 positionBase = Vector3.zero;
            private Vector3 positionDetail = Vector3.zero;
            private float positionCurl = 0;
            private Vector4[] positionHighAlt = { Vector4.zero, Vector4.zero, Vector4.zero };
            private AltosSkyDirector altosSkyDirector = null;
            private bool automaticUpdatesEnabled = true;

            private static int sPositionWeather = Shader.PropertyToID("altos_positionWeather");
            private static int sPositionBase = Shader.PropertyToID("altos_positionBase");
            private static int sPositionDetail = Shader.PropertyToID("altos_positionDetail");
            private static int sPositionCurl = Shader.PropertyToID("altos_positionCurl");
            private static int sPositionHighAlt = Shader.PropertyToID("altos_positionHighAlt");

            public Vector2 PositionWeather
            {
                get => positionWeather;
            }

            public Vector3 PositionBase
            {
                get => positionBase;
            }

            public Vector3 PositionDetail
            {
                get => positionDetail;
            }

            public float PositionCurl
            {
                get => positionCurl;
            }

            /// <summary>
            /// The offset of the high altitude weather texture.
            /// </summary>
            public Vector2 positionHighAltWeather
            {
                get => positionHighAlt[0];
            }

            /// <summary>
            /// The offset of the high altitude texture 1.
            /// </summary>
            public Vector2 positionHighAltTex1
            {
                get => positionHighAlt[1];
            }

            /// <summary>
            /// The offset of the high altitude texture 2.
            /// </summary>
            public Vector2 positionHighAltTex2
            {
                get => positionHighAlt[2];
            }

            /// <summary>
            /// Create a new CloudState.
            /// </summary>
            /// <param name="altosSkyDirector">Expects the current sky director. Will automatically pull the cloud definition from it.</param>
            internal CloudState(AltosSkyDirector altosSkyDirector)
            {
                this.altosSkyDirector = altosSkyDirector;
            }

            internal void Update()
            {
                if (altosSkyDirector.cloudDefinition == null)
                    return;

                UpdatePositions();
                UpdateShaders();
            }

            public void SetAutomaticUpdateState(bool automaticUpdatesEnabled)
            {
                this.automaticUpdatesEnabled = automaticUpdatesEnabled;
            }

            private void UpdatePositions()
            {
                if (!automaticUpdatesEnabled)
                    return;

                if (altosSkyDirector.GetWind(out AltosWindZone altosWindZone))
                {
                    float windSpeed = altosWindZone.GetCloudWindData().speed;
                    Vector3 wind3 = altosWindZone.GetCloudWindData().velocity;
                    Vector2 wind2 = new Vector2(wind3.x, wind3.z);
                    float wind = windSpeed;

                    positionWeather += Time.deltaTime * wind2 * 0.1f;
                    positionBase += Time.deltaTime * wind3 * 0.05f;
                    positionDetail += Time.deltaTime * wind3 * 0.2f;
                    positionCurl += Time.deltaTime * wind * 0.01f;
                    positionHighAlt[0] += Time.deltaTime * (Vector4)wind2 * 0.01f;
                    positionHighAlt[1] += Time.deltaTime * (Vector4)wind2 * 0.1f;
                    positionHighAlt[2] += Time.deltaTime * (Vector4)wind2 * 0.1f;
                }
                else
                {
                    positionWeather += Time.deltaTime * altosSkyDirector.cloudDefinition.weathermapVelocity * 0.1f;
                    positionBase += Time.deltaTime * altosSkyDirector.cloudDefinition.baseTextureTimescale * 0.05f;

                    positionDetail += Time.deltaTime * altosSkyDirector.cloudDefinition.detail1TextureTimescale * 0.2f;
                    positionCurl += Time.deltaTime * altosSkyDirector.cloudDefinition.curlTextureTimescale * 0.01f;

                    positionHighAlt[0] += Time.deltaTime * (Vector4)altosSkyDirector.cloudDefinition.highAltTimescale1 *
                                          0.01f;
                    positionHighAlt[1] += Time.deltaTime * (Vector4)altosSkyDirector.cloudDefinition.highAltTimescale2 *
                                          0.1f;
                    positionHighAlt[2] += Time.deltaTime * (Vector4)altosSkyDirector.cloudDefinition.highAltTimescale3 *
                                          0.1f;
                }
            }

            private void UpdateShaders()
            {
                Shader.SetGlobalVector(sPositionWeather, positionWeather);
                Shader.SetGlobalVector(sPositionBase, positionBase);
                Shader.SetGlobalVector(sPositionDetail, positionDetail);
                Shader.SetGlobalFloat(sPositionCurl, positionCurl);
                Shader.SetGlobalVectorArray(sPositionHighAlt, positionHighAlt);
            }

        }
    }
}
