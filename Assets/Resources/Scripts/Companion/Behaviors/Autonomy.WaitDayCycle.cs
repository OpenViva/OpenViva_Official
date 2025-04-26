namespace Viva
{
    public partial class AutonomyWaitDayCycle : Autonomy.Task
    {
        private float duration;
        private float dayCycleStarted;
        public bool loop = false;
        private float? minTime;
        private float? maxTime;

        public AutonomyWaitDayCycle(Autonomy _autonomy, string _name, float _duration, float? _minTime = null, float? _maxTime = null) : base(_autonomy, _name)
        {
            duration = _duration;
            minTime = _minTime;
            maxTime = _maxTime;

            onRegistered += delegate { dayCycleStarted = GameDirector.newSkyDirector.skyDefinition.SystemTime; };
        }

        public override bool? Progress()
        {
            float currentTime = GameDirector.newSkyDirector.skyDefinition.SystemTime;
            float timeElapsed = currentTime - dayCycleStarted;

            if (timeElapsed > duration)
            {
                bool withinTimeRange = true;

                if (minTime.HasValue && maxTime.HasValue)
                {
                    float currentHour = GameDirector.newSkyDirector.skyDefinition.CurrentTime;
                    withinTimeRange = currentHour >= minTime.Value && currentHour <= maxTime.Value;
                }

                if (withinTimeRange)
                {
                    if (loop)
                    {
                        dayCycleStarted = currentTime;
                        Reset();
                    }
                    return true;
                }
            }
            return null;
        }
    }
}