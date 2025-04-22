//Anything that works with Steamworks should only compile if Steamworks is available
#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;
using UnityEngine;

//You must "use" the Heathen namespace to access our tools
using Heathen.SteamworksIntegration;
//In our examples we will also work with the API so we add its namespace as well
using Heathen.SteamworksIntegration.API;
//In some cases you also need the Steamworks namespace so you can work with its native enums and similar


//You must ALWAYS define your code in a properly formed namespace
//You can obviously name it whatever you want
namespace MyProperlyFormedNamespaceName
{
    [Obsolete("This script is for demonstration purposes only and should NEVER be used as is in any project.")]
    public class Snipit_AchievementData : MonoBehaviour
    {
#if UNITY_EDITOR
        AchievementData myAch;

        private void Start()
        {
            myAch = "Your Achievement API Name";
        }

        public void Example_ReadAchievement()
        {
            //Is this achievement achieved?
            var isAchieved = myAch.IsAchieved;

            //When was this unlocked
            var dateTime = myAch.UnlockTime;

            //What is the icon for this achievement
            //This is an asynchronous call so its parameter is a delegate
            //that will be ran when the call is completed
            myAch.GetIcon(texture2D =>
            {
                //texture2D is a UnityEngine.Texture2D that can be used as needed
                //This will be the icon the user sees e.g. if the achievement is unlocked
                //this will be the unlocked version
                //if the achievement is locked it will be the locked version
                //If you unlock the achievement or if its status changes you can get the icon again
                //to get the new icon version
            });
        }

        public void Example_ReadAchievementForUser(UserData user)
        {
            //Before we can read other user's data we have to ask Steam to cash it
            var requestedData = user.RequestInformation();
            //if the resulting bool is true, that means the request has been sent and we should wait for 
            //Person State Change to let us know when its ready
            // ... This is not a functional event listener ... just highlighting the event you would use
            Friends.Client.EventPersonaStateChange.AddListener(null /*Some Function Name*/);
            //The parameters on this event tell us who the change is about so we can know if we now have this users data


            //You can read the achievements of other uses once you have there data
            var achState = myAch.GetAchievementAndUnlockTime(user);
            //The return is a tuple that defines the unlocked
            var isAchieved = achState.unlocked;
            //and unlock time
            var dateTime = achState.unlockTime;
        }

        public void Example_SetAchievement()
        {
            myAch.IsAchieved = true;
            // or
            myAch.Unlock();
        }
#endif
    }
}
#endif