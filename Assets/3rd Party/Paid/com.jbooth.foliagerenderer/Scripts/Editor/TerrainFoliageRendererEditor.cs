//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace JBooth.FoliageRendering
{
    [CustomEditor(typeof(TerrainFoliageRenderer))]
    public class TerrainFoliageRendererEditor : Editor
    {
        public static bool _autoRefresh = true;

        [MenuItem("GameObject/Create Foliage Renderer", false)]
        static void CreateIndirectRenderer()
        {
            if (TerrainFoliageRenderer.hasInstance)
            {
                Selection.activeObject = TerrainFoliageRenderer.instance.gameObject;
                EditorGUIUtility.PingObject(TerrainFoliageRenderer.instance.gameObject);
            }
            else
            {
                Selection.activeObject = null;
                GameObject go = new GameObject("Foliage Renderer");
                go.AddComponent<TerrainFoliageRenderer>();
                go.transform.localScale = Vector3.one;
                go.transform.position = Vector3.zero;
            }

            if(IndirectRenderer.hasInstance == false)
            {
                IndirectRenderer i = IndirectRenderer.instance;
            }
        }

        SelectionGrid.Selectable[] selectableTrees;
        int selectedTreeIndex = -1;
        SelectionGrid.Selectable[] selectableDetails;
        int selectedDetailIndex = -1;


        void LoadTreeIcons(List<TerrainFoliageRenderer.TreeOverrideOptions> trees)
        {
            if (trees == null || trees.Count == 0)
            {
                selectableTrees = new SelectionGrid.Selectable[0];
                //treeIcons[0] = new GUIContent("No Trees");
            }
            else
            {
                // Locate the proto types asset preview textures
                selectableTrees = new SelectionGrid.Selectable[trees.Count];
                for (int i = 0; i < selectableTrees.Length; i++)
                {
                    selectableTrees[i] = new SelectionGrid.Selectable();
                    if (trees[i].prototype != null)
                    {
                        Texture tex = AssetPreview.GetAssetPreview(trees[i].prototype.prefab);
                        selectableTrees[i].image = tex != null ? tex : null;
                    }
                    selectableTrees[i].text = selectableTrees[i].tooltip = trees[i].prototype.prefab != null ? trees[i].prototype.prefab.name : "Missing";
                }
            }
        }

        void LoadDetailIcons(List<TerrainFoliageRenderer.DetailOverrideOptions> details)
        {
            if (details == null || details.Count == 0)
            {
                selectableDetails = new SelectionGrid.Selectable[0];
                //treeIcons[0] = new GUIContent("No Trees");
            }
            else
            {
                // Locate the proto types asset preview textures
                selectableDetails = new SelectionGrid.Selectable[details.Count];
                for (int i = 0; i < selectableDetails.Length; i++)
                {
                    selectableDetails[i] = new SelectionGrid.Selectable();
                    if (details[i].prototype != null)
                    {
                        Texture tex = AssetPreview.GetAssetPreview(details[i].prototype.prototype);
                        selectableDetails[i].image = tex != null ? tex : null;
                    }

                    selectableDetails[i].text = selectableDetails[i].tooltip = details[i].prototype.prototype != null ? details[i].prototype.prototype.name : "Missing";

                    selectableDetails[i].overrideEnabled = details[i].overrideDrawOptions;
                }
            }
        }

        void DrawMaterialOptions(TerrainFoliageRenderer mgr, Terrain[] terrains)
        {

            if (FoliageGUIUtil.DrawRollup("Material Options"))
            {
                using (new EditorGUILayout.VerticalScope(FoliageGUIUtil.boxStyle))
                {
                    EditorGUI.indentLevel++;
                    mgr.materialOptions.supplyHeightmaps = EditorGUILayout.Toggle("Supply Height Maps", mgr.materialOptions.supplyHeightmaps);
                    mgr.materialOptions.supplyNormalmaps = EditorGUILayout.Toggle("Supply Normal Maps", mgr.materialOptions.supplyNormalmaps);
                    mgr.materialOptions.albedoMapGeneration = (TerrainFoliageRenderer.MaterialOptions.BaseMapRes)EditorGUILayout.EnumPopup("Generate Albedo Map", mgr.materialOptions.albedoMapGeneration);
                    EditorGUILayout.LabelField("Extra Textures");
                    EditorGUI.indentLevel++;
                    for (int j = 0; j < mgr.materialOptions.extraTextureOptions.Count; ++j)
                    {
                        var eto = mgr.materialOptions.extraTextureOptions[j];
                        eto.config = (TerrainExtraTextureConfig)EditorGUILayout.ObjectField(new GUIContent("Config"), eto.config, typeof(TerrainExtraTextureConfig), false);
                        // remove dead entries
                        for (int i = 0; i < eto.textureEntries.Count; ++i)
                        {
                            var te = eto.textureEntries[i];
                            if (te.terrain == null)
                            {
                                eto.textureEntries.RemoveAt(i);
                                i--;
                            }
                            else
                            {
                                bool found = false;
                                foreach (var t in terrains)
                                {
                                    if (t == te.terrain)
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    eto.textureEntries.RemoveAt(i);
                                    i--;
                                }
                            }
                        }
                        foreach (var t in terrains)
                        {
                            bool found = false;
                            foreach (var te in eto.textureEntries)
                            {
                                if (t == te.terrain)
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                eto.textureEntries.Add(new TerrainFoliageRenderer.TerrainExtraTexture() { terrain = t });
                            }
                        }

                        // now draw editor
                        foreach (var te in eto.textureEntries)
                        {
                            te.texture = (Texture)EditorGUILayout.ObjectField(te.terrain.name, te.texture, typeof(Texture2D), true);
                        }
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("Delete Entry", GUILayout.Width(150)))
                            {
                                mgr.materialOptions.extraTextureOptions.RemoveAt(j);
                                j--;
                            }
                        }
                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    }
                    EditorGUI.indentLevel--;

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("New Entry", GUILayout.Width(150)))
                        {
                            mgr.materialOptions.extraTextureOptions.Add(new TerrainFoliageRenderer.ExtraTextureOption());
                        }
                        GUILayout.FlexibleSpace();
                    }

                    EditorGUI.indentLevel--;
                }
            }
        }

        void GetAllPrototypes(Terrain[] terrains)
        {
            allTrees.Clear();
            allDetails.Clear();
            allGameObjects.Clear();
            foreach (var terrain in terrains)
            {
                if (terrain.terrainData == null)
                    continue;

                var details = terrain.terrainData.detailPrototypes;
                var trees = terrain.terrainData.treePrototypes;

                allDetails.AddRange(details);
                allTrees.AddRange(trees);
            }

            allTrees = allTrees.Distinct().ToList();
            allDetails = allDetails.Distinct().ToList();

            foreach (var t in allTrees)
            {
                if (t.prefab != null) allGameObjects.Add(t.prefab);
            }
            foreach (var d in allDetails)
            {
                if (d.prototype != null) allGameObjects.Add(d.prototype);
            }
            allGameObjects = allGameObjects.Distinct().ToList(); 
        }

        List<DetailPrototype> allDetails = new List<DetailPrototype>();
        List<TreePrototype> allTrees = new List<TreePrototype>();
        List<GameObject> allGameObjects = new List<GameObject>();

        void DrawPrototypeSettings(TerrainFoliageRenderer mgr, Terrain[] terrains)
        {
            SerializedObject so = new SerializedObject(mgr);
            so.Update();

            if (FoliageGUIUtil.DrawRollup("Prototype Settings"))
            {
                if (FoliageGUIUtil.DrawRollup("Default Settings", true, true))
                {                   
                    SerializedProperty treeOptions = so.FindProperty(nameof(TerrainFoliageRenderer.treeOptions));
                    SerializedProperty detailOptions = so.FindProperty(nameof(TerrainFoliageRenderer.detailOptions));

                    SerializedProperty detailMode = so.FindProperty(nameof(TerrainFoliageRenderer.detailCachingMode));
                    SerializedProperty detailDist = so.FindProperty(nameof(TerrainFoliageRenderer.detailCachingDistance));

                    using (new EditorGUILayout.VerticalScope(FoliageGUIUtil.boxStyle))
                    {
                        EditorGUILayout.LabelField("Trees:");
                        if(FoliageGUIUtil.DrawOptionsGUI(treeOptions, true))
                        {
                            so.ApplyModifiedProperties();
                            mgr.RefreshAllProviders();
                        }
                        FoliageGUIUtil.DrawSeparator();
                        EditorGUILayout.LabelField("Details:");
                        if(FoliageGUIUtil.DrawOptionsGUI(detailOptions, true, true, null, true))
                        {
                            so.ApplyModifiedProperties();
                            mgr.RefreshAllProviders();
                        }
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(detailMode, true);
                        if (detailMode.enumValueIndex == 1)
                            EditorGUILayout.PropertyField(detailDist, true);
                        EditorGUI.indentLevel--;
                    }
                }

                // clear out old, add new
                for (int i = 0; i < mgr.treeOverrides.Count; ++i)
                {
                    var proto = mgr.treeOverrides[i].prototype;
                    bool found = false;
                    foreach (var tree in allTrees)
                    {
                        if (proto.IsEqualToTree(tree))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        mgr.treeOverrides.RemoveAt(i);
                        i--;
                    }
                }
                foreach (var tree in allTrees)
                {
                    var op = mgr.FindTreeOverrideOption(tree);
                    if (op == null)
                    {
                        op = new TerrainFoliageRenderer.TreeOverrideOptions();
                        op.prototype = new TreePrototypeSerializable(tree);
                        op.drawOptions = new DrawOptions(mgr.treeOptions);
                        mgr.treeOverrides.Add(op);
                    }
                }

                for (int i = 0; i < mgr.detailOverrides.Count; ++i)
                {
                    var proto = mgr.detailOverrides[i].prototype;
                    bool found = false;
                    foreach (var detail in allDetails)
                    {
                        if (proto.IsEqualToDetail(detail))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        mgr.detailOverrides.RemoveAt(i);
                        i--;
                    }
                }
                foreach (var detail in allDetails)
                {
                    var op = mgr.FindDetailOverrideOption(detail);
                    if (op == null)
                    {
                        op = new TerrainFoliageRenderer.DetailOverrideOptions();
                        op.prototype = new DetailPrototypeSerializable(detail);
                        op.drawOptions = new DrawOptions(mgr.treeOptions);
                        mgr.detailOverrides.Add(op);
                    }
                }

                LoadTreeIcons(mgr.treeOverrides);
                // grid selection
                if (FoliageGUIUtil.DrawRollup("Tree Overrides", true, true))
                {
                    using (new EditorGUILayout.VerticalScope(FoliageGUIUtil.boxStyle))
                    {
                        // get current active state
                        for (int i = 0; i < mgr.treeOverrides.Count; i++)
                        {
                            selectableTrees[i].overrideEnabled = mgr.treeOverrides[i].overrideDrawOptions;
                        }

                        bool changed = SelectionGrid.ShowSelectionGrid(ref selectedTreeIndex, selectableTrees, 128);

                        if (changed)
                        {
                            EditorUtility.SetDirty(mgr);
                        }

                        if (selectedTreeIndex >= 0 && selectedTreeIndex < mgr.treeOverrides.Count)
                        {
                            SerializedProperty details = so.FindProperty(nameof(TerrainFoliageRenderer.treeOverrides));
                            SerializedProperty t = details.GetArrayElementAtIndex(selectedTreeIndex);
                            SerializedProperty tOptions = t.FindPropertyRelative(nameof(TerrainFoliageRenderer.TreeOverrideOptions.drawOptions));
                            SerializedProperty tEnabled = t.FindPropertyRelative(nameof(TerrainFoliageRenderer.TreeOverrideOptions.overrideDrawOptions));
                            tEnabled.boolValue = GUILayout.Toggle(tEnabled.boolValue, "Custom Draw Options");
                            so.ApplyModifiedProperties();
                            EditorGUI.BeginDisabledGroup(!tEnabled.boolValue);
                            SerializedProperty prefabObject = t.FindPropertyRelative(nameof(TerrainFoliageRenderer.TreeOverrideOptions.prototype));
                            SerializedProperty proto = prefabObject.FindPropertyRelative(nameof(TreePrototypeSerializable.prefab));
                            GameObject go = proto.objectReferenceValue != null ? proto.objectReferenceValue as GameObject : null;
                            SerializedProperty treeOptions = so.FindProperty(nameof(TerrainFoliageRenderer.treeOptions));
                            if(FoliageGUIUtil.DrawOptionsGUI(tOptions, true, true, go, false, treeOptions))
                            {
                                so.ApplyModifiedProperties();
                                mgr.RefreshAllProviders();
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                    }
                }
                LoadDetailIcons(mgr.detailOverrides);
                
                if (FoliageGUIUtil.DrawRollup("Detail Overrides"))
                {
                    using (new EditorGUILayout.VerticalScope(FoliageGUIUtil.boxStyle))
                    {
                        // get current active state
                        for (int i = 0; i < mgr.detailOverrides.Count; i++)
                        {
                            selectableDetails[i].overrideEnabled = mgr.detailOverrides[i].overrideDrawOptions;
                        }

                        bool changed = SelectionGrid.ShowSelectionGrid(ref selectedDetailIndex, selectableDetails, 128);

                        if (changed)
                        {
                            EditorUtility.SetDirty(mgr);
                        }

                        if (selectedDetailIndex >= 0 && selectedDetailIndex < mgr.detailOverrides.Count)
                        {
                            SerializedProperty details = so.FindProperty(nameof(TerrainFoliageRenderer.detailOverrides));
                            SerializedProperty t = details.GetArrayElementAtIndex(selectedDetailIndex);
                            SerializedProperty tOptions = t.FindPropertyRelative(nameof(TerrainFoliageRenderer.DetailOverrideOptions.drawOptions));
                            SerializedProperty tEnabled = t.FindPropertyRelative(nameof(TerrainFoliageRenderer.DetailOverrideOptions.overrideDrawOptions));
                            tEnabled.boolValue = GUILayout.Toggle(tEnabled.boolValue, "Custom Draw Options");
                            so.ApplyModifiedProperties();

                            EditorGUI.BeginDisabledGroup(!tEnabled.boolValue);
                            SerializedProperty prefabObject = t.FindPropertyRelative(nameof(TerrainFoliageRenderer.DetailOverrideOptions.prototype));
                            SerializedProperty proto = prefabObject.FindPropertyRelative(nameof(DetailPrototypeSerializable.prototype));
                            GameObject go = proto.objectReferenceValue != null ? proto.objectReferenceValue as GameObject : null;
                            SerializedProperty detailOptions = so.FindProperty(nameof(TerrainFoliageRenderer.detailOptions));
                            if(FoliageGUIUtil.DrawOptionsGUI(tOptions, true, true, go, true, detailOptions))
                            {
                                so.ApplyModifiedProperties();
                                mgr.RefreshAllProviders();
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                    }
                }
            }
            so.ApplyModifiedProperties();
        }

        void DrawConversionGUI(TerrainFoliageRenderer mgr)
        {
            shaderPatching.DrawPatchingGUI(allGameObjects);
        }
        ShaderPatching shaderPatching = new ShaderPatching();
        public override void OnInspectorGUI()
        {
            FoliageGUIUtil.DrawHeaderLogo();
            serializedObject.Update();
            
            TerrainFoliageRenderer mgr = (target as TerrainFoliageRenderer);

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope(GUILayout.Height(18.0f)))
            {
                _autoRefresh = EditorGUILayout.ToggleLeft("Auto Refresh Terrains", _autoRefresh);
                EditorGUILayout.Space();
                if(GUILayout.Button(new GUIContent("Refresh All", "Refresh all Terrains and Foliage Providers"), EditorStyles.miniButton))
                    mgr.RefreshAllProviders();
            }
            EditorGUILayout.Space();

            if(_autoRefresh )
                mgr.RefreshTerrains();
            var terrains = mgr.terrains;
            GetAllPrototypes(terrains);
            DrawConversionGUI(mgr);
            EditorGUI.BeginChangeCheck();
            DrawPrototypeSettings(mgr, terrains);
            DrawMaterialOptions(mgr, terrains);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(mgr);               
            }
            if(serializedObject.ApplyModifiedProperties())
                mgr.RefreshAllProviders();
        }

    }
}
