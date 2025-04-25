//Anything that works with Steamworks should only compile if Steamworks is available
#if !DISABLESTEAMWORKS  && STEAMWORKSNET

using UnityEngine;
using System;

//You must "use" the Heathen namespace to access our tools
using Heathen.SteamworksIntegration;
//In our examples we will also work with the API so we add its namespace as well
using Heathen.SteamworksIntegration.API;
//In most cases you also need the Steamworks namespace so you can work with its native enums and similar
using Steamworks;

//You must ALWAYS define your code in a properly formed namespace
//You can obviously name it whatever you want
namespace MyProperlyFormedNamespaceName
{
    [Obsolete("This script is for demonstration purposes only and should NEVER be used as is in any project.")]
    public class Snipit_LeaderboardData : MonoBehaviour
    {

        //This will make it so this code ONLY compiles in Unity Editor ... we do this to try and make sure you actually read and
        //DO NOT try to use this in a build as is ... it IS NOT meant to be copy and pasted its meant to be read like an article
        //A teach tool, not a working production ready but of code.
#if UNITY_EDITOR

        //We will need to know the API name of the board we want to work with
        [SerializeField]
        private string apiName;

        //We need to store the board we find so we can use it in later examples
        private LeaderboardData targetBoard;

        //This is used in the UploadScore example below and is just an example
        //You would not typically create a struct in this manner for this use
        [Serializable]
        public struct ExampleSerializableDataForAttachment
        {
            public int anInt;
            public string aString;
            public bool aBool;
        }

        void Start()
        {
            //Before we can use a leaderboard we need to get its ID ... not its API Name ... its ID
            //The static function takes in the api name and a delegate to be called when the process is completed
            //This is an asynchronous method ... a delegate is simply a pointer to a function
            //You could give it a named function you defined in your class but far more commonly you use an anonymous function as we have here
            LeaderboardData.Get(apiName, (data, ioError) =>
            {
                if (!ioError)
                {
                    targetBoard = data;
                    Debug.Log($"Found {apiName} with ID {targetBoard.id.m_SteamLeaderboard}");

                    //At this point you now have the board and do things with it .... see the functions below that give examples of working with the board such as reading and writing data
                }
                else
                {
                    Debug.LogError($"An IO error occurred while attempting to read {apiName}");
                }
            });
        }

        public void Example_UploadScore()
        {
            //First we need to know the score we want to upload, for this example we will hardcode a value of 42
            var score = 42;

            //We also need to know the upload rule we want to use ... we can ask Steam to only take the score if its better
            //than the existing score ... or we can tell Steam to take the score no matter what
            //For this example we will use Keep Best which is the most common
            var uploadMethod = ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest;

            //Optionally we can store "details" on the record this is an array of int values with a max length of 64
            //You do not have to do this and you shouldn't unless you have a use for additional data related to the score
            var details = new int[]{ 13, 26, 78, 92 };

            //Finally we can attach a file to the entry, this is done via Steam's User Generated Content feature and depends
            //on the Steam Remote Storage quota ... this has to be set up in your Steamworks Developer portal to tell Steam
            //how many files and what max size the game is allowed to take up.
            //Note that once attached the file can be removed from the user's storage it will be cashed by Steam on CDN
            //Our tools can upload any serializable data you would like as a file for this purpose ... 
            var fileData = new ExampleSerializableDataForAttachment
            {
                anInt = 77,
                aString = "Hello World!",
                aBool = true,
            };

            //Now that we have everything we want to upload, we can upload and attach the file
            //We should of course check and make sure the board is valid though
            if(targetBoard.Valid)
            {
                //As with any async function call we will be passing in a delegate as the last parameter
                //this delegate gets ran when the process is completed and contains data about the results
                //You can use a named function here or an anonymous function as we have
                targetBoard.UploadScore(score, details, uploadMethod, (callbackResult, ioError) =>
                {
                    if(!ioError)
                    {
                        if (callbackResult.Success)
                        {
                            Debug.Log($"Score Updated: New Rank = {callbackResult.GlobalRankNew}, Previous Rank = {callbackResult.GlobalRankPrevious}");

                            //Now that the entry has been updated we can attach our file
                            //Note the file will need a name but this is just the name of the file for the temporary upload
                            //So this is the name that will be in the user's Remote Storage, once attached we can remove it to save space.
                            //we like to us the same name all the time "tempFile" and we do not bother removing it instead we just let it 
                            //overwrite each time as a means to hold the space for future attachments
                            targetBoard.AttachUGC("tempFile", fileData, (ugcResult, ugcIoError) =>
                            {
                                if (!ugcIoError)
                                {
                                    if (ugcResult.Result == Steamworks.EResult.k_EResultOK)
                                        Debug.Log($"Attached file data to user entry on board {apiName}");
                                    else
                                        Debug.LogError($"Failed to attach file data to user entry on board {apiName}, Response Result = {ugcResult.Result}");
                                }
                                else
                                {
                                    Debug.LogError($"Failed to attach file data to user entry on board {apiName}, IO Error!");
                                }
                            });
                        }
                        else
                        {
                            Debug.LogError($"Steam refused to upload the score, this is usually because the board is set to write mode trusted and can thus only be updated from Steamworks Web API calls from a trusted server.");
                        }
                    }
                    else
                    {
                        Debug.LogError($"Failed to upload score on board {apiName}, IO Error!");
                    }
                });
            }
            else
            {
                Debug.LogError($"The board is note valid, this usually means you have not yet 'got' the board");
            }
        }

        public void Example_ReadUserScore()
        {
            //We need to tell the query how many details we want to read from the board
            //Details is the int[] you can optionally upload to a board when uploading a score
            //You do not have to read any so 0 is a valid value here
            //This only determines the size of the buffer used to read the details it doesn't modify the details on the board
            var detailEntriesCount = 0;

            //Before we use it we check and make sure the board ID is valid
            if (targetBoard.Valid)
            {
                //The last parameter as always with async calls to Steamworks is a delegate
                //The delegate includes the LeaderboardEntry which is the found entry if any, and a bool indicating if an IO Error had occurred ... true is bad not good
                targetBoard.GetUserEntry(detailEntriesCount, (foundEntry, ioError) =>
                {
                    if (!ioError)
                    {
                        if (foundEntry != null)
                        {
                            Debug.Log($"Found Entry: Score = {foundEntry.Score}, Rank = {foundEntry.Rank}, Detail Count Found = {(foundEntry.details != null ? foundEntry.details.Length : 0)}, Has Attachment? {foundEntry.UgcHandle != UGCHandle_t.Invalid}");
                        }
                        else
                        {
                            Debug.Log("No Entry Found: this suggests the user does not' have an entry for this board.");
                        }
                    }
                    else
                    {
                        Debug.LogError($"An IO error occurred while attempting to read {apiName}");
                    }
                });
            }
            else
            {
                Debug.LogError($"The board is note valid, this usually means you have not yet 'got' the board");
            }
        }

        public void Example_ReadTopScores()
        {
            //For brevity see the ReadUserScore to see what all you can read from any given LeaderboardEntry record
            // E.g. how you can read the score, rank, etc.

            //First we need to know how many entries we want to read for
            var entriesToRead = 10;

            //We need to tell the query how many details we want to read from the board
            //Details is the int[] you can optionally upload to a board when uploading a score
            //You do not have to read any so 0 is a valid value here
            //This only determines the size of the buffer used to read the details it doesn't modify the details on the board
            var detailEntriesCount = 0;

            //Before we use it we check and make sure the board ID is valid
            if (targetBoard.Valid)
            {
                //The last parameter as always with async calls to Steamworks is a delegate
                //The delegate includes the LeaderboardEntry which is the found entires if any, and a bool indicating if an IO Error had occurred ... true is bad not good
                targetBoard.GetTopEntries(entriesToRead, detailEntriesCount, (entriesFound, ioError) =>
                {
                    if (!ioError)
                    {
                        // entriesFound is an array of LeaderboardEntry so we can either hand that off to some other function to create UI elements for it
                        // or we can iterate over them and read them right here
                        foreach (var entry in entriesFound)
                        {
                            //Do Work on each entry
                        }
                    }
                    else
                    {
                        Debug.LogError($"An IO error occurred while attempting to read {apiName}");
                    }
                });
            }
            else
            {
                Debug.LogError($"The board is note valid, this usually means you have not yet 'got' the board");
            }
        }

        public void Example_ReadAttachment(LeaderboardEntry entry)
        {
            //Assuming your attachment is a serializable type we can read it for you
            entry.GetAttachedUgc<ExampleSerializableDataForAttachment>((dataFound, ioError) =>
            {
                if(!ioError)
                {
                    //Data Found is your attachment if any. Its up to you to check and see if that is a default value or actually set data
                    //how you do that depends on what kind of object it is ... for a struct you should always implement the IEquitable, IComparable and overloads for == and != operators
                }
            });

            //If its not you can read it your self from Remote Storage
            RemoteStorage.Client.UGCDownload(entry.UgcHandle, 0, (ugcDownloadResult, downloadIOError) =>
            {
                if (!downloadIOError && ugcDownloadResult.m_eResult == EResult.k_EResultOK)
                {
                    //Once its downloaded we can read its name
                    var theFileName = ugcDownloadResult.m_pchFileName;
                    //And we can read its data as a byte[]
                    var theFileData = RemoteStorage.Client.UGCRead(ugcDownloadResult.m_hFile);
                }
            });
        }
#endif
    }
}
#endif