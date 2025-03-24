using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Viva.Util;

namespace OccaSoftware.Altos.Runtime
{
    /// <summary>
    /// A Sky Object describes a sun, moon, or other celestial body that should be rendered independently.
    /// <br/>
    /// </summary>
    [AddComponentMenu("OccaSoftware/Altos/Sky Object")]
    public class SkyObject : MonoBehaviour
    {
        internal void Index()
        {
            GetSkyDirector()?.RegisterSkyObject(this);
        }

        internal void Delete()
        {
            GetSkyDirector()?.DeregisterSkyObject(this);
        }

        AltosSkyDirector skyDirector;

        private AltosSkyDirector GetSkyDirector()
        {
            if (skyDirector == null)
            {
                skyDirector = FindObjectOfType<AltosSkyDirector>();
            }

            return skyDirector;
        }

        public Transform GetChild()
        {
            return transform;
        }

        public Quaternion GetRotation()
        {
            return transform.rotation;
        }

        private Mesh quad = null;

        public Mesh Quad
        {
            get
            {
                if (quad == null)
                {
                    quad = Tools.CreateQuad();
                }

                return quad;
            }
        }

        private Light lightComponent;

        [Header("Identity")]
        [Tooltip(
            "When set to Sun, this object will be treated as the main directional light in the scene and will be used for shadow rendering, cloud rendering, and cloud shadow rendering. Only one sun is supported. A sun must be present in the scene."
        )]
        public ObjectType type = ObjectType.Sun;

        public enum ObjectType
        {
            Sun,
            Other
        }

        [Header("Rendering")]
        [Range(0, 90)]
        [Tooltip("The angular diameter of the object. Bigger values means a bigger object in the sky.")]
        public float angularDiameterDegrees = 0.52f;

        [Tooltip("The texture to use for the sky object. When no texture is set, the object will still be rendered as a circle.")]
        public Texture2D texture = null;

        [Tooltip("Tint color applied to the object.")]
        [ColorUsage(false, true)]
        public Color objectColor = Color.white;

        [Header("Lighting")]
        [Tooltip("Automatically set the color temperature based on the object's position in the sky.")]
        public bool automaticColor = true;

        [Tooltip("Set the color temperature curve. [0, 0.5] is below the horizon. [0.5, 1.0] is above the horizon.")]
        public AnimationCurve colorTemperatureCurve = new AnimationCurve(
            new Keyframe[] { new Keyframe(0, 2000), new Keyframe(0.45f, 2000), new Keyframe(1f, 6500) }
        );

        [Space()]
        [Tooltip("Automatically set the color brightness based on the object's position in the sky.")]
        public bool automaticBrightness = true;

        [Tooltip("Set the light intensity curve. [0, 0.5] is below the horizon. [0.5, 1.0] is above the horizon.")]
        public AnimationCurve intensityCurve = new AnimationCurve(
            new Keyframe[] { new Keyframe(0, 0), new Keyframe(0.45f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 1.5f) }
        );

        [Space()]
        [Tooltip(
            "Tint color applied to the directional light owned by the object. Lower the value to fade the object into the skybox. It will still occlude stars."
        )]
        [ColorUsage(false, true)]
        public Color lightingColorMask = Color.white;

        private Material material = null;

        public Material GetMaterial()
        {
            if (material == null)
            {
                Shader s = FindObjectOfType<AltosSkyDirector>()?.data.shaders.skyObjectShader;
                if (s != null)
                {
                    material = new Material(s);
                }
            }

            return material;
        }

        public float CalculateSize()
        {
            return Mathf.Tan(angularDiameterDegrees * Mathf.Deg2Rad) * sortOrder;
        }

        [HideInInspector]
        public Vector3 positionRelative;

        [Header("Positioning")]
        [Range(1, 10)]
        [Tooltip("Objects with a higher sort order are considered farther away and will be rendered behind objects with a lower sort order.")]
        public int sortOrder = 5;

        [Tooltip("Distance along the path around this planet.")]
        public float orbitOffset = 0f;

        [Tooltip("The angle between the path of this sky object and this planet's elliptical.")]
        public float inclinationOffset = 0f;

        [Tooltip("The orientation of the object relative to the East/West plane.")]
        public float orientationOffset = 0f;

        [Tooltip("When enabled, this sky object's position will be static throughout the day-night cycle.")]
        public bool positionIsStatic = false;

        [HideInInspector]
        public Vector3 direction;

        public Vector3 GetForward()
        {
            return -direction;
        }

        [Header("Sky Influence")]
        [Min(0)]
        [Tooltip("Higher values cause the color of this object to bleed into the sky color. Only applies for objects of type Sun.")]
        public float falloff = 1;

        [Header("Editor Properties")]
        [Tooltip("Sets the color of the handles used for this object in the editor.")]
        public Color handleColor = Color.white;

        private void OnEnable()
        {
            Index();
            SetIcon();
        }

        public void SetIcon()
        {
#if UNITY_EDITOR
            string directory = AltosData.packagePath + "/Editor/Icons/";
            string id = "sun-icon.png";
            if (type == ObjectType.Other)
            {
                id = "moon-icon.png";
            }
            Texture2D icon = (Texture2D)UnityEditor.AssetDatabase.LoadAssetAtPath(directory + id, typeof(Texture2D));
            UnityEditor.EditorGUIUtility.SetIconForObject(gameObject, icon);
#endif
        }

        private void OnDisable()
        {
            Delete();
        }

        private void OnValidate()
        {
            Index();
            SetIcon();

            if (orbitOffset > 360f)
                orbitOffset = 0f;
            if (orbitOffset < 0f)
                orbitOffset = 360f;

            if (inclinationOffset > 90f)
                inclinationOffset = -90f;
            if (inclinationOffset < -90f)
                inclinationOffset = 90f;

            if (orientationOffset < 0f)
                orientationOffset = 360f;
            if (orientationOffset > 360f)
                orientationOffset = 0f;
        }

        void Update()
        {
            UpdateRotations();
            UpdatePositionAndDirection();

            GetLight();
            if (automaticColor || automaticBrightness)
                UpdateLightProperties();

            SetShaderProperties();
        }

        public Color GetColor()
        {
            return GetLightColor();
        }

        public float GetFalloff()
        {
            return falloff;
        }

        public Vector4 GetDirection()
        {
            return direction;
        }

        /// <summary>
        /// Sets the shader properties for self-rendering and for global variables (like sun color and intensity).
        /// Local variables (like atmosphere falloff) are set in during the relevant render pass.
        /// </summary>
        private void SetShaderProperties()
        {
            if (type == ObjectType.Sun)
            {
                Shader.SetGlobalVector(ShaderParams._SunDirection, direction);
                Shader.SetGlobalColor(ShaderParams._SunColor, GetLightColor());
                Shader.SetGlobalFloat(ShaderParams._SunIntensity, GetLightIntensity());
                Shader.SetGlobalFloat(ShaderParams._SunFalloff, falloff);
            }

            GetMaterial().SetTexture(ShaderParams._MainTex, texture);

            GetMaterial().SetColor(ShaderParams._Color, objectColor);
        }

        internal static class ShaderParams
        {
            public static int _SunDirection = Shader.PropertyToID("_SunDirection");
            public static int _SunColor = Shader.PropertyToID("_SunColor");
            public static int _SunIntensity = Shader.PropertyToID("_SunIntensity");
            public static int _SunFalloff = Shader.PropertyToID("_SunFalloff");

            public static int _MainTex = Shader.PropertyToID("_MainTex");
            public static int _Color = Shader.PropertyToID("_Color");
        }

        private void UpdateRotations()
        {
            Quaternion a = Quaternion.Euler(-inclinationOffset, 0, 0);

            float timeOfDayOffset = 0f;
            if (!positionIsStatic)
            {
                if (GetSkyDirector().skyDefinition != null)
                {
                    timeOfDayOffset = GetSkyDirector().skyDefinition.CurrentTime * AltosSkyDirector._HOURS_TO_DEGREES;
                }
            }

            Quaternion b = Quaternion.Euler(0, 0, orbitOffset + timeOfDayOffset) * a;
            Quaternion c = Quaternion.Euler(0, orientationOffset, 0) * b;

            transform.position = GetSkyDirector().transform.position + c * Vector3.down * sortOrder;
            transform.LookAt(skyDirector.transform, transform.up);
        }

        private void UpdatePositionAndDirection()
        {
            positionRelative = transform.position - GetSkyDirector().transform.position;
            direction = positionRelative.normalized;
        }

        public Light GetLight()
        {
            if (lightComponent == null)
            {
                lightComponent = GetComponent<Light>();
            }

            return lightComponent;
        }

        public float GetLightIntensity()
        {
            if (lightComponent == null)
                return 0;
            return lightComponent.intensity;
        }

        public Color GetLightColor()
        {
            if (lightComponent == null)
                return objectColor;

            return Mathf.CorrelatedColorTemperatureToRGB(lightComponent.colorTemperature) * lightComponent.color;
        }

        /* Source: Wikipedia (https://en.wikipedia.org/wiki/Golden_hour_(photography))
         * The color temperature of daylight varies with the time of day.
         * It tends to be around 2,000 K shortly after sunrise or before sunset,
         * around 3,500 K during "golden hour",
         * and around 5,500 K at midday.
        */
        float cachedAngle;

        private bool UpdateLightProperties()
        {
            if (lightComponent == null)
                return false;

            lightComponent.type = LightType.Directional;
            float lightAngle = direction.y * 180f;

            lightComponent.useColorTemperature = true;
            lightComponent.color = lightingColorMask;
            float cachedTemp = lightComponent.colorTemperature;
            float cachedIntensity = lightComponent.intensity;

            // if (Mathf.Abs(cachedAngle - lightAngle) < 0.1f)
            // {
            //     return false;
            // }

            cachedAngle = lightAngle;
            float t = Tools.RemapTo01(lightAngle, -180f, 180f);
            float intensity = intensityCurve.Evaluate(t);
            float temperature = colorTemperatureCurve.Evaluate(t);

            if (automaticBrightness && skyDirector.skyOverride == null)
            {
                lightComponent.intensity = intensity;
            }

            if (automaticColor && skyDirector.skyOverride == null)
            {
                lightComponent.colorTemperature = temperature;
            }

            if (Mathf.Abs(cachedTemp - lightComponent.colorTemperature) > 1f || Mathf.Abs(cachedIntensity - lightComponent.intensity) > 0.01f)
            {
                return true;
            }

            return false;
        }

        public float CalculateDiscIntensity()
        {
            float solidAngle = 2f * Mathf.PI * (1f - Mathf.Cos(0.5f * angularDiameterDegrees * Mathf.Deg2Rad));
            float v = lightComponent.intensity / solidAngle;
            Debug.Log(v);
            return v;
        }

        #region Editor
#if UNITY_EDITOR
        Transform[] selection;

        private void OnDrawGizmos()
        {
            selection = UnityEditor.Selection.GetTransforms(
                UnityEditor.SelectionMode.ExcludePrefab | UnityEditor.SelectionMode.Editable | UnityEditor.SelectionMode.TopLevel
            );
            foreach (Transform selectedItem in selection)
            {
                if (selectedItem.root == skyDirector.transform)
                {
                    DrawGizmos();
                    return;
                }
            }
        }

        private void DrawGizmos()
        {
            UnityEditor.Handles.color = handleColor;
            Gizmos.color = handleColor;

            //float euclidianDistance = Mathf.Sqrt(positionRelative.y * positionRelative.y + positionRelative.x * positionRelative.x);
            float euclidianDistance = Vector3.Magnitude(positionRelative);
            DrawUpperArc();

            DimColorIfBelowHorizon();
            DrawDirectLine();
            DrawSphere();

            UseDimColor();
            DrawIndirectLines();
            DrawLowerArc();
            DrawDiscHighlight();

            DrawText();
        }

        private void DimColorIfBelowHorizon()
        {
            if (positionRelative.y < 0)
            {
                UnityEditor.Handles.color = new Color(handleColor.r, handleColor.g, handleColor.b, 0.2f);
                Gizmos.color = new Color(handleColor.r, handleColor.g, handleColor.b, 0.2f);
            }
        }

        private void UseDimColor()
        {
            float dimColorAlpha = 0.15f;
            UnityEditor.Handles.color = new Color(handleColor.r, handleColor.g, handleColor.b, dimColorAlpha);
            Gizmos.color = new Color(handleColor.r, handleColor.g, handleColor.b, dimColorAlpha);
        }

        private void DrawDirectLine()
        {
            UnityEditor.Handles.DrawLine(GetSkyDirector().transform.position, transform.position, 3f);
        }

        private void DrawSphere()
        {
            Gizmos.DrawWireSphere(transform.position, Tools.Remap(angularDiameterDegrees, 0, 90, 0.2f, 1f));
        }

        private void DrawIndirectLines()
        {
            UnityEditor.Handles.DrawLine(
                GetSkyDirector().transform.position,
                new Vector3(transform.position.x, GetSkyDirector().transform.position.y, transform.position.z),
                0f
            );
            UnityEditor.Handles.DrawLine(
                new Vector3(transform.position.x, GetSkyDirector().transform.position.y, transform.position.z),
                transform.position,
                0f
            );
        }

        private void DrawUpperArc()
        {
            Vector3 center = new Vector3(0, 0, sortOrder * Mathf.Sin(Mathf.Deg2Rad * inclinationOffset));
            Quaternion rot = Quaternion.Euler(0, orientationOffset, 0);
            center = rot * center;
            Vector3 normal = Vector3.forward;
            normal = rot * normal;
            Vector3 from = Vector3.right;
            from = rot * from;
            float radius = sortOrder * Mathf.Cos(Mathf.Deg2Rad * inclinationOffset);
            UnityEditor.Handles.DrawWireArc(center + GetSkyDirector().transform.position, normal, from, 180f, radius, 2f);
        }

        private void DrawLowerArc()
        {
            Vector3 center = new Vector3(0, 0, sortOrder * Mathf.Sin(Mathf.Deg2Rad * inclinationOffset));
            Quaternion rot = Quaternion.Euler(0, orientationOffset, 0);
            center = rot * center;
            Vector3 normal = Vector3.forward;
            normal = rot * normal;
            Vector3 from = Vector3.right;
            from = rot * from;
            float radius = sortOrder * Mathf.Cos(Mathf.Deg2Rad * inclinationOffset);
            UnityEditor.Handles.DrawWireArc(center + GetSkyDirector().transform.position, normal, from, -180f, radius, 2f);
        }

        private void DrawDiscHighlight()
        {
            UnityEditor.Handles.color = new Color(1, 1, 1, 0.3f);
            UnityEditor.Handles.DrawWireDisc(GetSkyDirector().transform.position, Vector3.up, sortOrder, 1f);
        }

        private void DrawText()
        {
            GUIStyle s = new GUIStyle();
            s.fontSize = 12;
            s.normal.textColor = Color.white;

            s.alignment = TextAnchor.UpperRight;
            s.fontStyle = FontStyle.Bold;

            UnityEditor.Handles.color = new Color(1, 1, 1, 0.5f);
            Vector3 offset = Vector3.right;
            UnityEditor.Handles.Label(transform.position + offset * 3f, new GUIContent($"{name.ToLower()}"), s);

            s.fontSize = 9;
            s.normal.textColor = new Color(1, 1, 1, 0.8f);
            UnityEditor.Handles.Label(
                transform.position + offset * 3f,
                new GUIContent(
                    $"\n\ndiameter\n {angularDiameterDegrees:0.00}�\n\norbit\n {orbitOffset:0.00}�\n\ninclination\n {inclinationOffset:0.00}�\n\norientation\n {orientationOffset:0.00}�\n\nintensity\n {GetLightIntensity():0.00}"
                ),
                s
            );

            UnityEditor.Handles.DrawLine(transform.position + offset * 2f, transform.position, 0f);
        }
#endif
        #endregion
    }
}
