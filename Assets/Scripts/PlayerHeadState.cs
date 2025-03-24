using UnityEngine;
using UnityEngine.SpatialTracking;


namespace Viva
{

    public class PlayerHeadState : HeadState
    {
        [SerializeField]
        private TrackedPoseDriver m_behaviourPose;
        public TrackedPoseDriver behaviourPose { get { return m_behaviourPose; } }
    }

}
