using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Viva.Util;

namespace Viva
{


    public partial class PauseMenu : UIMenu
    {


        private bool m_finishedDisplayHint = true;
        public bool finishedDisplayedHint { get { return m_finishedDisplayHint; } }

        [SerializeField]
        private GameObject tutorialDuckPrefab = null;

        [SerializeField]
        private GameObject tutorialFramePrefab = null;
        
        [SerializeField]
        private RectTransform HUD_canvas;

        [SerializeField]
        private GameObject tutorialCircle = null;

  

        [SerializeField]
        private GameObject achievementPrefab;

        GameObject duck;

        Item frame;

        public enum HintType
        {
            ACHIEVEMENT,
            HINT,
            HINT_NO_IMAGE
        }

        [SerializeField]
        private Sprite[] hintTypeFrames = new Sprite[System.Enum.GetValues(typeof(HintType)).Length];
        [SerializeField]
        private Sprite[] hintTypeImages = new Sprite[System.Enum.GetValues(typeof(HintType)).Length];

        [SerializeField]
        private AudioClip menuSound;

        [SerializeField]
        private AudioClip achievementSound;

        private int achievementPanelsActive = 0;
        

        //TODO: Move everything to do with Hints and Achievement Messages to its own class (GameDirector.Hud)
        public void DisplayHUDMessage(string message, bool playsound, HintType hintType, MenuTutorial waitForPhase = MenuTutorial.NONE)
        {
            GameDirector.instance.StartCoroutine(HandleHUDMessage(message, playsound, hintType, waitForPhase));
        }

        private void OrientHUDToPlayer()
        {

            if (GameDirector.player.controls == Player.ControlType.KEYBOARD)
            {
                HUD_canvas.transform.localScale = Vector3.one * 0.001f;
                HUD_canvas.transform.position = GameDirector.instance.mainCamera.transform.position + GameDirector.instance.mainCamera.transform.forward * 0.8f;
                HUD_canvas.transform.rotation = Quaternion.LookRotation(GameDirector.instance.mainCamera.transform.forward, GameDirector.instance.mainCamera.transform.up);
            }
            else
            {
                HUD_canvas.transform.localScale = Vector3.one * 0.002f;
                Vector3 floorForward = GameDirector.instance.mainCamera.transform.forward;
                floorForward.y = 0.001f;
                floorForward = floorForward.normalized;

                HUD_canvas.transform.position = GameDirector.player.head.position + floorForward * 1.5f - Vector3.up * 0.5f;
                HUD_canvas.transform.rotation = Quaternion.LookRotation(floorForward, Vector3.up);
            }
        }

        private void EnsureVisibleHUD()
        {

            //stick HUD close to player if he moves far
            Player player = GameDirector.player;
            if (player.controls == Player.ControlType.KEYBOARD)
            {
                OrientHUDToPlayer();
            }
            else
            {  //if VR
                if ((HUD_canvas.position - player.head.position).sqrMagnitude > 6.0f)
                {
                    OrientHUDToPlayer();
                }
                else if (Mathf.Abs(Tools.Bearing(player.head, HUD_canvas.position)) > 40.0f)
                {
                    OrientHUDToPlayer();
                }
            }
        }

        public IEnumerator HandleHUDMessage(string message, bool playsound, HintType hintType, MenuTutorial waitForPhase)
        {
            m_finishedDisplayHint = false;

            GameObject achievementPanel = Instantiate(achievementPrefab, Vector3.zero, HUD_canvas.rotation, HUD_canvas);

            Vector3 targetPos = new Vector3(0.0f, achievementPanelsActive++ * -40.0f + 360.0f, 0.0f);
            if (playsound)
            {
                if (hintType == HintType.ACHIEVEMENT)
                    GameDirector.instance.PlayGlobalSound(achievementSound);
                else if (hintType == HintType.HINT)
                    GameDirector.instance.PlayGlobalSound(menuSound);
            }

            Image image = achievementPanel.GetComponent(typeof(Image)) as Image;
            image.sprite = hintTypeFrames[(int)hintType];
            Text text = achievementPanel.transform.GetChild(0).GetComponent(typeof(Text)) as Text;
            text.text = message;

            Image hintImage = achievementPanel.transform.GetChild(1).GetComponent(typeof(Image)) as Image;
            hintImage.sprite = hintTypeImages[(int)hintType];

            Color fade = Color.white;
            Color textFade = Color.black;

            float timer = 0.0f;
            while (timer < 0.4f)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Min(1.0f, timer / 0.4f);
                achievementPanel.transform.localPosition = Vector3.LerpUnclamped(Vector3.zero, targetPos, Tools.EaseOutQuad(alpha));
                achievementPanel.transform.localScale = Vector3.one * Mathf.Lerp(1.0f, 2.0f, 1.0f - Mathf.Abs(0.5f - alpha) * 2.0f);

                fade.a = alpha;
                image.color = fade;
                hintImage.color = fade;
                textFade.a = alpha;
                text.color = textFade;
                EnsureVisibleHUD();
                yield return null;
            }

            if (waitForPhase != MenuTutorial.NONE)
            {
                while (true)
                {

                    if (Tutorial.instance.GetMenuTutorialPhase() >= waitForPhase)
                    {
                        break;
                    }
                    EnsureVisibleHUD();
                    yield return null;
                }
            }
            else
            {
                float length = 2.0f + message.Length * 0.06f;
                while (timer < length)
                {
                    timer += Time.deltaTime;
                    EnsureVisibleHUD();
                    yield return null;
                }
            }
            timer = 0.0f;
            achievementPanelsActive--;
            m_finishedDisplayHint = true;
            while (timer < 0.5f)
            {
                timer += Time.deltaTime;
                float alpha = 1.0f - timer / 0.5f;
                fade.a = alpha;
                image.color = fade;
                hintImage.color = fade;
                textFade.a = alpha;
                text.color = textFade;
                yield return null;
            }
            Destroy(achievementPanel);
        }
    }

}