using System;
using UnityEngine;

namespace Viva.CharacterFramework
{
    public class CharacterEnums
    {
        public enum NeedType
        {
            Hunger = 0,
            Energy = 1,
            Social = 2,
            Comfort = 3,
        }

        public static class NeedTypeInfo
        {
            public const int Count = 4;
        }

        // Core stats
        public enum CharacterStat
        {
            StaminaRecovery = 0, // TBA stat, needs testing
            TaskSpeed = 1,
            MoveSpeed = 2,
        }

        public static class CharacterStatInfo
        {
            public const int Count = 3;
        }

        [Flags]
        public enum ConditionMask
        {
            None = 0,
            Working = 1 << 0,
            Resting = 1 << 1,
            Sleeping = 1 << 2,
            Daytime = 1 << 3,
            Nighttime = 1 << 4,
        }

        public enum InteractionType
        {
            None = 0,
            Poke,
            Headpat,
            Hug,
            HoldingHands, // Refuse this for now
            Follow,
            GoToLocation,
            Gift,
            CallToPlayer,
            Stop,
            Work,
        }

        public static class InteractionTypeInfo
        {
            public static readonly int Count = Enum.GetValues(typeof(InteractionType)).Length;
        }

        // Physical is self explanatory, Command is via key or menu
        public enum InteractionChannel
        {
            Physical,
            Command,
        }

        public enum InteractionPhase
        {
            Instant,
            Begin,
            Sustain,
            End,
        }

        public enum InteractionSource
        {
            DesktopMouse,
            DesktopWheel,
            VRGesture,
            VRWheel,
            UI,
            Script,
        }

        public enum InteractionResult
        {
            /// <summary>
            /// Invalid target, impossible task or something else
            /// </summary>
            Rejected,
            /// <summary>
            /// Valid interaction but no effect
            /// </summary>
            Ignored,
            /// <summary>
            /// Accepted and reacted
            /// </summary>
            Accepted,
            /// <summary>
            /// Understood but refused
            /// </summary>
            Refused,
        }

        public enum HandSide
        {
            None,
            Left,
            Right,
            Both
        }

        public enum AffinityTier
        {
            Hostile,
            Neutral,
            Friendly,
            Trusted,
        }

        // Cycle through these when in negative affinity
        public enum RebelliousAction
        {
            CrossArms,
            WalkAway,
            IgnorePlayer
        }

        public enum BodyRegion
        {
            Head = 0,
            Chest = 1,
            Arms = 2,
            Other = 3,
        }

        [Flags]
        public enum BodyRegionMask
        {
            None = 0,
            Head = 1 << 0,
            Chest = 1 << 1,
            Arms = 1 << 2,
            Other = 1 << 3,
            All = Head | Chest | Arms | Other
        }

        [Flags]
        public enum PlatformMask
        {
            None = 0,
            Desktop = 1 << 0,
            VR = 1 << 1,
            All = Desktop | VR
        }

        public static class BodyRegionUtil
        {
            public static BodyRegion FromBone(HumanBodyBones bone)
            {
                switch (bone)
                {
                    case HumanBodyBones.Head:
                    case HumanBodyBones.Neck:
                    case HumanBodyBones.Jaw:
                    case HumanBodyBones.LeftEye:
                    case HumanBodyBones.RightEye:
                        return BodyRegion.Head;

                    case HumanBodyBones.Spine:
                    case HumanBodyBones.Chest:
                    case HumanBodyBones.UpperChest:
                        return BodyRegion.Chest;

                    case HumanBodyBones.LeftShoulder:
                    case HumanBodyBones.RightShoulder:
                    case HumanBodyBones.LeftUpperArm:
                    case HumanBodyBones.RightUpperArm:
                    case HumanBodyBones.LeftLowerArm:
                    case HumanBodyBones.RightLowerArm:
                    case HumanBodyBones.LeftHand:
                    case HumanBodyBones.RightHand:
                        return BodyRegion.Arms;

                    default:
                        // Finger bones form one contiguous block in the enum
                        if (bone >= HumanBodyBones.LeftThumbProximal && bone <= HumanBodyBones.RightLittleDistal)
                            return BodyRegion.Arms;
                        return BodyRegion.Other;
                }
            }
        }

        public static BodyRegionMask ToMask(BodyRegion region)
        {
            return (BodyRegionMask)(1 << (int)region);
        }

        public static bool Contains(BodyRegionMask mask, BodyRegion region)
        {
            return (mask & ToMask(region)) != 0;
        }
    }
}