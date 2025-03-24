using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.PlayerLoop;
using Viva.Util;

namespace OccaSoftware.Altos.Runtime
{
    [ExecuteAlways]
    [CreateAssetMenu(fileName = "Precipitation Definition", menuName = "Altos/Precipitation Definition")]
    public class PrecipitationDefinition : ScriptableObject
    {
        [Header("Static")]
        [Range(0, 1f)]
        public float precipitation;

        [Header("Dynamics")]
        public bool dynamicPrecipitation = true;

        [Tooltip("Sets the precipitation percentage.")]
        [Range(-1, 1)]
        public float precipitationMin;

        [Tooltip("Sets the precipitation percentage")]
        [Range(-1, 1)]
        public float precipitationMax;

        [Header("Debugging")]
        public float currentPrecipitation;

        /// <summary>
        /// This method is called when we want the precipitation definition to update the current precipitation.
        /// </summary>
        /// <param name="time"></param>
        public void UpdatePrecipitation(float time)
        {
            if (overrideState == OverrideState.Overriden)
                return;

            if (dynamicPrecipitation)
            {
                currentPrecipitation = GetPrecipitationAtTime(time);
            }
            else
            {
                currentPrecipitation = precipitation;
            }
        }

        public float GetPrecipitationAtTime(float time)
        {
            if (!dynamicPrecipitation)
                return currentPrecipitation;

            float a = Mathf.Floor(time);
            float b = Mathf.Ceil(time);
            float x = Mathf.Clamp01(Tools.RemapFrom01(Tools.Random(a, 609), precipitationMin, precipitationMax));
            float y = Mathf.Clamp01(Tools.RemapFrom01(Tools.Random(b, 609), precipitationMin, precipitationMax));
            float z = Tools.Smoothstep(x, y, Tools.Frac(time));
            return z;
        }

        private OverrideState overrideState = OverrideState.Normal;

        public void OverridePrecipitation(float precipitation)
        {
            currentPrecipitation = precipitation;
            overrideState = OverrideState.Overriden;
        }

        public void ReleaseOverride()
        {
            overrideState = OverrideState.Normal;
        }
    }
}
