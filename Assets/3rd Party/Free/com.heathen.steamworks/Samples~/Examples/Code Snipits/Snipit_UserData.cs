//Anything that works with Steamworks should only compile if Steamworks is available
#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;
using UnityEngine;

//You must "use" the Heathen namespace to access our tools
using Heathen.SteamworksIntegration;
//In our examples we will also work with the API so we add its namespace as well
using Heathen.SteamworksIntegration.API;
//In some cases you also need the Steamworks namespace so you can work with its native enums and similar
using Steamworks;


//You must ALWAYS define your code in a properly formed namespace
//You can obviously name it whatever you want
namespace MyProperlyFormedNamespaceName
{
    [Obsolete("This script is for demonstration purposes only and should NEVER be used as is in any project.")]
    public class Snipit_UserData : MonoBehaviour
    {
        //This will make it so this code ONLY compiles in Unity Editor ... we do this to try and make sure you actually read and
        //DO NOT try to use this in a build as is ... it IS NOT meant to be copy and pasted its meant to be read like an article
        //A teach tool, not a working production ready but of code.
#if UNITY_EDITOR
        private void Start()
        {
            //You can get the current user's data at any point
            var theUser = UserData.Me;

            //With a UserData (local user or any other Steam user) you can read

            //Name
            var theName = theUser.Name;

            //Level
            var theLevel = theUser.Level;

            //Game Info
            //This checks to see if this user is in any game, not just this one
            var isInAGame = theUser.InGame;
            //This checks to see if this user is in THIS game, not just any game
            var isInTHISGame = theUser.InThisGame;
            //This holds info about the game session the user is in if any
            var theInfo = theUser.GameInfo;

            Debug.Log($"My Name is = {theName}, My Level is = {theLevel}");
        }

        public void Example_GetMyHexId()
        {
            var myHexId = UserData.Me.ToString();
            //or
            myHexId = UserData.Me.HexId;
        }

        public void Example_GetUserDataFromHex(string friendHexId)
        {
            var myFriend = UserData.Get(friendHexId);
        }

        public void Example_ListMyFriends()
        {
            var myFriends = UserData.MyFriends;
        }

        public void Example_GetAvatarImage(UserData user)
        {
            //This is an asynchronous function so it has a delegate parameter that will be called
            //when the process is complete, it the image is already loaded in memory it will use that
            //so its safe to call this as frequently as you like it does NOT reload the image every time
            user.LoadAvatar(image =>
            {
                //image is the Texture2D you can use
            });
        }

        public void Example_SetRichPresence()
        {
            //This can only be done on the local user
            UserData.SetRichPresence("SomeKey", "SomeValue");
        }

        public void Example_GetRichPresence(UserData user)
        {
            //You would need to know what key your looking for but can read any friend's data
            var value = user.GetRichPresenceValue("someKey");
        }

        public void Example_SetAchievement()
        {
            //You can do this from many different places
            AchievementData theAchievement = "The Achievement API Name";
            theAchievement.Unlock();
            //or
            theAchievement.IsAchieved = true;

            //You could also do
            UserData.Me.SetAchievement(theAchievement);
            //or even
            UserData.Me.SetAchievement("The Achievement API Name");
        }

        public void Example_GetMyAchievementState()
        {
            //You can do this from many different places
            AchievementData theAchievement = "The Achievement API Name";

            var isAchieved = theAchievement.IsAchieved;
            var unlockTime = theAchievement.UnlockTime;
        }

        public void Example_GetAchievementState(UserData user)
        {
            //Before we can read other user's data we have to ask Steam to cash it
            var requestedData = user.RequestInformation();
            //if the resulting bool is true, that means the request has been sent and we should wait for 
            //Person State Change to let us know when its ready
            // ... This is not a functional event listener ... just highlighting the event you would use
            Friends.Client.EventPersonaStateChange.AddListener(null /*Some Function Name*/);
            //The parameters on this event tell us who the change is about so we can know if we now have this users data

            //If RequestInformation returns false it means we already have the data and we can go ahead and use it
            var achState = user.GetAchievement("Achievement API Name");
            var isAchieved = achState.unlocked;
            var wasAchievedWhen = achState.unlockTime;

            // You could alternatively do
            achState = AchievementData.Get("Achievement API Name").GetAchievementAndUnlockTime(user);
        }

        public void Example_SetStat()
        {
            //You can do this most easily from a StatData
            StatData theStat = "The Stat API Name";

            //for float stats
            theStat.Set(42.5f);
            //or for int stats
            theStat.Set(42);
            //or for average rate
            theStat.Set(42.5f, 500);
        }

        public void Example_GetMyStat()
        {
            //You can do this most easily from a StatData
            StatData theStat = "The Stat API Name";

            var floatValue = theStat.FloatValue();
            var intValue = theStat.IntValue();
        }

        public void Example_GetStat(UserData user)
        {
            //You can do this most easily from a StatData
            StatData theStat = "The Stat API Name";

            var floatValue = theStat.FloatValue(user);
            var intValue = theStat.IntValue(user);
        }

        public void Example_InviteToLobby(UserData user, LobbyData lobby)
        {
            //A lobby is NOT a game
            //A lobby is NOT a network connection
            //A lobby is NOT a server
            
            //A lobby IS a chat room

            //You can do this in either direction
            user.InviteToLobby(lobby);

            //or
            lobby.InviteUserToLobby(user);

            //When a user is invited the EventLobbyInvite is invoked
            // ... This is not a functional event listener ... just highlighting the event you would use
            Matchmaking.Client.EventLobbyInvite.AddListener(null /*Some Function Name*/);
            // The parameters in this event tell you who invited you, for what game you where invited (hint its not always this one) and
            // The LobbyData of the lobby they invited you to

            //You do not "have" to listen on that event
            //When the event is sent it will also send to that user's Steam Friend Chat
            //They can then click the Accept button in there and
            //When that user clicks the Accept button in the Steam chat in Overlay
            //The Event Game Lobby Join Requested event is raised
            // ... This is not a functional event listener ... just highlighting the event you would use
            Overlay.Client.EventGameLobbyJoinRequested.AddListener(null /*Some Function Name*/);
            // The parameters in this event tell you who invites you and for what lobby ... it is always for THIS game
        }

        public void Example_InviteToGame(UserData user, string connectionString)
        {
            //The string can be anything you want it to be
            user.InviteToGame(connectionString);
            //When you call that the Event Game Rich Presence Join Requested event is called on that user
            // ... This is not a functional event listener ... just highlighting the event you would use
            Overlay.Client.EventGameRichPresenceJoinRequested.AddListener(null /*Some Function name*/);
            //This event has a parameter that tells you who the join is from and what the string is
        }

        public void Example_GetLobbyMemberData(LobbyData lobby)
        {
            //This of course assumes you are a member of the lobby
            //But you can create a member data on demand with
            var memberData = LobbyMemberData.Get(lobby, UserData.Me);
            //or ... more simply ... ask for it from the lobby
            memberData = lobby.Me;
            // You can get the data for use of course
            // Note you would need a real user we just set it default here as an example
            UserData someOtherUser = default;
            memberData = lobby[someOtherUser];
        }
#endif
    }
}
#endif