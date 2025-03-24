using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OccaSoftware.Altos.Runtime
{
    internal static class CloudShaderParamHandler
    {
        public static class ShaderParams
        {
            public static int _ScreenTexture = Shader.PropertyToID("_ScreenTexture");
            public static int depthCullReference = Shader.PropertyToID("_CLOUD_DEPTH_CULL_ON");
            public static int taaBlendFactorReference = Shader.PropertyToID("_TAA_BLEND_FACTOR");
            public static int _ViewProjM = Shader.PropertyToID("_ViewProjM");
            public static int _PrevViewProjM = Shader.PropertyToID("_PrevViewProjM");

            public static int _CastScreenCloudShadows = Shader.PropertyToID("_CastScreenCloudShadows");

            public static int _Halton_23_Sequence = Shader.PropertyToID("_Halton_23_Sequence");
            public static int _BLUE_NOISE = Shader.PropertyToID("_BLUE_NOISE");
            public static int _FrameId = Shader.PropertyToID("_FrameId");
            public static int _MainCameraOrigin = Shader.PropertyToID("_MainCameraOrigin");
            public static int _USE_DITHERED_DEPTH = Shader.PropertyToID("_USE_DITHERED_DEPTH");
            public static int _DitheredDepthTexture = Shader.PropertyToID("_DitheredDepthTexture");
            public static int _ShadowPass = Shader.PropertyToID("_ShadowPass");
            public static int _RenderTextureDimensions = Shader.PropertyToID("_RenderTextureDimensions");

            public static class Shadows
            {
                public static int _CloudShadowOrthoParams = Shader.PropertyToID("_CloudShadowOrthoParams");
                public static int _ShadowCasterCameraPosition = Shader.PropertyToID("_ShadowCasterCameraPosition");
                public static int _CloudShadow_WorldToShadowMatrix = Shader.PropertyToID("_CloudShadow_WorldToShadowMatrix");
                public static int _ShadowCasterCameraForward = Shader.PropertyToID("_ShadowCasterCameraForward");
                public static int _ShadowCasterCameraUp = Shader.PropertyToID("_ShadowCasterCameraUp");
                public static int _ShadowCasterCameraRight = Shader.PropertyToID("_ShadowCasterCameraRight");
                public static int _CLOUD_SHADOW_PREVIOUS_HISTORY = Shader.PropertyToID("_CLOUD_SHADOW_PREVIOUS_HISTORY");
                public static int _CLOUD_SHADOW_CURRENT_FRAME = Shader.PropertyToID("_CLOUD_SHADOW_CURRENT_FRAME");
                public static int _IntegrationRate = Shader.PropertyToID("_IntegrationRate");
                public static int _CloudShadowHistoryTexture = Shader.PropertyToID("_CloudShadowHistoryTexture");
                public static int _ShadowRenderStepCount = Shader.PropertyToID("_ShadowRenderStepCount");
                public static int _CloudShadowDistance = Shader.PropertyToID("_CloudShadowDistance");
                public static int _ShadowMapCascades = Shader.PropertyToID("_ShadowMapCascades");
                public static int _ShadowmapResolution = Shader.PropertyToID("_ShadowmapResolution");
                public static int _ShadowRadius = Shader.PropertyToID("_ShadowRadius");
                public static int _InverseViewProjectionM = Shader.PropertyToID("_InverseViewProjectionM");
                public static int _CloudScreenShadows = Shader.PropertyToID("_CloudScreenShadows");

                public static int _CloudShadowStrength = Shader.PropertyToID("_CloudShadowStrength");
                public static int _CloudShadowmap = Shader.PropertyToID("_CloudShadowmap");
            }

            public static class CloudData
            {
                public static int _COARSE_STEPS = Shader.PropertyToID("_COARSE_STEPS");
                public static int _DETAIL_STEPS = Shader.PropertyToID("_DETAIL_STEPS");
                public static int _MAXIMUM_EMPTY_DETAIL_STEPS = Shader.PropertyToID("_MAXIMUM_EMPTY_DETAIL_STEPS");

                public static int _MIPMAP_STAGES = Shader.PropertyToID("_MIPMAP_STAGES");

                public static int _CLOUD_STEP_COUNT = Shader.PropertyToID("_CLOUD_STEP_COUNT");
                public static int _CLOUD_BLUE_NOISE_STRENGTH = Shader.PropertyToID("_CLOUD_BLUE_NOISE_STRENGTH");
                public static int _CLOUD_BASE_TEX = Shader.PropertyToID("_CLOUD_BASE_TEX");
                public static int _CLOUD_DETAIL1_TEX = Shader.PropertyToID("_CLOUD_DETAIL1_TEX");
                public static int _CLOUD_EXTINCTION_COEFFICIENT = Shader.PropertyToID("_CLOUD_EXTINCTION_COEFFICIENT");
                public static int _CLOUD_COVERAGE = Shader.PropertyToID("_CLOUD_COVERAGE");

                public static int _CLOUD_ALBEDO = Shader.PropertyToID("_CLOUD_ALBEDO");
                public static int _CLOUD_SUN_COLOR_MASK = Shader.PropertyToID("_CLOUD_SUN_COLOR_MASK");
                public static int _CLOUD_LAYER_HEIGHT = Shader.PropertyToID("_CLOUD_LAYER_HEIGHT");
                public static int _CLOUD_LAYER_THICKNESS = Shader.PropertyToID("_CLOUD_LAYER_THICKNESS");
                public static int _CLOUD_FADE_DIST = Shader.PropertyToID("_CLOUD_FADE_DIST");
                public static int _CLOUD_BASE_SCALE = Shader.PropertyToID("_CLOUD_BASE_SCALE");
                public static int _CLOUD_DETAIL1_SCALE = Shader.PropertyToID("_CLOUD_DETAIL1_SCALE");
                public static int _CLOUD_DETAIL1_STRENGTH = Shader.PropertyToID("_CLOUD_DETAIL1_STRENGTH");
                public static int _CLOUD_BASE_TIMESCALE = Shader.PropertyToID("_CLOUD_BASE_TIMESCALE");
                public static int _CLOUD_DETAIL1_TIMESCALE = Shader.PropertyToID("_CLOUD_DETAIL1_TIMESCALE");
                public static int _CLOUD_FOG_POWER = Shader.PropertyToID("_CLOUD_FOG_POWER");
                public static int _CLOUD_MAX_LIGHTING_DIST = Shader.PropertyToID("_CLOUD_MAX_LIGHTING_DIST");
                public static int _SHADING_STRENGTH_FALLOFF = Shader.PropertyToID("_SHADING_STRENGTH_FALLOFF");
                public static int _IN_SCATTERING_MULTIPLIER = Shader.PropertyToID("_IN_SCATTERING_MULTIPLIER");
                public static int _IN_SCATTERING_STRENGTH = Shader.PropertyToID("_IN_SCATTERING_STRENGTH");
                public static int _CLOUD_PLANET_RADIUS = Shader.PropertyToID("_CLOUD_PLANET_RADIUS");

                public static int _CLOUD_CURL_TEX = Shader.PropertyToID("_CLOUD_CURL_TEX");
                public static int _CLOUD_CURL_SCALE = Shader.PropertyToID("_CLOUD_CURL_SCALE");
                public static int _CLOUD_CURL_STRENGTH = Shader.PropertyToID("_CLOUD_CURL_STRENGTH");
                public static int _CLOUD_CURL_TIMESCALE = Shader.PropertyToID("_CLOUD_CURL_TIMESCALE");
                public static int _CLOUD_CURL_ADJUSTMENT_BASE = Shader.PropertyToID("_CLOUD_CURL_ADJUSTMENT_BASE");

                public static int _CLOUD_DETAIL2_TEX = Shader.PropertyToID("_CLOUD_DETAIL2_TEX");
                public static int _CLOUD_DETAIL2_SCALE = Shader.PropertyToID("_CLOUD_DETAIL2_SCALE");
                public static int _CLOUD_DETAIL2_TIMESCALE = Shader.PropertyToID("_CLOUD_DETAIL2_TIMESCALE");
                public static int _CLOUD_DETAIL2_STRENGTH = Shader.PropertyToID("_CLOUD_DETAIL2_STRENGTH");

                public static int _CLOUD_HGFORWARD = Shader.PropertyToID("_CLOUD_HGFORWARD");
                public static int _CLOUD_HGBACK = Shader.PropertyToID("_CLOUD_HGBACK");
                public static int _CLOUD_HGBLEND = Shader.PropertyToID("_CLOUD_HGBLEND");
                public static int _CLOUD_HGSTRENGTH = Shader.PropertyToID("_CLOUD_HGSTRENGTH");

                public static int _CLOUD_AMBIENT_EXPOSURE = Shader.PropertyToID("_CLOUD_AMBIENT_EXPOSURE");

                public static int _CheapAmbientLighting = Shader.PropertyToID("_CheapAmbientLighting");

                public static int _CLOUD_DISTANT_COVERAGE_START_DEPTH = Shader.PropertyToID("_CLOUD_DISTANT_COVERAGE_START_DEPTH");
                public static int _CLOUD_DISTANT_CLOUD_COVERAGE = Shader.PropertyToID("_CLOUD_DISTANT_CLOUD_COVERAGE");
                public static int _USE_DISTANT_COVERAGE_OVERRIDE = Shader.PropertyToID("_USE_DISTANT_COVERAGE_OVERRIDE");
                public static int _CLOUD_DETAIL1_HEIGHT_REMAP = Shader.PropertyToID("_CLOUD_DETAIL1_HEIGHT_REMAP");

                public static int _CLOUD_DETAIL1_INVERT = Shader.PropertyToID("_CLOUD_DETAIL1_INVERT");
                public static int _CLOUD_DETAIL2_HEIGHT_REMAP = Shader.PropertyToID("_CLOUD_DETAIL2_HEIGHT_REMAP");
                public static int _CLOUD_DETAIL2_INVERT = Shader.PropertyToID("_CLOUD_DETAIL2_INVERT");
                public static int _CLOUD_HEIGHT_DENSITY_INFLUENCE = Shader.PropertyToID("_CLOUD_HEIGHT_DENSITY_INFLUENCE");
                public static int _CLOUD_COVERAGE_DENSITY_INFLUENCE = Shader.PropertyToID("_CLOUD_COVERAGE_DENSITY_INFLUENCE");

                public static int _CLOUD_HIGHALT_TEX_1 = Shader.PropertyToID("_CLOUD_HIGHALT_TEX_1");
                public static int _CLOUD_HIGHALT_TEX_2 = Shader.PropertyToID("_CLOUD_HIGHALT_TEX_2");
                public static int _CLOUD_HIGHALT_TEX_3 = Shader.PropertyToID("_CLOUD_HIGHALT_TEX_3");

                public static int _CLOUD_HIGHALT_OFFSET1 = Shader.PropertyToID("_CLOUD_HIGHALT_OFFSET1");
                public static int _CLOUD_HIGHALT_OFFSET2 = Shader.PropertyToID("_CLOUD_HIGHALT_OFFSET2");
                public static int _CLOUD_HIGHALT_OFFSET3 = Shader.PropertyToID("_CLOUD_HIGHALT_OFFSET3");
                public static int _CLOUD_HIGHALT_SCALE1 = Shader.PropertyToID("_CLOUD_HIGHALT_SCALE1");
                public static int _CLOUD_HIGHALT_SCALE2 = Shader.PropertyToID("_CLOUD_HIGHALT_SCALE2");
                public static int _CLOUD_HIGHALT_SCALE3 = Shader.PropertyToID("_CLOUD_HIGHALT_SCALE3");
                public static int _CLOUD_HIGHALT_COVERAGE = Shader.PropertyToID("_CLOUD_HIGHALT_COVERAGE");
                public static int _CLOUD_HIGHALT_INFLUENCE1 = Shader.PropertyToID("_CLOUD_HIGHALT_INFLUENCE1");
                public static int _CLOUD_HIGHALT_INFLUENCE2 = Shader.PropertyToID("_CLOUD_HIGHALT_INFLUENCE2");
                public static int _CLOUD_HIGHALT_INFLUENCE3 = Shader.PropertyToID("_CLOUD_HIGHALT_INFLUENCE3");
                public static int _CLOUD_BASE_RGBAInfluence = Shader.PropertyToID("_CLOUD_BASE_RGBAInfluence");
                public static int _CLOUD_DETAIL1_RGBAInfluence = Shader.PropertyToID("_CLOUD_DETAIL1_RGBAInfluence");
                public static int _CLOUD_DETAIL2_RGBAInfluence = Shader.PropertyToID("_CLOUD_DETAIL2_RGBAInfluence");
                public static int _CLOUD_HIGHALT_EXTINCTION = Shader.PropertyToID("_CLOUD_HIGHALT_EXTINCTION");

                public static int _CLOUD_HIGHALT_SHAPE_POWER = Shader.PropertyToID("_CLOUD_HIGHALT_SHAPE_POWER");
                public static int _CLOUD_SCATTERING_AMPGAIN = Shader.PropertyToID("_CLOUD_SCATTERING_AMPGAIN");
                public static int _CLOUD_SCATTERING_DENSITYGAIN = Shader.PropertyToID("_CLOUD_SCATTERING_DENSITYGAIN");
                public static int _CLOUD_SCATTERING_OCTAVES = Shader.PropertyToID("_CLOUD_SCATTERING_OCTAVES");

                public static int _CLOUD_SUBPIXEL_JITTER_ON = Shader.PropertyToID("_CLOUD_SUBPIXEL_JITTER_ON");
                public static int _CLOUD_WEATHERMAP_TEX = Shader.PropertyToID("_CLOUD_WEATHERMAP_TEX");
                public static int _CLOUD_WEATHERMAP_VELOCITY = Shader.PropertyToID("_CLOUD_WEATHERMAP_VELOCITY");
                public static int _CLOUD_WEATHERMAP_SCALE = Shader.PropertyToID("_CLOUD_WEATHERMAP_SCALE");
                public static int _CLOUD_WEATHERMAP_VALUE_RANGE = Shader.PropertyToID("_CLOUD_WEATHERMAP_VALUE_RANGE");
                public static int _USE_CLOUD_WEATHERMAP_TEX = Shader.PropertyToID("_USE_CLOUD_WEATHERMAP_TEX");

                public static int _CLOUD_DENSITY_CURVE_TEX = Shader.PropertyToID("_CLOUD_DENSITY_CURVE_TEX");

                public static int _WEATHERMAP_OCTAVES = Shader.PropertyToID("_WEATHERMAP_OCTAVES");
                public static int _WEATHERMAP_GAIN = Shader.PropertyToID("_WEATHERMAP_GAIN");
                public static int _WEATHERMAP_LACUNARITY = Shader.PropertyToID("_WEATHERMAP_LACUNARITY");
            }
        }

        public static void ConfigureTAAParams(CommandBuffer cmd, CloudDefinition cloudDefinition)
        {
            cmd.SetGlobalFloat(ShaderParams.taaBlendFactorReference, cloudDefinition.temporalDenoisingFactor);
        }

        public static void IgnoreTAAThisFrame(CommandBuffer cmd)
        {
            cmd.SetGlobalFloat(ShaderParams.taaBlendFactorReference, 1);
        }

        static Vector4[] lightningArray = new Vector4[8];

        public static void SetCloudMaterialSettings(CommandBuffer cmd, CloudDefinition d)
        {
            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_AMBIENT_EXPOSURE, d.ambientExposure);
            cmd.SetGlobalInt(ShaderParams.CloudData._CheapAmbientLighting, d.cheapAmbientLighting ? 1 : 0);

            cmd.SetGlobalFloat(ShaderParams.CloudData._COARSE_STEPS, d.coarseSteps);
            cmd.SetGlobalFloat(ShaderParams.CloudData._DETAIL_STEPS, d.detailSteps);
            cmd.SetGlobalFloat(ShaderParams.CloudData._MAXIMUM_EMPTY_DETAIL_STEPS, d.maximumEmptyDetailSteps);

            cmd.SetGlobalVector(ShaderParams.CloudData._MIPMAP_STAGES, d.mipmapStages);

            cmd.SetGlobalFloat(ShaderParams.CloudData._SHADING_STRENGTH_FALLOFF, d.shadingStrengthFalloff);
            cmd.SetGlobalFloat(ShaderParams.CloudData._IN_SCATTERING_MULTIPLIER, d.inScatteringMultiplier);
            cmd.SetGlobalFloat(ShaderParams.CloudData._IN_SCATTERING_STRENGTH, d.inScatteringStrength);

            cmd.SetGlobalVector(ShaderParams.CloudData._CLOUD_BASE_SCALE, d.baseTextureScale);
            cmd.SetGlobalTexture(ShaderParams.CloudData._CLOUD_BASE_TEX, d.baseTexture);
            cmd.SetGlobalVector(ShaderParams.CloudData._CLOUD_BASE_TIMESCALE, d.baseTextureTimescale);
            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_BLUE_NOISE_STRENGTH, d.blueNoise);
            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_COVERAGE, d.currentCloudiness);
            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_COVERAGE_DENSITY_INFLUENCE, d.cloudinessDensityInfluence);

            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_CURL_SCALE, d.curlTextureScale);
            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_CURL_STRENGTH, d.curlTextureInfluence);
            cmd.SetGlobalTexture(ShaderParams.CloudData._CLOUD_CURL_TEX, d.curlTexture);
            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_CURL_TIMESCALE, d.curlTextureTimescale);

            cmd.SetGlobalVector(ShaderParams.CloudData._CLOUD_DETAIL1_SCALE, d.detail1TextureScale);
            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_DETAIL1_STRENGTH, d.detail1TextureInfluence);
            cmd.SetGlobalTexture(ShaderParams.CloudData._CLOUD_DETAIL1_TEX, d.detail1Texture);
            cmd.SetGlobalVector(ShaderParams.CloudData._CLOUD_DETAIL1_TIMESCALE, d.detail1TextureTimescale);

            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_DISTANT_CLOUD_COVERAGE, d.distantCoverageAmount);
            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_DISTANT_COVERAGE_START_DEPTH, d.distantCoverageDepth);
            cmd.SetGlobalInt(ShaderParams.CloudData._USE_DISTANT_COVERAGE_OVERRIDE, d.overrideDistantCoverage ? 1 : 0);
            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_EXTINCTION_COEFFICIENT, d.extinctionCoefficient);
            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_FADE_DIST, d.cloudFadeDistance);
            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_FOG_POWER, d.GetAtmosphereAttenuationDensity());
            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_HEIGHT_DENSITY_INFLUENCE, d.heightDensityInfluence);

            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_HGFORWARD, d.HGEccentricityForward);
            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_HGBACK, d.HGEccentricityBackward);
            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_HGSTRENGTH, d.HGStrength);

            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_HIGHALT_COVERAGE, d.highAltCloudiness);
            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_HIGHALT_EXTINCTION, d.highAltExtinctionCoefficient);
            cmd.SetGlobalVector(ShaderParams.CloudData._CLOUD_HIGHALT_OFFSET1, d.highAltTimescale1);
            cmd.SetGlobalVector(ShaderParams.CloudData._CLOUD_HIGHALT_OFFSET2, d.highAltTimescale2);
            cmd.SetGlobalVector(ShaderParams.CloudData._CLOUD_HIGHALT_OFFSET3, d.highAltTimescale3);
            cmd.SetGlobalVector(ShaderParams.CloudData._CLOUD_HIGHALT_SCALE1, d.highAltScale1);
            cmd.SetGlobalVector(ShaderParams.CloudData._CLOUD_HIGHALT_SCALE2, d.highAltScale2);
            cmd.SetGlobalVector(ShaderParams.CloudData._CLOUD_HIGHALT_SCALE3, d.highAltScale3);
            cmd.SetGlobalTexture(ShaderParams.CloudData._CLOUD_HIGHALT_TEX_1, d.highAltTex1);
            cmd.SetGlobalTexture(ShaderParams.CloudData._CLOUD_HIGHALT_TEX_2, d.highAltTex2);
            cmd.SetGlobalTexture(ShaderParams.CloudData._CLOUD_HIGHALT_TEX_3, d.highAltTex3);

            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_LAYER_HEIGHT, d.cloudLayerHeight);
            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_LAYER_THICKNESS, d.cloudLayerThickness);
            cmd.SetGlobalInt(ShaderParams.CloudData._CLOUD_MAX_LIGHTING_DIST, d.maxLightingDistance);
            cmd.SetGlobalInt(ShaderParams.CloudData._CLOUD_PLANET_RADIUS, d.planetRadius);
            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_SCATTERING_AMPGAIN, d.multipleScatteringAmpGain);
            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_SCATTERING_DENSITYGAIN, d.multipleScatteringDensityGain);
            cmd.SetGlobalInt(ShaderParams.CloudData._CLOUD_SCATTERING_OCTAVES, d.multipleScatteringOctaves);
            cmd.SetGlobalInt(ShaderParams.CloudData._CLOUD_STEP_COUNT, d.stepCount);
            cmd.SetGlobalVector(ShaderParams.CloudData._CLOUD_SUN_COLOR_MASK, d.sunColor);
            cmd.SetGlobalVector(ShaderParams.CloudData._CLOUD_ALBEDO, d.cloudAlbedoColor);

            cmd.SetGlobalInt(ShaderParams.CloudData._CLOUD_SUBPIXEL_JITTER_ON, d.subpixelJitterEnabled == true ? 1 : 0);
            cmd.SetGlobalTexture(ShaderParams.CloudData._CLOUD_WEATHERMAP_TEX, d.weathermapTexture);
            cmd.SetGlobalVector(ShaderParams.CloudData._CLOUD_WEATHERMAP_VELOCITY, d.weathermapVelocity);
            cmd.SetGlobalFloat(ShaderParams.CloudData._CLOUD_WEATHERMAP_SCALE, d.weathermapScale);
            cmd.SetGlobalInt(ShaderParams.CloudData._USE_CLOUD_WEATHERMAP_TEX, d.weathermapType == WeathermapType.Texture ? 1 : 0);

            cmd.SetGlobalFloat(ShaderParams.CloudData._WEATHERMAP_OCTAVES, d.weathermapOctaves);
            cmd.SetGlobalFloat(ShaderParams.CloudData._WEATHERMAP_GAIN, d.weathermapGain);
            cmd.SetGlobalFloat(ShaderParams.CloudData._WEATHERMAP_LACUNARITY, d.weathermapLacunarity);

            int lightningArraySize = 0;
            foreach (AltosLight light in AltosLight.altosLightMap)
            {
                lightningArray[lightningArraySize] = new Vector4(light.Position.x, light.Position.y, light.Position.z, light.Intensity);
                lightningArraySize++;
            }

            cmd.SetGlobalVectorArray("altos_LightningArray", lightningArray);
            cmd.SetGlobalInt("altos_LightningArraySize", lightningArraySize);
        }
    }
}
