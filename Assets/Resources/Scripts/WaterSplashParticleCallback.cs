using UnityEngine;


namespace Viva
{

    public class WaterSplashParticleCallback : MonoBehaviour
    {
        private void OnParticleCollision(GameObject gameObject)
        {
            OnWaterCollision(gameObject, transform.position);
        }

        public static void OnWaterCollision(GameObject gameObject, Vector3 sourcePos)
        {
            CharacterCollisionCallback ccc = gameObject.GetComponent<CharacterCollisionCallback>();
            if (ccc)
            {
                ccc.owner.OnCharacterSplashed(sourcePos);
            }
        }
    }

}