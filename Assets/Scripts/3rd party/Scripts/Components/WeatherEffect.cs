using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.VFX;
using Viva.Util;

namespace OccaSoftware.Altos.Runtime
{
    [AddComponentMenu("OccaSoftware/Altos/Weather Effect")]
    public class WeatherEffect : MonoBehaviour
    {
        [Header("Components")]
        public VisualEffect visualEffect;

        [Header("Follow Options")]
        public CameraFollowSettings cameraFollowSettings = new CameraFollowSettings();

        [System.Serializable]
        public class CameraFollowSettings
        {
            public bool enabled;
            public Camera mainCamera;
            public Vector3 positionOffset = new Vector3(0, 5, 0);
        }

        [Header("Precipitation Options")]
        public WeathermapIntegrationSettings weathermapIntegrationSettings = new WeathermapIntegrationSettings();
        public PrecipitationCollisionSettings precipitationCollisionSettings = new PrecipitationCollisionSettings();

        [Header("Temperature Options")]
        public TemperatureIntegrationSettings temperatureIntegrationSettings = new TemperatureIntegrationSettings();

        [Header("Wind Zone Options")]
        public WindZoneIntegrationSettings windZoneIntegrationSettings = new WindZoneIntegrationSettings();

        [Header("Origin Management Options")]
        public OriginManagementSettings originManagementSettings = new OriginManagementSettings();

        [Header("Time of Day Options")]
        public DaytimeFactorSettings daytimeFactorSettings = new DaytimeFactorSettings();

        [System.Serializable]
        public class OriginManagementSettings
        {
            public bool enabled;
            public string originOffsetPropertyName = "altos_Origin";
        }

        [System.Serializable]
        public class TemperatureIntegrationSettings
        {
            public bool enabled;
            public string temperaturePropertyName = "altos_Temperature";
        }

        [System.Serializable]
        public class WindZoneIntegrationSettings
        {
            public bool enabled;
            public string windZonePropertyName = "altos_WindZone";
        }

        [System.Serializable]
        public class DaytimeFactorSettings
        {
            public bool enabled;
            public string daytimeFactorPropertyName = "altos_DaytimeFactor";
        }

        [System.Serializable]
        public class WeathermapIntegrationSettings
        {
            public bool enabled;
            public bool onlyPrecipitateBelowCloudLayer;
            public string intensityPropertyName = "altos_PrecipitationAmount";
        }

        [System.Serializable]
        public class PrecipitationCollisionSettings
        {
            public bool enabled;
            public Camera camera;
            public string texture = "altos_PrecipitationDepthBufferTexture";
            public string enabledProperty = "altos_PrecipitationDepthEnabled";
            public string worldToCameraMatrixProperty = "altos_PrecipitationDepthWorldToCameraMatrix";
            public string cameraOrthographicSizeProperty = "altos_PrecipitationDepthCameraOrthographicSize";
            public string cameraClipPlanesProperty = "altos_PrecipitationDepthCameraClipPlanes";
        }

        public void Reset()
        {
            visualEffect = GetComponent<VisualEffect>();
        }

        private void LateUpdate()
        {
            UpdatePosition();
            UpdateProperties();
        }

        private void UpdatePosition()
        {
            if (!cameraFollowSettings.enabled)
                return;

            if (cameraFollowSettings.mainCamera == null)
                return;

            transform.position = cameraFollowSettings.mainCamera.transform.position + cameraFollowSettings.positionOffset;
        }

        void UpdateProperties()
        {
            if (AltosSkyDirector.Instance == null || visualEffect == null)
                return;
            
            UpdatePrecipitation();
            UpdateWindZone();
            UpdateDaytimeFactor();
            UpdatePrecipitationDepthBuffer();
        }

        private void UpdatePrecipitation()
        {
            if (!weathermapIntegrationSettings.enabled)
                return;

            if (AltosSkyDirector.Instance.weathermap == null)
                return;
            
            float intensity = AltosSkyDirector.Instance.weathermap.GetIntensity(transform.position);

            if (weathermapIntegrationSettings.onlyPrecipitateBelowCloudLayer)
            {
                float intensityModifierByHeight = 1.0f - Tools.RemapTo01(transform.position.y, 
                    AltosSkyDirector.Instance.cloudDefinition.GetCloudFloor(),
                    AltosSkyDirector.Instance.cloudDefinition.GetCloudCenter());

                intensity *= intensityModifierByHeight;
            }

            visualEffect.SetFloat(weathermapIntegrationSettings.intensityPropertyName, intensity);
        }
        

        private void UpdateWindZone()
        {
            if (!windZoneIntegrationSettings.enabled)
                return;

            if (AltosSkyDirector.Instance.GetWind(out AltosWindZone altosWindZone))
            {
                visualEffect.SetVector3(windZoneIntegrationSettings.windZonePropertyName, altosWindZone.GetVFXWindData().velocity);
            }
        }

        void UpdateDaytimeFactor()
        {
            if (!daytimeFactorSettings.enabled)
                return;
            visualEffect.SetFloat(daytimeFactorSettings.daytimeFactorPropertyName, AltosSkyDirector.Instance.daytimeFactor);
        }

        void UpdatePrecipitationDepthBuffer()
        {
            if (!precipitationCollisionSettings.enabled)
                return;

            if (precipitationCollisionSettings.camera == null)
            {
                return;
            }

            if (precipitationCollisionSettings.camera.targetTexture == null)
            {
                return;
            }

            visualEffect.SetBool(precipitationCollisionSettings.enabledProperty, precipitationCollisionSettings.enabled);
            visualEffect.SetTexture(precipitationCollisionSettings.texture, precipitationCollisionSettings.camera.targetTexture);
            visualEffect.SetVector2(
                precipitationCollisionSettings.cameraClipPlanesProperty,
                new Vector2(precipitationCollisionSettings.camera.nearClipPlane, precipitationCollisionSettings.camera.farClipPlane)
            );
            visualEffect.SetMatrix4x4(
                precipitationCollisionSettings.worldToCameraMatrixProperty,
                precipitationCollisionSettings.camera.worldToCameraMatrix
            );
            visualEffect.SetFloat(
                precipitationCollisionSettings.cameraOrthographicSizeProperty,
                precipitationCollisionSettings.camera.orthographicSize
            );
        }
    }
}
