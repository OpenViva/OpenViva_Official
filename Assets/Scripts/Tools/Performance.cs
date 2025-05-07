using System;
using System.Diagnostics;

namespace Viva.Util
{

    public static class Performance
    {
        public enum FrameRateCategory
        {
            Unplayable,
            VeryBad,
            Bad,
            Average,
            Good,
            VeryGood,
            Count,
        }
        public static Func<int> GetMemoryUsage = null;
        public static Func<int> GetGarbageCollections = null;
        private static Stopwatch Stopwatch = Stopwatch.StartNew();
        private static int frames;
        public static int TargetFrameRate = 60;
        private static int[] frameBuckets = new int[6];
        private static float[] frameBucketFractions = new float[6];

        public static FrameRateCategory FramerateCategory => CategorizeFrameRate(FrameCountLastSecond);

        public static int FrameCountLastSecond { get; private set; }

        public static double AvgFrameTimeLastSecond => 1000 / FrameCountLastSecond;

        public static int MemoryUsage { get; private set; }

        public static int GarbageCollections { get; private set; }

        public static float SecondsSinceLastConnection { get; private set; }

        public static int[] CategorizedFrameCount => frameBuckets;

        public static void Frame()
        {
            ++frames;
            if (Stopwatch.Elapsed.TotalSeconds < 1.0)
                return;
            OneSecond(Stopwatch.Elapsed.TotalSeconds);
            Stopwatch.Reset();
            Stopwatch.Start();
        }

        private static void OneSecond(double timelapse)
        {
            FrameCountLastSecond = frames;
            frames = 0;
            MemoryUsage = GetMemoryUsage == null ? (int)(GC.GetTotalMemory(false) / 1024L / 1024L) : GetMemoryUsage();
            GarbageCollections = GetGarbageCollections == null ? GC.CollectionCount(0) : GetGarbageCollections();
            UpdateFrameBuckets();
        }

        public static FrameRateCategory CategorizeFrameRate(int i)
        {
            if (i < TargetFrameRate / 4)
                return FrameRateCategory.Unplayable;
            if (i < TargetFrameRate / 2)
                return FrameRateCategory.VeryBad;
            if (i < TargetFrameRate - 10)
                return FrameRateCategory.Bad;
            if (i < TargetFrameRate + 10)
                return FrameRateCategory.Average;
            return i < TargetFrameRate + 30 ? FrameRateCategory.Good : FrameRateCategory.VeryGood;
        }

        private static void UpdateFrameBuckets()
        {
            ++frameBuckets[(int)FramerateCategory];
            int frameR = 0;
            for (int i = 0; i < frameBuckets.Length; ++i)
                frameR += frameBuckets[i];
            for (int i = 0; i < frameBuckets.Length; ++i)
                frameBucketFractions[i] = frameBuckets[i] / frameR;
        }

        public static int GetFrameCount(FrameRateCategory category) => frameBuckets[(int)category];

        public static float GetFrameFraction(FrameRateCategory category) => frameBucketFractions[(int)category];
    }
}