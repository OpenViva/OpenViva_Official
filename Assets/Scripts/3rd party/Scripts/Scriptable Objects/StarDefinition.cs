using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace OccaSoftware.Altos.Runtime
{
    [CreateAssetMenu(fileName = "Star Definition", menuName = "Altos/Star Definition")]
    public class StarDefinition : ScriptableObject
    {
        [Header("Appearance")]
        [Tooltip(
            "Altos will render the number of stars listed here. Adjusting the number of stars forces the compute shader to rebuild, so expect jitter when changing this value."
        )]
        [Range(100, 200000)]
        public int count = 20000;
        private int count_c;

        [Tooltip("Altos will automatically assign a color to each star. These colors are based on scientific measurements of visible stars.")]
        public bool automaticColor = true;

        private bool automaticColor_c;

        [Tooltip(
            "Altos will automatically assign a brightness for each star. These brightness values are based on scientific measurements of visible stars."
        )]
        public bool automaticBrightness = true;
        private bool automaticBrightness_c;

        [Tooltip(
            "The texture used for the star color. Stars are rendered using additive blending, so a black texture will result in no star being rendered."
        )]
        public Texture2D texture;

        [Tooltip("The color used for star color."), ColorUsage(false, true)]
        public Color color = Color.white;

        [Min(0), Tooltip("Set the star brightness during the daytime.")]
        public float dayBrightness = 0.1f;

        [Min(0), Tooltip("The star brightness.")]
        public float brightness = 1;

        [Header("Flickering")]
        [Min(0), Tooltip("The rate at which the stars will flicker.")]
        public float flickerFrequency = 20;

        [Min(0), Tooltip("The intensity of the flicker effect.")]
        public float flickerStrength = 0.1f;

        [Header("Size")]
        [Min(0), Tooltip("The average size of the stars.")]
        public float size = 1;
        private float size_c;

        [Header("Positioning")]
        [Tooltip("When enabled, the stars will not automatically rotate due to time of day changes.")]
        public bool positionStatic = false;

        [Range(-180, 180), Tooltip("Tilts the star sphere.")]
        public float inclination;

        /// <summary>
        /// Checks if any values encoded in the compute shader array properties need to be reloaded.
        /// </summary>
        /// <returns>True if the compute shader needs to be rebuilt.</returns>
        public bool IsDirty()
        {
            int sCurrent = Mathf.RoundToInt(size * 100);
            int sOld = Mathf.RoundToInt(size_c * 100);
            if (Mathf.Abs(sCurrent - sOld) > 0)
            {
                size_c = size;
                return true;
            }

            if (automaticColor != automaticColor_c)
            {
                automaticColor_c = automaticColor;
                return true;
            }

            if (automaticBrightness != automaticBrightness_c)
            {
                automaticBrightness_c = automaticBrightness;
                return true;
            }

            if (count != count_c)
            {
                count_c = count;
                return true;
            }

            return false;
        }
    }
}
