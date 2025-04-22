#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Steam User Voice features
    /// </summary>
    public static class Voice
    {
        public static class Client
        {
            public static uint OptimalSampleRate => SteamUser.GetVoiceOptimalSampleRate();

            /// <summary>
            /// Decodes the compressed voice data returned by GetVoice.
            /// </summary>
            /// <param name="compressedData">The compressed data received from GetVoice.</param>
            /// <param name="resultBuffer">The buffer where the raw audio data will be returned. This can then be passed to your audio subsystems for playback.</param>
            /// <param name="resultsWrittenSize">Returns the number of bytes written to pDestBuffer, or size of the buffer required to decompress the given data if cbDestBufferSize is not large enough (and k_EVoiceResultBufferTooSmall is returned).</param>
            /// <param name="desiredSampleRate">The sample rate that will be returned. This can be from 11025 to 48000, you should either use the rate that works best for your audio playback system, which likely takes the users audio hardware into account, or you can use GetVoiceOptimalSampleRate to get the native sample rate of the Steam voice decoder.</param>
            /// <returns></returns>
            public static EVoiceResult DecompressVoice(byte[] compressedData, byte[] resultBuffer, out uint resultsWrittenSize, uint desiredSampleRate) => SteamUser.DecompressVoice(compressedData, (uint)compressedData.Length, resultBuffer, (uint)resultBuffer.Length, out resultsWrittenSize, desiredSampleRate);
            /// <summary>
            /// Checks to see if there is captured audio data available from GetVoice, and gets the size of the data.
            /// </summary>
            /// <param name="pcbCompressed">Returns the size of the available voice data in bytes.</param>
            /// <returns></returns>
            public static EVoiceResult GetAvailableVoice(out uint pcbCompressed) => SteamUser.GetAvailableVoice(out pcbCompressed);
            /// <summary>
            /// Read captured audio data from the microphone buffer.
            /// </summary>
            /// <param name="pDestBuffer">The buffer where the audio data will be copied into.</param>
            /// <param name="nBytesWritten">Returns the number of bytes written into pDestBuffer. This should always be the size returned by ISteamUser::GetAvailableVoice.</param>
            /// <returns></returns>
            public static EVoiceResult GetVoice(byte[] pDestBuffer, out uint nBytesWritten) => SteamUser.GetVoice(true, pDestBuffer, (uint)pDestBuffer.Length, out nBytesWritten);
            /// <summary>
            /// Starts voice recording.
            /// </summary>
            public static void StartRecording() => SteamUser.StartVoiceRecording();
            /// <summary>
            /// Stops voice recording.
            /// </summary>
            public static void StopRecording() => SteamUser.StopVoiceRecording();
        }
    }
}
#endif