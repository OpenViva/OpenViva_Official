using System;
using UnityEngine;

public static class Globals
{
    // --- UI ---
    public static bool isMenuOpen = false;
    public static bool allowMenuOpen = true;

    // --- Movement ---
    public static bool handleMovement = true;
    public static bool handleKBLook = true;

    // --- Toggles ---
    public static bool isDesktopMode = true;

    // --- Events ---
    public static event Action<Camera> OnCameraChanged;

    // Call this method whenever you toggle between Desktop and VR
    public static void SetGameMode(bool desktopMode, Camera activeCamera)
    {
        isDesktopMode = desktopMode;
        OnCameraChanged?.Invoke(activeCamera);
    }

    public static event Action OnBookOpened;
    public static event Action OnFollowCalled;

    public static void RaiseBookOpened() => OnBookOpened?.Invoke();
    public static void RaiseFollowCalled() => OnFollowCalled?.Invoke();
}
