using UnityEngine;

namespace Viva
{


    public abstract class ClothingScript : MonoBehaviour
    {

        public abstract void OnBeginWearing(Companion companion);
        public abstract void OnApplywearing();
    }

}