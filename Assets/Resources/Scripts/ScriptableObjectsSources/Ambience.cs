using OccaSoftware.Altos.Runtime;
using System;
using UnityEngine;

namespace Viva
{


    [System.Serializable]
    [CreateAssetMenu(fileName = "Ambience", menuName = "Logic/Ambience", order = 1)]
    public class Ambience : ScriptableObject
    {
        public AudioClip morningIndoor;
        public AudioClip morningOutdoor;
        public AudioClip eveningIndoor;
        public AudioClip eveningOutdoor;
        public AudioClip nightIndoor;
        public AudioClip nightOutdoor;

        public SoundSet randomSounds;

        public int priority;

        public enum DaySegment
        {
            MORNING,
            DAY,
            NIGHT,
            NONE,
        }

        public AudioClip GetAudio(DaySegment current, bool indoor)
        {
            if (indoor)
            {
                switch (current)
                {
                    case DaySegment.MORNING:
                        return morningIndoor;
                    case DaySegment.DAY:
                        return eveningIndoor;
                    case DaySegment.NIGHT:
                        return nightIndoor;
                }
            }
            else
            {
                switch (current)
                {
                    case DaySegment.MORNING:
                        return morningOutdoor;
                    case DaySegment.DAY:
                        return eveningOutdoor;
                    case DaySegment.NIGHT:
                        return nightOutdoor;
                }
            }
            return null;
        }

        internal AudioClip GetAudio(Func<SkyDirector.DaySegment> getDaySegment, bool userIsIndoors)
        {
            throw new NotImplementedException();
        }
    }

}