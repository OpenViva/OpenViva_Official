using System.IO;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    [SerializeField] private GameSettingsData gameSettingsData;
    [SerializeField] private string savePath;

    private void InitializeSavePath()
    {
        savePath = Path.Combine(Application.persistentDataPath, "GameSettings.json");
    }

    public void ApplySettings()
    {
        QualitySettings.SetQualityLevel(gameSettingsData.qualityLevel, false);
        // QualitySettings.antiAliasing = antiAliasing;
        QualitySettings.vSyncCount = gameSettingsData.vSync ? 1 : 0;
        QualitySettings.lodBias = gameSettingsData.lodDistance;
        Application.targetFrameRate = gameSettingsData.vSync ? -1 : gameSettingsData.fpsLimit;
        Screen.fullScreen = gameSettingsData.fullscreen;
    }
}
