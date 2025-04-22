using System.Collections.Generic;
using UnityEngine;
using Viva.Util;

namespace Viva
{

    using ServiceUserEntry = Tuple<Service.ServiceUser, Service.ServiceUser.SetupCallback>;

    public abstract class Service : Mechanism
    {


        [System.Serializable]
        public class EmployeeInfo
        {
            public Vector3 localPos = Vector3.zero;
            public Vector3 localRootFacePos = Vector3.forward;
        }


        public class ServiceUser
        {

            public delegate void SetupCallback(Service service, EmployeeInfo info);

            public readonly Companion companion;
            public EmployeeInfo info { get; private set; }
            public Service service { get; private set; }

            public ServiceUser(Companion companion, ref SetupCallback accessor)
            {
                this.companion = companion;
                accessor = Setup;
            }

            private void Setup(Service _service, EmployeeInfo _info)
            {
                service = _service;
                info = _info;
            }
        }

        [SerializeField]
        private Texture2D employeeNametag;
        [SerializeField]
        private float employeeNametagYOffset;
        [SerializeField]
        private ActiveBehaviors.Behavior m_targetBehavior;
        public ActiveBehaviors.Behavior targetBehavior { get { return m_targetBehavior; } private set { m_targetBehavior = value; } }
        [SerializeField]
        private List<EmployeeInfo> employeeInfos = new List<EmployeeInfo>();
        public int employeeInfosAvailable { get { return employeeInfos.Count; } }

        private List<ServiceUser> activeServiceUsers = new List<ServiceUser>();
        // private MethodInfo onServiceMethod;

        private static List<ServiceUserEntry> serviceUserEntries = new List<ServiceUserEntry>();

        public override void OnMechanismAwake()
        {
        }

        public override sealed bool AttemptCommandUse(Companion targetCompanion, Character commandSource)
        {
            if (targetCompanion == null)
            {
                return false;
            }
            if (!targetCompanion.IsHappy() || targetCompanion.IsTired())
            {   //must be happy and not tired
                targetCompanion.active.idle.PlayAvailableRefuseAnimation();
                return false;
            }
            bool success = Employ(targetCompanion);
            if (success)
            {
                targetCompanion.active.SetTask(targetCompanion.active.GetTask(targetBehavior));
                GameDirector.player.objectFingerPointer.selectedCompanions.Remove(targetCompanion);
                targetCompanion.OnUnselected();
            }
            else
            {
                var playAnim = CompanionUtility.CreateSpeechAnimation(targetCompanion, AnimationSet.REFUSE, SpeechBubble.FULL);
                targetCompanion.autonomy.Interrupt(playAnim);
            }
            return success;
        }

        protected abstract void OnInitializeEmployment(Companion targetCompanion);


        private void OnTaskChange(Companion companion, ActiveBehaviors.Behavior newBehavior)
        {
            if (newBehavior != targetBehavior)
            {
                Unemploy(companion);
            }
        }

        private static ServiceUserEntry EnsureServiceUser(Companion companion)
        {
            if (companion == null)
            {
                return null;
            }
            for (int i = 0; i < serviceUserEntries.Count; i++)
            {
                if (serviceUserEntries[i]._1.companion == companion)
                {
                    return serviceUserEntries[i];
                }
            }

            ServiceUser.SetupCallback accessor = null;
            var serviceUser = new ServiceUser(companion, ref accessor);
            var entry = new ServiceUserEntry(serviceUser, accessor);
            serviceUserEntries.Add(entry);
            return entry;
        }

        public static int GetServiceIndex(Companion companion)
        {
            if (companion == null)
            {
                return -1;
            }
            var entry = EnsureServiceUser(companion);
            if (entry != null)
            {
                return GameDirector.instance.town.services.IndexOf(entry._1.service);
            }
            return -1;
        }

        public ServiceUser GetActiveServiceUser(int index)
        {
            if (index < 0 || index >= activeServiceUsers.Count)
            {
                return null;
            }
            return activeServiceUsers[index];
        }

        public override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.green;
            foreach (var employeeInfo in employeeInfos)
            {
                Vector3 worldPos = employeeInfo.localPos + Vector3.up * 0.02f;
                Gizmos.DrawLine(worldPos, worldPos - Quaternion.Euler(0.0f, +15, 0.0f) * employeeInfo.localRootFacePos * 0.1f);
                Gizmos.DrawLine(worldPos, worldPos - employeeInfo.localRootFacePos * 0.3f);
                Gizmos.DrawLine(worldPos, worldPos - Quaternion.Euler(0.0f, -15, 0.0f) * employeeInfo.localRootFacePos * 0.1f);
            }
        }

        private EmployeeInfo GetNextEmptyEmployeeInfo()
        {
            if (employeeInfos.Count > 0)
            {
                return employeeInfos[0];
            }
            return null;
        }

        public EmployeeInfo GetEmployeeInfo(int index)
        {
            return employeeInfos[index];
        }

        public EmployeeInfo FindActiveEmployeeInfo(Companion companion)
        {
            if (companion == null)
            {
                return null;
            }
            foreach (var serviceUser in activeServiceUsers)
            {
                if (serviceUser.companion == companion)
                {
                    return serviceUser.info;
                }
            }
            return null;
        }

        public bool Employ(Companion companion)
        {
            if (companion == null)
            {
                Debug.LogError("[Service] Cannot employ null companion");
                return false;
            }
            var entry = EnsureServiceUser(companion);
            if (entry == null)
            {
                Debug.LogError("[Service] Could not ensure service user");
                return false;
            }
            if (entry._1.service != null)
            {
                if (entry._1.service == this)
                {
                    return true;
                }
                else
                {
                    Debug.LogError("[Service] Companion already employed in a service");
                    return false;
                }
            }
            EmployeeInfo targetInfo = GetNextEmptyEmployeeInfo();
            if (targetInfo == null)
            {
                return false;
            }

            //assign new employee info
            entry._2.Invoke(this, targetInfo);
            employeeInfos.Remove(targetInfo);
            activeServiceUsers.Add(entry._1);
            companion.onTaskChange += OnTaskChange;

            if (activeServiceUsers.Count > 0)
            {
                GameDirector.mechanisms.Add(this);
            }
            companion.SetNameTagTexture(employeeNametag, employeeNametagYOffset);
            OnInitializeEmployment(companion);
            return true;
        }

        private void Unemploy(Companion companion)
        {
            var entry = EnsureServiceUser(companion);
            if (entry == null)
            {
                Debug.LogError("[Service] Could not ensure service user");
                return;
            }
            if (entry._1.service != this)
            {
                Debug.LogError("[Service] Companion not employed to service " + this);
                return;
            }
            //restore employee info
            employeeInfos.Add(entry._1.info);
            entry._2.Invoke(null, null);
            activeServiceUsers.Remove(entry._1);
            companion.onTaskChange -= OnTaskChange;

            companion.SetNameTagTexture(null, 0);

            if (activeServiceUsers.Count == 0)
            {
                GameDirector.mechanisms.Remove(this);
            }
        }

        public AutonomyMoveTo CreateGoToEmploymentPosition(Companion companion)
        {

            if (companion == null)
            {
                return null;
            }
            var employeeInfo = FindActiveEmployeeInfo(companion);
            if (employeeInfo == null)
            {
                Debug.LogError("[OnsenClerk] employee info is null");
                return null;
            }

            var receptionPos = transform.TransformPoint(employeeInfo.localPos);
            var walkToReception = new AutonomyMoveTo(
                companion.autonomy,
                "walk to reception",
                delegate (TaskTarget target) { target.SetTargetPosition(receptionPos); },
                0.1f,
                BodyState.STAND,
                delegate (TaskTarget target)
                {
                    target.SetTargetPosition(receptionPos + transform.TransformDirection(employeeInfo.localRootFacePos));
                }
            );

            return walkToReception;
        }
    }

}