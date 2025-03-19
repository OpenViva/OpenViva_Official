//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using JBooth.MicroSplat;
using System.Linq;


public class MicroSplatBlendableMaterialEditor : ShaderGUI 
{
   public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] props)
   {
      EditorGUILayout.HelpBox("Material Properties automatically managed by the terrain and the MicroSplatBlendableObject component", MessageType.Info);
   }
}

