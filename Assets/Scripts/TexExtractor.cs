using System;
using System.IO;
using UnityEngine;

public class TexExtractor : MonoBehaviour
{
    private void Start()
    {
        // LoadAndSaveTextureAsPng("towel_BaseColorMap");
        // LoadAndSaveTextureAsPng("towel_MaskMap");
        // LoadAndSaveTextureAsPng("towel_NormalMap");
    }

    public void LoadAndSaveTextureAsPng(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Cannot load texture from a null filepath");
            return;
        }

        // Ensure the file has the correct extension
        if (Path.GetExtension(path) != ".tex") path += ".tex";

        path = Path.GetFullPath(Path.Combine("Assets", path));

        // Load raw texture from hard drive
        try
        {
            using (var stream = File.Open(path, FileMode.Open))
            {
                var bw = new BinaryReader(stream);
                var transparent = bw.ReadBoolean();
                int resolution = bw.ReadInt32();
                int byteCount = bw.ReadInt32();
                byte[] bytes = bw.ReadBytes(byteCount);
                var textureName = bw.ReadString();
                Debug.Log("+Texture " + textureName + " size:" + byteCount + " transparent:" + transparent);

                TextureFormat format = transparent ? TextureFormat.DXT5 : TextureFormat.DXT1;
                Texture2D texture = new Texture2D(resolution, resolution, format, true);
                texture.LoadRawTextureData(bytes);
                texture.Apply();

                // Workaround to handle compressed texture formats
                RenderTexture renderTex = RenderTexture.GetTemporary(resolution, resolution);
                Graphics.Blit(texture, renderTex);
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = renderTex;
                Texture2D uncompressedTexture = new Texture2D(resolution, resolution);
                uncompressedTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
                uncompressedTexture.Apply();
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(renderTex);

                byte[] pngBytes = uncompressedTexture.EncodeToPNG();
                File.WriteAllBytes(Path.Combine(Application.dataPath, textureName + ".png"), pngBytes);

                Debug.Log("Texture saved as PNG: " + textureName + ".png");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Could not load texture \"" + path + "\": " + e.Message);
        }
    }
}