//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace JBooth.FoliageRendering
{
    public class IndirectTargetDataFoliage
    {
        public static int GetBufferSize()
        {
            return 4; // 24 bit index, 8 bit crossfade
        }

        public static int GetEmitBufferSize()
        {
            return 32;      // compressed, float3, uint3, uint2
        }

        public static int GetPropertyID()
        {
            return Shader.PropertyToID("FRVisibleInstances");
        }

        public static string GetComputeKeyword() { return "_FOLIAGERENDERER"; }
        public static string GetMaterialKeyword() { return "_FOLIAGERENDERER"; }
        public static int GetAllBufferID()
        {
            return Shader.PropertyToID("FRAllMatrices");
        }
    }

    // mesh data
    public class DrawMesh
    {
        private DrawMesh() { }
        public DrawMesh(MeshFilter mf, UnityEngine.Renderer r, Material[] mats, Bounds b, RenderParams rp)
        {
            mesh = mf.sharedMesh;
            materials = mats;
            renderParams = rp;
            renderParams.worldBounds = b;
            renderParams.shadowCastingMode = r.shadowCastingMode;
            renderParams.renderingLayerMask = r.renderingLayerMask;
            renderParams.rendererPriority = r.rendererPriority;
            renderParams.reflectionProbeUsage = r.reflectionProbeUsage;
            renderParams.receiveShadows = r.receiveShadows;
            renderParams.motionVectorMode = r.motionVectorGenerationMode;
            renderParams.lightProbeUsage = r.lightProbeUsage;
            renderParams.layer = r.gameObject.layer;

            if (renderParams.matProps == null)
                renderParams.matProps = new MaterialPropertyBlock();

        }

        public Mesh mesh;
        public Material[] materials;
        public RenderParams renderParams;
    }

    [System.Serializable]
    public class DrawOptions
    {
        public enum MaxLOD
        {
            LOD0,
            LOD1,
            LOD2,
            LOD3
        }

        public DrawOptions()
        {

        }

        public DrawOptions(DrawOptions o)
        {
            maxShadowLOD = o.maxShadowLOD;
            distanceMode = o.distanceMode;
            _maxDrawDistance = o.maxDrawDistance;
            shadowDistanceMode = o.shadowDistanceMode;
            shadowDistance = o.shadowDistance;
            boundsExpand = o.boundsExpand;
            useWorldYClip = o.useWorldYClip;
            worldYClipHeight = o.worldYClipHeight;
            density = o.density;
        }

        public enum DistanceMode
        {
            UseQualitySettings,
            Manual
        }

        [Tooltip("You can override the maximum drawing distance")]
        public DistanceMode distanceMode = DistanceMode.UseQualitySettings;

        [Tooltip("Maximum Draw Distance for objects")]
        [FormerlySerializedAs("maxDrawDistance")]
        [SerializeField]
        private float _maxDrawDistance = 2000;

        [HideInInspector]
        [System.NonSerialized]
        //non serialized draw distance we read/write as needed while calculating detail jobs
        public float maxDrawDistance;

        public float GetMaxDrawDistance(DrawOptions defaultOptions, Terrain t)
        {
            if (distanceMode == DistanceMode.Manual)
                return _maxDrawDistance;
            else if (defaultOptions != null)
                return defaultOptions.GetMaxDrawDistance(null, t);
            else if (t != null)
                return t.detailObjectDistance;
            else
                return 250.0f;
        }

        [Tooltip("You can override the maximum shadowing distance")]
        public DistanceMode shadowDistanceMode = DistanceMode.UseQualitySettings;
        public float shadowDistance = 256;

        [Tooltip("Set the maximum LOD level you allow for shadows")]
        public MaxLOD maxShadowLOD = MaxLOD.LOD0;

        [Tooltip("When using vertex shader offsets, you might need to expand the bounds of the object slighly in case the vertex offset causes it to move beyond the bounds")]
        public Vector3 boundsExpand = Vector3.zero;

        [Range(0, 1)]
        [Tooltip("Set the max Detail Density")]
        public float density = 1.0f;

        // for terrain
        [System.NonSerialized] public bool useWorldYClip = false;
        [System.NonSerialized] public float worldYClipHeight = 0;
    }

    // for things that override draw options
    [System.Serializable]
    public class OverrideOptions
    {
        public bool overrideDrawOptions;
        public DrawOptions drawOptions;
    }

    // draw abstraction
    public class Draw : IDisposable
    {
        public GameObject prefab;
        public RenderParams defaultParams;
        public List<DrawMesh> meshLOD0;
        public List<DrawMesh> meshLOD1;
        public List<DrawMesh> meshLOD2;
        public List<DrawMesh> meshLOD3;
        public Vector4 lodScreenHeights;
        public Vector4 lodTransitionWidths;
        public bool useCrossFade;
        public NativeList<Matrix4x4> mtxs;
        public GraphicsBuffer mtxBuffer;
        public Bounds bounds;
        public DrawOptions drawOptions = new DrawOptions();
        public Matrix4x4 localToWorld;

        public void Dispose()
        {
            if (mtxs.IsCreated) mtxs.Dispose();
            if (mtxBuffer != null) mtxBuffer.Dispose();
            mtxBuffer = null;
            drawPool.ReturnObject(this);
        }

        public class MeshCache
        {
            public List<RenderData> lod0 = new List<RenderData>();
            public List<RenderData> lod1 = new List<RenderData>();
            public List<RenderData> lod2 = new List<RenderData>();
            public List<RenderData> lod3 = new List<RenderData>();
            public Vector4 lodTransitionWidths;
            public Vector4 lodScreenHeights;
            public bool useCrossFade;
            public Vector3 prefabScale;

            public class RenderData
            {
                public MeshFilter filter;
                public MeshRenderer renderer;
                public Material[] materials;
            }

            static void ExtractRenderData(Renderer[] rends, List<RenderData> rds)
            {
                foreach (var r in rends)
                {
                    if (r != null)
                    {
                        var mf = r.GetComponent<MeshFilter>();
                        var mr = r as MeshRenderer;
                        if (mf == null || mf.sharedMesh == null || r == null)
                            continue;
                        RenderData rd = new RenderData();
                        rd.filter = mf;
                        rd.renderer = mr;
                        rd.materials = mr.sharedMaterials;
                        rds.Add(rd);
                    }
                }
            }

            static Dictionary<GameObject, MeshCache> cache = new Dictionary<GameObject, MeshCache>();

            public static MeshCache Get(GameObject prefab)
            {
                if (cache.ContainsKey(prefab))
                    return cache[prefab];

                MeshCache mc = new MeshCache();
                cache.Add(prefab, mc);
                var lodGroups = prefab.GetComponentsInChildren<LODGroup>();
                if (lodGroups == null || lodGroups.Length == 0)
                {
                    var meshFilters = prefab.GetComponentsInChildren<MeshFilter>();
                    foreach (var mf in meshFilters)
                    {
                        RenderData rd = new RenderData();
                        rd.filter = mf;
                        MeshRenderer mr = mf.GetComponent<MeshRenderer>();
                        if (mr != null)
                        {
                            rd.renderer = mr;
                            rd.materials = mr.sharedMaterials;
                            mc.lod0.Add(rd);
                        }
                    }
                }
                else
                {
                    foreach (var lod in lodGroups)
                    {
                        if (lod.lodCount > 4)
                        {
                            // TODO: Move to shader patcher warning..
                            Debug.LogWarning(prefab + " has more than 4 lods");
                        }
                        if (lod.lodCount == 0)
                        {
                            Debug.LogError(prefab + " has 0 LODs, will not draw");
                            break;
                        }
                        var lds = lod.GetLODs();
                        mc.lodTransitionWidths = new Vector4(0.05f, 0.05f, 0.05f, 0.05f);
                        mc.useCrossFade = lod.fadeMode == LODFadeMode.CrossFade;

                        // We take the last 4 lods if someone provides too many.
                        int initialIndex = lds.Length - 4;
                        if (initialIndex < 0)
                            initialIndex = 0;
                        {
                            var rends = lds[initialIndex].renderers;

                            if (rends == null || rends.Length == 0)
                            {
                                Debug.LogError(prefab + " has no renderers in lod0");
                                break;
                            }
                            mc.lodScreenHeights.x = lds[initialIndex].screenRelativeTransitionHeight;
                            if (!lod.animateCrossFading)
                                mc.lodTransitionWidths.x = lds[initialIndex].fadeTransitionWidth;
                            ExtractRenderData(rends, mc.lod0);
                        }
                        if (lds.Length > 1)
                        {
                            var rends = lds[initialIndex+1].renderers;

                            if (rends == null || rends.Length == 0)
                            {
                                Debug.LogError(prefab + " has no renderers in lod1");
                                break;
                            }
                            mc.lodScreenHeights.y = lds[initialIndex+1].screenRelativeTransitionHeight;
                            if (!lod.animateCrossFading)
                                mc.lodTransitionWidths.y = lds[initialIndex+1].fadeTransitionWidth;
                            ExtractRenderData(rends, mc.lod1);
                        }
                        if (lds.Length > 2)
                        {
                            var rends = lds[initialIndex+2].renderers;

                            if (rends == null || rends.Length == 0)
                            {
                                Debug.LogError(prefab + " has no renderers in lod2");
                                break;
                            }
                            mc.lodScreenHeights.z = lds[initialIndex+2].screenRelativeTransitionHeight;
                            if (!lod.animateCrossFading)
                                mc.lodTransitionWidths.z = lds[initialIndex+2].fadeTransitionWidth;
                            ExtractRenderData(rends, mc.lod2);
                        }
                        if (lds.Length > 3)
                        {
                            var rends = lds[initialIndex+3].renderers;

                            if (rends == null || rends.Length == 0)
                            {
                                Debug.LogError(prefab + " has no renderers in lod1");
                                break;
                            }
                            mc.lodScreenHeights.w = lds[initialIndex+3].screenRelativeTransitionHeight;
                            if (!lod.animateCrossFading)
                                mc.lodTransitionWidths.w = lds[initialIndex+3].fadeTransitionWidth;
                            ExtractRenderData(rends, mc.lod3);
                        }

                    }
                }

                return mc;

            }

            public static void ClearCache()
            {
                cache.Clear();
            }
        }

        static void SetKeywordsOnMaterial(Material[] materials)
        {
            var keyword = IndirectTargetDataFoliage.GetMaterialKeyword();
            foreach (var mat in materials)
            {
                mat.EnableKeyword(keyword);
            }
        }

        static void ProcessLODs(Draw d, MeshCache mc, Bounds worldBounds, RenderParams renderParams)
        {
            foreach (var rd in mc.lod0)
            {
                d.meshLOD0 = new List<DrawMesh>();
                DrawMesh dm = new DrawMesh(rd.filter, rd.renderer, rd.materials, worldBounds, renderParams);
                SetKeywordsOnMaterial(dm.materials);
                d.meshLOD0.Add(dm);
            }
            foreach (var rd in mc.lod1)
            {
                d.meshLOD1 = new List<DrawMesh>();
                DrawMesh dm = new DrawMesh(rd.filter, rd.renderer, rd.materials, worldBounds, renderParams);
                SetKeywordsOnMaterial(dm.materials);
                d.meshLOD1.Add(dm);
            }
            foreach (var rd in mc.lod2)
            {
                d.meshLOD2 = new List<DrawMesh>();
                DrawMesh dm = new DrawMesh(rd.filter, rd.renderer, rd.materials, worldBounds, renderParams);
                SetKeywordsOnMaterial(dm.materials);
                d.meshLOD2.Add(dm);
            }
            foreach (var rd in mc.lod3)
            {
                d.meshLOD3 = new List<DrawMesh>();
                DrawMesh dm = new DrawMesh(rd.filter, rd.renderer, rd.materials, worldBounds, renderParams);
                SetKeywordsOnMaterial(dm.materials);
                d.meshLOD3.Add(dm);
            }
        }

        public static void AddToDrawList(GameObject prefab, List<Draw> drawList,
           NativeList<Matrix4x4> mtxArray, RenderParams renderParams, Bounds worldBounds,
           DrawOptions drawOptions, Matrix4x4 localToWorld, bool createGraphicsBuffer = false)
        {
            UnityEngine.Profiling.Profiler.BeginSample("Mesh Caching");
            var mc = MeshCache.Get(prefab);
            UnityEngine.Profiling.Profiler.EndSample();

            Draw d = drawPool.GetObject();
            d.defaultParams = renderParams;
            d.prefab = prefab;
            d.localToWorld = localToWorld;
            d.drawOptions = drawOptions;
            d.bounds = worldBounds;
            d.useCrossFade = mc.useCrossFade;
            d.lodTransitionWidths = mc.lodTransitionWidths;
            d.lodScreenHeights = mc.lodScreenHeights;
            d.mtxs = mtxArray;

            if (createGraphicsBuffer)
            {
                d.mtxBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mtxArray.Length, 16 * 4);
                d.mtxBuffer.SetData(mtxArray.AsArray());
            }

            ProcessLODs(d, mc, worldBounds, renderParams);

            drawList.Add(d);
        }

        static ObjectPool<Draw> drawPool = new ObjectPool<Draw>(2048);

        public static void AddToDrawList(GameObject prefab, List<Draw> drawList,
          GraphicsBuffer mtxBuffer, RenderParams renderParams, Bounds worldBounds,
          DrawOptions drawOptions, Matrix4x4 localToWorld)
        {
            UnityEngine.Profiling.Profiler.BeginSample("Mesh Caching");
            var mc = MeshCache.Get(prefab);
            UnityEngine.Profiling.Profiler.EndSample();
            Draw d = drawPool.GetObject();
            d.defaultParams = renderParams;
            d.prefab = prefab;
            d.localToWorld = localToWorld;
            d.drawOptions = drawOptions;
            d.bounds = worldBounds;
            d.useCrossFade = mc.useCrossFade;
            d.lodTransitionWidths = mc.lodTransitionWidths;
            d.lodScreenHeights = mc.lodScreenHeights;
            d.mtxBuffer = mtxBuffer;

            ProcessLODs(d, mc, worldBounds, renderParams);

            drawList.Add(d);
        }


    }


}
