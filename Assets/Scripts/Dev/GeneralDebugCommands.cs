using IngameDebugConsole;
using UnityEngine;

public class GeneralDebugCommands : MonoBehaviour
{
    void Start()
    {
        //DebugLogConsole.AddCommandInstance("example", "Change the maximum FPS, a value between 15 and 1000", nameof(ChangeFPSTarget), this);
        DebugLogConsole.AddCommandInstance("exit", "Exit the game...", nameof(ExitApplication), this);
        DebugLogConsole.AddCommandInstance("set_fps", "Change the maximum FPS, a value between 15 and 1000", nameof(ChangeFPSTarget), this, "targetFPS");
    }

    void ExitApplication()
    {
        Application.Quit();
    }

    void ChangeFPSTarget(int target)
    {
        Application.targetFrameRate = Mathf.Clamp(target, 15, 1000);
    }
}
