using MinigameUIController;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static FruitCutMinigame;

public class Empty : MonoBehaviour
{
    //private void EnterMinigame(InputAction.CallbackContext context)
    //{
    //    if (_isPlaying || !Globals.isDesktopMode) { return; }

    //    // Check for fruit
    //    bool fruitFound = false;
    //    bool otherFound = false;
    //    if (_selectedFruit == Fruit.None)
    //    {
    //        if (PlayerManager.Instance.LeftHandOccupied)
    //        {
    //            int itemInLeft = PlayerManager.Instance.GetItemLeft();
    //            PlayerKB_GrabObject grabScript = PlayerManager.Instance.GetObjectLeft().GetComponent<PlayerKB_GrabObject>();
    //            grabScript.SetIsActive(false, 1);

    //            if (Enum.IsDefined(typeof(Fruit), itemInLeft))
    //            {
    //                _selectedFruit = (Fruit)itemInLeft;
    //                Destroy(grabScript.gameObject);
    //                fruitFound = true;
    //            }
    //            else
    //            {
    //                _otherObjectGrabScriptL = grabScript;
    //                otherFound = true;
    //            }
    //        }
    //        if (PlayerManager.Instance.RightHandOccupied)
    //        {
    //            int itemInRight = PlayerManager.Instance.GetItemRight();
    //            PlayerKB_GrabObject grabScript = PlayerManager.Instance.GetObjectRight().GetComponent<PlayerKB_GrabObject>();
    //            grabScript.SetIsActive(false, 2);

    //            if (Enum.IsDefined(typeof(Fruit), itemInRight) && !fruitFound)
    //            {
    //                _selectedFruit = (Fruit)itemInRight;
    //                Destroy(grabScript.gameObject);
    //                fruitFound = true;
    //            }
    //            else
    //            {
    //                _otherObjectGrabScriptL = grabScript;
    //                otherFound = true;
    //            }
    //        }

    //        // if (!otherFound) { _otherObjectGrabScriptL =  null; }

    //        if (fruitFound)
    //        {
    //            CuttingBoard.Instance.EnableFirstObject(_selectedFruit);
    //            SetNewTarget();

    //            switch (_selectedFruit)
    //            {
    //                case Fruit.Strawberry: _totalCutsNeeded = CuttingBoard.STRAWBERRY_MAX_CUTS; break;
    //                case Fruit.Peach: _totalCutsNeeded = CuttingBoard.PEACH_MAX_CUTS; break;
    //                case Fruit.Cantaloupe: _totalCutsNeeded = CuttingBoard.CANTALOUPE_MAX_CUTS; break;
    //            }
    //        }
    //    }
    //    else { fruitFound = true; }

    //    // Setup
    //    if (!fruitFound) { return; }
    //    _isPlaying = true;
    //    _mainCamera.enabled = false;
    //    _minigameCamera.enabled = true;
    //    _minigameHands.enabled = true;
    //    PlayerManager.Instance.HideHands(true);
    //    PlayerManager.Instance.LeftHandOccupied = true;
    //    PlayerManager.Instance.RightHandOccupied = true;
    //    Globals.handleKBLook = false;
    //    Globals.handleMovement = false;
    //    Globals.allowMenuOpen = false;
    //    _knife.transform.SetPositionAndRotation(_knifeHoldTransform.position, _knifeHoldTransform.rotation);

    //    // Show UI
    //    CuttingMinigame.Instance.CuttingMinigameUIEnabled(true);
    //}
}
