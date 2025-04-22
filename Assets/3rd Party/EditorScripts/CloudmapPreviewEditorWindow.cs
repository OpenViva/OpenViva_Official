using OccaSoftware.Altos.Runtime;

using UnityEditor;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace OccaSoftware.Altos.Editor
{
    public class CloudmapPreviewEditorWindow : EditorWindow
    {
        Image img;
        bool showRed = true;
        bool showGreen = true;
        bool showBlue = true;

        private GUIStyle onButtonStyle;
        private GUIStyle offButtonStyle;

        [MenuItem("Tools/OccaSoftware/Altos/Cloudmap Preview")]
        public static void Open()
        {
            CloudmapPreviewEditorWindow wnd = GetWindow<CloudmapPreviewEditorWindow>();
            wnd.titleContent = new GUIContent("Cloudmap Preview");
        }

        private void Update()
        {
            t = Shader.GetGlobalTexture("altos_cloud_map");
            Repaint();
        }

        Texture t;

        private void OnGUI()
        {
            t = Shader.GetGlobalTexture("altos_cloud_map");

            if (onButtonStyle == null)
            {
                // Create a custom style for the on state buttons
                onButtonStyle = new GUIStyle(GUI.skin.button);
                onButtonStyle.padding = new RectOffset(10, 10, 5, 5);
                onButtonStyle.margin = new RectOffset(5, 5, 5, 5);
                onButtonStyle.normal.textColor = Color.white;
                onButtonStyle.hover.textColor = Color.white;
                onButtonStyle.active.textColor = Color.white;
                onButtonStyle.onActive.textColor = Color.white;
                onButtonStyle.alignment = TextAnchor.MiddleLeft;
                onButtonStyle.normal.background = MakeTex(2, 2, new Color(0.2f, 0.2f, 0.2f, 1f));
                onButtonStyle.hover.background = MakeTex(2, 2, new Color(0.4f, 0.4f, 0.4f, 1f));
                onButtonStyle.active.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 1f));
                onButtonStyle.border = new RectOffset(1, 1, 1, 1);
            }

            if (offButtonStyle == null)
            {
                // Create a custom style for the off state buttons
                offButtonStyle = new GUIStyle(GUI.skin.button);
                offButtonStyle.padding = new RectOffset(10, 10, 5, 5);
                offButtonStyle.margin = new RectOffset(5, 5, 5, 5);
                offButtonStyle.normal.textColor = Color.gray;
                offButtonStyle.hover.textColor = Color.gray;
                offButtonStyle.active.textColor = Color.gray;
                offButtonStyle.onActive.textColor = Color.gray;
                offButtonStyle.alignment = TextAnchor.MiddleLeft;
                offButtonStyle.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 1f));
                offButtonStyle.hover.background = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f, 1f));
                offButtonStyle.active.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 1f));
                offButtonStyle.border = new RectOffset(1, 1, 1, 1);
            }

            GUILayout.BeginHorizontal(); // Begin horizontal layout

            // Create custom-styled toggle buttons for Red
            if (GUILayout.Button("Coverage", showRed ? onButtonStyle : offButtonStyle))
            {
                showRed = !showRed;
            }

            // Create custom-styled toggle buttons for Green
            if (GUILayout.Button("Precipitation", showGreen ? onButtonStyle : offButtonStyle))
            {
                showGreen = !showGreen;
            }

            // Create custom-styled toggle buttons for Blue
            if (GUILayout.Button("Type", showBlue ? onButtonStyle : offButtonStyle))
            {
                showBlue = !showBlue;
            }

            GUILayout.EndHorizontal(); // End horizontal layout

            if (t)
            {
                ColorWriteMask colorMask = 0;
                if (showRed)
                    colorMask |= ColorWriteMask.Red;
                if (showGreen)
                    colorMask |= ColorWriteMask.Green;
                if (showBlue)
                    colorMask |= ColorWriteMask.Blue;

                // Calculate the scaled size to fit the window
                Rect imageRect = new Rect(5, 40, position.width - 10, position.height - 50); // Adjust margin and position

                if (position.width < t.width)
                {
                    float scale = imageRect.width / t.width;
                    imageRect.width = t.width * scale;
                }

                if (position.height < t.height)
                {
                    float scale = imageRect.height / t.height;
                    imageRect.height = t.height * scale;
                }
                EditorGUI.DrawPreviewTexture(imageRect, t, null, ScaleMode.ScaleToFit, 0, 0, colorMask);
            }
        }

        // Helper method to create a colored texture for button backgrounds
        private Texture2D MakeTex(int width, int height, Color color)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = color;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
