//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace JBooth.FoliageRendering
{
    public class FoliageGUIUtil
    {
        private const string _KEY_DITHERFADE = "_DitherFade";
        private const string _PROP_MAXDRAW = "_maxDrawDistance";
        private static int _SHADER_DITHERFADE { get { return Shader.PropertyToID(_KEY_DITHERFADE); } }

        private static GameObject _lastPrefab;
        private static bool _showMaterials = false;
        private static Material[] _materials = null;

        private static bool MaterialsHasProperty(Material[] mats, int propID)
        {
            if (mats == null)
                return false;
            int count = mats.Length;
            for(int a = 0; a < count; a++)
            {
                Material m = mats[a];
                if(m.HasProperty(propID))
                    return true;
            }
            return false;
        }

        public static bool DrawOptionsGUI(
            SerializedProperty options, 
            bool allowDefaultOptions = false, 
            bool indent = true, 
            GameObject prefab = null,
            bool showDensity = false,
            SerializedProperty defaultOptions = null)
        {
            if(_lastPrefab != prefab)
            {
                _materials = GetMaterialsFromPrefab(prefab);
                _lastPrefab = prefab;
            }
            options.serializedObject.Update();

            SerializedProperty distMode = options.FindPropertyRelative(nameof(DrawOptions.distanceMode));
            SerializedProperty maxDraw = options.FindPropertyRelative(_PROP_MAXDRAW);
            SerializedProperty shadowMode = options.FindPropertyRelative(nameof(DrawOptions.shadowDistanceMode));
            SerializedProperty shadowDist = options.FindPropertyRelative(nameof(DrawOptions.shadowDistance));
            SerializedProperty shadowLOD = options.FindPropertyRelative(nameof(DrawOptions.maxShadowLOD));
            SerializedProperty bounds = options.FindPropertyRelative(nameof(DrawOptions.boundsExpand));
            SerializedProperty density = options.FindPropertyRelative(nameof(DrawOptions.density));

            if (indent)
                EditorGUI.indentLevel++;

            if (prefab != null)
            {
                EditorGUILayout.ObjectField("Object", prefab, typeof(GameObject), true);
                _showMaterials = EditorGUILayout.Foldout(_showMaterials, new GUIContent("Materials", "Materials in use by the assigned prefab."));
                if(_showMaterials)
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField("Materials in use, click to Find.", EditorStyles.miniBoldLabel);
                        int count = _materials.Length;
                        for(int a = 0; a < count; a++)
                        {
                            Material m = _materials[a];
                            if (GUILayout.Button(new GUIContent(m.name, m.name), EditorStyles.miniButton))
                                EditorGUIUtility.PingObject(m);
                        }
                    }
                }
            }

            EditorGUILayout.HelpBox("Make sure you keep any important Material properties and LOD draw distances in sync with your Distance Settings.", MessageType.Info);

            bool syncMats = false;

            if (allowDefaultOptions)
            {
                EditorGUILayout.Space();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(distMode, true);
                if(EditorGUI.EndChangeCheck())
                    syncMats = true;
            }

            if(allowDefaultOptions == false || distMode.enumValueIndex == 1)
            {
                if (allowDefaultOptions)
                    EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                maxDraw.floatValue = EditorGUILayout.DelayedFloatField(new GUIContent(maxDraw.displayName), maxDraw.floatValue);
                if (EditorGUI.EndChangeCheck())
                    syncMats = true;
                if (allowDefaultOptions)
                    EditorGUI.indentLevel--;
            }

            float defaultValue = 250.0f;
            if (defaultOptions != null)
                defaultValue = defaultOptions.
                    FindPropertyRelative(_PROP_MAXDRAW).
                    floatValue;
            float targetValue = distMode.enumValueIndex == 0 ?
                defaultValue :
                maxDraw.floatValue;

            if (MaterialsHasProperty(_materials, _SHADER_DITHERFADE) && syncMats)               
                TrySetMaterialsVectorY(_materials, _SHADER_DITHERFADE, targetValue);

            if (prefab != null)
            {
                if (DrawLOD(prefab, targetValue))
                {
                    TerrainFoliageRenderer tfr = options.serializedObject.targetObject as TerrainFoliageRenderer;
                    if (tfr != null)
                        tfr.RefreshAllProviders();
                }
            }

            if (showDensity)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(density, true);
            }

            if(allowDefaultOptions)
                EditorGUILayout.PropertyField(shadowMode, true);

            if(allowDefaultOptions == false ||  shadowMode.enumValueIndex == 1)
            {
                if(allowDefaultOptions)
                    EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField (shadowDist, true);
                if (EditorGUI.EndChangeCheck())
                {
                    if (shadowDist.floatValue < 0.0f)
                        shadowDist.floatValue = 0.0f;
                }                
                DrawShadowLOD(shadowLOD, prefab);
                if(allowDefaultOptions)
                    EditorGUI.indentLevel--;
            }
            else
            {
                DrawShadowLOD(shadowLOD, prefab);
            }

            EditorGUILayout.PropertyField(bounds, true);

            if(indent)
                EditorGUI.indentLevel--;

            return options.serializedObject.ApplyModifiedProperties();
        }

        private static int TrySetMaterialsFloat(Material[] mats, int id, float value)
        {
            int count = mats.Length;
            int setCount = 0;
            for(int a = 0; a < count; a++)
            {
                Material m = mats[a];
                if(m.HasProperty(id))
                {
                    float currValue = m.GetFloat(id);
                    if(currValue != value)
                    {
                        m.SetFloat(id, currValue);
                        EditorUtility.SetDirty(m);
                        setCount++;
                    }
                }
            }

            if (setCount > 0)
                Debug.Log("Synchronized " + setCount + " Materials");
            return setCount;
        }

        private static int TrySetMaterialsVectorY(Material[] mats, int id, float value)
        {
            int count = mats.Length;
            int setCount = 0;
            for (int a = 0; a < count; a++)
            {
                Material m = mats[a];
                if (m.HasProperty(id))
                {
                    Vector4 currValue = m.GetVector(id);
                    if (currValue.y != value)
                    {
                        currValue.y = value;
                        m.SetVector(id, currValue);
                        EditorUtility.SetDirty(m);
                        setCount++;
                    }
                }
            }

            if (setCount > 0)
                Debug.Log("Synchronized " + setCount + " Materials");
            return setCount;
        }

        private static void DrawShadowLOD(SerializedProperty shadowLOD, GameObject prefab)
        {
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(shadowLOD, true);
            if (EditorGUI.EndChangeCheck())
            {
                if (prefab != null)
                {
                    LODGroup group = prefab.GetComponentInChildren<LODGroup>();
                    if (group != null)
                    {
                        int shadowVal = shadowLOD.enumValueIndex;
                        if (shadowVal > group.lodCount)
                        {
                            shadowLOD.enumValueIndex = group.lodCount - 1;
                            ShowNotification("Clamped ShadowLOD to highest LOD in the LODGroup (" + shadowLOD.enumValueIndex + ").");
                        }
                    }
                }
            }
        }

        private static bool DrawLOD(GameObject prefab, float maxDist)
        {
            LODGroup lod = prefab.GetComponent<LODGroup>();
            if(lod == null)
            {
                //get in parent doesn't work here......
                Transform p = prefab.transform.parent;
                if(p != null) 
                    lod = p.GetComponent<LODGroup>();
            }
            if (lod != null)
            {
                SerializedObject so = new SerializedObject(lod);
                so.Update();
                //cross fade 
                string crossName = "m_FadeMode";
                SerializedProperty crossfade = so.FindProperty(crossName);

                EditorGUILayout.LabelField("LOD Group Settings");
                EditorGUI.indentLevel++;
                if(crossfade != null)
                    EditorGUILayout.PropertyField(crossfade, true);

                //culling distance
                LOD[] lods = lod.GetLODs();
                int count = lods.Length;

                float cullPercent = lods[count -1 ].screenRelativeTransitionHeight;
                float cullingDisplay = cullPercent * 100.0f;
                EditorGUILayout.LabelField("The final " + cullingDisplay.ToString("N1") + "% of this LOD Group is completely culled and will not draw.", EditorStyles.miniLabel);

                float vizRange = (1.0f - cullPercent) * maxDist;
                EditorGUILayout.LabelField("Max visible distance = " + vizRange.ToString("N1"), EditorStyles.miniLabel);
                if(cullingDisplay > 1.1f)
                {
                    if(GUILayout.Button(new GUIContent("Set Culled Range to 1%", 
                        "Uniformly increase the coverage of LODs in this LOD Group to maximize your draw distance based on Terrain and Foliage Renderer settings."), EditorStyles.miniButton))
                    {
                        Undo.RecordObject(lod, "Increase LODGroup Coverage");
                        float cullDelta = cullPercent - 0.01f;
                        for(int a = count; a > 0; a--)
                        {
                            lods[a-1].screenRelativeTransitionHeight -= cullDelta;
                        }
                        lod.SetLODs(lods);
                        EditorUtility.SetDirty(lod);
                        so.ApplyModifiedProperties();
                        EditorGUI.indentLevel--;
                        return true;
                    }
                }

                EditorGUI.indentLevel--;
                so.ApplyModifiedProperties();                
            }
            return false;
        }

        private static Material[] GetMaterialsFromPrefab(GameObject prefab)
        {
            if (prefab == null)
                return null;
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            int count = renderers.Length;
            List<Material> mats = new List<Material>();
            for(int a = 0; a < count; a++)
            {
                Renderer r = renderers[a];
                Material[] m = r.sharedMaterials;
                int matCount = m.Length;
                for(int b = 0; b < matCount; b++)
                {
                    Material material = m[b];
                    if (mats.Contains(material) == false)
                        mats.Add(material);
                }
            }
            return mats.ToArray();
        }

        static string GetPackageVersion()
        {
            List<UnityEditor.PackageManager.PackageInfo> packages = AssetDatabase.FindAssets("package")
                .Select(AssetDatabase.GUIDToAssetPath).Where(x => AssetDatabase.LoadAssetAtPath<TextAsset>(x) != null)
                .Select(UnityEditor.PackageManager.PackageInfo.FindForAssetPath).ToList();

            foreach (var p in packages)
            {
                if (p != null && p.name == "com.jbooth.foliagerenderer")
                {
                    return p.version;
                }
            }
            return "0.0";
        }

        static Texture2D gradient;
        static Texture2D logo;
        static string _version;
        public static void DrawHeaderLogo()
        {
            if (gradient == null)
            {
                gradient = Resources.Load<Texture2D>("foliagerenderer_background");
                logo = Resources.Load<Texture2D>("foliagerenderer_logo");
            }

            if (string.IsNullOrEmpty(_version))
                _version = GetPackageVersion();

            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(128));
            EditorGUI.DrawPreviewTexture(rect, gradient);
            GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit, true);
            var buttonRect = new Rect(rect);
            buttonRect.x += rect.width - 20.0f;
            buttonRect.y += 2.0f;
            buttonRect.width = 18.0f;
            buttonRect.height = 18.0f;
            if (GUI.Button(buttonRect, "?"))
                Application.OpenURL("https://dicewrenchdesigns.com/category/assets/");
            rect.y += (rect.height * 0.5f) - 8.0f;

            GUI.Label(rect, Application.unityVersion);
            rect.x += rect.width - 33;
            GUI.Label(rect, _version);
        }

        public static void ShowNotification(string message, EditorWindow window = null)
        {
            if (window == null)
                window = EditorWindow.mouseOverWindow ?? EditorWindow.focusedWindow;
            if (window != null)
            {
                window.ShowNotification(new GUIContent(message));
            }
        }

        static Dictionary<string, bool> rolloutStates = new Dictionary<string, bool>();
        static GUIStyle rolloutStyle;
        public static bool DrawRollup(string text, bool defaultState = true, bool inset = false)
        {
            if (rolloutStyle == null)
            {
                rolloutStyle = new GUIStyle(GUI.skin.box);
                rolloutStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            }
            var oldColor = GUI.contentColor;
            GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            if (inset == true)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.GetControlRect(GUILayout.Width(40));
            }

            if (!rolloutStates.ContainsKey(text))
            {
                rolloutStates[text] = defaultState;
                string key = text;
                if (EditorPrefs.HasKey(key))
                {
                    rolloutStates[text] = EditorPrefs.GetBool(key);
                }
            }
            if (GUILayout.Button(text, rolloutStyle, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(20) }))
            {
                rolloutStates[text] = !rolloutStates[text];
                string key = text;
                EditorPrefs.SetBool(key, rolloutStates[text]);
            }
            if (inset == true)
            {
                EditorGUILayout.GetControlRect(GUILayout.Width(40));
                EditorGUILayout.EndHorizontal();
            }
            GUI.contentColor = oldColor;
            return rolloutStates[text];
        }
        public static void DrawSeparator()
        {
            EditorGUILayout.Separator();
            GUILayout.Box("", boxStyle, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
            EditorGUILayout.Separator();
        }

        public static GUIStyle _boxStyle;
        public static GUIStyle boxStyle
        {
            get
            {
                if (_boxStyle == null)
                {
                    _boxStyle = new GUIStyle(EditorStyles.helpBox);
                    _boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                    _boxStyle.fontStyle = FontStyle.Bold;
                    _boxStyle.fontSize = 11;
                    _boxStyle.alignment = TextAnchor.UpperLeft;
                }
                return _boxStyle;
            }
        }
    }

    

    /// <summary>
    /// A replacement for Unity's selection grid.
    /// This one supports an active checkbox and multiple selection
    /// </summary>
    public class SelectionGrid
    {
        /// <summary>
        /// Data structure which is used in the selection grid
        /// </summary>
        public class Selectable
        {
            public string text;
            public string tooltip;
            public Texture image;
            public bool overrideEnabled;
        }

        /// <summary>
        /// Show the selection grid in the inspector
        /// </summary>
        /// <param name="selectedIndexes">The selected indexes</param>
        /// <param name="selectables">Objects used for the cells of the selection grid</param>
        /// <param name="cellSize">The size of the cells</param>
        /// <param name="dynamicResize">Whether the cells should become smaller with increasing items</param>
        /// <param name="title">Optional title</param>
        /// <returns></returns>
        public static bool ShowSelectionGrid(ref int userSelectedIndex, Selectable[] selectables, int cellSize, bool dynamicResize = false, string title = null, bool missingDataTextVisible = false)
        {
            bool changed = false;

            // title
            if (!string.IsNullOrEmpty(title))
            {
                GUIContent terrainLayers = EditorGUIUtility.TrTextContent(title);
                GUILayout.Label(terrainLayers, EditorStyles.boldLabel);
            }

            if (selectables == null || selectables.Length == 0)
            {
                if (missingDataTextVisible)
                {
                    EditorGUILayout.HelpBox("No objects found", MessageType.Info);
                }
                return changed;
            }

            if (dynamicResize)
            {
                // with more than 10 textures the texture size is reduced by a given percentage
                // ie the more items, the smaller the thumbnails
                if (selectables.Length > 10)
                {
                    cellSize = (int)(cellSize * (1 - 1f / cellSize));
                }

            }

            // calculate number of columns and rows               
            int columns = (int)(EditorGUIUtility.currentViewWidth - cellSize) / cellSize + 1;
            int rows = (int)Mathf.Ceil((selectables.Length + columns - 2) / (float)columns);

            int currentIndex = -1;
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();                
            }

            for (int row = 0; row < rows; row++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    for (int col = 0; col < columns; col++)
                    {
                        currentIndex++;

                        if (currentIndex < selectables.Length)
                        {
                            Texture previewTexture = selectables[currentIndex].image;
                            string label = selectables[currentIndex].text;
                            bool overrideEnabled = selectables[currentIndex].overrideEnabled;

                            // check if the cell is selected
                            bool isCellSelected = currentIndex == userSelectedIndex;

                            // visualization style
                            GUIStyle style;

                            if (isCellSelected)
                                style = GUIStylesSelectionGrid.TextureSelectionStyleInclude;
                            else
                                style = GUIStylesSelectionGrid.TextureSelectionStyleUnselected;

                            // show background image
                            GUILayout.Label(previewTexture, style, GUILayout.Width(cellSize), GUILayout.Height(cellSize));

                            // get clickable area of cell
                            Rect previewTextureRect = GUILayoutUtility.GetLastRect();
                            bool previewTextureClicked = Event.current.rawType == EventType.MouseDown && previewTextureRect.Contains(Event.current.mousePosition);

                            if (previewTextureClicked)
                            {
                                userSelectedIndex = currentIndex;
                                changed = true;
                            }

                            // margin for the selection                            
                            int margin = 3;

                            // get image rect
                            Rect labelRect = GUILayoutUtility.GetLastRect();

                            // reduce image rect by margin
                            labelRect.x += margin;
                            labelRect.y += margin;
                            labelRect.width -= margin * 2;
                            labelRect.height -= margin * 2;

                            // rect of the toggle, depends on the label rect
                            Rect toggleRect = new Rect(labelRect);

                            // label rect
                            labelRect.height = GUIStylesSelectionGrid.SelectionElementLabelStyle.CalcHeight(new GUIContent(label), labelRect.width);

                            // label background
                            GUI.DrawTexture(labelRect, GUIStylesSelectionGrid.LabelBackgroundTexture, ScaleMode.StretchToFill);
                            GUI.Box(labelRect, label, GUIStylesSelectionGrid.SelectionElementLabelStyle);

                            // override not enabled: make the cell appear grey-ish, ie inaccessible
                            if (!overrideEnabled /* currentIndex != userSelectedIndex */)
                            {
                                Color overlay = Color.grey;
                                overlay.a = 0.5f;

                                EditorGUI.DrawRect(previewTextureRect, overlay);
                            }

                            // active toggle
                            float lineHeight = GUI.skin.label.lineHeight;

                            // reduce the checkbox area, otherwise outside of the box we'd get a mouse cursor change on hovering where a label would be
                            toggleRect.x += margin;
                            toggleRect.y += toggleRect.height - lineHeight - margin;
                            toggleRect.height = lineHeight;
                            toggleRect.width = GUI.skin.toggle.CalcHeight(GUIContent.none, toggleRect.width);

                        }
                    }
                }
            }

            return changed;
        }
    }

    public class GUIStylesSelectionGrid
    {

        /// <summary>
        /// Used for include and unselected texture border
        /// </summary>
        private static GUIStyle _textureSelectionStyleUnselected;
        public static GUIStyle TextureSelectionStyleUnselected
        {
            get
            {
                if (_textureSelectionStyleUnselected == null || _textureSelectionStyleUnselected.normal.background == null) // background check because it's null when a scene reloads
                {
                    _textureSelectionStyleUnselected = new GUIStyle(GUI.skin.label);
                    _textureSelectionStyleUnselected.normal.background = GUI.skin.box.normal.background;
                    _textureSelectionStyleUnselected.stretchWidth = true;
                    _textureSelectionStyleUnselected.border = new RectOffset(0, 0, 0, 0);
                    _textureSelectionStyleUnselected.margin = new RectOffset(2, 2, 2, 2);
                    _textureSelectionStyleUnselected.padding = new RectOffset(2, 2, 2, 2);

                }
                return _textureSelectionStyleUnselected;
            }
        }

        private static GUIStyle _textureSelectionStyleInclude;
        public static GUIStyle TextureSelectionStyleInclude
        {
            get
            {
                if (_textureSelectionStyleInclude == null || _textureSelectionStyleInclude.normal.background == null) // background check because it's null when a scene reloads
                {
                    _textureSelectionStyleInclude = new GUIStyle(TextureSelectionStyleUnselected);
                    _textureSelectionStyleInclude.normal.background = CreateColorPixel(Color.green * 0.8f); // or use MV color new Color(0, 0, 128)
                }
                return _textureSelectionStyleInclude;
            }
        }

        private static GUIStyle _textureSelectionStyleExclude;
        public static GUIStyle TextureSelectionStyleExclude
        {
            get
            {
                if (_textureSelectionStyleExclude == null || _textureSelectionStyleExclude.normal.background == null) // background check because it's null when a scene reloads
                {
                    _textureSelectionStyleExclude = new GUIStyle(TextureSelectionStyleUnselected);
                    _textureSelectionStyleExclude.normal.background = CreateColorPixel(Color.red);
                }
                return _textureSelectionStyleExclude;
            }
        }

        static Texture2D _labelBackgroundTexture;
        public static Texture2D LabelBackgroundTexture
        {
            get
            {
                if (_labelBackgroundTexture == null)
                {
                    _labelBackgroundTexture = new Texture2D(1, 1);
                    _labelBackgroundTexture.SetPixel(0, 0, new Color(0.0f, 0.0f, 0.0f, 0.5f));
                    _labelBackgroundTexture.Apply();
                }

                return _labelBackgroundTexture;
            }
        }
        static GUIStyle _selectionElementLabelStyle;

        public static GUIStyle SelectionElementLabelStyle
        {
            get
            {
                if (_selectionElementLabelStyle == null)
                {
                    _selectionElementLabelStyle = new GUIStyle(EditorStyles.wordWrappedMiniLabel);
                    _selectionElementLabelStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                    _selectionElementLabelStyle.fontStyle = FontStyle.Bold;
                    _selectionElementLabelStyle.alignment = TextAnchor.UpperCenter;
                }
                return _selectionElementLabelStyle;
            }
        }

        /// <summary>
        /// Creates a 1x1 texture
        /// </summary>
        /// <param name="Background">Color of the texture</param>
        /// <returns></returns>
        public static Texture2D CreateColorPixel(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}