using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace OccaSoftware.Altos.Runtime
{
    [AddComponentMenu("OccaSoftware/Altos/Weather Manager")]
    [ExecuteAlways]
    public class WeatherManager : MonoBehaviour
    {
        public static HashSet<WeatherManager> WeatherManagers
        {
            get => weatherManagers;
        }
        private static HashSet<WeatherManager> weatherManagers = new HashSet<WeatherManager>();

        [Range(0f, 1f)]
        public float precipitationIntensity = 1f;

        [Min(0)]
        public float radius = 1000f;

        private void OnEnable()
        {
            weatherManagers.Add(this);
        }

        private void OnDisable()
        {
            weatherManagers.Remove(this);
        }

        /// <summary>
        /// Set the precipitation amount on this Weather Zone
        /// </summary>
        /// <param name="intensity"></param>
        public void SetPrecipitation(float intensity)
        {
            precipitationIntensity = intensity;
        }

        /// <summary>
        /// Set the radius of this Weather Zone
        /// </summary>
        /// <param name="radius"></param>
        public void SetRadius(float radius)
        {
            this.radius = radius;
        }
    }
}
