using UnityEngine;


namespace Viva
{


    public class Lamp : MonoBehaviour
    {

        [SerializeField] private MeshRenderer[] targetMeshRenderers;
        [SerializeField] private GameObject lightContainer;
        [SerializeField] private bool invert;

        private static int emissionColorID = Shader.PropertyToID("_EmissionColor");


        private void Awake()
        {
            if (invert)
            {
                SetOn(true);
            }
        }

        private void OnDrawGizmosSelected()
        {

            var light = gameObject.GetComponentInChildren<Light>(true);
            if (light)
            {
                Gizmos.color = new Color(1.0f, 1.0f, 0.0f, 0.3f);
                Gizmos.DrawSphere(light.transform.position, -light.range);
            }
        }

        public void SetOn(bool on)
        {
            if (invert)
            {
                on = !on;
            }

            if (lightContainer == null)
            {
                Debug.LogError("[Lamp] Has no container! " + name);
                if (transform.parent != null)
                {
                    Debug.LogError("...from " + transform.parent.name);
                }

                return;
            }

            lightContainer.SetActive(on);

            foreach (var mr in targetMeshRenderers)
            {
                if(on)
                    mr.material.EnableKeyword("_EMISSION");
                else
                    mr.material.DisableKeyword("_EMISSION");
            }
        }
    }
}