using UnityEngine;

namespace Viva.Interaction
{
    public class VRPointerSource : MonoBehaviour, IPointerSource
    {
        [SerializeField] private Transform aimTransform;

        public bool IsActive => enabled && gameObject.activeInHierarchy && aimTransform != null;

        public Ray GetRay()
        {
            if (aimTransform == null) return new Ray(transform.position, transform.forward);

            return new Ray(aimTransform.position, aimTransform.forward);
        }
    }
}