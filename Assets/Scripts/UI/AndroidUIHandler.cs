using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Viva.Util;

namespace Viva
{
    public class AndroidUIHandler : MonoBehaviour
    {

        public static AndroidUIHandler Instance { get; private set; }

        [SerializeField]
        private GameObject mobileUI;
        [SerializeField]
        private CustomButton dropButton;

        public bool isUIActive => mobileUI.activeSelf;

        public bool IsAndroidBuild()
        {

            return Application.platform == RuntimePlatform.Android;
        }

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("There are more than one AndroidUIHandler in the scene, There should only be one.");
            }
            
        }

        private void Start()
        {
            if (mobileUI)
            {
                mobileUI.SetActive(IsAndroidBuild());
            }
        }

        private void Update()
        {
            
        }

        public void PlayGestureButton(GestureButton button)
        {
            GameDirector.player.objectFingerPointer.FireGesture(GetRandomGestureHand(), button.GestureToFire);
        }

        public ObjectFingerPointer.GestureHand GetRandomGestureHand()
        {
            switch (Random.Range(0, 1))
            {
                case 0:
                    return GameDirector.player.objectFingerPointer.rightGestureHand;
                case 1:
                    return GameDirector.player.objectFingerPointer.leftGestureHand;
                default:
                    return GameDirector.player.objectFingerPointer.rightGestureHand;
            }

        }

        public void PickupOrDropItem(string hand)
        {
            PlayerHandState handState = GetPlayerHandStateFromString(hand);
            if (dropButton.buttonIsPressed)
            {
                GameDirector.player.DoItemDrop(handState);
            }
            else
            {
                GameDirector.player.DoItemPickup(handState);
            }
        }

        public static PlayerHandState GetPlayerHandStateFromString(string hand)
        {
            switch (hand)
            {
                case "right":
                    return GameDirector.player.rightPlayerHandState;
                case "left":
                    return GameDirector.player.leftPlayerHandState;
                default:
                    Debug.LogError("HandState is null");
                    return null;
            }
        }

    }
}