//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;
using System.Text;

namespace JBooth.MicroSplat
{
   #if __MICROSPLAT__
   [InitializeOnLoad]
   public class MicroSplatTerrainBlending : FeatureDescriptor
   {
      const string sDefine = "__MICROSPLAT_TERRAINBLEND__";
      static MicroSplatTerrainBlending()
      {
         MicroSplatDefines.InitDefine(sDefine);
      }
      [PostProcessSceneAttribute (0)]
      public static void OnPostprocessScene()
      { 
         MicroSplatDefines.InitDefine(sDefine);
      }

      public override string ModuleName()
      {
         return "Terrain Blending";
      }

      public override string GetHelpPath ()
      {
         return "https://docs.google.com/document/d/1mH3uCP8jNQpaHjbSYdxW4cDWZNHlKWdvNR7ASmiOwYU/edit?usp=sharing";
      }

      public enum DefineFeature
      {
         _TERRAINBLENDING,
         _TBDISABLE_DETAILNOISE,
         _TBDISABLE_DISTANCENOISE,
         _TBDISABLE_DISTANCERESAMPLE,
         _TBDISABLE_NOISENORMAL,
         _TBOBJECTNORMALBLEND,
         _TBDISABLE_ALPHACONTROL,
         _TBNOISE,
         _TBNOISEFBM,
         _TBDISABLEALPHAHOLES,
         _TERRAINBLENDMATCHWORLDUV,
         kNumFeatures,
      }
         
      static TextAsset props;

      public enum BlendNoise
      {
         None,
         Simple,
         Complex
      }

      public bool terrainBlend;
      public bool alphaShader;
      public bool disableDetailNoise;
      public bool disableDistanceNoise;
      public bool disableDistanceResampling;
      public bool disableNoiseNormal;
      public bool objectNormalBlend;
      public bool disableAlphaControl;
      public bool disableAlphaHoles;
      public bool triplanarWhenWorldUV;
      public BlendNoise blendNoise = BlendNoise.None;

      GUIContent CTerrainBlend = new GUIContent("Terrain Blending", "Generate shader and enable system for mesh:terrain blending");
      GUIContent CDisableDetailNoise = new GUIContent("Disable Detail Noise", "Disabled detail noise in blendable object shader");
      GUIContent CDisableDistanceNoise = new GUIContent("Disable Distance Noise", "Disabled distance noise in blendable object shader");
      GUIContent CDisableDistanceResample = new GUIContent("Disable Distance Resample", "Disabled distance resample in blendable object shader");
      GUIContent CDisableNoiseNormal = new GUIContent("Disable Noise Normal", "Disable noise normal in the blendable object shader");
      GUIContent CObjectNormalBlend = new GUIContent("Object Normal Blending", "When enabled, allows you to blend with the objects original normal map if provided");
      GUIContent CDisableAlphaControl = new GUIContent("Disable Alpha", "By default, the vertex color's alpha channel can be used to control the blend manually. However, if the shader being blended with already uses the vertex color alpha for something it may be necissary to disable it");
      GUIContent CBlendNoise = new GUIContent("Blend Noise", "Uses one or three octave noise for break up the blend");
      GUIContent CDisableAlphaHoles = new GUIContent ("Disable Alpha Holes", "Prevent alpha holes from clipping the terrain blended shader");
      GUIContent CTriplanarWorldUV = new GUIContent("Enable Triplanar", "When using worldUVs, you are able to turn triplanar on just for the blendable shader");
      // Can we template these somehow?
      static Dictionary<DefineFeature, string> sFeatureNames = new Dictionary<DefineFeature, string>();
      public static string GetFeatureName(DefineFeature feature)
      {
         string ret;
         if (sFeatureNames.TryGetValue(feature, out ret))
         {
            return ret;
         }
         string fn = System.Enum.GetName(typeof(DefineFeature), feature);
         sFeatureNames[feature] = fn;
         return fn;
      }

      public static bool HasFeature(string[] keywords, DefineFeature feature)
      {
         string f = GetFeatureName(feature);
         for (int i = 0; i < keywords.Length; ++i)
         {
            if (keywords[i] == f)
               return true;
         }
         return false;
      }
         
      public override string GetVersion()
      {
         return "3.9";
      }

      public override MicroSplatShaderGUI.MicroSplatCompiler.AuxShader GetAuxShader ()
      {
         var aux = new MicroSplatShaderGUI.MicroSplatCompiler.AuxShader ("_TERRAINBLENDING", "_TerrainObjectBlend");
         aux.customEditor = "MicroSplatBlendableMaterialEditor";
         aux.options = "       Alpha \"Blend\"\n";
         return aux;
      }

      public override void ModifyKeywordsForAuxShader (List<string> keywords)
      {
         if (keywords.Contains ("_TERRAINBLENDING"))
         {
            keywords.Remove ("_TESSDISTANCE");
            keywords.Add ("_TERRAINBLENDABLESHADER");

            if (!keywords.Contains("_TRIPLANAR") && keywords.Contains("_TERRAINBLENDMATCHWORLDUV"))
            {
               keywords.Add("_TRIPLANAR");
            }
            if (keywords.Contains("_TBDISABLE_NOISENORMAL"))
            {
               keywords.Remove("_NORMALNOISE");
               keywords.Remove("_NORMALNOISE2");
               keywords.Remove("_NORMALNOISE3");
            }
            if (keywords.Contains("_TBDISABLE_DETAILNOISE"))
            {
               keywords.Remove("_DETAILNOISE");
            }
            if (keywords.Contains ("_TBDISABLE_DETAILNOISE"))
            {
               keywords.Remove ("_ANTITILEARRAYDETAIL");
            }
            if (keywords.Contains ("_TBDISABLE_DISTANCENOISE"))
            {
               keywords.Remove ("_DISTANCENOISE");
            }
            if (keywords.Contains ("_TBDISABLE_DISTANCENOISE"))
            {
               keywords.Remove ("_ANTITILEARRAYDISTANCE");
            }
            if (keywords.Contains ("_TBDISABLE_DISTANCERESAMPLE"))
            {
               keywords.Remove ("_DISTANCERESAMPLE");
            }

            if (keywords.Contains("_PARALLAX"))
            {
               keywords.Remove("_PARALLAX");
            }
            if (keywords.Contains("_TESSDISTANCE"))
            {
               keywords.Remove("_TESSDISTANCE");
            }
         }
      }

      public override void DrawFeatureGUI(MicroSplatKeywords keywords)
      {
         if (keywords.IsKeywordEnabled ("_MICROMESH"))
         {
            return;
         }
         bool old = terrainBlend;
         terrainBlend = EditorGUILayout.Toggle(CTerrainBlend, terrainBlend);
         if (old)
         {
            EditorGUI.indentLevel++;

            objectNormalBlend = EditorGUILayout.Toggle(CObjectNormalBlend, objectNormalBlend);
            disableAlphaControl = EditorGUILayout.Toggle(CDisableAlphaControl, disableAlphaControl);
            blendNoise = (BlendNoise)EditorGUILayout.EnumPopup(CBlendNoise, blendNoise);
            disableAlphaHoles = EditorGUILayout.Toggle(CDisableAlphaHoles, disableAlphaHoles);
            if (keywords.IsKeywordEnabled("_DETAILNOISE") || keywords.IsKeywordEnabled("_ANTITILEARRAYDETAIL"))
            {
               disableDetailNoise = EditorGUILayout.Toggle(CDisableDetailNoise, disableDetailNoise);
            }
            if (keywords.IsKeywordEnabled("_DISTANCENOISE") || keywords.IsKeywordEnabled("_ANTITILEARRAYDISTANCE"))
            {
               disableDistanceNoise = EditorGUILayout.Toggle(CDisableDistanceNoise, disableDistanceNoise);
            }
            if (keywords.IsKeywordEnabled("_DISTANCERESAMPLE"))
            {
               disableDistanceResampling = EditorGUILayout.Toggle(CDisableDistanceResample, disableDistanceResampling);
            }
            if (keywords.IsKeywordEnabled("_NORMALNOISE") || keywords.IsKeywordEnabled("_NORMALNOISE2") || keywords.IsKeywordEnabled("_NORMALNOISE3"))
            {
               disableNoiseNormal = EditorGUILayout.Toggle(CDisableNoiseNormal, disableNoiseNormal);
            }
#if __MICROSPLAT_TRIPLANAR__
            if (keywords.IsKeywordEnabled("_WORLDUV") && !keywords.IsKeywordEnabled("_TRIPLANAR"))
            {
               triplanarWhenWorldUV = EditorGUILayout.Toggle(CTriplanarWorldUV, triplanarWhenWorldUV);
            }
#endif
            EditorGUI.indentLevel--;
         }
      }

      public override void DrawShaderGUI(MicroSplatShaderGUI shaderGUI, MicroSplatKeywords keywords, Material mat, MaterialEditor materialEditor, MaterialProperty[] props)
      {

      }

      public override string[] Pack()
      {
         List<string> features = new List<string>();
         if (terrainBlend)
         {
            features.Add(GetFeatureName(DefineFeature._TERRAINBLENDING));

            if (blendNoise == BlendNoise.Simple)
            {
               features.Add(GetFeatureName(DefineFeature._TBNOISE));
            }
            else if (blendNoise == BlendNoise.Complex)
            {
               features.Add(GetFeatureName(DefineFeature._TBNOISEFBM));
            }

            if (disableNoiseNormal)
            {
               features.Add(GetFeatureName(DefineFeature._TBDISABLE_NOISENORMAL));
            }
            if (triplanarWhenWorldUV)
            {
               features.Add(GetFeatureName(DefineFeature._TERRAINBLENDMATCHWORLDUV));
            }

            if (disableAlphaControl)
            {
               features.Add(GetFeatureName(DefineFeature._TBDISABLE_ALPHACONTROL));
            }
            if (objectNormalBlend)
            {
               features.Add(GetFeatureName(DefineFeature._TBOBJECTNORMALBLEND));
            }

            if (disableDetailNoise)
            {
               features.Add(GetFeatureName(DefineFeature._TBDISABLE_DETAILNOISE));
            }
            if (disableDistanceNoise)
            {
               features.Add(GetFeatureName(DefineFeature._TBDISABLE_DISTANCENOISE));
            }
            if (disableDistanceResampling)
            {
               features.Add(GetFeatureName(DefineFeature._TBDISABLE_DISTANCERESAMPLE));
            }
            if (disableAlphaHoles)
            {
               features.Add (GetFeatureName (DefineFeature._TBDISABLEALPHAHOLES));
            }
         }
         return features.ToArray();
      }

      public override void Unpack(string[] keywords)
      {
         terrainBlend = HasFeature(keywords, DefineFeature._TERRAINBLENDING);
         if (terrainBlend)
         {
            disableAlphaControl = HasFeature(keywords, DefineFeature._TBDISABLE_ALPHACONTROL);
            disableDetailNoise = HasFeature(keywords, DefineFeature._TBDISABLE_DETAILNOISE);
            disableDistanceNoise = HasFeature(keywords, DefineFeature._TBDISABLE_DISTANCENOISE);
            disableDistanceResampling = HasFeature(keywords, DefineFeature._TBDISABLE_DISTANCERESAMPLE);
            disableNoiseNormal = HasFeature(keywords, DefineFeature._TBDISABLE_NOISENORMAL);
            objectNormalBlend = HasFeature(keywords, DefineFeature._TBOBJECTNORMALBLEND);
            triplanarWhenWorldUV = HasFeature(keywords, DefineFeature._TERRAINBLENDMATCHWORLDUV);
            blendNoise = BlendNoise.None;
            if (HasFeature(keywords, DefineFeature._TBNOISE))
            {
               blendNoise = BlendNoise.Simple;
            }
            else if (HasFeature(keywords, DefineFeature._TBNOISEFBM))
            {
               blendNoise = BlendNoise.Complex;
            }
            disableAlphaHoles = HasFeature (keywords, DefineFeature._TBDISABLEALPHAHOLES);
         }
         
      }

      public override void InitCompiler(string[] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths[i];
            if (p.EndsWith("microsplat_properties_terrainblend.txt"))
            {
               props = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
         }
      }

      public override void WriteProperties(string[] features, System.Text.StringBuilder sb)
      {
         if (terrainBlend)
         {
            sb.AppendLine(props.text);
            if (objectNormalBlend)
            {
               sb.AppendLine("      [NoScaleOffset]_NormalOriginal (\"Normal(from original)\", 2D) = \"bump\" {}");
            }
            if (blendNoise != BlendNoise.None)
            {
               sb.AppendLine("      _TBNoiseScale(\"Noise Scale\", Float) = 1");
            }
         }
      }

      public override void WriteFunctions(string [] features, System.Text.StringBuilder sb)
      {

      }

      public override void ComputeSampleCounts(string[] features, ref int arraySampleCount, ref int textureSampleCount, ref int maxSamples, ref int tessellationSamples, ref int depTexReadLevel)
      {
  
      }

   }   

   #endif
}