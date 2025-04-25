using UnityEngine;

namespace Viva
{

    public class FireworkExplosion : MonoBehaviour
    {

        [Range(10, 30)]
        [SerializeField]
        private float lifespan = 30;
        [SerializeField]
        private ParticleSystem fireworksPSys;
        [SerializeField]
        private ParticleSystem fireworksSmokeTrailPSys;
        [SerializeField]
        private MeshRenderer flashMR;
        [Range(0, 0.4f)]
        [SerializeField]
        private float flashDuration = 0.4f;
        [SerializeField]
        private SoundSet fireworkBooms;
        [SerializeField]
        private Light environmentLight;
        [Range(1, 4f)]
        [SerializeField]
        private float lightDuration = 2.0f;

        private float timeStart = 0.0f;
        private static readonly int colorID = Shader.PropertyToID("_Color");
        private static readonly int sizeID = Shader.PropertyToID("_Size");


        public void SetFireworkColor(Color color)
        {
            flashMR.material.SetColor(colorID, color);
            var main = fireworksPSys.main;
            main.startColor = color;
            environmentLight.color = color;
        }

        public void LateUpdate()
        {
            float flashAnim = (Time.time - timeStart) / flashDuration;
            if (flashAnim > 1.0f)
            {
                flashMR.enabled = false;
            }
            else
            {
                flashMR.material.SetFloat(sizeID, Mathf.LerpUnclamped(0.0f, 4.0f, Mathf.Clamp01(flashAnim)));
            }

            float lightAnim = (Time.time - timeStart) / lightDuration;
            if (lightAnim > 1.0f)
            {
                environmentLight.enabled = false;
            }
            else
            {
                environmentLight.intensity = Mathf.Pow(1.0f - lightAnim, 3) * 10.0f;
            }
        }

        private void Awake()
        {
            timeStart = Time.time;

            var seed = (uint)(1000 * Random.value);
            fireworksPSys.randomSeed = seed;
            fireworksSmokeTrailPSys.randomSeed = seed;

            fireworksPSys.gameObject.SetActive(true);
            fireworksSmokeTrailPSys.gameObject.SetActive(true);

            var handle = SoundManager.main.RequestHandle(transform.position);
            handle.maxDistance = 200.0f;
            float delay = Vector3.Distance(GameDirector.instance.mainCamera.transform.position, transform.position) / 343;
            handle.PlayDelayed(fireworkBooms.GetRandomAudioClip(), delay);

            SetFireworkColor(Random.ColorHSV(0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f));

            Destroy(this, lifespan);

            AlertLocalCompanions();
        }

        private void OnDestroy()
        {
            foreach (Character character in GameDirector.characters.objects)
            {
                Companion companion = character as Companion;
                if (companion == null)
                {
                    continue;
                }
                companion.autonomy.RemoveFromQueue("fireworks impressed");

            }
        }

        private void AlertLocalCompanions()
        {

            foreach (Character character in GameDirector.characters.objects)
            {
                Companion companion = character as Companion;
                if (companion == null)
                {
                    continue;
                }
                var diff = character.floorPos - transform.position;

                float sqDist = diff.x * diff.x + diff.z * diff.z;
                if (sqDist < 50)
                { //10
                    companion.passive.scared.Scare(4.0f);
                    //Debug.LogError(sqDist);
                }
                else if (sqDist < 62500)
                { //250
                    var impressedAnim = Companion.Animation.NONE;
                    switch (companion.bodyState)
                    {
                        case BodyState.STAND:
                            impressedAnim = Companion.Animation.STAND_IMPRESSED1;
                            break;
                        case BodyState.FLOOR_SIT:
                            impressedAnim = Companion.Animation.FLOOR_SIT_IMPRESSED1;
                            break;
                        case BodyState.SQUAT:
                        case BodyState.RELAX:
                            impressedAnim = Companion.Animation.SQUAT_IMPRESSED1;
                            break;
                    }
                    if (impressedAnim != Companion.Animation.NONE)
                    {

                        Vector3 toFirework = transform.position - companion.head.position;
                        if (Physics.Raycast(companion.head.position, toFirework.normalized, 8.0f, WorldUtil.wallsMask, QueryTriggerInteraction.Ignore))
                        {
                            continue;
                        }
                        var playImpressed = new AutonomyPlayAnimation(companion.autonomy, "fireworks impressed", impressedAnim);

                        playImpressed.AddRequirement(new AutonomyFaceDirection(companion.autonomy, "face firework", delegate (TaskTarget target)
                        {
                            if(transform.position != null)
                            {
                                target.SetTargetPosition(transform.position);
                            }
                            else
                            {
                                playImpressed.FlagForFailure();
                            }
                            
                        }));
                        playImpressed.AddRequirement(new AutonomyWait(companion.autonomy, "random wait", Random.value));

                        companion.autonomy.Interrupt(playImpressed);

                        companion.SetLookAtTarget(transform);
                        companion.SetViewAwarenessTimeout(4.0f);
                    }
                }
            }
        }
    }

}