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

    public static event System.Action OnBookOpened;
    public static event System.Action OnFollowCalled;

    public static void RaiseBookOpened() => OnBookOpened?.Invoke();
    public static void RaiseFollowCalled() => OnFollowCalled?.Invoke();
}
