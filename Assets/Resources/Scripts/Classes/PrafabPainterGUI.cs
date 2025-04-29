
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.Text;

namespace nTools.PrefabPainter
{

    //
    // class Strings
    //
    static public class Strings
    {
        public static GUIContent    windowTitle                 = new GUIContent("Prefab Painter", "Prefab placement tool");


        public static GUIContent    brushName                   = new GUIContent("Brush Name", "Brush name");
        public static GUIContent    brushRadius                 = new GUIContent("Radius", "Brush radius in world units");
        public static GUIContent    brushSpacing                = new GUIContent("Spacing", "");

        public static GUIContent    brushPositionOffset         = new GUIContent("Surface Offset", "");

        public static GUIContent    brushOrientationTransform   = new GUIContent("Transform Mode", "");
        public static GUIContent    brushOrientationMode        = new GUIContent("Orientation", "");
        public static GUIContent    brushFlipOrientation        = new GUIContent("Flip Orientation", "");
        public static GUIContent    brushRotation               = new GUIContent("Aux Rotation", "");
        public static GUIContent    brushRandomizeOrientationX  = new GUIContent("Randomize X %", "");
        public static GUIContent    brushRandomizeOrientationY  = new GUIContent("Randomize Y %", "");
        public static GUIContent    brushRandomizeOrientationZ  = new GUIContent("Randomize Z %", "");

        public static GUIContent    brushScaleTransformMode     = new GUIContent("Transform Mode", "");
        public static GUIContent    brushScaleMode              = new GUIContent("Mode", "");
        public static GUIContent    brushScaleUniformMin        = new GUIContent("Min", "");
        public static GUIContent    brushScaleUniformMax        = new GUIContent("Max", "");
        public static GUIContent    brushScalePerAxisMin        = new GUIContent("Min", "");
        public static GUIContent    brushScalePerAxisMax        = new GUIContent("Max", "");

        public static GUIContent    brushPaintOnSelected        = new GUIContent("Paint On Selected Only", "");
        public static GUIContent    brushPaintOnLayers          = new GUIContent("Paint On Layers", "");
        public static GUIContent    brushIgnoreLayers           = new GUIContent("Ignore Layers", "");
        public static GUIContent    brushPlaceUnder             = new GUIContent("Place Under", "");
        public static GUIContent    brushCustomSceneObject      = new GUIContent("Custom Scene Object", "");
        public static GUIContent    brushGroupPrefabs           = new GUIContent("Group Prefabs", "");
        public static GUIContent    brushOverwritePrefabLayer   = new GUIContent("Overwrite Prefab Layer", "");
        public static GUIContent    brushPrefabPlaceLayer       = new GUIContent("Prefab Place Layer", "");


        public static GUIContent    settingsMaxBrushRadius      = new GUIContent("Max Brush Size", "");
        public static GUIContent    settingsMaxBrushSpacing     = new GUIContent("Max Brush Spacing", "");
        public static GUIContent    settingsPrecisePlaceSnapAngle = new GUIContent("Precise Place Snap Angle", "");
        public static GUIContent    settingsSurfaceCoords       = new GUIContent("Surface Coords", "");
        public static GUIContent    settingsHideSceneSettingsObject = new GUIContent("Hide Scene Settings Object", "");
        public static GUIContent    settingsGroupName           = new GUIContent("Group Name Prefix", "");

        public static string[]      colorTagNames               = Enum.GetNames (typeof (ColorTag));


        public static string        brushSettingsFoldout        = "Brush Settings";
        public static string        positionSettingsFoldout     = "Position";
        public static string        orientationSettingsFoldout  = "Orientation";
        public static string        scaleSettingsFoldout        = "Scale";
        public static string        commonSettingsFoldout       = "Common Settings";

        public static string        settingsLabel               = "Settings";
        public static string        helpLabel                   = "Help";


        public static string[]      selectModeNames             = Enum.GetNames (typeof (SelectMode));

        public static string[]      orientationModeNames        = { "Surface", "X", "Y", "Z", "-X", "-Y", "-Z" };

        public static string[]      axisNames                   = { "X", "Y", "Z", "-X", "-Y", "-Z" };

        public static string        undoAddBrush                = "PP: Add Brush";
        public static string        undoRelinkPrefab            = "PP: Relink Brush Prefab";
        public static string        undoPaintPrefabs            = "PP: Paint Prefabs";
        public static string        undoMoveBrushes             = "PP: Move Brush(es)";
        public static string        undoDuplicateBrushes        = "PP: Duplicate Brush(es)";
        public static string        undoDeleteBrushes           = "PP: Delete Brush(es)";
        public static string        undoResetBrushes            = "PP: Reset Brush(es)";
        public static string        undoPasteBrushSettings      = "PP: Paste Brush Settings";
        public static string        undoAddTab                  = "PP: Add Tab";
        public static string        undoDeleteTab               = "PP: Delete Tab";

        public static string        dragNDropHere               = "Drag & Drop Prefab Here";
        public static string        missingPrefab               = "Missing";
        public static string        shiftDragRelink             = "Shift+Drag\nRelink";
        public static string        selectBrush                 = "Select Brush";
        public static string        multiSelBrush               = "Multiple Selected Brushes";
        public static string        newSettingsName             = "Untitled Settings";
        public static string        newTabName                  = "New Tab";


        public static string helpText =
            "Shortcuts:\n" +
            "    Shift+drag\t- Relink prefab\n" +
            "    []\t\t- Change brush size\n" +
            "    ESC\t\t- Abort paint\n" +
            "    F\t\t- Frame camera on brush\n";// +
            //"    ; '\t\t- Change grid offet (in Grid mode)\n" +
            //"    , .\t\t- Change grid size (in Grid mode)\n";

    }


    //
    // class Styles
    //
    static public class Styles
    {
        public static GUIStyle		logoFont;

        public static GUIStyle      iconLabelText;

        public static GUIStyle      tabLabelText;
        public static GUIStyle      tabButton;
        public static Color         tabTintColor;

        public static GUIStyle      multibrushIconText;

        public static GUIStyle      buttonLeft  = "buttonleft";
        public static GUIStyle      buttonMid   = "buttonmid";
        public static GUIStyle      buttonRight = "buttonright";

        public static GUIStyle      boldFoldout;
        public static GUIStyle      handlesBoldTextStyle;
        public static GUIStyle      handlesTextStyle;
        public static GUIStyle      toolbarSeachCancelButtonStyle = "ToolbarSearchCancelButton";

        public static Color         colorBlue = new Color32 (62, 125, 231, 255);
        public static Color         backgroundColor;

        public static GUIStyle      leftGreyMiniLabel;

        public static Color         colorTagRed     = new Color32 (232, 19,  19, 255);
        public static Color         colorTagOrange  = new Color32 (251, 138,  0, 255);
        public static Color         colorTagYellow  = new Color32 (255, 223,  0, 255);
        public static Color         colorTagGreen   = new Color32 (61,  200, 53, 255);
        public static Color         colorTagBlue    = new Color32 (20,  130, 224, 255);
        public static Color         colorTagViolet  = new Color32 (155, 86,  251, 255);

        public static Color         foldoutTintColor;
        public static Color         iconTintColor;

        public static GUIStyle      miniHorizontalScrollbar;
        public static GUIStyle      miniHorizontalScrollbarThumb;


        static Styles()
        {
            logoFont = new GUIStyle(EditorStyles.label);
            logoFont.alignment = TextAnchor.MiddleCenter;
            logoFont.fontSize = 20;

            backgroundColor = EditorGUIUtility.isProSkin
                ? new Color32(41, 41, 41, 255)
                : new Color32(162, 162, 162, 255);


            iconLabelText = new GUIStyle(EditorStyles.miniLabel);
            iconLabelText.alignment = TextAnchor.LowerCenter;
            iconLabelText.clipping  = TextClipping.Clip;


            tabLabelText = new GUIStyle(EditorStyles.label);
            tabLabelText.alignment = TextAnchor.MiddleCenter;
            tabLabelText.clipping  = TextClipping.Clip;
            tabLabelText.fontStyle = FontStyle.Normal;

            multibrushIconText = new GUIStyle(EditorStyles.miniLabel);

            tabButton = new GUIStyle(EditorStyles.toolbarButton);
            tabButton.fixedHeight = 0;
            tabButton.fixedWidth = 0;

            tabTintColor = EditorGUIUtility.isProSkin
                ? new Color (1f, 1f, 1f, 0.1f)
                : new Color (0f, 0f, 0f, 0.1f);



            if(buttonLeft == null || buttonMid == null || buttonRight == null)
            {
                buttonLeft  = new GUIStyle(EditorStyles.miniButtonLeft);
                buttonMid   = new GUIStyle(EditorStyles.miniButtonMid);
                buttonRight = new GUIStyle(EditorStyles.miniButtonRight);
            }


            boldFoldout = new GUIStyle(EditorStyles.foldout);
            boldFoldout.fontStyle = FontStyle.Bold;

            handlesBoldTextStyle = new GUIStyle(EditorStyles.largeLabel);
            handlesBoldTextStyle.normal.textColor = Color.red;
            handlesBoldTextStyle.fontStyle = FontStyle.Bold;
            handlesBoldTextStyle.fontSize = 18;

            handlesTextStyle = new GUIStyle(EditorStyles.largeLabel);
            handlesTextStyle.normal.textColor = Color.red;

            if (toolbarSeachCancelButtonStyle == null)
            {
                toolbarSeachCancelButtonStyle = EditorStyles.miniButton;
            }

            leftGreyMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            leftGreyMiniLabel.alignment = TextAnchor.MiddleLeft;


            foldoutTintColor = EditorGUIUtility.isProSkin
                ? new Color (1f, 1f, 1f, 0.05f)
                : new Color (0f, 0f, 0f, 0.05f);


            iconTintColor = EditorGUIUtility.isProSkin
                ? new Color(0.67f, 0.67f, 0.67f, 1f)
                : new Color(0.2f, 0.2f, 0.2f, 1f);


            miniHorizontalScrollbar = new GUIStyle(GUI.skin.horizontalScrollbar);
            miniHorizontalScrollbar.fixedWidth = 0f;
            miniHorizontalScrollbar.fixedHeight = 0f;
            miniHorizontalScrollbarThumb = new GUIStyle(GUI.skin.horizontalScrollbarThumb);
            miniHorizontalScrollbarThumb.fixedWidth = 0f;
            miniHorizontalScrollbarThumb.fixedHeight = 0f;
        }
    }


    public static class InternalGUI
    {
        static int s_FlatButtonHash = "nTools.GUI.FlatButton".GetHashCode();
        static int s_ToggleHash = "nTools.GUI.Toggle".GetHashCode();
        static GUIContent[] s_XYLabels = { new GUIContent("X"), new GUIContent("Y") };
        static GUIContent[] s_XYZLabels = { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z") };
        static GUIContent[] s_MinMaxLabels = { new GUIContent(""), new GUIContent("") };
        static float[] s_Vector3Floats = new float[3];
        static float[] s_Vector2Floats = new float[2];

        public static bool Toggle(Rect rect, GUIContent content, bool value, GUIStyle style)
        {
            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(s_ToggleHash, FocusType.Passive, rect);

            switch (e.GetTypeForControl(controlID))
            {
            case EventType.MouseDown:
                if (rect.Contains(e.mousePosition) && e.button == 0)
                {
                    GUIUtility.keyboardControl = controlID;
                    GUIUtility.hotControl = controlID;
                    e.Use();
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && e.button == 0)
                {
                    GUI.changed = true;
                    value = !value;
                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;
            case EventType.Repaint:
                {
                    style.Draw(rect, content, GUI.enabled && GUIUtility.hotControl == controlID, GUI.enabled && GUIUtility.hotControl == controlID, value, false);
                }
                break;
            }

            return value;
        }

        public static int Toolbar(Rect rect, int tool, GUIContent[] items)
        {
            if (items.Length == 0)
                return tool;

            rect.width /= items.Length;

            Color contentColor = GUI.contentColor;
            GUI.contentColor = Styles.iconTintColor;

            for (int i = 0; i < items.Length; i++)
            {
                EditorGUI.BeginChangeCheck();
                Toggle(rect, items[i], i == tool, (i == 0) ? Styles.buttonLeft : (i == items.Length - 1 ? Styles.buttonRight : Styles.buttonMid));
                if (EditorGUI.EndChangeCheck())
                    tool = tool == i ? -1 : i;
                rect.x += rect.width;
            }

            GUI.contentColor = contentColor;
            return tool;
        }


        public static Vector2 Vector2Field(GUIContent label, Vector2 value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            Rect position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, EditorStyles.numberField);
            s_Vector2Floats[0] = value.x;
            s_Vector2Floats[1] = value.y;
            EditorGUI.MultiFloatField(position, s_XYLabels, s_Vector2Floats);
            EditorGUILayout.EndHorizontal();
            return new Vector2(s_Vector2Floats[0], s_Vector2Floats[1]);
        }

        public static Vector3 Vector3Field(GUIContent label, Vector3 value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            Rect position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, EditorStyles.numberField);
            s_Vector3Floats[0] = value.x;
            s_Vector3Floats[1] = value.y;
            s_Vector3Floats[2] = value.z;
            EditorGUI.MultiFloatField(position, s_XYZLabels, s_Vector3Floats);
            EditorGUILayout.EndHorizontal();
            return new Vector3(s_Vector3Floats[0], s_Vector3Floats[1], s_Vector3Floats[2]);
        }


        public static void Vector2MinMaxField(GUIContent label, SerializedProperty minProperty, SerializedProperty maxProperty)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            Rect position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, EditorStyles.numberField);
            s_Vector2Floats[0] = minProperty.floatValue;
            s_Vector2Floats[1] = maxProperty.floatValue;
            EditorGUI.BeginChangeCheck();
            EditorGUI.MultiFloatField(position, s_MinMaxLabels, s_Vector2Floats);
            if (EditorGUI.EndChangeCheck())
            {
                minProperty.floatValue = s_Vector2Floats[0];
                maxProperty.floatValue = s_Vector2Floats[1];
            }

            EditorGUILayout.EndHorizontal();
        }

        public static bool FlatTexturedButton(Rect rect, GUIContent content)
        {
            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(s_FlatButtonHash, FocusType.Passive, rect);
            bool clicked = false;

            switch (e.type)
            {
            case EventType.MouseDown:
                if (rect.Contains(e.mousePosition) && e.button == 0)
                {
                    GUIUtility.keyboardControl = controlID;
                    GUIUtility.hotControl = controlID;
                    e.Use();
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && e.button == 0)
                {
                    GUI.changed = clicked = true;

                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;
            case EventType.Repaint:
                {
                    if (GUIUtility.hotControl == controlID)
                        EditorGUI.DrawRect(rect, Styles.colorBlue);

                    GUI.color = Styles.iconTintColor;
                    GUI.DrawTexture(new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4), content.image);
                    GUI.color = Color.white;
                }
                break;
            }

            return clicked;
        }



        public static bool Foldout(bool foldout, string content)
        {
            Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, EditorStyles.foldout);

            EditorGUI.DrawRect(EditorGUI.IndentedRect(rect), Styles.foldoutTintColor);

            Rect foldoutRect = rect;
            foldoutRect.width = EditorGUIUtility.singleLineHeight;
            foldout = EditorGUI.Foldout(rect, foldout, "", true);

            rect.x += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(rect, content, EditorStyles.boldLabel);

            return foldout;
        }

        public static bool FoldoutToggle(bool foldout, SerializedProperty toggle, string content)
        {
            Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, EditorStyles.foldout);

            EditorGUI.DrawRect(EditorGUI.IndentedRect(rect), Styles.foldoutTintColor);

            Rect foldoutRect = rect;
            foldoutRect.width = EditorGUIUtility.singleLineHeight;

            Rect toggleRect = rect;
            toggleRect.x += EditorGUIUtility.singleLineHeight;
            toggleRect.width = EditorGUIUtility.singleLineHeight;
            toggle.boolValue = EditorGUI.Toggle(toggleRect, toggle.boolValue);


            foldout = EditorGUI.Foldout(rect, foldout, "", true);

            rect.x += EditorGUIUtility.singleLineHeight * 2;
            EditorGUI.LabelField(rect, content, EditorStyles.boldLabel);

            return foldout;
        }

        public static bool FoldoutReset(bool foldout, Action reset, string content, Texture2D resetIcon)
        {
            Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, EditorStyles.foldout);

            EditorGUI.DrawRect(EditorGUI.IndentedRect(rect), Styles.foldoutTintColor);

            Rect foldoutRect = rect;
            foldoutRect.width = EditorGUIUtility.singleLineHeight;

            Rect buttonRect = rect;
            buttonRect.x += rect.width - EditorGUIUtility.singleLineHeight;
            buttonRect.width = EditorGUIUtility.singleLineHeight;
            if (FlatTexturedButton(buttonRect, new GUIContent(resetIcon, "Reset")))
                reset.Invoke();

            foldout = EditorGUI.Foldout(rect, foldout, "", true);

            rect.x += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(rect, content, EditorStyles.boldLabel);

            return foldout;
        }

        public static bool FoldoutToggleReset(bool toggle, ref bool foldout, Action reset, string content, Texture2D resetIcon)
        {
            Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, EditorStyles.foldout);

            EditorGUI.DrawRect(EditorGUI.IndentedRect(rect), Styles.foldoutTintColor);

            Rect foldoutRect = rect;
            foldoutRect.width = EditorGUIUtility.singleLineHeight;

            Rect toggleRect = rect;
            toggleRect.x += EditorGUIUtility.singleLineHeight;
            toggleRect.width = EditorGUIUtility.singleLineHeight;
            toggle = EditorGUI.Toggle(toggleRect, toggle);

            Rect buttonRect = rect;
            buttonRect.x += rect.width - EditorGUIUtility.singleLineHeight;
            buttonRect.width = EditorGUIUtility.singleLineHeight;
            if (FlatTexturedButton(buttonRect, new GUIContent(resetIcon, "Reset")))
                reset.Invoke();

            foldout = EditorGUI.Foldout(rect, foldout, "", true);

            rect.x += EditorGUIUtility.singleLineHeight * 2;
            EditorGUI.LabelField(rect, content, EditorStyles.boldLabel);

            return toggle;
        }

        public static LayerMask LayerMaskField(GUIContent label, LayerMask layerMask)
        {
            List<string> layers = new List<string>(32);
            List<int> layerNumbers = new List<int>(32);

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) != 0)
                    maskWithoutEmpty |= (1 << i);
            }

            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) != 0)
                    mask |= (1 << layerNumbers[i]);
            }
            layerMask.value = mask;
            return layerMask;
        }

        public static bool Button(Rect position, GUIContent content, bool pressed)
        {
            return GUI.Toggle(position, pressed, content, "button") != pressed;
        }
    }



    //
    // class BrushPresetDatabase
    //
    public class BrushPresetDatabase
    {
        public string 		   m_PresetsDirectory = "";
        public List<string>    m_Presets = new List<string>();
        static string 		   kPresetFileExtension = ".json";

        public BrushPresetDatabase(string presetsDirectory)
        {
            this.m_PresetsDirectory = presetsDirectory;
            Refresh ();
        }

        public void Refresh()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(m_PresetsDirectory);

            m_Presets.Clear();

            foreach(FileInfo fileInfo in directoryInfo.GetFiles())
            {
                if(String.Compare(fileInfo.Extension, kPresetFileExtension, true) == 0)
                {
                    m_Presets.Add(Path.GetFileNameWithoutExtension(fileInfo.FullName));
                }
            }
        }

        public void SavePreset(string name, BrushSettings settings)
        {
            string filePath = Path.Combine(m_PresetsDirectory, name + kPresetFileExtension);

            string xml = JsonUtility.ToJson(settings, true);
            if(xml != null)
            {
                File.WriteAllText(filePath, xml);
                AssetDatabase.Refresh();
                Refresh();
            }
        }

        public BrushSettings LoadPreset(string name)
        {
            string text = File.ReadAllText (Path.Combine(m_PresetsDirectory, name + kPresetFileExtension));
            return JsonUtility.FromJson<BrushSettings>(text);
        }

        public void DeletePreset(string name)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(m_PresetsDirectory);

            foreach(FileInfo fileInfo in directoryInfo.GetFiles())
            {
                if(String.Compare(fileInfo.Extension, kPresetFileExtension, true) == 0)
                {
                    if(Path.GetFileNameWithoutExtension(fileInfo.FullName) == name)
                    {
                        string filePath = fileInfo.FullName.Replace(Application.dataPath, "Assets");

                        File.Delete(filePath);
                        AssetDatabase.Refresh();
                        Refresh();
                    }
                }
            }
        }

        public void DeleteAll()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(m_PresetsDirectory);

            foreach(FileInfo fileInfo in directoryInfo.GetFiles())
            {
                if(String.Compare(fileInfo.Extension, kPresetFileExtension, true) == 0)
                {
                    string filePath = fileInfo.FullName.Replace(Application.dataPath, "Assets");

                    File.Delete(filePath);
                    AssetDatabase.Refresh();
                    Refresh();
                }
            }
        }
    }



    //
    // class PrefabPainter
    //
    // GUI implementation
    //
    //
    public partial class PrefabPainter : EditorWindow
    {

        //
        // class InternalDragAndDrop
        //
        static class InternalDragAndDrop
        {
            enum State
            {
                None,
                DragPrepare,
                DragReady,
                Dragging,
                DragPerform
            }

            static object          m_DragData = null;
            static Vector2         m_MouseDownPosition;
            static State           m_State = State.None;
            const float            kDragStartDistance = 7.0f;


            public static void OnBeginGUI()
            {
                Event e = Event.current;

                switch(m_State)
                {
                case State.None:
                    {
                        if (e.type == EventType.MouseDown && e.button == 0)
                        {
                            m_MouseDownPosition = e.mousePosition;
                            m_State = State.DragPrepare;
                        }
                    }
                    break;
                case State.DragPrepare:
                    {
                        if (e.type == EventType.MouseUp && e.button == 0)
                        {
                            m_State = State.None;
                        }
                    }
                    break;
                case State.DragReady:
                    {
                        if (e.type == EventType.MouseUp && e.button == 0)
                        {
                            m_State = State.None;
                        }
                    }
                    break;
                case State.Dragging:
                    {
                        if (e.type == EventType.MouseUp && e.button == 0)
                        {
                            m_State = State.DragPerform;
                            e.Use();
                        }

                        if (e.type == EventType.MouseDrag)
                        {
                            e.Use();
                        }
                    }
                    break;
                }
            }


            public static void OnEndGUI()
            {
                Event e = Event.current;

                switch(m_State)
                {
                case State.DragReady:
                    if (e.type == EventType.Repaint)
                    {
                        m_State = State.None;
                    }
                    break;
                case State.DragPrepare:
                    if (e.type == EventType.MouseDrag &&
                        ((m_MouseDownPosition - e.mousePosition).magnitude > kDragStartDistance))
                    {
                        m_State = State.DragReady;
                    }
                    break;
                case State.DragPerform:
                    {
                        if (e.type == EventType.Repaint)
                        {
                            m_DragData = null;
                            m_State = State.None;
                        }
                    }
                    break;
                }
            }



            public static bool IsDragReady()
            {
                return m_State == State.DragReady;
            }

            public static void StartDrag(object data)
            {
                if (data == null || m_State != State.DragReady)
                    return;

                m_DragData = data;
                m_State = State.Dragging;
            }

            public static bool IsDragging()
            {
                return m_State == State.Dragging;
            }

            public static bool IsDragPerform()
            {
                return m_State == State.DragPerform;
            }

            public static object GetData()
            {
                return m_DragData;
            }

            public static Vector2 DragStartPosition()
            {
                return m_MouseDownPosition;
            }

        }


        //
        // class AssetPreviewCacheController
        //
        public static class AssetPreviewCacheController
        {
            const int kMinCacheSize = 50;
            const int kMaxCacheSize = 1000;
            static int m_CurrentCacheSize = 50;
            static Dictionary<string, int> requests = new Dictionary<string, int>();

            public static void AddCacheRequest(string id, int count)
            {
                if(count < 0)
                    return;

                requests[id] = count;

                int requestCache = Mathf.Clamp(requests.Sum(x => x.Value), kMinCacheSize, kMaxCacheSize) + 10;

                if(requestCache > m_CurrentCacheSize)
                {
                    AssetPreview.SetPreviewTextureCacheSize(requestCache);
                    m_CurrentCacheSize = requestCache;
                }
            }
        }


        const float                 kUIRepaintInterval = 0.5f;
        float                       m_LastUIRepaintTime = 0;

        BrushPresetDatabase         m_PresetDatabase = null;

        string                      m_SearchString = "";
        Vector2                     m_WindowScrollPos;
        bool                        m_OnBeginTabRename = false;
        int                         m_MultibrushItemDoubleClicked = -1;


        Dictionary<string, string>  m_BrushShortNamesDictionary = new Dictionary<string, string>();
        Dictionary<string, string>  m_TabShortNamesDictionary = new Dictionary<string, string>();
        Dictionary<int, Vector2>    m_BrushWindowsScrollDictionary = new Dictionary<int, Vector2>();
        Dictionary<int, float>      m_MultibrushScrollDictionary = new Dictionary<int, float>();



        // Textures
        Texture2D                   m_AddIconTexture = null;
        Texture2D                   m_CloseIconTexture = null;
        Texture2D                   m_TagTexture = null;
        Texture2D                   m_ResetIconTexture = null;
        Texture2D                   m_LevelsIconTexture = null;
        GUIContent[]                m_ToolbarItems = null;

        Material                    m_AssetPreviewMaterial = null;
        bool                        m_AssetPreviewMaterialShaderError = false;
        bool m_DisableAssetPreviewLevelShader = false;


        static int                  s_TabsWindowHash = "nTools.PrefabPainter.GUI.TabsWindow".GetHashCode();
        static int                  s_BrushWindowHash = "nTools.PrefabPainter.GUI.BrushWindow".GetHashCode();
        static int                  s_BrushWindowResizeBarHash = "nTools.PrefabPainter.GUI.BrushWindowResize".GetHashCode();
        static int                  s_RaitingBarHash = "nTools.PrefabPainter.GUI.RatingBar".GetHashCode();
        static int                  s_MultibrushHash = "PPGUI.Multibrush".GetHashCode();
        static int                  s_BrushWindowObjectPickerHash = "nTools.PrefabPainter.GUIGUI.BrushWindow.ObjectPicker".GetHashCode();
        static int                  s_MultibrushObjectPickerHash = "nTools.PrefabPainter.GUIGUI.Multibrush.ObjectPicker".GetHashCode();
        static string               s_tabTextFieldName = "TabNameTextField";
        static float                s_HorizontalMiniScrollBar_GrabPosition;
        static int                  s_HorizontalMiniScrollBarHash = "nTools.PrefabPainter.GUI.HorizontalMiniScrollBar".GetHashCode();

//#pragma warning restore 0414


        // UI Settings
        const int                   kBrushWindowResizeBarHeight = 8;

        const int                   kBrushIconWidth  = 60;
        const int                   kBrushIconHeight = 72;

        float                       m_BrushIconScale = 1.0f;

        const int                   kTabWidth = 70;
        const int                   kTabHeight = 20;

        const int                   kMultibrushIconSize = 70;
        const int                   kMultibrushIconBorderSize = 2;
        const float                 kMultibrushRaitingBarHeightPercent = 0.1f;


        bool                        m_BrushToolFoldout       = true;
        bool                        m_PositionSettingsFoldout    = true;
        bool                        m_OrientationSettingsFoldout = true;
        bool                        m_ScaleSettingsFoldout       = true;
        bool                        m_PhysicsSettingsFoldout       = true;
        bool                        m_CommonSettingsFoldout      = true;
        bool                        m_MultibrushSettingsFoldout  = true;
        bool                        m_MultibrushSlotSettingsFoldout  = false;
        bool                        m_PivotEditorFoldout         = false;
        bool                        m_GridFoldout                = true;
        bool                        m_SlopeFilterFoldout         = true;
        bool                        m_PinToolFoldout             = true;
        bool                        m_PlaceToolFoldout           = true;
        bool                        m_ThrowToolFoldout = true;
        bool                        m_EraseToolFoldout           = true;
        bool                        m_SelectToolFoldout          = true;
        bool                        m_ModifyToolFoldout          = true;
        bool                        m_OrientToolFoldout          = true;
        bool                        m_MoveToolFoldout            = true;
        bool                        m_ToolSettingsFoldout        = true;
        bool                        m_HelpFoldout                = true;
        float                       m_BrushWindowHeight          = 225.0f;




        Texture2D LoadGUITexture(string filename)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath(Path.Combine(GetGUIDirectory(), filename), typeof(Texture2D)) as Texture2D;
            if(texture != null)
                return texture;
            return Texture2D.whiteTexture;
        }

        void OnInitGUI()
        {

            m_BrushToolFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.brushSettingsFoldout", m_BrushToolFoldout);
            m_PositionSettingsFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.positionSettingsFoldout", m_PositionSettingsFoldout);
            m_OrientationSettingsFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.orientationSettingsFoldout", m_OrientationSettingsFoldout);
            m_ScaleSettingsFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.scaleSettingsFoldout", m_ScaleSettingsFoldout);
            m_PhysicsSettingsFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.physicsSettingsFoldout", m_PhysicsSettingsFoldout);
            m_CommonSettingsFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.commonSettingsFoldout", m_CommonSettingsFoldout);
            m_MultibrushSettingsFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.multibrushSettingsFoldout", m_MultibrushSettingsFoldout);
            m_MultibrushSlotSettingsFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.multibrushSlotSettingsFoldout", m_MultibrushSlotSettingsFoldout);
            m_PivotEditorFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.pivotEditorFoldout", m_PivotEditorFoldout);
            m_GridFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.gridFoldout", m_GridFoldout);
            m_SlopeFilterFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.slopeFilterFoldout", m_SlopeFilterFoldout);
            m_PinToolFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.pinToolFoldout", m_PinToolFoldout);
            m_PlaceToolFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.placeToolFoldout", m_PlaceToolFoldout);
            m_ThrowToolFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.throwToolFoldout", m_ThrowToolFoldout);
            m_EraseToolFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.eraseSettingsFoldout", m_EraseToolFoldout);
            m_SelectToolFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.selectSettingsFoldout", m_SelectToolFoldout);
            m_ModifyToolFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.modifyToolFoldout", m_SelectToolFoldout);
            m_OrientToolFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.orientToolFoldout", m_OrientToolFoldout);
            m_MoveToolFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.moveToolFoldout", m_MoveToolFoldout);
            m_ToolSettingsFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.toolSettingsFoldout", m_ToolSettingsFoldout);
            m_HelpFoldout = EditorPrefs.GetBool("nTools.PrefabPainter.helpFoldout", m_HelpFoldout);
            m_BrushWindowHeight = EditorPrefs.GetFloat("nTools.PrefabPainter.brushWindowHeight", m_BrushWindowHeight);
            m_BrushIconScale = EditorPrefs.GetFloat("nTools.PrefabPainter.brushIconScale", m_BrushIconScale);
            m_DisableAssetPreviewLevelShader = EditorPrefs.GetBool("nTools.PrefabPainter.disableAssetPreviewLevelShader", m_DisableAssetPreviewLevelShader);


            // Load presets
            m_PresetDatabase = new BrushPresetDatabase (GetPresetsDirectory ());


            // Load Textures
            string HiDPI = EditorGUIUtility.pixelsPerPoint > 1.8f ? "_HIDPI.png" : ".png";

            m_AddIconTexture = LoadGUITexture("AddIcon.png");
            m_CloseIconTexture = LoadGUITexture("CloseIcon.png");
            m_TagTexture = LoadGUITexture("TagIcon.png");
            m_LevelsIconTexture = LoadGUITexture("LevelsIcon.png");
            m_ResetIconTexture = LoadGUITexture("ResetIcon" + HiDPI);

            m_ToolbarItems = new GUIContent[] {
                new GUIContent("", LoadGUITexture("BrushTool" + HiDPI), "Brush Tool"),
                new GUIContent("", LoadGUITexture("PinTool" + HiDPI), "Pin Tool"),
                new GUIContent("", LoadGUITexture("PlaceTool" + HiDPI), "Place Tool"),
                new GUIContent("", LoadGUITexture("ThrowTool" + HiDPI), "Throw Tool"),
                new GUIContent("", LoadGUITexture("EraseTool" + HiDPI), "Erase Tool"),
                new GUIContent("", LoadGUITexture("SelectTool" + HiDPI), "Select Tool"),
                new GUIContent("", LoadGUITexture("MoveTool" + HiDPI), "Move Tool"),
                new GUIContent("", LoadGUITexture("ModifyTool" + HiDPI), "Modify Tool"),
                new GUIContent("", LoadGUITexture("OrientTool" + HiDPI), "Orient Tool"),
                new GUIContent("", LoadGUITexture("SettingsTool" + HiDPI), "Settings"),
            };

            if (EditorGUIUtility.isProSkin)
                titleContent = new GUIContent(Strings.windowTitle.text, LoadGUITexture("PPIconW.png"), "");
            else
                titleContent = new GUIContent(Strings.windowTitle.text, LoadGUITexture("PPIcon.png"), "");


            // load asset preview material
            if (m_AssetPreviewMaterial == null && !m_AssetPreviewMaterialShaderError)
            {
                Shader shader = Shader.Find("Hidden/nTools/PrefabPainter/GUITextureClipLevels");
                if (shader)
                {
#if UNITY_2019_1_OR_NEWER
                    if (ShaderUtil.ShaderHasError(shader) || m_DisableAssetPreviewLevelShader)
                    {
                        m_AssetPreviewMaterialShaderError = true;
                    }
                    else
#else
                    if (m_DisableAssetPreviewLevelShader)
                    {
                        m_AssetPreviewMaterialShaderError = true;
                    }
                    else
#endif
                    {
                        m_AssetPreviewMaterial = new Material(shader);
                        m_AssetPreviewMaterial.hideFlags = HideFlags.HideAndDontSave;
                    }
                }
            }



        }

        void OnCleanupGUI()
        {
            EditorPrefs.SetBool("nTools.PrefabPainter.brushSettingsFoldout", m_BrushToolFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.positionSettingsFoldout", m_PositionSettingsFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.orientationSettingsFoldout", m_OrientationSettingsFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.scaleSettingsFoldout", m_ScaleSettingsFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.physicsSettingsFoldout", m_PhysicsSettingsFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.commonSettingsFoldout", m_CommonSettingsFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.multibrushSettingsFoldout", m_MultibrushSettingsFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.multibrushSlotSettingsFoldout", m_MultibrushSlotSettingsFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.pivotEditorFoldout", m_PivotEditorFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.gridFoldout", m_GridFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.slopeFilterFoldout", m_SlopeFilterFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.pinToolFoldout", m_PinToolFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.placeToolFoldout", m_PlaceToolFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.throwToolFoldout", m_ThrowToolFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.eraseSettingsFoldout", m_EraseToolFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.selectSettingsFoldout", m_SelectToolFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.modifyToolFoldout", m_ModifyToolFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.orientToolFoldout", m_OrientToolFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.moveToolFoldout", m_MoveToolFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.toolSettingsFoldout", m_ToolSettingsFoldout);
            EditorPrefs.SetBool("nTools.PrefabPainter.helpFoldout", m_HelpFoldout);
            EditorPrefs.SetFloat("nTools.PrefabPainter.brushWindowHeight", m_BrushWindowHeight);
            EditorPrefs.SetFloat("nTools.PrefabPainter.brushIconScale", m_BrushIconScale);
            EditorPrefs.SetBool("nTools.PrefabPainter.disableAssetPreviewLevelShader", m_DisableAssetPreviewLevelShader);

        }



#region Context Menus

        GenericMenu PresetMenu()
        {
            GenericMenu menu = new GenericMenu();

            Tab activeTab = m_Settings.GetActiveTab();
            bool hasSelectedBrushes = activeTab.HasSelectedBrushes();
            bool hasMultipleSelectedBrushes = activeTab.HasMultipleSelectedBrushes();
            Brush brush = activeTab.GetFirstSelectedBrush();

            if(!hasSelectedBrushes)
                return menu;


            // Reveal in Project
            if(!hasMultipleSelectedBrushes && brush != null && !brush.settings.multibrushEnabled)
            {
                GameObject prefab = brush.GetFirstAssociatedPrefab();

                if(prefab != null)
                {
                    menu.AddItem(new GUIContent("Reveal in Project"), false, ContextMenuCallback, new Action(() => EditorGUIUtility.PingObject(prefab)));
                    menu.AddSeparator ("");
                }
            }

            // Copy/Paste/Delete/Duplicate
            menu.AddItem(new GUIContent("Cut %x"), false, ContextMenuCallback, new Action(() => { m_Settings.ClipboardCutBrushes(); }));
            menu.AddItem(new GUIContent("Copy %c"), false, ContextMenuCallback, new Action(() => { m_Settings.ClipboardCopyBrushes(); }));
            if(m_Settings.ClipboardIsCanPasteBrushes())
                menu.AddItem(new GUIContent("Paste %v"), false, ContextMenuCallback, new Action(() => { m_Settings.ClipboardPasteBrushes(); }));
            else
                menu.AddDisabledItem(new GUIContent("Paste %v"));
            menu.AddItem(new GUIContent("Duplicate %d"), false, ContextMenuCallback, new Action(() => activeTab.DuplicateSelectedBrushes()));
#if UNITY_EDITOR_OSX
            menu.AddItem(new GUIContent("Delete %\b"), false, ContextMenuCallback, new Action(() => activeTab.DeleteSelectedBrushes()));
#else
            menu.AddItem(new GUIContent("Delete #DEL"), false, ContextMenuCallback, new Action(() => activeTab.DeleteSelectedBrushes()));
#endif


            // Selection
            menu.AddSeparator ("");
            menu.AddItem(new GUIContent("Select All %a"), false, ContextMenuCallback, new Action(() => { m_Settings.GetActiveTab().SelectAllBrushes(); }));
            menu.AddItem(new GUIContent("Invert Selection"), false, ContextMenuCallback, new Action(() => { m_Settings.GetActiveTab().InvertSelection(); }));


            // Settings
            menu.AddSeparator ("");

            if(!hasMultipleSelectedBrushes)
                menu.AddItem(new GUIContent("Copy Settings"), false, ContextMenuCallback, new Action(() => m_Settings.ClipboardCopySettings()));
            if(m_Settings.ClipboardIsCanPasteSettings())
                menu.AddItem(new GUIContent("Paste Settings"), false, ContextMenuCallback, new Action(() => m_Settings.ClipboardPasteSettings()));
            else
                menu.AddDisabledItem(new GUIContent("Paste Settings"));



            // Apply Saved Presets
            m_PresetDatabase.Refresh();

            for (int i = 0; i < m_PresetDatabase.m_Presets.Count; i++)
            {
                string name = m_PresetDatabase.m_Presets[i];

                menu.AddItem(new GUIContent("Apply Settings/" + name), false, ContextMenuCallback, new Action(() => {
                    Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Apply Preset Settings");
                    activeTab.brushes.ForEach((b) =>    {
                        if(b.selected)
                            b.settings.CopyFrom(m_PresetDatabase.LoadPreset(name));
                    });
                }));
            }


            // Save/Delete Presets
            menu.AddSeparator ("Apply Settings/");
            if (!hasMultipleSelectedBrushes)
                menu.AddItem(new GUIContent("Apply Settings/Save Settings..."), false, ContextMenuCallback, new Action(() => SaveSettingsDialog.ShowDialog(m_PresetDatabase, m_Settings)));
            else
                menu.AddDisabledItem(new GUIContent("Apply Settings/Save Settings..."));
            menu.AddItem(new GUIContent("Apply Settings/Delete Settings..."), false, ContextMenuCallback, new Action(() => DeleteSettingsDialog.ShowDialog(m_PresetDatabase)));

            menu.AddItem(new GUIContent("Reset Settings"), false, ContextMenuCallback, new Action(() => activeTab.ResetSelectedBrushes()));




            // Extensions

            bool isAllMutibrush = true;
            bool isAllSlopeFilter = true;
            bool isAllGrid = true;

            activeTab.brushes.ForEach((b) => {
                if (b.selected) {
                    if (!b.settings.multibrushEnabled) isAllMutibrush = false;
                    if (!b.settings.slopeEnabled) isAllSlopeFilter = false;
                    if (!b.settings.gridEnabled) isAllGrid = false;
                }
            });

            menu.AddSeparator ("");
            menu.AddItem(new GUIContent("Multibrush"), isAllMutibrush, ContextMenuCallback, new Action(() => {
                Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Change Brush Settings");
                activeTab.brushes.ForEach((b) => { if (b.selected) b.settings.multibrushEnabled = !isAllMutibrush; });

            }));

            menu.AddItem(new GUIContent("Slope Filter"), isAllSlopeFilter, ContextMenuCallback, new Action(() => {
                Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Change Brush Settings");
                activeTab.brushes.ForEach((b) => { if (b.selected) b.settings.slopeEnabled = !isAllSlopeFilter; });

            }));
            menu.AddItem(new GUIContent("Grid"), isAllGrid, ContextMenuCallback, new Action(() => {
                Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Change Brush Settings");
                activeTab.brushes.ForEach((b) => { if (b.selected) b.settings.gridEnabled = !isAllGrid; });

            }));



            // Tags
            menu.AddSeparator ("");
            menu.AddItem(new GUIContent("Tag/None"),   !hasMultipleSelectedBrushes && brush.colorTag == ColorTag.None,   ContextMenuCallback, new Action(() => activeTab.SetSelectedBrushesTag(ColorTag.None)));
            menu.AddItem(new GUIContent("Tag/Red"),    !hasMultipleSelectedBrushes && brush.colorTag == ColorTag.Red,    ContextMenuCallback, new Action(() => activeTab.SetSelectedBrushesTag(ColorTag.Red)));
            menu.AddItem(new GUIContent("Tag/Orange"), !hasMultipleSelectedBrushes && brush.colorTag == ColorTag.Orange, ContextMenuCallback, new Action(() => activeTab.SetSelectedBrushesTag(ColorTag.Orange)));
            menu.AddItem(new GUIContent("Tag/Yellow"), !hasMultipleSelectedBrushes && brush.colorTag == ColorTag.Yellow, ContextMenuCallback, new Action(() => activeTab.SetSelectedBrushesTag(ColorTag.Yellow)));
            menu.AddItem(new GUIContent("Tag/Green"),  !hasMultipleSelectedBrushes && brush.colorTag == ColorTag.Green,  ContextMenuCallback, new Action(() => activeTab.SetSelectedBrushesTag(ColorTag.Green)));
            menu.AddItem(new GUIContent("Tag/Blue"),   !hasMultipleSelectedBrushes && brush.colorTag == ColorTag.Blue,   ContextMenuCallback, new Action(() => activeTab.SetSelectedBrushesTag(ColorTag.Blue)));
            menu.AddItem(new GUIContent("Tag/Violet"), !hasMultipleSelectedBrushes && brush.colorTag == ColorTag.Violet, ContextMenuCallback, new Action(() => activeTab.SetSelectedBrushesTag(ColorTag.Violet)));

            // Custom Thumbnail
            menu.AddSeparator ("");
            if(brush.customPreviewImage != null)
                menu.AddItem(new GUIContent("Remove Custom Thumbnail"), false, ContextMenuCallback, new Action(() =>
                {
                    Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Remove Custom Thumbnail");
                    brush.customPreviewImage = null;
                }));
            else
                menu.AddDisabledItem(new GUIContent("No Custom Thumbnail"));

            return menu;
        }


        GenericMenu BrushWindowMenu()
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Add Prefab"), false, ContextMenuCallback, new Action(() => { this.SendEvent(EditorGUIUtility.CommandEvent("BrushWindowShowObjectSelector")); }));

            menu.AddSeparator ("");

            if(m_Settings.ClipboardIsCanPasteBrushes())
                menu.AddItem(new GUIContent("Paste %v"), false, ContextMenuCallback, new Action(() => { m_Settings.ClipboardPasteBrushes(); }));
            else
                menu.AddDisabledItem(new GUIContent("Paste %v"));

            menu.AddSeparator ("");

            if(m_Settings.GetActiveTab().GetBrushCount() > 0)
            {
                menu.AddItem(new GUIContent("Select All %a"), false, ContextMenuCallback, new Action(() => { m_Settings.GetActiveTab().SelectAllBrushes(); }));
                menu.AddItem(new GUIContent("Invert Selection"), false, ContextMenuCallback, new Action(() => { m_Settings.GetActiveTab().InvertSelection(); }));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Select All %a"));
                menu.AddDisabledItem(new GUIContent("Invert Selection"));
            }

            menu.AddSeparator ("");

            menu.AddItem(new GUIContent("Arrange By Name"), false, ContextMenuCallback, new Action(() => { m_Settings.GetActiveTab().ArrangeBrushesByName(); }));

            return menu;
        }




        GenericMenu TabMenu()
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Add Tab"), false, ContextMenuCallback, new Action(() => { m_Settings.SetActiveTabIndex(m_Settings.AddNewTab(Strings.newTabName, m_Settings.GetActiveTabIndex())); }));

            menu.AddSeparator ("");
            menu.AddItem(new GUIContent("Rename"), false, ContextMenuCallback, new Action(() => { m_OnBeginTabRename = true; }));
            menu.AddItem(new GUIContent("Duplicate"), false, ContextMenuCallback, new Action(() => { m_Settings.DuplicateTab(m_Settings.GetActiveTabIndex()); }));
            if(m_Settings.GetTabCount() > 1)
                menu.AddItem(new GUIContent("Delete"), false, ContextMenuCallback, new Action(() => { m_Settings.DeleteTab(m_Settings.GetActiveTabIndex()); }));
            else
                menu.AddDisabledItem(new GUIContent("Delete"));

            return menu;
        }


        GenericMenu MultibrushPrefabMenu(Brush brush, int prefabSlot)
        {
            GenericMenu menu = new GenericMenu();

            if (brush.settings.multibrushSlots[prefabSlot].enabled)
                menu.AddItem(new GUIContent("Disable"), false, ContextMenuCallback, new Action(() => { Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Disable Slot"); brush.settings.multibrushSlots[prefabSlot].enabled = false; }));
            else
                menu.AddItem(new GUIContent("Enable"), false, ContextMenuCallback, new Action(() => { Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Enable Slot"); brush.settings.multibrushSlots[prefabSlot].enabled = true; }));

            if(brush.prefabSlots[prefabSlot].gameObject != null)
                menu.AddItem(new GUIContent("Clear"), false, ContextMenuCallback, new Action(() => { brush.ClearPrefab(prefabSlot); }));
            else
                menu.AddDisabledItem(new GUIContent("Clear"));


            menu.AddSeparator ("");
            if(brush.prefabSlots[prefabSlot].gameObject != null)
                menu.AddItem(new GUIContent("Reveal in Project"), false, ContextMenuCallback, new Action(() => EditorGUIUtility.PingObject(brush.prefabSlots[prefabSlot].gameObject)));
            else
                menu.AddDisabledItem(new GUIContent("Reveal in Project"));

            menu.AddSeparator ("");
            int rating = Mathf.RoundToInt(brush.settings.multibrushSlots[prefabSlot].raiting * 100.0f);
            menu.AddDisabledItem(new GUIContent("Rating " + rating + "%"));
            return menu;
        }

        void ContextMenuCallback(object obj)
        {
            if (obj != null && obj is Action)
                (obj as Action).Invoke();
        }

#endregion // Context Menus



#region GUI

        bool IsModifierDown(EventModifiers modifiers)
        {
            Event e = Event.current;

            if ((e.modifiers & EventModifiers.FunctionKey) != 0)
                return false;

            EventModifiers mask = EventModifiers.Alt | EventModifiers.Control | EventModifiers.Shift | EventModifiers.Command;
            modifiers &= mask;

            if (modifiers == 0 && (e.modifiers & (mask & ~modifiers)) == 0)
                return true;

            if ((e.modifiers & modifiers) != 0 && (e.modifiers & (mask & ~modifiers)) == 0)
                return true;

            return false;
        }



        void HandleKeyboardEvents()
        {
            Event e = Event.current;
            Tab currentTab = m_Settings.GetActiveTab();
            Brush selectedBrush = currentTab.GetFirstSelectedBrush ();
            BrushSettings brushSettings = selectedBrush != null ? selectedBrush.settings : null;

            switch (e.type)
            {
            case EventType.ScrollWheel:
                switch (m_CurrentTool)
                {
                case PaintTool.Place:
                    if (selectedBrush != null && !currentTab.HasMultipleSelectedBrushes())
                    {
                        if (IsModifierDown(EventModifiers.Control))
                        {
                            brushSettings.placeScale -= m_Settings.placeScaleStep * e.delta.y;
                            if (brushSettings.placeScale <= 0)
                                brushSettings.placeScale = 0.01f;
                            brushSettings.placeScale = Mathf.Round(brushSettings.placeScale / m_Settings.placeScaleStep) * m_Settings.placeScaleStep;

                            HandleUtility.Repaint();
                            e.Use();
                        }
                        else if (IsModifierDown(EventModifiers.Command))
                        {
                            float delta = Mathf.Abs(e.delta.y) < 1f ? Mathf.Sign(e.delta.y) : e.delta.y;
                            delta = Mathf.Clamp(delta, -2f, 2f);

                            brushSettings.placeEulerAngles.y += m_Settings.placeAngleStep * delta;
                            brushSettings.placeEulerAngles.y %= 360f;
                            brushSettings.placeEulerAngles.y = Mathf.Round(brushSettings.placeEulerAngles.y / m_Settings.placeAngleStep) * m_Settings.placeAngleStep;

                            HandleUtility.Repaint();
                            e.Use();
                        }
                    }
                    break;
                }
                break;
            case EventType.KeyDown:
                switch(m_CurrentTool)
                {
                case PaintTool.Brush:
                    if(selectedBrush != null && !currentTab.HasMultipleSelectedBrushes() && IsModifierDown(EventModifiers.None))
                    {
                        switch(e.keyCode) {
                        case KeyCode.LeftBracket:
                            brushSettings.brushRadius = Mathf.Max (0.05f, brushSettings.brushRadius - brushSettings.brushRadius * 0.1f);
                            HandleUtility.Repaint ();
                            e.Use();
                            break;
                        case KeyCode.RightBracket:
                            brushSettings.brushRadius = Mathf.Min (brushSettings.brushRadius + brushSettings.brushRadius * 0.1f, m_Settings.maxBrushRadius);
                            HandleUtility.Repaint ();
                            e.Use();
                            break;
                        case KeyCode.Comma:
                            if(brushSettings.gridEnabled)
                            {
                                //brushSettings.gridStep -= Vector2.one;
                                //UpdateGrid(selectedBrush);
                                //HandleUtility.Repaint ();
                                //e.Use();
                            }
                            break;
                        case KeyCode.Period:
                            if(brushSettings.gridEnabled)
                            {
                                //brushSettings.gridStep += Vector2.one;
                                //UpdateGrid(selectedBrush);
                                //HandleUtility.Repaint ();
                                //e.Use();
                            }
                            break;
                        case KeyCode.Semicolon:
                            if(brushSettings.gridEnabled)
                            {
                                brushSettings.gridOrigin -= new Vector2(0.5f, 0.5f);
                                UpdateGrid(selectedBrush);
                                HandleUtility.Repaint ();
                                e.Use();
                            }
                            break;
                        case KeyCode.Quote:
                            if(brushSettings.gridEnabled)
                            {
                                brushSettings.gridOrigin += new Vector2(0.5f, 0.5f);
                                UpdateGrid(selectedBrush);
                                HandleUtility.Repaint ();
                                e.Use();
                            }
                            break;
                        }
                    }
                    break;
                case PaintTool.Pin:
                    if(selectedBrush != null && !currentTab.HasMultipleSelectedBrushes() && IsModifierDown(EventModifiers.None))
                    {
                        switch(e.keyCode) {
                        case KeyCode.Comma:
                            if(brushSettings.gridEnabled)
                            {
                                //brushSettings.gridStep -= Vector2.one;
                                //UpdateGrid(selectedBrush);
                                //HandleUtility.Repaint ();
                                //e.Use();
                            }
                            break;
                        case KeyCode.Period:
                            if(brushSettings.gridEnabled)
                            {
                                //brushSettings.gridStep += Vector2.one;
                                //UpdateGrid(selectedBrush);
                                //HandleUtility.Repaint ();
                                //e.Use();
                            }
                            break;
                        case KeyCode.Semicolon:
                            if(brushSettings.gridEnabled)
                            {
                                brushSettings.gridOrigin -= new Vector2(0.5f, 0.5f);
                                UpdateGrid(selectedBrush);
                                HandleUtility.Repaint ();
                                e.Use();
                            }
                            break;
                        case KeyCode.Quote:
                            if(brushSettings.gridEnabled)
                            {
                                brushSettings.gridOrigin += new Vector2(0.5f, 0.5f);
                                UpdateGrid(selectedBrush);
                                HandleUtility.Repaint ();
                                e.Use();
                            }
                            break;
                        }
                    }
                    break;
                case PaintTool.Place:
                    if (selectedBrush != null && !currentTab.HasMultipleSelectedBrushes() && IsModifierDown(EventModifiers.None))
                    {
                        switch (e.keyCode)
                        {
                        case KeyCode.W:
                            if (m_Settings.placeScaleStep > 0)
                            {
                                brushSettings.placeScale += m_Settings.placeScaleStep;
                                brushSettings.placeScale = Mathf.Round(brushSettings.placeScale / m_Settings.placeScaleStep) * m_Settings.placeScaleStep;
                                HandleUtility.Repaint();
                            }
                            e.Use();
                            break;
                        case KeyCode.S:
                            if (m_Settings.placeScaleStep > 0 && brushSettings.placeScale - m_Settings.placeScaleStep > 0)
                            {
                                brushSettings.placeScale -= m_Settings.placeScaleStep;
                                brushSettings.placeScale = Mathf.Round(brushSettings.placeScale / m_Settings.placeScaleStep) * m_Settings.placeScaleStep;
                                HandleUtility.Repaint();
                            }
                            e.Use();
                            break;
                        case KeyCode.C:
                            if (m_Settings.placeAngleStep > 0)
                            {
                                brushSettings.placeEulerAngles.x += m_Settings.placeAngleStep;
                                brushSettings.placeEulerAngles.x %= 360f;
                                brushSettings.placeEulerAngles.x = Mathf.Round(brushSettings.placeEulerAngles.x / m_Settings.placeAngleStep) * m_Settings.placeAngleStep;

                                HandleUtility.Repaint();
                            }
                            e.Use();
                            break;
                        case KeyCode.Z:
                            if (m_Settings.placeAngleStep > 0)
                            {
                                brushSettings.placeEulerAngles.x -= m_Settings.placeAngleStep;
                                brushSettings.placeEulerAngles.x %= 360f;
                                brushSettings.placeEulerAngles.x = Mathf.Round(brushSettings.placeEulerAngles.x / m_Settings.placeAngleStep) * m_Settings.placeAngleStep;

                                HandleUtility.Repaint();
                            }
                            e.Use();
                            break;
                        case KeyCode.D:
                            if (m_Settings.placeAngleStep > 0)
                            {
                                brushSettings.placeEulerAngles.y += m_Settings.placeAngleStep;
                                brushSettings.placeEulerAngles.y %= 360f;
                                brushSettings.placeEulerAngles.y = Mathf.Round(brushSettings.placeEulerAngles.y / m_Settings.placeAngleStep) * m_Settings.placeAngleStep;

                                HandleUtility.Repaint();
                            }
                            e.Use();
                            break;
                        case KeyCode.A:
                            if (m_Settings.placeAngleStep > 0)
                            {
                                brushSettings.placeEulerAngles.y -= m_Settings.placeAngleStep;
                                brushSettings.placeEulerAngles.y %= 360f;
                                brushSettings.placeEulerAngles.y = Mathf.Round(brushSettings.placeEulerAngles.y / m_Settings.placeAngleStep) * m_Settings.placeAngleStep;
                                HandleUtility.Repaint();
                            }
                            e.Use();
                            break;
                        case KeyCode.Q:
                            if (m_Settings.placeAngleStep > 0)
                            {
                                brushSettings.placeEulerAngles.z += m_Settings.placeAngleStep;
                                brushSettings.placeEulerAngles.z %= 360f;
                                brushSettings.placeEulerAngles.z = Mathf.Round(brushSettings.placeEulerAngles.z / m_Settings.placeAngleStep) * m_Settings.placeAngleStep;

                                HandleUtility.Repaint();
                            }
                            e.Use();
                            break;
                        case KeyCode.E:
                            if (m_Settings.placeAngleStep > 0)
                            {
                                brushSettings.placeEulerAngles.z -= m_Settings.placeAngleStep;
                                brushSettings.placeEulerAngles.z %= 360f;
                                brushSettings.placeEulerAngles.z = Mathf.Round(brushSettings.placeEulerAngles.z / m_Settings.placeAngleStep) * m_Settings.placeAngleStep;

                                HandleUtility.Repaint();
                            }
                            e.Use();
                            break;
                        case KeyCode.X:
                            brushSettings.placeEulerAngles = Vector3.zero;
                            HandleUtility.Repaint();
                            e.Use();
                            break;
                        }
                    }
                        break;
                case PaintTool.Throw:
                    if (selectedBrush != null && !currentTab.HasMultipleSelectedBrushes() && IsModifierDown(EventModifiers.None))
                    {
                        switch (e.keyCode)
                        {
                        case KeyCode.LeftBracket:
                            brushSettings.throwRadius = Mathf.Max(0.05f, brushSettings.throwRadius - brushSettings.throwRadius * 0.1f);
                            HandleUtility.Repaint();
                            e.Use();
                            break;
                        case KeyCode.RightBracket:
                            brushSettings.throwRadius = Mathf.Min(brushSettings.throwRadius + brushSettings.throwRadius * 0.1f, m_Settings.maxBrushRadius);
                            HandleUtility.Repaint();
                            e.Use();
                            break;
                        }
                    }
                    break;

                case PaintTool.Erase:
                    if (IsModifierDown(EventModifiers.None))
                    {
                        switch (e.keyCode)
                        {
                        case KeyCode.LeftBracket:
                            m_Settings.eraseBrushRadius = Mathf.Max(0.05f, m_Settings.eraseBrushRadius - m_Settings.eraseBrushRadius * 0.1f);
                            HandleUtility.Repaint();
                            e.Use();
                            break;
                        case KeyCode.RightBracket:
                            m_Settings.eraseBrushRadius = Mathf.Min(m_Settings.eraseBrushRadius + m_Settings.eraseBrushRadius * 0.1f, m_Settings.maxBrushRadius);
                            HandleUtility.Repaint();
                            e.Use();
                            break;
                        }
                    }
                    break;
                case PaintTool.Select:
                    if (IsModifierDown(EventModifiers.None))
                    {
                        switch (e.keyCode)
                        {
                        case KeyCode.LeftBracket:
                            m_Settings.selectBrushRadius = Mathf.Max(0.05f, m_Settings.selectBrushRadius - m_Settings.selectBrushRadius * 0.1f);
                            HandleUtility.Repaint();
                            e.Use();
                            break;
                        case KeyCode.RightBracket:
                            m_Settings.selectBrushRadius = Mathf.Min(m_Settings.selectBrushRadius + m_Settings.selectBrushRadius * 0.1f, m_Settings.maxBrushRadius);
                            HandleUtility.Repaint();
                            e.Use();
                            break;
                        }
                    }
                    break;
                case PaintTool.Modify:
                    if (IsModifierDown(EventModifiers.None))
                    {
                        switch (e.keyCode)
                        {
                        case KeyCode.LeftBracket:
                            m_Settings.modifyBrushRadius = Mathf.Max(0.05f, m_Settings.modifyBrushRadius - m_Settings.modifyBrushRadius * 0.1f);
                            HandleUtility.Repaint();
                            e.Use();
                            break;
                        case KeyCode.RightBracket:
                            m_Settings.modifyBrushRadius = Mathf.Min(m_Settings.modifyBrushRadius + m_Settings.modifyBrushRadius * 0.1f, m_Settings.maxBrushRadius);
                            HandleUtility.Repaint();
                            e.Use();
                            break;
                        }
                    }
                    break;
                }

                // ESC
                if(e.keyCode == KeyCode.Escape && IsModifierDown(EventModifiers.None))
                {
                    if(m_CurrentTool != PaintTool.None)
                    {
                        Tools.current = Tool.Move;
                        m_CurrentTool = PaintTool.None;
                    }

                    Repaint();
                    e.Use ();
                }
                break;
            }
        }




        string GetShortNameForBrush(string name)
        {
            if(name == null || name.Length == 0)
                return "";

            string shortString;

            if(m_BrushShortNamesDictionary.TryGetValue(name, out shortString))
                return shortString;

            return m_BrushShortNamesDictionary[name] = Utility.TruncateString(name, Styles.iconLabelText, (int)(kBrushIconWidth * m_BrushIconScale));
        }



        string GetShortNameForTab(string name)
        {
            if(name == null || name.Length == 0)
                return "";

            string shortString;

            if(m_TabShortNamesDictionary.TryGetValue(name, out shortString))
                return shortString;

            return m_TabShortNamesDictionary[name] = Utility.TruncateString(name, Styles.tabLabelText, kTabWidth);
        }


        Vector2 GetBrushWindowScrollPosition(Tab tab)
        {
            Vector2 scrollPosition;

            if(m_BrushWindowsScrollDictionary.TryGetValue(tab.id, out scrollPosition))
                return scrollPosition;

            return Vector2.zero;
        }

        void SetBrushWindowScrollPosition(Tab tab, Vector2 scrollPosition)
        {
            m_BrushWindowsScrollDictionary[tab.id] = scrollPosition;
        }


        float GetMultibrushScrollPosition(Brush brush)
        {
            float scrollPosition;

            if(m_MultibrushScrollDictionary.TryGetValue(brush.id, out scrollPosition))
                return scrollPosition;

            return 0f;
        }

        void SetMultibrushScrollPosition(Brush brush, float scrollPosition)
        {
            m_MultibrushScrollDictionary[brush.id] = scrollPosition;
        }




        public class LevelsPopupWindow : PopupWindowContent
        {
            public PrefabPainter prefabPainter = null;

            public override Vector2 GetWindowSize()
            {
                return new Vector2(280, 140);
            }

            public override void OnGUI(Rect rect)
            {
                if (prefabPainter == null)
                {
                    editorWindow.Close();
                    return;
                }

                if (prefabPainter.m_AssetPreviewMaterial == null)
                {
                    EditorGUILayout.HelpBox("Shader Error", MessageType.Info);
                    return;
                }

                Event e = Event.current;
                Tab tab = prefabPainter.m_Settings.GetActiveTab();

                if (e.isKey && e.keyCode == KeyCode.Escape)
                    editorWindow.Close();

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Levels", EditorStyles.boldLabel);

                Rect labelRect = GUILayoutUtility.GetLastRect();

                GUI.color = Styles.iconTintColor;
                if (GUI.Button(new Rect(labelRect.x + labelRect.width - 20, labelRect.y, 20, labelRect.height), prefabPainter.m_ResetIconTexture, EditorStyles.label))
                {
                    Undo.RegisterCompleteObjectUndo(prefabPainter.m_Settings, "PP: Reset Levels");
                    tab.ResetLevels();
                    prefabPainter.Repaint();
                }
                GUI.color = Color.white;

                EditorGUILayout.Space();

                EditorGUI.BeginChangeCheck();
                tab.levelsInBlack = EditorGUILayout.Slider("Input Black", tab.levelsInBlack, 0, 255);
                tab.levelsInWhite = EditorGUILayout.Slider("Input White", tab.levelsInWhite, 0, 255);
                tab.levelsGamma = EditorGUILayout.Slider("Gamma", tab.levelsGamma, 0.01f, 5);
                tab.levelsOutBlack = EditorGUILayout.Slider("Output Black", tab.levelsOutBlack, 0, 255);
                tab.levelsOutWhite = EditorGUILayout.Slider("Output White", tab.levelsOutWhite, 0, 255);

                if(EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(prefabPainter.m_Settings, "PP: Levels");
                    prefabPainter.Repaint();
                }
            }
        }



        void TabsGUI()
        {
            Event e = Event.current;

            int tabCount = m_Settings.GetTabCount();
            if(tabCount == 0)
                return;


            int tabWidth = kTabWidth;
            int tabHeight = kTabHeight;


            float windowWidth = EditorGUIUtility.currentViewWidth - 16;

            tabCount = tabCount + 1;
            int tabsPerLine = Mathf.Max(1, Mathf.FloorToInt(windowWidth / tabWidth));
            int tabLines = Mathf.CeilToInt((float)tabCount / tabsPerLine);
            int tabIndex = 0;
            int tabUnderCursor = -1;

            tabWidth = Mathf.FloorToInt(windowWidth / tabsPerLine);

            // Get dragging data
            Brush[] draggingBrushes = null;
            Tab draggingTab = null;
            Rect dragRect = new Rect();
            if (InternalDragAndDrop.IsDragging() || InternalDragAndDrop.IsDragPerform())
            {
                if(InternalDragAndDrop.GetData() is Brush[])
                    draggingBrushes = (Brush[])InternalDragAndDrop.GetData();
                else
                    if(InternalDragAndDrop.GetData() is Tab)
                        draggingTab = (Tab)InternalDragAndDrop.GetData();
            }


            int tabWindowControlID = GUIUtility.GetControlID(s_TabsWindowHash, FocusType.Passive);


            for(int line = 0; line < tabLines; line++)
            {
                Rect lineRect = EditorGUILayout.GetControlRect(GUILayout.Height(tabHeight));

                for(int tab = 0; tab < tabsPerLine && tabIndex < tabCount; tab++, tabIndex++)
                {
                    // '+' Tab
                    if(tabIndex == tabCount-1)
                    {
                        Rect tabPlusRect = new Rect(lineRect.x + tabWidth * tab, lineRect.y, tabHeight * 1.2f, tabHeight);
                        Color contentColor = GUI.contentColor;
                        GUI.contentColor = Styles.iconTintColor;
                        EditorGUI.LabelField(tabPlusRect, new GUIContent(m_AddIconTexture), Styles.tabButton);
                        GUI.contentColor = contentColor;

                        if (tabPlusRect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 0)
                        {
                            EditorApplication.delayCall += () => {
                                m_Settings.SetActiveTabIndex(m_Settings.AddNewTab(Strings.newTabName));
                                Repaint();
                            };
                        }
                        break;
                    }



                    Rect tabRect = new Rect(lineRect.x + tabWidth * tab, lineRect.y, tabWidth, tabHeight);


                    // Tab under cursor
                    if(tabRect.Contains(e.mousePosition))
                    {
                        tabUnderCursor = tabIndex;

                        // Drop brushes
                        if (draggingBrushes != null)
                        {
                            EditorGUIUtility.AddCursorRect (tabRect, UnityEditor.MouseCursor.MoveArrow);

                            if(InternalDragAndDrop.IsDragPerform())
                            {
                                EditorApplication.delayCall += () => { m_Settings.MoveBrushes(draggingBrushes, m_Settings.GetTab(tabUnderCursor)); Repaint(); };
                            }
                        }

                        if (draggingTab != null)
                        {
                            EditorGUIUtility.AddCursorRect (tabRect, UnityEditor.MouseCursor.MoveArrow);

                            bool isAfter = (e.mousePosition.x - tabRect.xMin) > tabRect.width/2;

                            dragRect = new Rect(tabRect);

                            if(isAfter)
                            {
                                dragRect.xMin = dragRect.xMax - 2;
                                dragRect.xMax = dragRect.xMax + 2;
                            }
                            else
                            {
                                dragRect.xMax = dragRect.xMin + 2;
                                dragRect.xMin = dragRect.xMin - 2;
                            }

                            if(InternalDragAndDrop.IsDragPerform())
                            {
                                m_Settings.InsertSelectedTab(tabIndex, isAfter);
                            }
                        }
                    }


                    EditorGUI.LabelField(tabRect, "", Styles.tabButton);
                    if(tabIndex == m_Settings.GetActiveTabIndex())
                        EditorGUI.DrawRect(EditorGUI.IndentedRect(tabRect), Styles.tabTintColor);



                    // Tab Rename
                    if (tabIndex == m_Settings.GetActiveTabIndex())
                    {
                        Tab activeTab = m_Settings.GetTab(tabIndex);

                        // make TextField and set focus to it
                        if (m_OnBeginTabRename)
                        {
                            GUI.SetNextControlName(s_tabTextFieldName);
                            activeTab.name = EditorGUI.DelayedTextField(tabRect, activeTab.name);

                            TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                            if (textEditor != null) {
                                textEditor.SelectAll();
                            }

                            GUI.FocusControl(s_tabTextFieldName);
                            m_OnBeginTabRename = false;
                        }
                        else
                        {
                            // if TextField still in focus - continue rename
                            if (GUI.GetNameOfFocusedControl() == s_tabTextFieldName)
                            {
                                GUI.SetNextControlName(s_tabTextFieldName);
                                EditorGUI.BeginChangeCheck();
                                string newTabName = EditorGUI.DelayedTextField(tabRect, activeTab.name);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Tab Rename");
                                    activeTab.name = newTabName;
                                }


                                // Unfocus TextField - finish rename
                                bool onFinishRenameKeyDown = (e.isKey && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.Escape));
                                if(onFinishRenameKeyDown) {
                                    GUIUtility.keyboardControl = 0;
                                    GUIUtility.hotControl = 0;
                                    e.Use();
                                }

                            } else {
                                // TextField lost focus - finish rename
                                EditorGUI.LabelField(tabRect, GetShortNameForTab(activeTab.name), Styles.tabLabelText);
                            }
                        }
                    }
                    else {
                        EditorGUI.LabelField(tabRect, GetShortNameForTab(m_Settings.GetTab(tabIndex).name), Styles.tabLabelText);
                    }


                }
            }

            // Dragging cursor
            if(draggingTab != null)
                EditorGUI.DrawRect(dragRect, Color.white);



            switch(e.type)
            {
            case EventType.MouseDown:
                if(tabUnderCursor != -1 && e.button == 0)
                {
                    GUIUtility.keyboardControl = tabWindowControlID;
                    GUIUtility.hotControl = 0;

                    m_Settings.SetActiveTabIndex(tabUnderCursor);

                    if(e.clickCount > 1)
                    {
                        m_OnBeginTabRename = true;
                    }

                    e.Use();
                }
                break;
            case EventType.MouseUp:
                break;
            case EventType.ContextClick:
                if(tabUnderCursor != -1)
                {
                    GUIUtility.keyboardControl = tabWindowControlID;
                    GUIUtility.hotControl = 0;

                    if(m_Settings.GetActiveTabIndex() != tabUnderCursor)
                    {
                        m_Settings.SetActiveTabIndex(tabUnderCursor);
                    } else {
                        TabMenu().ShowAsContext();
                    }

                    e.Use();
                }
                break;
            case EventType.ValidateCommand:
                if(GUIUtility.keyboardControl == tabWindowControlID)
                {
                    switch(e.commandName)
                    {
                    case "SelectAll":
                    case "Delete":
                    case "Duplicate":
                    case "Cut":
                    case "Copy":
                    case "Paste":
                        e.Use();
                        break;
                    }
                }
                break;
            case EventType.ExecuteCommand:
                if(GUIUtility.keyboardControl == tabWindowControlID)
                {
                    switch(e.commandName)
                    {
                    case "SelectAll":
                        EditorApplication.delayCall += () => { m_Settings.GetActiveTab().SelectAllBrushes(); Repaint(); };
                        e.Use();
                        break;
                    case "Delete":
                        EditorApplication.delayCall += () => { m_Settings.GetActiveTab().DeleteSelectedBrushes(); Repaint(); };
                        e.Use();
                        break;
                    case "Duplicate":
                        EditorApplication.delayCall += () => { m_Settings.GetActiveTab().DuplicateSelectedBrushes(); Repaint(); };
                        e.Use();
                        break;
                    case "Cut":
                        EditorApplication.delayCall += () => { m_Settings.ClipboardCutBrushes(); Repaint(); };
                        e.Use();
                        break;
                    case "Copy":
                        m_Settings.ClipboardCopyBrushes();
                        e.Use();
                        break;
                    case "Paste":
                        EditorApplication.delayCall += () => { m_Settings.ClipboardPasteBrushes(); Repaint(); };
                        e.Use();
                        break;
                    }
                }
                break;
            }


            // Start drag tab
            //
            if (InternalDragAndDrop.IsDragReady() && tabUnderCursor != -1)
            {
                InternalDragAndDrop.StartDrag(m_Settings.GetTab(tabUnderCursor));
            }
        }



        Texture2D GetPrefabPreviewTexture(GameObject prefab)
        {
            Texture2D previewTexture;

            // BLUEGOOGAMES: Get preview from sprite, to show correct aspect ratio
            SpriteRenderer sr;
            if ((sr = prefab.GetComponent<SpriteRenderer>()) != null) {
                previewTexture = AssetPreview.GetAssetPreview(sr.sprite);
                return previewTexture;
            }

            if((previewTexture = AssetPreview.GetAssetPreview(prefab)) != null)
                return previewTexture;

            if ((previewTexture = AssetPreview.GetMiniThumbnail(prefab)) != null)
                return previewTexture;

            return AssetPreview.GetMiniTypeThumbnail(typeof(GameObject));
        }

        void BrushWindowGUI()
        {
            Event e = Event.current;

            Tab currentTab = m_Settings.GetActiveTab();
            int brushCount = currentTab.GetBrushCount();

            if (m_AssetPreviewMaterial)
            {
                float invGamma = 1f / Mathf.Max(0.01f, currentTab.levelsGamma);
                float inBlack = currentTab.levelsInBlack / 255f;
                float inWhite = currentTab.levelsInWhite / 255f;
                float outBlack = currentTab.levelsOutBlack / 255f;
                float outWhite = currentTab.levelsOutWhite / 255f;

                if (inBlack >= inWhite)
                {
                    inBlack = 0;
                    inWhite = 1;
                }

                m_AssetPreviewMaterial.SetColor("_InvGamma", new Color(invGamma, invGamma, invGamma, invGamma));
                m_AssetPreviewMaterial.SetColor("_InBlack", new Color(inBlack, inBlack, inBlack, inBlack));
                m_AssetPreviewMaterial.SetColor("_InWhite", new Color(inWhite, inWhite, inWhite, inWhite));
                m_AssetPreviewMaterial.SetColor("_OutBlack", new Color(outBlack, outBlack, outBlack, outBlack));
                m_AssetPreviewMaterial.SetColor("_OutWhite", new Color(outWhite, outWhite, outWhite, outWhite));
            }


            int brushIconWidth = (int)(kBrushIconWidth * m_BrushIconScale);
            int brushIconHeight = (int)(kBrushIconHeight * m_BrushIconScale);
            int brushIconSpace = 3;

            Rect brushWindowRect = EditorGUILayout.GetControlRect(GUILayout.Height(Mathf.Max(0.0f, m_BrushWindowHeight)) );
            int brushWindowControlID = GUIUtility.GetControlID(s_BrushWindowHash, FocusType.Passive, brushWindowRect);

            Rect offscreenRect = new Rect(brushWindowRect);
            {
                offscreenRect.width = Mathf.Max(offscreenRect.width - 20, 1); // space for scroll bar

                float iconColumnsf = Mathf.Max(1, offscreenRect.width / (brushIconWidth + brushIconSpace * 2));
                int iconColumns = Mathf.FloorToInt(iconColumnsf);
                int iconRows   = Mathf.CeilToInt((float)brushCount / iconColumns);

                float scaleToFit = offscreenRect.width / (iconColumns * (brushIconWidth + brushIconSpace * 2));
                brushIconWidth = (int)(brushIconWidth * scaleToFit);
                brushIconHeight = (int)(brushIconHeight * scaleToFit);

                offscreenRect.height = Mathf.Max(offscreenRect.height, (brushIconHeight + brushIconSpace * 2) * iconRows);
            }



            // draw brushes window background
            GUI.Label(brushWindowRect, "", EditorStyles.helpBox);

            Vector2 brushWindowScrollPos = GetBrushWindowScrollPosition(currentTab);
            brushWindowScrollPos = GUI.BeginScrollView(brushWindowRect, brushWindowScrollPos, offscreenRect, false, true);



            // Empty preset list - show Drag&Drop Info
            if (brushCount == 0)
            {
                GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
                labelStyle.fontStyle = FontStyle.Bold;
                labelStyle.alignment = TextAnchor.MiddleCenter;
                EditorGUI.LabelField(brushWindowRect, "Drag & Drop Prefab Here\nShift+drag - Relink prefab", labelStyle);
            }


            Rect dragRect = new Rect(0, 0, 0, 0);
            int brushIndex = 0;
            int brushUnderCursor = -1;
            int iconDrawCount = 0;


            // if we dragging brushes get it
            Brush[] draggingBrushes = null;
            if ((InternalDragAndDrop.IsDragging() || InternalDragAndDrop.IsDragPerform())
                && InternalDragAndDrop.GetData() is Brush[] && offscreenRect.Contains (e.mousePosition))
            {
                draggingBrushes = (Brush[])InternalDragAndDrop.GetData();
                EditorGUIUtility.AddCursorRect (brushWindowRect, UnityEditor.MouseCursor.MoveArrow);
            }



            for (int y = (int)offscreenRect.yMin + brushIconSpace; y < (int)offscreenRect.yMax; y += brushIconHeight + brushIconSpace*2)
            {
                if (brushIndex >= brushCount)
                    break;

                for (int x = (int)offscreenRect.xMin + brushIconSpace; (x+brushIconWidth) <= (int)(offscreenRect.xMax); x += brushIconWidth + brushIconSpace*2)
                {
                    // filter brushes
                    while (brushIndex < brushCount)
                    {
                        if (String.IsNullOrEmpty(m_SearchString) || currentTab.brushes[brushIndex].name
                                .IndexOf(m_SearchString, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            break;
                        }

                        brushIndex++;
                    }

                    if (brushIndex >= brushCount)
                        break;


                    Rect brushIconRect = new Rect(x, y, brushIconWidth, brushIconHeight);

                    Rect brushIconRectScrolled = new Rect(brushIconRect);
                    brushIconRectScrolled.position -= brushWindowScrollPos;

                    // only visible incons
                    if(brushIconRectScrolled.Overlaps(brushWindowRect))
                    {


                        if(brushIconRect.Contains(e.mousePosition))
                            brushUnderCursor = brushIndex;



                        Brush brush = currentTab.brushes[brushIndex];



                        // Draw selected Prefab preview blue rect
                        if(brush.selected)
                            EditorGUI.DrawRect(brushIconRect, Styles.colorBlue);




                        if (draggingBrushes != null && e.type == EventType.Repaint)
                        {
                            if (brushIconRect.Contains(e.mousePosition))
                            {
                                bool isAfter = (e.mousePosition.x - brushIconRect.xMin) > brushIconRect.width/2;

                                dragRect = new Rect(brushIconRect);

                                if(isAfter)
                                {
                                    dragRect.xMin = dragRect.xMax - 2;
                                    dragRect.xMax = dragRect.xMax + 2;
                                }
                                else
                                {
                                    dragRect.xMax = dragRect.xMin + 2;
                                    dragRect.xMin = dragRect.xMin - 2;
                                }

                                if(InternalDragAndDrop.IsDragPerform())
                                {
                                    currentTab.InsertSelectedBrushes(brushIndex, isAfter);
                                }
                            }

//							if(brushWindowRect.Contains(e.mousePosition))
//							{
//								if(brushCount-1 == brushIndex)
//								{
//									dragRect = new Rect(brushIconRect);
//									dragRect.xMin = dragRect.xMax - 2;
//									dragRect.xMax = dragRect.xMax + 2;
//								}
//
//								if(InternalDragAndDrop.IsDragPerform())
//									AddDelayedAction(() => currentTab.InsertSelectedBrushes(brushCount-1, true));
//							}
                        }


                        // Prefab preview
                        if(e.type == EventType.Repaint)
                        {

                            Rect previewRect = new Rect(brushIconRect.x+2, brushIconRect.y+2, brushIconRect.width-4, brushIconRect.width-4);
                            Color dimmedColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);

                            if (brush.customPreviewImage != null)
                            {
                                EditorGUI.DrawPreviewTexture(previewRect, brush.customPreviewImage, m_AssetPreviewMaterial);
                            }
                            else if(brush.settings.multibrushEnabled)
                            {
                                iconDrawCount += 4;

                                Rect[] icons =
                                {   new Rect(previewRect.x, previewRect.y, previewRect.width/2-1, previewRect.height/2-1),
                                    new Rect(previewRect.x+previewRect.width/2, previewRect.y, previewRect.width/2, previewRect.height/2-1),
                                    new Rect(previewRect.x, previewRect.y+previewRect.height/2, previewRect.width/2-1, previewRect.height/2),
                                    new Rect(previewRect.x+previewRect.width/2, previewRect.y+previewRect.height/2, previewRect.width/2, previewRect.height/2)
                                };

                                GameObject[] prefabs = new GameObject[4];

                                for(int i = 0, j = 0; i < brush.prefabSlots.Length && j < 4; i++)
                                {
                                    if(brush.prefabSlots[i].gameObject != null)
                                    {
                                        prefabs[j] = brush.prefabSlots[i].gameObject;
                                        j++;
                                    }
                                }

                                for(int i = 0; i < 4; i++)
                                {
                                    if(prefabs[i] != null)
                                    {
                                        Texture2D preview = GetPrefabPreviewTexture(prefabs[i]);
                                        if (preview != null)
                                            // BLUEGOOGAMES: Keep aspect ratio of sprites
                                            EditorGUI.DrawPreviewTexture(icons[i], preview, m_AssetPreviewMaterial, ScaleMode.ScaleToFit, 0);
                                    }
                                    else
                                        EditorGUI.DrawRect(icons[i], dimmedColor);
                                }
                            }
                            else // !Multibrush
                            {
                                iconDrawCount += 1;

                                GameObject prefab = brush.GetFirstAssociatedPrefab();

                                if (prefab != null)
                                {
                                    Texture2D preview = GetPrefabPreviewTexture(prefab);
                                    if (preview != null)
                                        // BLUEGOOGAMES: Keep aspect ratio of sprites
                                        EditorGUI.DrawPreviewTexture(previewRect, preview, m_AssetPreviewMaterial, ScaleMode.ScaleToFit, 0);
                                }
                                else
                                {
                                    EditorGUI.DrawRect(previewRect, dimmedColor);
                                }
                            }

                            // Prefab name
                            Styles.iconLabelText.Draw(brushIconRect, GetShortNameForBrush(brush.name), false, false, false, false);

                            // Color tag
                            if(brush.colorTag != ColorTag.None)
                            {
                                float size = previewRect.width * 0.3f;
                                Rect tagRect = new Rect(previewRect.x + previewRect.width - size, previewRect.y, size, size);

                                Color guiColor = GUI.color;
                                switch(brush.colorTag)
                                {
                                case ColorTag.Red:      GUI.color = Styles.colorTagRed; break;
                                case ColorTag.Orange:   GUI.color = Styles.colorTagOrange; break;
                                case ColorTag.Yellow:   GUI.color = Styles.colorTagYellow; break;
                                case ColorTag.Green:    GUI.color = Styles.colorTagGreen; break;
                                case ColorTag.Blue:     GUI.color = Styles.colorTagBlue; break;
                                case ColorTag.Violet:   GUI.color = Styles.colorTagViolet; break;
                                }
                                GUI.DrawTexture(tagRect, m_TagTexture, ScaleMode.ScaleToFit, true, 0);
                                GUI.color = guiColor;
                            }
                        }

                    }

                    brushIndex++;
                } // x
            } // y


            // Dragging cursor
            if(draggingBrushes != null)
                EditorGUI.DrawRect(dragRect, Color.white);


            // increase preview cache size if needed
            AssetPreviewCacheController.AddCacheRequest("BrushWindow", iconDrawCount * 2);


            switch(e.type)
            {
            case EventType.MouseDown:

                if(offscreenRect.Contains(e.mousePosition) && e.button == 0)
                {
                    GUIUtility.keyboardControl = brushWindowControlID;
                    GUIUtility.hotControl = brushWindowControlID;

                    // Double click on background
                    if(e.clickCount == 2 && brushUnderCursor == -1)
                    {
                        this.SendEvent(EditorGUIUtility.CommandEvent("BrushWindowShowObjectSelector"));
                    }


                    if(brushUnderCursor != -1)
                    {
#if UNITY_EDITOR_OSX
                        if (e.command)
#else
                        if (e.control)
#endif
                        {
                            currentTab.SelectBrushAdditive(brushUnderCursor);
                        }
                        else if (e.shift)
                        {
                            currentTab.SelectBrushRange(brushUnderCursor);
                        }
                        else {
                            if(currentTab.IsBrushSelected(brushUnderCursor) == false) // Deselect other on mouse up for drag operation
                                currentTab.SelectBrush(brushUnderCursor);
                        }
                    } else {
                        currentTab.DeselectAllBrushes();
                    }

                    e.Use();
                }
                break;
            case EventType.MouseUp:
                if (e.button == 0 && offscreenRect.Contains(e.mousePosition))
                {
                    GUIUtility.hotControl = 0;

#if UNITY_EDITOR_OSX
                    if (brushUnderCursor != -1 && !e.command && !e.shift)
#else
                    if (brushUnderCursor != -1 && !e.control && !e.shift)
#endif
                    {
                        EditorApplication.delayCall += () => { currentTab.SelectBrush(brushUnderCursor); Repaint(); };
                    }


                    e.Use();
                }
                break;
            case EventType.ContextClick:
                if(brushUnderCursor != -1)
                {
                    if (currentTab.IsBrushSelected(brushUnderCursor))
                    {
                        PresetMenu().ShowAsContext();
                    }
                    else
                    {
                        currentTab.SelectBrush(brushUnderCursor);
                    }
                    e.Use();
                }
                if(offscreenRect.Contains (e.mousePosition) && brushUnderCursor == -1)
                {
                    GUIUtility.keyboardControl = brushWindowControlID;

                    BrushWindowMenu().ShowAsContext();
                    e.Use();
                }
                break;
            case EventType.ValidateCommand:
                if(GUIUtility.keyboardControl == brushWindowControlID)
                {
                    switch(e.commandName)
                    {
                    case "SelectAll":
                    case "Delete":
                    case "Duplicate":
                    case "Cut":
                    case "Copy":
                    case "Paste":
                    case "BrushWindowShowObjectSelector":
                    case "ObjectSelectorClosed":
                        e.Use();
                        break;
                    }
                }
                break;
            case EventType.ExecuteCommand:
                if(GUIUtility.keyboardControl == brushWindowControlID)
                {
                    switch(e.commandName)
                    {
                    case "SelectAll":
                        EditorApplication.delayCall += () => { currentTab.SelectAllBrushes(); Repaint(); };
                        e.Use();
                        break;
                    case "Delete":
                        EditorApplication.delayCall += () => { currentTab.DeleteSelectedBrushes(); Repaint(); };
                        e.Use();
                        break;
                    case "Duplicate":
                        EditorApplication.delayCall += () => { currentTab.DuplicateSelectedBrushes(); Repaint(); };
                        e.Use();
                        break;
                    case "Cut":
                        EditorApplication.delayCall += () => { m_Settings.ClipboardCutBrushes(); Repaint(); };
                        e.Use();
                        break;
                    case "Copy":
                        m_Settings.ClipboardCopyBrushes();
                        e.Use();
                        break;
                    case "Paste":
                        EditorApplication.delayCall += () => { m_Settings.ClipboardPasteBrushes(); Repaint(); };
                        e.Use();
                        break;
                    case "BrushWindowShowObjectSelector":
                        EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "", s_BrushWindowObjectPickerHash);
                        e.Use();
                        break;
                    case "ObjectSelectorClosed":
                        if(EditorGUIUtility.GetObjectPickerControlID() == s_BrushWindowObjectPickerHash)
                        {
                            UnityEngine.Object pickedObject = EditorGUIUtility.GetObjectPickerObject();

                            if (IsAcceptablePrefab(pickedObject))
                            {
                                Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Add Prefab");
                                currentTab.brushes.Add(new Brush(pickedObject as GameObject));
                            }

                            e.Use();
                        }
                        break;
                    }
                }
                break;
            }



            // Drag brushes
            if (InternalDragAndDrop.IsDragReady() && currentTab.HasSelectedBrushes() && offscreenRect.Contains (InternalDragAndDrop.DragStartPosition()) && GUIUtility.hotControl == brushWindowControlID)
            {
                InternalDragAndDrop.StartDrag(currentTab.brushes.FindAll((b) => b.selected).ToArray());
            }



            // Drop operation
            //
            if (e.type == EventType.DragUpdated || e.type == EventType.DragPerform)
            {
                // Custom preview image
                if (brushUnderCursor != -1 && DragAndDrop.objectReferences.Length > 0 &&
                    DragAndDrop.objectReferences[0] != null &&
                    DragAndDrop.objectReferences[0] is Texture2D texture2D)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;

                    if (e.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Assign Custom Thumbnail");
                        currentTab.brushes[brushUnderCursor].customPreviewImage = texture2D;
                    }
                }
                else
                if (e.shift && brushUnderCursor != -1) // Relink Prefab
                {
                    if(currentTab.brushes[brushUnderCursor].settings.multibrushEnabled)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    }
                    else
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;

                        if (e.type == EventType.DragPerform) {
                            DragAndDrop.AcceptDrag ();

                            foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                            {
                                if (IsAcceptablePrefab(draggedObject))
                                {
                                        Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Relink Prefab");
                                           currentTab.brushes[brushUnderCursor].AssignPrefab(draggedObject as GameObject, 0);
                                }
                            }
                        }
                    }

                    e.Use();
                }
                else // Add Prefab
                {
                    // Add Prefab
                    if(offscreenRect.Contains (e.mousePosition))
                    {
                        if (DragAndDrop.objectReferences.Length > 0)
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (e.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag ();

                            List<GameObject> draggedGameObjects = new List<GameObject>();

                            foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                            {
                                if (IsAcceptablePrefab(draggedObject))
                                {
                                            draggedGameObjects.Add (draggedObject as GameObject);
                                }
                            }

                            if(draggedGameObjects.Count > 0)
                            {
                                Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Add Prefab(s)");

                                draggedGameObjects.Sort (delegate(GameObject x, GameObject y) {
                                    return EditorUtility.NaturalCompare(x.name, y.name);
                                });

                                foreach(GameObject go in draggedGameObjects)
                                    currentTab.brushes.Add(new Brush(go));
                            }
                        }
                        e.Use();
                    }
                }
            }




            GUI.EndScrollView();
            SetBrushWindowScrollPosition(currentTab, brushWindowScrollPos);



            // Scroll window while dragging up/down
            if(InternalDragAndDrop.IsDragging() && e.type == EventType.Repaint)
            {
                float scrollSpeed = 300.0f;
                float factor = ((e.mousePosition.y - brushWindowRect.y) - brushWindowRect.height/2.0f) / brushWindowRect.height * 2.0f;
                //factor = Mathf.Abs(factor) < 0.5f ? 0.0f : factor; // dead zone
                factor = factor * factor * Mathf.Sign(factor); // non linear speed
                brushWindowScrollPos.y += factor * scrollSpeed * Time.deltaTime;
            }



            // Status Bar
            {
                int settingsButtonWidth = 25;
                int sliderWidth = 100;

                Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(16));

                // draw bg
                EditorGUI.LabelField(rect, "", Styles.tabButton);

                // search field
                Rect searchRect = new Rect(rect.x, rect.y, rect.width - sliderWidth - 16 - settingsButtonWidth - 16 - 16, rect.height);
                Rect seachCancelButtonRect = new Rect(rect.x + rect.width - sliderWidth - 16 - settingsButtonWidth - 16 - 16, rect.y, 16, rect.height);
                m_SearchString = EditorGUI.TextField(searchRect, m_SearchString, EditorStyles.toolbarSearchField);
                if (GUI.Button(seachCancelButtonRect, "", Styles.toolbarSeachCancelButtonStyle))
                {
                    // Remove focus if cleared
                    m_SearchString = "";
                    GUI.FocusControl(null);
                }


                // levels button
                Rect levelsRect = new Rect(rect.x + rect.width - sliderWidth - 16 - settingsButtonWidth - 16, rect.y, settingsButtonWidth, rect.height);

                EditorGUI.LabelField(levelsRect, "", Styles.tabButton);
                GUI.color = Styles.iconTintColor;
                if (GUI.Button(new Rect(levelsRect.x + (levelsRect.width - levelsRect.height) / 2, levelsRect.y, levelsRect.height, levelsRect.height), m_LevelsIconTexture, EditorStyles.label))
                {
                    LevelsPopupWindow levelsPopupWindow = new LevelsPopupWindow();
                    levelsPopupWindow.prefabPainter = this;
                    PopupWindow.Show(new Rect(rect.x + rect.width - sliderWidth - 16 - settingsButtonWidth - 16, rect.y, settingsButtonWidth, rect.height), levelsPopupWindow);
                }
                GUI.color = Color.white;

                // icons scale slider
                EditorGUI.BeginChangeCheck();
                m_BrushIconScale = GUI.HorizontalSlider(new Rect(rect.x + rect.width - sliderWidth - 16, rect.y, sliderWidth, rect.height), m_BrushIconScale, 0.7f, 1.8f);
                if(EditorGUI.EndChangeCheck())
                {
                    m_BrushShortNamesDictionary.Clear();
                }
            }


            //
            // Brush window resize bar
            {
                Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(kBrushWindowResizeBarHeight));
                int controlID = GUIUtility.GetControlID(s_BrushWindowResizeBarHash, FocusType.Passive, rect);

                EditorGUIUtility.AddCursorRect (rect, UnityEditor.MouseCursor.SplitResizeUpDown);

                switch(e.type)
                {
                case EventType.MouseDown:
                    if(rect.Contains(e.mousePosition) && e.button == 0)
                    {
                        GUIUtility.keyboardControl = controlID;
                        GUIUtility.hotControl = controlID;
                        e.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID && e.button == 0)
                    {
                        GUIUtility.hotControl = 0;
                        e.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        Rect windowRect = EditorGUIUtility.ScreenToGUIRect(this.position);

                        // Clamp brush window size to min/max values
                        m_BrushWindowHeight = Mathf.Clamp(m_BrushWindowHeight + e.delta.y,
                            kBrushIconHeight, m_BrushWindowHeight + (windowRect.yMax - rect.yMax));

                        e.Use();
                    }
                    break;
                case EventType.Repaint:
                    {
                        Rect drawRect = rect;
                        drawRect.yMax -= 2; drawRect.yMin += 2;
                        EditorGUI.DrawRect(drawRect, Styles.backgroundColor);
                    }
                    break;
                }
            }


        }



        static Rect TransformRect(Rect rect, float xOffset, float yOffset, float xScale, float yScale)
        {
            return new Rect(rect.x + rect.width * xOffset, rect.y + rect.height * yOffset, rect.width * xScale, rect.height * yScale);
        }



        static float RaitingBar(Rect rect, float value)
        {
            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(s_RaitingBarHash, FocusType.Passive, rect);

            switch(e.type)
            {
            case EventType.MouseDown:
                if(rect.Contains(e.mousePosition) && e.button == 0)
                {
                    value = (e.mousePosition.x - rect.x) / rect.width;
                    GUI.changed = true;

                    GUIUtility.keyboardControl = controlID;
                    GUIUtility.hotControl = controlID;
                    e.Use();
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && e.button == 0)
                {
                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;
            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlID)
                {
                    value = (e.mousePosition.x - rect.x) / rect.width;
                    GUI.changed = true;

                    e.Use();
                }
                break;
            case EventType.Repaint:
                {
                    Rect drawRect = rect;
                    drawRect.width *= value;
                    Color  colorBlue = new Color32 (30, 80, 180, 255);
                    EditorGUI.DrawRect(drawRect, colorBlue);
                }
                break;
            }

            return Mathf.Clamp01(value);
        }



        static void DrawBoxGUI(Rect rect, int lineWidth, Color color)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width,                       lineWidth), color); // top
            EditorGUI.DrawRect(new Rect(rect.x, rect.y+rect.height-lineWidth, rect.width, lineWidth), color); // bottom

            EditorGUI.DrawRect(new Rect(rect.x, rect.y, lineWidth,                        rect.height), color); // left
            EditorGUI.DrawRect(new Rect(rect.x+rect.width-lineWidth, rect.y, lineWidth,   rect.height), color); // right
        }



        static float HorizontalScrollBar(Rect windowRect, float position, Rect viewRect)
        {
            Event e = Event.current;


            float scrollbarScale;

            if(viewRect.width != 0f)
                scrollbarScale = Mathf.Clamp01(windowRect.width / viewRect.width);
            else
                scrollbarScale = 1.0f;


            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(GUI.skin.horizontalScrollbar.fixedHeight));
            rect = EditorGUI.IndentedRect(rect);

            int controlID = GUIUtility.GetControlID(s_HorizontalMiniScrollBarHash, FocusType.Passive, rect);

            Rect scrollBarRect = rect;
            scrollBarRect.x += position * scrollbarScale;
            scrollBarRect.width = rect.width * scrollbarScale;

            switch(e.type)
            {
            case EventType.MouseDown:
                if(rect.Contains(e.mousePosition) && e.button == 0)
                {
                    if(scrollBarRect.Contains(e.mousePosition))
                    {
                        s_HorizontalMiniScrollBar_GrabPosition = e.mousePosition.x;
                        GUIUtility.hotControl = controlID;
                    }
                    else
                    {
                        if(e.mousePosition.x < scrollBarRect.x)
                        {
                            position -= scrollbarScale * viewRect.width * 0.5f;
                        }
                        else
                        {
                            position += scrollbarScale * viewRect.width * 0.5f;
                        }

                        GUI.changed = true;
                    }

                    GUIUtility.keyboardControl = controlID;
                    e.Use();
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && e.button == 0)
                {
                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;
            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlID)
                {
                    if(rect.width != 0f)
                    {
                        position += (e.mousePosition.x - s_HorizontalMiniScrollBar_GrabPosition) * (viewRect.width / rect.width);
                        s_HorizontalMiniScrollBar_GrabPosition = e.mousePosition.x;
                        GUI.changed = true;
                    }

                    e.Use();
                }
                break;
            case EventType.Repaint:
                {
                    GUI.skin.horizontalScrollbar.Draw(rect, false, false, false, false);
                    GUI.skin.horizontalScrollbarThumb.Draw(scrollBarRect, false, false, false, false);
                }
                break;
            }

            return Mathf.Clamp(position, 0, Math.Max(0, viewRect.width - windowRect.width));
        }



        void MultibrushGUI(Brush brush, bool showSettings)
        {
            Event e = Event.current;
            Tab currentTab = m_Settings.GetActiveTab();

            if (!brush.settings.multibrushEnabled)
                return;

            if (m_AssetPreviewMaterial)
            {
                float invGamma = 1f / Mathf.Max(0.01f, currentTab.levelsGamma);
                float inBlack = currentTab.levelsInBlack / 255f;
                float inWhite = currentTab.levelsInWhite / 255f;
                float outBlack = currentTab.levelsOutBlack / 255f;
                float outWhite = currentTab.levelsOutWhite / 255f;

                if (inBlack >= inWhite)
                {
                    inBlack = 0;
                    inWhite = 1;
                }

                m_AssetPreviewMaterial.SetColor("_InvGamma", new Color(invGamma, invGamma, invGamma, invGamma));
                m_AssetPreviewMaterial.SetColor("_InBlack", new Color(inBlack, inBlack, inBlack, inBlack));
                m_AssetPreviewMaterial.SetColor("_InWhite", new Color(inWhite, inWhite, inWhite, inWhite));
                m_AssetPreviewMaterial.SetColor("_OutBlack", new Color(outBlack, outBlack, outBlack, outBlack));
                m_AssetPreviewMaterial.SetColor("_OutWhite", new Color(outWhite, outWhite, outWhite, outWhite));
            }


            if ((m_MultibrushSettingsFoldout = InternalGUI.FoldoutReset(m_MultibrushSettingsFoldout,
                () => { Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Multibrush Settings"); brush.settings.ResetMultibrush(); },
                "Multibrush", m_ResetIconTexture)) == false)
            {
                return;
            }


            int iconSize = kMultibrushIconSize;
            int borderSize = kMultibrushIconBorderSize;
            int raitingBarHeight = Mathf.RoundToInt(iconSize * kMultibrushRaitingBarHeightPercent);
            int prefabCount = brush.prefabSlots.Length;





            // increase preview cache size if needed
            AssetPreviewCacheController.AddCacheRequest("MultibrushWindow", prefabCount);

            int raitigBarBorder = 1;
            int totalWindowWitdh = (borderSize + iconSize + borderSize)  * prefabCount;

            ++EditorGUI.indentLevel;

            // Get rect for prefab icons
            Rect windowRect = EditorGUILayout.GetControlRect(GUILayout.Height(borderSize + iconSize + raitigBarBorder + raitingBarHeight + borderSize));


            // center rect
            if(windowRect.width > totalWindowWitdh)
            {
                float size = windowRect.width - totalWindowWitdh;
                windowRect.width -= size;
                windowRect.x += size/2;
            }
            else
            {
                windowRect = EditorGUI.IndentedRect(windowRect);
            }

            Rect viewRect = windowRect;
            viewRect.width = totalWindowWitdh;




            int controlID = GUIUtility.GetControlID(s_MultibrushHash, FocusType.Passive, windowRect);

            // unfocus other elements
            if(e.type == EventType.MouseDown && windowRect.Contains(e.mousePosition))
            {
                GUIUtility.keyboardControl = controlID;
                GUIUtility.hotControl = 0;
            }



            float scrollPosition = GetMultibrushScrollPosition(brush);


            GUI.BeginGroup(windowRect, EditorStyles.textArea);

            Rect itemRect = new Rect(borderSize - scrollPosition, borderSize, iconSize, iconSize);
            int itemUnderCursor = -1;

            for(int i = 0; i < prefabCount; i++)
            {
                if(e.type == EventType.Repaint)
                {
                    if(brush.selectedSlot == i)
                    {
                        Rect selectedRect = itemRect;
                        selectedRect.x -= borderSize;
                        selectedRect.y -= borderSize;
                        selectedRect.width += borderSize*2;
                        selectedRect.height += borderSize*2 + raitingBarHeight + 1;

                        EditorGUI.DrawRect(selectedRect, Styles.colorBlue);
                    }

                    if(brush.prefabSlots[i].gameObject != null)
                    {
                        Texture2D preview = GetPrefabPreviewTexture(brush.prefabSlots[i].gameObject);
                        if (preview != null)
                            // BLUEGOOGAMES: Keep aspect ratio of brush slots
                            EditorGUI.DrawPreviewTexture(itemRect, preview, m_AssetPreviewMaterial, ScaleMode.ScaleToFit, 0);

                        // fade disabled prefabs
                        if(!brush.settings.multibrushSlots[i].enabled)
                            EditorGUI.DrawRect(itemRect, new Color(0.5f, 0.5f, 0.5f, 0.8f));

                        // Prefab name
                        Styles.multibrushIconText.alignment = TextAnchor.LowerCenter;
                        Styles.multibrushIconText.normal.textColor = Color.white;
                        Styles.multibrushIconText.Draw(itemRect, brush.prefabSlots[i].gameObject.name, false, false, false, false);

                        // Remove button
                        Rect closeButtonRect = TransformRect(itemRect, 0.75f, 0.05f, 0.2f, 0.2f);
                        GUI.DrawTexture(closeButtonRect, m_CloseIconTexture);
                    }
                    else
                    {
                        Styles.multibrushIconText.alignment = TextAnchor.MiddleCenter;
                        Styles.multibrushIconText.normal.textColor = Color.white;
                        Styles.multibrushIconText.Draw(itemRect, "Drag\nprefab\nhere", false, false, false, false);

                        EditorGUI.DrawRect(itemRect, new Color(0, 0, 0, 0.15f));
                    }

                    // prefab number
                    Styles.multibrushIconText.alignment = TextAnchor.UpperLeft;
                    Styles.multibrushIconText.normal.textColor = Color.white;
                    Styles.multibrushIconText.Draw(itemRect, "#" + i, false, false, false, false);
                }


                if(itemRect.Contains(e.mousePosition))
                {
                    itemUnderCursor = i;

                    if (e.type == EventType.MouseDown && e.button == 0)
                    {
                        // Clear button
                        Rect buttonRect = TransformRect(itemRect, 0.75f, 0.05f, 0.2f, 0.2f);
                        if (buttonRect.Contains(e.mousePosition))
                        {
                            int index = i;
                            EditorApplication.delayCall += () => { brush.ClearPrefab(index); };
                            e.Use();
                        }

                        if(e.clickCount == 2) // Double click
                        {
                            m_MultibrushItemDoubleClicked = i;
                            this.SendEvent(EditorGUIUtility.CommandEvent("MultibrushShowObjectSelector"));
                            e.Use();
                        }
                        else // single click - select
                        {
                            brush.SelectPrefab(i);
                            e.Use();
                        }
                    }

                    // Context Menu
                    if (e.type == EventType.ContextClick)
                    {
                        MultibrushPrefabMenu(brush, i).ShowAsContext();
                        e.Use();
                    }
                }

                // Rating Bar
                float raiting = RaitingBar(new Rect(itemRect.x, itemRect.y + itemRect.height + raitigBarBorder, itemRect.width, raitingBarHeight), brush.settings.multibrushSlots[i].raiting);
                if(raiting != brush.settings.multibrushSlots[i].raiting)
                {
                    brush.settings.multibrushSlots[i].raiting = raiting;
                }


                itemRect.x += iconSize + borderSize*2;
            }

            GUI.EndGroup();



            // Scroll Bar
            scrollPosition = HorizontalScrollBar(windowRect, scrollPosition, viewRect);
            SetMultibrushScrollPosition(brush, scrollPosition);




            // Object Picker Window
            if (e.type == EventType.ValidateCommand)
            {
                switch(e.commandName)
                {
                case "MultibrushShowObjectSelector":
                case "ObjectSelectorClosed":
                    e.Use();
                    break;
                }
            }
            if (e.type == EventType.ExecuteCommand)
            {
                switch(e.commandName)
                {
                case "MultibrushShowObjectSelector":
                    EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "", s_MultibrushObjectPickerHash);
                    e.Use();
                    break;
                case "ObjectSelectorClosed":
                    if(EditorGUIUtility.GetObjectPickerControlID() == s_MultibrushObjectPickerHash)
                    {
                        UnityEngine.Object pickedObject = EditorGUIUtility.GetObjectPickerObject();

                        if (IsAcceptablePrefab(pickedObject))
                        {
                                brush.AssignPrefab(pickedObject as GameObject, m_MultibrushItemDoubleClicked);
                        }

                        e.Use();
                    }
                    break;
                }
            }




            // Drag & Drop
            if((e.type == EventType.DragUpdated || e.type == EventType.DragPerform) && itemUnderCursor != -1)
            {
                if(DragAndDrop.objectReferences.Length > 1)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (e.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag ();

                        foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                        {
                            if (IsAcceptablePrefab(draggedObject))
                            {
                                        brush.AssignPrefabToEmptySlot(draggedObject as GameObject);
                            }
                        }
                    }
                }
                else
                {
                    if(brush.prefabSlots[itemUnderCursor].gameObject != null)
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    else
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;


                    if (e.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag ();

                        foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                        {
                            if (IsAcceptablePrefab(draggedObject))
                            {
                                brush.AssignPrefab(draggedObject as GameObject, itemUnderCursor);
                            }
                        }
                    }
                }

                e.Use();
            }






            if(showSettings)
            {
                int selectedSlot = brush.selectedSlot;
                BrushSettings brushSettings = brush.settings;

                m_MultibrushSlotSettingsFoldout = InternalGUI.FoldoutReset(m_MultibrushSlotSettingsFoldout,
                    () => { Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Slot Settings"); brush.settings.ResetMultibrushSlot(brush.selectedSlot); },
                    "Slot #" + brush.selectedSlot + " Settings", m_ResetIconTexture);

                if(m_MultibrushSlotSettingsFoldout)
                {
                    ++EditorGUI.indentLevel;
                    MakeUndoOnChange(ref brushSettings.multibrushSlots[selectedSlot].pivotMode, (PivotMode)EditorGUILayout.EnumPopup("Pivot Mode", brushSettings.multibrushSlots[selectedSlot].pivotMode));
                    //if (m_LastTool == PaintTool.Brush)
                    {
                        MakeUndoOnChange(ref brushSettings.multibrushSlots[selectedSlot].position, InternalGUI.Vector3Field(new GUIContent("Position"), brushSettings.multibrushSlots[selectedSlot].position));
                        MakeUndoOnChange(ref brushSettings.multibrushSlots[selectedSlot].rotation, InternalGUI.Vector3Field(new GUIContent("Rotation"), brushSettings.multibrushSlots[selectedSlot].rotation));
                        MakeUndoOnChange(ref brushSettings.multibrushSlots[selectedSlot].scale, InternalGUI.Vector3Field(new GUIContent("Scale"), brushSettings.multibrushSlots[selectedSlot].scale));
                    }
                    --EditorGUI.indentLevel;
                }

                MakeUndoOnChange(ref brushSettings.multibrushPaintSelectedSlot, EditorGUILayout.Toggle (new GUIContent("Paint Selected Slot"), brushSettings.multibrushPaintSelectedSlot));
                GUI.enabled = !brushSettings.multibrushPaintSelectedSlot;
                MakeUndoOnChange(ref brushSettings.multibrushMode, (MultibrushMode)EditorGUILayout.EnumPopup (new GUIContent("Mode"), brushSettings.multibrushMode));
                if(brushSettings.multibrushMode == MultibrushMode.Pattern)
                {
                    MakeUndoOnChange(ref brushSettings.multibrushPattern, EditorGUILayout.TextField (new GUIContent("Pattern"), brushSettings.multibrushPattern));
                    MakeUndoOnChange(ref brushSettings.multibrushPatternContinue, EditorGUILayout.Toggle (new GUIContent("Continue Pattern", "Continue pattern from last position in new stroke"), brushSettings.multibrushPatternContinue));
                }
                GUI.enabled = true;
            }



            --EditorGUI.indentLevel;

            EditorGUILayout.Space();
        }





        void GridUI(Brush brush, BrushSettings brushSettings)
        {
            if (brush.settings.gridEnabled)
            {
                m_GridFoldout = InternalGUI.FoldoutReset(m_GridFoldout,
                    () => { Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Grid"); brush.settings.ResetGrid(); }, "Grid", m_ResetIconTexture);
                if (m_GridFoldout)
                {
                    ++EditorGUI.indentLevel;
                    MakeUndoOnChange(ref brushSettings.gridOrigin, InternalGUI.Vector2Field(new GUIContent("Origin"), brushSettings.gridOrigin));
                    MakeUndoOnChange(ref brushSettings.gridStep, Vector2.Max(new Vector2(0.001f, 0.001f), InternalGUI.Vector2Field(new GUIContent("Step"), brushSettings.gridStep)));
                    MakeUndoOnChange(ref brushSettings.gridAngle, EditorGUILayout.FloatField(new GUIContent("Angle"), brushSettings.gridAngle));
                    MakeUndoOnChange(ref brushSettings.gridPlane, (GridPlane)EditorGUILayout.EnumPopup(new GUIContent("Plane"), brushSettings.gridPlane));
                    if (brushSettings.gridPlane == GridPlane.Custom)
                    {
                        ++EditorGUI.indentLevel;
                        EditorGUILayout.BeginHorizontal();
                        MakeUndoOnChange(ref brushSettings.gridNormal, InternalGUI.Vector3Field(new GUIContent("Normal Vector"), brushSettings.gridNormal));

                        string pickMessage = "Pick Grid Orientation";
                        if (InternalGUI.Button(EditorGUILayout.GetControlRect(GUILayout.Width(18)), new GUIContent("+", "Pick vector in scene view"), (m_CurrentTool == PaintTool.PickObject && pickMessage == m_OnPickObjectMessage)))
                        {
                            PickObject(pickMessage, (raycastInfo) => {
                                if (raycastInfo.isHit)
                                {
                                    MakeUndoOnChange(ref brushSettings.gridNormal, raycastInfo.normal);
                                }
                            });
                        }
                        EditorGUILayout.EndHorizontal();
                        --EditorGUI.indentLevel;
                    }
                    --EditorGUI.indentLevel;
                }
            }
        }


        void BrushSettingsGUI()
        {
            if(m_SceneSettings == null)
                return;

            Tab activeTab = m_Settings.GetActiveTab();
            bool hasSelectedBrushes = activeTab.HasSelectedBrushes();
            bool hasMultipleSelectedBrushes = activeTab.HasMultipleSelectedBrushes();
            Brush brush = activeTab.GetFirstSelectedBrush();
            BrushSettings brushSettings = brush != null ? brush.settings : null;

            // Begin Scroll area
            m_WindowScrollPos = EditorGUILayout.BeginScrollView(m_WindowScrollPos);

            switch(m_LastTool)
            {
            case PaintTool.None:
            case PaintTool.Brush:
                {
                    if (!hasSelectedBrushes || brush == null)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox(Strings.selectBrush, MessageType.Info);
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        break;
                    }
                    else if (hasMultipleSelectedBrushes)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox(Strings.multiSelBrush, MessageType.Info);
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        break;
                    }





                    m_BrushToolFoldout = InternalGUI.FoldoutReset(m_BrushToolFoldout,
                        () => { Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Brush Settings"); brushSettings.ResetBrush(); }, Strings.brushSettingsFoldout, m_ResetIconTexture);
                    if (m_BrushToolFoldout)
                    {
                        ++EditorGUI.indentLevel;

                        MakeUndoOnChange(ref brush.name, EditorGUILayout.DelayedTextField(Strings.brushName, brush.name));
                        MakeUndoOnChange(ref brushSettings.brushRadius, EditorGUILayout.Slider(Strings.brushRadius, brushSettings.brushRadius, 0.01f, m_Settings.maxBrushRadius));
                        MakeUndoOnChange(ref brushSettings.brushSpacing, EditorGUILayout.Slider(Strings.brushSpacing, brushSettings.brushSpacing, 0.01f, m_Settings.maxBrushSpacing));

                        MakeUndoOnChange(ref brushSettings.brushOverlapCheckMode, (OverlapCheckMode)EditorGUILayout.EnumPopup(new GUIContent("Overlap Check"), brushSettings.brushOverlapCheckMode));

                        if (brushSettings.brushOverlapCheckMode != OverlapCheckMode.None)
                        {
                            ++EditorGUI.indentLevel;

                            if(brushSettings.brushOverlapCheckMode == OverlapCheckMode.Distance)
                            {
                                MakeUndoOnChange(ref brushSettings.brushOverlapDistance, EditorGUILayout.FloatField (new GUIContent("Min Distance"), brushSettings.brushOverlapDistance));
                            }
                            MakeUndoOnChange(ref brushSettings.brushOverlapCheckObjects, (OverlapCheckObjects)EditorGUILayout.EnumPopup (new GUIContent("Check"), brushSettings.brushOverlapCheckObjects));
                            if(brushSettings.brushOverlapCheckObjects == OverlapCheckObjects.OtherLayers)
                            {
                                MakeUndoOnChange(ref brushSettings.brushOverlapCheckLayers, InternalGUI.LayerMaskField(new GUIContent("Layers"), brushSettings.brushOverlapCheckLayers));
                            }
                            --EditorGUI.indentLevel;
                        }
                        --EditorGUI.indentLevel;
                    }


                    MultibrushGUI(brush, true);


                    m_PositionSettingsFoldout = InternalGUI.FoldoutReset(m_PositionSettingsFoldout,
                        () => { Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Position Settings"); brushSettings.ResetPosition(); }, Strings.positionSettingsFoldout, m_ResetIconTexture);
                    if (m_PositionSettingsFoldout)
                    {
                        ++EditorGUI.indentLevel;
                        if (!brush.settings.multibrushEnabled)
                        {
                            int slot = brush.GetFirstAssociatedPrefabSlot();
                            slot = slot >= 0 ? slot : 0;
                            MakeUndoOnChange(ref brushSettings.multibrushSlots[slot].pivotMode, (PivotMode)EditorGUILayout.EnumPopup("Pivot Mode", brushSettings.multibrushSlots[slot].pivotMode));
                        }
                        MakeUndoOnChange(ref brushSettings.surfaceOffset, EditorGUILayout.FloatField("Surface Offset", brushSettings.surfaceOffset));

                        --EditorGUI.indentLevel;
                    }





                    m_OrientationSettingsFoldout = InternalGUI.FoldoutReset(m_OrientationSettingsFoldout,
                        () => { Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Orientation Settings"); brushSettings.ResetOrientation(); }, Strings.orientationSettingsFoldout, m_ResetIconTexture);
                    if (m_OrientationSettingsFoldout)
                    {
                        ++EditorGUI.indentLevel;
                        MakeUndoOnChange(ref brushSettings.orientationTransformMode, (TransformMode)EditorGUILayout.EnumPopup (Strings.brushOrientationTransform, brushSettings.orientationTransformMode));
                        MakeUndoOnChange(ref brushSettings.orientationMode, (OrientationMode)EditorGUILayout.EnumPopup ("Up", brushSettings.orientationMode));
                        MakeUndoOnChange(ref brushSettings.alongBrushStroke, EditorGUILayout.Toggle ("Along Brush Stroke", brushSettings.alongBrushStroke));
                        MakeUndoOnChange(ref brushSettings.rotation, InternalGUI.Vector3Field(Strings.brushRotation, brushSettings.rotation));
                        MakeUndoOnChange(ref brushSettings.randomizeOrientationX, EditorGUILayout.Slider (Strings.brushRandomizeOrientationX, brushSettings.randomizeOrientationX, 0.0f, 100.0f));
                        MakeUndoOnChange(ref brushSettings.randomizeOrientationY, EditorGUILayout.Slider (Strings.brushRandomizeOrientationY, brushSettings.randomizeOrientationY, 0.0f, 100.0f));
                        MakeUndoOnChange(ref brushSettings.randomizeOrientationZ, EditorGUILayout.Slider (Strings.brushRandomizeOrientationZ, brushSettings.randomizeOrientationZ, 0.0f, 100.0f));
                        --EditorGUI.indentLevel;
                    }



                    m_ScaleSettingsFoldout = InternalGUI.FoldoutReset(m_ScaleSettingsFoldout,
                        () => { Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Scale Settings"); brush.settings.ResetScale(); }, Strings.scaleSettingsFoldout, m_ResetIconTexture);
                    if (m_ScaleSettingsFoldout)
                    {
                        ++EditorGUI.indentLevel;
                        MakeUndoOnChange(ref brushSettings.scaleTransformMode, (TransformMode)EditorGUILayout.EnumPopup (Strings.brushScaleTransformMode, brushSettings.scaleTransformMode));
                        MakeUndoOnChange(ref brushSettings.scaleMode, (AxisMode)EditorGUILayout.EnumPopup (Strings.brushScaleMode, brushSettings.scaleMode));
                        if (brushSettings.scaleMode == AxisMode.Uniform)
                        {
                            MakeUndoOnChange(ref brushSettings.scaleUniformMin, EditorGUILayout.FloatField (Strings.brushScaleUniformMin, brushSettings.scaleUniformMin));
                            MakeUndoOnChange(ref brushSettings.scaleUniformMax, EditorGUILayout.FloatField (Strings.brushScaleUniformMax, brushSettings.scaleUniformMax));
                        }
                        else
                        {
                            MakeUndoOnChange(ref brushSettings.scalePerAxisMin, InternalGUI.Vector3Field (Strings.brushScalePerAxisMin, brushSettings.scalePerAxisMin));
                            MakeUndoOnChange(ref brushSettings.scalePerAxisMax, InternalGUI.Vector3Field (Strings.brushScalePerAxisMax, brushSettings.scalePerAxisMax));
                        }
                        --EditorGUI.indentLevel;
                    }

                    if(brush.settings.slopeEnabled)
                    {
                        m_SlopeFilterFoldout = InternalGUI.FoldoutReset(m_SlopeFilterFoldout,
                            () => { Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Slope Filter"); brush.settings.ResetSlopeFilter(); }, "Slope Filter", m_ResetIconTexture);
                        if (m_SlopeFilterFoldout)
                        {
                            ++EditorGUI.indentLevel;
                            MakeUndoOnChange(ref brushSettings.slopeAngleMin, Mathf.Clamp(EditorGUILayout.FloatField (new GUIContent("Min Angle"), brushSettings.slopeAngleMin), 0f, 180f));
                            MakeUndoOnChange(ref brushSettings.slopeAngleMax, Mathf.Clamp(EditorGUILayout.FloatField (new GUIContent("Max Angle"), brushSettings.slopeAngleMax), 0f, 180f));
                            MakeUndoOnChange(ref brushSettings.slopeVector, (SlopeVector)EditorGUILayout.EnumPopup (new GUIContent("Reference Vector"), brushSettings.slopeVector));
                            if(brushSettings.slopeVector == SlopeVector.Custom)
                            {
                                ++EditorGUI.indentLevel;
                                EditorGUILayout.BeginHorizontal();
                                MakeUndoOnChange(ref brushSettings.slopeVectorCustom, InternalGUI.Vector3Field(new GUIContent("Custom Vector"), brushSettings.slopeVectorCustom));

                                string pickMessage = "Pick Slope Vector";
                                if (InternalGUI.Button(EditorGUILayout.GetControlRect(GUILayout.Width(18)), new GUIContent("+", "Pick vector in scene view"), m_CurrentTool == PaintTool.PickObject && m_OnPickObjectMessage == pickMessage))
                                {
                                    PickObject(pickMessage, (raycastInfo) => {
                                        if(raycastInfo.isHit) {
                                            MakeUndoOnChange(ref brushSettings.slopeVectorCustom, raycastInfo.normal);
                                        }
                                    });
                                }

                                EditorGUILayout.EndHorizontal();
                                --EditorGUI.indentLevel;
                            }
                            MakeUndoOnChange(ref brushSettings.slopeVectorFlip, EditorGUILayout.Toggle (new GUIContent("Flip Vector"), brushSettings.slopeVectorFlip));
                            --EditorGUI.indentLevel;
                        }
                    }

                    GridUI(brush, brushSettings);


                }
                break;
            case PaintTool.Pin:
                {
                    if (!hasSelectedBrushes || brush == null)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox(Strings.selectBrush, MessageType.Info);
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        break;
                    }
                    else if (hasMultipleSelectedBrushes)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox(Strings.multiSelBrush, MessageType.Info);
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        break;
                    }






                    m_PinToolFoldout = InternalGUI.FoldoutReset(m_PinToolFoldout,
                        () => {
                            Undo.RegisterCompleteObjectUndo(m_Settings, "Reset Settings");
                            m_Settings.ResetPinSnapSettings();
                        }, "Pin Settings", m_ResetIconTexture);

                    if (m_PinToolFoldout)
                    {
                        ++EditorGUI.indentLevel;

                        MakeUndoOnChange(ref brush.name, EditorGUILayout.DelayedTextField(Strings.brushName, brush.name));

                        EditorGUILayout.BeginHorizontal();
                        if (m_CurrentTool == PaintTool.Pin && Event.current.control)
                            EditorGUILayout.Toggle(new GUIContent("Snap Rotation"), !m_Settings.pinSnapRotation);
                        else
                            MakeUndoOnChange(ref m_Settings.pinSnapRotation, EditorGUILayout.Toggle(new GUIContent("Snap Rotation"), m_Settings.pinSnapRotation));

                        EditorGUILayout.LabelField("(Hold Control)", Styles.leftGreyMiniLabel);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        if (m_CurrentTool == PaintTool.Pin && Event.current.shift)
                            EditorGUILayout.Toggle(new GUIContent("Snap Scale"), !m_Settings.pinSnapScale);
                        else
                            MakeUndoOnChange(ref m_Settings.pinSnapScale, EditorGUILayout.Toggle(new GUIContent("Snap Scale"), m_Settings.pinSnapScale));
                        EditorGUILayout.LabelField("(Hold Shift)", Styles.leftGreyMiniLabel);
                        EditorGUILayout.EndHorizontal();

                        MakeUndoOnChange(ref m_Settings.pinSnapRotationValue, Mathf.Max(EditorGUILayout.FloatField(new GUIContent("Snap Rotation Angle"), m_Settings.pinSnapRotationValue), 0.001f));
                        MakeUndoOnChange(ref m_Settings.pinSnapScaleValue, Mathf.Max(EditorGUILayout.FloatField(new GUIContent("Snap Scale Value"), m_Settings.pinSnapScaleValue), 0.001f));

                        --EditorGUI.indentLevel;
                    }

                    m_PositionSettingsFoldout = InternalGUI.FoldoutReset(m_PositionSettingsFoldout,
                       () => { Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Position Settings"); brushSettings.ResetPosition(); }, Strings.positionSettingsFoldout, m_ResetIconTexture);
                    if (m_PositionSettingsFoldout)
                    {
                        ++EditorGUI.indentLevel;
                        if (!brush.settings.multibrushEnabled)
                        {
                            int slot = brush.GetFirstAssociatedPrefabSlot();
                            slot = slot >= 0 ? slot : 0;
                            MakeUndoOnChange(ref brushSettings.multibrushSlots[slot].pivotMode, (PivotMode)EditorGUILayout.EnumPopup("Pivot Mode", brushSettings.multibrushSlots[slot].pivotMode));
                        }
                        MakeUndoOnChange(ref brushSettings.surfaceOffset, EditorGUILayout.FloatField(Strings.brushPositionOffset, brushSettings.surfaceOffset));
                        --EditorGUI.indentLevel;
                    }

                    m_OrientationSettingsFoldout = InternalGUI.FoldoutReset(m_OrientationSettingsFoldout,
                        () => { Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Orientation Settings"); brushSettings.ResetPinOrientation(); }, Strings.orientationSettingsFoldout, m_ResetIconTexture);
                    if (m_OrientationSettingsFoldout)
                    {
                        ++EditorGUI.indentLevel;
                        MakeUndoOnChange(ref brushSettings.orientationTransformMode, (TransformMode)EditorGUILayout.EnumPopup(Strings.brushOrientationTransform, brushSettings.orientationTransformMode));
                        MakeUndoOnChange(ref brushSettings.orientationMode, (OrientationMode)EditorGUILayout.EnumPopup("Up", brushSettings.orientationMode));
                        MakeUndoOnChange(ref brushSettings.pinFixedRotation, EditorGUILayout.Toggle(new GUIContent("Fixed Rotation"), brushSettings.pinFixedRotation));
                        if (brushSettings.pinFixedRotation)
                        {
                            ++EditorGUI.indentLevel;
                            MakeUndoOnChange(ref brushSettings.pinFixedRotationValue, InternalGUI.Vector3Field(new GUIContent("Rotation"), brushSettings.pinFixedRotationValue));
                            --EditorGUI.indentLevel;
                        }
                        --EditorGUI.indentLevel;
                    }



                    m_ScaleSettingsFoldout = InternalGUI.FoldoutReset(m_ScaleSettingsFoldout,
                        () => { Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Scale Settings"); brush.settings.ResetPinScale(); }, Strings.scaleSettingsFoldout, m_ResetIconTexture);
                    if (m_ScaleSettingsFoldout)
                    {
                        ++EditorGUI.indentLevel;
                        MakeUndoOnChange(ref brushSettings.scaleTransformMode, (TransformMode)EditorGUILayout.EnumPopup(Strings.brushScaleTransformMode, brushSettings.scaleTransformMode));
                        MakeUndoOnChange(ref brushSettings.pinFixedScale, EditorGUILayout.Toggle(new GUIContent("Fixed Scale"), brushSettings.pinFixedScale));
                        if (brushSettings.pinFixedScale)
                        {
                            ++EditorGUI.indentLevel;

                            MakeUndoOnChange(ref brushSettings.pinFixedScaleValue, InternalGUI.Vector3Field(new GUIContent("Scale"), brushSettings.pinFixedScaleValue));
                            --EditorGUI.indentLevel;
                        }
                        --EditorGUI.indentLevel;
                    }

                    MultibrushGUI(brush, true);

                    GridUI(brush, brushSettings);
                }
                break;
            case PaintTool.Place:
                {
                    if (!hasSelectedBrushes || brush == null)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox(Strings.selectBrush, MessageType.Info);
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        break;
                    }
                    else if (hasMultipleSelectedBrushes)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox(Strings.multiSelBrush, MessageType.Info);
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        break;
                    }

                    m_PlaceToolFoldout = InternalGUI.FoldoutReset(m_PlaceToolFoldout,
                        () => {
                            Undo.RegisterCompleteObjectUndo(m_Settings, "Reset Settings");
                            m_Settings.ResetPlaceSettings();
                        }, "Place Settings", m_ResetIconTexture);

                    if (m_PlaceToolFoldout)
                    {
                        ++EditorGUI.indentLevel;

                        MakeUndoOnChange(ref brush.name, EditorGUILayout.DelayedTextField(Strings.brushName, brush.name));
                        MakeUndoOnChange(ref m_Settings.placeAngleStep, Mathf.Max(EditorGUILayout.FloatField(new GUIContent("Rotation Step"), m_Settings.placeAngleStep), 0.001f));
                        MakeUndoOnChange(ref m_Settings.placeScaleStep, Mathf.Max(EditorGUILayout.FloatField(new GUIContent("Scale Step"), m_Settings.placeScaleStep), 0.001f));

                        --EditorGUI.indentLevel;
                    }

                    EditorGUI.BeginChangeCheck();

                    m_PositionSettingsFoldout = InternalGUI.FoldoutReset(m_PositionSettingsFoldout,
                       () => { Undo.RegisterCompleteObjectUndo(m_Settings, "Reset Position Settings"); brushSettings.ResetPosition(); }, Strings.positionSettingsFoldout, m_ResetIconTexture);
                    if (m_PositionSettingsFoldout)
                    {
                        ++EditorGUI.indentLevel;

                        if(!brush.settings.multibrushEnabled)
                        {
                            int slot = brush.GetFirstAssociatedPrefabSlot();
                            slot = slot >= 0 ? slot : 0;
                            MakeUndoOnChange(ref brushSettings.multibrushSlots[slot].pivotMode, (PivotMode)EditorGUILayout.EnumPopup("Pivot Mode", brushSettings.multibrushSlots[slot].pivotMode));
                        }

                        MakeUndoOnChange(ref brushSettings.surfaceOffset, EditorGUILayout.FloatField(Strings.brushPositionOffset, brushSettings.surfaceOffset));
                        --EditorGUI.indentLevel;
                    }

                    m_OrientationSettingsFoldout = InternalGUI.FoldoutReset(m_OrientationSettingsFoldout,
                        () => { Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Orientation Settings"); brushSettings.ResetPlaceOrientation(); }, Strings.orientationSettingsFoldout, m_ResetIconTexture);
                    if (m_OrientationSettingsFoldout)
                    {
                        ++EditorGUI.indentLevel;
                        MakeUndoOnChange(ref brushSettings.orientationTransformMode, (TransformMode)EditorGUILayout.EnumPopup(Strings.brushOrientationTransform, brushSettings.orientationTransformMode));
                        MakeUndoOnChange(ref brushSettings.orientationMode, (OrientationMode)EditorGUILayout.EnumPopup(new GUIContent("Up"), brushSettings.orientationMode));
                        MakeUndoOnChange(ref brushSettings.placeEulerAngles, InternalGUI.Vector3Field (new GUIContent("Rotation"), brushSettings.placeEulerAngles));
                        --EditorGUI.indentLevel;
                    }

                    m_ScaleSettingsFoldout = InternalGUI.FoldoutReset(m_ScaleSettingsFoldout,
                        () => { Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Scale Settings"); brush.settings.ResetPlaceScale(); }, Strings.scaleSettingsFoldout, m_ResetIconTexture);
                    if (m_ScaleSettingsFoldout)
                    {
                        ++EditorGUI.indentLevel;
                        MakeUndoOnChange(ref brushSettings.scaleTransformMode, (TransformMode)EditorGUILayout.EnumPopup(Strings.brushScaleTransformMode, brushSettings.scaleTransformMode));
                        MakeUndoOnChange(ref brushSettings.placeScale, EditorGUILayout.FloatField(new GUIContent("Scale"), brushSettings.placeScale));
                        --EditorGUI.indentLevel;
                    }

                    MultibrushGUI(brush, true);

                    GridUI(brush, brushSettings);

                    // if settings changed - recreate preview object
                    if (EditorGUI.EndChangeCheck() && m_CurrentTool == PaintTool.Place && m_PlaceTool.placedObjectInfo != null)
                    {
                        DestroyObject(m_PlaceTool.placedObjectInfo);
                        m_PlaceTool.placedObjectInfo = null;
                    }

                    break;
                }
            case PaintTool.Throw:
                {
                    if (!hasSelectedBrushes || brush == null)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox(Strings.selectBrush, MessageType.Info);
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        break;
                    }
                    else if (hasMultipleSelectedBrushes)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox(Strings.multiSelBrush, MessageType.Info);
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        break;
                    }

                    m_ThrowToolFoldout = InternalGUI.FoldoutReset(m_ThrowToolFoldout,
                        () =>
                        {
                            Undo.RegisterCompleteObjectUndo(m_Settings, "Reset Settings");
                            brushSettings.ResetThrowSettings();
                        }, "Throw Settings", m_ResetIconTexture);

                    if (m_ThrowToolFoldout)
                    {
                        ++EditorGUI.indentLevel;

                        MakeUndoOnChange(ref brush.name, EditorGUILayout.DelayedTextField(Strings.brushName, brush.name));
                        MakeUndoOnChange(ref brushSettings.throwRadius, EditorGUILayout.Slider(Strings.brushRadius, brushSettings.throwRadius, 0.01f, m_Settings.maxBrushRadius));
                        MakeUndoOnChange(ref brushSettings.throwSpacing, EditorGUILayout.Slider(Strings.brushSpacing, brushSettings.throwSpacing, 0.01f, m_Settings.maxBrushRadius));

                        MakeUndoOnChange(ref brushSettings.throwVelocity, EditorGUILayout.FloatField("Velocity", brushSettings.throwVelocity));
                        MakeUndoOnChange(ref brushSettings.throwHeight, EditorGUILayout.FloatField("Height", brushSettings.throwHeight));

                        --EditorGUI.indentLevel;

                        m_OrientationSettingsFoldout = InternalGUI.FoldoutReset(m_OrientationSettingsFoldout,
                            () => { Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Orientation Settings"); brushSettings.ResetThrowOrientation(); }, Strings.orientationSettingsFoldout, m_ResetIconTexture);
                        if (m_OrientationSettingsFoldout)
                        {
                            ++EditorGUI.indentLevel;
                            MakeUndoOnChange(ref brushSettings.orientationTransformMode,
                                (TransformMode) EditorGUILayout.EnumPopup(Strings.brushOrientationTransform,
                                    brushSettings.orientationTransformMode));
                            MakeUndoOnChange(ref brushSettings.throwRandomRotation.x,
                                EditorGUILayout.Slider(Strings.brushRandomizeOrientationX,
                                    brushSettings.throwRandomRotation.x, 0.0f, 100.0f));
                            MakeUndoOnChange(ref brushSettings.throwRandomRotation.y,
                                EditorGUILayout.Slider(Strings.brushRandomizeOrientationY,
                                    brushSettings.throwRandomRotation.y, 0.0f, 100.0f));
                            MakeUndoOnChange(ref brushSettings.throwRandomRotation.z,
                                EditorGUILayout.Slider(Strings.brushRandomizeOrientationZ,
                                    brushSettings.throwRandomRotation.z, 0.0f, 100.0f));
                            --EditorGUI.indentLevel;
                        }


                        m_ScaleSettingsFoldout = InternalGUI.FoldoutReset(m_ScaleSettingsFoldout,
                            () => { Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Scale Settings"); brush.settings.ResetThrowScale(); }, Strings.scaleSettingsFoldout, m_ResetIconTexture);
                        if (m_ScaleSettingsFoldout)
                        {
                            ++EditorGUI.indentLevel;
                            MakeUndoOnChange(ref brushSettings.scaleTransformMode,
                                (TransformMode) EditorGUILayout.EnumPopup(Strings.brushScaleTransformMode,
                                    brushSettings.scaleTransformMode));
                            MakeUndoOnChange(ref brushSettings.throwScaleMinMax.x,
                                EditorGUILayout.FloatField(Strings.brushScaleUniformMin,
                                    brushSettings.throwScaleMinMax.x));
                            MakeUndoOnChange(ref brushSettings.throwScaleMinMax.y,
                                EditorGUILayout.FloatField(Strings.brushScaleUniformMax,
                                    brushSettings.throwScaleMinMax.y));
                            --EditorGUI.indentLevel;
                        }

                        MultibrushGUI(brush, true);

                        /*m_PhysicsSettingsFoldout = InternalGUI.FoldoutReset(m_PhysicsSettingsFoldout,
                            () => { Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Physics Settings"); m_Settings.ResetThrowSettings(); }, "Physics", m_ResetIconTexture);
                        if (m_PhysicsSettingsFoldout)
                        {
                            ++EditorGUI.indentLevel;
                            --EditorGUI.indentLevel;
                        }*/

                    }
                }
                break;
            case PaintTool.Erase:
                {
                    if (!hasSelectedBrushes && !m_Settings.eraseByLayer)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox("Select Brushes (you can select multiple)", MessageType.Info);
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                    }


                    m_EraseToolFoldout = InternalGUI.FoldoutReset(m_EraseToolFoldout,
                        () => {
                            Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Erase Settings");
                            m_Settings.ResetEraseSettings();
                        }, "Erase Settings", m_ResetIconTexture);

                    if (m_EraseToolFoldout)
                    {
                        ++EditorGUI.indentLevel;
                        if(hasSelectedBrushes && !hasMultipleSelectedBrushes)
                            MakeUndoOnChange(ref brush.name, EditorGUILayout.DelayedTextField(Strings.brushName, brush.name));

                        MakeUndoOnChange(ref m_Settings.eraseBrushRadius, EditorGUILayout.Slider(Strings.brushRadius, m_Settings.eraseBrushRadius, 0.01f, m_Settings.maxBrushRadius));
                        MakeUndoOnChange(ref m_Settings.eraseByLayer, EditorGUILayout.Toggle (new GUIContent("Erase By Layer"), m_Settings.eraseByLayer));
                        GUI.enabled = m_Settings.eraseByLayer;
                        MakeUndoOnChange(ref m_Settings.eraseLayers, InternalGUI.LayerMaskField (new GUIContent("Erase Layers"), m_Settings.eraseLayers));
                        GUI.enabled = true;
                        --EditorGUI.indentLevel;
                    }


                    if(brush != null && !hasMultipleSelectedBrushes)
                        MultibrushGUI(brush, false);
                }
                break;
            case PaintTool.Select:
                if (!hasSelectedBrushes && !m_Settings.selectByLayer)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("Select Brushes (you can select multiple)", MessageType.Info);
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }



                m_SelectToolFoldout = InternalGUI.FoldoutReset(m_SelectToolFoldout,
                    () => {
                        Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Select Settings");
                        m_Settings.ResetSelectSettings();
                    }, "Select Settings", m_ResetIconTexture);

                if (m_SelectToolFoldout)
                {
                    ++EditorGUI.indentLevel;

                    if(hasSelectedBrushes && !hasMultipleSelectedBrushes)
                        MakeUndoOnChange(ref brush.name, EditorGUILayout.DelayedTextField(Strings.brushName, brush.name));

                    MakeUndoOnChange(ref m_Settings.selectBrushRadius, EditorGUILayout.Slider(Strings.brushRadius, m_Settings.selectBrushRadius,  0.01f, m_Settings.maxBrushRadius));


                    MakeUndoOnChange(ref m_Settings.selectByLayer, EditorGUILayout.Toggle (new GUIContent("Select By Layer"), m_Settings.selectByLayer));

                    GUI.enabled = m_Settings.selectByLayer;
                    MakeUndoOnChange(ref m_Settings.selectLayers, InternalGUI.LayerMaskField (new GUIContent("Select Layers"), m_Settings.selectLayers));
                    GUI.enabled = true;

                    --EditorGUI.indentLevel;
                }

                if(brush != null && !hasMultipleSelectedBrushes)
                    MultibrushGUI(brush, false);


                break;
            case PaintTool.Modify:
                if (!hasSelectedBrushes && !m_Settings.modifyByLayer)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("Select Brushes (you can select multiple)", MessageType.Info);
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }

                m_ModifyToolFoldout = InternalGUI.FoldoutReset(m_ModifyToolFoldout,
                    () => {
                        Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Settings");
                        m_Settings.ResetModifySettings();
                    }, "Modify Settings", m_ResetIconTexture);

                if (m_ModifyToolFoldout)
                {
                    ++EditorGUI.indentLevel;
                    if (hasSelectedBrushes && !hasMultipleSelectedBrushes)
                        MakeUndoOnChange(ref brush.name, EditorGUILayout.DelayedTextField(Strings.brushName, brush.name));

                    MakeUndoOnChange(ref m_Settings.modifyBrushRadius, EditorGUILayout.Slider(Strings.brushRadius, m_Settings.modifyBrushRadius, 0.01f, m_Settings.maxBrushRadius));
                    MakeUndoOnChange(ref m_Settings.modifyStrength, EditorGUILayout.Slider("Strength", m_Settings.modifyStrength, 0.0f, 20.0f));
                    MakeUndoOnChange(ref m_Settings.modifyByLayer, EditorGUILayout.Toggle(new GUIContent("Modify By Layer"), m_Settings.modifyByLayer));
                    GUI.enabled = m_Settings.modifyByLayer;
                    MakeUndoOnChange(ref m_Settings.modifyLayers, InternalGUI.LayerMaskField(new GUIContent("Modify Layers"), m_Settings.modifyLayers));
                    GUI.enabled = true;
                    --EditorGUI.indentLevel;
                }

                m_PositionSettingsFoldout = InternalGUI.FoldoutReset(m_PositionSettingsFoldout, () => {
                    Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Settings");
                    m_Settings.ResetModifyPositionSettings();
                }, Strings.positionSettingsFoldout, m_ResetIconTexture);

                if (m_PositionSettingsFoldout)
                {
                    ++EditorGUI.indentLevel;
                    MakeUndoOnChange(ref m_Settings.modifyPivotMode, (PivotMode)EditorGUILayout.EnumPopup("Pivot Mode", m_Settings.modifyPivotMode));
                    --EditorGUI.indentLevel;
                }

                m_OrientationSettingsFoldout = InternalGUI.FoldoutReset(m_OrientationSettingsFoldout, () => {
                    Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Settings");
                    m_Settings.ResetModifyOrientationSettings();
                }, "Rotation", m_ResetIconTexture);

                if (m_OrientationSettingsFoldout)
                {
                    ++EditorGUI.indentLevel;

                    MakeUndoOnChange(ref m_Settings.modifyRandomRotation, EditorGUILayout.Toggle("Random Rotation", m_Settings.modifyRandomRotation));

                    if (m_Settings.modifyRandomRotation)
                    {
                        MakeUndoOnChange(ref m_Settings.modifyRandomRotationValues.x, EditorGUILayout.Slider("Randomize X", m_Settings.modifyRandomRotationValues.x, 0.0f, 1.0f));
                        MakeUndoOnChange(ref m_Settings.modifyRandomRotationValues.y, EditorGUILayout.Slider("Randomize Y", m_Settings.modifyRandomRotationValues.y, 0.0f, 1.0f));
                        MakeUndoOnChange(ref m_Settings.modifyRandomRotationValues.z, EditorGUILayout.Slider("Randomize Z", m_Settings.modifyRandomRotationValues.z, 0.0f, 1.0f));
                    }
                    else
                    {
                        MakeUndoOnChange(ref m_Settings.modifyRotationValues.x, EditorGUILayout.Slider("Rotation X", m_Settings.modifyRotationValues.x, -1.0f, 1.0f));
                        MakeUndoOnChange(ref m_Settings.modifyRotationValues.y, EditorGUILayout.Slider("Rotation Y", m_Settings.modifyRotationValues.y, -1.0f, 1.0f));
                        MakeUndoOnChange(ref m_Settings.modifyRotationValues.z, EditorGUILayout.Slider("Rotation Z", m_Settings.modifyRotationValues.z, -1.0f, 1.0f));
                    }

                    --EditorGUI.indentLevel;
                }

                m_ScaleSettingsFoldout = InternalGUI.FoldoutReset(m_ScaleSettingsFoldout, () => {
                    Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Settings");
                    m_Settings.ResetModifyScaleSettings();
                }, Strings.scaleSettingsFoldout, m_ResetIconTexture);

                if (m_ScaleSettingsFoldout)
                {
                    ++EditorGUI.indentLevel;
                    MakeUndoOnChange(ref m_Settings.modifyScaleRandomize, EditorGUILayout.Slider("Randomize", m_Settings.modifyScaleRandomize, 0.0f, 100.0f));
                    MakeUndoOnChange(ref m_Settings.modifyScale, EditorGUILayout.Slider("Scale", m_Settings.modifyScale, -1.0f, 1.0f));
                    --EditorGUI.indentLevel;
                }

                break;
            case PaintTool.Orient:

                m_OrientToolFoldout = InternalGUI.FoldoutReset(m_OrientToolFoldout,
                () => {
                    Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Settings");
                    m_Settings.ResetOrientSettings();
                }, "Orient Settings", m_ResetIconTexture);

                if (m_OrientToolFoldout)
                {
                    ++EditorGUI.indentLevel;
                    MakeUndoOnChange(ref m_Settings.orientPivotMode, (PivotMode)EditorGUILayout.EnumPopup("Pivot Mode", m_Settings.orientPivotMode));
                    MakeUndoOnChange(ref m_Settings.orientLockUp, EditorGUILayout.Toggle(new GUIContent("Lock Object Up"), m_Settings.orientLockUp));
                    MakeUndoOnChange(ref m_Settings.orientSameDirection, EditorGUILayout.Toggle(new GUIContent("Same Direction"), m_Settings.orientSameDirection));
                    MakeUndoOnChange(ref m_Settings.orientFlipDirection, EditorGUILayout.Toggle(new GUIContent("Flip Direction"), m_Settings.orientFlipDirection));
                    MakeUndoOnChange(ref m_Settings.orientRotation, InternalGUI.Vector3Field(new GUIContent("Aux Rotation"), m_Settings.orientRotation));
                    --EditorGUI.indentLevel;
                }

                break;
            case PaintTool.Move:

                if (m_CurrentTool == PaintTool.Move && m_MoveTool.objects.Count == 0)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("Select object(s)", MessageType.Warning);
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }

                m_MoveToolFoldout = InternalGUI.FoldoutReset(m_MoveToolFoldout,
                () => {
                    Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Settings");
                    m_Settings.ResetMoveSettings();
                }, "Move Settings", m_ResetIconTexture);

                if (m_MoveToolFoldout)
                {
                    ++EditorGUI.indentLevel;
                    MakeUndoOnChange(ref m_Settings.moveSurfaceOffset, EditorGUILayout.FloatField(Strings.brushPositionOffset, m_Settings.moveSurfaceOffset));
                    MakeUndoOnChange(ref m_Settings.movePivotMode, (PivotMode)EditorGUILayout.EnumPopup("Pivot Mode", m_Settings.movePivotMode));
                    MakeUndoOnChange(ref m_Settings.moveLockUp, EditorGUILayout.Toggle(new GUIContent("Lock Object Up"), m_Settings.moveLockUp));
                    GUI.enabled = !m_Settings.moveLockUp;
                    MakeUndoOnChange(ref m_Settings.moveOrientationMode, (OrientationMode)EditorGUILayout.EnumPopup(new GUIContent("Up"), m_Settings.moveOrientationMode));
                    GUI.enabled = true;
                    --EditorGUI.indentLevel;
                }

                break;
            }







            m_CommonSettingsFoldout = InternalGUI.FoldoutReset(m_CommonSettingsFoldout,
                () => {
                    Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset Common Settings");
                    m_Settings.ResetCommonSettings();
                },
                Strings.commonSettingsFoldout, m_ResetIconTexture);
            if (m_CommonSettingsFoldout)
            {
                ++EditorGUI.indentLevel;

                EditorGUI.BeginChangeCheck();
                MakeUndoOnChange(ref m_Settings.paintOnSelected, EditorGUILayout.Toggle(Strings.brushPaintOnSelected, m_Settings.paintOnSelected));
                if (EditorGUI.EndChangeCheck())
                {
                    if (m_Settings.paintOnSelected) {
                        m_SelectedObjects = Selection.objects;
                    }
                }
                GUI.enabled = !m_Settings.paintOnSelected;
                MakeUndoOnChange(ref m_Settings.paintLayers, InternalGUI.LayerMaskField(Strings.brushPaintOnLayers, m_Settings.paintLayers));
                MakeUndoOnChange(ref m_Settings.ignoreLayers, InternalGUI.LayerMaskField(Strings.brushIgnoreLayers, m_Settings.ignoreLayers));
                GUI.enabled = true;


                MakeUndoOnChange(ref m_Settings.placeUnder, (Placement)EditorGUILayout.EnumPopup(Strings.brushPlaceUnder, m_Settings.placeUnder));
                if (m_Settings.placeUnder == Placement.CustomObject)
                {
                    ++EditorGUI.indentLevel;

                    GameObject newParentForPrefabs = (GameObject)EditorGUILayout.ObjectField(Strings.brushCustomSceneObject, m_SceneSettings.parentForPrefabs, typeof(GameObject), true);
                    if (newParentForPrefabs != m_SceneSettings.parentForPrefabs)
                    {
                        Undo.RegisterCompleteObjectUndo(m_SceneSettings, "PP: Change value");

                        m_SceneSettings.parentForPrefabs = newParentForPrefabs;

                        if (m_SceneSettings.parentForPrefabs != null && AssetDatabase.Contains(m_SceneSettings.parentForPrefabs)) {
                            m_SceneSettings.parentForPrefabs = null;
                        }

                        Utility.MarkActiveSceneDirty();
                    }
                    --EditorGUI.indentLevel;
                }

                MakeUndoOnChange(ref m_Settings.overwritePrefabLayer, EditorGUILayout.Toggle(Strings.brushOverwritePrefabLayer, m_Settings.overwritePrefabLayer));
                GUI.enabled = m_Settings.overwritePrefabLayer;
                ++EditorGUI.indentLevel;
                MakeUndoOnChange(ref m_Settings.prefabPlaceLayer, EditorGUILayout.LayerField(Strings.brushPrefabPlaceLayer, m_Settings.prefabPlaceLayer));
                --EditorGUI.indentLevel;
                GUI.enabled = true;

                --EditorGUI.indentLevel;
            }


            EditorGUILayout.EndScrollView();

        }


        void MakeUndoOnChange<T>(ref T rvalue, T lvalue)
        {
            if(!rvalue.Equals(lvalue))
            {
                RegisterValueUndo();
                rvalue = lvalue;
            }
        }

        void RegisterValueUndo()
        {
            Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Change value");
        }


        void OnMainGUI ()
        {
            // Logo
            Rect logoRect = EditorGUILayout.GetControlRect(GUILayout.Height(56));
            if(Event.current.type == EventType.Repaint)
                Styles.logoFont.Draw(logoRect, "nTools|PrefabPainter", false, false, false, false);

            //SerializedObject serializedObject = new SerializedObject();

            float kToolbarWidth = 280;
            float kToolbarHeight = 26;

            // Tool select
            EditorGUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            m_CurrentTool = (PaintTool)InternalGUI.Toolbar(EditorGUILayout.GetControlRect(GUILayout.MaxWidth(kToolbarWidth), GUILayout.Height(kToolbarHeight)), (int)m_CurrentTool, m_ToolbarItems);
            GUILayout.FlexibleSpace ();
            EditorGUILayout.EndHorizontal ();


            switch(m_CurrentTool)
            {
            case PaintTool.PickObject:
            case PaintTool.None:

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("No Tool Selected");
                EditorGUILayout.EndVertical();

                if (m_LastTool != PaintTool.Orient && m_LastTool != PaintTool.Move)
                {
                    TabsGUI();
                    BrushWindowGUI();
                }
                BrushSettingsGUI();
                break;
            case PaintTool.Brush:

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Brush Tool");
                EditorGUILayout.LabelField("Click and drag on surface to place objects", EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.EndVertical();


                TabsGUI();
                BrushWindowGUI();
                BrushSettingsGUI();
                break;
            case PaintTool.Pin:

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Pin Tool");
                EditorGUILayout.LabelField("Hold Control/Shift to snap rotation/scale", EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.EndVertical();

                TabsGUI();
                BrushWindowGUI();
                BrushSettingsGUI();
                break;
            case PaintTool.Place:

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Place Tool");
                EditorGUILayout.LabelField("Buttons: WS - scale, ADQEZC - rotate, X - reset", EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.EndVertical();

                TabsGUI();
                BrushWindowGUI();
                BrushSettingsGUI();
                break;
            case PaintTool.Throw:

                EditorGUILayout.HelpBox("EXPERIMENTAL TOOL. USE WITH CAUTION\n" +
                "It runs a physical simulation and could potentially move a rigidbody's/breaks the scene!", MessageType.Error);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Throw Tool");
                EditorGUILayout.LabelField("Click and drag on the surface to throw objects/Right-click to freeze them",
                    EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.EndVertical();

                TabsGUI();
                BrushWindowGUI();
                BrushSettingsGUI();
                break;
            case PaintTool.Erase:

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Erase Tool");
                EditorGUILayout.LabelField("Erase objects by layer/type", EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.EndVertical();

                TabsGUI();
                BrushWindowGUI();
                BrushSettingsGUI();
                break;
            case PaintTool.Select:

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Select Tool");
                EditorGUILayout.LabelField("Select objects by layer/type. Hold Shift/Control to Add/Substract selection", EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.EndVertical();

                TabsGUI();
                BrushWindowGUI();
                BrushSettingsGUI();
                break;
            case PaintTool.Modify:

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Modify Tool");
                EditorGUILayout.LabelField("Modifies the size and orientation of objects", EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.EndVertical();

                TabsGUI();
                BrushWindowGUI();
                BrushSettingsGUI();
                break;
            case PaintTool.Orient:

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Orient Tool");
                EditorGUILayout.LabelField("Orientates a group of objects around a point", EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.EndVertical();

                BrushSettingsGUI();
                break;
            case PaintTool.Move:

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Move Tool");
                EditorGUILayout.LabelField("Moves a group of objects along a surface", EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.EndVertical();

                BrushSettingsGUI();
                break;
            case PaintTool.Settings:

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Settings");
                EditorGUILayout.EndVertical();


                m_ToolSettingsFoldout = InternalGUI.FoldoutReset(m_ToolSettingsFoldout,
                    () => {
                        Undo.RegisterCompleteObjectUndo(m_Settings, "PP: Reset PrefabPainter Settings");
                        m_Settings.ResetToolSettings();
                    },
                    "Settings", m_ResetIconTexture);

                if (m_ToolSettingsFoldout)
                {
                    float defaultLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = EditorGUIUtility.labelWidth * 2.0f;

                    ++EditorGUI.indentLevel;

                    MakeUndoOnChange(ref m_Settings.maxBrushRadius, Mathf.Max(EditorGUILayout.FloatField(Strings.settingsMaxBrushRadius, m_Settings.maxBrushRadius), 0.5f));
                    MakeUndoOnChange(ref m_Settings.maxBrushSpacing, Mathf.Max(EditorGUILayout.FloatField(Strings.settingsMaxBrushSpacing, m_Settings.maxBrushSpacing), 0.5f));
                    MakeUndoOnChange(ref m_Settings.surfaceCoords, (SurfaceCoords)EditorGUILayout.EnumPopup(Strings.settingsSurfaceCoords, m_Settings.surfaceCoords));

                    MakeUndoOnChange(ref m_Settings.groupPrefabs, EditorGUILayout.Toggle(Strings.brushGroupPrefabs, m_Settings.groupPrefabs));
                    GUI.enabled = m_Settings.groupPrefabs;
                    MakeUndoOnChange(ref m_Settings.groupName, EditorGUILayout.TextField(Strings.settingsGroupName, m_Settings.groupName));
                    GUI.enabled = true;

                    MakeUndoOnChange(ref m_Settings.hideSceneSettingsObject, EditorGUILayout.Toggle (Strings.settingsHideSceneSettingsObject, m_Settings.hideSceneSettingsObject));
                    if(m_SceneSettings != null)
                    {
                        const HideFlags kHideFlags = HideFlags.HideInHierarchy|HideFlags.HideInInspector|HideFlags.DontSaveInBuild;
                        const HideFlags kDontHideFlags = HideFlags.DontSaveInBuild;
                        HideFlags flags;

                        if(m_Settings.hideSceneSettingsObject)
                            flags = kHideFlags;
                        else
                            flags = kDontHideFlags;

                        if(m_SceneSettings.gameObject.hideFlags != flags)
                        {
                            m_SceneSettings.gameObject.hideFlags = flags;

                            Utility.MarkActiveSceneDirty();
                            EditorApplication.RepaintHierarchyWindow();
                        }


                    }


                    MakeUndoOnChange(ref m_Settings.handlesColor, EditorGUILayout.ColorField("Handles Color", m_Settings.handlesColor));
                    m_Settings.handlesColor.a = 1.0f;

                    /*
                    MakeUndoOnChange(ref m_Settings.enableToolsShortcuts, EditorGUILayout.Toggle(new GUIContent("Enable Tools Shortcuts", ""), m_Settings.enableToolsShortcuts));
                    if (m_Settings.enableToolsShortcuts)
                    {
                        ++EditorGUI.indentLevel;
                        EditorGUILayout.HelpBox("Brush (B), Pin (P), Place (L), Erase (A), Select (S), Modify (M)", MessageType.None, true);
                        --EditorGUI.indentLevel;
                    }*/


                    MakeUndoOnChange(ref m_Settings.gridRaycastHeight, EditorGUILayout.FloatField(new GUIContent("Grid Raycast Height"), m_Settings.gridRaycastHeight));
                    MakeUndoOnChange(ref m_Settings.useAdditionalVertexStreams, EditorGUILayout.Toggle (new GUIContent("\"additionalVertexStreams\" (Use with caution)", "Used by some assets to deform meshes, can cause bugs with GI"),
                        m_Settings.useAdditionalVertexStreams));

                    EditorGUI.BeginChangeCheck();
                    m_DisableAssetPreviewLevelShader = EditorGUILayout.Toggle(new GUIContent("Disable Levels Shader (restart PP)"), m_DisableAssetPreviewLevelShader);
                    if(EditorGUI.EndChangeCheck())
                    {
                        m_AssetPreviewMaterialShaderError = m_DisableAssetPreviewLevelShader;
                    }

                    MakeUndoOnChange(ref m_Settings.disableUndo,
                        EditorGUILayout.Toggle(new GUIContent("Disable Undo System",
                        "Improves performance for large-scale environments"), m_Settings.disableUndo));

                    MakeUndoOnChange(ref m_Settings.throwPhysicsTimeStep,
                        Mathf.Max(EditorGUILayout.FloatField("Physics Time Step", m_Settings.throwPhysicsTimeStep), 0.001f));

                    --EditorGUI.indentLevel;


                    EditorGUIUtility.labelWidth = defaultLabelWidth;
                }


                break;
            }


            EditorUtility.SetDirty(m_Settings);
        }




        void OnGUI ()
        {
            // Close this window if new one created
            if (s_ActiveWindow != null && s_ActiveWindow != this)
                this.Close ();


            InternalDragAndDrop.OnBeginGUI();

            OnMainGUI ();

            HandleKeyboardEvents();

            InternalDragAndDrop.OnEndGUI();

            // repaint every time for dynamic effects like drag scrolling
            if(InternalDragAndDrop.IsDragging() || InternalDragAndDrop.IsDragPerform())
                Repaint();
        }

#endregion // GUI


    } // class PrefabPainter




#region Dialogs
    //
    // class SaveSettingsDialog
    //
    public class SaveSettingsDialog : EditorWindow
    {
        BrushPresetDatabase presetDatabase = null;
        string settingsName = "Untitled Settings";
        PrefabPainterSettings settings = null;
        bool focusTextField = true;


        void SaveSettings(string newSettingsName)
        {
            Brush selectedBrush = settings.GetActiveTab().GetFirstSelectedBrush();
            if (selectedBrush == null)
                return;

            BrushSettings newSettings = new BrushSettings(selectedBrush.settings);

            presetDatabase.SavePreset(newSettingsName, newSettings);
        }

        void OnGUI()
        {
            if(settings == null || presetDatabase == null)
                return;

            bool isNameInvalid = false;
            bool isWillOverwrite = false;



            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Name:");
            GUI.SetNextControlName("nameTextField");
            settingsName = EditorGUILayout.TextField(settingsName);

            if (focusTextField)
            {
                focusTextField = false;

                TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                if (textEditor != null)
                    textEditor.SelectAll();

                GUI.FocusControl("nameTextField");
            }

            foreach(string saved in presetDatabase.m_Presets) {
                if (String.Equals(saved, settingsName, StringComparison.CurrentCultureIgnoreCase)) {
                    isWillOverwrite = true;
                    break;
                }
            }

            if(settingsName.Length == 0 || !UnityEditorInternal.InternalEditorUtility.IsValidFileName(settingsName))
                isNameInvalid = true;


            if(isWillOverwrite) {
                EditorGUILayout.LabelField("This will overwrite existing settings!");
            } else
                if(isNameInvalid) {
                    EditorGUILayout.LabelField("Invalid name.");
                } else {
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if(GUILayout.Button ("Cancel", GUILayout.Width (70))) {
                Close();
            }

            GUI.enabled = !isNameInvalid;
            if(GUILayout.Button (isWillOverwrite ? "Overwrite" : "Save", GUILayout.Width (70))) {
                SaveSettings(settingsName);
                Close();
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if(Event.current.isKey && Event.current.keyCode == KeyCode.Escape)
                Close();

            if(Event.current.isKey && Event.current.keyCode == KeyCode.Return && !isNameInvalid)
            {
                SaveSettings(settingsName);
                Close();
            }
        }

        void OnLostFocus() {
            Close();
        }

        public static void ShowDialog(BrushPresetDatabase presetDatabase, PrefabPainterSettings settings)
        {
            Vector2 size = new Vector2(300, 90);

            presetDatabase.Refresh();
            SaveSettingsDialog window = (SaveSettingsDialog)EditorWindow.GetWindow (typeof (SaveSettingsDialog));
            window.minSize = size;
            window.maxSize = size;
            //window.position = new Rect((Screen.currentResolution.width-size.x)/2, (Screen.currentResolution.height-size.x)/2, size.x, size.y);
            window.titleContent = new GUIContent("Save Settings", "");
            window.settings = settings;
            window.presetDatabase = presetDatabase;
            window.ShowPopup();
        }
    }










    //
    // class DeleteSettingsDialog
    //
    public class DeleteSettingsDialog : EditorWindow
    {
        public BrushPresetDatabase presetDatabase = null;
        string[] settingsNames = null;
        int selected = 0;
        bool deleteAll = false;

        void OnGUI()
        {
            if (presetDatabase == null)
                return;

            if (settingsNames == null) {
                settingsNames = presetDatabase.m_Presets.ToArray ();
            }

            EditorGUILayout.BeginVertical ();

            EditorGUILayout.Space();
            GUI.enabled = settingsNames.Length != 0 && deleteAll == false;
            selected = EditorGUILayout.Popup(selected, settingsNames);
            GUI.enabled = true;

            GUI.enabled = settingsNames.Length != 0;
            deleteAll = EditorGUILayout.Toggle("Delete All", deleteAll);
            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button ("Cancel", GUILayout.Width (70))) {
                Close();
            }
            GUI.enabled = settingsNames.Length != 0;
            if(GUILayout.Button ("Delete", GUILayout.Width (70))) {
                if (deleteAll)
                    presetDatabase.DeleteAll();
                else
                    presetDatabase.DeletePreset(settingsNames[selected]);
                Close();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            if(Event.current.isKey && Event.current.keyCode == KeyCode.Escape)
                Close();

            EditorGUILayout.EndVertical ();
        }

        void OnLostFocus() {
            Close();
        }

        public static void ShowDialog(BrushPresetDatabase presetDatabase)
        {
            Vector2 size = new Vector2(300, 90);

            presetDatabase.Refresh();
            DeleteSettingsDialog window = (DeleteSettingsDialog)EditorWindow.GetWindow (typeof (DeleteSettingsDialog));
            window.presetDatabase = presetDatabase;
            window.minSize = size;
            window.maxSize = size;
            //window.position = new Rect((Screen.currentResolution.width-size.x)/2, (Screen.currentResolution.height-size.x)/2, size.x, size.y);
            window.titleContent = new GUIContent("Delete Settings", "");
            window.ShowPopup();
        }
    }

#endregion // Dialogs



} // namespace nTools.PrefabPainter
