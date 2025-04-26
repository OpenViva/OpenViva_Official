using UnityEngine;
using UnityEngine.VFX;

namespace OccaSoftware.Altos.Runtime
{
    [ExecuteAlways]
    [RequireComponent(typeof(VisualEffect))]
    [AddComponentMenu("OccaSoftware/Altos/VFX Set Time of Day")]
    public class VFXSetTimeOfDay : MonoBehaviour
    {
        public VisualEffect visualEffect;

        void Update()
        {
            if (AltosSkyDirector.Instance == null || visualEffect == null)
            {
                return;
            }

            visualEffect.SetFloat("altos_daytimeFactor", AltosSkyDirector.Instance.daytimeFactor);
        }
    }
}
