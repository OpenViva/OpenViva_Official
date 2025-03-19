//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JBooth.MicroSplat;

#if __MICROSPLAT__
[CustomEditor(typeof(MicroSplatBlendableObject))]
[CanEditMultipleObjects]
public class MicroSplatBlendableObjectEditor : Editor 
{
   static GUIContent COverrideObject = new GUIContent ("Override Blend Target", "If null, the terrain below this object will be automatically detected. However, if the pivot of your object is not over the terrain, or you want to manually specify it, you can set that here");
   static GUIContent CBlendDistance = new GUIContent("Blend Distance", "How far to blend the terrain onto the object");
   static GUIContent CBlendNormalDistance = new GUIContent("Blend Normal Distance", "How far to blend the terrain's normals onto the object");
   static GUIContent CBlendContrast = new GUIContent("Blend Contrast", "Blend between Linear or height based blend");
   static GUIContent CBlendCurve = new GUIContent("Blend Curve", "Controls the curve of the blend, 0.5 will transition quickly, while 2 would transition slower");
   static GUIContent CSlopeFilter = new GUIContent("Slope Filter", "When set below one, terrain will appear on the tops of the object. Useful for sand or moss, drapped on the top of rock");
   static GUIContent CSlopeContrast = new GUIContent("Slope Contrast", "Contrast for slope filter");
   static GUIContent CSlopeNoise = new GUIContent("Slope Noise", "How much noise to blend into the transition");
   static GUIContent CSnowWidth = new GUIContent("Snow Width", "Allows you exagerate the snow based on angle");

   static GUIContent CTerrainBlending = new GUIContent("Terrain Blending", "Turn on/off blending with the terrain");
   static GUIContent CSnow = new GUIContent("Object Snow", "Turn on/off snow accumulation on object");
   static GUIContent CNormalFromObject = new GUIContent("Normal from object", "Used to make blending with terrain take object normal map into account - especially useful on low-poly props relying on normals to hide their topology. Automatically taken from shaders with a _BumpMap property");
   static GUIContent CNoiseScale = new GUIContent("Blend Noise Scale", "Scale of noise");
   static GUIContent CMatrixBlend = new GUIContent("Lighting Matrix Blend", "How much to blend of the lighting matrix; this can help prevent over lighting on the back side of a blended object");
   public override void OnInspectorGUI()
   {
      MicroSplatBlendableObject bo = (MicroSplatBlendableObject)target;

      if (bo.msObject == null && bo.msOverrideObject == null)
      {
         EditorGUILayout.HelpBox ("Terrain not detected below object, nothing to blend with. You may assign a terrain manually to the Override Blend Target", MessageType.Error);
         serializedObject.Update ();
         var over = serializedObject.FindProperty ("msOverrideObject");
         EditorGUILayout.PropertyField (over, COverrideObject);
         serializedObject.ApplyModifiedProperties ();
         return;
      }
      var msObj = bo.msObject;
      if (msObj == null)
      {
         msObj = bo.msOverrideObject;
      }


      if (msObj.blendMat == null)
      {
         EditorGUILayout.HelpBox("Terrain shader is not setup for blending, please enable this on the terrain material in the Shader Generator section", MessageType.Error);
         return;
      }


      serializedObject.Update();
      var obt = serializedObject.FindProperty ("msOverrideObject");
      var nfo = serializedObject.FindProperty("normalFromObject");
      var bd = serializedObject.FindProperty("blendDistance");
      var bc = serializedObject.FindProperty("blendContrast");
      var bcv = serializedObject.FindProperty("blendCurve");
      var sf = serializedObject.FindProperty("slopeFilter");
      var sc = serializedObject.FindProperty("slopeContrast");
      var sn = serializedObject.FindProperty("slopeNoise");
      var tbn = serializedObject.FindProperty("normalBlendDistance");
      var snml = serializedObject.FindProperty("snowWidth");
      var doSnow = serializedObject.FindProperty("doSnow");
      var doTerrain = serializedObject.FindProperty("doTerrainBlend");
      var noiseScale = serializedObject.FindProperty("noiseScale");
      var matrixBlend = serializedObject.FindProperty("matrixBlend");

      Material bmInstance = msObj.GetBlendMatInstance();
      bool normalBlendEnabled = (bmInstance != null && msObj.keywordSO != null && msObj.keywordSO.IsKeywordEnabled("_TBOBJECTNORMALBLEND")) ;
      if (nfo.objectReferenceValue == null && normalBlendEnabled)
      {
         Renderer r = bo.GetComponent<Renderer>();
         var materials = r.sharedMaterials;
         int suitableIndex = -1;
         for (int i = 0; i < materials.Length; ++i)
         {
            if (materials[i] != bmInstance && materials[i].HasProperty("_BumpMap"))
               suitableIndex = i;
         }

         if (suitableIndex != -1)
         {
            nfo.objectReferenceValue = materials[suitableIndex].GetTexture("_BumpMap");
         }
      }




      EditorGUI.BeginChangeCheck();
      if (msObj.keywordSO != null && msObj.keywordSO.IsKeywordEnabled("_TERRAINBLENDING"))
      {
         using (new GUILayout.VerticalScope(GUI.skin.box))
         {
            var old = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Detected Terrain", msObj, typeof(MicroSplatObject), false);
            GUI.enabled = old;
            EditorGUILayout.PropertyField (obt, COverrideObject);
            EditorGUILayout.PropertyField(doTerrain, CTerrainBlending);
            if (doTerrain.boolValue == true)
            {
               EditorGUILayout.PropertyField(bd, CBlendDistance);
               EditorGUILayout.PropertyField(tbn, CBlendNormalDistance);
               EditorGUILayout.PropertyField(bc, CBlendContrast);
               EditorGUILayout.PropertyField(matrixBlend, CMatrixBlend);
               EditorGUILayout.PropertyField(bcv, CBlendCurve);
               EditorGUILayout.PropertyField(sf, CSlopeFilter);
               EditorGUILayout.PropertyField(sc, CSlopeContrast);
               EditorGUILayout.PropertyField(sn, CSlopeNoise);
               if (msObj.keywordSO.IsKeywordEnabled("_TBNOISE") || msObj.keywordSO.IsKeywordEnabled("_TBNOISEFBM"))
               {
                  EditorGUILayout.PropertyField(noiseScale, CNoiseScale);
               }

               if (normalBlendEnabled)
               {
                  EditorGUILayout.PropertyField(nfo, CNormalFromObject);
               }
            }
         }
      }
      if (msObj.keywordSO != null && msObj.keywordSO.IsKeywordEnabled("_SNOW"))
      {
         using (new GUILayout.VerticalScope(GUI.skin.box))
         {
            EditorGUILayout.PropertyField(doSnow, CSnow);
            if (doSnow.boolValue == true)
            {
               EditorGUILayout.PropertyField(snml, CSnowWidth);
            }
         }
      }


      if (EditorGUI.EndChangeCheck())
      {
         serializedObject.ApplyModifiedProperties();
         for (int i = 0; i < targets.Length; ++i)
         {
            MicroSplatBlendableObject o = (MicroSplatBlendableObject)targets[i];
            if (o != null)
            {
               o.Sync();
            }
         }
         SceneView.RepaintAll();
      }


      var mf = bo.GetComponent<MeshFilter>();
      if (mf != null && mf.sharedMesh != null && mf.sharedMesh.subMeshCount > 2)
      {
         EditorGUILayout.HelpBox("Sub-Meshes cannot not blend properly, only the first submesh will blend with the terrain.", MessageType.Warning);
      }

   }
	
}
#endif
