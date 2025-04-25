#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [HelpURL("https://kb.heathen.group/assets/steamworks/voice")]
    public class VoiceStream : MonoBehaviour
    {
        /// <summary>
        /// The audio source to output received and decoded voice messages to.
        /// </summary>
        public AudioSource outputSource;
        public SampleRateMethod sampleRateMethod = SampleRateMethod.Optimal;
        [Range(11025, 48000)]
        public uint customSampleRate = 28000;
        [Range(0, 3)]
        public float playbackDelay = 0.25f;
        private int sampleRate;
        private Queue<float> audioBuffer = new Queue<float>(48000);

        public double encodingTime = 0;

        private void Start()
        {
            outputSource.loop = true;
            if(playbackDelay > 0)
            {
                var nSample = sampleRateMethod == SampleRateMethod.Optimal ? (int)SteamUser.GetVoiceOptimalSampleRate() : sampleRateMethod == SampleRateMethod.Native ? AudioSettings.outputSampleRate : (int)customSampleRate;
                var delaySamples = (int)(nSample *  playbackDelay);
                for (int i = 0; i < delaySamples; i++)
                {
                    //Prebuffer silence equal to our delay, this will cause our playback to be behind real time by the amount of delay
                    audioBuffer.Enqueue(0);
                }
            }
        }

        private void Update()
        {
            var nSample = sampleRateMethod == SampleRateMethod.Optimal ? (int)SteamUser.GetVoiceOptimalSampleRate() : sampleRateMethod == SampleRateMethod.Native ? AudioSettings.outputSampleRate : (int)customSampleRate;

            if (nSample != sampleRate)
            {
                sampleRate = nSample;
                outputSource.Stop();

                if (outputSource.clip != null)
                    Destroy(outputSource.clip);

                outputSource.clip = AudioClip.Create("VOICE", sampleRate * 2, 1, (int)sampleRate, true, OnAudioRead);
                outputSource.Play();
            }
        }

        /// <summary>
        /// Players a recieved Steamworks Voice package through the <see cref="outputSource"/> <see cref="AudioSource"/>.
        /// </summary>
        /// <param name="buffer"></param>
        public void PlayVoiceData(byte[] buffer)
        {
            uint bytesWritten;
            byte[] destBuffer = new byte[20000];
            var result = API.Voice.Client.DecompressVoice(buffer, destBuffer, out bytesWritten, (uint)sampleRate);
            var timeStamp = DateTime.Now;

            if (result == EVoiceResult.k_EVoiceResultBufferTooSmall)
            {
                destBuffer = new byte[bytesWritten];
                result = API.Voice.Client.DecompressVoice(buffer, destBuffer, out bytesWritten, (uint)sampleRate);
            }

            //Handle audio encoding result == EVoiceResult.k_EVoiceResultOK && 
            if (bytesWritten > 0)
            {
                //We are currently playing so enqueue this data and let the reader handle it
                for (int i = 0; i < bytesWritten; i += 2)
                {
                    audioBuffer.Enqueue((short)(destBuffer[i] | destBuffer[i + 1] << 8) / 32768f);
                }

                var clip = (DateTime.Now - timeStamp).TotalMilliseconds;
                if (clip > encodingTime)
                    encodingTime = clip;
            }
            else
            {
                Debug.LogWarning("Unknown result message: " + result.ToString());
            }
        }

        private void OnAudioRead(float[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (audioBuffer.Count > 0)
                {
                    //If we have data write it
                    data[i] = audioBuffer.Dequeue();
                }
                else
                {
                    //If we don't write silence
                    data[i] = 0;
                }
            }
        }
    }

}
#endif