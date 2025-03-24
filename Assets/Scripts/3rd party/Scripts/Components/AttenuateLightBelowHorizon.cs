using UnityEngine;

namespace OccaSoftware.Altos.Runtime
{
    [ExecuteAlways]
    [AddComponentMenu("OccaSoftware/Altos/Attenuate Light Below Horizon")]
    public class AttenuateLightBelowHorizon : MonoBehaviour
    {
        [SerializeField]
        float intensity = 1f;

        [SerializeField]
        Light lightComponent = null;

        float cachedForward = 0;

        void Update()
        {
            if (lightComponent == null)
                return;

            if (Mathf.Abs(lightComponent.transform.forward.y - cachedForward) < 0.005f)
                return;

            cachedForward = lightComponent.transform.forward.y;
            float dir = Mathf.Clamp01(-lightComponent.transform.forward.y);
            lightComponent.intensity = Mathf.Lerp(0, intensity, dir);
        }
    }
}
