using UnityEngine;
using UnityEngine.Rendering;
using Viva.Util;

namespace OccaSoftware.Altos.Runtime
{
    [CreateAssetMenu(fileName = "Cloud Definition", menuName = "Viva/Cloud Definition")]
    public class CloudDefinition : ScriptableObject
    {
        [Range(0, 1f)]
        public float cloudiness;

        public bool dynamicCloudiness = true;

        [Tooltip("Sets the cloudiness percentage.")]
        [Range(-1, 1)]
        public float cloudinessMin = 0.4f;

        [Tooltip("Sets the cloudiness percentage.")]
        [Range(-1, 1)]
        public float cloudinessMax = 0.7f;

        [Range(0, 1)]
        public float currentCloudiness;

        public float inScatteringMultiplier = 100;

        [Range(0, 1)]
        public float inScatteringStrength = 1.0f;

        /// <summary>
        /// This method is called when we want the cloud definition to update the current cloudiness.
        /// </summary>
        /// <param name="time"></param>
        public void UpdateCloudiness(float time)
        {
            if (overrideState == OverrideState.Overriden)
                return;

            if (dynamicCloudiness)
            {
                currentCloudiness = GetCloudinessAtTime(time);
            }
            else
            {
                currentCloudiness = cloudiness;
            }
        }

        public float GetCloudinessAtTime(float time)
        {
            if (!dynamicCloudiness)
                return currentCloudiness;

            float a = Mathf.Floor(time);
            float b = Mathf.Ceil(time);
            float x = Mathf.Clamp01(Tools.RemapFrom01(Tools.Random(a, 775), cloudinessMin, cloudinessMax));
            float y = Mathf.Clamp01(Tools.RemapFrom01(Tools.Random(b, 775), cloudinessMin, cloudinessMax));
            float z = Tools.Smoothstep(x, y, Tools.Frac(time));
            return z;
        }

        private OverrideState overrideState = OverrideState.Normal;

        /// <summary>
        /// Get the floor of the volumetric cloud layer, in meters.
        /// </summary>
        public float GetCloudFloor()
        {
            return cloudLayerHeight * 1000f;
        }

        /// <summary>
        /// Get the ceiling of the volumetric cloud layer, in meters.
        /// </summary>
        public float GetCloudCeiling()
        {
            return (cloudLayerHeight + cloudLayerThickness) * 1000f;
        }

        /// <summary>
        /// Get the midpoint of the volumetric cloud layer.
        /// </summary>
        public float GetCloudCenter()
        {
            return (cloudLayerHeight + cloudLayerThickness * 0.5f) * 1000f;
        }

        private const string _VOLUME_TEXTURE_DIRECTORY = "Assets/3rd Party/Paid/com.occasoftware.altos/Textures/Noise Textures/VolumeTextures/";

        private void OnEnable()
        {
            Setup();
        }

        private void OnValidate()
        {
            Setup();
        }

        private void Setup()
        {
            planetRadius = GetRadiusFromCelestialBodySelection(celestialBodySelection, planetRadius);
            extinctionCoefficient = Mathf.Max(0, extinctionCoefficient);
            maxLightingDistance = Mathf.Max(0, maxLightingDistance);

            curlTextureInfluence = Mathf.Max(0, curlTextureInfluence);
            curlTextureScale = Mathf.Max(0, curlTextureScale);

            detail1TextureInfluence = Mathf.Clamp(detail1TextureInfluence, 0f, 1f);
            detail1TextureScale = Vector3.Max(Vector3.zero, detail1TextureScale);

            baseTexture = LoadVolumeTexture(baseTextureID, baseTextureQuality, baseTexture);
            detail1Texture = LoadVolumeTexture(detail1TextureID, detail1TextureQuality, detail1Texture);

            if (detail1Texture == null)
                detail1TextureInfluence = 0.0f;

            highAltExtinctionCoefficient = Mathf.Max(0, highAltExtinctionCoefficient);

            highAltScale1 = Vector2.Max(Vector2.zero, highAltScale1);
            highAltScale2 = Vector2.Max(Vector2.zero, highAltScale2);
            highAltScale3 = Vector2.Max(Vector2.zero, highAltScale3);

            blueNoise = RoundTo2Decimals(blueNoise);
            HGEccentricityBackward = RoundTo2Decimals(HGEccentricityBackward);
            HGEccentricityForward = RoundTo2Decimals(HGEccentricityForward);
            cloudiness = RoundTo2Decimals(cloudiness);
            cloudinessDensityInfluence = RoundTo2Decimals(cloudinessDensityInfluence);
            heightDensityInfluence = RoundTo2Decimals(heightDensityInfluence);

            detail1TextureInfluence = RoundTo2Decimals(detail1TextureInfluence);

            highAltCloudiness = RoundTo2Decimals(highAltCloudiness);

            ambientExposure = Mathf.Max(ambientExposure, 0);
            cloudLayerHeight = Mathf.Max(cloudLayerHeight, 0);

            distantCoverageDepth = Mathf.Min(cloudFadeDistance, distantCoverageDepth);
        }

        private Texture3D EmptyTexture3D
        {
            get
            {
                if (emptyTexture3D == null)
                {
                    emptyTexture3D = new Texture3D(1, 1, 1, TextureFormat.RGBA32, false);
                    emptyTexture3D.SetPixel(0, 0, 0, Color.white);
                    emptyTexture3D.Apply();
                }

                return emptyTexture3D;
            }
        }
        private Texture3D emptyTexture3D;

        /// <summary>
        /// Fetches the correct volume texture given the input texture type and quality settings.
        /// </summary>
        /// <param name="id">The type of texture</param>
        /// <param name="quality">The quality level for the texture</param>
        /// <returns>A Texture3D matching the input type and quality.</returns>
        private Texture3D LoadVolumeTexture(TextureIdentifier id, TextureQuality quality, Texture3D currentTexture)
        {
#if UNITY_EDITOR
            string loadTarget = _VOLUME_TEXTURE_DIRECTORY;
            switch (id)
            {
                case TextureIdentifier.None:
                    return EmptyTexture3D;
                case TextureIdentifier.Perlin:
                    loadTarget += "Perlin/Perlin";
                    break;
                case TextureIdentifier.PerlinWorley:
                    loadTarget += "PerlinWorley/PerlinWorley";
                    break;
                case TextureIdentifier.PerlinWorleyMix:
                    loadTarget += "PerlinWorleyMix/PerlinWorleyMix";
                    break;
                case TextureIdentifier.Worley:
                    loadTarget += "Worley/Worley";
                    break;
                case TextureIdentifier.Billow:
                    loadTarget += "Billow/Billow";
                    break;
                default:
                    return EmptyTexture3D;
            }

            switch (quality)
            {
                case TextureQuality.Medium:
                    loadTarget += "32";
                    break;
                case TextureQuality.High:
                    loadTarget += "64";
                    break;
                case TextureQuality.Ultra:
                    loadTarget += "128";
                    break;
                default:
                    return EmptyTexture3D;
            }
            loadTarget += ".asset";
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Texture3D>(loadTarget);
#else
            return currentTexture;
#endif
        }

        /// <summary>
        /// Rounds the input value to a maximum of two decimal points.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private float RoundTo2Decimals(float input)
        {
            return (float)System.Math.Round((double)input, 2);
        }

        /// <summary>
        /// Clamps the input vector to the [0,1] range.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private Vector2 ClampVec2_01(Vector2 input)
        {
            return Vector2.Min(Vector2.one, Vector2.Max(Vector2.zero, input));
        }

        #region Volumetric Basic Setup
        /// <summary>
        /// The number of steps taken through the cloud volume to sample the cloud data. Higher values are more costly but yield better results.
        /// </summary>
        public int stepCount = 128;

        public int coarseSteps = 64;
        public int detailSteps = 196;
        public int maximumEmptyDetailSteps = 5;

        public Vector2 mipmapStages = new Vector2(12500, 25000);

        /// <summary>
        /// The intensity of the noise term applied to the clouds to deband the cloud sampling results.
        /// </summary>
        public float blueNoise = 1.0f;

        public DepthCullOptions depthOptions = DepthCullOptions.RespectGeometry;

        public bool useReprojection = true;

        public ResolutionOptions resolutionOptions = ResolutionOptions.Full;

        public bool useTemporalDenoising = false;

        public Color cloudAlbedoColor = new Color(1.0f, 0.964f, 0.92f);

        /// <summary>
        /// The color of the sun used by the clouds for rendering.
        /// </summary>
        public Color sunColor = Color.white;

        /// <summary>
        /// Overall intensity of the ambient exposure term. Higher values mean the ambient lighting is stronger.
        /// </summary>
        public float ambientExposure = 1.0f;

        /// <summary>
        /// Uses a more performant ambient lighting algorithm.
        /// </summary>
        public bool cheapAmbientLighting = true;

        /// <summary>
        /// The Henyey-Greenstein term used for forward scattering light. A higher value results in a sharper falloff.
        /// </summary>
        public float HGEccentricityForward = 0.6f;

        /// <summary>
        /// The Henyey-Greenstein term used for backscattering light. A higher value results in a sharper falloff.
        /// </summary>
        public float HGEccentricityBackward = -0.2f;

        /// <summary>
        /// The intensity of the Henyey-Greenstein lighting effect. This effect dictates the intensity of the forward and backscattering of light as it passes through the cloud volume.
        /// </summary>
        public float HGStrength = 1.0f;

        /// <summary>
        /// A reference planet (or moon) used to automatically set the radius. The planet radius is used to control the curvature of the cloud volume.
        /// </summary>
        public CelestialBodySelection celestialBodySelection;

        /// <summary>
        /// The planet radius is used to control the curvature of the cloud volume.
        /// </summary>
        public int planetRadius = 6378;

        /// <summary>
        /// The cloud layer height is the altitude at which the low altitude cloud volume begins.
        /// </summary>
        public float cloudLayerHeight = 600f;

        /// <summary>
        /// The cloud layer thickness dictates the altitude at which the low altitude cloud volume ends by the formula cloudLayerHeight + cloudLayerThickness = cloudLayerEndHeight. Increase this to create taller clouds.
        /// </summary>
        public float cloudLayerThickness = 4000f;

        /// <summary>
        /// The cloud fade distance dictates the distance at which clouds will no longer be rendered.
        /// </summary>
        [Min(1000)]
        public float cloudFadeDistance = 40000f;

        /// <summary>
        /// The visibility term controls the fog falloff. Typically, you want to set this to a value greater than or equal to the cloud fade distance.
        /// </summary>
        [Min(1000)]
        public float visibility = 30000f;

        [Tooltip("When disabled, Altos uses the Atmosphere Config's visibility. When enabled, you can override this value with a custom one.")]
        public bool overrideAtmosphericVisibility;

        public float GetAtmosphereAttenuationDensity()
        {
            float visibility = AltosSkyDirector.Instance.atmosphereDefinition.end;

            if (overrideAtmosphericVisibility)
            {
                visibility = this.visibility;
            }

            return Tools.GetDensityFromVisibilityDistance(visibility);
        }

        /// <summary>
        /// When enabled, the clouds will be rendered in scene view.
        /// </summary>
        public bool renderInSceneView = true;

        /// <summary>
        /// Configures the influence of recent frame data over the temporal blending.
        /// </summary>
        public float temporalDenoisingFactor = 0.03f;

        /// <summary>
        /// Enables sub-pixel jittering to increase the quality of cloud data when conducting temporal anti-aliasing.
        /// </summary>
        public bool subpixelJitterEnabled = true;

        /// <summary>
        /// When enabled, the clouds will write shadow data to the cloud shadow textures.
        /// </summary>
        public bool castShadowsEnabled = true;

        /// <summary>
        /// Sets the number of steps that the raymarcher will take when building the cloud shadowmaps.
        /// </summary>
        [Range(4, 32)]
        public int shadowStepCount = 16;

        /// <summary>
        /// When enabled, the clouds will automatically write cloud shadow data to the screen during rendering. If disabled, you must sample the cloud shadow data yourself.
        /// </summary>
        public bool screenShadows = true;

        /// <summary>
        /// Controls the strength of cloud shadows.
        /// </summary>
        [Min(0)]
        public float shadowStrength = 1.0f;

        /// <summary>
        /// Controls the resolution (and resulting quality) of cloud shadows.
        /// </summary>
        public ShadowResolution shadowResolution = ShadowResolution.Medium;

        /// <summary>
        /// Controls the maximum distance at which cloud shadows will be rendered.
        /// </summary>
        [Min(0)]
        public int shadowDistance = 10000;
        #endregion

        #region Low Altitude

        #region Rendering
        /// <summary>
        /// Controls the density of the low altitude clouds.
        /// </summary>
        public float extinctionCoefficient = 10f;

        /// <summary>
        /// Sets the maximum distance at which cloud lighting will be sampled. Cloud lighting normally attempts to sample to the end of the cloud volume in the direction of the main light, but in some cases (like when the sun is close to the horizon), this effect can break down.
        /// </summary>
        public int maxLightingDistance = 2000;

        /// <summary>
        /// Control the percentage density retained by each shading step.
        /// </summary>
        [Range(0, 1)]
        public float shadingStrengthFalloff = 0.2f;

        /// <summary>
        /// Controls the intensity of the multiple scattering effect's octaves.
        /// </summary>
        public float multipleScatteringAmpGain = 0.3f;

        /// <summary>
        /// Controls the density of the multiple scattering effect's octaves.
        /// </summary>
        public float multipleScatteringDensityGain = 0.1f;

        /// <summary>
        /// Controls the number of octaves to sample for the multiple scattering approximation. This improves the quality of the cloud lighting by simulating bounced lighting within the cloud volume.
        /// </summary>
        public int multipleScatteringOctaves = 3;
        #endregion

        #region Modeling

        public bool overrideDistantCoverage = false;

        /// <summary>
        /// Used to set the start distance at which the distant coverage cloudiness term will take effect.
        /// </summary>
        [Min(5)]
        public float distantCoverageDepth = 20000f;

        /// <summary>
        /// Controls the coverage starting from the distant coverage depth.
        /// </summary>
        public float distantCoverageAmount = 0.8f;

        /// <summary>
        /// Used to increase the cloud density by the cloud altitude.
        /// </summary>
        public float heightDensityInfluence = 1.0f;

        /// <summary>
        /// Used to increase the cloud density as the cloud coverage value increases.
        /// </summary>
        public float cloudinessDensityInfluence = 1.0f;
        #endregion

        #region Weather
        /// <summary>
        /// Describes whether the cloud volume will reference a procedural weather volume or an input texture volume.
        /// </summary>
        public WeathermapType weathermapType = WeathermapType.Procedural;

        /// <summary>
        /// When using the Weathermap Type of Texture, describes the weathermap that will be used.
        /// </summary>
        public Texture2D weathermapTexture = null;

        /// <summary>
        /// When using a procedural weathermap, describes the amplitude
        /// </summary>
        [Range(0, 1)]
        public float weathermapGain = 0.5f;

        /// <summary>
        /// When using a procedural weathermap, describes the lacunarity (frequency).
        /// </summary>
        [Range(2, 5)]
        public int weathermapLacunarity = 2;

        /// <summary>
        /// When using a procedural weathermap, describes the number of octaves.
        /// </summary>
        [Range(1, 6)]
        public int weathermapOctaves = 3;

        /// <summary>
        /// Used to set the speed at which the weathermap moves across the scene.
        /// </summary>
        public Vector2 weathermapVelocity = Vector2.zero;

        /// <summary>
        /// Used to set the overall scale of the weathermap. Higher values mean a more dense weathermap.
        /// </summary>
        public float weathermapScale = 5.0f;
        #endregion


        #region Base Volume Model
        /// <summary>
        /// The type of the volume texture used for the base cloud shape.
        /// </summary>
        public TextureIdentifier baseTextureID = TextureIdentifier.Perlin;

        /// <summary>
        /// The quality of the volume texture used for the base cloud shape.
        /// </summary>
        public TextureQuality baseTextureQuality = TextureQuality.High;

        /// <summary>
        /// The actual texture3D used for the base cloud shape.
        /// </summary>
        public Texture3D baseTexture = null;

        /// <summary>
        /// The scale of the base cloud shape.
        /// </summary>
        public Vector3 baseTextureScale = new Vector3(5, 5, 5);

        /// <summary>
        /// The rate at which the base cloud shape will displace over time.
        /// </summary>
        public Vector3 baseTextureTimescale = new Vector3(2f, -1f, 1f);
        #endregion

        #region Detail 1 Volume Model
        /// <summary>
        /// The type of texture used for the detail texture.
        /// </summary>
        public TextureIdentifier detail1TextureID = TextureIdentifier.Worley;

        /// <summary>
        /// The quality of the texture used for the detail texture.
        /// </summary>
        public TextureQuality detail1TextureQuality = TextureQuality.Medium;

        /// <summary>
        /// The actual Texture3D used for the detail texture.
        /// </summary>
        public Texture3D detail1Texture = null;

        /// <summary>
        /// The strength of the detail texture as applied as an erosion to the base cloud shape.
        /// </summary>
        public float detail1TextureInfluence = 0.3f;

        /// <summary>
        /// The scale of the detail texture volume.
        /// </summary>
        public Vector3 detail1TextureScale = new Vector3(15, 15, 15);

        /// <summary>
        /// The rate at which the detail texture will displace over time.
        /// </summary>
        public Vector3 detail1TextureTimescale = new Vector3(1f, -2f, -2f);

        #endregion

        #region Detail Curl 2D Model
        /// <summary>
        /// The displacement texture used to offset noise, simulating detail wind.
        /// </summary>
        public Texture2D curlTexture;

        /// <summary>
        /// The strength of this displacement texture.
        /// </summary>
        public float curlTextureInfluence;

        /// <summary>
        /// The scale of the displacement texture.
        /// </summary>
        public float curlTextureScale;

        /// <summary>
        /// The rate at which the displacement texture moves across the cloud volume.
        /// </summary>
        public float curlTextureTimescale;
        #endregion

        #endregion

        #region High Altitude
        /// <summary>
        /// The density of high altitude clouds.
        /// </summary>
        public float highAltExtinctionCoefficient = 0.2f;

        /// <summary>
        /// The cloud coverage for high altitude clouds.
        /// </summary>
        public float highAltCloudiness = 0.5f;

        /// <summary>
        /// The weathermap used in conjunction with the cloudiness value to set the cloud coverage areas.
        /// </summary>
        public Texture2D highAltTex1 = null;

        /// <summary>
        /// The scale of the high altitude weathermap.
        /// </summary>
        public Vector2 highAltScale1 = new Vector2(5f, 5f);

        /// <summary>
        /// The rate at which the high altitude weathermap pans over the scene.
        /// </summary>
        public Vector2 highAltTimescale1 = new Vector2(5f, 5f);

        /// <summary>
        /// One of the two erosion textures used for the high altitude clouds.
        /// </summary>
        public Texture2D highAltTex2 = null;

        /// <summary>
        /// The scale of this high altitude texture.
        /// </summary>
        public Vector2 highAltScale2 = new Vector2(5f, 5f);

        /// <summary>
        /// The rate at which this high altitude texture pans over the scene.
        /// </summary>
        public Vector2 highAltTimescale2 = new Vector2(5f, 5f);

        /// <summary>
        /// One of the two erosion textures used for the high altitude clouds.
        /// </summary>
        public Texture2D highAltTex3 = null;

        /// <summary>
        /// The scale of this high altitude texture.
        /// </summary>
        public Vector2 highAltScale3 = new Vector2(5f, 5f);

        /// <summary>
        /// The rate at which this high altitude texture pans over the scene.
        /// </summary>
        public Vector2 highAltTimescale3 = new Vector2(5f, 5f);

        #endregion

        /// <summary>
        /// A helper method in which we've encoded the radius of each celestial body in KM.
        /// </summary>
        /// <param name="celestialBodySelection"></param>
        /// <param name="currentVal"></param>
        /// <returns></returns>
        private int GetRadiusFromCelestialBodySelection(CelestialBodySelection celestialBodySelection, int currentVal)
        {
            switch (celestialBodySelection)
            {
                case CelestialBodySelection.Earth:
                    return 6378;
                case CelestialBodySelection.Mars:
                    return 3389;
                case CelestialBodySelection.Venus:
                    return 6052;
                case CelestialBodySelection.Luna:
                    return 1737;
                case CelestialBodySelection.Titan:
                    return 2575;
                case CelestialBodySelection.Enceladus:
                    return 252;
                default:
                    return Mathf.Max(0, currentVal);
            }
        }
    }

    /// <summary>
    /// The type of texture for this volume texture.
    /// </summary>
    public enum TextureIdentifier
    {
        None,
        Perlin,
        PerlinWorley,
        PerlinWorleyMix,
        Worley,
        Billow
    }

    /// <summary>
    /// The resolution of the shadow map.
    /// </summary>
    public enum ShadowResolution
    {
        Low = 256,
        Medium = 512,
        High = 1024,
    }

    /// <summary>
    /// The quality of the Volume Texture to fetch.
    /// </summary>
    public enum TextureQuality
    {
        Medium,
        High,
        Ultra
    }

    /// <summary>
    /// The planet to reference for cloud curvature.
    /// </summary>
    public enum CelestialBodySelection
    {
        Earth,
        Mars,
        Venus,
        Luna,
        Titan,
        Enceladus,
        Custom
    }

    public enum ResolutionOptions
    {
        Full,
        Half
    }

    /// <summary>
    /// Whether to depth-test the clouds during rendering.
    /// </summary>
    public enum DepthCullOptions
    {
        AlwaysRenderBehindGeometry,
        RespectGeometry
    }

    /// <summary>
    /// The type of weathermap to use for coverage sampling.
    /// </summary>
    public enum WeathermapType
    {
        Procedural,
        Texture
    }
}
