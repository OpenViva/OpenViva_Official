//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Unity.Mathematics;

namespace JBooth.FoliageRendering
{
    [HelpURL("https://dicewrenchdesigns.com/category/assets/")]
    [ExecuteAlways]
    public class IndirectRenderer : MonoBehaviour
    {
        public enum ModeToggle
        {
            Off,
            On
        }
        [HideInInspector]
        public RenderTexture hizBuffer;
        [Tooltip("Should we cull things based on the direction light shadow frustum? Might need to turn off if using multiple shadow casters")]
        public ModeToggle shadowFrustumCulling = ModeToggle.On;
        [Tooltip("Shadow Casting directional light")]
        public Light sun;
        [Tooltip("Hi-Z occlusion can increase framerate when scene has blocking objects")]
        public ModeToggle hiZOcclusion = ModeToggle.Off;
        [Header("Terrain Update Settings")]
        [Tooltip("Should Terrain distance from Camera be checked before providing Foliage Details?  For small scenes, disabling this may improve performance.")]
        public ModeToggle distanceCheckUpdates = ModeToggle.On;
        [Tooltip("Should Terrain see if it is visible before providing Foliage Details?  For small scenes, disabling this may improve performance.")]
        public ModeToggle frustumCheckUpdate = ModeToggle.On;
        
        public static bool CheckDistance 
        { 
            get 
            {
                if (instance == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning("Cannot Check Distance before updating.  No IndirectRenderer Instance present!");
#endif
                    return false;
                }
                return instance.distanceCheckUpdates == ModeToggle.On ? true : false;
            } 
        }

        public static bool CheckFrustum
        {
            get
            {
                if (instance == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning("Cannot Check Frustum before updating.  No IndirectRenderer Instance present!");
#endif
                    return false;
                }
                return instance.frustumCheckUpdate == ModeToggle.On ? true : false;
            }
        }

        [Range(1, 60)]
        [Tooltip("Controls the caching of graphics buffers. Higher values mean less allocations, but more memory use")]
        public int maxGraphicsBufferCaches = 8;

        [Tooltip("Force culling to happen from this camera")]
        [FormerlySerializedAs("debugCullingCamera")]
        public Camera cullingCamera;

        [HideInInspector] public static int drawCalls = 0;
        [HideInInspector] public static int unculledInstanceCount;

        static IndirectRenderer sInstance;
        public static bool hasInstance { get { return sInstance != null; } }
        public static IndirectRenderer instance
        {
            get
            {
                if (sInstance == null)
                {
                    sInstance = FindObjectOfType<IndirectRenderer>();
                    if (sInstance == null)
                    {
                        var go = new GameObject("Indirect Renderer");
                        sInstance = go.AddComponent<IndirectRenderer>();
                    }
                }
                return sInstance;
            }
        }

        private Camera _drawCamera;
        public Camera drawCamera 
        { 
            get
            {
                if (cullingCamera != null)
                {
                    if (_drawCamera != cullingCamera)
                    {
                        _drawCamera = cullingCamera;
                    }
                }
                if (_drawCamera == null && cullingCamera == null && Camera.main != null)
                {
                    if (_drawCamera != Camera.main)
                    {
                        _drawCamera = Camera.main;                        
                    }
                }
                return _drawCamera;
            }
        }

        public static bool doPostCullingCapture;
        
        bool reInitHiZ = false;

        // Holds the data for one LOD worth of draws
        [HideInInspector] public static int postCullingInstanceCount;
        [HideInInspector] public static int postCullingShadowCount;

        private static ObjectPool<DrawMeshIndirectLOD> poolDrawMeshIndirectLOD = new ObjectPool<DrawMeshIndirectLOD>(256);
        public static ObjectPool<DrawMeshIndirectLOD> PoolDrawMeshIndirectLOD { get { return poolDrawMeshIndirectLOD; } }
        static List<GraphicsBuffer.IndirectDrawIndexedArgs[]> indirectArgsPool = new List<GraphicsBuffer.IndirectDrawIndexedArgs[]>();

        static ObjectPool<DrawMeshIndirect> poolDrawMeshIndirect = new ObjectPool<DrawMeshIndirect>(512);


        // ugh, must be static otherwise something overwrites it? WTF?
        static Dictionary<string, List<DrawMeshIndirect>> indirectBatches = new Dictionary<string, List<DrawMeshIndirect>>();

        private static GraphicsBufferPool _graphicsBufferPool;
        public static GraphicsBufferPool GraphicsBufferPool
        {
            get
            {
                if (_graphicsBufferPool == null)
                {
                    int count = 8;
                    if (IndirectRenderer.hasInstance == false)
                        Debug.LogWarning("Attempting to access GraphicsBufferPool without IndirectRenderer Instance!  Pool initializing with count of 8.");
                    else
                        count = IndirectRenderer.instance.maxGraphicsBufferCaches;
                    _graphicsBufferPool = new GraphicsBufferPool(count);
                }
                return _graphicsBufferPool;
            }
        }

        ComputeShader packingShader;
        static readonly int _OutputBuffer = Shader.PropertyToID("_OutputBuffer");
        // convert a draw list into batches
        ObjectListPool<List<DrawMeshIndirect>, DrawMeshIndirect> poolBatches = new ObjectListPool<List<DrawMeshIndirect>, DrawMeshIndirect>();

        private static ComputeShader cullingShader;
        public static ComputeShader CullingShader
        {
            get
            {
                if (cullingShader == null)
                {
                    cullingShader = Resources.Load<ComputeShader>(CULLING_FILENAME);
                    if (cullingShader == null)
                    {
                        Debug.LogError("Cannot find culling shader!  Use the Menu Item 'Foliage Renderer/Validate Culling Shader' or the IndirectRenderer Inspector to fix!", IndirectRenderer.instance);
                        return null;
                    }
                }
                return cullingShader;
            }
        }

        private HiZBuffer hiz;

        private Plane[] _cameraFrustum;


        /// <summary>
        /// Checks if the given <see cref="Terrain"/> is within
        /// the <see cref="Camera"/> Frustum.
        /// </summary>
        /// <param name="terrain"></param>
        /// <returns></returns>
        public bool TerrainInView(Terrain terrain)
        {
            if (drawCamera == null)
                return false;
            _cameraFrustum = GeometryUtility.CalculateFrustumPlanes(drawCamera);
            return GeometryUtility.TestPlanesAABB(_cameraFrustum, terrain.terrainData.bounds);
        }

        /// <summary>
        /// Checks if there are any prototypes (Trees, Details) in
        /// range to Draw.  Use this for early outs.
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="maxPrototypeDistance"></param>
        /// <param name="unloadDistance"></param>
        /// <param name="loadedCount"></param>
        /// <returns></returns>
        public bool TerrainInDrawDistance(Terrain terrain, float maxPrototypeDistance, float unloadDistance, int loadedCount)
        {
            if (unloadDistance < maxPrototypeDistance + 5)
                unloadDistance = maxPrototypeDistance + 5;
            if (loadedCount <= 0)
            {
                // early out check
                var ts = terrain.terrainData.size;
                float distToTerrain = Vector3.Distance(drawCamera.transform.position, terrain.GetPosition() + ts * 0.5f);
                float mx = math.max(ts.x, ts.z);
                if (distToTerrain > unloadDistance + mx * 0.5f)
                {
                    return false;
                }
            }

            return true;
        }


        private void OnDestroy()
        {
            sInstance = null;
        }

        private void Awake()
        {
            CheckInstance();
        }

        private void CheckInstance()
        {
            if (sInstance == null)
                sInstance = this;
            else
            {
#if UNITY_EDITOR
                if (sInstance != this)
                {
                    if (Application.isPlaying)
                        Destroy(gameObject);
                    else
                        DestroyImmediate(gameObject);
                }
#else
                if (sInstance != this)
                    Destroy(gameObject);
#endif
            }
        }

        private void OnEnable()
        {
            CheckInstance();
            if (GraphicsBufferPool != null)
            {
                GraphicsBufferPool.ReleaseUnusedBuffers();
            }
            Draw.MeshCache.ClearCache();
            reInitHiZ = true;

            UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += PreRender;
#if UNITY_EDITOR
            UnityEditor.SceneView.beforeSceneGui += OnScene;
#endif
        }

#if UNITY_EDITOR
        private void OnScene(SceneView view)
        {
            if (FoliageMenuOptions.ForceSceneRefresh)
            {
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();                
                if (UnityEngine.QualitySettings.renderPipeline == null && !Application.isPlaying)
                    _drawCamera = view.camera;
                UnityEditor.SceneView.RepaintAll();
            }
        }
#endif

        private void OnDisable()
        {
            UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= PreRender;
#if UNITY_EDITOR
            UnityEditor.SceneView.beforeSceneGui -= OnScene;
#endif
            foreach (var provider in indirectBatches.Keys)
            {
                List<DrawMeshIndirect> indirect;
                if (indirectBatches.TryGetValue(provider, out indirect))
                {
                    foreach (var l in indirect)
                    {
                        poolDrawMeshIndirect.ReturnObject(l);
                    }
                    poolBatches.ReturnObject(indirect);
                }
            }
            indirectBatches.Clear();
            if (GraphicsBufferPool != null)
            {
                GraphicsBufferPool.ReleaseUnusedBuffers();
                _graphicsBufferPool = null;
            }
        }

        private void PreRender(ScriptableRenderContext context, Camera camera)
        {
            if (Camera.main)
                _drawCamera = Camera.main;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (camera.cameraType == CameraType.SceneView && FoliageMenuOptions.ForceSceneRefresh)
                    _drawCamera = camera;
            }
            else
            {
                if (camera == Camera.main)
                    _drawCamera = camera;
                else if (camera.cameraType == CameraType.SceneView)
                    _drawCamera = Camera.main;
            }
#endif
        }

        private void LateUpdate()
        {
            Execute();
        }

        public void Execute()
        {
            if (maxGraphicsBufferCaches != GraphicsBufferPool.maxFreeBuffers)
                GraphicsBufferPool.maxFreeBuffers = maxGraphicsBufferCaches;
            Render();
        }
        
        /// <summary>
        /// Return the <see cref="GraphicsBuffer"/> at index count - 1,
        /// which is garaunteed to have at least 'count' elements.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static GraphicsBuffer.IndirectDrawIndexedArgs[] GetIndirectArgs(int count)
        {
            while (indirectArgsPool.Count < count)
                indirectArgsPool.Add(new GraphicsBuffer.IndirectDrawIndexedArgs[indirectArgsPool.Count + 1]);
            return indirectArgsPool[count - 1];
        }
       
        public void Unregister(string provider)
        {
            UnityEngine.Profiling.Profiler.BeginSample("Unregister");
            List<DrawMeshIndirect> indirect;
            if (indirectBatches.TryGetValue(provider, out indirect))
            {
                foreach (var l in indirect)
                {
                    l.Dispose();
                }
            }
            indirectBatches.Remove(provider);
            UnityEngine.Profiling.Profiler.EndSample();
        }
        
        public void Register(string id, List<Draw> drawList)
        {
            Unregister(id);
            UnityEngine.Profiling.Profiler.BeginSample("Register Draws");
            List<DrawMeshIndirect> batch = poolBatches.GetObject();
            indirectBatches.Add(id, batch);
            foreach (var d in drawList)
            {
                if ((d.mtxs.IsCreated == false || d.mtxs.Length == 0) && d.mtxBuffer == null)
                {
                    continue;
                }

                UnityEngine.Profiling.Profiler.BeginSample("Register LOD Buffers");
                DrawMeshIndirect dmi = poolDrawMeshIndirect.GetObject();
                dmi.options = d.drawOptions;
                dmi.useCrossFade = d.useCrossFade;
                dmi.lodScreenHeights = d.lodScreenHeights;
                dmi.lodTransitionWidth = d.lodTransitionWidths;
                dmi.bounds = d.bounds;
                UnityEngine.Profiling.Profiler.BeginSample("Alloc Graphics Buffers");
                if (d.mtxBuffer != null)
                {
                    dmi.allInstances = d.mtxBuffer;
                    dmi.allInstances.name = "All Instances";
                    dmi.allInstanceCount = d.mtxBuffer.count;
                }
                else
                {
                    dmi.allInstances = new GraphicsBuffer(GraphicsBuffer.Target.Structured, d.mtxs.Length, 16 * 4);
                    dmi.allInstances.name = "All Instances";
                    UnityEngine.Profiling.Profiler.BeginSample("Upload Buffer");
                    dmi.allInstances.SetData(d.mtxs.AsArray()); // this is a big bottleneck when the instance count is very high
                    UnityEngine.Profiling.Profiler.EndSample();
                    dmi.allInstanceCount = d.mtxs.Length;
                }

                dmi.packedInstances = GraphicsBufferPool.GetBuffer(GraphicsBuffer.Target.Structured, dmi.allInstanceCount, IndirectTargetDataFoliage.GetEmitBufferSize());
                dmi.packedInstances.name = "Packed Transforms";
                
                UnityEngine.Profiling.Profiler.EndSample();

                if (packingShader == null)
                {
                    packingShader = Resources.Load<ComputeShader>("FoliageRendererInverse");
                    if (packingShader == null)
                    {
                        Debug.LogError("Cannot find packing shader");
                    }
                }

                // this will do the local to world transform, and generate a packed transform list if needed
                int packID = packingShader.FindKernel("CSMain");
                packingShader.SetBuffer(packID, _InputBuffer, dmi.allInstances);
                packingShader.SetMatrix(_LocalToWorld, d.localToWorld);
                packingShader.SetInt(_Count, dmi.allInstanceCount);
                packingShader.EnableKeyword(IndirectTargetDataFoliage.GetComputeKeyword());
                packingShader.SetBuffer(packID, _OutputBuffer, dmi.packedInstances);
                

                UnityEngine.Profiling.Profiler.BeginSample("Packing All Instances");
                packingShader.Dispatch(packID, Mathf.CeilToInt((float)dmi.allInstanceCount / 256), 1, 1);
                UnityEngine.Profiling.Profiler.EndSample();
                UnityEngine.Profiling.Profiler.BeginSample("LODs to Buffers");
                bool doShadows = d.drawOptions.shadowDistance > 0;
                foreach (var lod in d.meshLOD0)
                {
                    dmi.lod0 = poolDrawMeshIndirectLOD.GetObject();
                    dmi.lod0.drawMesh.Add(lod);
                    IndirectToBuffers(ref dmi, ref dmi.lod0, lod, doShadows);
                }
                if (d.meshLOD1 != null)
                {
                    foreach (var lod in d.meshLOD1)
                    {
                        dmi.lod1 = poolDrawMeshIndirectLOD.GetObject();
                        dmi.lod1.drawMesh.Add(lod);
                        IndirectToBuffers(ref dmi, ref dmi.lod1, lod, doShadows);
                    }
                }
                if (d.meshLOD2 != null)
                {
                    foreach (var lod in d.meshLOD2)
                    {
                        dmi.lod2 = poolDrawMeshIndirectLOD.GetObject();
                        dmi.lod2.drawMesh.Add(lod);
                        IndirectToBuffers(ref dmi, ref dmi.lod2, lod, doShadows);
                    }
                }
                if (d.meshLOD3 != null)
                {
                    foreach (var lod in d.meshLOD3)
                    {
                        dmi.lod3 = poolDrawMeshIndirectLOD.GetObject();
                        dmi.lod3.drawMesh.Add(lod);
                        IndirectToBuffers(ref dmi, ref dmi.lod3, lod, doShadows);
                    }
                }
                UnityEngine.Profiling.Profiler.EndSample();
                batch.Add(dmi);
                
                UnityEngine.Profiling.Profiler.EndSample();
            }
            UnityEngine.Profiling.Profiler.EndSample();
        } 

        private void IndirectToBuffers(ref DrawMeshIndirect dmi, ref DrawMeshIndirectLOD lod,  DrawMesh dm, bool shadows)
        {
            UnityEngine.Profiling.Profiler.BeginSample("IndirectToBuffers");
            lod.culledInstances = GraphicsBufferPool.GetBuffer(GraphicsBuffer.Target.Append, dmi.allInstanceCount, IndirectTargetDataFoliage.GetBufferSize());
            lod.argsBuffer = GraphicsBufferPool.GetBuffer(GraphicsBuffer.Target.IndirectArguments, dm.mesh.subMeshCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
            lod.culledInstances.name = "culled instances";
            lod.argsBuffer.name = "args buffer";
                       
            if (shadows)
            {
                lod.shadowInstances = GraphicsBufferPool.GetBuffer(GraphicsBuffer.Target.Append, dmi.allInstanceCount, IndirectTargetDataFoliage.GetBufferSize());
                lod.shadowArgsBuffer = GraphicsBufferPool.GetBuffer(GraphicsBuffer.Target.IndirectArguments, dm.mesh.subMeshCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
                lod.shadowInstances.name = "shadow instances";
                lod.shadowArgsBuffer.name = "shadow args buffer";
            }
            
            UnityEngine.Profiling.Profiler.EndSample();
        }

        
        private void Render()
        {
            if (doPostCullingCapture)
            {
                postCullingInstanceCount = 0;
                postCullingShadowCount = 0;
            }
            unculledInstanceCount = 0;
            drawCalls = 0;
            UnityEngine.Profiling.Profiler.BeginSample("RenderSetup");

            if (hiZOcclusion != ModeToggle.Off && drawCamera != null)
            {
                hiz = drawCamera.gameObject.GetComponent<HiZBuffer>();
                if (hiz == null)
                    hiz = drawCamera.gameObject.AddComponent<HiZBuffer>();

                if (reInitHiZ)
                {
                    hiz.ReInit();
                    reInitHiZ = false;
                }
                hiz.hideFlags = HideFlags.DontSave;
#if !_URP && !_HDRP
                hiz.Render();
#endif
#if !_HDRP
                hizBuffer = (RenderTexture)Shader.GetGlobalTexture(_HiZTexture);
#endif
            }
            UnityEngine.Profiling.Profiler.EndSample();
            DrawAllIndirect();
            doPostCullingCapture = false;
        }

        #region ShaderPropertyIDs
        static readonly int _CameraFrustumPlane0 = Shader.PropertyToID("_CameraFrustumPlane0");
        static readonly int _CameraFrustumPlane1 = Shader.PropertyToID("_CameraFrustumPlane1");
        static readonly int _CameraFrustumPlane2 = Shader.PropertyToID("_CameraFrustumPlane2");
        static readonly int _CameraFrustumPlane3 = Shader.PropertyToID("_CameraFrustumPlane3");
        static readonly int _CameraFrustumPlane4 = Shader.PropertyToID("_CameraFrustumPlane4");
        static readonly int _CameraFrustumPlane5 = Shader.PropertyToID("_CameraFrustumPlane5");

        static readonly int _HiZTexture = Shader.PropertyToID("_HiZTexture");
        static readonly int _HiZTextureSize = Shader.PropertyToID("_HiZTextureSize");
#if _HDRP
        static readonly int _CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
        static readonly int _ZBufferParams = Shader.PropertyToID("_ZBufferParams");
#endif
        static readonly int _LodBias = Shader.PropertyToID("_LodBias");
        static readonly int _LightDir = Shader.PropertyToID("_LightDir");

        static readonly int _PositionCount = Shader.PropertyToID("_PositionCount");
        static readonly int _CullDistance = Shader.PropertyToID("_CullDistance");
        static readonly int _CameraWorldPosition = Shader.PropertyToID("_CameraWorldPosition");
        static readonly int _CameraDirection = Shader.PropertyToID("_CameraDirection");
        static readonly int _ViewProjection = Shader.PropertyToID("_ViewProjection");
        static readonly int _BoundsCenter = Shader.PropertyToID("_BoundsCenter");
        static readonly int _BoundsExtents = Shader.PropertyToID("_BoundsExtents");
        static readonly int _LODScreenHeights = Shader.PropertyToID("_LODScreenHeights");
        static readonly int _CameraFOV = Shader.PropertyToID("_CameraFOV");
        static readonly int _BoundsExpand = Shader.PropertyToID("_BoundsExpand");
        static readonly int _InputBuffer = Shader.PropertyToID("_InputBuffer");
        static readonly int _OutputBufferLOD0 = Shader.PropertyToID("_OutputBufferLOD0");
        static readonly int _OutputBufferLOD1 = Shader.PropertyToID("_OutputBufferLOD1");
        static readonly int _OutputBufferLOD2 = Shader.PropertyToID("_OutputBufferLOD2");
        static readonly int _OutputBufferLOD3 = Shader.PropertyToID("_OutputBufferLOD3");
        static readonly int _ShadowBufferLOD0 = Shader.PropertyToID("_ShadowBufferLOD0");
        static readonly int _ShadowBufferLOD1 = Shader.PropertyToID("_ShadowBufferLOD1");
        static readonly int _ShadowBufferLOD2 = Shader.PropertyToID("_ShadowBufferLOD2");
        static readonly int _ShadowBufferLOD3 = Shader.PropertyToID("_ShadowBufferLOD3");
        static readonly int _ShadowDistance = Shader.PropertyToID("_ShadowDistance");
        static readonly int _LocalToWorld = Shader.PropertyToID("_LocalToWorld");
        static readonly int _LODTransitionWidth = Shader.PropertyToID("_LODTransitionWidth");
        static readonly int _Count = Shader.PropertyToID("_Count");
        static readonly int _PlaneOrigin = Shader.PropertyToID("_PlaneOrigin");

        public static readonly string CULLING_FILENAME = "FoliageRenderer_Culling";
        #endregion

        public void ApplyHiZToComputeShader(int id)
        {
#if _HDRP
            CullingShader.EnableKeyword("_HDRP");
            float4 zbuffer = Shader.GetGlobalVector(_ZBufferParams);
            CullingShader.SetVector(_ZBufferParams, zbuffer);
#else
            CullingShader.DisableKeyword("_HDRP");
            CullingShader.SetTextureFromGlobal(id, _HiZTexture, _HiZTexture);
            CullingShader.SetVector(_HiZTextureSize, Shader.GetGlobalVector(_HiZTextureSize));
#endif
        }

        private bool HiZTextureReady
        {
            get
            {
#if _HDRP
                return Shader.GetGlobalTexture(_CameraDepthTexture) != null;
#else
                return Shader.GetGlobalTexture(_HiZTexture) != null;
#endif
            }
        }

        public void DrawAllIndirect()
        {
            if (drawCamera == null)
                return;

            if (CullingShader == null)
                return;

            UnityEngine.Profiling.Profiler.BeginSample("DrawAllSetup");

            FrustumUtil.CalculateFrustrumPlanes(drawCamera);

            Vector4 cameraFrustumPlane0 = new Vector4(FrustumUtil.frustumPlanes[0].normal.x, FrustumUtil.frustumPlanes[0].normal.y, FrustumUtil.frustumPlanes[0].normal.z, FrustumUtil.frustumPlanes[0].distance);
            Vector4 cameraFrustumPlane1 = new Vector4(FrustumUtil.frustumPlanes[1].normal.x, FrustumUtil.frustumPlanes[1].normal.y, FrustumUtil.frustumPlanes[1].normal.z, FrustumUtil.frustumPlanes[1].distance);
            Vector4 cameraFrustumPlane2 = new Vector4(FrustumUtil.frustumPlanes[2].normal.x, FrustumUtil.frustumPlanes[2].normal.y, FrustumUtil.frustumPlanes[2].normal.z, FrustumUtil.frustumPlanes[2].distance);
            Vector4 cameraFrustumPlane3 = new Vector4(FrustumUtil.frustumPlanes[3].normal.x, FrustumUtil.frustumPlanes[3].normal.y, FrustumUtil.frustumPlanes[3].normal.z, FrustumUtil.frustumPlanes[3].distance);
            Vector4 cameraFrustumPlane4 = new Vector4(FrustumUtil.frustumPlanes[4].normal.x, FrustumUtil.frustumPlanes[4].normal.y, FrustumUtil.frustumPlanes[4].normal.z, FrustumUtil.frustumPlanes[4].distance);
            Vector4 cameraFrustumPlane5 = new Vector4(FrustumUtil.frustumPlanes[5].normal.x, FrustumUtil.frustumPlanes[5].normal.y, FrustumUtil.frustumPlanes[5].normal.z, FrustumUtil.frustumPlanes[5].distance);


            int cullID = CullingShader.FindKernel("CullKernel");
            CullingShader.SetVector(_CameraFrustumPlane0, cameraFrustumPlane0);
            CullingShader.SetVector(_CameraFrustumPlane1, cameraFrustumPlane1);
            CullingShader.SetVector(_CameraFrustumPlane2, cameraFrustumPlane2);
            CullingShader.SetVector(_CameraFrustumPlane3, cameraFrustumPlane3);
            CullingShader.SetVector(_CameraFrustumPlane4, cameraFrustumPlane4);
            CullingShader.SetVector(_CameraFrustumPlane5, cameraFrustumPlane5);

            if (shadowFrustumCulling == ModeToggle.Off)
                CullingShader.EnableKeyword("_NOSHADOWCULL");
            else
                CullingShader.DisableKeyword("_NOSHADOWCULL");

            CullingShader.DisableKeyword("_SKIP_SHADOWS");

            CullingShader.DisableKeyword("_USEHIZ");

            if (HiZTextureReady && hiZOcclusion == ModeToggle.On)
            {
                CullingShader.EnableKeyword("_USEHIZ");
                ApplyHiZToComputeShader(cullID);                
            }
            else
            {
                CullingShader.SetTexture(cullID, _HiZTexture, Texture2D.blackTexture);
                CullingShader.SetVector(_HiZTextureSize, Vector3.one);
            }

            Vector3 lightDir;
            if (sun == null)
            {
                Light[] lights = FindObjectsOfType<Light>();
                float maxIntensity = float.MinValue;

                foreach (Light light in lights)
                {
                    if (light.type == LightType.Directional && light.intensity > maxIntensity)
                    {
                        sun = light;
                        maxIntensity = light.intensity;
                    }
                }
            }
            if (sun == null)
            {
                lightDir = new Vector3(0.5f, -0.5f, 0).normalized;
            }
            else
            {
                lightDir = sun.transform.forward;
            }

            CullingShader.SetFloat(_LodBias, QualitySettings.lodBias);
            CullingShader.SetVector(_LightDir, lightDir);
            UnityEngine.Profiling.Profiler.EndSample();
            foreach (var batch in indirectBatches.Values)
            {
                foreach (DrawMeshIndirect draw in batch)
                {
                    if (draw.packedInstances != null && draw.allInstanceCount > 0)
                    {
                        //CullingShader.SetMatrix(_LocalToWorld, draw.localToWorld);
                        if (draw.options == null) draw.options = new DrawOptions();

                        // set format
                        CullingShader.EnableKeyword(IndirectTargetDataFoliage.GetComputeKeyword());

                        UnityEngine.Profiling.Profiler.BeginSample("Bounds Cull");
                        // cull bounds
                        var dist = Vector3.Distance(draw.bounds.ClosestPoint(drawCamera.transform.position), drawCamera.transform.position);
                        if (draw.bounds.extents == Vector3.zero || dist > draw.options.maxDrawDistance)
                        {
                            UnityEngine.Profiling.Profiler.EndSample();
                            continue;
                        }
                        UnityEngine.Profiling.Profiler.EndSample();

                        UnityEngine.Profiling.Profiler.BeginSample("Setup Culling");

                        CullingShader.DisableKeyword("_USELOD1");
                        CullingShader.DisableKeyword("_USELOD2");
                        CullingShader.DisableKeyword("_USELOD3");

                        if (draw.useCrossFade)
                        {
                            CullingShader.EnableKeyword("_USELODCROSSFADE");
                        }
                        else
                        {
                            CullingShader.DisableKeyword("_USELODCROSSFADE");
                        }

                        CullingShader.SetVector(_LODScreenHeights, draw.lodScreenHeights);
                        CullingShader.SetFloat(_CameraFOV, Mathf.Tan(Mathf.Deg2Rad * drawCamera.fieldOfView * 0.5f));
                        CullingShader.SetVector(_LODTransitionWidth, draw.lodTransitionWidth);
                        CullingShader.SetInt(_PositionCount, draw.allInstanceCount);
                        CullingShader.SetFloat(_CullDistance, draw.options.maxDrawDistance);
                        CullingShader.SetVector(_CameraWorldPosition, drawCamera.transform.position);
                        CullingShader.SetVector(_CameraDirection, drawCamera.transform.forward);
                        CullingShader.SetMatrix(_ViewProjection, drawCamera.projectionMatrix * drawCamera.worldToCameraMatrix);
 
                        CullingShader.SetVector(_BoundsExpand, draw.options.boundsExpand);
                        if (draw.options.useWorldYClip)
                        {
                            CullingShader.SetVector(_PlaneOrigin, new Vector3(0, draw.options.worldYClipHeight, 0));
                        }
                        else
                        {
                            CullingShader.SetVector(_PlaneOrigin, new Vector3(0, -99999, 0));
                        }
                        // set all buffers to something, even if not in use
                        CullingShader.SetBuffer(cullID, _InputBuffer, draw.allInstances);

                        CullingShader.SetBuffer(cullID, _OutputBufferLOD0, draw.lod0.culledInstances);
                        CullingShader.SetBuffer(cullID, _OutputBufferLOD1, draw.lod0.culledInstances);
                        CullingShader.SetBuffer(cullID, _OutputBufferLOD2, draw.lod0.culledInstances);
                        CullingShader.SetBuffer(cullID, _OutputBufferLOD3, draw.lod0.culledInstances);

                        float shadowDist = draw.options.shadowDistance;
                        if (shadowDist <= 0.0f)
                        {
                            CullingShader.EnableKeyword("_SKIP_SHADOWS");
                            CullingShader.DisableKeyword("_NOSHADOWCULL");
                        }
                        else if (draw.lod0.shadowInstances != null)
                        {
                            CullingShader.DisableKeyword("_SKIP_SHADOWS");
                            CullingShader.SetBuffer(cullID, _ShadowBufferLOD0, draw.lod0.shadowInstances);
                            CullingShader.SetBuffer(cullID, _ShadowBufferLOD1, draw.lod0.shadowInstances);
                            CullingShader.SetBuffer(cullID, _ShadowBufferLOD2, draw.lod0.shadowInstances);
                            CullingShader.SetBuffer(cullID, _ShadowBufferLOD3, draw.lod0.shadowInstances);
                        }

                        CullingShader.SetFloat(_ShadowDistance, shadowDist);

                        var bounds = draw.lod0.drawMesh[0].mesh.bounds;
                        foreach (var dm in draw.lod0.drawMesh)
                        {
                            bounds.Encapsulate(dm.mesh.bounds);
                        }
                        CullingShader.SetVector(_BoundsCenter, bounds.center);
                        CullingShader.SetVector(_BoundsExtents, bounds.extents);
                        
                        draw.lod0.culledInstances.SetCounterValue(0);
                        if (draw.lod0.shadowInstances != null)
                        {
                            draw.lod0.shadowInstances.SetCounterValue(0);
                        }

                        int lodLevel = 0;
                        if (draw.lod1 != null && draw.lod1.culledInstances != null)
                        {
                            lodLevel = 1;
                            draw.lod1.culledInstances.SetCounterValue(0);
                            CullingShader.SetBuffer(cullID, _OutputBufferLOD1, draw.lod1.culledInstances);
                            if (draw.lod1.shadowInstances != null)
                            {
                                draw.lod1.shadowInstances.SetCounterValue(0);
                                CullingShader.SetBuffer(cullID, _ShadowBufferLOD1, draw.lod1.shadowInstances);
                            }
                        }
                        if (draw.lod2 != null && draw.lod2.culledInstances != null)
                        {
                            lodLevel = 2;
                            draw.lod2.culledInstances.SetCounterValue(0);
                            CullingShader.SetBuffer(cullID, _OutputBufferLOD2, draw.lod2.culledInstances);

                            if (draw.lod2.shadowInstances != null)
                            {
                                draw.lod2.shadowInstances.SetCounterValue(0);
                                CullingShader.SetBuffer(cullID, _ShadowBufferLOD2, draw.lod2.shadowInstances);
                            }
                        }
                        if (draw.lod3 != null && draw.lod3.culledInstances != null)
                        {
                            lodLevel = 3;
                            draw.lod3.culledInstances.SetCounterValue(0);
                            CullingShader.SetBuffer(cullID, _OutputBufferLOD3, draw.lod3.culledInstances);
                            if (draw.lod3.shadowInstances != null)
                            {
                                draw.lod3.shadowInstances.SetCounterValue(0);
                                CullingShader.SetBuffer(cullID, _ShadowBufferLOD3, draw.lod3.shadowInstances);
                            }
                        }

                        if (lodLevel == 1)
                        {
                            CullingShader.EnableKeyword("_USELOD1");
                        }
                        else if (lodLevel == 2)
                        {
                            CullingShader.EnableKeyword("_USELOD2");
                        }
                        else if (lodLevel == 3)
                        {
                            CullingShader.EnableKeyword("_USELOD3");
                        }

                        CullingShader.DisableKeyword("_MAXSHADOWLOD1");
                        CullingShader.DisableKeyword("_MAXSHADOWLOD2");
                        CullingShader.DisableKeyword("_MAXSHADOWLOD3");
                        if (draw.options.maxShadowLOD == DrawOptions.MaxLOD.LOD1)
                        {
                            CullingShader.EnableKeyword("_MAXSHADOWLOD1");
                        }
                        else if (draw.options.maxShadowLOD == DrawOptions.MaxLOD.LOD2)
                        {
                            CullingShader.EnableKeyword("_MAXSHADOWLOD2");
                        }
                        else if (draw.options.maxShadowLOD == DrawOptions.MaxLOD.LOD3)
                        {
                            CullingShader.EnableKeyword("_MAXSHADOWLOD3");
                        }

                        UnityEngine.Profiling.Profiler.EndSample();
                        UnityEngine.Profiling.Profiler.BeginSample("Dispatch Culling");
                        CullingShader.Dispatch(cullID, Mathf.CeilToInt((float)draw.allInstanceCount / 256), 1, 1);
                        UnityEngine.Profiling.Profiler.EndSample();
      
                        UnityEngine.Profiling.Profiler.BeginSample("Draw");
                        unculledInstanceCount += draw.allInstanceCount;
                        draw.lod0.Draw(draw.packedInstances);

                        if (draw.lod1 != null && draw.lod1.culledInstances != null && draw.lod1.drawMesh != null)
                        {
                            draw.lod1.Draw(draw.packedInstances);
                        }
                        if (draw.lod2 != null && draw.lod2.culledInstances != null && draw.lod2.drawMesh != null)
                        {
                            draw.lod2.Draw(draw.packedInstances);
                        }
                        if (draw.lod3 != null && draw.lod3.culledInstances != null && draw.lod3.drawMesh != null)
                        {
                            draw.lod3.Draw(draw.packedInstances);
                        }
                        UnityEngine.Profiling.Profiler.EndSample();
                    }
                }
            }
        }
    }
}
