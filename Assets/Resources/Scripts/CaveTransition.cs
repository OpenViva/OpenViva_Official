using OccaSoftware.Altos.Runtime;
using UnityEngine;

namespace Viva
{

    public class CaveTransition : MonoBehaviour
    {
        public Collider caveCollider;
        public float blendDistance = 5f;

        void Update()
        {

            AltosSkyDirector newSkyDirector = AltosSkyDirector.Instance;

            Vector3 closestPoint = caveCollider.ClosestPoint(GameDirector.player.head.position);
            float distance = Vector3.Distance(GameDirector.player.head.position, closestPoint);

            float blendFactor = Mathf.Clamp01(distance / blendDistance);
            newSkyDirector.environmentLightingExposure = Mathf.Lerp(0f, 2f, blendFactor);
        }

    }
}