using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

namespace OccaSoftware.Altos.Runtime
{

    [RequireComponent(typeof(ReflectionProbe))]
    [AddComponentMenu("OccaSoftware/Altos/Reflection Baker")]
    public class ReflectionBaker : MonoBehaviour
    {
        [Header("Time")]
        [Tooltip("Set whether the reflection probe should require a reflection trigger to update based on time changes.")]
        public bool requireReflectionTriggerForTimeUpdates;

        [Tooltip("Set the maximum world time between updates. (1 = 1 hour).")]
        public float timeThreshold = 0.1f;

        [Header("Settings")]
        [Tooltip("When enabled, this component will update your global reflection cubemap in your Environment Settings tab.")]
        public bool updateEnvironmentReflections = true;

        [Tooltip(
            "The baker needs a reference to a sky director to support time delta tracking. You can provide a specific sky director. If you don't set a value here, the baker will attempt to find a sky director for you. If it can't find a sky director, then time differential baking will be disabled."
        )]
        public AltosSkyDirector altosSkyDirector;

        [Header("Debugging")]
        public bool forceRebakeNow = false;

        float previousUpdateTime = -1;
        Vector3 previousUpdatePositionWS = Vector3.zero;
        ReflectionProbe probe;
        int renderId = -1;

        private void OnEnable()
        {
            probe = GetComponent<ReflectionProbe>();

            SetupDefaultProbeSettings();

            if (altosSkyDirector == null)
            {
                altosSkyDirector = AltosSkyDirector.Instance;
            }

            UpdateProbe();
        }

        private void LateUpdate()
        {
            if (ShouldUpdateProbe())
            {
                UpdateProbe();
            }
            UpdateEnvironmentReflections();
        }
        
        public void UpdateProbe()
        {
            Render();
            UpdateTrackedProperties();
        }

        private void SetupDefaultProbeSettings()
        {
            probe.mode = ReflectionProbeMode.Realtime;
            probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
            probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
        }

        private void Render()
        {
            renderId = probe.RenderProbe();
        }

        private void UpdateEnvironmentReflections()
        {
            if (!updateEnvironmentReflections)
                return;

            SetReflectionTexture();
        }

        private void SetReflectionTexture()
        {
            if (probe.realtimeTexture == null)
                return;

            probe.realtimeTexture.name = "Canyon Reflection Texture";
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture = probe.realtimeTexture;
        }
        
        private bool IsTimeDrivenUpdate()
        {
            if (requireReflectionTriggerForTimeUpdates)
                return false;

            if (altosSkyDirector != null)
                return false;

            float time = altosSkyDirector.skyDefinition.timeSystem;
            if (Mathf.Abs(time - previousUpdateTime) < timeThreshold)
                return false;

            return true;
        }

        private bool ShouldUpdateProbe()
        {
            if (forceRebakeNow)
            {
                forceRebakeNow = false;
                return true;
            }

            if (!probe.IsFinishedRendering(renderId))
                return false;

            if (!IsTimeDrivenUpdate())
                return false;

            return true;
        }

        private void UpdateTrackedProperties()
        {
            previousUpdatePositionWS = probe.transform.position;

            if (altosSkyDirector != null)
            {
                previousUpdateTime = altosSkyDirector.skyDefinition.timeSystem;
            }
        }
    }
}
