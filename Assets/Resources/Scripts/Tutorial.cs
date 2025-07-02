using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace Viva
{
    
    public enum MenuTutorial
    {
        NONE,
        WAIT_TO_OPEN_PAUSE_MENU,
        WAIT_TO_ENTER_CHECKLIST,
        WAIT_TO_EXIT_CHECKLIST,
        WAIT_TO_CROUCH,

        WAIT_TO_WAVE,
        WAIT_TO_COME_HERE,
        WAIT_TO_PICKUP_DUCKY,
        WAIT_TO_GIVE_DUCKY,
            
        // WAIT_TO_SELECT,
        // WAIT_TO_SELECT_ACTIVITY,

        WAIT_TO_PICKUP_CAMERA,
        WAIT_TO_TAKE_PICTURE,
        WAIT_TO_PICKUP_FRAME,
        WAIT_TO_RIP_FRAME,

        FINISHED_ALL
    }
    
    //TODO: This was rushed for beta clean it up 
    public class Tutorial : MonoBehaviour
    {
        
        public static Tutorial instance;

        [SerializeField] private Transform playerTeleportTransform;
        private MenuTutorial menuTutorialPhase = MenuTutorial.NONE;
        private Coroutine tutorialCoroutine = null;
        private Vector3 lastPositionBeforeTutorial = Vector3.zero;

        [SerializeField] private Animator[] gateAnimators;

        private GameObject camera;
        private GameObject duck;
        public GameObject frame;

        [SerializeField] private GameObject cameraPrefab;
        [SerializeField] private GameObject duckPrefab;

        [SerializeField] private Transform companionSpawnTransform;

        [SerializeField] private Transform gateItemSpawn3;
        
        private void Awake()
        {
            instance = this;
        }

        public void OnStartTutorial()
        {
            if (tutorialCoroutine != null)
            {
                return;
            }
            lastPositionBeforeTutorial = GameDirector.player.transform.position;
            GameDirector.player.transform.position = playerTeleportTransform.transform.position;
            menuTutorialPhase = MenuTutorial.NONE;
            tutorialCoroutine = GameDirector.instance.StartCoroutine(TutorialCoroutine());
        }

        public void OpenGate(int index)
        {
            gateAnimators[index].SetBool("Open", true);
        }

        public void CloseGate(int index)
        {
            gateAnimators[index].SetBool("Open", false);
        }

        public void CloseAllGates()
        {
            for (int i = 0; i < gateAnimators.Length - 1; i++)
            {
                CloseGate(i);
            }
        }

        public void DisplayHUDMessage(string message, bool playsound, PauseMenu.HintType hintType, MenuTutorial waitForPhase = MenuTutorial.NONE)
        {
            GameDirector.instance.StartCoroutine(GameDirector.player.pauseMenu.HandleHUDMessage(message, playsound, hintType, waitForPhase));
        }
        
        private void ExitTutorial()
        {
            if (tutorialCoroutine == null)
            {
                return;
            }
            Debug.Log("[TUTORIAL] Exited");

            menuTutorialPhase = MenuTutorial.FINISHED_ALL;
            GameDirector.instance.StopCoroutine(tutorialCoroutine);
            tutorialCoroutine = null;
            CloseAllGates();
            //destroy tutorial objects
            Destroy(frame);
            Destroy(duck);
            //resume game
            GameDirector.player.transform.position = lastPositionBeforeTutorial;
            //reset music
            GameDirector.instance.SetMusic(GameDirector.instance.GetDefaultMusic(), 3.0f);
            GameDirector.instance.SetUserIsExploring(false);
        }
        // private void CheckIfExitedTutorialCircle()
        // {
        //     if (Vector3.SqrMagnitude(transform.position - tutorialCircle.transform.position) > 90.0f)
        //     {
        //         ExitTutorial();
        //     }
        // }

        public void ContinueTutorial(MenuTutorial continuePhase)
        {
            if (tutorialCoroutine != null)
            {
                if ((MenuTutorial)((int)menuTutorialPhase + 1) == continuePhase)
                {
                    menuTutorialPhase = continuePhase;
                    Debug.Log("Continued tutorial! Now at " + menuTutorialPhase);
                }
            }
        }
        
        public MenuTutorial GetMenuTutorialPhase()
        {
            return menuTutorialPhase;
        }

        private IEnumerator TutorialCoroutine()
        {
            GameDirector.instance.SetUserIsExploring(true);
            //tutorialCircle.SetActive(true);
            Player player = GameDirector.player;
            yield return new WaitForSeconds(1.0f);
            GameDirector.instance.town.BuildTownLolis(new string[] { "kyaru" }, 1, companionSpawnTransform.position);
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Welcome"), true, PauseMenu.HintType.HINT_NO_IMAGE);
            //DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "LeaveGreenCircle"), true, PauseMenu.HintType.HINT);

            while (!GameDirector.player.pauseMenu.finishedDisplayedHint)
            {
                //CheckIfExitedTutorialCircle();
                yield return null;
            }
            if (player.controls == Player.ControlType.KEYBOARD)
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "OpenPauseBook"), true, PauseMenu.HintType.HINT, MenuTutorial.WAIT_TO_OPEN_PAUSE_MENU);
                while (menuTutorialPhase <= MenuTutorial.NONE)
                {
                    //CheckIfExitedTutorialCircle();
                    yield return null;
                }
            }
            else
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "OpenPauseBookVR"), true, PauseMenu.HintType.HINT, MenuTutorial.WAIT_TO_OPEN_PAUSE_MENU);
                while (menuTutorialPhase <= MenuTutorial.NONE)
                {
                    //CheckIfExitedTutorialCircle();
                    yield return null;
                }
            }
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Welcome"), true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_ENTER_CHECKLIST);
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "RecommendCalibrate"), true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_ENTER_CHECKLIST);
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "WhenReady"), true, PauseMenu.HintType.HINT, MenuTutorial.WAIT_TO_ENTER_CHECKLIST);
            while (menuTutorialPhase <= MenuTutorial.WAIT_TO_OPEN_PAUSE_MENU)
            {
                //CheckIfExitedTutorialCircle();
                yield return null;
            }
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "CompleteTasks"), true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_EXIT_CHECKLIST);
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "WhenReady2"), true, PauseMenu.HintType.HINT, MenuTutorial.WAIT_TO_EXIT_CHECKLIST);
            while (menuTutorialPhase <= MenuTutorial.WAIT_TO_ENTER_CHECKLIST)
            {
                //CheckIfExitedTutorialCircle();
                yield return null;
            }

            if (player.controls == Player.ControlType.KEYBOARD)
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "WalkRun"), true, PauseMenu.HintType.HINT);
            }
            else
            {
                if (GameSettings.main.vrControls == Player.VRControlType.TRACKPAD)
                {
                    DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "WalkRunVR"), true, PauseMenu.HintType.HINT_NO_IMAGE);
                    DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "RunFaster"), true, PauseMenu.HintType.HINT);
                }
                else
                {
                    DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Teleport"), true, PauseMenu.HintType.HINT);
                }
            }
            while (!GameDirector.player.pauseMenu.finishedDisplayedHint)
            {
                //CheckIfExitedTutorialCircle();
                yield return null;
            }
            DisplayHUDMessage("Good now move to the next area", true, PauseMenu.HintType.HINT_NO_IMAGE);
            OpenGate(0);
            if (player.controls == Player.ControlType.KEYBOARD)
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "SitDown"), true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_CROUCH);
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "HandGames"), true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_CROUCH);
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Crouch"), true, PauseMenu.HintType.HINT, MenuTutorial.WAIT_TO_CROUCH);
                while (menuTutorialPhase <= MenuTutorial.WAIT_TO_EXIT_CHECKLIST)
                {
                    //CheckIfExitedTutorialCircle();
                    if (player.GetKeyboardCurrentHeight() < 1.0f)
                    {
                        ContinueTutorial(MenuTutorial.WAIT_TO_CROUCH);
                    }
                    yield return null;
                }
            }
            else
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "SitDown"), true, PauseMenu.HintType.HINT_NO_IMAGE);
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "HandGames"), true, PauseMenu.HintType.HINT);
                float time = Time.time;
                while (Time.time - time < 5.0f)
                {
                    //CheckIfExitedTutorialCircle();
                    yield return null;
                }
                menuTutorialPhase = MenuTutorial.WAIT_TO_CROUCH;
            }

            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "MakeGestures"), true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_WAVE);
            if (player.controls == Player.ControlType.KEYBOARD)
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "SayHello"), true, PauseMenu.HintType.HINT, MenuTutorial.WAIT_TO_WAVE);
            }
            else
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "SayHelloVR"), true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_WAVE);
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "UntilIcon"), true, PauseMenu.HintType.HINT, MenuTutorial.WAIT_TO_WAVE);
            }
            while (menuTutorialPhase <= MenuTutorial.WAIT_TO_CROUCH)
            {
                //CheckIfExitedTutorialCircle();
                yield return null;
            }
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Good"), true, PauseMenu.HintType.HINT);
            while (!GameDirector.player.pauseMenu.finishedDisplayedHint)
            {
                //CheckIfExitedTutorialCircle();
                yield return null;
            }
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "NowGesture"), true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_COME_HERE);
            if (player.controls == Player.ControlType.KEYBOARD)
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Follow"), true, PauseMenu.HintType.HINT, MenuTutorial.WAIT_TO_COME_HERE);
            }
            else
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "FollowVR"), true, PauseMenu.HintType.HINT, MenuTutorial.WAIT_TO_COME_HERE);
            }
            while (menuTutorialPhase <= MenuTutorial.WAIT_TO_WAVE)
            {
                //CheckIfExitedTutorialCircle();
                yield return null;
            }
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "FollowYou"), true, PauseMenu.HintType.HINT);
            while (!GameDirector.player.pauseMenu.finishedDisplayedHint)
            {
                //CheckIfExitedTutorialCircle();
                yield return null;
            }
            DisplayHUDMessage("Good now move to the next area", true, PauseMenu.HintType.HINT_NO_IMAGE);
            OpenGate(1);
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "PickupDuck"), true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_PICKUP_DUCKY);
            duck = GameObject.Instantiate(duckPrefab, gateItemSpawn3.transform.position + Vector3.up * 0.1f, Quaternion.identity);
            if (player.controls == Player.ControlType.KEYBOARD)
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Pickup"), true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_PICKUP_DUCKY);
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Drop"), true, PauseMenu.HintType.HINT, MenuTutorial.WAIT_TO_PICKUP_DUCKY);
            }
            else
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "PickupDropVR"), true, PauseMenu.HintType.HINT, MenuTutorial.WAIT_TO_PICKUP_DUCKY);
            }
            while (menuTutorialPhase <= MenuTutorial.WAIT_TO_COME_HERE)
            {
                //CheckIfExitedTutorialCircle();
                if (player.rightHandState.heldItem != null)
                {
                    if (player.rightHandState.heldItem.gameObject == duck)
                    {
                        ContinueTutorial(MenuTutorial.WAIT_TO_PICKUP_DUCKY);
                    }
                }
                else if (player.leftHandState.heldItem != null)
                {
                    if (player.leftHandState.heldItem.gameObject == duck)
                    {
                        ContinueTutorial(MenuTutorial.WAIT_TO_PICKUP_DUCKY);
                    }
                }
                yield return null;
            }
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "ExtendDucky"), true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_GIVE_DUCKY);
            if (player.controls == Player.ControlType.KEYBOARD)
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Drop"), true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_GIVE_DUCKY);
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "ExtendOffering"), true, PauseMenu.HintType.HINT, MenuTutorial.WAIT_TO_GIVE_DUCKY);
            }
            while (menuTutorialPhase <= MenuTutorial.WAIT_TO_PICKUP_DUCKY)
            {
                //CheckIfExitedTutorialCircle();
                yield return null;
            }
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "LikeThat"), true, PauseMenu.HintType.HINT);
            while (!GameDirector.player.pauseMenu.finishedDisplayedHint)
            {
                //CheckIfExitedTutorialCircle();
                yield return null;
            }

            //polaroid camera tutorial section
            camera = GameObject.Instantiate(cameraPrefab, gateItemSpawn3.transform.position + Vector3.up * 0.1f, Quaternion.identity);
            DisplayHUDMessage("Now Pickup The camera in the center", true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_PICKUP_CAMERA);
            
            while (menuTutorialPhase <= MenuTutorial.WAIT_TO_GIVE_DUCKY)
            {
                //CheckIfExitedTutorialCircle();
                if (player.rightHandState.heldItem != null)
                {
                    if (player.rightHandState.heldItem.gameObject == camera)
                    {
                        ContinueTutorial(MenuTutorial.WAIT_TO_PICKUP_CAMERA);
                    }
                }
                else if (player.leftHandState.heldItem != null)
                {
                    if (player.leftHandState.heldItem.gameObject == camera)
                    {
                        ContinueTutorial(MenuTutorial.WAIT_TO_PICKUP_CAMERA);
                    }
                }
                yield return null;
            }

            if (player.controls == Player.ControlType.KEYBOARD)
            {
                DisplayHUDMessage("Now Take a picture using the Left Mouse button", true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_TAKE_PICTURE);
            }
            else
            {
                DisplayHUDMessage("Now Take a picture using the Right Trigger", true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_TAKE_PICTURE);
            }

            while (menuTutorialPhase <= MenuTutorial.WAIT_TO_PICKUP_CAMERA)
            {
                //
                yield return null;
            }
            
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "PickupPolaroid"), true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_PICKUP_FRAME);
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "DropRest"), true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_PICKUP_FRAME);
            if (player.controls == Player.ControlType.KEYBOARD)
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Drop"), true, PauseMenu.HintType.HINT, MenuTutorial.WAIT_TO_PICKUP_FRAME);
            }
            else
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "PickupDropVR"), true, PauseMenu.HintType.HINT, MenuTutorial.WAIT_TO_PICKUP_FRAME);
            }
            
            while (menuTutorialPhase <= MenuTutorial.WAIT_TO_TAKE_PICTURE)
            {
                //CheckIfExitedTutorialCircle();
                if (player.rightHandState.heldItem != null)
                {
                    if (player.rightHandState.heldItem.gameObject == frame)
                    {
                        ContinueTutorial(MenuTutorial.WAIT_TO_PICKUP_FRAME);
                    }
                }
                else if (player.leftHandState.heldItem != null)
                {
                    if (player.leftHandState.heldItem.gameObject == frame)
                    {
                        ContinueTutorial(MenuTutorial.WAIT_TO_PICKUP_FRAME);
                    }
                }
                yield return null;
            }

            if (player.controls == Player.ControlType.KEYBOARD)
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "RipFrame"), true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_RIP_FRAME);
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "OnceItsHeld"), true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_RIP_FRAME);
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "ClickWithBoth"), true, PauseMenu.HintType.HINT, MenuTutorial.WAIT_TO_RIP_FRAME);
            }
            else
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "RipFrameVR"), true, PauseMenu.HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_RIP_FRAME);
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "ClickWithBothVR"), true, PauseMenu.HintType.HINT, MenuTutorial.WAIT_TO_RIP_FRAME);
            }
            while (menuTutorialPhase <= MenuTutorial.WAIT_TO_PICKUP_FRAME)
            {
                //CheckIfExitedTutorialCircle();
                if (frame == null)
                {
                    ContinueTutorial(MenuTutorial.WAIT_TO_RIP_FRAME);
                }
                yield return null;
            }

            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "TutorialFinished"), true, PauseMenu.HintType.HINT);
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "UnderDevelopment"), true, PauseMenu.HintType.HINT_NO_IMAGE);          
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "PreviousLocation"), true, PauseMenu.HintType.HINT_NO_IMAGE);

            ExitTutorial();
        }

    }
}