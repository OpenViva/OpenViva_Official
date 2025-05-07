using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Viva
{

    public enum VolumeType
    {
        Master,
        Music,
        Sfx,
        Voice
    }
    
    public class VolumeAdjuster : MonoBehaviour
    {
        public VolumeType volumeType; //(Master, Music, SFX, Voice)

        [SerializeField] private Text volumeText;

        public void clickShiftVolume(float amount)
        {
            float currentVolume = GetCurrentVolume(volumeType);
            float newVolume = Mathf.Clamp(currentVolume + amount, 0f, 1f);

            ApplyAndUpdateVolume(newVolume, volumeType);
        }

        private void ApplyAndUpdateVolume(float volume, VolumeType volumeType)
        {
            switch (volumeType)
            {
                case VolumeType.Master:
                    GameSettings.main.ApplyVolumeSetting(volume, "MasterVolume", value => GameSettings.main.masterVolume = value);
                    break;
                case VolumeType.Music:
                    GameSettings.main.ApplyVolumeSetting(volume, "MusicVolume", value => GameSettings.main.musicVolume = value);
                    break;
                case VolumeType.Voice:
                    GameSettings.main.ApplyVolumeSetting(volume, "VoiceVolume", value => GameSettings.main.voiceVolume = value);
                    break;
                case VolumeType.Sfx:
                    GameSettings.main.ApplyVolumeSetting(volume, "SfxVolume", value => GameSettings.main.sfxVolume = value);
                    break;
                default:
                    Debug.LogWarning($"Unknown volume type: {volumeType}");
                    return;
            }

            if (volumeText != null)
            {
                volumeText.text = $"{(volume * 100f):0}%";
            }
        }

        private float GetCurrentVolume(VolumeType volumeType)
        {
            switch (volumeType)
            {
                case VolumeType.Master:
                    return GameSettings.main.masterVolume;
                case VolumeType.Music:
                    return GameSettings.main.musicVolume;
                case VolumeType.Voice:
                    return GameSettings.main.voiceVolume;
                case VolumeType.Sfx:
                    return GameSettings.main.sfxVolume;
                default:
                    Debug.LogWarning($"Unknown volume type: {volumeType}");
                    return 0f; // Default volume if unknown
            }
        }
    }

}