using System.IO;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MapCapture : MonoBehaviour
{
    [Header("Capture Settings")]
    [Tooltip("Resolution of the output map image")]
    public int imageWidth = 2048;
    public int imageHeight = 2048;

    [Tooltip("Name of the file saved in your Assets folder")]
    public string fileName = "WorldMap.png";

    [ContextMenu("Capture Map to PNG")]
    public void CaptureMap()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("MapCapture: No Camera component found on this GameObject.");
            return;
        }

        // 1. Disable fog
        bool oldFogState = RenderSettings.fog;
        RenderSettings.fog = false;

        // 2. Create a temporary render texture
        RenderTexture rt = new RenderTexture(imageWidth, imageHeight, 24);
        cam.targetTexture = rt;

        // 3. Trigger the camera render
        cam.Render();

        // 4. Read the pixels from the render texture into a Texture2D
        RenderTexture.active = rt;
        Texture2D mapTexture = new(imageWidth, imageHeight, TextureFormat.RGB24, false);
        mapTexture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
        mapTexture.Apply();

        // 5. Clean up references and temporary render texture
        cam.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);

        // 6. Restore the original fog state
        RenderSettings.fog = oldFogState;

        // 7. Save the texture as a PNG file inside the Assets folder
        byte[] bytes = mapTexture.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, fileName);
        File.WriteAllBytes(path, bytes);

        DestroyImmediate(mapTexture);

        Debug.Log($"Map successfully captured and saved to: {path}");

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh(); // Refreshes Unity editor so the image appears
#endif
    }
}