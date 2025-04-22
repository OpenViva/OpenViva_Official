#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    [HelpURL("https://kb.heathen.group/assets/steamworks/voice")]
    public class VoiceRecorder : MonoBehaviour
    {
        [Serializable]
        public class ByteArrayEvent : UnityEvent<byte[]>
        { }

        [Range(0f, 1f)]
        public float bufferLength = 0.25f;

        [ReadOnly(true)]
        [SerializeField]
        private bool isRecording = false;
        /// <summary>
        /// Is the system currently recording audio data.
        /// </summary>
        public bool IsRecording
        {
            get { return isRecording; }
            set
            {
                if(value != IsRecording)
                {
                    if (value)
                        StartRecording();
                    else
                        StopRecording();
                }

            }
        }
        /// <summary>
        /// Occurs when the Voice Result Restricted EVoiceResult is received from the Steamworks API.
        /// </summary>
        public UnityEvent evtStopedOnChatRestricted;
        /// <summary>
        /// Occurs every frame when the Steamworks API has a voice stream payload from the user.
        /// </summary>
        public ByteArrayEvent evtVoiceStream;
        private float packetCounter = 0;

        private void Start()
        {
            packetCounter = bufferLength;
        }

        private void Update()
        {
            packetCounter -= Time.unscaledDeltaTime;

            if (packetCounter <= 0)
            {
                packetCounter = bufferLength;

                if (isRecording)
                {
                    var result = SteamUser.GetAvailableVoice(out uint pcbCompressed);
                    switch (result)
                    {
                        case EVoiceResult.k_EVoiceResultOK:
                            //All is well check the compressed size to see if we have data and if so package it
                            byte[] buffer = new byte[pcbCompressed];
                            SteamUser.GetVoice(true, buffer, pcbCompressed, out uint bytesWritten);
                            if (bytesWritten > 0)
                                evtVoiceStream.Invoke(buffer);
                            break;
                        case EVoiceResult.k_EVoiceResultNoData:
                            //No data so do nothing
                            break;
                        case EVoiceResult.k_EVoiceResultNotInitialized:
                            //Not initialized ... report the error
                            Debug.LogError("The Steamworks Voice system is not initialized and will be stopped.");
                            SteamUser.StopVoiceRecording();
                            break;
                        case EVoiceResult.k_EVoiceResultNotRecording:
                            //We are not recording but think we are
                            SteamUser.StartVoiceRecording();
                            break;
                        case EVoiceResult.k_EVoiceResultRestricted:
                            //User is chat restricted ... report this out and turn off recording.
                            evtStopedOnChatRestricted.Invoke();
                            SteamUser.StopVoiceRecording();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Starts the Steamworks API recording audio from the user's configured mic
        /// </summary>
        public void StartRecording()
        {
            isRecording = true;
            API.Voice.Client.StartRecording();
        }

        /// <summary>
        /// Stops the Steamworks API from recording audio for the user's configured mic
        /// </summary>
        public void StopRecording()
        {
            isRecording = false;
            API.Voice.Client.StopRecording();
        }
    }

}
#endif