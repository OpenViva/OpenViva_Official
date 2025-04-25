using System.Collections;
using OccaSoftware.Altos.Runtime;
using UnityEngine;
using Viva.Util;


namespace Viva
{

    public class OnsenGhostMiniGame : MonoBehaviour
    {

        [SerializeField]
        private OnsenGhostBloodDoor[] bloodDoors;
        [SerializeField]
        private DayNightCycle.Phase spookySkyPhase;
        [SerializeField] 
        private PeriodOfDay spookySkyPeriod;
        [SerializeField]
        private Material skyboxMaterial;
        [SerializeField]
        private AudioClip suspenseSong;
        [SerializeField]
        private SoundSet doorPoundSounds;
        [SerializeField]
        private ManekiNeko nekoTimer;
        [SerializeField]
        private Transform repositionNekoTransform;
        [SerializeField]
        private Material ghostNearbyMat;
        [SerializeField]
        private OnsenGhost ghost;
        [SerializeField]
        private AudioSource ghostNearbyGlobal;
        [SerializeField]
        private float timeTillGhost = 10.0f;
        [SerializeField]
        private GameObject blockPath;
        [SerializeField]
        private Transform playerRanAwayGhostReposition;
        [Range(0.0f, 0.25f)]
        [SerializeField]
        public float normalGhostSpeed = 0.01f;
        [Range(0.0f, 3.0f)]
        [SerializeField]
        public float fastGhostSpeed = 0.15f;
        [SerializeField]
        private AudioClip ghostTimeOutClip;
        [SerializeField]
        private AudioClip winGame;

        private static readonly MeshRenderer[] dummyMRs = new MeshRenderer[0];
        private int alphaID = Shader.PropertyToID("_Alpha");
        private int distortionID = Shader.PropertyToID("_Distortion");
        private float timeStarted = 0.0f;
        private bool playedTimeOutSound = false;
        private bool checkedFinalState = false;
        private bool alternateWin = false;


        private void OnEnable()
        {
            GameDirector.newSkyDirector.SetSkyOverride(spookySkyPeriod);
            //GameDirector.newSkyDirector.enabled = false;
            GameDirector.newSkyDirector.Sun.GetLight().intensity = 0f;
            GameDirector.instance.SetMusic(GameDirector.Music.SUSPENSE, 1.0f);
            GameDirector.instance.LockMusic(true);

            nekoTimer.transform.position = repositionNekoTransform.position;
            nekoTimer.transform.rotation = repositionNekoTransform.rotation;
            nekoTimer.SetLookMode(false);
            playerRanAwayGhostReposition.gameObject.SetActive(true);

            foreach (var bloodDoor in bloodDoors)
            {
                bloodDoor.CheckClosedState();
            }
            GameDirector.instance.postProcessing.GhostScreen.SetActive(true);
            ghostNearbyMat.SetFloat(alphaID, 0f);
            ghostNearbyMat.SetFloat(distortionID, 0f);
            timeStarted = Time.time;

            blockPath.SetActive(true);
            ghost.currentSpeed = normalGhostSpeed;
            playedTimeOutSound = false;
            alternateWin = false;

            //scare all companions
            var companions = GameDirector.instance.FindCharactersInSphere((int)Character.Type.COMPANION, transform.position, 30.0f);
            foreach (var companion in companions)
            {
                (companion as Companion).passive.scared.Scare(30.0f);
            }
        }

        private void OnDisable()
        {
            GameDirector.newSkyDirector.SetSkyOverride(null);
            GameDirector.instance.LockMusic(false);
            GameDirector.instance.SetMusic(GameDirector.instance.GetDefaultMusic(), 3.0f);
            
            GameDirector.skyDirector.SetSkyMaterial(null);    //restore
            GameDirector.instance.postProcessing.GhostScreen.SetActive(false);
            GameDirector.skyDirector.enabled = true;
            nekoTimer.SetLookMode(true);
            nekoTimer.clockSpeed = 1.0f;

            ghostNearbyGlobal.Stop();

            blockPath.SetActive(false);
            StopAllCoroutines();

            foreach (var bloodDoor in bloodDoors)
            {
                bloodDoor.SetEnableBlood(false);
            }
        }

        public void PlayerRanAway()
        {
            if (playerRanAwayGhostReposition.gameObject.activeSelf)
            {
                playerRanAwayGhostReposition.gameObject.SetActive(false);
                timeStarted = Time.time - timeTillGhost;
                ghost.transform.position = playerRanAwayGhostReposition.position;
                ghost.currentSpeed = fastGhostSpeed;
            }
        }

        public void PlayerTouchedGhost()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        public void AlternateWin()
        {
            alternateWin = true;
            gameObject.SetActive(false);
        }

        public void FixedUpdate()
        {
            if (ghost.gameObject.activeSelf)
            {
                float dist = Vector3.Distance(ghost.transform.position, GameDirector.instance.mainCamera.transform.position);

                float distClamp = Tools.GetClampedRatio(2f, 7.0f, dist);
                float invertedDistClamp = 1.0f - distClamp;

                float curve = invertedDistClamp * invertedDistClamp;

                float alpha = curve * 0.95f;
                float distortion = curve * 0.3f;
                Debug.Log("Closeby var: " + alpha);
                ghostNearbyMat.SetFloat(alphaID, alpha);
                ghostNearbyMat.SetFloat(distortionID, distortion);
                if (alpha < 1.0f)
                {
                    if (!ghostNearbyGlobal.isPlaying)
                    {
                        ghostNearbyGlobal.Play();
                    }
                    ghostNearbyGlobal.volume = Mathf.Pow(1.0f - Tools.GetClampedRatio(0.0f, 8.0f, dist), 2.0f);
                }
                else
                {
                    ghostNearbyGlobal.Stop();
                }
            }
            if (Time.time - timeStarted > timeTillGhost - 13.0f)
            {
                if (!playedTimeOutSound)
                {
                    playedTimeOutSound = true;
                    ghostNearbyGlobal.PlayOneShot(ghostTimeOutClip);
                    nekoTimer.clockSpeed = 2.0f;
                }
            }
            if (Time.time - timeStarted > timeTillGhost)
            {
                Debug.Log("[OnsenGhost] Checking final ghost state...");
                if (nekoTimer.clockSpeed > 1.0f)
                {
                    nekoTimer.clockSpeed = 0.0f;

                    OnsenGhostBloodDoor failedDoor = null;
                    foreach (var bloodDoor in bloodDoors)
                    {
                        if (bloodDoor.hasAGap)
                        {
                            failedDoor = bloodDoor;
                            ghost.transform.position = bloodDoor.transform.position;
                            break;
                        }
                    }
                    if (failedDoor == null)
                    {
                        gameObject.SetActive(false);
                        GameDirector.instance.PlayGlobalSound(winGame);
                        ghostNearbyGlobal.Stop();
                    }
                    else
                    {
                        ghost.gameObject.SetActive(true);
                    }
                }
            }
        }

        public void PlayDelayedDoorBangScare(Transform doorTransform)
        {
            StartCoroutine(DoorBangScare(doorTransform));
        }

        private IEnumerator DoorBangScare(Transform doorTransform)
        {

            yield return new WaitForSeconds(Random.value * 5.0f);
            int doorBangs = (int)(Random.value * 4.0f);
            while (doorBangs-- > 0)
            {
                var handle = SoundManager.main.RequestHandle(Vector3.up, doorTransform);
                handle.pitch = 0.7f + Random.value * 0.4f;
                handle.maxDistance = 10.0f;
                handle.PlayOneShot(doorPoundSounds.GetRandomAudioClip());
                yield return new WaitForSeconds(0.4f + Random.value * 0.5f);
            }
        }
    }

}