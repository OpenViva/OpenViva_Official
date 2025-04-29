using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Viva.Util;

namespace Viva
{

    public class GamePostProcessing : MonoBehaviour
    {
        //Only really used for animated effects
        public enum Effect
        {
            HURT,
            SPLASH
        }
        
        [SerializeField]
        private Material playerHurtMat;
        [SerializeField]
        private Material playerSplashMat;

        public ScriptableRendererFeature HurtScreen;
        public ScriptableRendererFeature SplashScreen;
        public ScriptableRendererFeature GhostScreen;

        [SerializeField] private Volume underWaterPP;

        private int alphaID = Shader.PropertyToID("_Alpha");
        private float lastScreenEffect = 0.0f;


        public void DisplayScreenEffect(Effect effect)
        {

            if (Time.time - lastScreenEffect < 4.0f)
            {
                return;
            }
            lastScreenEffect = Time.time;

            switch (effect)
            {

                case Effect.HURT:
                    GameDirector.instance.StartCoroutine(AnimateHurtMaterial());
                    break;
                case Effect.SPLASH:
                    GameDirector.instance.StartCoroutine(AnimateSplashMaterial());
                    break;
            }
        }


        private IEnumerator AnimateSplashMaterial()
        {
            SplashScreen.SetActive(true);
            float timerLength = 2.0f;
            float timer = 0.0f;
            while (timer < timerLength)
            {
                float ratio = 1.0f - timer / timerLength;
                playerSplashMat.SetFloat(alphaID, ratio * ratio * ratio);
                timer += Time.deltaTime;
                yield return null;
            }
            SplashScreen.SetActive(false);
        }

        private IEnumerator AnimateHurtMaterial()
        {
            HurtScreen.SetActive(true);
            float timerLength = 0.1f;
            float timer = 0.0f;
            while (timer < timerLength)
            {
                float ratio = 1.0f - timer / timerLength;
                playerHurtMat.SetFloat(alphaID, 1.0f - ratio * ratio);
                timer += Time.deltaTime;
                yield return null;
            }
            timerLength = 0.3f;
            timer = timerLength;
            while (timer > 0.0f)
            {
                playerHurtMat.SetFloat(alphaID, Tools.EaseInOutQuad(timer / timerLength));
                timer -= Time.deltaTime;
                yield return null;
            }
            HurtScreen.SetActive(false);
        }
        
        public void ToggleUnderWater(bool enable)
        {
            underWaterPP.weight = enable ? 1f : 0f;
        }
    }
    
    


}