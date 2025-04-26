using UnityEngine;


namespace Viva
{

    public class FootstepTypeVolume : MonoBehaviour
    {

        [SerializeField]
        private FootstepInfo.FootStepType footStepType;


        public void OnTriggerEnter(Collider collider)
        {
            FootstepInfo footstepInfo = collider.GetComponent<FootstepInfo>();
            if (footstepInfo == null)
            {
                return;
            }
            footstepInfo.AddtoFootstepRegion(footStepType);
        }

        public void OnTriggerExit(Collider collider)
        {
            FootstepInfo footstepInfo = collider.GetComponent<FootstepInfo>();
            if (footstepInfo == null)
            {
                return;
            }
            footstepInfo.RemoveFromFootstepRegion(footStepType);
        }
    }

}