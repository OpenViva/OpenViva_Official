using UnityEngine;
using Viva.Util;


namespace Viva
{


    public class ReceptionBell : MonoBehaviour
    {

        [SerializeField]
        private ParticleSystem bellFX;
        [SerializeField]
        private AudioClip bellSound;
        [SerializeField]
        private OnsenReception targetReception;

        public FilterUse filterUse { get; private set; } = new FilterUse();


        private void OnCollisionEnter(Collision collision)
        {

            if (collision.rigidbody && collision.relativeVelocity.magnitude > 0.5f)
            {

                var handle = SoundManager.main.RequestHandle(transform.position);
                handle.volume = Mathf.Clamp01(collision.relativeVelocity.magnitude - 0.5f);
                handle.PlayOneShot(bellSound);

                bellFX.Emit(1);

                var source = Tools.SearchTransformAncestors<Player>(collision.transform);
                if (source == null)
                {
                    var source2 = Tools.SearchTransformAncestors<Item>(collision.transform);
                    if (source2 == null) return;
                    targetReception.CreateClerkSession(source2.mainOwner, null);
                }
                else
                {
                    targetReception.CreateClerkSession(source, null);
                }
                
            }
        }
    }

}