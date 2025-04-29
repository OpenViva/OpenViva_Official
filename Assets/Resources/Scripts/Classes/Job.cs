using Viva.Util;

namespace Viva
{


    public abstract class Job
    {

        public enum JobType
        {
            LOCOMOTION,
            ACTIVE,
            PASSIVE,
            AUTONOMY
        }

        public Companion self { get; private set; }
        protected JobType jobType;

        public Job(Companion _self, JobType _jobType)
        {
            self = _self;
            jobType = _jobType;
        }

        public abstract class Task
        {

            public readonly Companion self;
            public readonly JobType jobType;
            private Set<Task> permissionBanSources = new Set<Task>();
            public bool permissionAllowed { get { return permissionBanSources.Count == 0; } }

            public Task(Companion _self, JobType _jobType)
            {
                self = _self;
                jobType = _jobType;
            }

            protected void BanTaskPermission(Task target)
            {
                target.permissionBanSources.Add(this);
            }
            protected void AllowTaskPermission(Task target)
            {
                target.permissionBanSources.Remove(target);
            }

            public virtual void OnAnimationChange(Companion.Animation oldAnim, Companion.Animation newAnim) { }
            public virtual void OnFixedUpdate() { }
            public virtual void OnUpdate() { }
            public virtual void OnLateUpdate() { }
            public virtual void OnLateUpdatePreLookAt() { }
            public virtual void OnLateUpdatePostIK() { }
            public virtual bool OnGesture(Item source, Gesture gesture)
            {
                return false;
            }
        }

        public virtual void OnFixedUpdate() { }
        public virtual void OnUpdate() { }
        public virtual void OnLateUpdate() { }
        public virtual void OnLateUpdatePostIK() { }
        public virtual void OnAnimationChange(Companion.Animation oldAnim, Companion.Animation newAnim) { }
    }

}