using System;
using System.Collections.Generic;
using UnityEngine;


namespace Viva
{

    using CompanionAnimationEvent = AnimationEvent<float[]>;

    public partial class Companion : Character
    {

        public class LocomotionInfo
        {

            public float maxSpeed;
            public float acceleration;
            public float faceYawMaxVel;
            public float faceYawAcc;

            public LocomotionInfo(float _maxSpeed, float _acceleration, float _faceYawMaxVel, float _faceYawAcc)
            {
                maxSpeed = _maxSpeed;
                acceleration = _acceleration;
                faceYawMaxVel = _faceYawMaxVel;
                faceYawAcc = _faceYawAcc;
            }
        }

        private static void RegisterLegSpeedInfo(string legStateName, float speed, float acc, float faceYawMaxVel, float faceYawAcc)
        {

            int stateHash = Animator.StringToHash(legStateName);
            // if( !animator.HasState(0,stateHash) ){
            // 	Debug.LogError("###ERROR### LegSpeedInfo does not exist! "+legStateName);
            // 	return;
            // }
            legSpeedInfos.Add(stateHash, new LocomotionInfo(speed * 1.4f, acc, faceYawMaxVel, faceYawAcc));
        }

        public static LocomotionInfo GetLegSpeedInfo(Animation anim)
        {

            LocomotionInfo info = null;
            if (legSpeedInfos.TryGetValue(animationInfos[anim].legsStateID, out info))
            {
                return info;
            }
            return null;
        }

        public static Dictionary<Animation, AnimationInfo> animationInfos;
        public static Dictionary<int, LocomotionInfo> legSpeedInfos = new Dictionary<int, LocomotionInfo>();
        public static List<int> holdAnimationStates = new List<int>();


        private static void RegisterAnimation(Animation animation, AnimationInfo info, bool createMirroredLeft = false)
        {

            animationInfos.Add(animation, info);

            if (createMirroredLeft)
            {
                TransitionHandle transitionHandle;
                if (info.transitionHandle != null)
                {
                    switch (info.transitionHandle.type)
                    {
                        case TransitionHandle.TransitionType.DEFAULT:
                            transitionHandle = new DefaultTransition(GetMirroredLeftEnum(((DefaultTransition)info.transitionHandle).nextAnim));
                            break;
                        default:
                            transitionHandle = info.transitionHandle;
                            break;
                    }
                }
                else
                {
                    transitionHandle = null;
                }

                AnimationInfo.Flag mirroredFlags = info.flag;

                animationInfos.Add(GetMirroredLeftEnum(animation),
                    new AnimationInfo(transitionHandle,
                        info.torsoStateName.Replace("_right", "_left"),
                        info.legsStateName.Replace("_right", "_left"),
                        info.faceStateName.Replace("_right", "_left"),
                        info.priority,
                        GetMirroredBodyState(info.conditionBodyState),
                        GetMirroredBodyState(info.newBodyState),
                        info.transitionTime, info.animLogicInfo,
                        GetMirroredAnimationEvents(info.animationEvents),
                        GetMirroredLeftEnum(mirroredFlags)
                    )
                );
            }
        }

        //generic getter
        private static T GetMirroredLeftEnum<T>(T enumValue) where T : Enum
        {
            T nextEnum = (T)(object)((int)(object)enumValue + 1);
            if (!nextEnum.ToString().EndsWith("_LEFT"))
            {
                //Debug.LogError("BAD ORDER. NEXT ENUM NOT LEFT: " + enumValue);
                return enumValue;
            }
            return nextEnum;
        }
        
        private static BodyState GetMirroredBodyState(BodyState bodyState)
        {
            int mirroredBodyState;
            if (((BodyState)bodyState).ToString().Contains("_RIGHT"))
            {
                mirroredBodyState = (int)bodyState + 1;
            }
            else if (((BodyState)bodyState).ToString().Contains("_LEFT"))
            {
                mirroredBodyState = (int)bodyState - 1;
            }
            else
            {
                mirroredBodyState = (int)bodyState;
            }
            return (BodyState)mirroredBodyState;
        }

        private static CompanionAnimationEvent[] GetMirroredAnimationEvents(CompanionAnimationEvent[] events)
        {
            if (events == null)
            {
                return null;
            }
            CompanionAnimationEvent[] mirroredEvents = new CompanionAnimationEvent[events.Length];
            for (int i = 0; i < mirroredEvents.Length; i++)
            {

                CompanionAnimationEvent animEvent = events[i];
                int mirroredName;
                if (((AnimationEventName)animEvent.nameID).ToString().Contains("_RIGHT"))
                {
                    mirroredName = (int)animEvent.nameID + 1;
                }
                else if (((AnimationEventName)animEvent.nameID).ToString().Contains("_LEFT"))
                {
                    mirroredName = (int)animEvent.nameID - 1;
                }
                else
                {
                    mirroredName = animEvent.nameID;
                }
                mirroredEvents[i] = new CompanionAnimationEvent(
                    animEvent.fireTimeNormalized,
                    mirroredName,
                    animEvent.parameter
                );
            }

            return mirroredEvents;
        }

        public static void GenerateAnimations()
        {

            if (animationInfos == null)
            {
                animationInfos = new Dictionary<Animation, AnimationInfo>();
            }

            for (int i = 0; i < System.Enum.GetValues(typeof(Companion.HoldFormAnimation)).Length; i++)
            {
                holdAnimationStates.Add(Animator.StringToHash("LOLI_HOLD_FORM_" + ((Companion.HoldFormAnimation)i)));
            }

            Companion.TransitionToIdle idleTransition = new Companion.TransitionToIdle();

            RegisterAnimation(Companion.Animation.NONE, new AnimationInfo(
                idleTransition,
                "APOSE", "APOSE", "APOSE",
                Priority.NONE, BodyState.STAND, BodyState.STAND, 0.01f, new AnimLogicInfo(), null
            ));

            //Add entire animation list
            RegisterAnimation(Companion.Animation.BATHTUB_TEST_WATER_IN_RIGHT,
                new Companion.AnimationInfo(new BathingBehavior.TransitionToBathtubAnim(),
                "bathtub_test_water_in_right", "bathtub_test_water_in_right", "bathtub_test_water_in_right",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(1, 1.0f, 0.5f, 0, 0.5f),
                null
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_TEST_WATER_IN_LEFT,
                new Companion.AnimationInfo(new BathingBehavior.TransitionToBathtubAnim(),
                "bathtub_test_water_in_left", "bathtub_test_water_in_left", "bathtub_test_water_in_left",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(1, 1.0f, 0.5f, 0, 0.5f),
                null
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_TEST_WATER_IN_COLD_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "bathtub_test_water_in_cold_right", "bathtub_test_water_in_cold_right", "bathtub_test_water_in_cold_right",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.5f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.STARTLE_SHORT, 0.0f } )
                }
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_TEST_WATER_IN_COLD_LEFT,
                new Companion.AnimationInfo(idleTransition,
                "bathtub_test_water_in_cold_left", "bathtub_test_water_in_cold_left", "bathtub_test_water_in_cold_left",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.5f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.STARTLE_SHORT, 0.0f } )
                }
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_TEST_WATER_LUKEWARM_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "bathtub_test_water_lukewarm_right", "bathtub_test_water_lukewarm_right", "bathtub_test_water_lukewarm_right",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.5f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } )
                }
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_TEST_WATER_LUKEWARM_LEFT,
                new Companion.AnimationInfo(idleTransition,
                "bathtub_test_water_lukewarm_left", "bathtub_test_water_lukewarm_left", "bathtub_test_water_lukewarm_left",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.5f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } )
                }
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_TEST_WATER_IN_HOT_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "bathtub_test_water_in_hot_right", "bathtub_test_water_in_hot_right", "bathtub_test_water_in_hot_right",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.2f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.SCREAMING, 0.0f } )
                }
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_TEST_WATER_IN_HOT_LEFT,
                new Companion.AnimationInfo(idleTransition,
                "bathtub_test_water_in_hot_left", "bathtub_test_water_in_hot_left", "bathtub_test_water_in_hot_left",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.2f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.SCREAMING, 0.0f } )
                }
            ));
            RegisterAnimation(Companion.Animation.STAND_POINT_OUT_IN_RIGHT,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_POINT_OUT_LOOP_RIGHT),
                "stand_point_out_in_right", "stand_point_out_in_right", "stand_point_out_in_right",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(1, 1.0f, 0.4f, 7, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.HUMPH, 0.0f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.STAND_POINT_OUT_LOOP_RIGHT,
                new Companion.AnimationInfo(null,
                "stand_point_out_loop_right", "stand_point_out_loop_right", "stand_point_out_loop_right",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(1, 1.0f, 0.4f, 7, 0.3f),
                null,
                AnimationInfo.Flag.IDLE_STATE
            ), true);

            RegisterAnimation(Companion.Animation.STAND_POINT_OUT_SOUND_2_3_RIGHT,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_POINT_OUT_LOOP_RIGHT),
                "stand_point_out_sound_2_3_right", "stand_point_out_sound_2_3_right", "stand_point_out_sound_2_3_right",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(1, 1.0f, 0.4f, 12, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.GET_OUT_2_3, 1.0f } ),
                }
            ), true);
            RegisterAnimation(Companion.Animation.STAND_POINT_OUT_TO_WAIT_RIGHT,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_WAIT_ANNOYED_LOOP),
                "stand_point_out_to_wait_right", "stand_point_out_to_wait_right", "stand_point_out_to_wait_right",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(1, 1.0f, 0.4f, 7, 0.3f),
                null
            ), true);
            RegisterAnimation(Companion.Animation.STAND_WAIT_ANNOYED_LOOP,
                new Companion.AnimationInfo(null,
                "stand_wait_annoyed_loop", "stand_wait_annoyed_loop", "stand_wait_annoyed_loop",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(3, 1.0f, 0.5f, 7, 0.3f),
                null,
                AnimationInfo.Flag.IDLE_STATE
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_RELAX_LOOP,
                new Companion.AnimationInfo(null,
                "bathtub_relax_loop", "bathtub_relax_loop", "bathtub_relax_loop",
                Companion.Priority.LOW, BodyState.BATHING_RELAX, BodyState.BATHING_RELAX,
                0.1f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.3f),
                null,
                AnimationInfo.Flag.IDLE_STATE
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_RELAX_TO_HAPPY_IDLE,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.BATHTUB_HAPPY_IDLE_LOOP),
                "bathtub_relax_to_happy_idle", "bathtub_relax_to_happy_idle", "bathtub_relax_to_happy_idle",
                Companion.Priority.HIGH, BodyState.BATHING_RELAX, BodyState.BATHING_IDLE,
                0.1f, new Companion.AnimLogicInfo(0, 1.0f, 1.0f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_MOVEMENT } ),
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_KERPLUNK } ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f,0.5f } ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f,1.0f, 0.5f } ),
                }
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_RELAX_TO_ANGRY_IDLE,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.BATHTUB_ANGRY_IDLE_LOOP),
                "bathtub_relax_to_angry_idle", "bathtub_relax_to_angry_idle", "bathtub_relax_to_angry_idle",
                Companion.Priority.HIGH, BodyState.BATHING_RELAX, BodyState.BATHING_IDLE,
                0.1f, new Companion.AnimLogicInfo(0, 1.0f, 0.6f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_SHORT, 1.0f } ),
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_MOVEMENT } ),
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_KERPLUNK } ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f,0.5f } ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f,1.0f, 0.5f } ),
                }
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_HAPPY_IDLE_LOOP,
                new Companion.AnimationInfo(null,
                "bathtub_happy_idle_loop", "bathtub_happy_idle_loop", "bathtub_happy_idle_loop",
                Companion.Priority.LOW, BodyState.BATHING_IDLE, BodyState.BATHING_IDLE,
                0.3f, new Companion.AnimLogicInfo(3, 1.0f, 0.5f, 7, 0.5f),
                null,
                AnimationInfo.Flag.IDLE_STATE
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_HAPPY_IDLE2,
                new Companion.AnimationInfo(idleTransition,
                "bathtub_happy_idle2", "bathtub_happy_idle2", "bathtub_happy_idle2",
                Companion.Priority.LOW, BodyState.BATHING_IDLE, BodyState.BATHING_IDLE,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.6f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.41f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f,0.25f } ),
                new CompanionAnimationEvent( 0.55f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f,0.25f } ),
                new CompanionAnimationEvent( 0.9f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f,1.0f, 0.5f } ),
                new CompanionAnimationEvent( 0.9f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f,0.5f } ),
                }
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_HAPPY_SWITCH_SIDES,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.BATHTUB_HAPPY_IDLE_TO_ON_KNEES),
                "bathtub_happy_switch_sides", "bathtub_happy_switch_sides", "bathtub_happy_switch_sides",
                Companion.Priority.HIGH, BodyState.BATHING_IDLE, BodyState.NONE,
                0.35f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_MOVEMENT } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f,0.5f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f,1.0f, 0.5f } ),
                }
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_ANGRY_IDLE_LOOP,
                new Companion.AnimationInfo(idleTransition,
                "bathtub_angry_idle_loop", "bathtub_angry_idle_loop", "bathtub_angry_idle_loop",
                Companion.Priority.LOW, BodyState.BATHING_IDLE, BodyState.BATHING_IDLE,
                0.3f, new Companion.AnimLogicInfo(1, 0.6f, 0.7f, 7, 0.5f),
                null,
                AnimationInfo.Flag.IDLE_STATE
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_SINK_ANGRY,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.BATHTUB_ANGRY_IDLE_LOOP),
                "bathtub_sink_angry", "bathtub_sink_angry", "bathtub_sink_angry",
                Companion.Priority.MEDIUM, BodyState.BATHING_IDLE, BodyState.BATHING_IDLE,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.3f, 12, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.06f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_SPLASH } ),
                }
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_HAPPY_IDLE3,
                new Companion.AnimationInfo(idleTransition,
                "bathtub_happy_idle3", "bathtub_happy_idle3", "bathtub_happy_idle3",
                Companion.Priority.LOW, BodyState.BATHING_IDLE, BodyState.BATHING_IDLE,
                0.4f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_MOVEMENT } ),
                new CompanionAnimationEvent( 0.27f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_SPLASH } ),
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 1.0f } ),
                new CompanionAnimationEvent( 0.82f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f,0.5f } ),
                new CompanionAnimationEvent( 0.82f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f,1.0f, 0.5f } ),
                new CompanionAnimationEvent( 0.84f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_SPLASH } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_POINT_OUT_ANTSY_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_point_out_antsy_right", "stand_point_out_antsy_right", "stand_point_out_antsy_right",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(1, 1.0f, 0.8f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.07f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_GRUMBLE_LONG, 1.0f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.BATHTUB_HAPPY_IDLE4,
                new Companion.AnimationInfo(idleTransition,
                "bathtub_happy_idle4", "bathtub_happy_idle4", "bathtub_happy_idle4",
                Companion.Priority.LOW, BodyState.BATHING_IDLE, BodyState.BATHING_IDLE,
                0.4f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_MOVEMENT } ),
                new CompanionAnimationEvent( 0.15f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_KERPLUNK } ),
                new CompanionAnimationEvent( 0.82f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f,0.5f } ),
                new CompanionAnimationEvent( 0.82f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f,1.0f, 0.5f } ),
                new CompanionAnimationEvent( 0.27f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_SPLASH } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.RELIEF, 1.0f } ),
                }
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_WAVE_HAPPY_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "bathtub_wave_happy_right", "bathtub_wave_happy_right", "bathtub_wave_happy_right",
                Companion.Priority.LOW, BodyState.BATHING_IDLE, BodyState.BATHING_IDLE,
                0.3f, new Companion.AnimLogicInfo(3, 0.7f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_MOVEMENT } ),
                new CompanionAnimationEvent( 0.11f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_SPLASH } ),
                new CompanionAnimationEvent( 0.125f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.GIGGLE, 1.0f } ),
                new CompanionAnimationEvent( 0.69f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_SPLASH } ),
                }
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_RELAX_HUMMING,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.BATHTUB_RELAX_LOOP),
                "bathtub_relax_humming", "bathtub_relax_humming", "bathtub_relax_humming",
                Companion.Priority.LOW, BodyState.BATHING_RELAX, BodyState.BATHING_RELAX,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.4f, 0, 0.5f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.HUMMING, 1.0f } ),
                }
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_HAPPY_IDLE_TO_ON_KNEES,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.BATHTUB_ON_KNEES_LOOP),
                "bathtub_happy_idle_to_on_knees", "bathtub_happy_idle_to_on_knees", "bathtub_happy_idle_to_on_knees",
                Companion.Priority.LOW, BodyState.BATHING_IDLE, BodyState.BATHING_ON_KNEES,
                0.4f, new Companion.AnimLogicInfo(0, 1.0f, 0.6f, 0, 0.5f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_SPLASH } ),
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 1.0f } ),
                new CompanionAnimationEvent( 0.6f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 0.9f, 0.5f } ),
                new CompanionAnimationEvent( 0.6f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.5f } ),
                }
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_ON_KNEES_TO_HAPPY_IDLE,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.BATHTUB_HAPPY_IDLE_LOOP),
                "bathtub_on_knees_to_happy_idle", "bathtub_on_knees_to_happy_idle", "bathtub_on_knees_to_happy_idle",
                Companion.Priority.MEDIUM, BodyState.BATHING_ON_KNEES, BodyState.BATHING_IDLE,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.6f, 0, 0.5f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_SPLASH } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 1.0f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f,0.5f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f,0.7f, 0.6f } ),
                }
            ));
            RegisterAnimation(Companion.Animation.BATHTUB_ON_KNEES_LOOP,
                new Companion.AnimationInfo(null,
                "bathtub_on_knees_loop", "bathtub_on_knees_loop", "bathtub_on_knees_loop",
                Companion.Priority.LOW, BodyState.BATHING_ON_KNEES, BodyState.BATHING_ON_KNEES,
                0.3f, new Companion.AnimLogicInfo(1, 0.3f, 0.7f, 0, 0.4f),
                null,
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.BATHTUB_HAPPY_BEG_LOOP,
                new Companion.AnimationInfo(null,
                "bathtub_happy_beg", "bathtub_happy_beg", "bathtub_happy_beg",
                Companion.Priority.LOW, BodyState.BATHING_ON_KNEES, BodyState.BATHING_ON_KNEES,
                0.3f, new Companion.AnimLogicInfo(3, 0.9f, 0.7f, 7, 0.4f)
            ));

            RegisterAnimation(Companion.Animation.BATHTUB_ON_KNEES_TO_TOWEL_IDLE,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.BATHTUB_TOWEL_IDLE_LOOP),
                "bathtub_on_knees_to_towel_idle", "bathtub_on_knees_to_towel_idle", "bathtub_on_knees_to_towel_idle",
                Companion.Priority.MEDIUM, BodyState.BATHING_ON_KNEES, BodyState.BATHING_STAND,
                0.3f, new Companion.AnimLogicInfo(1, 0.8f, 0.7f, 7, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_SPLASH } ),
                new CompanionAnimationEvent( 0.6f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 0.7f, 0.5f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.BATHTUB_TOWEL_OUT_SOUND_2_3,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.BATHTUB_TOWEL_IDLE_LOOP),
                "bathtub_towel_out_sound_2_3", "bathtub_towel_out_sound_2_3", "bathtub_towel_out_sound_2_3",
                Companion.Priority.MEDIUM, BodyState.BATHING_STAND, BodyState.BATHING_STAND,
                0.2f, new Companion.AnimLogicInfo(3, 1.0f, 0.4f, 12, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.GET_OUT_2_3, 1.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.BATHTUB_TOWEL_IDLE_LOOP,
                new Companion.AnimationInfo(null,
                "bathtub_towel_idle_loop", "bathtub_towel_idle_loop", "bathtub_towel_idle_loop",
                Companion.Priority.LOW, BodyState.BATHING_STAND, BodyState.BATHING_STAND,
                0.3f, new Companion.AnimLogicInfo(3, 0.8f, 0.7f, 12, 0.4f),
                null,
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.BATHTUB_TOWEL_EMBARRASSED_TO_BATHTUB_IDLE,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.BATHTUB_ANGRY_IDLE_LOOP),
                "bathtub_towel_embarrassed_to_bathtub_idle", "bathtub_towel_embarrassed_to_bathtub_idle", "bathtub_towel_embarrassed_to_bathtub_idle",
                Companion.Priority.HIGH, BodyState.BATHING_STAND, BodyState.BATHING_IDLE,
                0.3f, new Companion.AnimLogicInfo(1, 0.6f, 0.3f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.SCREAMING, 1.0f } ),
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_MOVEMENT } ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_KERPLUNK } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HAPPY_BEG_START,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_HAPPY_BEG_LOCOMOTION),
                "stand_happy_beg_start", "stand_happy_beg_start", "stand_happy_beg_start",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(1, 1.0f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.000f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.IMPRESSED, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HAPPY_BEG_LOCOMOTION,
                new Companion.AnimationInfo(null,
                "stand_happy_beg_loop", "stand_locomotion_spine_stable", "stand_happy_beg_loop",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(1, 0.8f, 0.5f, 12, 0.32f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.141f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f, 0.6f } ),
                new CompanionAnimationEvent( 0.185f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.5f } ),
                new CompanionAnimationEvent( 0.280f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 0.0f, 1.0f, 0.7f } ),
                new CompanionAnimationEvent( 0.280f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f, 0.7f } ),
                new CompanionAnimationEvent( 0.361f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 1.0f, 0.6f } ),
                new CompanionAnimationEvent( 0.361f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.6f } ),
                new CompanionAnimationEvent( 0.400f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 0.0f, 1.0f, 0.6f } ),
                new CompanionAnimationEvent( 0.400f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f, 0.6f } ),
                new CompanionAnimationEvent( 0.551f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 1.0f, 0.6f } ),
                new CompanionAnimationEvent( 0.551f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.6f } ),
                new CompanionAnimationEvent( 0.811f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 0.0f, 1.0f, 0.7f } ),
                new CompanionAnimationEvent( 0.811f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f, 0.5f } ),
                new CompanionAnimationEvent( 0.890f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 1.0f, 1.0f } ),
                new CompanionAnimationEvent( 0.890f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.4f } ),

                new CompanionAnimationEvent( 0.000f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.WHINE_SOFT, 1.0f } ),
                new CompanionAnimationEvent( 0.356f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.WHINE, 1.0f } ),
                new CompanionAnimationEvent( 0.800f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.WHINE_SOFT, 1.0f } ),
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 1f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HAPPY_BEG_END,
                new Companion.AnimationInfo(idleTransition,
                "stand_happy_beg_end", "stand_happy_beg_end", "stand_happy_beg_end",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(3, 0.6f, 0.5f, 7, 0.27997f),
                null
            ));

            RegisterAnimation(Companion.Animation.STAND_HAPPY_IDLE1,
                new Companion.AnimationInfo(null, "stand_happy_loop", "stand_happy_loop", "stand_happy_idle",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(3, 1.0f, 0.5f, 7, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 1f } ),
                },
                AnimationInfo.Flag.IDLE_STATE
            ));
            animationInfos[Companion.Animation.STAND_HAPPY_IDLE1].setTorsoSyncFloatID(Animator.StringToHash("sync_locomotion_happy"));

            RegisterAnimation(Companion.Animation.STAND_HAPPY_IDLE2,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_HAPPY_IDLE1),
                "stand_idle2_happy", "stand_locomotion_spine_stable", "stand_idle2_happy",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(3, 1.0f, 0.5f, 7, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 1f } ),
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.THINKING_LONG, 0.0f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.THINKING_SHORT, 0.0f } ),
                },
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.STAND_CONFUSED,
                new Companion.AnimationInfo(idleTransition,
                "stand_confused", "stand_confused", "stand_confused",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(1, 0.6f, 0.3f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.CONFUSED, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HAPPY_SOCIAL1,
                new Companion.AnimationInfo(idleTransition,
                "stand_happy_social1", "stand_happy_social1", "stand_happy_social1",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(1, 0.6f, 0.3f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                },
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.STAND_ANGRY_IDLE1,
                new Companion.AnimationInfo(null, "stand_angry_loop", "stand_angry_loop", "stand_angry_idle",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(3, 1.0f, 0.5f, 7, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 1f } ),
                },
                AnimationInfo.Flag.IDLE_STATE
            ));
            animationInfos[Companion.Animation.STAND_ANGRY_IDLE1].setTorsoSyncFloatID(Animator.StringToHash("sync_locomotion_angry"));

            RegisterAnimation(Companion.Animation.STAND_AGREE,
                new Companion.AnimationInfo(idleTransition,
                "stand_agree", "stand_locomotion_spine_stable", "floor_sit_agree",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(1, 0.5f, 0.3f, 12, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.GIGGLE, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_REFUSE,
                new Companion.AnimationInfo(idleTransition,
                "stand_angry_refuse", "stand_angry_refuse", "stand_angry_refuse",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.65f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.REFUSE, 0.0f } ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 1.1f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_TIRED_REFUSE,
                new Companion.AnimationInfo(idleTransition,
                "stand_tired_refuse", "stand_tired_refuse", "stand_tired_refuse",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.65f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.SLEEP_DISTURBED_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 0.5f, 0.7f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_STRETCH,
                new Companion.AnimationInfo(idleTransition,
                "stand_stretch", "stand_stretch", "stand_stretch",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.SLEEP_DISTURBED_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 0.5f, 0.5f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.4f } ),
                },
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.STAND_POSE_PEACE_IN,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_POSE_PEACE_LOOP),
                "stand_pose_peace_in", "stand_pose_peace_in", "stand_pose_peace_in",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.55f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.PEACE, 0.0f } ),
                },
                AnimationInfo.Flag.DISABLE_HOLDIK_BOTH
            ));

            RegisterAnimation(Companion.Animation.STAND_POSE_PEACE_LOOP,
                new Companion.AnimationInfo(null,
                "stand_pose_peace_loop", "stand_pose_peace_loop", "stand_pose_peace_loop",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.55f, 0, 0.4f), null,
                AnimationInfo.Flag.DISABLE_HOLDIK_BOTH
            ));

            RegisterAnimation(Companion.Animation.STAND_HAPPY_TAKE_PHOTO_IN,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_HAPPY_TAKE_PHOTO_LOOP),
                "stand_happy_take_photo_in", "stand_happy_take_photo_in", "stand_happy_take_photo_in",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{(float)Companion.VoiceLine.MISC_IDLE,0.0f} ),
                },
                AnimationInfo.Flag.DISABLE_HOLDIK_BOTH
            ));

            RegisterAnimation(Companion.Animation.STAND_HAPPY_TAKE_PHOTO_LOOP,
                new Companion.AnimationInfo(null,
                "stand_happy_take_photo_loop", "stand_happy_take_photo_loop", "stand_happy_take_photo_loop",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(1, 0.1f, 1.0f, 7, 1.0f),
                _flags: AnimationInfo.Flag.DISABLE_HOLDIK_BOTH
            ));

            RegisterAnimation(Companion.Animation.STAND_HAPPY_TAKE_PHOTO_OUT_RIGHT,
                new Companion.AnimationInfo(null,
                "stand_happy_take_photo_out_right_nm", "stand_happy_take_photo_out_right_nm", "stand_happy_take_photo_out_right_nm",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(3, 1.0f, 0.6f, 7, 0.5f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{(float)Companion.VoiceLine.THINKING_SHORT,0.0f} ),
                },
                AnimationInfo.Flag.DISABLE_HOLDIK_RIGHT
            ));

            RegisterAnimation(Companion.Animation.STAND_HAPPY_TAKE_PHOTO_OUT_LEFT,
                new Companion.AnimationInfo(null,
                "stand_happy_take_photo_out_left_nm", "stand_happy_take_photo_out_left_nm", "stand_happy_take_photo_out_left_nm",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(3, 1.0f, 0.6f, 7, 0.5f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{(float)Companion.VoiceLine.THINKING_SHORT,0.0f} ),
                },
                AnimationInfo.Flag.DISABLE_HOLDIK_LEFT
            ));

            RegisterAnimation(Companion.Animation.STAND_CATTAIL_IDLE1_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_cattail_idle1_right", "stand_cattail_idle1_right", "stand_cattail_idle1_right",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.4f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.000f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.HUMMING, 1.0f } ),
                new CompanionAnimationEvent( 0.272f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.CONFUSED, 1.0f } ),
                new CompanionAnimationEvent( 0.606f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.TROUBLED, 1.0f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.STAND_CATTAIL_SWING_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_cattail_swing_right", "stand_cattail_swing_right", "stand_cattail_swing_right",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.35f, new Companion.AnimLogicInfo(1, 0.4f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.05f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.EFFORT_ANGRY, 0.0f } ),
                new CompanionAnimationEvent( 0.45f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.EFFORT_ANGRY, 0.0f } ),
                new CompanionAnimationEvent( 0.349f, (int)Companion.AnimationEventName.ATTEMPT_CATTAIL_HIT, null ),
                new CompanionAnimationEvent( 0.651f, (int)Companion.AnimationEventName.ATTEMPT_CATTAIL_HIT, null ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.FLOOR_SIT_CHOPSTICKS_IN,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.FLOOR_SIT_CHOPSTICKS_IDLE),
                "sit_floor_chopsticks_in", "sit_floor_chopsticks_in", "sit_floor_chopsticks_idle",
                Companion.Priority.MEDIUM, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.2f, new Companion.AnimLogicInfo(1, 0.6f, 0.5f, 7, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_CHOPSTICKS_IDLE,
                new Companion.AnimationInfo(null,
                "sit_floor_chopsticks_idle", "sit_floor_chopsticks_idle", "sit_floor_chopsticks_idle",
                Companion.Priority.MEDIUM, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.2f, new Companion.AnimLogicInfo(1, 0.6f, 1.0f, 7, 0.6f),
                null
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_HYPE,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.FLOOR_SIT_LOCOMOTION_HAPPY),
                "sit_floor_hype", "sit_floor_hype", "sit_floor_hype",
                Companion.Priority.MEDIUM, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.2f, new Companion.AnimLogicInfo(1, 0.5f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.HEADPAT_HAPPY, 0.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_CHOPSTICKS_WORRIED,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.FLOOR_SIT_CHOPSTICKS_IDLE),
                "sit_floor_chopsticks_worried", "sit_floor_chopsticks_worried", "sit_floor_chopsticks_worried",
                Companion.Priority.MEDIUM, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.WORRY, 0.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_CHOPSTICKS_NEUTRAL,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.FLOOR_SIT_CHOPSTICKS_IDLE),
                "sit_floor_chopsticks_neutral", "sit_floor_chopsticks_neutral", "sit_floor_chopsticks_neutral",
                Companion.Priority.MEDIUM, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.25f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.05f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.THINKING_LONG, 0.0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.THINKING_LONG, 0.0f } ),
                new CompanionAnimationEvent( 0.75f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 1.0f, 0.6f } ),
                new CompanionAnimationEvent( 0.75f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.6f } )
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_CHOPSTICKS_CONFIDENT,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.FLOOR_SIT_CHOPSTICKS_IDLE),
                "sit_floor_chopsticks_confident", "sit_floor_chopsticks_confident", "sit_floor_chopsticks_confident",
                Companion.Priority.MEDIUM, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.2f, new Companion.AnimLogicInfo(1, 0.8f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.CONFIDENT, 0.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_CHOPSTICKS_LOSE,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.FLOOR_SIT_TO_STAND),
                "sit_floor_chopsticks_lose", "sit_floor_chopsticks_lose", "sit_floor_chopsticks_lose",
                Companion.Priority.MEDIUM, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.LOSE, 0.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_CHOPSTICKS_WIN,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.FLOOR_SIT_LOCOMOTION_HAPPY),
                "sit_floor_chopsticks_win", "sit_floor_chopsticks_win", "sit_floor_chopsticks_win",
                Companion.Priority.MEDIUM, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.WIN, 0.0f } ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 1.0f, 0.8f } ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.3f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_CHOPSTICKS_RECEIVE_RIGHT,
            new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.FLOOR_SIT_CHOPSTICKS_IDLE),
                "sit_floor_chopsticks_receive_right", "sit_floor_chopsticks_idle", "sit_floor_chopsticks_idle",
                Companion.Priority.MEDIUM, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.2f, null,
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_CHOPSTICKS_RECEIVE_LEFT,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.FLOOR_SIT_CHOPSTICKS_IDLE),
                "sit_floor_chopsticks_receive_left", "sit_floor_chopsticks_idle", "sit_floor_chopsticks_idle",
                Companion.Priority.MEDIUM, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.2f, null,
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_CHOPSTICKS_REDISTRIBUTE,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.FLOOR_SIT_CHOPSTICKS_IDLE),
                "sit_floor_chopsticks_redistribute", "sit_floor_chopsticks_idle", "sit_floor_chopsticks_idle",
                Companion.Priority.MEDIUM, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.2f, null,
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.35f, (int)Companion.AnimationEventName.REDISTRIBUTE_CHOPSTICKS_FINGERS, null ),
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HAPPY_JOY_JUMPS,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_HAPPY_IDLE1),
                "stand_happy_joy_jumps", "stand_happy_joy_jumps", "stand_happy_joy_jumps",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(0, 1.0f, 0.75f, 0, 0.4f), null
            ));

            RegisterAnimation(Companion.Animation.STAND_HAPPY_DISAPPOINTMENT,    //todo: separate animation?
                new Companion.AnimationInfo(idleTransition,
                "stand_headpat_happy_wanted_more", "stand_happy_loop", "stand_headpat_happy_wanted_more",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(3, 1.0f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.DISAPPOINTED_STARTLE, 0.0f } ),
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.DISAPPOINTED_SIGH, 0.0f } ),
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 0.0f, 1.0f, 0.8f } ),
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f, 0.3f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.8f } ),
                new CompanionAnimationEvent( 0.9f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.3f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_WAVE_HAPPY_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_wave_happy_right", "stand_wave_happy_right", "stand_wave_happy_right",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(1, 0.5f, 0.65f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.GIGGLE, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_WAVE_HAPPY_LEFT,
                new Companion.AnimationInfo(idleTransition,
                "stand_wave_happy_left", "stand_wave_happy_left", "stand_wave_happy_left",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(1, 0.5f, 0.65f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.GIGGLE, 0.0f } ),
                }
            ));

            RegisterLegSpeedInfo("stand_giddy_loop", 2.4f, 2.0f, 255.0f, 30.0f);


            RegisterAnimation(Companion.Animation.STAND_REACH_OUT_RIGHT,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_REACH_OUT_END_RIGHT),
                "stand_reach_out_right", "stand_reach_out_right", "stand_reach_out_right",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.1f, new Companion.AnimLogicInfo(3, 0.6f, 0.5f, 7, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.554f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f, 0.4f } ),
                new CompanionAnimationEvent( 0.554f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 0.0f, 1.0f, 0.6f } ),
                new CompanionAnimationEvent( 0.679f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 7.0f, 0.4f } ),
                new CompanionAnimationEvent( 0.679f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 0.6f, 0.6f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.STAND_REACH_OUT_END_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_reach_out_end_right", "stand_reach_out_end_right", "stand_reach_out_end_right",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(3, 0.6f, 0.5f, 7, 0.4f),
                null
            ), true);

            RegisterAnimation(Companion.Animation.STAND_TO_HORSEBACK_IDLE_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_to_horseback_Idle_right", "stand_to_horseback_Idle_right", "stand_to_horseback_Idle_right",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.HORSEBACK,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.3f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.2f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.EFFORT, 0.0f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 0.9f, 0.5f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 7.0f, 0.4f } ),
                new CompanionAnimationEvent( 0.9f, (int)Companion.AnimationEventName.BEGIN_MOUNT, null )
                }
            ), true);

            RegisterAnimation(Companion.Animation.HORSEBACK_IDLE_LOOP,
                new Companion.AnimationInfo(null,
                "horseback_Idle_loop", "horseback_Idle_loop", "horseback_Idle_loop",
                Companion.Priority.LOW, BodyState.HORSEBACK, BodyState.HORSEBACK,
                0.3f, new Companion.AnimLogicInfo(3, 0.9f, 0.5f, 7, 0.4f), null,
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.HORSEBACK_HARD_STOP,
                new Companion.AnimationInfo(idleTransition,
                "horseback_hard_stop", "horseback_hard_stop", "horseback_hard_stop",
                Companion.Priority.HIGH, BodyState.HORSEBACK, BodyState.HORSEBACK,
                0.3f, new Companion.AnimLogicInfo(0, 0.9f, 0.1f, 12, 0.1f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.SCARED_SHORT, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.HORSEBACK_JOY,
                new Companion.AnimationInfo(idleTransition,
                "horseback_joy", "horseback_joy", "horseback_joy",
                Companion.Priority.HIGH, BodyState.HORSEBACK, BodyState.HORSEBACK,
                0.3f, new Companion.AnimLogicInfo(0, 0.9f, 0.4f, 12, 0.1f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ECSTATIC, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.HORSEBACK_TO_STAND,
                new Companion.AnimationInfo(idleTransition,
                "horseback_to_stand", "horseback_to_stand", "stand_happy_idle",
                Companion.Priority.LOW, BodyState.HORSEBACK, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 0.9f, 0.4f, 12, 0.1f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.STOP_SPINE_ANCHOR, null ),
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.DISABLE_STAND_ON_GROUND, null ),
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_ANGRY_THROW_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_angry_throw_right", "stand_angry_throw_right", "stand_angry_throw_right",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(1, 0.4f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.400f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.EFFORT, 0.0f } ),
                new CompanionAnimationEvent( 0.483f, (int)Companion.AnimationEventName.THROW_RIGHT_HAND_OBJECT, null ),
                },
                AnimationInfo.Flag.DISABLE_HOLDIK_RIGHT
            ), true);

            RegisterAnimation(Companion.Animation.STAND_HAPPY_SMELL_ITEM_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_happy_donut_smell_right", "stand_happy_donut_smell_right", "stand_happy_donut_smell_right",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.45f, 0, 0.36f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.200f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.DONUT_SMELL_A, 1.0f } ),
                new CompanionAnimationEvent( 0.573f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.DONUT_SMELL_B, 1.0f } ),
                new CompanionAnimationEvent( 0.927f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.6f } ),
                new CompanionAnimationEvent( 0.927f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.4f } ),
                },
                AnimationInfo.Flag.DISABLE_HOLDIK_RIGHT
            ), true);

            RegisterAnimation(Companion.Animation.STAND_HAPPY_EAT_ITEM_RIGHT,
                new Companion.AnimationInfo(idleTransition
                , "stand_happy_donut_eat_right", "stand_locomotion_spine_stable", "stand_happy_donut_eat_right",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(0, 1.0f, 0.55f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.080f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.EAT, 1.0f } ),
                new CompanionAnimationEvent( 0.129f, (int)Companion.AnimationEventName.EAT_CURRENT_INTEREST, null ),
                new CompanionAnimationEvent( 0.800f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 1.0f } ),
                new CompanionAnimationEvent( 0.818f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 7.0f, 0.7f } ),
                },
                AnimationInfo.Flag.DISABLE_HOLDIK_RIGHT
            ), true);

            RegisterAnimation(Companion.Animation.STAND_HAPPY_DONUT_LAST_BITE_RIGHT,
                new Companion.AnimationInfo(idleTransition
                , "stand_happy_donut_last_bite_right", "stand_locomotion_spine_stable", "stand_happy_donut_last_bite_right",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.080f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.EAT, 1.0f } ),
                new CompanionAnimationEvent( 0.127f, (int)Companion.AnimationEventName.EAT_CURRENT_INTEREST, null )
                },
                AnimationInfo.Flag.DISABLE_HOLDIK_RIGHT
            ), true);

            RegisterAnimation(Companion.Animation.STAND_SHINOBU_YUMMY,
                new Companion.AnimationInfo(idleTransition
                , "stand_shinobu_yummy", "stand_shinobu_yummy", "stand_shinobu_yummy",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.080f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.YUMMY, 1.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_WEAR_SUNHAT_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_wear_sunhat_right", "stand_wear_sunhat_right", "stand_wear_sunhat_right",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.35f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.10f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 1.0f } ),
                new CompanionAnimationEvent( 0.6f, (int)Companion.AnimationEventName.WEAR_HAT_RIGHT_HAND, null ),
                },
                AnimationInfo.Flag.DISABLE_HOLDIK_RIGHT
            ), true);

            RegisterAnimation(Companion.Animation.STAND_POLAROID_FRAME_REACT_IN_RIGHT,
                new Companion.AnimationInfo(new IdleBehavior.TransitionToPolaroidFrameReact(),
                "stand_polaroid_frame_react_in_right", "stand_locomotion_spine_stable", "stand_polaroid_frame_react_in_right",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.55f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.000f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.THINKING_SHORT, 0.0f } ),
                },
                AnimationInfo.Flag.DISABLE_HOLDIK_RIGHT
            ), true);


            RegisterAnimation(Companion.Animation.STAND_POLAROID_FRAME_REACT_NORMAL_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_polaroid_frame_react_normal_right", "stand_locomotion_spine_stable", "stand_polaroid_frame_react_normal_right",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.55f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.250f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.THINKING_LONG, 1.0f } ),
                new CompanionAnimationEvent( 0.850f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 1.0f } ),
                new CompanionAnimationEvent( 0.850f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 7.0f, 1.0f } ),
                },
                AnimationInfo.Flag.DISABLE_HOLDIK_RIGHT
            ), true);

            RegisterAnimation(Companion.Animation.STAND_POLAROID_FRAME_REACT_PANTY_RIGHT,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_POLAROID_FRAME_REACT_RIP),
                "stand_polaroid_frame_react_panty_right", "stand_polaroid_frame_react_panty_right", "stand_polaroid_frame_react_panty_right",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.55f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.000f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.SCARED_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.400f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_GRUMBLE_LONG, 1.0f } ),
                new CompanionAnimationEvent( 0.500f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 1.0f, 0.4f } ),
                new CompanionAnimationEvent( 0.500f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.4f } ),
                },
                AnimationInfo.Flag.DISABLE_HOLDIK_RIGHT
            ), true);

            RegisterAnimation(Companion.Animation.STAND_POLAROID_FRAME_REACT_RIP,
                new Companion.AnimationInfo(idleTransition,
                "stand_polaroid_frame_rip", "stand_polaroid_frame_rip", "stand_polaroid_frame_rip",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.55f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.000f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.RIP, 1.0f } ),
                new CompanionAnimationEvent( 0.167f, (int)Companion.AnimationEventName.SPAWN_POLAROID_FRAME_RIPPED_FX, null ),
                new CompanionAnimationEvent( 0.283f, (int)Companion.AnimationEventName.SPAWN_POLAROID_FRAME_RIPPED_FX, null ),
                new CompanionAnimationEvent( 0.400f, (int)Companion.AnimationEventName.SPAWN_POLAROID_FRAME_RIPPED_FX, null ),
                new CompanionAnimationEvent( 0.500f, (int)Companion.AnimationEventName.SPAWN_POLAROID_FRAME_RIPPED_FX, null ),
                new CompanionAnimationEvent( 0.700f, (int)Companion.AnimationEventName.SPAWN_POLAROID_FRAME_RIPPED_FX, null ),
                new CompanionAnimationEvent( 0.800f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 1.1f } ),
                new CompanionAnimationEvent( 0.800f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 7.0f, 1.0f } ),
                },
                AnimationInfo.Flag.DISABLE_HOLDIK_BOTH
            ));

            RegisterAnimation(Companion.Animation.STAND_CHASE_LOCOMOTION,
                new Companion.AnimationInfo(null,
                "stand_jealous_loop", "stand_jealous_loop", "stand_jealous_loop",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.35f, new Companion.AnimLogicInfo(),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 1f } ),
                },
                AnimationInfo.Flag.IDLE_STATE
            ));
            animationInfos[Companion.Animation.STAND_CHASE_LOCOMOTION].setTorsoSyncFloatID(Animator.StringToHash("sync_locomotion_jealous"));


            RegisterAnimation(Companion.Animation.STAND_WEAR_BAG_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_wear_bag_right_nm", "stand_happy_loop", "stand_happy_idle",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.000f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 1.0f } ),
                new CompanionAnimationEvent( 0.675f, (int)Companion.AnimationEventName.ATTACH_BAG_SHOULDER, null ),
                new CompanionAnimationEvent( 0.800f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.5f } ),
                new CompanionAnimationEvent( 0.800f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 7.0f, 0.5f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.STAND_REMOVE_BAG_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_remove_bag_right_nm", "stand_happy_loop", "stand_happy_idle",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.000f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 1.0f } ),
                new CompanionAnimationEvent( 0.410f, (int)Companion.AnimationEventName.REMOVE_BAG_SHOULDER, null ),
                new CompanionAnimationEvent( 0.800f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.5f } ),
                new CompanionAnimationEvent( 0.800f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 7.0f, 0.5f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.STAND_BAG_PUT_IN_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_bag_put_in_right", "stand_happy_loop", "stand_happy_idle",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(0, 1.0f, 0.7f, 0, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.000f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 1.0f } ),
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.OPEN_BAG_RIGHT, null ),
                new CompanionAnimationEvent( 0.700f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.5f } ),
                new CompanionAnimationEvent( 0.700f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 7.0f, 0.5f } ),
                new CompanionAnimationEvent( 0.55f, (int)Companion.AnimationEventName.PUT_ITEM_IN_BAG_RIGHT, null ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.STAND_LOCOMOTION_JEALOUS,
                new Companion.AnimationInfo(null,
                "stand_jealous_loop", "stand_jealous_loop", "stand_jealous_loop",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.35f, new Companion.AnimLogicInfo(),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 1f } ),
                },
                AnimationInfo.Flag.IDLE_STATE
            ));
            animationInfos[Companion.Animation.STAND_LOCOMOTION_JEALOUS].setTorsoSyncFloatID(Animator.StringToHash("sync_locomotion_jealous"));

            RegisterAnimation(Companion.Animation.STAND_ANGRY_TIP_TOE_REACH,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_ANGRY_TIP_TOE_REACH_END),
                "stand_angry_tip_toe_reach", "stand_angry_tip_toe_reach", "stand_angry_tip_toe_reach",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(3, 1.0f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.05f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_LONG, 1.0f } ),
                new CompanionAnimationEvent( 0.100f, (int)Companion.AnimationEventName.TOGGLE_FACE_YAW_SUM_FOR_ANIMATION, null ),
                new CompanionAnimationEvent( 0.220f, (int)Companion.AnimationEventName.TOGGLE_FACE_YAW_SUM_FOR_ANIMATION, null ),
                new CompanionAnimationEvent( 0.312f, (int)Companion.AnimationEventName.TOGGLE_FACE_YAW_SUM_FOR_ANIMATION, null ),
                new CompanionAnimationEvent( 0.532f, (int)Companion.AnimationEventName.TOGGLE_FACE_YAW_SUM_FOR_ANIMATION, null ),
                new CompanionAnimationEvent( 0.578f, (int)Companion.AnimationEventName.TOGGLE_FACE_YAW_SUM_FOR_ANIMATION, null ),
                new CompanionAnimationEvent( 0.716f, (int)Companion.AnimationEventName.TOGGLE_FACE_YAW_SUM_FOR_ANIMATION, null ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_ANGRY_TIP_TOE_REACH_END,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_LOCOMOTION_JEALOUS),
                "stand_angry_tip_toe_reach_end", "stand_angry_tip_toe_reach_end", "stand_angry_tip_toe_reach_end",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(1, 1.0f, 0.5f, 12, 0.4f), null
            ));
            RegisterAnimation(Companion.Animation.STAND_ANGRY_JEALOUS_STEAL_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_angry_jealous_steal_right", "stand_angry_jealous_steal_right", "stand_angry_jealous_steal_right",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 12, 0.52f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.200f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.EFFORT_ANGRY, 0.0f } ),
				// new CompanionAnimationEvent( 0.460f, (int)Companion.AnimationEventName.STEAL_CURRENT_INTEREST, null ),
				new CompanionAnimationEvent( 0.460f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 0.8f, 0.5f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.STAND_ANGRY_JEALOUS,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_LOCOMOTION_JEALOUS),
                "stand_angry_jealous", "stand_angry_jealous", "stand_angry_jealous",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(3, 1.0f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.062f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.DISAPPOINTED_STARTLE, 0.0f } ),
                new CompanionAnimationEvent( 0.175f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 0.0f, 1.0f, 0.4f } ),
                new CompanionAnimationEvent( 0.200f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f, 0.4f } ),
                new CompanionAnimationEvent( 0.325f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_POUT, 0.0f } ),
                new CompanionAnimationEvent( 0.830f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.4f } ),
                new CompanionAnimationEvent( 0.830f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.4f } ),
                }
            ));

            RegisterLegSpeedInfo("stand_jealous_loop", 2.4f, 2.0f, 255.0f, 30.0f);


            RegisterAnimation(Companion.Animation.CRAWL_TIRED_TO_STAND,
                new Companion.AnimationInfo(idleTransition,
                "crawl_tired_to_stand", "crawl_tired_to_stand", "stand_happy_idle",
                Companion.Priority.MEDIUM, BodyState.CRAWL_TIRED, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.3f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{(float)Companion.VoiceLine.MISC_IDLE,0.1f} ),
                },
                AnimationInfo.Flag.DISABLE_RAGDOLL_CHECK
            ));

            RegisterAnimation(Companion.Animation.STAND_TIRED_TO_BED_CRAWL_TIRED,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.CRAWL_TIRED_IDLE),
                "stand_tired_to_bed_crawl_tired", "stand_tired_to_bed_crawl_tired", "stand_tired_to_bed_crawl_tired",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.CRAWL_TIRED,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.3f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{(float)Companion.VoiceLine.MISC_IDLE,0.0f} ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.JUMP_ON_BED, null )
                },
                AnimationInfo.Flag.DISABLE_RAGDOLL_CHECK
            ));

            RegisterAnimation(Companion.Animation.CRAWL_TIRED_IDLE,
                new Companion.AnimationInfo(null,
                "crawl_tired_idle", "crawl_tired_idle", "crawl_tired_idle",
                Companion.Priority.LOW, BodyState.CRAWL_TIRED, BodyState.CRAWL_TIRED,
                0.3f, new Companion.AnimLogicInfo(1, 0.5f, 0.3f, 7, 0.4f),
                null,
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.CRAWL_TIRED_TO_LAY_PILLOW_SIDE_HAPPY_RIGHT,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.AWAKE_PILLOW_SIDE_HAPPY_IDLE_RIGHT),
                "crawl_tired_to_lay_side_pillow_happy_right", "crawl_tired_to_lay_side_pillow_happy_right", "crawl_tired_to_lay_side_pillow_happy_right",
                Companion.Priority.LOW, BodyState.CRAWL_TIRED, BodyState.AWAKE_PILLOW_SIDE_RIGHT,
                0.3f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.ROLL_ON_BED, null )
                }
            ), true);


            RegisterAnimation(Companion.Animation.AWAKE_PILLOW_SIDE_HAPPY_IDLE_RIGHT,
                new Companion.AnimationInfo(null,
                "lay_side_pillow_happy_idle_loop_right", "lay_side_pillow_happy_idle_loop_right", "lay_side_pillow_happy_idle_loop_right",
                Companion.Priority.LOW, BodyState.AWAKE_PILLOW_SIDE_RIGHT, BodyState.AWAKE_PILLOW_SIDE_RIGHT,
                0.3f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 7, 0.4f),
                null,
                AnimationInfo.Flag.IDLE_STATE
            ), true);

            RegisterAnimation(Companion.Animation.AWAKE_PILLOW_SIDE_YAWN_LONG_RIGHT,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.AWAKE_PILLOW_SIDE_HAPPY_IDLE_RIGHT),
                "lay_side_pillow_yawn_long_right", "lay_side_pillow_yawn_long_right", "lay_side_pillow_yawn_long_right",
                Companion.Priority.MEDIUM, BodyState.AWAKE_PILLOW_SIDE_RIGHT, BodyState.AWAKE_PILLOW_SIDE_RIGHT,
                0.3f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.05f, (int)Companion.AnimationEventName.SPEAK, new float[]{(float)Companion.VoiceLine.YAWN_LONG,1.0f} ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.AWAKE_PILLOW_SIDE_TO_SLEEP_PILLOW_SIDE_RIGHT,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.SLEEP_PILLOW_SIDE_IDLE_RIGHT),
                "lay_side_pillow_to_sleep_side_pillow_right", "lay_side_pillow_to_sleep_side_pillow_right", "lay_side_pillow_to_sleep_side_pillow_right",
                Companion.Priority.MEDIUM, BodyState.AWAKE_PILLOW_SIDE_RIGHT, BodyState.SLEEP_PILLOW_SIDE_RIGHT,
                0.3f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.05f, (int)Companion.AnimationEventName.SPEAK, new float[]{(float)Companion.VoiceLine.MISC_IDLE,0.0f} ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.AWAKE_PILLOW_SIDE_SOUND_GOODNIGHT_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "lay_side_pillow_sound_goodnight_right", "lay_side_pillow_sound_goodnight_right", "lay_side_pillow_sound_goodnight_right",
                Companion.Priority.MEDIUM, BodyState.AWAKE_PILLOW_SIDE_RIGHT, BodyState.AWAKE_PILLOW_SIDE_RIGHT,
                0.3f, new Companion.AnimLogicInfo(0, 0.2f, 0.3f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.05f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.GOODNIGHT, 1.0f } ),
                }
            ), true);


            RegisterAnimation(Companion.Animation.SLEEP_PILLOW_SIDE_IDLE_RIGHT,
                new Companion.AnimationInfo(null,
                "sleep_side_pillow_idle_loop_right", "sleep_side_pillow_idle_loop_right", "sleep_side_pillow_idle_loop_right",
                Companion.Priority.LOW, BodyState.SLEEP_PILLOW_SIDE_RIGHT, BodyState.SLEEP_PILLOW_SIDE_RIGHT,
                0.3f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.SLEEP_BREATHING, 1.0f } )
                },
                AnimationInfo.Flag.IDLE_STATE
            ), true);

            RegisterAnimation(Companion.Animation.SLEEP_PILLOW_SIDE_TO_SLEEP_PILLOW_UP_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "sleep_side_pillow_to_sleep_pillow_up_right_nm", "sleep_side_pillow_to_sleep_pillow_up_right_nm", "sleep_side_pillow_to_sleep_pillow_up_right_nm",
                Companion.Priority.MEDIUM, BodyState.SLEEP_PILLOW_SIDE_RIGHT, BodyState.SLEEP_PILLOW_UP,
                0.1f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.SLEEP_DISTURBED_LONG, 0.0f } ),
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.ROLL_ON_BED, null )
                }
            ), true);

            RegisterAnimation(Companion.Animation.SLEEP_PILLOW_UP_IDLE,
                new Companion.AnimationInfo(null,
                "sleep_pillow_up_idle_loop", "sleep_pillow_up_idle_loop", "sleep_pillow_up_idle_loop",
                Companion.Priority.LOW, BodyState.SLEEP_PILLOW_UP, BodyState.SLEEP_PILLOW_UP,
                0.1f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.SLEEP_BREATHING, 1.0f } )
                },
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.SLEEP_PILLOW_UP_TO_SLEEP_PILLOW_SIDE_RIGHT,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.SLEEP_PILLOW_SIDE_IDLE_RIGHT),
                "sleep_pillow_up_to_sleep_side_pillow_right_nm", "sleep_pillow_up_to_sleep_side_pillow_right_nm", "sleep_pillow_up_to_sleep_side_pillow_right_nm",
                Companion.Priority.MEDIUM, BodyState.SLEEP_PILLOW_UP, BodyState.SLEEP_PILLOW_SIDE_RIGHT,
                0.1f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.SLEEP_DISTURBED_LONG, 0.0f } ),
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.ROLL_ON_BED, null )
                }
            ), true);


            RegisterAnimation(Companion.Animation.SLEEP_PILLOW_SIDE_HEADPAT_START_RIGHT,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.SLEEP_PILLOW_SIDE_IDLE_RIGHT),
                "sleep_side_pillow_headpat_start_right", "sleep_side_pillow_headpat_start_right", "sleep_side_pillow_headpat_start_right",
                Companion.Priority.LOW, BodyState.SLEEP_PILLOW_SIDE_RIGHT, BodyState.SLEEP_PILLOW_SIDE_RIGHT,
                0.3f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.HEADPAT_RETURN, 1.0f } )
                }
            ), true);

            RegisterAnimation(Companion.Animation.SLEEP_PILLOW_UP_TO_AWAKE_HAPPY_PILLOW_UP,
                new Companion.AnimationInfo(idleTransition,
                "sleep_pillow_up_to_awake_happy_pillow_up", "sleep_pillow_up_to_awake_happy_pillow_up", "sleep_pillow_up_to_awake_happy_pillow_up",
                Companion.Priority.MEDIUM, BodyState.SLEEP_PILLOW_UP, BodyState.AWAKE_PILLOW_UP,
                0.3f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.YAWN_SHORT, 1.0f } ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                new CompanionAnimationEvent( 0.55f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.4f } ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 0.65f, 0.5f } )
                }
            ));

            RegisterAnimation(Companion.Animation.SLEEP_PILLOW_UP_TO_AWAKE_ANGRY_PILLOW_UP,
                new Companion.AnimationInfo(idleTransition,
                "sleep_pillow_up_to_awake_angry_pillow_up", "sleep_pillow_up_to_awake_angry_pillow_up", "sleep_pillow_up_to_awake_angry_pillow_up",
                Companion.Priority.MEDIUM, BodyState.SLEEP_PILLOW_UP, BodyState.AWAKE_PILLOW_UP,
                0.3f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 1.0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_POUT, 0.0f } ),
                new CompanionAnimationEvent( 0.55f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.4f } ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 0.65f, 0.5f } )
                }
            ));

            RegisterAnimation(Companion.Animation.SLEEP_PILLOW_SIDE_TO_AWAKE_HAPPY_PILLOW_UP_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "sleep_side_pillow_to_awake_happy_pillow_up_right_nm", "sleep_side_pillow_to_awake_happy_pillow_up_right_nm", "sleep_side_pillow_to_awake_happy_pillow_up_right_nm",
                Companion.Priority.MEDIUM, BodyState.SLEEP_PILLOW_SIDE_RIGHT, BodyState.AWAKE_PILLOW_UP,
                0.3f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.03f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 1.0f } ),
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.TROUBLED, 1.0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.ROLL_ON_BED, null ),
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.4f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 0.65f, 0.5f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.SLEEP_PILLOW_SIDE_TO_AWAKE_ANGRY_PILLOW_UP_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "sleep_side_pillow_to_awake_angry_pillow_up_right_nm", "sleep_side_pillow_to_awake_angry_pillow_up_right_nm", "sleep_side_pillow_to_awake_angry_pillow_up_right_nm",
                Companion.Priority.MEDIUM, BodyState.SLEEP_PILLOW_SIDE_RIGHT, BodyState.AWAKE_PILLOW_UP,
                0.3f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_POUT, 0.0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.ROLL_ON_BED, null ),
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.4f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 0.65f, 0.5f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.AWAKE_HAPPY_PILLOW_UP_IDLE,
                new Companion.AnimationInfo(null,
                "awake_happy_pillow_up_idle_loop", "awake_happy_pillow_up_idle_loop", "awake_happy_pillow_up_idle_loop",
                Companion.Priority.LOW, BodyState.AWAKE_PILLOW_UP, BodyState.AWAKE_PILLOW_UP,
                0.3f, new Companion.AnimLogicInfo(1, 0.6f, 0.5f, 7, 0.4f),
                null,
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.AWAKE_ANGRY_PILLOW_UP_IDLE,
                new Companion.AnimationInfo(null,
                "awake_angry_pillow_up_idle_loop", "awake_angry_pillow_up_idle_loop", "awake_angry_pillow_up_idle_loop",
                Companion.Priority.LOW, BodyState.AWAKE_PILLOW_UP, BodyState.AWAKE_PILLOW_UP,
                0.3f, new Companion.AnimLogicInfo(1, 0.6f, 0.5f, 7, 0.4f),
                null,
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.AWAKE_HAPPY_PILLOW_UP_HEADPAT_LOOP,
                new Companion.AnimationInfo(null,
                "awake_happy_pillow_up_headpat_idle", "awake_happy_pillow_up_headpat_idle", "awake_happy_pillow_up_headpat_idle",
                Companion.Priority.LOW, BodyState.AWAKE_PILLOW_UP, BodyState.AWAKE_PILLOW_UP,
                0.5f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.45f, (int)Companion.AnimationEventName.HEADPAT_PROPER_SOUND, null )
                }
            ));



            RegisterAnimation(Companion.Animation.FLOOR_SIT_TO_CRAWL,
                new Companion.AnimationInfo(idleTransition,
                "floor_sit_to_crawl", "floor_sit_to_crawl", "stand_to_horseback_Idle_right",
                Companion.Priority.MEDIUM, BodyState.FLOOR_SIT, BodyState.CRAWL_TIRED,
                0.3f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.EFFORT, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.AWAKE_PILLOW_SIDE_TO_AWAKE_PILLOW_UP_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "awake_side_pillow_to_awake_pillow_up_right", "awake_side_pillow_to_awake_pillow_up_right", "stand_happy_idle",
                Companion.Priority.MEDIUM, BodyState.AWAKE_PILLOW_SIDE_RIGHT, BodyState.AWAKE_PILLOW_UP,
                0.3f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.AWAKE_PILLOW_UP_TO_CRAWL_TIRED,
                new Companion.AnimationInfo(idleTransition,
                "awake_pillow_up_to_crawl", "awake_pillow_up_to_crawl", "awake_pillow_up_to_crawl",
                Companion.Priority.MEDIUM, BodyState.AWAKE_PILLOW_UP, BodyState.CRAWL_TIRED,
                0.3f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.EFFORT, 0.0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.JUMP_ON_BED, null ),
                new CompanionAnimationEvent( 0.95f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.CRAWL_BED_TO_STAND,
                new Companion.AnimationInfo(idleTransition,
                "crawl_bed_to_stand", "crawl_bed_to_stand", "crawl_bed_to_stand",
                Companion.Priority.MEDIUM, BodyState.CRAWL_TIRED, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.EFFORT, 0.0f } ),
                new CompanionAnimationEvent( 0.2f, (int)Companion.AnimationEventName.ROLL_ON_BED, null ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.4f } ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 0.65f, 0.5f } ),
                },
                AnimationInfo.Flag.DISABLE_RAGDOLL_CHECK
            ));


            RegisterAnimation(Companion.Animation.STAND_SOUND_GOOD_MORNING,
                new Companion.AnimationInfo(idleTransition,
                "stand_sound_goodmorning_short", "stand_sound_goodmorning_short", "stand_sound_goodmorning_short",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(3, 0.7f, 0.3f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.05f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.GOODMORNING, 1.0f } ),
                }
            ));

            RegisterLegSpeedInfo("crawl_tired_idle", 0.65f, 1.0f, 100.0f, 7.0f);


            RegisterAnimation(Companion.Animation.STAND_GIDDY_SURPRISE,
                new Companion.AnimationInfo(idleTransition,
                "stand_giddy_surprise", "stand_giddy_surprise", "stand_giddy_surprise",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(3, 0.6f, 0.5f, 12, 1.2f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.080f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.IMPRESSED_VERY, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_GIDDY_LOCOMOTION,
                new Companion.AnimationInfo(null,
                "stand_giddy_loop", "stand_giddy_loop", "stand_giddy_loop",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(1, 1.0f, 0.5f, 7, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 1f } ),
                },
                AnimationInfo.Flag.IDLE_STATE
            ));
            animationInfos[Companion.Animation.STAND_GIDDY_LOCOMOTION].setTorsoSyncFloatID(Animator.StringToHash("sync_giddy_locomotion"));

            RegisterAnimation(Companion.Animation.STAND_PICKUP_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_pickup_right", "stand_pickup_right", "stand_happy_idle",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 0.6f, 0.5f, 7, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.STAND_SEARCH_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_search_right", "stand_search_right", "stand_search_right",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.4f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.DISAPPOINTED_STARTLE, 1.0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.TROUBLED, 1.0f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 1.0f } )
                },
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.STAND_HAPPY_CHANGE_ITEM_HANDS,
                new Companion.AnimationInfo(idleTransition,
                "stand_change_item_hands", "stand_happy_loop", "stand_happy_idle",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(3, 0.5f, 0.4f, 7, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_ANGRY_CHANGE_ITEM_HANDS,
                new Companion.AnimationInfo(idleTransition,
                "stand_change_item_hands", "stand_angry_loop", "stand_angry_idle",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(3, 0.5f, 0.4f, 7, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                }
            ));


            RegisterAnimation(Companion.Animation.STAND_TO_SIT_FLOOR,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.FLOOR_SIT_LOCOMOTION_HAPPY), "stand_to_sit_floor", "stand_to_sit_floor", null,
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.FLOOR_SIT,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.6f, 0, 0.6f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.6f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 1.0f, 0.8f } ),
                new CompanionAnimationEvent( 0.6f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.4f } )
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_LOCOMOTION_HAPPY,
                new Companion.AnimationInfo(null, "floor_sit_locomotion_happy", "floor_sit_locomotion_happy", "stand_happy_idle",
                Companion.Priority.LOW, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.5f, new Companion.AnimLogicInfo(1, 0.6f, 0.5f, 7, 0.4f), null,
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_LOCOMOTION_ANGRY,
                new Companion.AnimationInfo(null, "floor_sit_locomotion_angry", "floor_sit_locomotion_angry", "stand_angry_idle",
                Companion.Priority.LOW, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.5f, new Companion.AnimLogicInfo(1, 0.6f, 0.5f, 7, 0.4f), null,
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_TO_STAND,
                new Companion.AnimationInfo(idleTransition, "sit_floor_to_stand", "sit_floor_to_stand", null,
                Companion.Priority.MEDIUM, BodyState.FLOOR_SIT, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.4f), null));

            RegisterLegSpeedInfo("stand_happy_loop", 1.0f, 2.0f, 255.0f, 30.0f);
            RegisterLegSpeedInfo("stand_angry_loop", 1.1f, 2.0f, 255.0f, 30.0f);
            RegisterLegSpeedInfo("stand_locomotion_spine_stable", 1.0f, 2.0f, 255.0f, 30.0f);
            RegisterLegSpeedInfo("floor_sit_locomotion_happy", 0.6f, 1.6f, 100.0f, 10.0f);
            RegisterLegSpeedInfo("floor_sit_locomotion_angry", 0.6f, 1.6f, 100.0f, 10.0f);
            RegisterLegSpeedInfo("stand_happy_social1", 1.0f, 2.0f, 255.0f, 30.0f);

            RegisterAnimation(Companion.Animation.PHOTOSHOOT_1,
                new Companion.AnimationInfo(null,
                "photoshoot_1", "photoshoot_1", "photoshoot_1",
                Companion.Priority.VERY_HIGH, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.0f, 0, 0.0f)
            ));

            RegisterAnimation(Companion.Animation.PHOTOSHOOT_2,
                new Companion.AnimationInfo(null,
                "photoshoot_2", "photoshoot_2", "photoshoot_2",
                Companion.Priority.VERY_HIGH, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.0f, 0, 0.0f)
            ));

            RegisterAnimation(Companion.Animation.PHOTOSHOOT_3,
                new Companion.AnimationInfo(null,
                "photoshoot_3", "photoshoot_3", "photoshoot_3",
                Companion.Priority.VERY_HIGH, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.0f, 0, 0.0f)
            ));

            RegisterAnimation(Companion.Animation.STAND_OUTFIT_LIKE,
                new Companion.AnimationInfo(idleTransition,
                "stand_outfit_like", "stand_outfit_like", "stand_outfit_like",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.3f, 0, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.2f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.IMPRESSED_VERY, 1.0f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 7.0f, 0.5f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.6f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HANDHOLD_ANGRY_REFUSE_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_handhold_angry_refuse_right", "stand_handhold_angry_refuse_right", "stand_handhold_angry_refuse_right",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.6f, 0, 1.2f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.78f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 0.5f, 1.0f } ),
                new CompanionAnimationEvent( 0.78f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.4f } ),
                new CompanionAnimationEvent( 0.65f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_GRUMBLE_SHORT, 0.0f } ),
                }
            ));
            RegisterAnimation(Companion.Animation.STAND_HANDHOLD_ANGRY_REFUSE_LEFT,
                new Companion.AnimationInfo(idleTransition,
                "stand_handhold_angry_refuse_left", "stand_handhold_angry_refuse_left", "stand_handhold_angry_refuse_left",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.6f, 0, 1.2f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.78f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 0.5f, 1.0f } ),
                new CompanionAnimationEvent( 0.78f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.4f } ),
                new CompanionAnimationEvent( 0.65f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_GRUMBLE_SHORT, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HANDHOLD_HAPPY_EMBARRASSED_RIGHT,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_HANDHOLD_HAPPY_PULL_RIGHT),
                "stand_handhold_happy_embarrassed_right", "stand_handhold_happy_embarrassed_right", "stand_handhold_happy_embarrassed_right",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 12, 1.2f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.EMBARRASSED_HANDHOLD, 1.0f } ),

                new CompanionAnimationEvent( 0.075f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f, 0.5f } ),
                new CompanionAnimationEvent( 0.075f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 0.0f, 1.0f, 0.6f } ),

                new CompanionAnimationEvent( 0.655f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 0.5f, 0.6f } ),
                new CompanionAnimationEvent( 0.655f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.5f } ),

                new CompanionAnimationEvent( 0.735f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 0.0f, 1.0f, 0.6f } ),
                new CompanionAnimationEvent( 0.735f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f, 0.5f } ),

                new CompanionAnimationEvent( 0.88f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 0.5f, 0.6f } ),
                new CompanionAnimationEvent( 0.88f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.5f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.STAND_HANDHOLD_HAPPY_PULL_RIGHT,
                new Companion.AnimationInfo(null,
                "stand_handhold_happy_pull_hard_right", "stand_handhold_happy_pull_hard_right", "stand_handhold_happy_pull_hard_right",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(1, 0.5f, 0.5f, 7, 1.2f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 1f } ),
                }
            ));
            RegisterAnimation(Companion.Animation.STAND_HANDHOLD_HAPPY_PULL_LEFT,
                new Companion.AnimationInfo(null,
                "stand_handhold_happy_pull_hard_left", "stand_handhold_happy_pull_hard_left", "stand_handhold_happy_pull_hard_left",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(1, 0.5f, 0.5f, 7, 1.2f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 1f } ),
                }
            ));

            RegisterLegSpeedInfo("stand_handhold_happy_pull_hard_right", 2.4f, 1.0f, 255.0f, 30.0f);
            RegisterLegSpeedInfo("stand_handhold_happy_pull_hard_left", 2.4f, 1.0f, 255.0f, 30.0f);

            RegisterAnimation(Companion.Animation.STAND_TIRED_HANDHOLD_PULL_RIGHT,
                new Companion.AnimationInfo(null,
                "stand_tired_handhold_right", "stand_tired_handhold_right", "stand_tired_handhold_right",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(1, 0.7f, 0.5f, 7, 1.2f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 1f } ),
                }
            ));
            RegisterAnimation(Companion.Animation.STAND_TIRED_HANDHOLD_PULL_LEFT,
                new Companion.AnimationInfo(null,
                "stand_tired_handhold_left", "stand_tired_handhold_left", "stand_tired_handhold_left",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(1, 0.7f, 0.5f, 7, 1.2f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 1f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.BATHTUB_HEADPAT_BRUSH_AWAY,
                new Companion.AnimationInfo(idleTransition,
                "bathtub_headpat_brush_away", "bathtub_headpat_brush_away", "bathtub_headpat_brush_away",
                Companion.Priority.HIGH, BodyState.BATHING_IDLE, BodyState.BATHING_IDLE,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.3f, 0, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_LONG, 1.0f } ),
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_MOVEMENT } ),
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_SPLASH } ),
                }
            ));

            RegisterAnimation(Companion.Animation.BATHTUB_HEADPAT_ANGRY_IDLE,
                new Companion.AnimationInfo(null,
                "bathtub_headpat_angry_idle", "bathtub_headpat_angry_idle", "bathtub_headpat_angry_idle",
                Companion.Priority.LOW, BodyState.BATHING_IDLE, BodyState.BATHING_IDLE,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.3f, 7, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_POUT, 1.0f } ),
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_MOVEMENT } ),
                }
            ));

            RegisterAnimation(Companion.Animation.BATHTUB_HEADPAT_HAPPY_IDLE,
                new Companion.AnimationInfo(null,
                "bathtub_headpat_happy_idle", "bathtub_headpat_happy_idle", "bathtub_headpat_happy_idle",
                Companion.Priority.LOW, BodyState.BATHING_IDLE, BodyState.BATHING_IDLE,
                0.3f, new Companion.AnimLogicInfo(3, 1.0f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.05f, (int)Companion.AnimationEventName.HEADPAT_PROPER_SOUND, null ),
                new CompanionAnimationEvent( 0.45f, (int)Companion.AnimationEventName.HEADPAT_PROPER_SOUND, null ),
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_MOVEMENT } ),
                }
            ));

            RegisterAnimation(Companion.Animation.BATHTUB_HEADAPAT_ANGRY_PROPER_TO_HAPPY,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.BATHTUB_HEADPAT_HAPPY_IDLE),
                "bathtub_headpat_angry_proper_to_happy", "bathtub_headpat_angry_proper_to_happy", "bathtub_headpat_angry_proper_to_happy",
                Companion.Priority.LOW, BodyState.BATHING_IDLE, BodyState.BATHING_IDLE,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_HEADPAT_B, 1.0f } ),
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_MOVEMENT } ),
                }
            ));


            RegisterAnimation(Companion.Animation.BATHTUB_ON_KNEES_HEADPAT_IDLE,
                new Companion.AnimationInfo(null,
                "bathtub_on_knees_headpat_idle", "bathtub_on_knees_headpat_idle", "bathtub_on_knees_headpat_idle",
                Companion.Priority.MEDIUM, BodyState.BATHING_ON_KNEES, BodyState.BATHING_ON_KNEES,
                0.3f, new Companion.AnimLogicInfo(1, 0.2f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.05f, (int)Companion.AnimationEventName.HEADPAT_PROPER_SOUND, null ),
                new CompanionAnimationEvent( 0.45f, (int)Companion.AnimationEventName.HEADPAT_PROPER_SOUND, null ),
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_MOVEMENT } ),
                }
            ));

            RegisterAnimation(Companion.Animation.BATHTUB_ON_KNEES_HEADPAT_BRUSH_AWAY_TO_ANGRY_IDLE,
                new Companion.AnimationInfo(idleTransition,
                "bathtub_on_knees_headpat_brush_away_to_angry_idle", "bathtub_on_knees_headpat_brush_away_to_angry_idle", "bathtub_on_knees_headpat_brush_away_to_angry_idle",
                Companion.Priority.HIGH, BodyState.BATHING_ON_KNEES, BodyState.BATHING_IDLE,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.3f, 0, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_LONG, 0.0f } ),
                new CompanionAnimationEvent( 0.2f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_KERPLUNK } ),
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_MOVEMENT } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HEADPAT_HAPPY_START,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_HEADPAT_HAPPY_LOOP),
                "stand_headpat_happy_start", "stand_happy_loop", "stand_headpat_happy_start",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(2, 1.0f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.05f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.DISAPPOINTED_STARTLE, 0.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HEADPAT_HAPPY_AFTER_MORE,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_HEADPAT_HAPPY_LOOP),
                "stand_headpat_happy_after_more", "stand_happy_loop", "stand_headpat_happy_after_more",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(2, 1.0f, 0.5f, 12, 0.4f),

                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.HEADPAT_RETURN, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HEADPAT_HAPPY_WANTED_MORE,
                new Companion.AnimationInfo(idleTransition,
                "stand_headpat_happy_wanted_more", "stand_happy_loop", "stand_headpat_happy_wanted_more",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(3, 1.0f, 0.5f, 6, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.DISAPPOINTED_STARTLE, 0.0f } ),
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.DISAPPOINTED_SIGH, 0.0f } ),
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 0.0f, 1.0f, 0.8f } ),
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f, 0.3f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.8f } ),
                new CompanionAnimationEvent( 0.9f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.3f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HEADPAT_HAPPY_IDLE_SAD_RIGHT,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_HEADPAT_HAPPY_LOOP),
                "stand_headpat_happy_idle_sad_right", "stand_headpat_happy_idle_sad_right", "stand_headpat_happy_idle_sad_right",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(1, 0.4f, 0.5f, 12, 0.5f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.WHINE_SOFT, 0.0f } ),
                new CompanionAnimationEvent( 0.34f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 0.0f, 1.0f, 0.6f } ),
                new CompanionAnimationEvent( 0.34f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f, 0.3f } ),
                new CompanionAnimationEvent( 0.85f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.3f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.STAND_HEADPAT_HAPPY_SATISFACTION,
                new Companion.AnimationInfo(idleTransition,
                "stand_headpat_satisfaction", "stand_headpat_satisfaction", "stand_headpat_satisfaction",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ECSTATIC, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HEADPAT_HAPPY_LOOP,
                new Companion.AnimationInfo(null,
                "stand_headpat_happy_loop", "stand_happy_loop", "stand_headpat_happy_loop",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.5f, new Companion.AnimLogicInfo(0, 0.5f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.05f, (int)Companion.AnimationEventName.HEADPAT_PROPER_SOUND, null ),
                new CompanionAnimationEvent( 0.45f, (int)Companion.AnimationEventName.HEADPAT_PROPER_SOUND, null )
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HEADPAT_ANGRY_BRUSH_AWAY,
                new Companion.AnimationInfo(idleTransition,
                "stand_headpat_angry_brush_away", "stand_angry_loop", "stand_headpat_angry_brush_away",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_LONG, 1.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HEADPAT_ANGRY_LOOP,
                new Companion.AnimationInfo(null,
                "stand_headpat_angry_start", "stand_angry_loop", "stand_headpat_angry_start",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 0.5f, 0.5f, 7, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_POUT, 0.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HEADPAT_ANGRY_END,
                new Companion.AnimationInfo(idleTransition,
                "stand_headpat_angry_end", "stand_headpat_angry_end", "stand_headpat_angry_end",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.55f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_GRUMBLE_LONG, 0.0f } ),
                new CompanionAnimationEvent( 0.46f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f, 1.1f } ),
                new CompanionAnimationEvent( 0.76f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.7f } ),
                new CompanionAnimationEvent( 0.76f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 1.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HEADPAT_CLIMAX_TO_CANCEL_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_headpat_climax_to_cancel_right", "stand_angry_loop", "stand_headpat_climax_to_cancel_right",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.15f, new Companion.AnimLogicInfo(0, 1.0f, 0.85f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_LONG, 0.0f } ),
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 1.0f } )
                }
            ), true);

            RegisterAnimation(Companion.Animation.STAND_HEADPAT_CLIMAX_TO_HAPPY,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_HEADPAT_HAPPY_LOOP),
                "stand_headpat_climax_to_happy", "stand_angry_loop", "stand_headpat_climax_to_happy",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.3f, null,
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_HEADPAT_B, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HEADPAT_INTERRUPT,
                new Companion.AnimationInfo(idleTransition,
                "stand_headpat_angry_interrupt", "stand_angry_loop", "stand_headpat_angry_interrupt",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.HUMPH, 0.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_TIRED_HEADPAT_IDLE,
                new Companion.AnimationInfo(null,
                "stand_tired_headpat_idle_loop", "stand_tired_headpat_idle_loop", "stand_tired_headpat_idle_loop",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(3, 0.4f, 1.5f, 0, 0.4f), null
            ));

            RegisterAnimation(Companion.Animation.STAND_FACE_PROX_ANGRY_SURPRISE,
                new Companion.AnimationInfo(idleTransition,
                "stand_face_prox_angry_surprise", "stand_angry_loop", "stand_face_prox_angry_surprise",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(3, 0.2f, 1.5f, 4, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.SCARED_SHORT, 0.0f } )
                }
                ));

            RegisterAnimation(Companion.Animation.STAND_FACE_PROX_ANGRY_LOOP,
                new Companion.AnimationInfo(null,
                "stand_face_prox_angry_loop", "stand_angry_loop", "stand_face_prox_angry_loop",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.25f, new Companion.AnimLogicInfo(1, 0.6f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.112f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_GRUMBLE_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.412f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_GRUMBLE_LONG, 0.0f } ),
                new CompanionAnimationEvent( 0.837f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_GRUMBLE_LONG, 0.0f } )
                },
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.STAND_KISS_ANGRY_CHEEK_RIGHT,
                new Companion.AnimationInfo(new KissingBehavior.TransitionToPostKiss(),
                "stand_kiss_angry_cheek_right", "stand_angry_loop", "stand_kiss_angry_cheek_right",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 2.0f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.112f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.KISSED_ANGRY, 1.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_KISS_ANGRY_CHEEK_LEFT,
                new Companion.AnimationInfo(new KissingBehavior.TransitionToPostKiss(),
                "stand_kiss_angry_cheek_left", "stand_angry_loop", "stand_kiss_angry_cheek_left",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 2.0f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.112f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.KISSED_ANGRY, 1.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_KISS_ANGRY_LEFT_TO_HAPPY,
                new Companion.AnimationInfo(idleTransition,
                "stand_kiss_angry_left_to_happy", "stand_angry_loop", "stand_kiss_angry_left_to_happy",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.2f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.16f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.3f } ),
                new CompanionAnimationEvent( 0.16f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 0.3f, 0.5f } ),
                new CompanionAnimationEvent( 0.26f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f, 0.3f } ),
                new CompanionAnimationEvent( 0.35f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.3f } ),
                new CompanionAnimationEvent( 0.35f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.5f } ),
                new CompanionAnimationEvent( 0.55f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f, 0.3f } ),
                new CompanionAnimationEvent( 0.52f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 0.0f, 1.0f, 0.5f } ),
                new CompanionAnimationEvent( 0.80f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.3f } ),
                new CompanionAnimationEvent( 0.80f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.7f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.RELIEF, 1.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_KISS_ANGRY_RIGHT_TO_HAPPY,
                new Companion.AnimationInfo(idleTransition,
                "stand_kiss_angry_right_to_happy", "stand_angry_loop", "stand_kiss_angry_right_to_happy",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.2f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.16f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.3f } ),
                new CompanionAnimationEvent( 0.16f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 0.3f, 0.5f } ),
                new CompanionAnimationEvent( 0.26f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f, 0.3f } ),
                new CompanionAnimationEvent( 0.35f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.3f } ),
                new CompanionAnimationEvent( 0.35f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.5f } ),
                new CompanionAnimationEvent( 0.55f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f, 0.3f } ),
                new CompanionAnimationEvent( 0.52f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 0.0f, 1.0f, 0.5f } ),
                new CompanionAnimationEvent( 0.80f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.3f } ),
                new CompanionAnimationEvent( 0.80f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.7f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.RELIEF, 1.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_FACE_PROX_HAPPY_SURPRISE,
                new Companion.AnimationInfo(idleTransition,
                "stand_face_prox_happy_surprise", "stand_happy_loop", "stand_face_prox_happy_surprise",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(3, 1.0f, 0.7f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.SCARED_SHORT, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_FACE_PROX_HAPPY_LOOP,
                new Companion.AnimationInfo(idleTransition,
                "stand_face_prox_happy_loop", "stand_happy_loop", "stand_face_prox_happy_loop",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.25f, new Companion.AnimLogicInfo(1, 0.6f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.075f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.EMBARRASSED, 1.0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                },
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.STAND_KISS_HAPPY_CHEEK_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_kiss_happy_cheek_right", "stand_kiss_happy_cheek_right", "stand_kiss_happy_cheek_right",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 1.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.KISSED_HAPPY, 1.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_KISS_HAPPY_CHEEK_LEFT,
                new Companion.AnimationInfo(idleTransition
                , "stand_kiss_happy_cheek_left", "stand_kiss_happy_cheek_left", "stand_kiss_happy_cheek_left",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 1.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.KISSED_HAPPY, 1.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_KISS_ANGRY_LEFT_TO_ANGRY,
                new Companion.AnimationInfo(idleTransition
                , "stand_kiss_angry_left_to_angry", "stand_angry_loop", "stand_kiss_angry_left_to_angry",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.25f, new Companion.AnimLogicInfo(0, 1.0f, 1.5f, 12, 0.8f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.112f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_SHORT, 1.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_KISS_ANGRY_RIGHT_TO_ANGRY,
                new Companion.AnimationInfo(idleTransition
                , "stand_kiss_angry_right_to_angry", "stand_angry_loop", "stand_kiss_angry_right_to_angry",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.25f, new Companion.AnimLogicInfo(0, 1.0f, 1.5f, 12, 0.8f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.112f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_SHORT, 1.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_REACT_PERV_FRONT_IN,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_REACT_PERV_FRONT_IDLE),
                "stand_react_perv_front_in", "stand_react_perv_front_in", "stand_react_perv_front_in",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.15f, new Companion.AnimLogicInfo(1, 0.5f, 1.5f, 12, 1.0f), null));

            RegisterAnimation(Companion.Animation.STAND_REACT_PERV_FRONT_IDLE,
                new Companion.AnimationInfo(null,
                "stand_react_perv_front_idle_loop", "stand_react_perv_front_idle_loop", "stand_react_perv_front_idle_loop",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(1, 0.5f, 1.5f, 12, 1.0f), new CompanionAnimationEvent[]{
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_REACT_PERV_FRONT_OUT,
                new Companion.AnimationInfo(idleTransition,
                "stand_react_perv_front_out", "stand_react_perv_front_out", "stand_react_perv_front_out",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(3, 0.5f, 1.5f, 7, 1.0f), null));

            RegisterAnimation(Companion.Animation.STAND_TIRED_LOCOMOTION,
                new Companion.AnimationInfo(null,
                "stand_tired_idle", "stand_tired_idle", "stand_tired_idle",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.15f, new Companion.AnimLogicInfo(3, 0.5f, 1.5f, 7, 1.0f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.BIND_ANIMATION_TO_VIEW_MODE, new float[]{ (float)Companion.AwarenessMode.TIRED } ),

                },
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.STAND_SCARED_LOCOMOTION,
                new Companion.AnimationInfo(null,
                "stand_scared_locomotion", "stand_scared_locomotion", "stand_scared_locomotion",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(0, 1.0f, 0.1f, 12, 1.0f),
                null,
                AnimationInfo.Flag.IDLE_STATE
            ));
            RegisterLegSpeedInfo("stand_scared_locomotion", 2.0f, 2.5f, 350.0f, 45.0f);

            RegisterAnimation(Companion.Animation.STAND_SCARED_STARTLE,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_SCARED_LOCOMOTION),
                "stand_scared_startled", "stand_scared_startled", "stand_scared_startled",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.15f, new Companion.AnimLogicInfo(0, 1.0f, 0.1f, 12, 1.0f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.SCARED_SHORT, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_TO_STAND_TIRED,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_TIRED_LOCOMOTION),
                "stand_to_stand_tired", "stand_to_stand_tired", "stand_to_stand_tired",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 1.0f, 0, 0.5f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.05f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.YAWN_LONG, 1.0f } ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3, 0.5f, 0.8f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_TIRED_RUB_EYES_RIGHT,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_TIRED_LOCOMOTION),
                "stand_tired_rub_eyes_right", "stand_tired_rub_eyes_right", "stand_tired_rub_eyes_right",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.6f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.YAWN_SHORT, 1.0f } ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3, 0.5f, 0.8f } ),
                },
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.STAND_TIRED_RUB_EYES_LEFT,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_TIRED_LOCOMOTION),
                "stand_tired_rub_eyes_left", "stand_tired_rub_eyes_left", "stand_tired_rub_eyes_left",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.6f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.YAWN_SHORT, 1.0f } ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3, 0.5f, 0.8f } ),
                },
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterLegSpeedInfo("stand_tired_idle", 0.95f, 2.0f, 255.0f, 30.0f);
            PokeBehavior.TransitionToPostFacePokeAnim postFacePokeAnimTransition = new PokeBehavior.TransitionToPostFacePokeAnim();
            RegisterAnimation(Companion.Animation.BATHTUB_RELAX_FACE_POKE_RIGHT,
                new Companion.AnimationInfo(postFacePokeAnimTransition,
                "bathtub_relax_face_poke_right_nm", "bathtub_relax_face_poke_right_nm", "bathtub_relax_face_poke_right_nm",
                Companion.Priority.HIGH, BodyState.BATHING_RELAX, BodyState.BATHING_RELAX,
                0.1f, new Companion.AnimLogicInfo(1, 0.4f, 0.2f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.STARTLE_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 1.0f, 0.5f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.BATHTUB_IDLE_FACE_POKE_1_RIGHT,
                new Companion.AnimationInfo(postFacePokeAnimTransition,
                "bathtub_idle_face_poke_1_right_nm", "bathtub_idle_face_poke_1_right_nm", "bathtub_idle_face_poke_1_right_nm",
                Companion.Priority.HIGH, BodyState.BATHING_IDLE, BodyState.BATHING_IDLE,
                0.15f, new Companion.AnimLogicInfo(1, 0.4f, 0.2f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.STARTLE_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 1.0f, 0.5f } ),
                }
            ), true);
            RegisterAnimation(Companion.Animation.BATHTUB_IDLE_FACE_POKE_2_RIGHT,
                new Companion.AnimationInfo(postFacePokeAnimTransition,
                "bathtub_idle_face_poke_2_right_nm", "bathtub_idle_face_poke_2_right_nm", "bathtub_idle_face_poke_2_right_nm",
                Companion.Priority.HIGH, BodyState.BATHING_IDLE, BodyState.BATHING_IDLE,
                0.15f, new Companion.AnimLogicInfo(1, 0.4f, 0.2f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.STARTLE_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 1.0f, 0.5f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.STAND_TIRED_POKE_RIGHT,
                new Companion.AnimationInfo(postFacePokeAnimTransition,
                "stand_tired_poke_right", "stand_tired_poke_right", "stand_tired_poke_right",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.1f, new Companion.AnimLogicInfo(1, 0.2f, 0.2f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.WHIMPER_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 1.0f, 1.5f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.SLEEP_PILLOW_SIDE_BOTHER_RIGHT,
                new Companion.AnimationInfo(postFacePokeAnimTransition,
                "sleep_side_pillow_bother_right", "sleep_side_pillow_bother_right", "sleep_side_pillow_bother_right",
                Companion.Priority.MEDIUM, BodyState.SLEEP_PILLOW_SIDE_RIGHT, BodyState.SLEEP_PILLOW_SIDE_RIGHT,
                0.1f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.SNORE, 0.0f } )
                }
            ), true);

            RegisterAnimation(Companion.Animation.SLEEP_PILLOW_UP_BOTHER_RIGHT,
                new Companion.AnimationInfo(postFacePokeAnimTransition,
                "sleep_pillow_up_bother_right_nm", "sleep_pillow_up_bother_right_nm", "sleep_pillow_up_bother_right_nm",
                Companion.Priority.MEDIUM, BodyState.SLEEP_PILLOW_UP, BodyState.SLEEP_PILLOW_UP,
                0.1f, new Companion.AnimLogicInfo(0, 0.5f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.SLEEP_DISTURBED_SHORT, 0.0f } )
                }
            ), true);


            RegisterAnimation(Companion.Animation.AWAKE_PILLOW_UP_FACE_POKE_RIGHT,
                new Companion.AnimationInfo(postFacePokeAnimTransition,
                "awake_pillow_up_face_poke_right_nm", "awake_pillow_up_face_poke_right_nm", "awake_pillow_up_face_poke_right_nm",
                Companion.Priority.HIGH, BodyState.AWAKE_PILLOW_UP, BodyState.AWAKE_PILLOW_UP,
                0.1f, new Companion.AnimLogicInfo(0, 0.2f, 0.2f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.STARTLE_SHORT, 0.0f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.STAND_POKE_FACE_1_RIGHT,
                new Companion.AnimationInfo(postFacePokeAnimTransition,
                "stand_poke_face_1_right", "stand_poke_face_1_right", "stand_poke_face_1_right",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.15f, new Companion.AnimLogicInfo(1, 0.4f, 0.3f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.STARTLE_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.5f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_POKE_FACE_1_LEFT,
                new Companion.AnimationInfo(postFacePokeAnimTransition,
                "stand_poke_face_1_left", "stand_poke_face_1_left", "stand_poke_face_1_left",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.15f, new Companion.AnimLogicInfo(1, 0.4f, 0.3f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.STARTLE_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.5f } ),
                }
            ));
            RegisterAnimation(Companion.Animation.STAND_POKE_FACE_2_RIGHT,
                new Companion.AnimationInfo(postFacePokeAnimTransition,
                "stand_poke_face_2_right", "stand_poke_face_2_right", "stand_poke_face_2_right",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.15f, new Companion.AnimLogicInfo(1, 0.4f, 0.3f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.STARTLE_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.7f} ),
                }
            ));
            RegisterAnimation(Companion.Animation.STAND_POKE_FACE_2_LEFT,
                new Companion.AnimationInfo(postFacePokeAnimTransition,
                "stand_poke_face_2_left", "stand_poke_face_2_left", "stand_poke_face_2_left",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.15f, new Companion.AnimLogicInfo(1, 0.4f, 0.3f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.STARTLE_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.7f} ),
                }
            ));
            RegisterAnimation(Companion.Animation.STAND_POKE_FACE_3_RIGHT,
                new Companion.AnimationInfo(postFacePokeAnimTransition,
                "stand_poke_face_3_right", "stand_poke_face_3_right", "stand_poke_face_3_right",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.15f, new Companion.AnimLogicInfo(1, 0.4f, 0.3f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.STARTLE_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.7f} ),
                }
            ));
            RegisterAnimation(Companion.Animation.STAND_POKE_FACE_3_LEFT,
                new Companion.AnimationInfo(postFacePokeAnimTransition,
                "stand_poke_face_3_left", "stand_poke_face_3_left", "stand_poke_face_3_left",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.15f, new Companion.AnimLogicInfo(1, 0.4f, 0.3f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.STARTLE_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.7f} ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_WIPE_CHEEK_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_wipe_cheek_right", "stand_wipe_cheek_right", "stand_wipe_cheek_right",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.3f, 0, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.45f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.4f } ),
                new CompanionAnimationEvent( 0.45f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.4f} ),
                new CompanionAnimationEvent( 0.45f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_GRUMBLE_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.71f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_HEADPAT_A, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_WIPE_CHEEK_LEFT,
                new Companion.AnimationInfo(idleTransition,
                "stand_wipe_cheek_left", "stand_wipe_cheek_left", "stand_wipe_cheek_left",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.3f, 0, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.45f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.4f } ),
                new CompanionAnimationEvent( 0.45f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.4f} ),
                new CompanionAnimationEvent( 0.45f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_GRUMBLE_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.71f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_HEADPAT_A, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_ANGRY_BLOCK_RIGHT,
                new Companion.AnimationInfo(idleTransition, "stand_angry_block_right", "stand_angry_block_right", "stand_angry_block_right",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.15f, new Companion.AnimLogicInfo(0, 1.0f, 0.4f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_LONG, 0.0f } ),
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.8f } ),
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 1.0f, 1.0f} ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_ANGRY_BLOCK_LEFT,
                new Companion.AnimationInfo(idleTransition, "stand_angry_block_left", "stand_angry_block_left", "stand_angry_block_left",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.15f, new Companion.AnimLogicInfo(0, 1.0f, 0.4f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_LONG, 0.0f } ),
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.8f } ),
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 1.0f, 1.0f} ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_POKED_TUMMY_IN,
                new Companion.AnimationInfo(new PokeBehavior.TransitionToPostTummyPokeAnim()
                , "stand_poked_tummy_in", "stand_poked_tummy_in", "stand_poked_tummy_in",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.25f, new Companion.AnimLogicInfo(0, 1.0f, 0.3f, 0, 0.3f),
                null
            ));

            RegisterAnimation(Companion.Animation.STAND_POKED_TUMMY_LOOP,
                new Companion.AnimationInfo(null, "stand_poked_tummy_loop", "stand_poked_tummy_loop", "stand_poked_tummy_loop",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.4f, 12, 0.4f),
                null
            ));

            RegisterAnimation(Companion.Animation.STAND_POKED_TUMMY_OUT,
                new Companion.AnimationInfo(idleTransition
                , "stand_poked_tummy_out", "stand_poked_tummy_out", "stand_poked_tummy_out",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(0, 1.0f, 0.3f, 0, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.2f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.4f } ),
                new CompanionAnimationEvent( 0.2f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 1.8f } ),
                new CompanionAnimationEvent( 0.68f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.RELIEF, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_CHASE_LOW_LOCOMOTION,
                new Companion.AnimationInfo(null, "stand_chase_low_locomotion", "stand_chase_low_locomotion", "stand_jealous_loop",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(3, 1.0f, 0.5f, 7, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.FOOTSTEP_SOUND, new float[] { 1f } ),
                }
            ));
            animationInfos[Companion.Animation.STAND_CHASE_LOW_LOCOMOTION].setTorsoSyncFloatID(Animator.StringToHash("sync_chase_low_locomotion"));
            RegisterLegSpeedInfo("stand_chase_low_locomotion", 2.6f, 3.0f, 255.0f, 30.0f);

            RegisterAnimation(Companion.Animation.STAND_MORTAR_AND_PESTLE_GRIND_LOOP_RIGHT,
                new Companion.AnimationInfo(null, "stand_mortar_and_pestle_grind_loop_right_nm", "stand_locomotion_spine_stable", "stand_happy_idle",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(3, 0.5f, 0.4f, 7, 0.4f), null,
                AnimationInfo.Flag.DISABLE_HOLDIK_BOTH
            ), true);

            RegisterAnimation(Companion.Animation.STAND_WHEAT_INTO_MORTAR_RIGHT,
                new Companion.AnimationInfo(idleTransition, "stand_wheat_into_mortar_right_nm", "stand_locomotion_spine_stable", "stand_happy_idle",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(0, 0.5f, 0.4f, 7, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.DROP_LEFT_HAND_ITEM, null ),
                },
                AnimationInfo.Flag.DISABLE_HOLDIK_BOTH
            ), true);

            RegisterAnimation(Companion.Animation.STAND_SPLASHED_START_RIGHT,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.STAND_SPLASHED_END_RIGHT),
                "stand_splashed_start_right", "stand_locomotion_spine_stable", "stand_splashed_start_right",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.25f, new Companion.AnimLogicInfo(0, 1.0f, 0.55f, 12, 1.0f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.SCREAMING, 1.0f } )
                }
            ), true);

            RegisterAnimation(Companion.Animation.STAND_SPLASHED_LOOP,
                new Companion.AnimationInfo(idleTransition, "stand_splashed_loop", "stand_locomotion_spine_stable", "stand_splashed_loop",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.25f, new Companion.AnimLogicInfo(0, 1.0f, 0.55f, 12, 1.2f),
                null
            ));

            RegisterAnimation(Companion.Animation.STAND_SPLASHED_END_RIGHT,
                new Companion.AnimationInfo(idleTransition, "stand_splashed_end_right", "stand_splashed_end_right", "stand_splashed_end_right",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.8f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.781f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 1.0f } ),
                new CompanionAnimationEvent( 0.781f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.4f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.STAND_MIXING_BOWL_MIX_LOOP_RIGHT,
                new Companion.AnimationInfo(null, "stand_mixing_bowl_mix_right_nm_loop", "stand_locomotion_spine_stable", "stand_happy_idle",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(1, 0.5f, 0.4f, 7, 0.4f), null,
                AnimationInfo.Flag.DISABLE_HOLDIK_BOTH
            ), true);

            RegisterAnimation(Companion.Animation.STAND_POUR_MORTAR_INTO_MIXING_BOWL_RIGHT,
                new Companion.AnimationInfo(idleTransition, "stand_pour_mortar_into_mixing_bowl_right_nm", "stand_locomotion_spine_stable", "stand_happy_idle",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(1, 0.5f, 0.4f, 7, 0.4f), null,
                AnimationInfo.Flag.DISABLE_HOLDIK_BOTH
            ), true);

            RegisterAnimation(Companion.Animation.STAND_EGG_INTO_MIXING_BOWL_RIGHT,
                new Companion.AnimationInfo(idleTransition, "stand_egg_into_mixing_bowl_right_nm", "stand_locomotion_spine_stable", "stand_happy_idle",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.4f, new Companion.AnimLogicInfo(1, 0.5f, 0.4f, 7, 0.4f), null,
                AnimationInfo.Flag.DISABLE_HOLDIK_BOTH
            ), true);
            RegisterAnimation(Companion.Animation.BATHTUB_SPLASH_REACT_RIGHT,
                new Companion.AnimationInfo(idleTransition, "bathtub_splash_react_right_nm", "bathtub_splash_react_right_nm", "bathtub_splash_react_right_nm",
                Companion.Priority.MEDIUM, BodyState.BATHING_IDLE, BodyState.BATHING_IDLE,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 1.0f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.STARTLE_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 1.0f, 0.7f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.BATHTUB_HAPPY_SPLASH,
                new Companion.AnimationInfo(idleTransition, "bathtub_happy_splash", "bathtub_happy_splash", "bathtub_happy_splash",
                Companion.Priority.HIGH, BodyState.BATHING_IDLE, BodyState.BATHING_IDLE,
                0.3f, new Companion.AnimLogicInfo(1, 0.5f, 0.4f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.LAUGH_SHORT, 1.0f } ),
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.BATHTUB_PLAY_SOUND, new float[]{ (float)Bathtub.SoundType.WATER_SPLASH } ),
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.BATHTUB_SPLASH, null ),
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_HEADPAT_HAPPY_START,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.FLOOR_SIT_HEADPAT_HAPPY_LOOP),
                "floor_sit_headpat_happy_start", "floor_sit_headpat_happy_start", "stand_headpat_happy_start",
                Companion.Priority.LOW, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.4f, new Companion.AnimLogicInfo(2, 1.0f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.05f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.DISAPPOINTED_STARTLE, 0.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_HEADPAT_HAPPY_WANTED_MORE,
                new Companion.AnimationInfo(idleTransition,
                "floor_sit_headpat_happy_wanted_more", "floor_sit_headpat_happy_wanted_more", "stand_headpat_happy_wanted_more",
                Companion.Priority.MEDIUM, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.3f, new Companion.AnimLogicInfo(3, 1.0f, 0.5f, 6, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.DISAPPOINTED_STARTLE, 0.0f } ),
                new CompanionAnimationEvent( 0.35f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.DISAPPOINTED_SIGH, 0.0f } ),
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 0.0f, 1.0f, 0.8f } ),
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f, 0.3f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.8f } ),
                new CompanionAnimationEvent( 0.9f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.3f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_HEADPAT_HAPPY_LOOP,
                new Companion.AnimationInfo(null,
                "floor_sit_headpat_happy_loop", "floor_sit_headpat_happy_loop", "stand_headpat_happy_loop",
                Companion.Priority.LOW, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.5f, new Companion.AnimLogicInfo(0, 0.5f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.05f, (int)Companion.AnimationEventName.HEADPAT_PROPER_SOUND, null ),
                new CompanionAnimationEvent( 0.45f, (int)Companion.AnimationEventName.HEADPAT_PROPER_SOUND, null )
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_HEADPAT_ANGRY_LOOP,
                new Companion.AnimationInfo(null,
                "floor_sit_headpat_angry_loop", "floor_sit_headpat_angry_loop", "stand_angry_idle",
                Companion.Priority.LOW, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.3f, 7, 0.3f)
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_HEADPAT_ANGRY_BRUSH_AWAY,
                new Companion.AnimationInfo(idleTransition,
                "floor_sit_headpat_angry_brush_away", "floor_sit_headpat_angry_brush_away", "stand_headpat_angry_brush_away",
                Companion.Priority.HIGH, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_LONG, 1.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_HEADPAT_ANGRY_CLIMAX_TO_HAPPY,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.FLOOR_SIT_HEADPAT_HAPPY_LOOP),
                "floor_sit_headpat_climax_to_happy", "floor_sit_headpat_climax_to_happy", "stand_headpat_climax_to_happy",
                Companion.Priority.HIGH, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.3f, null,
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_HEADPAT_B, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HUG_FAR_TO_STAND,
                new Companion.AnimationInfo(idleTransition,
                "stand_hug_far_to_stand", "stand_locomotion_spine_stable", "stand_to_stand_hug_far_happy",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 12, 0.4f)
            ));

            RegisterAnimation(Companion.Animation.STAND_TO_STAND_HUG_FAR_HAPPY,
                new Companion.AnimationInfo(new DefaultTransition(Companion.Animation.STAND_HUG_HAPPY_LOOP),
                "stand_to_stand_hug_far_happy", "stand_locomotion_spine_stable", "stand_to_stand_hug_far_happy",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.GIGGLE, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HUG_HAPPY_LOOP,
                new Companion.AnimationInfo(null,
                "stand_hug_happy_loop", "stand_locomotion_spine_stable", "stand_hug_happy_loop",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 12, 0.4f),
                null,
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.STAND_TO_STAND_HUG_FAR_ANGRY,
                new Companion.AnimationInfo(new DefaultTransition(Companion.Animation.STAND_HUG_ANGRY_LOOP),
                "stand_to_stand_hug_far_angry", "stand_locomotion_spine_stable", "stand_to_stand_hug_far_angry",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_POUT, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.STAND_HUG_ANGRY_LOOP,
                new Companion.AnimationInfo(null,
                "stand_hug_angry_loop", "stand_locomotion_spine_stable", "stand_hug_angry_loop",
                Companion.Priority.HIGH, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.4f),
                null,
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_AGREE,
                new Companion.AnimationInfo(idleTransition,
                "floor_sit_agree", "floor_sit_agree", "floor_sit_agree",
                Companion.Priority.LOW, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.3f, new Companion.AnimLogicInfo(1, 0.5f, 0.3f, 12, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.GIGGLE, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_REFUSE,
                new Companion.AnimationInfo(idleTransition,
                "floor_sit_refuse", "floor_sit_refuse", "floor_sit_refuse",
                Companion.Priority.LOW, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.3f, new Companion.AnimLogicInfo(1, 0.0f, 0.3f, 12, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.REFUSE, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_BEG_IN_RIGHT,
                new Companion.AnimationInfo(new DefaultTransition(Companion.Animation.FLOOR_SIT_BEG_LOOP_RIGHT),
                "floor_sit_beg_in_right", "floor_sit_beg_in_right", "floor_sit_beg_in_right",
                Companion.Priority.LOW, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.3f, new Companion.AnimLogicInfo(1, 0.8f, 0.4f, 7, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.FLOOR_SIT_BEG_LOOP_RIGHT,
                new Companion.AnimationInfo(new DefaultTransition(Companion.Animation.FLOOR_SIT_BEG_LOOP_RIGHT),
                "floor_sit_beg_loop_right", "floor_sit_beg_loop_right", "floor_sit_beg_loop_right",
                Companion.Priority.LOW, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.3f, new Companion.AnimLogicInfo(1, 0.8f, 0.4f, 7, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.FLOOR_SIT_BEG_TO_CARD_FAN_START_RIGHT,
                new Companion.AnimationInfo(new DefaultTransition(Companion.Animation.FLOOR_SIT_BEG_LOOP_RIGHT),
                "floor_sit_beg_to_card_fan_start_right", "floor_sit_beg_to_card_fan_start_right", "floor_sit_beg_to_card_fan_start_right",
                Companion.Priority.LOW, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.3f, new Companion.AnimLogicInfo(0, 0.8f, 0.2f, 0, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.FLOOR_SIT_REACH_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "floor_sit_reach_right", "floor_sit_reach_right", "floor_sit_reach_right",
                Companion.Priority.MEDIUM, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.3f, new Companion.AnimLogicInfo(0, 0.8f, 0.2f, 0, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 0.8f, 0.7f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.5f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.FLOOR_SIT_PLACE_CARD_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "floor_sit_place_card_right", "floor_sit_place_card_right", "floor_sit_place_card_right",
                Companion.Priority.LOW, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.3f, new Companion.AnimLogicInfo(0, 0.8f, 0.2f, 12, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.EFFORT, 0.0f } ),
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 0.8f, 0.5f } ),
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.5f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.FLOOR_SIT_PICK_CARD_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "floor_sit_pick_card_right", "floor_sit_pick_card_right", "floor_sit_pick_card_right",
                Companion.Priority.LOW, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.3f, new Companion.AnimLogicInfo(1, 0.8f, 0.2f, 12, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.BLEND_CONTROLLER_RIGHT, new float[]{ 0.0f } ),
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.EXECUTE_PICK_CARD_RIGHT, null ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.STAND_IMPRESSED1,
                new Companion.AnimationInfo(idleTransition,
                "stand_impressed1", "stand_impressed1", "stand_impressed1",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(1, 0.8f, 0.2f, 12, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.IMPRESSED_VERY, 1.0f } ),
                new CompanionAnimationEvent( 0.2f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.26f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.34f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.42f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.50f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.58f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.66f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.74f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_SIT_IMPRESSED1,
                new Companion.AnimationInfo(idleTransition,
                "floor_sit_impressed1", "floor_sit_impressed1", "stand_impressed1",
                Companion.Priority.LOW, BodyState.FLOOR_SIT, BodyState.FLOOR_SIT,
                0.3f, new Companion.AnimLogicInfo(1, 0.8f, 0.2f, 12, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.IMPRESSED_VERY, 1.0f } ),
                new CompanionAnimationEvent( 0.2f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.26f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.34f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.42f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.50f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.58f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.66f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.74f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                }
            ));

            RegisterAnimation(Companion.Animation.PICKED_UP,
                new Companion.AnimationInfo(null,
                "picked_up_loop", "picked_up_loop", "picked_up_loop",
                Companion.Priority.LOW, BodyState.OFFBALANCE, BodyState.OFFBALANCE,
                0.3f, new Companion.AnimLogicInfo(1, 1.0f, 0.5f, 12, 0.4f),
                null
            ));

            RegisterAnimation(Companion.Animation.FALLING_LOOP,
                new Companion.AnimationInfo(null,
                "falling_loop", "falling_loop", "falling_loop",
                Companion.Priority.LOW, BodyState.OFFBALANCE, BodyState.OFFBALANCE,
                0.3f, new Companion.AnimLogicInfo(0, 0.6f, 0.5f, 12, 0.4f),
                null
            ));

            RegisterAnimation(Companion.Animation.FLOOR_CURL_LOOP,
                new Companion.AnimationInfo(null,
                "floor_curl_loop", "floor_curl_loop", "floor_curl_loop",
                Companion.Priority.LOW, BodyState.OFFBALANCE, BodyState.OFFBALANCE,
                0.35f, new Companion.AnimLogicInfo(1, 0.6f, 0.5f, 0, 0.4f),
                null
            ));

            RegisterAnimation(Companion.Animation.FLOOR_FACE_DOWN_TO_STAND,
                new Companion.AnimationInfo(idleTransition,
                "floor_face_down_to_stand", "floor_face_down_to_stand", "floor_face_down_to_stand",
                Companion.Priority.LOW, BodyState.OFFBALANCE, BodyState.STAND,
                0.0f, new Companion.AnimLogicInfo(0, 0.6f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.2f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 0.8f, 0.5f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 7.0f, 0.5f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_FACE_UP_TO_STAND,
                new Companion.AnimationInfo(idleTransition,
                "floor_face_up_to_stand", "floor_face_up_to_stand", "floor_face_up_to_stand",
                Companion.Priority.LOW, BodyState.OFFBALANCE, BodyState.STAND,
                0.0f, new Companion.AnimLogicInfo(0, 0.6f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.2f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 0.8f, 0.5f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 7.0f, 0.5f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_FACE_UP_IDLE,
                new Companion.AnimationInfo(idleTransition,
                "floor_face_up_idle", "floor_face_up_idle", "floor_face_up_idle",
                Companion.Priority.LOW, BodyState.OFFBALANCE, BodyState.OFFBALANCE,
                0.4f, new Companion.AnimLogicInfo(0, 0.6f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.2f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.FLOOR_FACE_SIDE_TO_STAND_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "floor_face_side_to_stand_right", "floor_face_side_to_stand_right", "floor_face_side_to_stand_right",
                Companion.Priority.LOW, BodyState.OFFBALANCE, BodyState.STAND,
                0.0f, new Companion.AnimLogicInfo(0, 0.6f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                new CompanionAnimationEvent( 0.85f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 0.8f, 0.5f } ),
                new CompanionAnimationEvent( 0.85f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 7.0f, 0.5f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.STAND_MERCHANT_IDLE1,
                new Companion.AnimationInfo(idleTransition,
                "stand_merchant_idle1", "stand_merchant_idle1", "stand_merchant_idle1",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(1, 0.5f, 0.3f, 0, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 0.0f, 1.0f, 0.5f } ),
                new CompanionAnimationEvent( 0.5f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 7.0f, 0.5f } ),
                new CompanionAnimationEvent( 1.0f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.5f } ),
                new CompanionAnimationEvent( 1.0f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 7.0f, 0.5f } ),
                }
            ));
            RegisterAnimation(Companion.Animation.STAND_BOW,
                new Companion.AnimationInfo(idleTransition,
                "stand_bow", "stand_bow", "stand_bow",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 0.3f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                new CompanionAnimationEvent( 0.7f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.4f } ),
                new CompanionAnimationEvent( 0.6f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.4f } ),
                }
            ));
            RegisterAnimation(Companion.Animation.STAND_FOLLOW_ME_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "stand_follow_me_right", "stand_follow_me_right", "stand_follow_me_right",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(0, 0.3f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ECSTATIC, 0.0f } ),
                }
            ));
            RegisterAnimation(Companion.Animation.STAND_SLIDINGDOOR_RIGHT,
                new Companion.AnimationInfo(new DefaultTransition(Companion.Animation.STAND_SLIDINGDOOR_IDLE_LOOP_RIGHT),
                "stand_slidingDoor_right", "stand_slidingDoor_right", "stand_happy_idle",
                Companion.Priority.MEDIUM, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(1, 0.7f, 0.3f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.STAND_SLIDINGDOOR_IDLE_LOOP_RIGHT,
                new Companion.AnimationInfo(null,
                "stand_slidingDoor_idle_right_loop", "stand_locomotion_spine_stable", "stand_happy_idle",
                Companion.Priority.LOW, BodyState.STAND, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(1, 0.0f, 0.3f, 7, 0.4f)
            ), true);

            RegisterAnimation(Companion.Animation.SQUAT_LOCOMOTION_HAPPY,
                new Companion.AnimationInfo(null,
                "squat_locomotion_happy", "squat_locomotion_happy", "stand_happy_idle",
                Companion.Priority.LOW, BodyState.SQUAT, BodyState.SQUAT,
                0.3f, new Companion.AnimLogicInfo(1, 1.0f, 0.3f, 7, 0.4f),
                null,
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.SQUAT_LOCOMOTION_ANGRY,
                new Companion.AnimationInfo(null,
                "squat_locomotion_happy", "squat_locomotion_happy", "stand_angry_idle",
                Companion.Priority.LOW, BodyState.SQUAT, BodyState.SQUAT,
                0.3f, new Companion.AnimLogicInfo(1, 1.0f, 0.3f, 7, 0.4f),
                null,
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterLegSpeedInfo("squat_locomotion_happy", 0.75f, 0.3f, 100.0f, 20.0f);

            RegisterAnimation(Companion.Animation.STAND_TO_SQUAT,
                new Companion.AnimationInfo(idleTransition,
                "stand_to_squat", "stand_to_squat", "stand_happy_idle",
                Companion.Priority.LOW, BodyState.STAND, BodyState.SQUAT,
                0.3f, new Companion.AnimLogicInfo(1, 0.3f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.SQUAT_TO_RELAX,
                new Companion.AnimationInfo(idleTransition,
                "squat_to_relax", "squat_to_relax", "squat_to_relax",
                Companion.Priority.LOW, BodyState.SQUAT, BodyState.RELAX,
                0.3f, new Companion.AnimLogicInfo(1, 0.3f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.RELIEF, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.RELAX_IDLE_LOOP,
                new Companion.AnimationInfo(null,
                "relax_idle_loop", "relax_idle_loop", "relax_idle_loop",
                Companion.Priority.LOW, BodyState.RELAX, BodyState.RELAX,
                0.3f, new Companion.AnimLogicInfo(1, 0.3f, 0.3f, 12, 0.4f),
                null,
                AnimationInfo.Flag.IDLE_STATE
            ));

            RegisterAnimation(Companion.Animation.RELAX_TO_SQUAT,
                new Companion.AnimationInfo(idleTransition,
                "relax_to_squat", "relax_to_squat", "relax_to_squat",
                Companion.Priority.LOW, BodyState.RELAX, BodyState.SQUAT,
                0.3f, new Companion.AnimLogicInfo(0, 0.3f, 0.3f, 12, 0.4f)
            ));

            RegisterAnimation(Companion.Animation.RELAX_TO_SQUAT_STARTLE,
                new Companion.AnimationInfo(idleTransition,
                "relax_to_squat_startle", "relax_to_squat_startle", "relax_to_squat_startle",
                Companion.Priority.MEDIUM, BodyState.RELAX, BodyState.SQUAT,
                0.3f, new Companion.AnimLogicInfo(1, 0.8f, 0.3f, 0, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.SCARED_SHORT, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.SQUAT_TO_STAND,
                new Companion.AnimationInfo(idleTransition,
                "squat_to_stand", "squat_to_stand", "stand_happy_idle",
                Companion.Priority.LOW, BodyState.SQUAT, BodyState.STAND,
                0.3f, new Companion.AnimLogicInfo(1, 0.3f, 0.3f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.MISC_IDLE, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.SQUAT_HEADPAT_HAPPY_START,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.SQUAT_HEADPAT_HAPPY_LOOP),
                "squat_headpat_happy_start", "squat_headpat_happy_start", "squat_headpat_happy_start",
                Companion.Priority.MEDIUM, BodyState.SQUAT, BodyState.SQUAT,
                0.4f, new Companion.AnimLogicInfo(2, 1.0f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.05f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.DISAPPOINTED_STARTLE, 0.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.SQUAT_HEADPAT_HAPPY_WANTED_MORE,
                new Companion.AnimationInfo(idleTransition,
                "squat_headpat_happy_wanted_more", "squat_headpat_happy_wanted_more", "squat_headpat_happy_wanted_more",
                Companion.Priority.MEDIUM, BodyState.SQUAT, BodyState.SQUAT,
                0.3f, new Companion.AnimLogicInfo(3, 1.0f, 0.5f, 6, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.DISAPPOINTED_STARTLE, 0.0f } ),
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.DISAPPOINTED_SIGH, 0.0f } ),
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 0.0f, 1.0f, 0.8f } ),
                new CompanionAnimationEvent( 0.3f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f, 0.3f } ),
                new CompanionAnimationEvent( 0.8f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 3.0f, 1.0f, 0.8f } ),
                new CompanionAnimationEvent( 0.9f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.3f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.SQUAT_HEADPAT_HAPPY_IDLE_SAD_RIGHT,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.SQUAT_HEADPAT_HAPPY_LOOP),
                "squat_headpat_happy_idle_sad_right", "squat_headpat_happy_idle_sad_right", "squat_headpat_happy_idle_sad_right",
                Companion.Priority.MEDIUM, BodyState.SQUAT, BodyState.SQUAT,
                0.3f, new Companion.AnimLogicInfo(1, 0.4f, 0.5f, 12, 0.5f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.WHINE_SOFT, 0.0f } ),
                new CompanionAnimationEvent( 0.34f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 0.0f, 1.0f, 0.6f } ),
                new CompanionAnimationEvent( 0.34f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 0.0f, 0.3f } ),
                new CompanionAnimationEvent( 0.85f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 0.3f } ),
                }
            ), true);

            // RegisterAnimation(Companion.Animation.SQUAT_HEADPAT_HAPPY_SATISFACTION,
            // 	new Companion.AnimationInfo(idleTransition,
            // 	"squat_headpat_satisfaction","squat_headpat_satisfaction","squat_headpat_satisfaction",
            // 	Companion.Priority.MEDIUM,BodyState.SQUAT,BodyState.SQUAT,
            // 	0.3f,new Companion.AnimLogicInfo(0,1.0f,0.5f,0,0.4f),
            // 	new CompanionAnimationEvent[]{
            // 		new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ECSTATIC, 0.0f } ),
            // 	}
            // ));

            RegisterAnimation(Companion.Animation.SQUAT_HEADPAT_HAPPY_LOOP,
                new Companion.AnimationInfo(null,
                "squat_headpat_happy_loop", "squat_headpat_happy_loop", "squat_headpat_happy_loop",
                Companion.Priority.LOW, BodyState.SQUAT, BodyState.SQUAT,
                0.5f, new Companion.AnimLogicInfo(0, 0.5f, 0.5f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.05f, (int)Companion.AnimationEventName.HEADPAT_PROPER_SOUND, null ),
                new CompanionAnimationEvent( 0.45f, (int)Companion.AnimationEventName.HEADPAT_PROPER_SOUND, null )
                }
            ));

            RegisterAnimation(Companion.Animation.SQUAT_HEADPAT_ANGRY_BRUSH_AWAY,
                new Companion.AnimationInfo(idleTransition,
                "squat_headpat_angry_brush_away", "squat_headpat_angry_brush_away", "stand_headpat_angry_brush_away",
                Companion.Priority.HIGH, BodyState.SQUAT, BodyState.SQUAT,
                0.2f, new Companion.AnimLogicInfo(0, 1.0f, 0.5f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_LONG, 1.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.SQUAT_HEADPAT_ANGRY_START,
                new Companion.AnimationInfo(null,
                "squat_headpat_angry_start", "squat_headpat_angry_start", "squat_headpat_angry_start",
                Companion.Priority.MEDIUM, BodyState.SQUAT, BodyState.SQUAT,
                0.2f, new Companion.AnimLogicInfo(0, 0.5f, 0.5f, 7, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_POUT, 0.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.SQUAT_HEADPAT_ANGRY_LOOP,
                new Companion.AnimationInfo(null,
                "squat_headpat_angry_loop", "squat_headpat_angry_loop", "squat_headpat_angry_loop",
                Companion.Priority.MEDIUM, BodyState.SQUAT, BodyState.SQUAT,
                0.2f, new Companion.AnimLogicInfo(0, 0.5f, 0.5f, 7, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_POUT, 0.0f } )
                }
            ));

            RegisterAnimation(Companion.Animation.SQUAT_HEADPAT_CLIMAX_TO_CANCEL_RIGHT,
                new Companion.AnimationInfo(idleTransition,
                "squat_headpat_climax_to_cancel_right", "squat_headpat_climax_to_cancel_right", "stand_headpat_climax_to_cancel_right",
                Companion.Priority.HIGH, BodyState.SQUAT, BodyState.SQUAT,
                0.15f, new Companion.AnimLogicInfo(0, 1.0f, 0.85f, 0, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_LONG, 0.0f } ),
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SET_EYE_FLAGS, new float[]{ 12.0f, 1.0f } )
                }
            ), true);

            RegisterAnimation(Companion.Animation.SQUAT_HEADPAT_CLIMAX_TO_HAPPY,
                new Companion.AnimationInfo(new Companion.DefaultTransition(Companion.Animation.SQUAT_HEADPAT_HAPPY_LOOP),
                "squat_headpat_climax_to_happy", "squat_headpat_climax_to_happy", "stand_headpat_climax_to_happy",
                Companion.Priority.HIGH, BodyState.SQUAT, BodyState.SQUAT,
                0.3f, null,
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.1f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.ANGRY_HEADPAT_B, 0.0f } ),
                }
            ));

            RegisterAnimation(Companion.Animation.SQUAT_FACE_POKE_1_RIGHT,
                new Companion.AnimationInfo(postFacePokeAnimTransition,
                "squat_poke_face_1_right", "squat_poke_face_1_right", "stand_poke_face_1_right",
                Companion.Priority.HIGH, BodyState.SQUAT, BodyState.SQUAT,
                0.15f, new Companion.AnimLogicInfo(1, 0.4f, 0.2f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.STARTLE_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 1.0f, 0.5f } ),
                }
            ), true);
            RegisterAnimation(Companion.Animation.SQUAT_FACE_POKE_2_RIGHT,
                new Companion.AnimationInfo(postFacePokeAnimTransition,
                "squat_poke_face_2_right", "squat_poke_face_2_right", "stand_poke_face_2_right",
                Companion.Priority.HIGH, BodyState.SQUAT, BodyState.SQUAT,
                0.15f, new Companion.AnimLogicInfo(1, 0.4f, 0.2f, 12, 0.4f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.STARTLE_SHORT, 0.0f } ),
                new CompanionAnimationEvent( 0.4f, (int)Companion.AnimationEventName.SET_BODY_FLAGS, new float[]{ 1.0f, 1.0f, 0.5f } ),
                }
            ), true);

            RegisterAnimation(Companion.Animation.SQUAT_IMPRESSED1,
                new Companion.AnimationInfo(idleTransition,
                "squat_impressed1", "squat_impressed1", "stand_impressed1",
                Companion.Priority.LOW, BodyState.SQUAT, BodyState.SQUAT,
                0.3f, new Companion.AnimLogicInfo(1, 0.8f, 0.2f, 12, 0.3f),
                new CompanionAnimationEvent[]{
                new CompanionAnimationEvent( 0.0f, (int)Companion.AnimationEventName.SPEAK, new float[]{ (float)Companion.VoiceLine.IMPRESSED_VERY, 1.0f } ),
                new CompanionAnimationEvent( 0.2f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.26f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.34f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.42f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.50f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.58f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.66f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                new CompanionAnimationEvent( 0.74f, (int)Companion.AnimationEventName.PLAY_CLAP_SOUND, null ),
                }
            ));
        }
    }

}