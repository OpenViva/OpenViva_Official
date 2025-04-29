using UnityEngine;

namespace OccaSoftware.Altos.Runtime
{
    [CreateAssetMenu(fileName = "Atmosphere Definition", menuName = "Altos/Atmosphere Definition")]
    public class AtmosphereDefinition : ScriptableObject
    {
        [Tooltip("Sets the atmosphere blending start distance. In most cases, 0 is fine. You can use negative or positive values.")]
        public float start;

        [Tooltip("Sets the atmosphere blending end distance. For best results, set this to less than your camera's far clip plane.")]
        public float end = 20000;

        /// <summary>
        /// Returns the density value to be used in shaders.
        /// </summary>
        /// <returns>A density value.</returns>
        public float GetDensity()
        {

            const float factor = 3.912023005f;
            return factor / end;
        }
    }
}
