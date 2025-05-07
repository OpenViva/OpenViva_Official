using UnityEngine;

namespace Viva
{

    public class CaveTransition : MonoBehaviour
    {
        public Collider caveCollider;
        public float blendDistance = 5f;

        void Update()
        {
            Vector3 closestPoint = caveCollider.ClosestPoint(GameDirector.player.head.position);
            float distance = Vector3.Distance(GameDirector.player.head.position, closestPoint);

            float blendFactor = Mathf.Clamp01(distance / blendDistance);
            GameDirector.newSkyDirector.environmentLightingExposure = Mathf.Lerp(0f, 2f, blendFactor);
        }

    }
}