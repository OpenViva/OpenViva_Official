//RealToonGUI
//MJQStudioWorks
//2020

//#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;

public class RealToonShaderGUI : ShaderGUI
{

#region foldout bools variable

    static bool ShowTextureColor;
    static bool ShowVivaProperties;
    static bool ShowNormalMap;
    static bool ShowTransparency;
    static bool ShowMatCap;
    static bool ShowCutout;
    static bool ShowColorAdjustment;
    static bool ShowOutline;
    static bool ShowSelfLit;
    static bool ShowGloss;
    static bool ShowShadow;
    static bool ShowLighting;
    static bool ShowReflection;
    static bool ShowFReflection;
    static bool ShowRimLight;
    static bool ShowDepth;
    static bool ShowSeeThrough;
    static bool ShowDisableEnable;
    static bool ShowTessellation;
    static bool ShowUI = true;

    static string ShowUIString = "Hide UI";

#endregion

#region Variables

    string shader_name;
    string shader_type;
    bool del_skw = false;
    static bool aruskw = false;

#endregion

#region Material Properties Variables

    MaterialProperty _DoubleSided;

    MaterialProperty _RefractionIntensity;
    
    MaterialProperty _TextureIntesnity;
    MaterialProperty _MainTex;
    MaterialProperty _TexturePatternStyle;
    MaterialProperty _MainColor;
    MaterialProperty _MainColorAffectTexture;
    MaterialProperty _MVCOL;
    MaterialProperty _MCIALO;
    MaterialProperty _EnableTextureTransparent;
    
    //Viva Specific
    MaterialProperty _SkinColor;
    MaterialProperty _FingerNailColor;
    MaterialProperty _ToeNailColor;

    MaterialProperty _PupilShrink;
    MaterialProperty _SideMultiplier;
    MaterialProperty _PupilRight;
    MaterialProperty _PupilUp;

    MaterialProperty _MCapIntensity;
    MaterialProperty _MCap;
    MaterialProperty _SPECMODE;
    MaterialProperty _SPECIN;
    MaterialProperty _MCapMask;

    MaterialProperty _Cutout;
    MaterialProperty _UseSecondaryCutout;
    MaterialProperty _SecondaryCutout;
    MaterialProperty _AlphaBaseCutout;

    MaterialProperty _Opacity;
    MaterialProperty _TransparentThreshold;
    MaterialProperty _MaskTransparency;

    MaterialProperty _NormalMap;
    MaterialProperty _NormalMapIntensity;

    MaterialProperty _Saturation;

    MaterialProperty _OutlineWidth;
    MaterialProperty _OutlineWidthControl;
    MaterialProperty _OutlineExtrudeMethod;
    MaterialProperty _ReduceOutlineBackFace;
    MaterialProperty _OutlineOffset;
    MaterialProperty _OutlineZPostionInCamera;
    MaterialProperty _DoubleSidedOutline;
    MaterialProperty _OutlineColor;
    MaterialProperty _MixMainTexToOutline;
    MaterialProperty _NoisyOutlineIntensity;
    MaterialProperty _DynamicNoisyOutline;
    MaterialProperty _LightAffectOutlineColor;
    MaterialProperty _OutlineWidthAffectedByViewDistance;
    MaterialProperty _FarDistanceMaxWidth;
    MaterialProperty _VertexColorBlueAffectOutlineWitdh;

    MaterialProperty _SelfLitIntensity;
    MaterialProperty _SelfLitColor;
    MaterialProperty _SelfLitPower;
    MaterialProperty _TEXMCOLINT;
    MaterialProperty _SelfLitHighContrast;
    MaterialProperty _MaskSelfLit;

    MaterialProperty _GlossIntensity;
    MaterialProperty _Glossiness;
    MaterialProperty _GlossSoftness;
    MaterialProperty _GlossColor;
    MaterialProperty _GlossColorPower;
    MaterialProperty _MaskGloss;

    MaterialProperty _GlossTexture;
    MaterialProperty _GlossTextureSoftness;
    MaterialProperty _GlossTextureRotate;
    MaterialProperty _PSGLOTEX;
    MaterialProperty _GlossTextureFollowObjectRotation;
    MaterialProperty _GlossTextureFollowLight;

    MaterialProperty _OverallShadowColor;
    MaterialProperty _OverallShadowColorPower;
    MaterialProperty _SelfShadowShadowTAtViewDirection;

    MaterialProperty _ReduceShadowPointLight;
    MaterialProperty _PointLightSVD;
    MaterialProperty _ReduceShadowSpotDirectionalLight;
    MaterialProperty _ShadowHardness;

    MaterialProperty _HighlightColor;
    MaterialProperty _HighlightColorPower;

    MaterialProperty _SelfShadowRealtimeShadowIntensity;
    MaterialProperty _SelfShadowIntensity;
    MaterialProperty _SelfShadowThreshold;
    MaterialProperty _VertexColorGreenControlSelfShadowThreshold;
    MaterialProperty _SelfShadowHardness;
    MaterialProperty _SelfShadowRealTimeShadowColor;
    MaterialProperty _SelfShadowRealTimeShadowColorPower;
    MaterialProperty _SelfShadowColor;
    MaterialProperty _SelfShadowColorPower;
    MaterialProperty _SelfShadowAffectedByLightShadowStrength;

    MaterialProperty _SmoothObjectNormal;
    MaterialProperty _VertexColorRedControlSmoothObjectNormal;
    MaterialProperty _XYZPosition;
    MaterialProperty _ShowNormal;

    MaterialProperty _ShadowColorTexture;
    MaterialProperty _ShadowColorTexturePower;

    MaterialProperty _ShadowTIntensity;
    MaterialProperty _ShadowT;
    MaterialProperty _ShadowTLightThreshold;
    MaterialProperty _ShadowTShadowThreshold;
    MaterialProperty _ShadowTColor;
    MaterialProperty _ShadowTColorPower;
    MaterialProperty _ShadowTHardness;
    MaterialProperty _STIL;
    MaterialProperty _N_F_STIS;
    MaterialProperty _N_F_STIAL;
    MaterialProperty _ShowInAmbientLightShadowIntensity;
    MaterialProperty _ShowInAmbientLightShadowThreshold;
    MaterialProperty _LightFalloffAffectShadowT;

    MaterialProperty _PTexture;
    MaterialProperty _PTexturePower;

    MaterialProperty _RELG;
    MaterialProperty _EnvironmentalLightingIntensity;

    MaterialProperty _GIFlatShade;
    MaterialProperty _GIShadeThreshold;
    MaterialProperty _LightAffectShadow;
    MaterialProperty _LightIntensity;

    MaterialProperty _DirectionalLightIntensity;
    MaterialProperty _PointSpotlightIntensity;
    MaterialProperty _LightFalloffSoftness;

    MaterialProperty _CustomLightDirectionIntensity;
    MaterialProperty _CustomLightDirectionFollowObjectRotation;
    MaterialProperty _CustomLightDirection;

    MaterialProperty _ReflectionIntensity;
    MaterialProperty _ReflectionRoughtness;
    MaterialProperty _RefMetallic;
    MaterialProperty _MaskReflection;
    MaterialProperty _FReflection;
    
    MaterialProperty _RimLightUnfill;
    MaterialProperty _RimLightColor;
    MaterialProperty _RimLightColorPower;
    MaterialProperty _RimLightSoftness;
    MaterialProperty _RimLightInLight;
    MaterialProperty _LightAffectRimLightColor;

    MaterialProperty _Depth;
    MaterialProperty _DepthEdgeHardness;
    MaterialProperty _DepthColor;
    MaterialProperty _DepthColorPower;

    MaterialProperty _TessellationSmoothness;
    MaterialProperty _TessellationTransition;
    MaterialProperty _TessellationNear;
    MaterialProperty _TessellationFar;

    MaterialProperty _RefVal;
    MaterialProperty _Oper;
    MaterialProperty _Compa;

    MaterialProperty _N_F_MC;
    MaterialProperty _N_F_NM;
    MaterialProperty _N_F_CO;
    MaterialProperty _N_F_O;
    MaterialProperty _N_F_CA;
    MaterialProperty _N_F_SL;
    MaterialProperty _N_F_GLO;
    MaterialProperty _N_F_GLOT;
    MaterialProperty _N_F_SS;
    MaterialProperty _N_F_SON;
    MaterialProperty _N_F_SCT;
    MaterialProperty _N_F_ST;
    MaterialProperty _N_F_PT;
    MaterialProperty _N_F_CLD;
    MaterialProperty _N_F_R;
    MaterialProperty _N_F_FR;
    MaterialProperty _N_F_RL;
    MaterialProperty _N_F_D;
    MaterialProperty _N_F_HDLS;
    MaterialProperty _N_F_HPSS;
    MaterialProperty _ZWrite;
    MaterialProperty _N_F_HCS;
    MaterialProperty _N_F_NLASOBF;

#endregion

#region List of SFKW

    enum SFKW
    {
        N_F_STIS_ON,
		N_F_STIAL_ON,
        N_F_MC_ON,
		N_F_NM_ON,
		N_F_CO_ON,
		N_F_O_ON,
		N_F_CA_ON,
		N_F_SL_ON,
		N_F_GLO_ON,
		N_F_GLOT_ON,
		N_F_SS_ON,
		N_F_SON_ON,
		N_F_SCT_ON,
		N_F_ST_ON,
		N_F_PT_ON,
        N_F_RELGI_ON,
        N_F_CLD_ON,
	    N_F_R_ON,
		N_F_FR_ON,
		N_F_RL_ON,
		N_F_HDLS_ON,
		N_F_HPSS_ON,
		N_F_NLASOBF_ON
    }

    #endregion

#region TOTIPS

    string[] TOTIPS =
    {

        //Double Sided [0]
        "Make the other side of a plane object or face visible." ,

        //Texture [1]
        "Main or base texture." , 

        //Texture Pattern Style [2]
        "Turn the 'Main/Base Texture' into pattern style." ,

        //Main Color [3]
        "Main or base color." ,

        //Mix Vertex Color [4]
        "Mix or show vertex color." ,

        //Main Color in Ambient Light Only [5]
        "Put the 'Main/Base Color' into ambient light." ,

        //Highlight Color [6]
        "Highlight color." ,

        //Highlight Color Power [7]
        "'Highlight Color' power or intensity." ,

        //Enable Texture Transparent [8]
        "This will enable 'Main/Base Texture' alpha/transparent." ,

        //Refraction Intensity [9]
        "'Refraction' intensity." ,

        //Main Color Affect Texture [10] [Refraction]
        "'Main/Base Color' affect texture." ,

        //Texture Intensity [11] [Refraction]
        "'Main/Base Texture' visibility." ,

        //Intensity [12] [MatCap]
        "MatCap intensity." ,

        //MatCap [13] [MatCap]
        "MatCap texture." ,

        //Specualar Mode [14] [MatCap]
        "Turn MatCap into specular." ,

        //Specular Power [15] [MatCap]
        "Specular intensity or power." ,

        //Mask MatCap [16] [MatCap]
        "Mask MatCap.\n\nUse a Black and White texture map.\nWhite means visible matcap while Black is not." ,

        //Cutout [17]
        "Cutout value or threshold." ,

        //Alpha Base Cutout [18] 
        "It will use the alpha/transparent channel of the 'Main/Base Texture' to cutout." ,

        //Use Secondary Cutout [19]
        "This will use the 'Secondary Cutout' to do the cutout." ,

        //Secondary Cutout [20]
        "Secondary texture cutout.\n\nUse a Black and White texture map.\nWhite means not cut out while Black is cutout." ,

        //Opacity [21] [Transparency]
        "Transparent - Opaque" ,

        //Transparent Threshold [22]
        "'Main/Base Texture' transparency threshold." ,

        //Mask Transparency [23]
        "Mask Transparency.\n\nWhite means opaque while Black means transparent." ,

        //Normal Map [24]
        "Normal Map." ,

        //Normal Map Intensity [25]
        "'Normal Map' intensity." ,

        //Saturation [26] [Color Adjustment]
        "Color saturation of the object." ,

        //Width [27] [Outline]
        "Outline main width." ,

        //Width Control [28] [Outline]
        "Controls the 'Outline Width' using texture Map.\n\nUse a Black and White texture map.\nWhite means 1 while Black means 0.\nThis will not work if the Outline main width value is 0." ,

        //Outline Extrude Method [29]
        "Outline Extrude Methods.\n\nNormal - The outline extrusion will be based on normal direction.\n\nOrigin - The outline extrusion will be based on the center of the object." ,

        //Outline Offset [30]
        "Outline XYZ position." ,

        //Double Sided Outline [31]
        "Show the front side of the outline.\n\nUseful for plane object.\n'Outline Z Position In Camera' option is needed to be adjust to show the object." ,

        //Color [32] [Outline]
        "Outline color." ,

        //Mix Main Texture To Outline [33]
        "Mix 'Main/Base Texture' to oultine." ,

        //Noisy Outline Intensity [34]
        "The power/intensity of outline distortion or noise." ,

        //Dynamic Noisy Outline [35]
        "Moving noisy or distort outline." ,

        //Light Affect Outline Color [36]
        "Light (Brightness and Color) affect Outline color." ,

        //Outline Width Affected By View Distance [37]
        "'Outline Width' affected by view distance." ,

        //Far Distance Max Width [38] [Outline]
        "The maximum 'Outline Width' limit when moving far from the object." ,

        //Vertex Color Blue Affect Outline Width [39] [Outline]
        "'Vertex Color Blue' will affect the Outline Width.\n\nThis will not work if the Outline main width value is 0.\nBlue means 1 while Black means 0." ,

        //Intensity [40] [SelfLit]
        "'Self Lit' intensity." ,

        //Color [41] [SelfLit]
        "Self Lit color" ,

        //Power [42] [SelfLit]
        "'Self Lit Color' power or intensity." ,

        //Texture and Main Color Intensity [43] [SelfLit]
        "'Main/Base Texture' and 'Main/Base Color' intensity.\n\nAdjust this if the 'Main/Base Texture' and 'Main/Base Color' is too strong or too bright for Self Lit." ,

        //High Contrast [44] [SelfLit]
        "Turn Self Lit into high contrast colors and mix 'Base/Main Texture' twice." ,

        //Mask Self Lit [45]
        "Mask Self Lit.\n\nUse a Black and White texture map.\nWhite means visible Self Lit while Black is not." ,

        //Gloss Intensity [46]
        "Gloss intensity." ,

        //Glossiness [47]
        "Glossiness." ,

        //Softness [48] [Gloss]
        "The softness of the gloss." ,

        //Color [49] [Gloss]
        "Gloss color" ,

        //Power [50] [Gloss]
        "'Gloss Color' power or intensity." ,

        //Mask Gloss [51]
        "Mask Gloss.\n\nWhite means visible Gloss while black is not." ,

        //Gloss Texture [52]
        "A Black and White texture map to be used as gloss.\n\nWhite means gloss while Black is not." ,

        //Softness [53] [Gloss Texture]
        "The softness of the 'Gloss Texture'." ,

        //Pattern Style [54] [Gloss Texture]
        "Turn 'Gloss Texture' into pattern style." ,

        //Rotate [55] [Gloss Texture]
        "Rotate 'Gloss Texture'." ,

        //Follow Object Rotation [56] [Gloss Texture]
        "'Gloss Texture' will follow the object local rotation." ,

        //Follow Light [57] [Gloss Texture]
        "'Gloss Texture' will follow the light direction or position." ,

        //Overall Shadow Color [58]
        "Overall shadow color.\n\nThis will affect Realtime Shadow, Self Shadow/Shade and ShadowT." ,

        //Overall Shadow Color Power [59]
        "'Overall shadow Color' power or intensity." ,

        //Self Shadow & ShadowT At View Direction [60]
        "'Self Shadow' and 'ShadowT' follow your view or camera view direction." ,

        //Reduce Shadow (Point Light) [61]
        "The amount of reduce self cast shadow.\n\nThis option will only take effect when there's a Point Light." ,

        //Point Light Shadow Visibility Distance [62]
        "The amount of visible Point Light shadow on the object when the Point Light is move away from the object." ,

        //Reduce Shadow (Spot Light & Directional Light) [63]
        "The amount of reduce self cast shadow.\n\nThis option will only take effect when there's a 'Directional Light' or' Spot Light'.\nThis will not work in 'Directional Light' if the 'Directional Light' bias is 0." ,

        //Shadow Hardness [64]
        "Real time shadow hardness" ,

        //Threshold [65] [Self Shadow]
        "The amount of 'Self Shadow/Shade' on the object." ,

        //Vertex Color Green Control Self Shadow Threshold [66]
        "Controls 'Self Shadow Threshold' by using vertex color Green." ,

        //Hardness [67] [Self Shadow]
        "'Self Shadow/Shade' hardness." ,

        //Self Shadow & Real Time Shadow Color [68]
        "'Self Shadow and Real Time Shadow Color'.\n\nBefore you set/change this, Set 'Overall Shadow Color' to White." ,

        //Self Shadow & Real Time Shadow Color Power [69]
        "'Self Shadow and Real Time Shadow Color' power or intensity." ,

        //Self Shadow Affected By Light Shadow Strength [70]
        "Light shadow strength will affect self shadow visibility." ,

        //Smooth Object Normal [71]
        "The amount of smooth object normal." ,

        //Vertex Color Red Control Smooth Object Normal [72]
        "'Vertex color Red' controls the amount of smooth object normal.\n\nRed means 1 while Black means 0." ,

        //XYZ Position [73] [Smooth Object Normal]
        "Normal's XYZ positions." ,

        //XYZ Hardness [74] [Smooth Object Normal]
        "Normal's XYZ hardness.\n\nHigher value is better." ,

        //Show Normal [75] [Smooth Object Normal]
        "Show the normal of the object." ,

        //Shadow Color Texture [76]
        "A texture to color shadow.\n\nThis includes (RealTime Shadow, Self Shadow/Shade and ShadowT.\nYou can also use your 'Main/Base Texture' and adjust 'Power' to make it dark." ,

        //Power [77] [Shadow Color Texture]
        "How strong or dark the 'Shadow Color Texture'." ,

        //Intensity [78] [ShadowT]
        "'ShadowT' intensity or visibility." ,

        //ShadowT [79]
        "ShadowT or Shadow Texture, shadows in texture form.\n\nUse Black or Gray and White Flat, Gradient and Smooth texture map.\nGray and White affected by light while Black is not.\n\nFor more info and how to use and make ShadowT texture maps, see 'Video Tutorials' and 'User Guide.pdf' at the bottom of this RealToon inspector.",

        //Light Threshold [80] [ShadowT]
        "The amount of light." ,

        //Shadow Threshold [81] [ShadowT]
        "The amount of ShadowT." ,

        //Hardness [82] [ShadowT]
        "'ShadowT' hardness." ,

        //Show In Shadow [83] [ShadowT]
        "Show 'ShadowT' in shadow.\n\nThis will only be visible if realtime shadow and self shadow/shade color is not Black." ,

        //Show In Ambient Light [84] [ShadowT]
        "Show 'ShadowT' in Ambient Light.\n\nThis will only be visible if there's an Ambient Light present or GI." ,

        //Show In Ambient Light & Shadow Intensity [85] [ShadowT]
        "'ShadowT' intensity or visibility in shadow and ambient light." ,

        //Show In Ambient Light & Shadow Threshold [86] [ShadowT]
        "'ShadowT' threshold in Ambient Light and shadow." ,

        //Light Falloff Affect ShadowT [87]
        "'Point light' and 'Spot Light' light falloff affect 'ShadowT'." ,

        //PTexture [88]
        "A Black and White texture to be used as pattern for shadow.\n\nBlack means pattern while White is nothing.\nThis will not be visible if the shadow color is Black." ,

        //Power [89] [PTexture]
        "How strong or dark the pattern is." ,

        //Receive Environmental Ligthing and GI [90] [Lighting]
        "Turn on or off receive 'Environmental Ligthing' or 'GI'." ,

        //Environmental Ligthing Intensity [91] [Lighting]
        "Ambient Light, GI or Environmental Ligthing intensity on the object." ,

        //GI Flat Shade [92] [Lighting]
        "Turn GI or SH lighting shade into flat shade." ,

        //GI Shade Threshold [93] [Lighting]
        "The amount of GI Shade on the object." ,

        //Light affect Shadow [94] [Lighting]
        "Light intensity, color and light falloff affect shadows.\n\nThis will affect (RealTime shadow, Self Shadow and ShadowT)." ,

        //Directional Light Intensity [95] [Lighting]
        "Directional Light intensity received on the object." ,

        //Point and Spot Light Intensity [96] [Lighting]
        "Point and Spot light intensity received on the object." ,

        //Light Falloff Softness [97] [Lighting]
        "How soft is the point and spot light light falloff." ,

        //Intensity [98] [Custom Light Direction]
        "The amount of custom light direction." ,

        //Custom Light Direction [99] [Custom Light Direction]
        "XYZ light position." ,

        //Follow Object Rotation [100] [Custom Light Direction]
        "'Custom Light Direction' follow object rotation." ,

        //Intensity [101] [Reflection]
        "The amount reflection visibility." ,

        //Roughtness [102] [Reflection]
        "'Reflection' roughtness." ,
        
        //Metallic [103] [Reflection]
        "The amount of reflection metallic look." ,
        
        //Mask Reflection [104]
        "Mask Reflection.\n\nWhite means visible relfection while Black means reflection not visible." ,

        //FReflection [105]
        "A texture or image to be used as reflection." ,

        //Unfill [106] [Rim Light]
        "Unfill 'Rim Light' on the object." ,

        //Softness [107] [Rim Light]
        "'Rim Light' softness." ,

        //Light Affect Rim Light [108] [Rim Light]
        "Light (Brightness and Color) affect 'Rim Light'." ,

        //Color [109] [Rim Light]
        "'Rim Light' color." ,

        //Color Power [110] [Rim Light]
        "'Rim Light Color' power or intensity." ,

        //Rim Light In Light [111]
        "'Rim Light' will be visible in light only." ,

        //Depth [112]
        "The amount of depth effect." ,

        //Edge Hardness [113] [Depth]
        "Depth effect edge hardness." ,

        //Color [114] [Depth]
        "Depth effect color." ,

        //Color Power [115] [Depth]
        "Depth effect color power or intensity." ,

        //Smoothness [116] [Tessellation]
        "Smooth tessellated faces." ,

        //Tessellation Transition [117]
        "Transition distance between Near and Far.\n\n0 means mostly near tessellation value while 1 means mostly far tessellation value." ,

        //Tessellation Near [118]
        "The amount of tessellation when near." ,

        //Tessellation Near [119]
        "The amount of tessellation when far." ,

        //ID [120] [See Through]
        "ID or reference value." ,

        //Set A [121] [See Through]
        "'A' The see through object while 'B' is the object to be seen through A'." ,

        //Set B [122] [See Through]
        "'A' The see through object while 'B' is the object to be seen through A'." ,

        //No Light and Shadow On Backface [123]
        "No light and shadow will be visible on a back of a plane/flat object or face.\n\nThis will only be take effect or visible if 'Double Sided' is turned on." ,

        //No Light On Backface. [124] [Default and Tesselation Transparency Only]
        "No light will be visible on a back of a plane/flat object or face.\n\nThis will only be take effect or visible if 'Double Sided' is turned on." ,

        //Hide Directional Light Shadow [125]
        "Hide received 'Directional Light' shadows on the object." ,

        //Hide Point & Spot Light Shadow [126]
        "Hide received 'Point and Spot Light' shadows on the object." ,

        //Hide Cast Shadow [127] [Default and Tesselation Transparency & Refraction Only]
        "Hide object cast shadow." ,

        //ZWrite [128] [Default and Tesselation Transparency & Refraction Only]
        "Turn on or off ZWrite." ,

        //Automatic Remove Unused Shader Keywords [129]
        "Remove unused shader keywords automatically in all materials with Realtoon Shader. This will take effect once this enabled and when the RealToon Inspector shown. Disable this if you experience too slow Inspector.\n\n(Warning: This will also remove stored previous shaders shader keywords.)",

        //Reduce Outline Backface [130] [Outline] [Default and Tesselation Transparency Only]
        "Reduce outline backface." ,

        //Outline Z Position In Camera [131] [Outline]
        "Adjust the outline Z position in camera space." ,

        //RealTime Shadow Intensity [132] [Shadow]
        "Adjust the realtime shadow intensity." ,

        //Self Shadow Intensity [133] [Shadow] Default and Tesselation Transparency & Refraction Only]
        "Adjust the 'Self Shadow' shadow intensity." ,

        //Self Shadow & RealTime Shadow Intensity [134][Shadow] [Default and Tesselation Transparency & Refraction Only]
        "Adjust the 'Self Shadow' and realtime shadow intensity." ,

        //Self Shadow Color [135] [Shadow] Default and Tesselation Transparency & Refraction Only]
        "'Self Shadow' color." ,

        //Self Shadow Color Power [136] [Shadow] Default and Tesselation Transparency & Refraction Only]
        "'Self Shadow' color power or intensity." ,

        //Color [137] [ShadowT]
        "'ShadowT' color." ,

        //Color Power [138] [ShadowT]
        "'ShadowT' color power or intensity.",

        //Ignore Light [139] [ShadowT]
        "'ShadowT' ignore direction light or light position.",

        //Light Intensity [140] [Lighting]
        "Light intensity in shadow."

    };

#endregion

#region TOTIPS for EnDisFeatures

    string[] TOTIPSEDF =
    {
        //MatCap [0]
        "MatCap or Material Capture.",

        //Normal Map [1]
        "Normal Map.",

        //Outline [2]
        "Outline.",

        //Cutout [3]
        "Cutout.",

        //Color Adjustment [4]
        "Adjust the color of the object.",

        //SelfLit [5]
        "Own light or Emission.",

        //Gloss [6]
        "Gloss.",

        //Gloss Texture [7]
        "Gloss in texture form.\n\nUse a Black and White texture map.\nWhite means gloss while Black is not.",

        //Self Shadow [8]
        "Self Shadow or Shade.",

        //Smooth Object Normal [9]
        "Smooth object normal or ignore object normal.",

        //Shadow Color Texture [10]
        "Color shadow using texture.",

        //ShadowT [11]
        "ShadowT or Shadow Texture, shadows in texture form.\n\nUse Black or Gray and White Flat, Gradient and Smooth texture map.\nGray and White affected by light while Black is not.\n\nFor more info and how to use and make ShadowT texture maps, see 'Video Tutorials' and 'User Guide.pdf' at the bottom of this RealToon inspector.",

        //PTexture [12]
        "PTexture or Pattern Texture.\n\nA Black and White texture to be used as pattern for shadow.\n\nBlack means pattern while White is nothing.\nThis will not be visible if the shadow color is Black.",

        //Custom Light Direction [13]
        "Custom light direction.",

        //Reflection [14]
        "Reflection.",

        //FReflection [15]
        "FReflection or Fake Reflection.\n\nUse any texture or image as reflection.",

        //Rim Light [16]
        "Rim light or fresnel effect.",

        //Depth [17]
        "Depth effect."

    };

#endregion

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        //This Material
        Material targetMat = materialEditor.target as Material;

        //Settings
        materialEditor.SetDefaultGUIWidths();

        //Content

#region Shader Name Switch

        switch (targetMat.shader.name)
        {
            case "Universal Render Pipeline/RealToon/Version 5/Default/Default":
                shader_name = "default_d";
                shader_type = "Default";
                break;
            case "RealToon/Version 5/Default/Fade Transparency":
                shader_name = "default_ft";
                shader_type = "Fade Transperancy";
                break;
            case "RealToon/Version 5/Default/Refraction":
                shader_name = "default_ref";
                shader_type = "Refraction";
                break;
            case "RealToon/Version 5/Tessellation/Default":
                shader_name = "tessellation_d";
                shader_type = "Tessellation - Default";
                break;
            case "RealToon/Version 5/Tessellation/Fade Transparency":
                shader_name = "tessellation_ft";
                shader_type = "Tessellation - Fade Transparency";
                break;
            case "RealToon/Version 5/Tessellation/Refraction":
                shader_name = "tessellation_ref";
                shader_type = "Tessellation - Refraction";
                break;
            default:
                shader_name = string.Empty;
                shader_type = string.Empty;
                break;
        }


#endregion

#region Material Properties

        if (shader_name == "default_ref" || shader_name == "tessellation_ref")
        {
            _RefractionIntensity = ShaderGUI.FindProperty("_RefractionIntensity", properties);
            _TextureIntesnity = ShaderGUI.FindProperty("_TextureIntesnity", properties);
            _MainColorAffectTexture = ShaderGUI.FindProperty("_MainColorAffectTexture", properties);
        }
        else if (shader_name == "default_ft" || shader_name == "tessellation_ft" || shader_name == "default_d" || shader_name == "tessellation_d")
        {
            _RefractionIntensity = null;
            _TextureIntesnity = null;
            _MainColorAffectTexture = null;
        }
        
        _DoubleSided = FindProperty("_Culling", properties);

        _MainTex = ShaderGUI.FindProperty("_MainTex", properties);
        _TexturePatternStyle = ShaderGUI.FindProperty("_TexturePatternStyle", properties);

        _MainColor = ShaderGUI.FindProperty("_MainColor", properties);

        //Viva
        _SkinColor = ShaderGUI.FindProperty("_SkinColor", properties);
        _FingerNailColor = ShaderGUI.FindProperty("_FingerNailColor", properties);
        _ToeNailColor = ShaderGUI.FindProperty("_ToeNailColor", properties);
        
        _PupilShrink = ShaderGUI.FindProperty("_PupilShrink", properties);
        _SideMultiplier = ShaderGUI.FindProperty("_SideMultiplier", properties);
        _PupilRight = ShaderGUI.FindProperty("_PupilRight", properties);
        _PupilUp = ShaderGUI.FindProperty("_PupilUp", properties);

        if (shader_name == "default_d" || shader_name == "tessellation_d" || shader_name == "default_ft" || shader_name == "tessellation_ft")
        {
            _MVCOL = ShaderGUI.FindProperty("_MVCOL", properties);
        }
        else if (shader_name == "default_ref" || shader_name == "tessellation_ref")
        {
            _MVCOL = null;
        }

        if (shader_name == "default_d" || shader_name == "default_ft" || shader_name == "tessellation_d" || shader_name == "tessellation_ft")
        {
            _MCIALO = ShaderGUI.FindProperty("_MCIALO", properties);
        }
        else if (shader_name == "tessellation_ref")
        {
            _MCIALO = null;
        }

        _MCapIntensity = ShaderGUI.FindProperty("_MCapIntensity", properties);
        _MCap = ShaderGUI.FindProperty("_MCap", properties);
        _SPECMODE = ShaderGUI.FindProperty("_SPECMODE", properties);
        _SPECIN = ShaderGUI.FindProperty("_SPECIN", properties);
        _MCapMask = ShaderGUI.FindProperty("_MCapMask", properties);

        if (shader_name == "default_d" || shader_name == "tessellation_d")
        {
            _Cutout = ShaderGUI.FindProperty("_Cutout", properties);
            _UseSecondaryCutout = ShaderGUI.FindProperty("_UseSecondaryCutout", properties);
            _SecondaryCutout = ShaderGUI.FindProperty("_SecondaryCutout", properties);
            _AlphaBaseCutout = ShaderGUI.FindProperty("_AlphaBaseCutout", properties);
        }
        else if (shader_name == "default_ft" || shader_name == "tessellation_ft" || shader_name == "default_ref" || shader_name == "tessellation_ref")
        {
            _Cutout = null;
            _UseSecondaryCutout = null;
            _SecondaryCutout = null;
            _AlphaBaseCutout = null;
        }

        if (shader_name == "default_d" || shader_name == "tessellation_d")
        {
            _EnableTextureTransparent = ShaderGUI.FindProperty("_TRANSMODE", properties);
            _TransparentThreshold = null;
            _Opacity = null;
            _MaskTransparency = null;

        }
        else if (shader_name == "default_ft" || shader_name == "tessellation_ft")
        {
            _EnableTextureTransparent = null;
            _Opacity = ShaderGUI.FindProperty("_Opacity", properties);
            _TransparentThreshold = ShaderGUI.FindProperty("_TransparentThreshold", properties);
            _MaskTransparency = ShaderGUI.FindProperty("_MaskTransparency", properties);
        }

        _NormalMap = ShaderGUI.FindProperty("_NormalMap", properties);
        _NormalMapIntensity = ShaderGUI.FindProperty("_NormalMapIntensity", properties);

        _Saturation = ShaderGUI.FindProperty("_Saturation", properties);

        if (shader_name == "default_d" || shader_name == "tessellation_d" || shader_name == "default_ft" || shader_name == "tessellation_ft")
        {
            _OutlineWidth = ShaderGUI.FindProperty("_OutlineWidth", properties);
            _OutlineWidthControl = ShaderGUI.FindProperty("_OutlineWidthControl", properties);

            if (shader_name == "default_ft" || shader_name == "tessellation_ft")
            {
                _ReduceOutlineBackFace = ShaderGUI.FindProperty("_ReduceOutlineBackFace", properties);
            }
            else if (shader_name == "default_ref" || shader_name == "tessellation_ref" || shader_name == "default_d" || shader_name == "tessellation_d")
            {
                _ReduceOutlineBackFace = null;
            }

            _OutlineExtrudeMethod = ShaderGUI.FindProperty("_OutlineExtrudeMethod", properties);
            _OutlineOffset = ShaderGUI.FindProperty("_OutlineOffset", properties);
            _OutlineZPostionInCamera = ShaderGUI.FindProperty("_OutlineZPostionInCamera", properties);
            _DoubleSidedOutline = ShaderGUI.FindProperty("_DoubleSidedOutline", properties);
            _OutlineColor = ShaderGUI.FindProperty("_OutlineColor", properties);
            _MixMainTexToOutline = ShaderGUI.FindProperty("_MixMainTexToOutline", properties);
            _NoisyOutlineIntensity = ShaderGUI.FindProperty("_NoisyOutlineIntensity", properties);
            _DynamicNoisyOutline = ShaderGUI.FindProperty("_DynamicNoisyOutline", properties);
            _LightAffectOutlineColor = ShaderGUI.FindProperty("_LightAffectOutlineColor", properties);
            _OutlineWidthAffectedByViewDistance = ShaderGUI.FindProperty("_OutlineWidthAffectedByViewDistance", properties);
            _FarDistanceMaxWidth = ShaderGUI.FindProperty("_FarDistanceMaxWidth", properties);
            _VertexColorBlueAffectOutlineWitdh = ShaderGUI.FindProperty("_VertexColorBlueAffectOutlineWitdh", properties);
        }
        else if (shader_name == "default_ref" || shader_name == "tessellation_ref")
        {
            _OutlineWidth = null;
            _OutlineWidthControl = null;
            _ReduceOutlineBackFace = null;
            _OutlineExtrudeMethod = null;
            _OutlineOffset = null;
            _OutlineZPostionInCamera = null;
            _DoubleSidedOutline = null;
            _OutlineColor = null;
            _MixMainTexToOutline = null;
            _NoisyOutlineIntensity = null;
            _DynamicNoisyOutline = null;
            _LightAffectOutlineColor = null;
            _OutlineWidthAffectedByViewDistance = null;
            _FarDistanceMaxWidth = null;
            _VertexColorBlueAffectOutlineWitdh = null;
        }

        _SelfLitIntensity = ShaderGUI.FindProperty("_SelfLitIntensity", properties);
        _SelfLitColor = ShaderGUI.FindProperty("_SelfLitColor", properties);
        _SelfLitPower = ShaderGUI.FindProperty("_SelfLitPower", properties);
        _TEXMCOLINT = ShaderGUI.FindProperty("_TEXMCOLINT", properties);
        _SelfLitHighContrast = ShaderGUI.FindProperty("_SelfLitHighContrast", properties);
        _MaskSelfLit = ShaderGUI.FindProperty("_MaskSelfLit", properties);

        _GlossIntensity = ShaderGUI.FindProperty("_GlossIntensity", properties);
        _Glossiness = ShaderGUI.FindProperty("_Glossiness", properties);
        _GlossSoftness = ShaderGUI.FindProperty("_GlossSoftness", properties);
        _GlossColor = ShaderGUI.FindProperty("_GlossColor", properties);
        _GlossColorPower = ShaderGUI.FindProperty("_GlossColorPower", properties);
        _MaskGloss = ShaderGUI.FindProperty("_MaskGloss", properties);

        _GlossTexture = ShaderGUI.FindProperty("_GlossTexture", properties);
        _GlossTextureSoftness = ShaderGUI.FindProperty("_GlossTextureSoftness", properties);
        _GlossTextureRotate = ShaderGUI.FindProperty("_GlossTextureRotate", properties);
        _PSGLOTEX = ShaderGUI.FindProperty("_PSGLOTEX", properties);
        _GlossTextureFollowObjectRotation = ShaderGUI.FindProperty("_GlossTextureFollowObjectRotation", properties);
        _GlossTextureFollowLight = ShaderGUI.FindProperty("_GlossTextureFollowLight", properties);

        _OverallShadowColor = ShaderGUI.FindProperty("_OverallShadowColor", properties);
        _OverallShadowColorPower = ShaderGUI.FindProperty("_OverallShadowColorPower", properties);
        _SelfShadowShadowTAtViewDirection = ShaderGUI.FindProperty("_SelfShadowShadowTAtViewDirection", properties);

        _HighlightColor = ShaderGUI.FindProperty("_HighlightColor", properties);
        _HighlightColorPower = ShaderGUI.FindProperty("_HighlightColorPower", properties);

        _SelfShadowThreshold = ShaderGUI.FindProperty("_SelfShadowThreshold", properties);
        _VertexColorGreenControlSelfShadowThreshold = ShaderGUI.FindProperty("_VertexColorGreenControlSelfShadowThreshold", properties);
        _SelfShadowHardness = ShaderGUI.FindProperty("_SelfShadowHardness", properties);

        if (shader_name == "default_d" || shader_name == "tessellation_d")
        {
            _SelfShadowRealtimeShadowIntensity = ShaderGUI.FindProperty("_SelfShadowRealtimeShadowIntensity", properties);

            _SelfShadowRealTimeShadowColor = ShaderGUI.FindProperty("_SelfShadowRealTimeShadowColor", properties);
            _SelfShadowRealTimeShadowColorPower = ShaderGUI.FindProperty("_SelfShadowRealTimeShadowColorPower", properties);

            _SelfShadowIntensity = null;
            _SelfShadowColor = null;
            _SelfShadowColorPower = null;
        }
        else if (shader_name == "default_ft" || shader_name == "tessellation_ft" || shader_name == "default_ref" || shader_name == "tessellation_ref")
        {
            _SelfShadowRealtimeShadowIntensity = null;
            _SelfShadowRealTimeShadowColor = null;
            _SelfShadowRealTimeShadowColorPower = null;

            _SelfShadowIntensity = ShaderGUI.FindProperty("_SelfShadowIntensity", properties);
            _SelfShadowColor = ShaderGUI.FindProperty("_SelfShadowColor", properties);
            _SelfShadowColorPower = ShaderGUI.FindProperty("_SelfShadowColorPower", properties);
        }

        _SelfShadowAffectedByLightShadowStrength = ShaderGUI.FindProperty("_SelfShadowAffectedByLightShadowStrength", properties);

        _SmoothObjectNormal = ShaderGUI.FindProperty("_SmoothObjectNormal", properties);
        _VertexColorRedControlSmoothObjectNormal = ShaderGUI.FindProperty("_VertexColorRedControlSmoothObjectNormal", properties);
        _XYZPosition = ShaderGUI.FindProperty("_XYZPosition", properties);
        _ShowNormal = ShaderGUI.FindProperty("_ShowNormal", properties);

        _ShadowColorTexture = ShaderGUI.FindProperty("_ShadowColorTexture", properties);
        _ShadowColorTexturePower = ShaderGUI.FindProperty("_ShadowColorTexturePower", properties);

        _ShadowTIntensity = ShaderGUI.FindProperty("_ShadowTIntensity", properties);
        _ShadowT = ShaderGUI.FindProperty("_ShadowT", properties);
        _ShadowTLightThreshold = ShaderGUI.FindProperty("_ShadowTLightThreshold", properties);
        _ShadowTShadowThreshold = ShaderGUI.FindProperty("_ShadowTShadowThreshold", properties);
        _ShadowTColor = ShaderGUI.FindProperty("_ShadowTColor", properties);
        _ShadowTColorPower = ShaderGUI.FindProperty("_ShadowTColorPower", properties);
        _ShadowTHardness = ShaderGUI.FindProperty("_ShadowTHardness", properties);
        _STIL = ShaderGUI.FindProperty("_STIL", properties);
        _N_F_STIS = ShaderGUI.FindProperty("_N_F_STIS", properties);
        _N_F_STIAL = ShaderGUI.FindProperty("_N_F_STIAL", properties);
        _ShowInAmbientLightShadowIntensity = ShaderGUI.FindProperty("_ShowInAmbientLightShadowIntensity", properties);
        _ShowInAmbientLightShadowThreshold = ShaderGUI.FindProperty("_ShowInAmbientLightShadowThreshold", properties);

        _LightFalloffAffectShadowT = ShaderGUI.FindProperty("_LightFalloffAffectShadowT", properties);

        _PTexture = ShaderGUI.FindProperty("_PTexture", properties);
        _PTexturePower = ShaderGUI.FindProperty("_PTexturePower", properties);

        _RELG = ShaderGUI.FindProperty("_RELG", properties);
        _EnvironmentalLightingIntensity = ShaderGUI.FindProperty("_EnvironmentalLightingIntensity", properties);

        _GIFlatShade = ShaderGUI.FindProperty("_GIFlatShade", properties);
        _GIShadeThreshold = ShaderGUI.FindProperty("_GIShadeThreshold", properties);
        _LightAffectShadow = ShaderGUI.FindProperty("_LightAffectShadow", properties);
        _LightIntensity = ShaderGUI.FindProperty("_LightIntensity", properties);

        _DirectionalLightIntensity = ShaderGUI.FindProperty("_DirectionalLightIntensity", properties);
        _PointSpotlightIntensity = ShaderGUI.FindProperty("_PointSpotlightIntensity", properties);
        _LightFalloffSoftness = ShaderGUI.FindProperty("_LightFalloffSoftness", properties);

        if (shader_name == "default_d" || shader_name == "tessellation_d")
        {
            _ReduceShadowPointLight = ShaderGUI.FindProperty("_ReduSha", properties);
            _ShadowHardness = ShaderGUI.FindProperty("_ShadowHardness", properties);
        }
        else if (shader_name == "default_ft" || shader_name == "tessellation_ft" || shader_name == "default_ref" || shader_name == "tessellation_ref")
        {
            _ReduceShadowPointLight = null;
            _PointLightSVD = null;
            _ReduceShadowSpotDirectionalLight = null;
            _ShadowHardness = null;
        }

        _CustomLightDirectionIntensity = ShaderGUI.FindProperty("_CustomLightDirectionIntensity", properties);
        _CustomLightDirectionFollowObjectRotation = ShaderGUI.FindProperty("_CustomLightDirectionFollowObjectRotation", properties);
        _CustomLightDirection = ShaderGUI.FindProperty("_CustomLightDirection", properties);

        _ReflectionIntensity = ShaderGUI.FindProperty("_ReflectionIntensity", properties);
        _ReflectionRoughtness = ShaderGUI.FindProperty("_ReflectionRoughtness", properties);

        if (shader_name == "default_d" || shader_name == "tessellation_d" || shader_name == "default_ft" || shader_name == "tessellation_ft")
        {
            _RefMetallic = ShaderGUI.FindProperty("_RefMetallic", properties);
        }

        _MaskReflection = ShaderGUI.FindProperty("_MaskReflection", properties);
        _FReflection = ShaderGUI.FindProperty("_FReflection", properties);
           
        _RimLightUnfill = ShaderGUI.FindProperty("_RimLightUnfill", properties);
        _RimLightColor = ShaderGUI.FindProperty("_RimLightColor", properties);
        _RimLightColorPower = ShaderGUI.FindProperty("_RimLightColorPower", properties);
        _RimLightSoftness = ShaderGUI.FindProperty("_RimLightSoftness", properties);
        _RimLightInLight = ShaderGUI.FindProperty("_RimLightInLight", properties);
        _LightAffectRimLightColor = ShaderGUI.FindProperty("_LightAffectRimLightColor", properties);

        if (shader_name == "default_ref" || shader_name == "tessellation_ref")
        {
            _Depth = ShaderGUI.FindProperty("_Depth", properties);
            _DepthEdgeHardness = ShaderGUI.FindProperty("_DepthEdgeHardness", properties);
            _DepthColor = ShaderGUI.FindProperty("_DepthColor", properties);
            _DepthColorPower = ShaderGUI.FindProperty("_DepthColorPower", properties);
        }
        else if(shader_name == "default_ft" || shader_name == "tessellation_ft" || shader_name == "default_d" || shader_name == "tessellation_d")
        {
            _Depth = null;
            _DepthEdgeHardness = null;
            _DepthColor = null;
            _DepthColorPower = null;
        }


        if (shader_name == "tessellation_d" || shader_name == "tessellation_ft" || shader_name == "tessellation_ref")
        {
            _TessellationSmoothness = ShaderGUI.FindProperty("_TessellationSmoothness", properties);
            _TessellationTransition = ShaderGUI.FindProperty("_TessellationTransition", properties);
            _TessellationNear = ShaderGUI.FindProperty("_TessellationNear", properties);
            _TessellationFar = ShaderGUI.FindProperty("_TessellationFar", properties);
        }
        else if (shader_name == "default_d" || shader_name == "default_ft" || shader_name == "default_ref")
        {

            _TessellationSmoothness = null;
            _TessellationTransition = null;
            _TessellationNear = null;
            _TessellationFar = null;

        }

        _RefVal = ShaderGUI.FindProperty("_RefVal", properties);
        _Oper = ShaderGUI.FindProperty("_Oper", properties);
        _Compa = ShaderGUI.FindProperty("_Compa", properties);

        _N_F_MC = ShaderGUI.FindProperty("_N_F_MC", properties);

        _N_F_NM = ShaderGUI.FindProperty("_N_F_NM", properties);

        if (shader_name == "default_d" || shader_name == "tessellation_d" || shader_name == "default_ft" || shader_name == "tessellation_ft")
        {
            if (shader_name == "default_d" || shader_name == "tessellation_d")
            {
                _N_F_CO = ShaderGUI.FindProperty("_N_F_CO", properties);
            }

            _N_F_O = ShaderGUI.FindProperty("_N_F_O", properties);
        }
        else if (shader_name == "default_ft" || shader_name == "tessellation_ft")
        {
            _N_F_CO = null;
            _N_F_O = null;
        }

        _N_F_CA = ShaderGUI.FindProperty("_N_F_CA", properties);
        _N_F_SL = ShaderGUI.FindProperty("_N_F_SL", properties);
        _N_F_GLO = ShaderGUI.FindProperty("_N_F_GLO", properties);
        _N_F_GLOT = ShaderGUI.FindProperty("_N_F_GLOT", properties);
        _N_F_SS = ShaderGUI.FindProperty("_N_F_SS", properties);
        _N_F_SON = ShaderGUI.FindProperty("_N_F_SON", properties);
        _N_F_SCT = ShaderGUI.FindProperty("_N_F_SCT", properties);
        _N_F_ST = ShaderGUI.FindProperty("_N_F_ST", properties);
        _N_F_PT = ShaderGUI.FindProperty("_N_F_PT", properties);
        _N_F_CLD = ShaderGUI.FindProperty("_N_F_CLD", properties);
        _N_F_R = ShaderGUI.FindProperty("_N_F_R", properties);
        _N_F_FR = ShaderGUI.FindProperty("_N_F_FR", properties);
        _N_F_RL = ShaderGUI.FindProperty("_N_F_RL", properties);

        if (shader_name == "default_ref" || shader_name == "tessellation_ref")
        {
            _N_F_D = ShaderGUI.FindProperty("_N_F_D", properties);
        }
        else if (shader_name == "default_ft" || shader_name == "tessellation_ft" || shader_name == "default_d" || shader_name == "tessellation_d")
        {
            _N_F_D = null;
        }


        if (shader_name == "default_d" || shader_name == "tessellation_d")
        {
            _N_F_HDLS = ShaderGUI.FindProperty("_N_F_HDLS", properties);
            _N_F_HPSS = ShaderGUI.FindProperty("_N_F_HPSS", properties);
            _ZWrite = null;
        }
        else if (shader_name == "default_ft" || shader_name == "tessellation_ft" || shader_name == "default_ref" || shader_name == "tessellation_ref")
        {
            _N_F_HDLS = null;
            _N_F_HPSS = null;
            _ZWrite = ShaderGUI.FindProperty("_ZWrite", properties);
        }

        if (shader_name == "default_ft" || shader_name == "tessellation_ft")
        {
            _N_F_HCS = ShaderGUI.FindProperty("_N_F_HCS", properties);
        }
        else if (shader_name == "default_ref" || shader_name == "tessellation_ref" || shader_name == "default_d" || shader_name == "tessellation_d")
        {
            _N_F_HCS = null;
        }

        _N_F_NLASOBF = ShaderGUI.FindProperty("_N_F_NLASOBF", properties);

#endregion

        //UI

#region UI


        if (ShowUI == true)
        {

            GUILayout.Space(20);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            materialEditor.ShaderProperty(_DoubleSided, _DoubleSided.displayName);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            
#region Viva
            
            Rect r_vivaProperties = EditorGUILayout.BeginVertical("Button");
            


            ShowVivaProperties = EditorGUILayout.Foldout(ShowVivaProperties, "(Viva Specific)", true, EditorStyles.foldout);

            if (ShowVivaProperties)
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                GUILayout.Label("Skin Settings", EditorStyles.boldLabel);
                GUILayout.Space(10);
                
                materialEditor.ShaderProperty(_SkinColor, new GUIContent(_SkinColor.displayName) );
                materialEditor.ShaderProperty(_FingerNailColor, new GUIContent(_FingerNailColor.displayName) );
                materialEditor.ShaderProperty(_ToeNailColor, new GUIContent(_ToeNailColor.displayName) );
                
                
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                GUILayout.Label("Pupil Controls", EditorStyles.boldLabel);
                GUILayout.Space(10);
                
                materialEditor.ShaderProperty(_PupilShrink, new GUIContent(_PupilShrink.displayName) );
                materialEditor.ShaderProperty(_SideMultiplier, new GUIContent(_SideMultiplier.displayName) );
                materialEditor.ShaderProperty(_PupilRight, new GUIContent(_PupilRight.displayName) );
                materialEditor.ShaderProperty(_PupilUp, new GUIContent(_PupilUp.displayName) );
                
                // EditorGUI.BeginDisabledGroup(_MainTex.textureValue == null);
                // materialEditor.ShaderProperty(_TexturePatternStyle, new GUIContent(_TexturePatternStyle.displayName, TOTIPS[2]) );
                // EditorGUI.EndDisabledGroup();
                
                GUILayout.Space(10);

            }

            EditorGUILayout.EndVertical();
#endregion

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            //Texture - Color - Refraction

#region Texture - Color - Refraction

            Rect r_texturecolor = EditorGUILayout.BeginVertical("Button");
            
            ShowTextureColor = EditorGUILayout.Foldout(ShowTextureColor, "(Texture - Color)", true, EditorStyles.foldout);

            if (ShowTextureColor)
            {
                GUILayout.Space(10);

                if (shader_name == "default_d" || shader_name == "tessellation_d" || shader_name == "default_ft" || shader_name == "tessellation_ft")
                {

                    materialEditor.ShaderProperty(_MainTex, new GUIContent(_MainTex.displayName, TOTIPS[1]) );

                    EditorGUI.BeginDisabledGroup(_MainTex.textureValue == null);
                    materialEditor.ShaderProperty(_TexturePatternStyle, new GUIContent(_TexturePatternStyle.displayName, TOTIPS[2]) );
                    EditorGUI.EndDisabledGroup();

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_MainColor, new GUIContent(_MainColor.displayName, TOTIPS[3]) );

                    
                    materialEditor.ShaderProperty(_MVCOL, new GUIContent(_MVCOL.displayName, TOTIPS[4]));

                    GUILayout.Space(10);
                    materialEditor.ShaderProperty(_MCIALO, new GUIContent(_MCIALO.displayName, TOTIPS[5]));


                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_HighlightColor, new GUIContent(_HighlightColor.displayName, TOTIPS[6]));
                    materialEditor.ShaderProperty(_HighlightColorPower, new GUIContent(_HighlightColorPower.displayName, TOTIPS[7]));

                    GUILayout.Space(10);

                    if (shader_name == "default_d" || shader_name == "tessellation_d")
                    {
                        materialEditor.ShaderProperty(_EnableTextureTransparent, new GUIContent(_EnableTextureTransparent.displayName, TOTIPS[8]));
                    }

                    else
                    {
                        _EnableTextureTransparent = null;
                    }

                }
            }

            EditorGUILayout.EndVertical();

#endregion



            //MatCap

#region MatCap

            if (_N_F_MC.floatValue == 1)
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                Rect r_matcap = EditorGUILayout.BeginVertical("Button");
                ShowMatCap = EditorGUILayout.Foldout(ShowMatCap, "(MatCap)", true, EditorStyles.foldout);

                if (ShowMatCap)
                {

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_MCapIntensity, new GUIContent(_MCapIntensity.displayName, TOTIPS[12]));
                    materialEditor.ShaderProperty(_MCap, new GUIContent(_MCap.displayName, TOTIPS[13]));

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_SPECMODE, new GUIContent(_SPECMODE.displayName, TOTIPS[14]));
                    EditorGUI.BeginDisabledGroup(_SPECMODE.floatValue == 0);
                    materialEditor.ShaderProperty(_SPECIN, new GUIContent(_SPECIN.displayName, TOTIPS[15]));
                    EditorGUI.EndDisabledGroup();

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_MCapMask, new GUIContent(_MCapMask.displayName, TOTIPS[16]));

                }

                EditorGUILayout.EndVertical();
            }

#endregion

            //Cutout

#region Cutout

            if (shader_name == "default_d" || shader_name == "tessellation_d")
            {
                if (_N_F_CO.floatValue == 1)
                {
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    EditorGUI.BeginDisabledGroup(_N_F_CO.floatValue == 0);

                    Rect r_cutout = EditorGUILayout.BeginVertical("Button");
                    ShowCutout = EditorGUILayout.Foldout(ShowCutout, "(Cutout)", true, EditorStyles.foldout);

                    if (ShowCutout)
                    {

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_Cutout, new GUIContent(_Cutout.displayName, TOTIPS[17]));
                        materialEditor.ShaderProperty(_AlphaBaseCutout, new GUIContent(_AlphaBaseCutout.displayName, TOTIPS[18]));

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_UseSecondaryCutout, new GUIContent(_UseSecondaryCutout.displayName, TOTIPS[19]));
                        materialEditor.ShaderProperty(_SecondaryCutout, new GUIContent(_SecondaryCutout.displayName, TOTIPS[20]));
                    }

                    EditorGUILayout.EndVertical();

                    EditorGUI.EndDisabledGroup();
                }
            }

#endregion

            //Transperancy

#region Transperancy

            if (shader_name == "default_ft" || shader_name == "tessellation_ft")
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                Rect r_transparency = EditorGUILayout.BeginVertical("Button");
                ShowTransparency = EditorGUILayout.Foldout(ShowTransparency, "(Transparency)", true, EditorStyles.foldout);

                if (ShowTransparency)
                {
                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_Opacity, new GUIContent(_Opacity.displayName, TOTIPS[21]));
                    materialEditor.ShaderProperty(_TransparentThreshold, new GUIContent(_TransparentThreshold.displayName, TOTIPS[22]));

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_MaskTransparency, new GUIContent(_MaskTransparency.displayName, TOTIPS[23]));

                }

                EditorGUILayout.EndVertical();
            }

#endregion

            //Normal Map

#region Normal Map

            if (_N_F_NM.floatValue == 1)
            {

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                Rect r_normalmap = EditorGUILayout.BeginVertical("Button");
                ShowNormalMap = EditorGUILayout.Foldout(ShowNormalMap, "(Normal Map)", true, EditorStyles.foldout);

                if (ShowNormalMap)
                {
                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_NormalMap, new GUIContent(_NormalMap.displayName, TOTIPS[24]));

                    EditorGUI.BeginDisabledGroup(_NormalMap.textureValue == null);
                    materialEditor.ShaderProperty(_NormalMapIntensity, new GUIContent(_NormalMapIntensity.displayName, TOTIPS[25]));
                    EditorGUI.EndDisabledGroup();
                }

                EditorGUILayout.EndVertical();
            }

#endregion

            //Color Adjustment

#region Color Adjustment

            if (_N_F_CA.floatValue == 1)
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                Rect r_cadjustment = EditorGUILayout.BeginVertical("Button");
                ShowColorAdjustment = EditorGUILayout.Foldout(ShowColorAdjustment, "Color Adjustment", true, EditorStyles.foldout);

                if (ShowColorAdjustment)
                {

                    GUILayout.Space(10);
                    materialEditor.ShaderProperty(_Saturation, new GUIContent(_Saturation.displayName, TOTIPS[26]));

                }

                EditorGUILayout.EndVertical();

            }

#endregion

            //Outline

#region Outline

            if (shader_name == "default_d" || shader_name == "tessellation_d" || shader_name == "default_ft" || shader_name == "tessellation_ft")
            {
                if (_N_F_O.floatValue == 1)
                {

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    Rect r_outline = EditorGUILayout.BeginVertical("Button");
                    ShowOutline = EditorGUILayout.Foldout(ShowOutline, "(Outline)", true, EditorStyles.foldout);


                    if (ShowOutline)
                    {

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_OutlineWidth, new GUIContent(_OutlineWidth.displayName, TOTIPS[27]));

                        materialEditor.ShaderProperty(_OutlineWidthControl, new GUIContent(_OutlineWidthControl.displayName, TOTIPS[28]));

                        if (shader_name == "default_ft" || shader_name == "tessellation_ft")
                        {
                            materialEditor.ShaderProperty(_ReduceOutlineBackFace, new GUIContent(_ReduceOutlineBackFace.displayName, TOTIPS[130]));
                        }

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_OutlineExtrudeMethod, new GUIContent(_OutlineExtrudeMethod.displayName, TOTIPS[29]));

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_OutlineOffset, new GUIContent(_OutlineOffset.displayName, TOTIPS[30]));

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_OutlineZPostionInCamera, new GUIContent(_OutlineZPostionInCamera.displayName, TOTIPS[131]));

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_DoubleSidedOutline, new GUIContent(_DoubleSidedOutline.displayName, TOTIPS[31]));

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_OutlineColor, new GUIContent(_OutlineColor.displayName, TOTIPS[32]));

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_MixMainTexToOutline, new GUIContent(_MixMainTexToOutline.displayName, TOTIPS[33]));

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_NoisyOutlineIntensity, new GUIContent(_NoisyOutlineIntensity.displayName, TOTIPS[34]));
                        materialEditor.ShaderProperty(_DynamicNoisyOutline, new GUIContent(_DynamicNoisyOutline.displayName, TOTIPS[35]));

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_LightAffectOutlineColor, new GUIContent(_LightAffectOutlineColor.displayName, TOTIPS[36]));

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_OutlineWidthAffectedByViewDistance, new GUIContent(_OutlineWidthAffectedByViewDistance.displayName, TOTIPS[37]));
                        EditorGUI.BeginDisabledGroup(_OutlineWidthAffectedByViewDistance.floatValue == 0);
                        materialEditor.ShaderProperty(_FarDistanceMaxWidth, new GUIContent(_FarDistanceMaxWidth.displayName, TOTIPS[38]));
                        EditorGUI.EndDisabledGroup();

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_VertexColorBlueAffectOutlineWitdh, new GUIContent(_VertexColorBlueAffectOutlineWitdh.displayName, TOTIPS[39]));

                    }

                    EditorGUILayout.EndVertical();

                }

            }

#endregion

            //Self Lit

#region SelfLit

            if (_N_F_SL.floatValue == 1)
            {

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                Rect r_selflit = EditorGUILayout.BeginVertical("Button");
                ShowSelfLit = EditorGUILayout.Foldout(ShowSelfLit, "(Self Lit)", true, EditorStyles.foldout);

                if (ShowSelfLit)
                {

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_SelfLitIntensity, new GUIContent(_SelfLitIntensity.displayName, TOTIPS[40]));

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_SelfLitColor, new GUIContent(_SelfLitColor.displayName, TOTIPS[41]));
                    materialEditor.ShaderProperty(_SelfLitPower, new GUIContent(_SelfLitPower.displayName, TOTIPS[42]));

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_TEXMCOLINT, new GUIContent(_TEXMCOLINT.displayName, TOTIPS[43]));

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_SelfLitHighContrast, new GUIContent(_SelfLitHighContrast.displayName, TOTIPS[44]));

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_MaskSelfLit, new GUIContent(_MaskSelfLit.displayName, TOTIPS[45]));

                }

                EditorGUILayout.EndVertical();

            }
#endregion

            //Gloss

#region Gloss

            if (_N_F_GLO.floatValue == 1)
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                Rect r_gloss = EditorGUILayout.BeginVertical("Button");
                ShowGloss = EditorGUILayout.Foldout(ShowGloss, "(Gloss)", true, EditorStyles.foldout);

                if (ShowGloss)
                {

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_GlossIntensity, new GUIContent(_GlossIntensity.displayName, TOTIPS[46]));
                    EditorGUI.BeginDisabledGroup(_N_F_GLOT.floatValue == 1);
                    materialEditor.ShaderProperty(_Glossiness, new GUIContent(_Glossiness.displayName, TOTIPS[47]));
                    materialEditor.ShaderProperty(_GlossSoftness, new GUIContent(_GlossSoftness.displayName, TOTIPS[48]));
                    EditorGUI.EndDisabledGroup();

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_GlossColor, new GUIContent(_GlossColor.displayName, TOTIPS[49]));
                    materialEditor.ShaderProperty(_GlossColorPower, new GUIContent(_GlossColorPower.displayName, TOTIPS[50]));

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_MaskGloss, new GUIContent(_MaskGloss.displayName, TOTIPS[51]));

                    GUILayout.Space(10);

                    //Gloss Texture

#region Gloss Texture

                    if (_N_F_GLOT.floatValue == 1)
                    {

                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                        Rect r_glosstexture = EditorGUILayout.BeginVertical("Button");
                        GUILayout.Label("Gloss Texture", EditorStyles.boldLabel);
                        EditorGUILayout.EndVertical();

                        if (_N_F_GLOT.floatValue == 1)
                        {

                            GUILayout.Space(10);

                            materialEditor.ShaderProperty(_GlossTexture, new GUIContent(_GlossTexture.displayName, TOTIPS[52]));

                            GUILayout.Space(10);
                            EditorGUI.BeginDisabledGroup(_GlossTexture.textureValue == null);
                            materialEditor.ShaderProperty(_GlossTextureSoftness, new GUIContent(_GlossTextureSoftness.displayName, TOTIPS[53]));

                            GUILayout.Space(10);

                            materialEditor.ShaderProperty(_PSGLOTEX, new GUIContent(_PSGLOTEX.displayName, TOTIPS[54]));

                            GUILayout.Space(10);

                            EditorGUI.BeginDisabledGroup(_PSGLOTEX.floatValue == 1);
                            materialEditor.ShaderProperty(_GlossTextureRotate, new GUIContent(_GlossTextureRotate.displayName, TOTIPS[55]));
                            materialEditor.ShaderProperty(_GlossTextureFollowObjectRotation, new GUIContent(_GlossTextureFollowObjectRotation.displayName, TOTIPS[56]));
                            materialEditor.ShaderProperty(_GlossTextureFollowLight, new GUIContent(_GlossTextureFollowLight.displayName , TOTIPS[57]));
                            EditorGUI.EndDisabledGroup();

                            EditorGUI.EndDisabledGroup();

                        }

                    }
#endregion

                }

                EditorGUILayout.EndVertical();

            }

#endregion

            //Shadow

#region Shadow

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            Rect r_shadow = EditorGUILayout.BeginVertical("Button");
            ShowShadow = EditorGUILayout.Foldout(ShowShadow, "(Shadow)", true, EditorStyles.foldout);

            if (ShowShadow)
            {

                GUILayout.Space(10);

                materialEditor.ShaderProperty(_OverallShadowColor, new GUIContent(_OverallShadowColor.displayName, TOTIPS[58]));
                materialEditor.ShaderProperty(_OverallShadowColorPower, new GUIContent(_OverallShadowColorPower.displayName, TOTIPS[59]));

                GUILayout.Space(10);

                materialEditor.ShaderProperty(_SelfShadowShadowTAtViewDirection, new GUIContent(_SelfShadowShadowTAtViewDirection.displayName, TOTIPS[60]));

                if (shader_name == "default_d" || shader_name == "tessellation_d")
                {
                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_ReduceShadowPointLight, new GUIContent(_ReduceShadowPointLight.displayName, TOTIPS[61]));

                    if (_N_F_HDLS.floatValue == 0 || _N_F_HPSS.floatValue == 0)
                    {
                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_ShadowHardness, new GUIContent(_ShadowHardness.displayName, TOTIPS[64]));
                    }

                }

                if (shader_name == "default_d" || shader_name == "tessellation_d")
                {

                    switch ((int)_N_F_SS.floatValue)
                    {
                        case 0:
                            materialEditor.ShaderProperty(_SelfShadowRealtimeShadowIntensity, new GUIContent("Realtime Shadow Intensity", TOTIPS[132]));
                            break;
                        case 1:
                            materialEditor.ShaderProperty(_SelfShadowRealtimeShadowIntensity, new GUIContent(_SelfShadowRealtimeShadowIntensity.displayName, TOTIPS[134]));
                            break;
                        default:
                            break;
                    }

                }

                //Self Shadow

#region Self Shadow

                if (_N_F_SS.floatValue == 1)
                {

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    Rect r_selfshadow = EditorGUILayout.BeginVertical("Button");
                    GUILayout.Label("Self Shadow", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();

                    if (_N_F_SS.floatValue == 1)
                    {

                        GUILayout.Space(10);

                        if (shader_name == "default_ft" || shader_name == "tessellation_ft" || shader_name == "default_ref" || shader_name == "tessellation_ref")
                        {
                            materialEditor.ShaderProperty(_SelfShadowIntensity, new GUIContent(_SelfShadowIntensity.displayName, TOTIPS[133]));
                        }

                        materialEditor.ShaderProperty(_SelfShadowThreshold, new GUIContent(_SelfShadowThreshold.displayName, TOTIPS[65]));
                        materialEditor.ShaderProperty(_VertexColorGreenControlSelfShadowThreshold, new GUIContent(_VertexColorGreenControlSelfShadowThreshold.displayName, TOTIPS[66]));
                        materialEditor.ShaderProperty(_SelfShadowHardness, new GUIContent(_SelfShadowHardness.displayName, TOTIPS[67]));

                        GUILayout.Space(10);

                        if (shader_name == "default_d" || shader_name == "tessellation_d")
                        {
                            materialEditor.ShaderProperty(_SelfShadowRealTimeShadowColor, new GUIContent(_SelfShadowRealTimeShadowColor.displayName, TOTIPS[68]));
                            materialEditor.ShaderProperty(_SelfShadowRealTimeShadowColorPower, new GUIContent(_SelfShadowRealTimeShadowColorPower.displayName, TOTIPS[69]));
                        }
                        else if (shader_name == "default_ft" || shader_name == "tessellation_ft" || shader_name == "default_ref" || shader_name == "tessellation_ref")
                        {
                            materialEditor.ShaderProperty(_SelfShadowColor, new GUIContent(_SelfShadowColor.displayName, TOTIPS[135]));
                            materialEditor.ShaderProperty(_SelfShadowColorPower, new GUIContent(_SelfShadowColor.displayName, TOTIPS[136]));
                        }

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_SelfShadowAffectedByLightShadowStrength, new GUIContent(_SelfShadowAffectedByLightShadowStrength.displayName, TOTIPS[70]));

                    }

                }
#endregion

                //Smooth Object Normal

#region Smooth Object normal

                if (_N_F_SON.floatValue == 1)
                {

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    if (_N_F_SS.floatValue == 0)
                    {
                        _N_F_SON.floatValue = 0;
                        targetMat.DisableKeyword("F_SS_ON");
                        _ShowNormal.floatValue = 0;
                    }

                    Rect r_smoothobjectnormal = EditorGUILayout.BeginVertical("Button");
                    GUILayout.Label("Smooth Object Normal", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();

                    if (_N_F_SON.floatValue == 1)
                    {

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_SmoothObjectNormal, new GUIContent(_SmoothObjectNormal.displayName, TOTIPS[71]));
                        materialEditor.ShaderProperty(_VertexColorRedControlSmoothObjectNormal, new GUIContent(_VertexColorRedControlSmoothObjectNormal.displayName, TOTIPS[72]));

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_ShowNormal, new GUIContent(_ShowNormal.displayName, TOTIPS[75]));

                    }

                }
#endregion

                //Shadow Color Texture

#region Shadow Color Texture

                if (_N_F_SCT.floatValue == 1)
                {
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    Rect r_shadowcolortexture = EditorGUILayout.BeginVertical("Button");
                    GUILayout.Label("Shadow Color Texture", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();

                    if (_N_F_SCT.floatValue == 1)
                    {

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_ShadowColorTexture, new GUIContent(_ShadowColorTexture.displayName, TOTIPS[76]));
                        materialEditor.ShaderProperty(_ShadowColorTexturePower, new GUIContent(_ShadowColorTexturePower.displayName, TOTIPS[77]));
                    }

                }

#endregion

                //ShadowT

#region ShadowT

                if (_N_F_ST.floatValue == 1)
                {
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    Rect r_shadowt = EditorGUILayout.BeginVertical("Button");
                    GUILayout.Label("ShadowT", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();

                    if (_N_F_ST.floatValue == 1)
                    {
                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_ShadowTIntensity, new GUIContent(_ShadowTIntensity.displayName, TOTIPS[78]));
                        materialEditor.ShaderProperty(_ShadowT, new GUIContent(_ShadowT.displayName, TOTIPS[79]));
                        materialEditor.ShaderProperty(_ShadowTLightThreshold, new GUIContent(_ShadowTLightThreshold.displayName, TOTIPS[80]));
                        materialEditor.ShaderProperty(_ShadowTShadowThreshold, new GUIContent(_ShadowTShadowThreshold.displayName, TOTIPS[81]));
                        materialEditor.ShaderProperty(_ShadowTHardness, new GUIContent(_ShadowTHardness.displayName, TOTIPS[82]));

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_ShadowTColor, new GUIContent(_ShadowTColor.displayName, TOTIPS[137]));
                        materialEditor.ShaderProperty(_ShadowTColorPower, new GUIContent(_ShadowTColorPower.displayName, TOTIPS[138]));

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_STIL, new GUIContent(_STIL.displayName, TOTIPS[139]));

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_N_F_STIS, new GUIContent(_N_F_STIS.displayName, TOTIPS[83]));
                        materialEditor.ShaderProperty(_N_F_STIAL, new GUIContent(_N_F_STIAL.displayName, TOTIPS[84]));

                        EditorGUI.BeginDisabledGroup(_N_F_STIAL.floatValue == 0 && _N_F_STIS.floatValue == 0);
                        materialEditor.ShaderProperty(_ShowInAmbientLightShadowIntensity, new GUIContent(_ShowInAmbientLightShadowIntensity.displayName, TOTIPS[85]));
                        EditorGUI.EndDisabledGroup();

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_ShowInAmbientLightShadowThreshold, new GUIContent(_ShowInAmbientLightShadowThreshold.displayName, TOTIPS[86]));

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_LightFalloffAffectShadowT, new GUIContent(_LightFalloffAffectShadowT.displayName, TOTIPS[87]));

                    }

                }

#endregion

                //Shadow PTexture

#region PTexture

                if (_N_F_PT.floatValue == 1)
                {
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    Rect r_ptexture = EditorGUILayout.BeginVertical("Button");
                    GUILayout.Label("PTexture", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();

                    if (_N_F_PT.floatValue == 1)
                    {
                        materialEditor.ShaderProperty(_PTexture, new GUIContent(_PTexture.displayName, TOTIPS[88]));
                        materialEditor.ShaderProperty(_PTexturePower, new GUIContent(_PTexturePower.displayName, TOTIPS[89]));
                    }

                }

#endregion

            }

            EditorGUILayout.EndVertical();

#endregion

            //Lighting

#region Lighting

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            Rect r_lighting = EditorGUILayout.BeginVertical("Button");
            ShowLighting = EditorGUILayout.Foldout(ShowLighting, "(Lighting)", true, EditorStyles.foldout);

            if (ShowLighting)
            {

                GUILayout.Space(10);

                materialEditor.ShaderProperty(_RELG, new GUIContent(_RELG.displayName, TOTIPS[90]));
                EditorGUI.BeginDisabledGroup(_RELG.floatValue == 0);
                materialEditor.ShaderProperty(_EnvironmentalLightingIntensity, new GUIContent(_EnvironmentalLightingIntensity.displayName, TOTIPS[91]));

                GUILayout.Space(10);

                materialEditor.ShaderProperty(_GIFlatShade, new GUIContent(_GIFlatShade.displayName, TOTIPS[92]));
                materialEditor.ShaderProperty(_GIShadeThreshold, new GUIContent(_GIShadeThreshold.displayName, TOTIPS[93]));
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(10);

                materialEditor.ShaderProperty(_LightAffectShadow, new GUIContent(_LightAffectShadow.displayName, TOTIPS[94]));
                EditorGUI.BeginDisabledGroup(_LightAffectShadow.floatValue == 0);
                materialEditor.ShaderProperty(_LightIntensity, new GUIContent(_LightIntensity.displayName, TOTIPS[140]));
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(10);
                materialEditor.ShaderProperty(_DirectionalLightIntensity, new GUIContent(_DirectionalLightIntensity.displayName, TOTIPS[95]));
                materialEditor.ShaderProperty(_PointSpotlightIntensity, new GUIContent(_PointSpotlightIntensity.displayName, TOTIPS[96]));

                GUILayout.Space(10);
                materialEditor.ShaderProperty(_LightFalloffSoftness, new GUIContent(_LightFalloffSoftness.displayName, TOTIPS[97]));


                //Custom Light Direction

#region Custom Light Direction

                if (_N_F_CLD.floatValue == 1)
                {

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    EditorGUI.BeginDisabledGroup(_N_F_CLD.floatValue == 0);

                    Rect r_customlightdirection = EditorGUILayout.BeginVertical("Button");
                    GUILayout.Label("Custom Light Direction", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();

                    if (_N_F_CLD.floatValue == 1)
                    {

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_CustomLightDirectionIntensity, new GUIContent(_CustomLightDirectionIntensity.displayName, TOTIPS[98]));
                        materialEditor.ShaderProperty(_CustomLightDirection, new GUIContent(_CustomLightDirection.displayName, TOTIPS[99]));
                        materialEditor.ShaderProperty(_CustomLightDirectionFollowObjectRotation, new GUIContent(_CustomLightDirectionFollowObjectRotation.displayName, TOTIPS[100]));

                    }

                    EditorGUI.EndDisabledGroup();

                }

#endregion
            }

            EditorGUILayout.EndVertical();

#endregion

            //Reflection

#region Reflection

            if (_N_F_R.floatValue == 1)
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                Rect r_reflection = EditorGUILayout.BeginVertical("Button");
                ShowReflection = EditorGUILayout.Foldout(ShowReflection, "(Reflection)", true, EditorStyles.foldout);

                if (ShowReflection)
                {

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_ReflectionIntensity, new GUIContent(_ReflectionIntensity.displayName, TOTIPS[101]));
                    materialEditor.ShaderProperty(_ReflectionRoughtness, new GUIContent(_ReflectionRoughtness.displayName, TOTIPS[102]));

                    if (shader_name == "default_d" || shader_name == "tessellation_d" || shader_name == "default_ft" || shader_name == "tessellation_ft")
                    {
                        materialEditor.ShaderProperty(_RefMetallic, new GUIContent(_RefMetallic.displayName, TOTIPS[103]));
                    }

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_MaskReflection, new GUIContent(_MaskReflection.displayName, TOTIPS[104]));

                    GUILayout.Space(10);

                    //FReflection

#region FReflection

                    if (_N_F_FR.floatValue == 1)
                    {

                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                        Rect r_freflection = EditorGUILayout.BeginVertical("Button");
                        GUILayout.Label("FReflection", EditorStyles.boldLabel);
                        EditorGUILayout.EndVertical();

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_FReflection, new GUIContent(_FReflection.displayName, TOTIPS[105]));

                    }

                }

#endregion

                EditorGUILayout.EndVertical();
            }

#endregion

            // Rim Light

#region Rim Light

            if (_N_F_RL.floatValue == 1)
            {

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                Rect r_rimlight = EditorGUILayout.BeginVertical("Button");
                ShowRimLight = EditorGUILayout.Foldout(ShowRimLight, "(Rim Light)", true, EditorStyles.foldout);

                if (ShowRimLight)
                {

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_RimLightUnfill, new GUIContent(_RimLightUnfill.displayName, TOTIPS[106]));
                    materialEditor.ShaderProperty(_RimLightSoftness, new GUIContent(_RimLightSoftness.displayName, TOTIPS[107]));

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_LightAffectRimLightColor, new GUIContent(_LightAffectRimLightColor.displayName, TOTIPS[108]));

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_RimLightColor, new GUIContent(_RimLightColor.displayName, TOTIPS[109]));
                    materialEditor.ShaderProperty(_RimLightColorPower, new GUIContent(_RimLightColorPower.displayName, TOTIPS[110]));

                    GUILayout.Space(10);
                    materialEditor.ShaderProperty(_RimLightInLight, new GUIContent(_RimLightInLight.displayName, TOTIPS[111]));

                }

                EditorGUILayout.EndVertical();

            }

#endregion

            // Depth

#region Depth

            if (_N_F_D != null)
            {

                if (_N_F_D.floatValue == 1)
                {

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    Rect r_depth = EditorGUILayout.BeginVertical("Button");
                    ShowDepth = EditorGUILayout.Foldout(ShowDepth, "(Depth)", true, EditorStyles.foldout);

                    if (ShowDepth)
                    {

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_Depth, new GUIContent(_Depth.displayName, TOTIPS[112]));
                        materialEditor.ShaderProperty(_DepthEdgeHardness, new GUIContent(_DepthEdgeHardness.displayName, TOTIPS[113]));

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_DepthColor, new GUIContent(_DepthColor.displayName, TOTIPS[114]));
                        materialEditor.ShaderProperty(_DepthColorPower, new GUIContent(_DepthColorPower.displayName, TOTIPS[115]));

                    }

                    EditorGUILayout.EndVertical();

                }

            }
#endregion

            //Tessellation

#region Tessellation

            if (shader_name == "tessellation_d" || shader_name == "tessellation_ft" || shader_name == "tessellation_ref")
            {

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                Rect r_tessellation = EditorGUILayout.BeginVertical("Button");
                ShowTessellation = EditorGUILayout.Foldout(ShowTessellation, "(Tessellation)", true, EditorStyles.foldout);

                if (ShowTessellation)
                {

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_TessellationSmoothness, new GUIContent(_TessellationSmoothness.displayName, TOTIPS[116]));

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_TessellationTransition, new GUIContent(_TessellationTransition.displayName, TOTIPS[117]));
                    materialEditor.ShaderProperty(_TessellationNear, new GUIContent(_TessellationNear.displayName, TOTIPS[118]));
                    materialEditor.ShaderProperty(_TessellationFar,  new GUIContent(_TessellationFar.displayName, TOTIPS[119]));

                }

                EditorGUILayout.EndVertical();
            }

#endregion

            //See Through

#region See Through

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            Rect r_seethrough = EditorGUILayout.BeginVertical("Button");
            ShowSeeThrough = EditorGUILayout.Foldout(ShowSeeThrough, "(See Through)", true, EditorStyles.foldout);

            if (ShowSeeThrough)
            {

                GUILayout.Space(10);

                materialEditor.ShaderProperty(_RefVal, new GUIContent(_RefVal.displayName, TOTIPS[120]));
                materialEditor.ShaderProperty(_Oper, new GUIContent(_Oper.displayName, TOTIPS[121]));
                materialEditor.ShaderProperty(_Compa, new GUIContent(_Compa.displayName, TOTIPS[122]));

            }

            EditorGUILayout.EndVertical();

            GUILayout.Space(20);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

#endregion

            //Disable/Enable Features

#region Disable/Enable Features

            Rect r_disableenablefeature = EditorGUILayout.BeginVertical("Button");
            ShowDisableEnable = EditorGUILayout.Foldout(ShowDisableEnable, "(Disable/Enable Features)", true, EditorStyles.foldout);

            if (ShowDisableEnable)
            {

                Rect r_mc = EditorGUILayout.BeginVertical("HelpBox");
                materialEditor.ShaderProperty(_N_F_MC, new GUIContent(_N_F_MC.displayName, TOTIPSEDF[0]));
                EditorGUILayout.EndVertical();

                Rect r_nm = EditorGUILayout.BeginVertical("HelpBox");
                materialEditor.ShaderProperty(_N_F_NM, new GUIContent(_N_F_NM.displayName, TOTIPSEDF[1]));
                EditorGUILayout.EndVertical();

                if (shader_name == "default_d" || shader_name == "tessellation_d" || shader_name == "default_ft" || shader_name == "tessellation_ft")
                {

                    Rect r_ou = EditorGUILayout.BeginVertical("HelpBox");

                    EditorGUI.BeginChangeCheck();

                    materialEditor.ShaderProperty(_N_F_O, new GUIContent(_N_F_O.displayName, TOTIPSEDF[2]));

                    if (EditorGUI.EndChangeCheck())
                    {
                        int f_o_int = (int)_N_F_O.floatValue;

                        foreach (Material m in materialEditor.targets)
                        {

                            switch (f_o_int)
                            {
                                case 0:
                                    m.SetShaderPassEnabled("Always", false);
                                    break;
                                case 1:
                                    m.SetShaderPassEnabled("Always", true);
                                    break;
                                default:
                                    break;
                            }

                        }

                    }

                    EditorGUILayout.EndVertical();

                    if (shader_name == "default_d" || shader_name == "tessellation_d")
                    {
                        Rect r_co = EditorGUILayout.BeginVertical("HelpBox");
                        materialEditor.ShaderProperty(_N_F_CO, new GUIContent(_N_F_CO.displayName, TOTIPSEDF[3]));
                        EditorGUILayout.EndVertical();
                    }
                }

                Rect r_ca = EditorGUILayout.BeginVertical("HelpBox");
                materialEditor.ShaderProperty(_N_F_CA, new GUIContent(_N_F_CA.displayName, TOTIPSEDF[4]));
                EditorGUILayout.EndVertical();

                Rect r_sl = EditorGUILayout.BeginVertical("HelpBox");
                materialEditor.ShaderProperty(_N_F_SL, new GUIContent(_N_F_SL.displayName, TOTIPSEDF[5]));
                EditorGUILayout.EndVertical();

                Rect r_o = EditorGUILayout.BeginVertical("HelpBox");
                materialEditor.ShaderProperty(_N_F_GLO, new GUIContent(_N_F_GLO.displayName, TOTIPSEDF[6]));
                EditorGUILayout.EndVertical();

                Rect r_glot = EditorGUILayout.BeginVertical("HelpBox");
                materialEditor.ShaderProperty(_N_F_GLOT, new GUIContent(_N_F_GLOT.displayName, TOTIPSEDF[7]));
                EditorGUILayout.EndVertical();

                EditorGUI.BeginChangeCheck();

                Rect r_ss = EditorGUILayout.BeginVertical("HelpBox");
                materialEditor.ShaderProperty(_N_F_SS, new GUIContent(_N_F_SS.displayName, TOTIPSEDF[8]));
                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    int f_ss_int = (int)_N_F_SS.floatValue;
                    foreach (Material m in materialEditor.targets)
                    {
                        switch (f_ss_int)
                        {
                            case 0:
                                m.DisableKeyword("N_F_SON_ON");
                                _N_F_SON.floatValue = 0;
                                break;
                            case 1:
                                break;
                            default:
                                break;
                        }
                    }

                }

                EditorGUI.BeginDisabledGroup(_N_F_SS.floatValue == 0);

                Rect r_son = EditorGUILayout.BeginVertical("HelpBox");
                materialEditor.ShaderProperty(_N_F_SON, new GUIContent(_N_F_SON.displayName, TOTIPSEDF[9]));
                EditorGUILayout.EndVertical();

                EditorGUI.EndDisabledGroup();

                Rect r_sct = EditorGUILayout.BeginVertical("HelpBox");
                materialEditor.ShaderProperty(_N_F_SCT, new GUIContent(_N_F_SCT.displayName, TOTIPSEDF[10]));
                EditorGUILayout.EndVertical();

                Rect r_st = EditorGUILayout.BeginVertical("HelpBox");
                materialEditor.ShaderProperty(_N_F_ST, new GUIContent(_N_F_ST.displayName, TOTIPSEDF[11]));
                EditorGUILayout.EndVertical();

                Rect r_pt = EditorGUILayout.BeginVertical("HelpBox");
                materialEditor.ShaderProperty(_N_F_PT, new GUIContent(_N_F_PT.displayName, TOTIPSEDF[12]));
                EditorGUILayout.EndVertical();

                Rect r_cld = EditorGUILayout.BeginVertical("HelpBox");
                materialEditor.ShaderProperty(_N_F_CLD, new GUIContent(_N_F_CLD.displayName, TOTIPSEDF[13]));
                EditorGUILayout.EndVertical();

                Rect r_r = EditorGUILayout.BeginVertical("HelpBox");
                materialEditor.ShaderProperty(_N_F_R, new GUIContent(_N_F_R.displayName, TOTIPSEDF[14]));
                EditorGUILayout.EndVertical();

                Rect r_fr = EditorGUILayout.BeginVertical("HelpBox");
                materialEditor.ShaderProperty(_N_F_FR, new GUIContent(_N_F_FR.displayName, TOTIPSEDF[15]));
                EditorGUILayout.EndVertical();

                Rect r_rl = EditorGUILayout.BeginVertical("HelpBox");
                materialEditor.ShaderProperty(_N_F_RL, new GUIContent(_N_F_RL.displayName, TOTIPSEDF[16]));
                EditorGUILayout.EndVertical();

                if (shader_name == "default_ref" || shader_name == "tessellation_ref")
                {
                    Rect r_d = EditorGUILayout.BeginVertical("HelpBox");
                    materialEditor.ShaderProperty(_N_F_D, new GUIContent(_N_F_D.displayName, TOTIPSEDF[17]));
                    EditorGUILayout.EndVertical();
                }

            }

            EditorGUILayout.EndVertical();

#endregion

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Space(10);

            materialEditor.ShaderProperty(_N_F_NLASOBF, new GUIContent(_N_F_NLASOBF.displayName, TOTIPS[123]));

            if (shader_name == "default_d" || shader_name == "tessellation_d")
            {
                materialEditor.ShaderProperty(_N_F_HDLS, new GUIContent(_N_F_HDLS.displayName, TOTIPS[125]));
                materialEditor.ShaderProperty(_N_F_HPSS, new GUIContent(_N_F_HPSS.displayName, TOTIPS[126]));
            }

            else if (shader_name == "default_ft" || shader_name == "tessellation_ft" || shader_name == "default_ref" || shader_name == "tessellation_ref")
            {
                if (shader_name == "default_ft" || shader_name == "tessellation_ft")
                {
                    materialEditor.ShaderProperty(_N_F_HCS, new GUIContent(_N_F_HCS.displayName, TOTIPS[127]));
                }

                materialEditor.ShaderProperty(_ZWrite, new GUIContent(_ZWrite.displayName, TOTIPS[128]));
            }

            GUILayout.Space(10);

            materialEditor.RenderQueueField();

            GUILayout.Space(10);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Space(10);

            materialEditor.EnableInstancingField();
            aruskw = EditorGUILayout.Toggle(new GUIContent("Automatic Remove Unused Shader Keywords (Global)", TOTIPS[129]), aruskw);

            GUILayout.Space(10);

        }

#region Automatic Remove UorOSKW
        if (aruskw == true)
        {
            foreach (Material m1 in materialEditor.targets)
            {
                for (int x = 0; x < m1.shaderKeywords.Length; x++)
                {
                    if (m1.shaderKeywords[x] != String.Empty)
                    {
                        for (int y = 0; y < Enum.GetValues(typeof(SFKW)).Length; y++)
                        {
                            if (m1.shaderKeywords[x] == Enum.GetValues(typeof(SFKW)).GetValue(y).ToString())
                            {
                                del_skw = false;
                                break;
                            }
                            else
                            {
                                del_skw = true;
                            }
                        }

                        if (del_skw == true)
                        {
                            m1.DisableKeyword(m1.shaderKeywords[x]);
                            del_skw = false;
                        }

                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
#endregion

        //Footbar
#region Footbar
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        Rect r_footbar = EditorGUILayout.BeginHorizontal();
        GUILayout.Space(5);

        if (GUILayout.Button("| " + ShowUIString + " (Global) |", "Toolbar"))
        {
            if (ShowUI == false)
            {
                ShowUI = true;
                ShowUIString = "Hide UI";
            }
            else
            {
                ShowUI = false;
                ShowUIString = "Show UI";
            }
        }

        EditorGUILayout.EndHorizontal();

#endregion

#endregion

    }

}

//#endif