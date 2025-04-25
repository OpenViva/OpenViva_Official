using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace OccaSoftware.Altos.Runtime
{
    internal static class TimeManager
    {
        private static float managedTime = 0;
        private static int frameCount = 0;

        public static float ManagedTime
        {
            get => managedTime;
        }

        public static int FrameCount
        {
            get => frameCount;
        }

        public static void Update()
        {
            float unityRealtimeSinceStartup = Time.realtimeSinceStartup;
            int unityFrameCount = Time.frameCount;

            bool newFrame;
            if (Application.isPlaying)
            {
                newFrame = frameCount != unityFrameCount;
                frameCount = unityFrameCount;
            }
            else
            {
                newFrame = (unityRealtimeSinceStartup - managedTime) > 0.0166f;
                if (newFrame)
                    frameCount++;
            }

            if (newFrame)
            {
                managedTime = unityRealtimeSinceStartup;
            }
        }
    }
}
