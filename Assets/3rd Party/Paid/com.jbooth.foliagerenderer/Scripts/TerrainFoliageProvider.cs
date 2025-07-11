//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Profiling;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JBooth.FoliageRendering
{
    [ExecuteAlways]
    [HelpURL("https://dicewrenchdesigns.com/category/assets/")]
    public class TerrainFoliageProvider : MonoBehaviour
    {       
        List<Draw> treeDrawList = new List<Draw>();
        private Terrain terrain;

        public RenderTexture albedoTexture;

        bool needsRefresh = false;

        private CellSystem cellSystem;

        private void RenderTerrainAlbedo(int res)
        {
            if (albedoTexture != null)
            {
                albedoTexture.Release();
                albedoTexture = null;
            }
            var splatMaps = terrain.terrainData.alphamapTextures;
            var layers = terrain.terrainData.terrainLayers;

            Material albedoMat = new Material(Resources.Load<Shader>("FoliageRendererAlbedoGen"));
            Vector2 uvScaler = new Vector2(terrain.terrainData.size.x, terrain.terrainData.size.z);
            for (int i = 0; i < layers.Length; ++i)
            {
                string name = "_Albedo" + i;
                albedoMat.SetTexture(name, layers[i].diffuseTexture);
                albedoMat.SetTextureScale(name, uvScaler / layers[i].tileSize);
            }
            for (int i = 0; i < splatMaps.Length; ++i)
            {
                albedoMat.SetTexture("_Control" + i, splatMaps[i]);
            }

            RenderTexture rt = new RenderTexture(res, res, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            Graphics.Blit(null, rt, albedoMat);
            albedoTexture = rt;
            RenderTexture.active = null;
            DestroyImmediate(albedoMat);
        }

        public RenderTexture GetAlbedoTexture(int res)
        {
            if (albedoTexture == null || albedoTexture.width != res)
            {
                RenderTerrainAlbedo(res);
            }
            return albedoTexture;
        }

        private void Unregister()
        {
            if (IndirectRenderer.hasInstance)
            {
                IndirectRenderer.instance.Unregister(GetInstanceID().ToString());
            }
            cellSystem.UnregisterCells();

            Bounds worldBounds = terrain.terrainData.bounds;
            worldBounds.center += terrain.GetPosition();
            cellSystem.SetupCells(terrain.terrainData.detailPatchCount, terrain.terrainData.detailPatchCount, worldBounds);
        }
        
        public void Refresh()
        {
            needsRefresh = true;
        }

        private bool TerrainChangedFlagsIsRelevant(TerrainChangedFlags flags)
        {
            if (flags == TerrainChangedFlags.Heightmap ||
                flags == TerrainChangedFlags.TreeInstances ||
                flags == TerrainChangedFlags.FlushEverythingImmediately ||
                flags == TerrainChangedFlags.RemoveDirtyDetailsImmediately ||
                flags == TerrainChangedFlags.Holes)
                return true;
            else
                return false;
        }

        private bool TerrainChangedFlagsShouldUpdateTrees(TerrainChangedFlags flags)
        {
            if (flags == TerrainChangedFlags.Heightmap ||
                flags == TerrainChangedFlags.TreeInstances ||
                flags == TerrainChangedFlags.FlushEverythingImmediately ||
                flags == TerrainChangedFlags.Holes)
                return true;
            else
                return false;
        }

        private bool TerrainChangedFlagsShouldUpdateDetails(TerrainChangedFlags flags)
        {
            if (flags == TerrainChangedFlags.Heightmap ||
                flags == TerrainChangedFlags.FlushEverythingImmediately ||
                flags == TerrainChangedFlags.RemoveDirtyDetailsImmediately ||
                flags == TerrainChangedFlags.Holes)
                return true;
            else
                return false;
        }

        private void OnEnable()
        {
            if(terrain == null)
                terrain = GetComponent<Terrain>();
            terrain.drawTreesAndFoliage = false;
            needsRefresh = true;
             
            var worldBounds = terrain.terrainData.bounds;
            worldBounds.center += terrain.GetPosition();
            if (cellSystem == null)
            {
                cellSystem = new CellSystem((GetInstanceID()).ToString() + "_detail");
            }
            
            cellSystem.SetupCells(terrain.terrainData.detailPatchCount, terrain.terrainData.detailPatchCount, worldBounds);
        }

        private Camera ActiveIndirectCamera
        { 
            get
            {
                if (IndirectRenderer.hasInstance == false)
                    return null;
                else
                    return IndirectRenderer.instance.drawCamera;
            }
        }
        

        private void OnDisable()
        {
            if (this == null) // fucking unity null operator - setting .enabled can trigger this when we're already deleted
                return;
            if (cellSystem != null)
            {
                cellSystem.Dispose();
                cellSystem = null;
            }
            terrain.drawTreesAndFoliage = true;
            
            if (IndirectRenderer.hasInstance)
                IndirectRenderer.instance.Unregister(GetInstanceID().ToString());

            if (albedoTexture != null)
            {
                albedoTexture.Release();
            }
        }

        [ExecuteInEditMode]

        private void Update()
        {
            if (terrain.terrainData == null)
                return;
            Camera cam = ActiveIndirectCamera;
            if (cam != null)
                LoadUnloadCells(cam.transform.position);
        }


        [ExecuteInEditMode]
        private void LateUpdate()
        {
            if (terrain.terrainData == null)
                return;

            Camera cam = ActiveIndirectCamera;
            if (needsRefresh)
            {
                Unregister();                
                if(cam != null)
                    ReadFromTerrain(cam.transform.position);
                if (albedoTexture != null)
                {
                    albedoTexture.Release();
                    albedoTexture = null;
                }               
                needsRefresh = false;
            }            
        }

        private void OnTerrainChanged(TerrainChangedFlags flags)
        {
            if(TerrainChangedFlagsShouldUpdateTrees(flags))
                Refresh();
        }

        [BurstCompile]
        struct ExtractDrawMatrixList : IJobParallelFor
        {
            [ReadOnly] public int index;
            [ReadOnly] public float3 terrainPosition;
            [ReadOnly] public float3 terrainSize;
            [ReadOnly] public NativeArray<TreeInstance> treeInstances;
            [ReadOnly] public float3 prototypeScale;
            [WriteOnly] public NativeList<Matrix4x4>.ParallelWriter output;
           
            public void Execute(int i)
            {
                TreeInstance ti = treeInstances[i];
                
                if (ti.prototypeIndex == index)
                {
                    float3 position = ti.position;
                    position *= terrainSize;
                    position += terrainPosition;
                    float width = ti.widthScale;
                    Quaternion rotation = Quaternion.Euler(0, ti.rotation * Mathf.Rad2Deg, 0);

                    var mtx = Matrix4x4.TRS(position, rotation, new Vector3(width, ti.heightScale, width) * prototypeScale);
                    output.AddNoResize(mtx);
                }
            }
        }

        [BurstCompile]
        struct ExtractDrawMatrixListDetails : IJobParallelFor
        {
            [ReadOnly] public float3 terrainPosition;
            [ReadOnly] public NativeArray<DetailInstanceTransform> instances;
#if UNITY_2022_3_OR_NEWER
            [ReadOnly] public NativeArray<Vector3> normals;
            [ReadOnly] public float align;
#endif
            [WriteOnly] public NativeList<Matrix4x4>.ParallelWriter output;
            
            public void Execute(int i)
            {
                DetailInstanceTransform di = instances[i];

                float3 position = terrainPosition;
                position.x += di.posX;
                position.y += di.posY;
                position.z += di.posZ;

#if UNITY_2022_3_OR_NEWER
                Quaternion rotationFromY = Quaternion.Euler(0f, di.rotationY * Mathf.Rad2Deg, 0f);
                Quaternion combinedRotation;
                if (align > 0)
                {
                    Quaternion rotationFromNormal = Quaternion.FromToRotation(Vector3.up, math.lerp(Vector3.up, normals[i], align));
                    combinedRotation = rotationFromNormal * rotationFromY;
                }
                else
                {
                    combinedRotation = rotationFromY;
                }
#else
                Quaternion combinedRotation = Quaternion.Euler(0, di.rotationY * Mathf.Rad2Deg, 0); 
#endif         
                var mtx = Matrix4x4.TRS(position, combinedRotation, new Vector3(di.scaleXZ, di.scaleY, di.scaleXZ));
                output.AddNoResize(mtx);
            }
        }

        #region ShaderPropertyIDs
        static int _TerrainHeightC = Shader.PropertyToID("_TerrainHeightC");
        static int _TerrainHeightProvided = Shader.PropertyToID("_TerrainHeightProvided");
        static int _TerrainSize = Shader.PropertyToID("_TerrainSize");
        static int _TerrainPosition = Shader.PropertyToID("_TerrainPosition");
        static int _TerrainHeightUL = Shader.PropertyToID("_TerrainHeightUL");
        static int _TerrainHeightUR = Shader.PropertyToID("_TerrainHeightUR");
        static int _TerrainHeightU = Shader.PropertyToID("_TerrainHeightU");
        static int _TerrainHeightL = Shader.PropertyToID("_TerrainHeightL");
        static int _TerrainHeightR = Shader.PropertyToID("_TerrainHeightR");
        static int _TerrainHeightBL = Shader.PropertyToID("_TerrainHeightBL");
        static int _TerrainHeightB = Shader.PropertyToID("_TerrainHeightB");
        static int _TerrainHeightBR = Shader.PropertyToID("_TerrainHeightBR");
        static int _TerrainNormalProvided = Shader.PropertyToID("_TerrainNormalProvided");
        static int _TerrainNormalC = Shader.PropertyToID("_TerrainNormalC");
        static int _TerrainNormalUL = Shader.PropertyToID("_TerrainNormalUL");
        static int _TerrainNormalUR = Shader.PropertyToID("_TerrainNormalUR");
        static int _TerrainNormalU = Shader.PropertyToID("_TerrainNormalU");
        static int _TerrainNormalL = Shader.PropertyToID("_TerrainNormalL");
        static int _TerrainNormalR = Shader.PropertyToID("_TerrainNormalR");
        static int _TerrainNormalBL = Shader.PropertyToID("_TerrainNormalBL");
        static int _TerrainNormalB = Shader.PropertyToID("_TerrainNormalB");
        static int _TerrainNormalBR = Shader.PropertyToID("_TerrainNormalBR");

        static int _TerrainAlbedoProvided = Shader.PropertyToID("_TerrainAlbedoProvided");
        static int _TerrainAlbedoC = Shader.PropertyToID("_TerrainAlbedoC");
        static int _TerrainAlbedoUL = Shader.PropertyToID("_TerrainAlbedoUL");
        static int _TerrainAlbedoUR = Shader.PropertyToID("_TerrainAlbedoUR");
        static int _TerrainAlbedoU = Shader.PropertyToID("_TerrainAlbedoU");
        static int _TerrainAlbedoL = Shader.PropertyToID("_TerrainAlbedoL");
        static int _TerrainAlbedoR = Shader.PropertyToID("_TerrainAlbedoR");
        static int _TerrainAlbedoBL = Shader.PropertyToID("_TerrainAlbedoBL");
        static int _TerrainAlbedoB = Shader.PropertyToID("_TerrainAlbedoB");
        static int _TerrainAlbedoBR = Shader.PropertyToID("_TerrainAlbedoBR");
        #endregion


        private void SetupMPBForTerrain(MaterialPropertyBlock mpb, Terrain terrain)
        {
            var materialOptions = TerrainFoliageRenderer.instance.materialOptions;
            if ((materialOptions.extraTextureOptions == null || materialOptions.extraTextureOptions.Count == 0) &&
                materialOptions.supplyHeightmaps == false && materialOptions.supplyHeightmaps == false)
            {
                return;
            }
            Profiler.BeginSample("Setup MPB for Terrains");

            bool diagonalUpLeft = false;
            bool diagonalUpRight = false;
            bool diagonalDownLeft = false;
            bool diagonalDownRight = false;

            bool doHeight = materialOptions.supplyHeightmaps;
            bool doAlbedo = materialOptions.albedoMapGeneration != TerrainFoliageRenderer.MaterialOptions.BaseMapRes.Off;
            int res = (int)materialOptions.albedoMapGeneration;
            bool doNormal = materialOptions.supplyNormalmaps;

            mpb.SetVector(_TerrainSize, terrain.terrainData.size);
            mpb.SetVector(_TerrainPosition, terrain.transform.position);

            //Center
            if (doHeight)
            {
                mpb.SetFloat(_TerrainHeightProvided, 1);
                mpb.SetTexture(_TerrainHeightC, terrain.terrainData.heightmapTexture);
            }
            if (doAlbedo)
            {
                mpb.SetFloat(_TerrainAlbedoProvided, 1);
                mpb.SetTexture(_TerrainAlbedoC, GetAlbedoTexture((int)materialOptions.albedoMapGeneration));
            }
            if (doNormal)
            {
                mpb.SetFloat(_TerrainNormalProvided, 1);
                mpb.SetTexture(_TerrainNormalC, terrain.normalmapTexture);
            }

            //Left 
            if (terrain.leftNeighbor != null)
            {
                if (doHeight) mpb.SetTexture(_TerrainHeightL, terrain.leftNeighbor.terrainData.heightmapTexture);
                if (doAlbedo)
                {
                    TerrainFoliageProvider leftFP = terrain.leftNeighbor.GetComponent<TerrainFoliageProvider>();
                    if (leftFP)
                        mpb.SetTexture(_TerrainAlbedoL, leftFP.GetAlbedoTexture(res));
                }
                if (doNormal) mpb.SetTexture(_TerrainNormalL, terrain.leftNeighbor.normalmapTexture);

                if (terrain.leftNeighbor.topNeighbor != null)
                {
                    diagonalUpLeft = true;

                    if (doHeight) mpb.SetTexture(_TerrainHeightUL, terrain.leftNeighbor.topNeighbor.terrainData.heightmapTexture);
                    if (doNormal) mpb.SetTexture(_TerrainNormalUL, terrain.leftNeighbor.topNeighbor.normalmapTexture);
                    if (doAlbedo)
                    {
                        TerrainFoliageProvider leftTopFP = terrain.leftNeighbor.topNeighbor.GetComponent<TerrainFoliageProvider>();
                        if (leftTopFP)
                            mpb.SetTexture(_TerrainAlbedoUL, leftTopFP.GetAlbedoTexture(res));
                    }
                }

                if (terrain.leftNeighbor.bottomNeighbor != null)
                {
                    diagonalDownLeft = true;

                    if (doHeight) mpb.SetTexture(_TerrainHeightBL, terrain.leftNeighbor.bottomNeighbor.terrainData.heightmapTexture);
                    if (doNormal) mpb.SetTexture(_TerrainNormalBL, terrain.leftNeighbor.bottomNeighbor.normalmapTexture);
                    if (doAlbedo)
                    {
                        TerrainFoliageProvider leftBottomFP = terrain.leftNeighbor.bottomNeighbor.GetComponent<TerrainFoliageProvider>();
                        if (leftBottomFP)
                            mpb.SetTexture(_TerrainAlbedoBL, leftBottomFP.GetAlbedoTexture(res));
                    }
                }
            }

            //Right
            if (terrain.rightNeighbor != null)
            {
                if (doHeight) mpb.SetTexture(_TerrainHeightR, terrain.rightNeighbor.terrainData.heightmapTexture);
                if (doNormal) mpb.SetTexture(_TerrainNormalR, terrain.rightNeighbor.normalmapTexture);
                if (doAlbedo)
                {
                    TerrainFoliageProvider rightFP = terrain.rightNeighbor.GetComponent<TerrainFoliageProvider>();
                    if (rightFP)
                        mpb.SetTexture(_TerrainAlbedoR, rightFP.GetAlbedoTexture(res));
                }

                if (terrain.rightNeighbor.topNeighbor != null)
                {
                    diagonalUpRight = true;
                    if (doHeight) mpb.SetTexture(_TerrainHeightUR, terrain.rightNeighbor.topNeighbor.terrainData.heightmapTexture);
                    if (doNormal) mpb.SetTexture(_TerrainNormalUR, terrain.rightNeighbor.topNeighbor.normalmapTexture);
                    if (doAlbedo)
                    {
                        TerrainFoliageProvider rightTopFP = terrain.rightNeighbor.topNeighbor.GetComponent<TerrainFoliageProvider>();
                        if (rightTopFP)
                            mpb.SetTexture(_TerrainAlbedoUR, rightTopFP.GetAlbedoTexture(res));
                    }
                }

                if (terrain.rightNeighbor.bottomNeighbor != null)
                {
                    diagonalDownRight = true;
                    if (doHeight) mpb.SetTexture(_TerrainHeightBR, terrain.rightNeighbor.bottomNeighbor.terrainData.heightmapTexture);
                    if (doNormal) mpb.SetTexture(_TerrainNormalBR, terrain.rightNeighbor.bottomNeighbor.normalmapTexture);
                    if (doAlbedo)
                    {
                        TerrainFoliageProvider rightBottomFP = terrain.rightNeighbor.bottomNeighbor.GetComponent<TerrainFoliageProvider>();
                        if (rightBottomFP)
                            mpb.SetTexture(_TerrainAlbedoBR, rightBottomFP.GetAlbedoTexture(res));
                    }
                }
            }

            //Top
            if (terrain.topNeighbor != null)
            {
                if (doHeight) mpb.SetTexture(_TerrainHeightU, terrain.topNeighbor.terrainData.heightmapTexture);
                if (doNormal) mpb.SetTexture(_TerrainNormalU, terrain.topNeighbor.normalmapTexture);
                if (doAlbedo)
                {
                    TerrainFoliageProvider topFP = terrain.topNeighbor.GetComponent<TerrainFoliageProvider>();
                    if (topFP)
                        mpb.SetTexture(_TerrainAlbedoU, topFP.GetAlbedoTexture(res));
                }
                //make sure we haven't already set this
                if (diagonalUpLeft == false && terrain.topNeighbor.leftNeighbor != null)
                {
                    if (doHeight) mpb.SetTexture(_TerrainHeightUL, terrain.topNeighbor.leftNeighbor.terrainData.heightmapTexture);
                    if (doNormal) mpb.SetTexture(_TerrainNormalUL, terrain.topNeighbor.leftNeighbor.normalmapTexture);
                    if (doAlbedo)
                    {
                        TerrainFoliageProvider leftTopFP = terrain.topNeighbor.leftNeighbor.GetComponent<TerrainFoliageProvider>();
                        if (leftTopFP)
                            mpb.SetTexture(_TerrainAlbedoUL, leftTopFP.GetAlbedoTexture(res));
                    }
                }
                //make sure we haven't already set this
                if (diagonalUpRight == false && terrain.topNeighbor.rightNeighbor != null)
                {
                    if (doHeight) mpb.SetTexture(_TerrainHeightUR, terrain.topNeighbor.rightNeighbor.terrainData.heightmapTexture);
                    if (doNormal) mpb.SetTexture(_TerrainNormalUR, terrain.topNeighbor.rightNeighbor.normalmapTexture);
                    if (doAlbedo)
                    {
                        TerrainFoliageProvider rightTopFP = terrain.topNeighbor.rightNeighbor.GetComponent<TerrainFoliageProvider>();
                        if (rightTopFP)
                            mpb.SetTexture(_TerrainAlbedoUR, rightTopFP.GetAlbedoTexture(res));
                    }
                }
            }

            //Bottom
            if (terrain.bottomNeighbor != null)
            {
                if (doHeight) mpb.SetTexture(_TerrainHeightB, terrain.bottomNeighbor.terrainData.heightmapTexture);
                if (doNormal) mpb.SetTexture(_TerrainNormalB, terrain.bottomNeighbor.normalmapTexture);
                if (doAlbedo)
                {
                    TerrainFoliageProvider bottom = terrain.bottomNeighbor.GetComponent<TerrainFoliageProvider>();
                    if (bottom)
                        mpb.SetTexture(_TerrainAlbedoB, bottom.GetAlbedoTexture(res));
                }
                //make sure we don't already have this
                if (diagonalDownLeft == false && terrain.bottomNeighbor.leftNeighbor != null)
                {
                    if (doHeight) mpb.SetTexture(_TerrainHeightBL, terrain.bottomNeighbor.leftNeighbor.terrainData.heightmapTexture);
                    if (doNormal) mpb.SetTexture(_TerrainNormalBL, terrain.bottomNeighbor.leftNeighbor.normalmapTexture);
                    if (doAlbedo)
                    {
                        TerrainFoliageProvider leftBottomFP = terrain.bottomNeighbor.leftNeighbor.GetComponent<TerrainFoliageProvider>();
                        if (leftBottomFP)
                            mpb.SetTexture(_TerrainAlbedoBL, leftBottomFP.GetAlbedoTexture(res));
                    }
                }
                //make sure we don't already have this
                if (diagonalDownRight == false && terrain.bottomNeighbor.rightNeighbor != null)
                {
                    diagonalDownRight = true;
                    if (doHeight) mpb.SetTexture(_TerrainHeightBR, terrain.bottomNeighbor.rightNeighbor.terrainData.heightmapTexture);
                    if (doNormal) mpb.SetTexture(_TerrainNormalBR, terrain.bottomNeighbor.rightNeighbor.normalmapTexture);
                    if (doAlbedo)
                    {
                        TerrainFoliageProvider rightBottomFP = terrain.bottomNeighbor.rightNeighbor.GetComponent<TerrainFoliageProvider>();
                        if (rightBottomFP)
                            mpb.SetTexture(_TerrainAlbedoBR, rightBottomFP.GetAlbedoTexture(res));
                    }
                }
            }

            if (materialOptions.extraTextureOptions != null)
            {
                foreach (var eto in materialOptions.extraTextureOptions)
                {
                    if (eto.config == null)
                        continue;

                    mpb.SetFloat(eto.config.activeProperty, 1);

                    var tex = eto.FindExtraTexture(terrain);
                    if (tex != null)
                        mpb.SetTexture(eto.config.propertyC, tex);

                    if (terrain.leftNeighbor != null)
                    {
                        materialOptions.SetNeighborProperty(terrain.leftNeighbor, eto.config, mpb, eto.config.propertyC);
                        if (terrain.leftNeighbor.topNeighbor != null)
                        {
                            materialOptions.SetNeighborProperty(terrain.leftNeighbor.topNeighbor, eto.config, mpb, eto.config.propertyUL);
                        }
                        if (terrain.leftNeighbor.bottomNeighbor != null)
                        {
                            materialOptions.SetNeighborProperty(terrain.leftNeighbor.bottomNeighbor, eto.config, mpb, eto.config.propertyBL);
                        }
                    }
                    if (terrain.rightNeighbor != null)
                    {
                        materialOptions.SetNeighborProperty(terrain.rightNeighbor, eto.config, mpb, eto.config.propertyR);
                        if (terrain.rightNeighbor.topNeighbor != null)
                        {
                            materialOptions.SetNeighborProperty(terrain.rightNeighbor.topNeighbor, eto.config, mpb, eto.config.propertyUR);
                        }
                        if (terrain.rightNeighbor.bottomNeighbor != null)
                        {
                            materialOptions.SetNeighborProperty(terrain.rightNeighbor.bottomNeighbor, eto.config, mpb, eto.config.propertyBR);
                        }
                    }
                    if (terrain.topNeighbor != null)
                    {
                        materialOptions.SetNeighborProperty(terrain.topNeighbor, eto.config, mpb, eto.config.propertyU);
                        if (diagonalUpLeft == false && terrain.topNeighbor.leftNeighbor != null)
                        {
                            materialOptions.SetNeighborProperty(terrain.topNeighbor.leftNeighbor, eto.config, mpb, eto.config.propertyUL);
                        }
                        if (diagonalUpRight == false && terrain.topNeighbor.rightNeighbor != null)
                        {
                            materialOptions.SetNeighborProperty(terrain.topNeighbor.rightNeighbor, eto.config, mpb, eto.config.propertyUR);
                        }
                    }
                    if (terrain.bottomNeighbor != null)
                    {
                        materialOptions.SetNeighborProperty(terrain.bottomNeighbor, eto.config, mpb, eto.config.propertyB);
                        if (diagonalDownLeft == false && terrain.bottomNeighbor.leftNeighbor != null)
                        {
                            materialOptions.SetNeighborProperty(terrain.bottomNeighbor.leftNeighbor, eto.config, mpb, eto.config.propertyBL);
                        }
                        if (diagonalDownRight == false && terrain.bottomNeighbor.rightNeighbor != null)
                        {
                            materialOptions.SetNeighborProperty(terrain.bottomNeighbor.rightNeighbor, eto.config, mpb, eto.config.propertyBR);
                        }
                    }
                }
            }
            Profiler.EndSample();
        }

        

        class DetailJobHolder
        {
            public NativeArray<DetailInstanceTransform> array;
            public NativeArray<Vector3> normals;
            public JobHandle handle;
            public GameObject proto;
            public DrawOptions drawOption;
            public RenderParams renderParams;
            public Bounds bounds;
            public CellSystem.CellLoader loader;
            public NativeList<Matrix4x4> mtxs;
        }

        class LayerJobHolder
        {
            public List<DetailJobHolder> jobHolders = new List<DetailJobHolder>(8);
            public CellSystem.CellLoader loader;
        }

        List<LayerJobHolder> detailJobs = new List<LayerJobHolder>(64);
        List<Draw> tempDetailDraws = new List<Draw>(1);
        int loaded = 0;
        int loading = 0;
        int unloading = 0;

        private void LoadUnloadCells(float3 cameraWorldPosition)
        {
            if (IndirectRenderer.CheckFrustum &&
                !IndirectRenderer.instance.TerrainInView(terrain))
                return;

            float maxDistance = TerrainFoliageRenderer.instance.FindMaxDetailDrawDistance(terrain);
            float unloadDistance =
                math.max(
                    maxDistance,
                    maxDistance + TerrainFoliageRenderer.instance.detailCachingDistance);

            if (IndirectRenderer.CheckDistance &&
                !IndirectRenderer.instance.TerrainInDrawDistance(
                terrain,
                maxPrototypeDistance: maxDistance,
                unloadDistance: unloadDistance,
                loaded))
                return;

            DrawOptions defaultDetailOptions = TerrainFoliageRenderer.instance.detailOptions;

            loaded = 0;
            loading = 0;
            unloading = 0;

            Profiler.BeginSample("Detail Cell Loading");

            Bounds worldTerrainBounds = terrain.terrainData.bounds;
            worldTerrainBounds.center += terrain.transform.position;
            worldTerrainBounds.center -= worldTerrainBounds.extents;
            int pc = terrain.terrainData.detailPatchCount;

            Profiler.BeginSample("Compute Visibility");
            
            bool visibilityChanged = cellSystem.ComputeCellVisibility(cameraWorldPosition, maxDistance, unloadDistance);
            Profiler.EndSample();
            var cachingMode = TerrainFoliageRenderer.instance.detailCachingMode;

            RenderParams rp = new RenderParams();
            rp.layer = terrain.gameObject.layer;

            detailJobs.Clear();
            MaterialPropertyBlock mpb = null;
            if (cellSystem.cells.IsCreated)
            {
                for (int i = 0; i < cellSystem.cells.Length / 2; ++i)
                {
                    var cellVisible = cellSystem.IsPotentiallyVisible(i);
                    var cellShouldUnload = cellSystem.NeedUnload(i);
                    var loader = cellSystem.cellLoaders[i];

                    if (cachingMode == TerrainFoliageRenderer.DetailCachingMode.Streaming && cellShouldUnload == true && loader.loaded == true)
                    {
                        loader.Dispose();
                        unloading++;
                    }
                    else if ((cachingMode == TerrainFoliageRenderer.DetailCachingMode.AtLoad || cellVisible) && loader.loaded == false)
                    {
                        loader.loaded = true;
                        loading++;
                        var layerHolder = new LayerJobHolder() {  loader = loader };
                        detailJobs.Add(layerHolder);
                        var detailPrototypes = terrain.terrainData.detailPrototypes;
                        for (int layer = 0; layer < detailPrototypes.Length; ++layer)
                        {
                            DetailPrototype proto = detailPrototypes[layer];
                            DrawOptions detailO = TerrainFoliageRenderer.instance.FindDetailDrawOption(proto);
                            Bounds bounds;

                            Profiler.BeginSample("Read Detail Instances");
                            int x = (int)(i % pc);
                            int y = (int)(i / pc);

                            DetailInstanceTransform[] details = terrain.terrainData.ComputeDetailInstanceTransforms(
                                x, 
                                y, 
                                layer,
                                math.min(detailO.density, terrain.detailObjectDensity), 
                                out bounds);
                            Profiler.EndSample();
                            bounds.center += terrain.transform.position;
                            if (details != null && details.Length > 0)
                            {
                                detailO.maxDrawDistance = detailO.GetMaxDrawDistance(defaultDetailOptions, terrain);
                                if (detailO.shadowDistanceMode == DrawOptions.DistanceMode.UseQualitySettings)
                                {
                                    detailO.shadowDistance = math.min(terrain.detailObjectDistance, detailO.maxDrawDistance);
                                }
                                
                                detailO.useWorldYClip = true;
                                detailO.worldYClipHeight = terrain.transform.position.y;

                                NativeList<Matrix4x4> mtxs = new NativeList<Matrix4x4>(details.Length, Allocator.Persistent);

                                var detailArray = new NativeArray<DetailInstanceTransform>(details, Allocator.TempJob);

                                var job = new ExtractDrawMatrixListDetails()
                                {
                                    instances = detailArray,
                                    output = mtxs.AsParallelWriter(),
                                    terrainPosition = terrain.transform.position
                                };

                                var djh = new DetailJobHolder()
                                {
                                    array = detailArray,
                                    proto = proto.prototype,
                                    drawOption = detailO,
                                    renderParams = rp,
                                    bounds = bounds,
                                    loader = loader,
                                    mtxs = mtxs,
                                };

                                // in 2022, unity added the ability to align details to normals, but didn't bother to update
                                // the function that computes the matrix's for details, returning unaligned versions, effectively.
                                // And of course there is exactly 0 chance they fix this.
#if UNITY_2022_3_OR_NEWER                              
                                NativeArray<Vector3> normals = new NativeArray<Vector3>(details.Length, Allocator.TempJob);
                                if (proto.alignToGround > 0)
                                {
                                    var terrainSize = terrain.terrainData.size;
                                    float tSizeX = 1 / terrainSize.x;
                                    float tSizeY = 1 / terrainSize.z;
                                    for (int j = 0; j < details.Length; ++j)
                                    {
                                        var detail = details[j];
                                        normals[j] = terrain.terrainData.GetInterpolatedNormal(detail.posX * tSizeX,  detail.posZ * tSizeY);
                                    }
                                }
                                job.normals = normals;
                                job.align = proto.alignToGround;
                                djh.normals = normals;
#endif                          
                                var handle = job.Schedule(details.Length, 1024);
                                djh.handle = handle;
                                
                                if (mpb == null)
                                {
                                    mpb = new MaterialPropertyBlock();
                                    SetupMPBForTerrain(mpb, terrain);
                                }
                                rp.matProps = mpb;
                                layerHolder.jobHolders.Add(djh);
                            }
                        }
                    }
                    if (loader.loaded) loaded++;
                }
            }
            foreach (var lh in detailJobs)
            {
                foreach (var djh in lh.jobHolders)
                {
                    djh.handle.Complete();
                    djh.array.Dispose();
                    if (djh.normals.IsCreated) djh.normals.Dispose();
                    Draw.AddToDrawList(djh.proto, tempDetailDraws, djh.mtxs, rp, djh.bounds, djh.drawOption, Matrix4x4.identity, true);
                    djh.loader.entries.Add(tempDetailDraws[0]);
                    cellSystem.paramSets[djh.proto] = new CellSystem.ParamSet()
                    {
                        //bounds = tempDetailDraws[0].bounds,
                        renderParams = tempDetailDraws[0].defaultParams,
                        drawOptions = tempDetailDraws[0].drawOptions
                    };
                    tempDetailDraws.Clear();
                }
                lh.loader.loaded = true;
            }
            
            
            detailJobs.Clear();
            // only run this when we need to. I thought about frustum culling in this step,
            // since it would reduce the number of cells and thus the number of instances
            // we send to the GPU, but the registration times of sending it to the indirect
            // renderer make it not worth it, just cull on the GPU instead.
            if (loading > 0 || unloading > 0 || visibilityChanged)
            {
                cellSystem.CombineVisibleAndDraw(cameraWorldPosition, maxDistance);
            }
            //Debug.Log("Visible : " + visible + " loaded: " + loaded + " loading: " + loading + " unloading:" + unloading);
            Profiler.EndSample();
        }

        private void ReadFromTerrain(Vector3 cameraWorldPosition)
        {
            float maxDistance = TerrainFoliageRenderer.instance.FindMaxTreeDrawDistance(terrain);
            if (IndirectRenderer.CheckFrustum &&
                !IndirectRenderer.instance.TerrainInView(terrain))
                return;
            if (IndirectRenderer.CheckDistance &&
                !IndirectRenderer.instance.TerrainInDrawDistance(
                terrain,
                maxPrototypeDistance: maxDistance,
                unloadDistance: maxDistance,
                loaded))
                return;

#if _HDRP
            Profiler.BeginSample("HDRP Find Shadow Volume");
            UnityEngine.Rendering.HighDefinition.HDShadowSettings settings = null;
            var volumes = FindObjectsOfType<UnityEngine.Rendering.Volume>();
            foreach (var v in volumes)
            {
                if (v.isGlobal && v.isActiveAndEnabled)
                {
                    if (!v.profile.TryGet<UnityEngine.Rendering.HighDefinition.HDShadowSettings>(out settings))
                    {
                        settings = null;
                    }
                    break;
                }
            }
            Profiler.EndSample();
#endif
            if (terrain.terrainData == null)
            {
                Debug.LogError("Terrain " + terrain + " has no terrain data");
                return;
            }

            Profiler.BeginSample("Read From Terrain");
            treeDrawList.Clear();
            var prototypes = terrain.terrainData.treePrototypes;
            Profiler.BeginSample("Read Trees");
            var treeInstances = new NativeArray<TreeInstance>(terrain.terrainData.treeInstances, Allocator.TempJob);
            Profiler.EndSample();

            RenderParams rp = new RenderParams();
            rp.layer = terrain.gameObject.layer;
            List<NativeList<Matrix4x4>> clearMtxs = new List<Unity.Collections.NativeList<Matrix4x4>>();
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            SetupMPBForTerrain(mpb, terrain);
            rp.matProps = mpb;
            Profiler.BeginSample("Process trees");
            for (int i = 0; i < prototypes.Length; ++i)
            {
                TreePrototype p = prototypes[i];
                
                NativeList<Matrix4x4> mtxs = new NativeList<Matrix4x4>(treeInstances.Length, Allocator.TempJob);
                var job = new ExtractDrawMatrixList()
                {
                    index = i,
                    terrainPosition = terrain.transform.position,
                    terrainSize = terrain.terrainData.size,
                    treeInstances = treeInstances,
                    prototypeScale = p.prefab.transform.localScale,
                    output = mtxs.AsParallelWriter(),
                };
                job.Schedule(treeInstances.Length, 256).Complete();

                var center = terrain.transform.position;
                center.x += terrain.terrainData.size.x / 2;
                center.z += terrain.terrainData.size.z / 2;
                Bounds b = new (center, terrain.terrainData.size);

                var tOptions = TerrainFoliageRenderer.instance.FindTreeDrawOption(p);
                
                tOptions.useWorldYClip = true;
                tOptions.worldYClipHeight = terrain.transform.position.y;
                if (tOptions.distanceMode == DrawOptions.DistanceMode.UseQualitySettings)
                {
                    tOptions.maxDrawDistance = terrain.treeDistance;
                }
                if (tOptions.shadowDistanceMode == DrawOptions.DistanceMode.UseQualitySettings)
                {
                    tOptions.shadowDistance = QualitySettings.shadowDistance;
#if _URP
                    var urpAsset = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
                    if (urpAsset != null)
                    {
                        tOptions.shadowDistance = urpAsset.shadowDistance;
                    }
#elif _HDRP
                    if (settings != null)
                    {
                        tOptions.shadowDistance = settings.maxShadowDistance.value;
                    }
                    else
                    {
                        tOptions.shadowDistance = 500; // no way to get the default that I can find. WTF, HDRP. 
                    }
#endif
                }
                if (p.prefab != null)
                {
                    Draw.AddToDrawList(p.prefab, treeDrawList, mtxs, rp, b, tOptions, Matrix4x4.identity);
                    clearMtxs.Add(mtxs);
                }
            }

            TerrainFoliageRenderer.EnsureExists();
            IndirectRenderer.instance.Register(GetInstanceID().ToString(), treeDrawList);
            foreach (var t in clearMtxs)
            {
                t.Dispose();
            }
            treeDrawList.Clear();
            Profiler.EndSample();
            treeInstances.Dispose();
        }
    }

}