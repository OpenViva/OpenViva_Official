using System.Collections.Generic;
using UnityEngine;

namespace Viva
{

    public class KitchenFacilities : Mechanism
    {


        [SerializeField]
        public Transform approachLocation;

        private static List<KitchenFacilities> facilities = new List<KitchenFacilities>();

        public static KitchenFacilities FindNearestFacility(Vector3 pos)
        {
            float least = Mathf.Infinity;
            KitchenFacilities nearest = null;
            foreach (KitchenFacilities facility in facilities)
            {
                float sqDst = Vector3.SqrMagnitude(pos - facility.approachLocation.position);
                if (sqDst < least)
                {
                    least = sqDst;
                    nearest = facility;
                }
            }
            return nearest;
        }

        public override void OnMechanismAwake()
        {
            facilities.Add(this);
        }

        public override bool AttemptCommandUse(Companion targetCompanion, Character commandSource)
        {
            Debug.Log("Start Cookin");
            if (targetCompanion == null)
            {
                return false;
            }
            
            return targetCompanion.active.cooking.AttemptBeginCooking(this);
        }

        public override void EndUse(Character targetCharacter)
        {
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.TransformPoint(approachLocation.position), 0.2f);
        }
    }

}