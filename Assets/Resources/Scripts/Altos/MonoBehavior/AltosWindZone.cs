using System.Collections.Generic;

using UnityEngine;
using Viva.Util;

namespace OccaSoftware.Altos.Runtime
{
    public enum OverrideState
    {
        Normal,
        Overriden
    }

    [ExecuteAlways]
    [RequireComponent(typeof(WindZone))]
    [AddComponentMenu("OccaSoftware/Altos/Altos Wind Zone")]
    public class AltosWindZone : MonoBehaviour
    {
        public WindZone windZone;

        [Header("Dynamics")]
        public bool dynamicSpeed = true;

        [Min(0)]
        public float minSpeed = 1.0f;

        [Min(0)]
        public float maxSpeed = 2.0f;
        public bool dynamicDirection = true;

        [Header("Relative Intensity")]
        public float cloudModifier = 0.1f;
        public float vfxModifier = 1.0f;

        [Header("Debugging")]
        public bool debugViewEnabled = true;

        private WindData cloudWindData = new WindData();
        private WindData vfxWindData = new WindData();

        public class WindData
        {
            public float speed = 0;
            public Vector3 velocity = Vector3.zero;
        }

        public WindData GetCloudWindData()
        {
            return cloudWindData;
        }

        public WindData GetVFXWindData()
        {
            return vfxWindData;
        }

        [SerializeField, HideInInspector]
        private float cachedWindSpeed;

        [SerializeField, HideInInspector]
        private bool cachedDynamicSpeed;

        [SerializeField, HideInInspector]
        private Vector3 cachedForward;

        [SerializeField, HideInInspector]
        private bool cachedDynamicDirection;

        private void Reset()
        {
            windZone = GetComponent<WindZone>();
        }

        private void OnEnable()
        {
            windZone = GetComponent<WindZone>();
        }

        private void OnValidate()
        {
            if (!cachedDynamicSpeed && dynamicSpeed)
            {
                cachedDynamicSpeed = dynamicSpeed;
                cachedWindSpeed = windZone.windMain;
            }
            else if (cachedDynamicSpeed && !dynamicSpeed)
            {
                cachedDynamicSpeed = dynamicSpeed;
                windZone.windMain = cachedWindSpeed;
            }

            if (!cachedDynamicDirection && dynamicDirection)
            {
                cachedDynamicDirection = dynamicDirection;
                cachedForward = windZone.transform.forward;
            }
            else if (cachedDynamicDirection && !dynamicDirection)
            {
                cachedDynamicDirection = dynamicDirection;
                windZone.transform.forward = cachedForward;
            }
        }

        private void Update()
        {
            AltosSkyDirector.Instance.SetWindZone(this);
            ChangeSpeedOverTime();
            ChangeDirectionOverTime();

            SetWindData(cloudWindData, cloudModifier);
            SetWindData(vfxWindData, vfxModifier);
        }

        private OverrideState speedState = OverrideState.Normal;
        private OverrideState directionState = OverrideState.Normal;

        public void OverrideWindSpeed(float speed)
        {
            windZone.windMain = speed;
            speedState = OverrideState.Overriden;
        }

        public void ReleaseSpeedOverride()
        {
            speedState = OverrideState.Normal;
        }

        public void OverrideDirection(Vector3 direction)
        {
            windZone.transform.forward = direction;
            directionState = OverrideState.Overriden;
        }

        public void ReleaseDirectionOverride()
        {
            directionState = OverrideState.Normal;
        }

        public float GetCurrentWindSpeed()
        {
            return windZone.windMain;
        }

        public Vector3 GetCurrentWindDirection()
        {
            return windZone.transform.forward;
        }

        // to do: probably best to cache some of these.
        // to do: might want to show these in the inspector or editor somewhere.
        private void ChangeSpeedOverTime()
        {
            UpdateWindSpeedList();
            if (dynamicSpeed && speedState == OverrideState.Normal)
            {
                windZone.windMain = GetWindSpeedAtTime(AltosSkyDirector.Instance.skyDefinition.SystemTime);
            }
        }

        private void UpdateWindSpeedList()
        {
            windSpeeds.Clear();
            for (int i = 0; i < 12; i++)
            {
                windSpeeds.Add(GetWindSpeedAtTime((int)AltosSkyDirector.Instance.skyDefinition.SystemTime + i));
            }
        }

        private void UpdateWindDirectionList()
        {
            windDirections.Clear();
            for (int i = 0; i < 12; i++)
            {
                windDirections.Add(GetWindDirectionAtTime((int)AltosSkyDirector.Instance.skyDefinition.SystemTime + i));
            }
        }

        public float GetWindSpeedAtTime(float time)
        {
            float a = Mathf.Floor(time);
            float b = Mathf.Ceil(time);

            float x = Tools.GradientNoiseLayered(a, 0.11f, 1, 2, 0.3f) + 0.5f;
            float y = Tools.GradientNoiseLayered(b, 0.11f, 1, 2, 0.3f) + 0.5f;

            float z = Tools.Smoothstep(x, y, Tools.Frac(time));

            return z * (maxSpeed - minSpeed) + minSpeed;
        }

        /// <summary>
        /// Gets the wind direction at the given time.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public Vector3 GetWindDirectionAtTime(float time)
        {
            float a = Mathf.Floor(time);
            float b = Mathf.Ceil(time);

            Vector2 x;
            Vector2 y;
            float c = Tools.GradientNoiseLayered(a, 0.09f, 1, 2, 0.4f) * 2 * Mathf.PI;
            float d = Tools.GradientNoiseLayered(b, 0.09f, 1, 2, 0.4f) * 2 * Mathf.PI;
            x.x = Mathf.Cos(c);
            x.y = Mathf.Sin(c);
            y.x = Mathf.Cos(d);
            y.y = Mathf.Sin(d);

            Vector3 z;
            z.x = Tools.Smoothstep(x.x, y.x, Tools.Frac(time));
            z.y = 0;
            z.z = Tools.Smoothstep(x.y, y.y, Tools.Frac(time));
            return z;
        }

        private List<float> windSpeeds = new List<float>(12);

        private List<Vector3> windDirections = new List<Vector3>(12);

        private void ChangeDirectionOverTime()
        {
            UpdateWindDirectionList();
            if (dynamicDirection && directionState == OverrideState.Normal)
            {
                windZone.transform.forward = GetWindDirectionAtTime(AltosSkyDirector.Instance.skyDefinition.SystemTime);
            }
        }

        private void SetWindData(WindData windData, float modifier)
        {
            windData.speed = windZone.windMain * modifier;
            windData.velocity = windData.speed * transform.forward;
        }

        private void OnDisable()
        {
            if (AltosSkyDirector.Instance?.windZone == this)
            {
                AltosSkyDirector.Instance.ClearWindZone();
            }
        }

        public static string GetCompassDirection(Vector3 direction)
        {
            // Calculate the angle of the vector in radians
            float angle = Mathf.Atan2(direction.x, direction.z);

            // Convert the angle to degrees
            float angleInDegrees = angle * Mathf.Rad2Deg;

            // Ensure the angle is positive
            if (angleInDegrees < 0)
            {
                angleInDegrees += 360;
            }

            // Map the angle to compass directions
            if (angleInDegrees >= 337.5 || angleInDegrees < 22.5)
            {
                return "N";
            }
            else if (angleInDegrees >= 22.5 && angleInDegrees < 67.5)
            {
                return "NE";
            }
            else if (angleInDegrees >= 67.5 && angleInDegrees < 112.5)
            {
                return "E";
            }
            else if (angleInDegrees >= 112.5 && angleInDegrees < 157.5)
            {
                return "SE";
            }
            else if (angleInDegrees >= 157.5 && angleInDegrees < 202.5)
            {
                return "S";
            }
            else if (angleInDegrees >= 202.5 && angleInDegrees < 247.5)
            {
                return "SW";
            }
            else if (angleInDegrees >= 247.5 && angleInDegrees < 292.5)
            {
                return "W";
            }
            else if (angleInDegrees >= 292.5 && angleInDegrees < 337.5)
            {
                return "NW";
            }
            else
            {
                return "Unknown"; // Handle any other direction as needed
            }
        }
    }
}
