using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamSetup : MonoBehaviour
{
    public static SteamSetup main;

    [SerializeField]
    private uint appID = 0;

    public bool useSteam = true;
    public bool initalized = false;

    private void Start()
    {
        main = this;

        //Try to Initalize steam other wise give an exception
        try
        {
            if (useSteam)
            {
                SteamClient.Init(appID);
                initalized = true;
            }      
        }
        catch ( System.Exception e)
        {
            Debug.LogError("Failed to Initialize Steam: " + e.Message);
        }
        
    }
    private void OnDestroy()
    {
        //shutdown steam
        SteamClient.Shutdown();
        initalized = false;
        
    }
}
