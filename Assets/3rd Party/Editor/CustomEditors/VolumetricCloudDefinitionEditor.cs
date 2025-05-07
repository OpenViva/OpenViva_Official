using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using OccaSoftware.Altos.Runtime;

namespace OccaSoftware.Altos.Editor
{
    [CustomEditor(typeof(CloudDefinition))]
    [CanEditMultipleObjects]
    public class VolumetricCloudDefinitionEditor : UnityEditor.Editor
    {
        private class EditorParams
        {
            public SerializedProperty pageSelection;
            public SerializedProperty lowAltitudeModelingState;
            public SerializedProperty lowAltitudeLightingState;
            public SerializedProperty lowAltitudeWeatherState;
            public SerializedProperty lowAltitudeBaseState;
            public SerializedProperty lowAltitudeDetail1State;
            public SerializedProperty lowAltitudeCurlState;

            public EditorParams(SerializedObject serializedObject)
            {
                SetFieldsByName(serializedObject, this);
            }
        }

        private class BasicParams
        {
            public SerializedProperty stepCount;
            public SerializedProperty coarseSteps;
            public SerializedProperty detailSteps;
            public SerializedProperty maximumEmptyDetailSteps;

            public SerializedProperty blueNoise;
            public SerializedProperty depthOptions;
            public SerializedProperty useReprojection;
            public SerializedProperty resolutionOptions;
            public SerializedProperty useTemporalDenoising;

            public SerializedProperty mipmapStages;

            public SerializedProperty cloudAlbedoColor;
            public SerializedProperty sunColor;
            public SerializedProperty ambientExposure;
            public SerializedProperty cheapAmbientLighting;
            public SerializedProperty HGEccentricityForward;
            public SerializedProperty HGEccentricityBackward;
            public SerializedProperty HGBlend;
            public SerializedProperty HGStrength;

            public SerializedProperty celestialBodySelection;
            public SerializedProperty planetRadius;
            public SerializedProperty cloudLayerHeight;
            public SerializedProperty cloudLayerThickness;
            public SerializedProperty cloudFadeDistance;
            public SerializedProperty overrideAtmosphericVisibility;
            public SerializedProperty visibility;

            public SerializedProperty renderInSceneView;
            public SerializedProperty temporalDenoisingFactor;
            public SerializedProperty subpixelJitterEnabled;

            public SerializedProperty castShadowsEnabled;
            public SerializedProperty screenShadows;
            public SerializedProperty shadowStrength;
            public SerializedProperty shadowResolution;
            public SerializedProperty shadowStepCount;
            public SerializedProperty shadowDistance;

            public BasicParams(SerializedObject serializedObject)
            {
                SetFieldsByName(serializedObject, this);
            }
        }

        public class LowAltitudeParams
        {
            public SerializedProperty extinctionCoefficient;

            // Cloudiness stuff
            public SerializedProperty cloudiness;
            public SerializedProperty currentCloudiness;
            public SerializedProperty dynamicCloudiness;
            public SerializedProperty cloudinessMin;
            public SerializedProperty cloudinessMax;

            public SerializedProperty heightDensityInfluence;
            public SerializedProperty cloudinessDensityInfluence;
            public SerializedProperty distantCoverageDepth;
            public SerializedProperty overrideDistantCoverage;
            public SerializedProperty distantCoverageAmount;

            public SerializedProperty maxLightingDistance;
            public SerializedProperty shadingStrengthFalloff;
            public SerializedProperty inScatteringMultiplier;
            public SerializedProperty inScatteringStrength;
            public SerializedProperty multipleScatteringAmpGain;
            public SerializedProperty multipleScatteringDensityGain;
            public SerializedProperty multipleScatteringOctaves;

            public LowAltitudeParams(SerializedObject serializedObject)
            {
                SetFieldsByName(serializedObject, this);
            }
        }

        private class WeatherParams
        {
            public SerializedProperty weathermapTexture;
            public SerializedProperty weathermapVelocity;
            public SerializedProperty weathermapScale;
            public SerializedProperty weathermapType;
            public SerializedProperty weathermapGain;
            public SerializedProperty weathermapLacunarity;
            public SerializedProperty weathermapOctaves;

            public WeatherParams(SerializedObject serializedObject)
            {
                SetFieldsByName(serializedObject, this);
            }
        }

        private class BaseParams
        {
            public SerializedProperty baseTextureID;
            public SerializedProperty baseTextureQuality;
            public SerializedProperty baseTextureScale;
            public SerializedProperty baseTextureTimescale;

            public SerializedProperty baseFalloffSelection;
            public SerializedProperty baseTextureRInfluence;
            public SerializedProperty baseTextureGInfluence;
            public SerializedProperty baseTextureBInfluence;
            public SerializedProperty baseTextureAInfluence;

            public BaseParams(SerializedObject serializedObject)
            {
                SetFieldsByName(serializedObject, this);
            }
        }

        private class DetailParams
        {
            public SerializedProperty detail1TextureID;
            public SerializedProperty detail1TextureQuality;
            public SerializedProperty detail1TextureInfluence;
            public SerializedProperty detail1TextureScale;
            public SerializedProperty detail1TextureTimescale;

            public SerializedProperty detail1FalloffSelection;
            public SerializedProperty detail1TextureRInfluence;
            public SerializedProperty detail1TextureGInfluence;
            public SerializedProperty detail1TextureBInfluence;
            public SerializedProperty detail1TextureAInfluence;

            public SerializedProperty detail1TextureHeightRemap;

            public DetailParams(SerializedObject serializedObject)
            {
                SetFieldsByName(serializedObject, this);
            }
        }

        private class CurlParams
        {
            public SerializedProperty curlTexture;
            public SerializedProperty curlTextureInfluence;
            public SerializedProperty curlTextureScale;
            public SerializedProperty curlTextureTimescale;

            public CurlParams(SerializedObject serializedObject)
            {
                SetFieldsByName(serializedObject, this);
            }
        }

        private class HighAltitudeParams
        {
            public SerializedProperty highAltExtinctionCoefficient;
            public SerializedProperty highAltCloudiness;

            public SerializedProperty highAltTex1;
            public SerializedProperty highAltScale1;
            public SerializedProperty highAltTimescale1;

            public SerializedProperty highAltTex2;
            public SerializedProperty highAltScale2;
            public SerializedProperty highAltTimescale2;

            public SerializedProperty highAltTex3;
            public SerializedProperty highAltScale3;
            public SerializedProperty highAltTimescale3;

            public HighAltitudeParams(SerializedObject serializedObject)
            {
                SetFieldsByName(serializedObject, this);
            }
        }

        private static void SetFieldsByName<T>(SerializedObject serializedObject, T target)
        {
            foreach (FieldInfo field in typeof(T).GetFields())
            {
                field.SetValue(target, serializedObject.FindProperty(field.Name));
            }
        }

        private EditorParams editorParams;
        private BasicParams basicParams;
        private LowAltitudeParams lowAltitudeParams;
        private WeatherParams weatherParams;
        private BaseParams baseParams;
        private DetailParams detailParams;
        private CurlParams curlParams;
        private HighAltitudeParams highAltitudeParams;

        private void OnEnable()
        {
            editorParams = new EditorParams(serializedObject);
            basicParams = new BasicParams(serializedObject);
            lowAltitudeParams = new LowAltitudeParams(serializedObject);
            weatherParams = new WeatherParams(serializedObject);
            baseParams = new BaseParams(serializedObject);
            detailParams = new DetailParams(serializedObject);
            curlParams = new CurlParams(serializedObject);
            highAltitudeParams = new HighAltitudeParams(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //DrawCloudDefinitionHeader();
            Draw();

            serializedObject.ApplyModifiedProperties();
        }

        private void Draw()
        {
            EditorGUILayout.LabelField("Common Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            DrawVolumetricBasicSetup();
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("Low Altitude", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            HandleLowAltitudeDrawing();
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("High Altitude", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            HandleHighAltitudeDrawing();
            EditorGUI.indentLevel--;
        }

        #region Draw Basic Setup
        void DrawVolumetricBasicSetup()
        {
            #region Basic Setup
            EditorHelpers.HandleIndentedGroup("Rendering", DrawBasicRendering);
            EditorHelpers.HandleIndentedGroup("Editor", DrawEditorOptions);
            EditorHelpers.HandleIndentedGroup("Lighting", DrawBasicLighting);
            EditorHelpers.HandleIndentedGroup("Atmosphere", DrawBasicAtmosphere);
            EditorHelpers.HandleIndentedGroup("Shadows", DrawBasicShadows);
            #endregion
        }

        void DrawBasicRendering()
        {
            EditorGUILayout.PropertyField(basicParams.coarseSteps);
            EditorGUILayout.PropertyField(basicParams.detailSteps);
            EditorGUILayout.PropertyField(basicParams.maximumEmptyDetailSteps);

            EditorGUILayout.Slider(basicParams.blueNoise, 0f, 1f, new GUIContent("Noise"));

            EditorGUILayout.PropertyField(basicParams.depthOptions);
            EditorGUILayout.PropertyField(basicParams.useReprojection);
            EditorGUILayout.PropertyField(basicParams.resolutionOptions);
            EditorGUILayout.PropertyField(basicParams.useTemporalDenoising);
            EditorGUI.indentLevel++;
            EditorGUILayout.Slider(basicParams.temporalDenoisingFactor, 0f, 1f);
            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(basicParams.subpixelJitterEnabled);
            EditorGUILayout.PropertyField(basicParams.mipmapStages);
        }

        void DrawEditorOptions()
        {
            EditorGUILayout.PropertyField(basicParams.renderInSceneView);
        }

        void DrawBasicLighting()
        {
            basicParams.cloudAlbedoColor.colorValue = EditorGUILayout.ColorField(
                new GUIContent("Cloud Albedo Color", "Default: RGB(1.0f, 0.964f, 0.92f)"),
                basicParams.cloudAlbedoColor.colorValue,
                false,
                false,
                false
            );

            basicParams.sunColor.colorValue = EditorGUILayout.ColorField(
                new GUIContent("Sun Color Mask", "This value is multiplied by the color of your main directional light."),
                basicParams.sunColor.colorValue,
                false,
                false,
                true
            );
            EditorGUILayout.PropertyField(basicParams.ambientExposure);
            EditorGUILayout.PropertyField(basicParams.cheapAmbientLighting);
            EditorGUILayout.Slider(basicParams.HGStrength, 0f, 1f);
            EditorGUILayout.Slider(basicParams.HGEccentricityForward, 0f, 0.99f);
            EditorGUILayout.Slider(basicParams.HGEccentricityBackward, -0.99f, 0f);
        }

        void DrawBasicAtmosphere()
        {
            EditorGUILayout.PropertyField(basicParams.celestialBodySelection, new GUIContent("Planet Radius"));

            if (GetEnum<CelestialBodySelection>(basicParams.celestialBodySelection) == CelestialBodySelection.Custom)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(
                    basicParams.planetRadius,
                    new GUIContent(
                        "Planet Radius (km)",
                        "Sets the size of the simulated planet. Larger values mean the cloud layer appears flatter. Smaller values mean the cloud layer is more obviously curved."
                    )
                );
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(
                basicParams.cloudLayerHeight,
                new GUIContent("Cloud Layer Altitude (m)", "Sets the floor of the cloud layer. Larger values mean the clouds start higher up.")
            );
            EditorGUILayout.Slider(
                basicParams.cloudLayerThickness,
                100f,
                8000f,
                new GUIContent("Cloud Layer Thickness (m)", "Sets the size of the cloud layer. Larger values mean taller clouds.")
            );
            EditorGUILayout.PropertyField(
                basicParams.cloudFadeDistance,
                new GUIContent(
                    "Max Render Distance (m)",
                    "Sets the maximum rendering distance for clouds. Larger values mean clouds render farther away."
                )
            );

            EditorGUILayout.PropertyField(basicParams.overrideAtmosphericVisibility);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                basicParams.visibility,
                new GUIContent(
                    "Visibility",
                    "Sets the distance over which the clouds fade into the atmosphere. Larger values mean the clouds remain visible for longer."
                )
            );
            EditorGUI.indentLevel--;
        }

        void DrawBasicShadows()
        {
            EditorGUILayout.PropertyField(basicParams.castShadowsEnabled, new GUIContent("Cast Cloud Shadows"));
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                basicParams.screenShadows,
                new GUIContent(
                    "Screen Shadows Enabled",
                    "When enabled, Altos applies cloud shadows for you as a post-process. This is easy, but it is not physically realistic: It will equally attenuate ambient, additional, and direct lighting. This only applies to depth-tested opaque geometry. When disabled, Altos allows you to write your own shadow sampler into your frag shader stage for objects in your scene for more realistic results. More complicated, but better results. See docs for details."
                )
            );
            EditorGUILayout.PropertyField(basicParams.shadowResolution, new GUIContent("Resolution"));
            EditorGUILayout.PropertyField(basicParams.shadowStepCount, new GUIContent("Step Count"));
            EditorGUILayout.PropertyField(basicParams.shadowStrength, new GUIContent("Strength"));
            EditorGUILayout.PropertyField(
                basicParams.shadowDistance,
                new GUIContent("Max Distance (m)", "Radius around the camera in which cloud shadows will be rendered.")
            );
            EditorGUI.indentLevel--;
        }

        #endregion

        #region Low Altitude
        void HandleLowAltitudeDrawing()
        {
            EditorHelpers.HandleIndentedGroup("Modeling", DrawLowAltitudeModeling);
            EditorHelpers.HandleIndentedGroup("Lighting", DrawLowAltitudeLighting);
            EditorHelpers.HandleIndentedGroup("Weather", DrawLowAltitudeWeather);
            EditorHelpers.HandleIndentedGroup("Base Clouds", DrawLowAltitudeBase);

            if (GetEnum<TextureIdentifier>(baseParams.baseTextureID) != TextureIdentifier.None)
            {
                EditorHelpers.HandleIndentedGroup("Cloud Detail", DrawLowAltitudeDetail1);
                EditorHelpers.HandleIndentedGroup("Cloud Distortion", DrawLowAltitudeDistortion);
            }
        }

        void DrawLowAltitudeModeling()
        {
            #region Modeling
            EditorGUILayout.LabelField("Density", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(lowAltitudeParams.extinctionCoefficient);
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("Cloudiness", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(lowAltitudeParams.dynamicCloudiness);

            if (lowAltitudeParams.dynamicCloudiness.boolValue)
            {
                EditorGUILayout.LabelField($"Current Cloudiness: {lowAltitudeParams.currentCloudiness.floatValue.ToString("0.00")}");
                EditorGUILayout.PropertyField(lowAltitudeParams.cloudinessMin);
                EditorGUILayout.PropertyField(lowAltitudeParams.cloudinessMax);
            }
            else
            {
                EditorGUILayout.Slider(lowAltitudeParams.cloudiness, 0f, 1f);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("Influence Factors", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.Slider(lowAltitudeParams.cloudinessDensityInfluence, 0f, 1f);
            EditorGUILayout.Slider(lowAltitudeParams.heightDensityInfluence, 0f, 1f);
            EditorGUI.indentLevel--;
            #endregion

            #region Distant Coverage Configuration
            // Distant Coverage Configuration
            EditorGUILayout.LabelField("Distant Coverage", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(lowAltitudeParams.overrideDistantCoverage);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(lowAltitudeParams.distantCoverageDepth, new GUIContent("Start Distance (m)"));
            EditorGUILayout.Slider(lowAltitudeParams.distantCoverageAmount, 0f, 1f, new GUIContent("Cloudiness"));
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
            #endregion
        }

        void DrawLowAltitudeLighting()
        {
            EditorGUILayout.PropertyField(lowAltitudeParams.maxLightingDistance);
            EditorGUILayout.PropertyField(lowAltitudeParams.shadingStrengthFalloff);
            EditorGUILayout.PropertyField(lowAltitudeParams.inScatteringMultiplier);
            EditorGUILayout.PropertyField(lowAltitudeParams.inScatteringStrength);
            EditorGUILayout.LabelField("Multiple Scattering", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.IntSlider(lowAltitudeParams.multipleScatteringOctaves, 1, 4, new GUIContent("Octaves"));
            if (lowAltitudeParams.multipleScatteringOctaves.intValue > 1)
            {
                EditorGUILayout.Slider(lowAltitudeParams.multipleScatteringAmpGain, 0f, 1f, new GUIContent("Amp Gain"));
                EditorGUILayout.Slider(lowAltitudeParams.multipleScatteringDensityGain, 0f, 1f, new GUIContent("Density Gain"));
            }
            EditorGUI.indentLevel--;
        }

        void DrawLowAltitudeWeather()
        {
            EditorGUILayout.PropertyField(weatherParams.weathermapType);

            if (weatherParams.weathermapType.enumValueIndex == (int)WeathermapType.Texture)
            {
                EditorGUILayout.PropertyField(weatherParams.weathermapTexture);
            }
            else
            {
                EditorGUILayout.LabelField("Type: Perlin");
                EditorGUILayout.PropertyField(weatherParams.weathermapOctaves, new GUIContent("Octaves"));
                if (weatherParams.weathermapOctaves.intValue > 1)
                {
                    EditorGUILayout.PropertyField(weatherParams.weathermapGain, new GUIContent("Gain"));
                    EditorGUILayout.PropertyField(weatherParams.weathermapLacunarity, new GUIContent("Frequency"));
                }

                EditorGUILayout.PropertyField(weatherParams.weathermapScale, new GUIContent("Scale"));
            }

            EditorGUILayout.PropertyField(weatherParams.weathermapVelocity, new GUIContent("Velocity"));
        }

        #region Draw Base
        void DrawLowAltitudeBase()
        {
            EditorGUILayout.PropertyField(baseParams.baseTextureID, new GUIContent("Type"));

            if (GetEnum<TextureIdentifier>(baseParams.baseTextureID) != TextureIdentifier.None)
            {
                EditorGUILayout.PropertyField(baseParams.baseTextureQuality, new GUIContent("Quality"));
                EditorGUILayout.Space(5);

                //EditorGUILayout.LabelField("Scale", EditorStyles.boldLabel);
                float s = EditorGUILayout.FloatField("Scale", baseParams.baseTextureScale.vector3Value.x);
                baseParams.baseTextureScale.vector3Value = new Vector3(s, s, s);
                EditorGUILayout.PropertyField(baseParams.baseTextureTimescale, new GUIContent("Velocity"));
                EditorGUILayout.Space(5);
            }
        }
        #endregion


        #region Draw Detail
        private void DrawLowAltitudeDetail1()
        {
            DetailData detailData = new DetailData
            {
                texture = detailParams.detail1TextureID,
                quality = detailParams.detail1TextureQuality,
                influence = detailParams.detail1TextureInfluence,
                heightRemap = detailParams.detail1TextureHeightRemap,
                scale = detailParams.detail1TextureScale,
                timescale = detailParams.detail1TextureTimescale,
                falloffSelection = detailParams.detail1FalloffSelection,
            };

            DrawDetail(detailData);
        }

        private void DrawDetail(DetailData detailData)
        {
            EditorGUILayout.PropertyField(detailData.texture, new GUIContent("Type"));

            if (GetEnum<TextureIdentifier>(detailData.texture) != TextureIdentifier.None)
            {
                EditorGUILayout.PropertyField(detailData.quality, new GUIContent("Quality"));
                EditorGUILayout.Space(5);

                //EditorGUILayout.LabelField("Strength", EditorStyles.boldLabel);
                EditorGUILayout.Slider(detailData.influence, 0f, 1f, new GUIContent("Intensity"));
                EditorGUILayout.Space(5);

                //EditorGUILayout.LabelField("Scale", EditorStyles.boldLabel);
                float s = EditorGUILayout.FloatField("Scale", detailData.scale.vector3Value.x);
                detailData.scale.vector3Value = new Vector3(s, s, s);
                EditorGUILayout.PropertyField(detailData.timescale, new GUIContent("Velocity"));
                EditorGUILayout.Space(5);
            }
        }

        private struct DetailData
        {
            public SerializedProperty texture;
            public SerializedProperty quality;
            public SerializedProperty influence;
            public SerializedProperty heightRemap;
            public SerializedProperty scale;
            public SerializedProperty timescale;
            public SerializedProperty falloffSelection;
        }
        #endregion


        #region Draw Distortion
        private void DrawLowAltitudeDistortion()
        {
            EditorGUILayout.PropertyField(curlParams.curlTexture, new GUIContent("Texture"));
            if (curlParams.curlTexture.objectReferenceValue != null)
            {
                EditorGUILayout.PropertyField(curlParams.curlTextureInfluence, new GUIContent("Intensity"));
                EditorGUILayout.PropertyField(curlParams.curlTextureScale, new GUIContent("Scale"));
                EditorGUILayout.PropertyField(curlParams.curlTextureTimescale, new GUIContent("Speed"));
            }
        }
        #endregion
        #endregion

        void HandleHighAltitudeDrawing()
        {
            bool tex1State = highAltitudeParams.highAltTex1.objectReferenceValue != null ? true : false;
            bool tex2State = highAltitudeParams.highAltTex2.objectReferenceValue != null ? true : false;
            bool tex3State = highAltitudeParams.highAltTex3.objectReferenceValue != null ? true : false;
            bool aggTexState = tex1State || tex2State || tex3State ? true : false;

            if (aggTexState)
            {
                EditorGUILayout.LabelField("Modeling", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(highAltitudeParams.highAltExtinctionCoefficient, new GUIContent("Extinction Coefficient"));
                EditorGUILayout.Slider(highAltitudeParams.highAltCloudiness, 0f, 1f, new GUIContent("Cloudiness"));
                EditorGUI.indentLevel--;
            }

            if (aggTexState)
            {
                EditorGUILayout.Space(10f);
                EditorGUILayout.LabelField("Textures", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
            }

            EditorGUILayout.LabelField("Weathermap", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(highAltitudeParams.highAltTex1, new GUIContent("Texture"));
            if (tex1State)
            {
                EditorGUILayout.PropertyField(highAltitudeParams.highAltScale1, new GUIContent("Scale"));
                EditorGUILayout.PropertyField(highAltitudeParams.highAltTimescale1, new GUIContent("Timescale"));
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Cloud Texture 1", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(highAltitudeParams.highAltTex2, new GUIContent("Texture"));
            if (tex2State)
            {
                EditorGUILayout.PropertyField(highAltitudeParams.highAltScale2, new GUIContent("Scale"));
                EditorGUILayout.PropertyField(highAltitudeParams.highAltTimescale2, new GUIContent("Timescale"));
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Cloud Texture 2", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(highAltitudeParams.highAltTex3, new GUIContent("Texture"));
            if (tex3State)
            {
                EditorGUILayout.PropertyField(highAltitudeParams.highAltScale3, new GUIContent("Scale"));
                EditorGUILayout.PropertyField(highAltitudeParams.highAltTimescale3, new GUIContent("Timescale"));
            }
            EditorGUI.indentLevel--;

            if (aggTexState)
                EditorGUI.indentLevel--;
        }

        T GetEnum<T>(SerializedProperty property)
        {
            return (T)Enum.ToObject(typeof(T), property.enumValueIndex);
        }
    }

    internal static class EditorHelpers
    {
        public static bool HandleFoldOutGroup(bool state, string header, Action controls)
        {
            state = EditorGUILayout.BeginFoldoutHeaderGroup(state, header);

            if (state)
            {
                EditorGUI.indentLevel++;
                controls();
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(10);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            return state;
        }

        public static void HandleIndentedGroup(string header, Action controls)
        {
            EditorGUILayout.LabelField(header, EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            controls();
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10);
        }
    }
}
