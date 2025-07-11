//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;

namespace JBooth.FoliageRendering
{
    public class FoliageLitCMI : ShaderGUI
    {
        #region ShaderProps

        //Vertex Wind
        private static string _KEY_WIND = "_VERTEXWIND";
        private static string _SHADER_WINDTOGGLE = "_VertexWind";
        private static string _SHADER_BEND = "_InitialBend";
        private static string _SHADER_STIFFNESS ="_Stiffness";
        private static string _SHADER_DRAG ="_Drag";
        private static string _SHADER_SHIVER ="_ShiverDrag";
        private static string _SHADER_WINDNORMAL ="_WindNormalInfluence";

        //LitDev
        private static string _KEY_ALPHADITHER = "_ALPHA_DITHERFADE";
        private static string _SHADER_MAINTEX ="_MainTex";
        private static string _SHADER_TINT ="_Tint";
        private static string _SHADER_ALPHAMODE ="_Alpha";        
        private static string _SHADER_DITHER ="_DitherFade";
        private static string _SHADER_ALPHACUTOFF ="_AlphaThreshold";

        private static string _KEY_BRUSH = "_BRUSHLIGHTING";
        private static string _SHADER_BRUSH ="_BrushLighting";
        private static string _SHADER_NORMAL ="_NormalMap";
        private static string _SHADER_NORMALSTRENGTH ="_NormalStrength";

        private static string _KEY_MASK = "_MASKMAP";
        private static string _SHADER_MASKTOGGLE ="_UseMaskMap";
        private static string _SHADER_MASKMAP ="_MaskMap";

        private static string _KEY_EMISSION = "_EMISSION";
        private static string _SHADER_EMISSIONTOGGLE ="_UseEmission";
        private static string _SHADER_EMISSIONMAP ="_EmissionMap";
        private static string _SHADER_EMISSIONSTRENGTH ="_EmissionStrength";
        private static string _SHADER_EMISSIONTINT ="_EmissionTint";

        private static string _KEY_DETAIL = "_DETAIL";
        private static string _SHADER_DETAILTOGGLE ="_UseDetail";
        private static string _SHADER_DETAILMAP ="_DetailMap";
        private static string _SHADER_DETAILALBEDO ="_DetailAlbedoStrength";
        private static string _SHADER_DETAILNORMAL ="_DetailNormalStrength";
        private static string _SHADER_DETAILSMOOTHNESS ="_DetailSmoothnessStrength";


        //TerrainAlignedDetail
        private static string _KEY_ALIGN = "_TERRAINALIGNEDDETAIL";
        private static string _SHADER_TERRAINALIGN ="_TerrainAlignedDetail";
        private static string _SHADER_TERRAINALBEDO ="_TerrainBlendAlbedo";
        private static string _SHADER_TERRAINYOFFSET ="_TerrainYOffset";

        //Double Sided
        private static string _SHADER_CULL ="_CullMode";
        private static string _SHADER_DOUBLESIDED ="_DoubleSidedNormalMode";
        #endregion

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            materialEditor.serializedObject.Update();

            FoliageGUIUtil.DrawHeaderLogo();

            EditorGUILayout.Space();

            Material mat = materialEditor.target as Material;

            //VERTEX WIND
            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                bool wind = DrawLeftToggle(materialEditor, properties, _SHADER_WINDTOGGLE, "", _KEY_WIND);
                if(wind)
                {
                    EditorGUI.indentLevel++;
                    MaterialProperty bend = FindProperty(_SHADER_BEND, properties);
                    MaterialProperty stiff = FindProperty(_SHADER_STIFFNESS, properties);
                    MaterialProperty drag = FindProperty(_SHADER_DRAG, properties);
                    MaterialProperty shiver = FindProperty(_SHADER_SHIVER, properties);
                    MaterialProperty windNormal = FindProperty(_SHADER_WINDNORMAL, properties);

                    materialEditor.ShaderProperty(bend, bend.displayName);
                    materialEditor.ShaderProperty(stiff, stiff.displayName);
                    materialEditor.ShaderProperty(drag, drag.displayName);
                    materialEditor.ShaderProperty(shiver, shiver.displayName);
                    materialEditor.ShaderProperty(windNormal, windNormal.displayName);

                    EditorGUI.indentLevel--;
                }
            }

            /////////////////////// LIT DEV //////////////////////////
            //BASE STUFF
            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                MaterialProperty mainTex = FindProperty(_SHADER_MAINTEX, properties);
                MaterialProperty tint = FindProperty(_SHADER_TINT, properties);

                materialEditor.ShaderProperty(mainTex, mainTex.displayName);
                materialEditor.ShaderProperty(tint, tint.displayName);

                bool brush = DrawLeftToggle(materialEditor, properties, _SHADER_BRUSH, "", _KEY_BRUSH);
            }

            //Alpha Mode
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                MaterialProperty alphaMode = FindProperty(_SHADER_ALPHAMODE, properties);

                materialEditor.ShaderProperty(alphaMode, alphaMode.displayName);

                MaterialProperty alphaCut = FindProperty(_SHADER_ALPHACUTOFF, properties);

                EditorGUI.indentLevel++;
                materialEditor.ShaderProperty(alphaCut, alphaCut.displayName);

                bool isDither = mat.IsKeywordEnabled(_KEY_ALPHADITHER);
                if (isDither)
                {
                    MaterialProperty dither = FindProperty(_SHADER_DITHER, properties);
                    Vector4 currentDither = dither.vectorValue;
                    Vector2 currentShort = new Vector2(currentDither.x, currentDither.y);
                    Vector2 newShort = EditorGUILayout.Vector2Field(new GUIContent(dither.displayName), currentShort);
                    if(currentShort != newShort)
                    {
                        currentDither.x = newShort.x;
                        currentDither.y = newShort.y;
                        dither.vectorValue = currentDither;
                    }    
                }
                EditorGUI.indentLevel--;
            }

            //Normal map
            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                MaterialProperty norm = FindProperty(_SHADER_NORMAL, properties);
                materialEditor.ShaderProperty(norm, norm.displayName);

                if(norm.textureValue != null)
                {
                    MaterialProperty normStrength = FindProperty(_SHADER_NORMALSTRENGTH, properties);
                    materialEditor.ShaderProperty(normStrength, normStrength.displayName);
                }
            }

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                bool mask = DrawLeftToggle(materialEditor, properties, _SHADER_MASKTOGGLE, "", _KEY_MASK);
                if(mask)
                {
                    MaterialProperty maskMap = FindProperty(_SHADER_MASKMAP, properties);
                    materialEditor.ShaderProperty(maskMap, maskMap.displayName);
                }
            }

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                bool emission = DrawLeftToggle(materialEditor, properties, _SHADER_EMISSIONTOGGLE, "", _KEY_EMISSION);
                if(emission)
                {
                    MaterialProperty emMap = FindProperty(_SHADER_EMISSIONMAP, properties);
                    MaterialProperty emStrength = FindProperty(_SHADER_EMISSIONSTRENGTH, properties);
                    MaterialProperty emTint = FindProperty(_SHADER_EMISSIONTINT, properties);

                    EditorGUI.indentLevel++;
                    materialEditor.ShaderProperty(emMap, emMap.displayName);
                    materialEditor.ShaderProperty(emStrength, emStrength.displayName);
                    materialEditor.ShaderProperty(emTint, emTint.displayName);
                    EditorGUI.indentLevel--;
                }
            }

            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                bool detail = DrawLeftToggle(materialEditor, properties, _SHADER_DETAILTOGGLE, "", _KEY_DETAIL);
                if(detail)
                {
                    MaterialProperty detailMap = FindProperty(_SHADER_DETAILMAP, properties);
                    MaterialProperty detailAlbedo = FindProperty(_SHADER_DETAILALBEDO, properties);
                    MaterialProperty detailNormal = FindProperty(_SHADER_DETAILNORMAL, properties);
                    MaterialProperty detailSmoothness = FindProperty(_SHADER_DETAILSMOOTHNESS, properties);

                    EditorGUI.indentLevel++;
                    materialEditor.ShaderProperty(detailMap, detailMap.displayName);
                    materialEditor.ShaderProperty(detailAlbedo, detailAlbedo.displayName);
                    materialEditor.ShaderProperty(detailNormal, detailNormal.displayName);
                    materialEditor.ShaderProperty(detailSmoothness, detailSmoothness.displayName);
                    EditorGUI.indentLevel--;
                }
            }

            //////////////////////////////////////////////////////////

            //TERRAIN ALIGNMENT
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                bool align = DrawLeftToggle(materialEditor, properties, _SHADER_TERRAINALIGN, "Align Mesh to Terrain Shape", _KEY_ALIGN);
                if (align)
                {
                    EditorGUI.indentLevel++;
                    MaterialProperty albedo = FindProperty(_SHADER_TERRAINALBEDO, properties);
                    MaterialProperty yOffset = FindProperty(_SHADER_TERRAINYOFFSET, properties);

                    materialEditor.ShaderProperty(albedo, albedo.displayName);
                    materialEditor.ShaderProperty(yOffset, yOffset.displayName);

                    EditorGUI.indentLevel--;
                }
            }

            //DOUBLE SIDED
            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                MaterialProperty cull = FindProperty(_SHADER_CULL, properties);
                MaterialProperty doubleSided = FindProperty(_SHADER_DOUBLESIDED, properties);

                materialEditor.ShaderProperty(cull, cull.displayName);
                materialEditor.ShaderProperty(doubleSided, doubleSided.displayName);
            }

            materialEditor.serializedObject.ApplyModifiedProperties();
        }

        private bool DrawLeftToggle(MaterialEditor editor, MaterialProperty[] properties, string propName, string display = "", string keyword = "")
        {
            MaterialProperty toggleProp = FindProperty(propName, properties);
            if(string.IsNullOrEmpty(display))
                display = toggleProp.displayName;
            bool toggleValue = toggleProp.floatValue > 0.0f ? true : false;
            bool newValue = EditorGUILayout.ToggleLeft(display, toggleValue);
            if(newValue != toggleValue)
            {
                toggleProp.floatValue = newValue == true ? 1.0f : 0.0f;
                if(string.IsNullOrEmpty(keyword) == false)
                {
                    int count = editor.targets.Length;
                    for(int a = 0; a < count; a++)
                    {
                        Material m = editor.targets[a] as Material;
                        if(m != null)
                        {
                            if(newValue == true)
                                m.EnableKeyword(keyword);
                            else
                                m.DisableKeyword(keyword);
                        }
                    }
                }
            }
            return newValue;
        }
    }
}