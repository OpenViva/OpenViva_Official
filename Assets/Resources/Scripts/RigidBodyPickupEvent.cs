using UnityEngine;
using UnityEngine.Events;

namespace Viva
{


    public class RigidBodyPickupEvent : MonoBehaviour
    {
        public UnityEvent onPickup;
        public UnityEvent onDrop;
    }

}