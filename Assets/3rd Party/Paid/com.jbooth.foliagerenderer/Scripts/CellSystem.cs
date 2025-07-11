//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

// dynamic load/unload of cells from terrain data. 

using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace JBooth.FoliageRendering
{
    public class CellSystem
    {
        public CellSystem(string id)
        {
            this.id = id;
        }

        public class CellLoader : IDisposable
        {
            public bool loaded;
            public List<Draw> entries = new List<Draw>();

            public void Dispose()
            {
                foreach (var e in entries)
                {
                    if (e.mtxs.IsCreated) e.mtxs.Dispose();
                    if (e.mtxBuffer != null) e.mtxBuffer.Dispose();
                }
                entries.Clear();
                loaded = false;
            }
        }

        /* this was an attempt to put data from the mesh workflow into the cell system, 
         * never got it to work/finished it though..
         
        struct ExtractJobHolder
        {
            public JobHandle handle;
            public CellExtractJob job;
            public NativeList<Matrix4x4> mtxs;
            public CellLoader loader;
        }
        [BurstCompile]
        struct CellExtractJob : IJobParallelFor
        {
            [ReadOnly] public Bounds cellBounds;
            [ReadOnly] public NativeArray<Matrix4x4> mtxs;
            public NativeList<Matrix4x4>.ParallelWriter outputMatrices;

            public void Execute(int index)
            {
                var mtx = mtxs[index];
                if (cellBounds.Contains(mtx.GetPosition()))
                    outputMatrices.AddNoResize(mtx);
            }
        }
        
        List<ExtractJobHolder> extractJobs = new List<ExtractJobHolder>(32);
        public void SubdivideAndPopulateCells(List<Draw> dList)
        {
            // total bounds
            Bounds worldBounds = new Bounds();
            if (dList.Count > 0)
            {
                var draw = dList[0];
                worldBounds = dList[0].bounds;
                paramSets[draw.prefab] = draw;
                for (int i = 1; i < dList.Count; ++i)
                {
                    draw = dList[i];
                    worldBounds.Encapsulate(draw.bounds);
                }
            }

            // copy draw list into each loader
            foreach (var loader in cellLoaders)
            {
                loader.entries = new List<Draw>(dList);
            }

            int cellX = (int)math.ceil(worldBounds.size.x / 50);
            int cellZ = (int)math.ceil(worldBounds.size.z / 50);
            float cellWidth = worldBounds.size.x / cellX;
            float cellHeight = worldBounds.size.z / cellZ;
            extractJobs.Clear();

            foreach (var draw in dList)
            {
                for (int x = 0; x < cellX; x++)
                {
                    for (int z = 0; z < cellZ; z++)
                    {
                        Vector3 cellMin = worldBounds.min + new Vector3(x * cellWidth, 0, z * cellHeight);
                        Bounds cellBounds = new Bounds(cellMin + new Vector3(cellWidth, 0, cellHeight) * 0.5f, new Vector3(cellWidth, 99999, cellHeight));
                        var loader = cellLoaders[z * cellX + x];
                        loader.loaded = true;
                        NativeList<Matrix4x4> entries = new NativeList<Matrix4x4>(draw.mtxs.Length, Allocator.Persistent);
                        CellExtractJob job = new CellExtractJob()
                        {
                            mtxs = draw.mtxs,
                            cellBounds = cellBounds,
                            outputMatrices = entries.AsParallelWriter(),
                        };
                        var handle = job.Schedule(draw.mtxs.Length, 512);
                        extractJobs.Add(new ExtractJobHolder() { handle = handle, mtxs = entries, job = job, loader = loader});
                    }
                }

                for (int i = 0; i < extractJobs.Count; ++i)
                {
                    CellLoader l = extractJobs[i].loader;
                    extractJobs[i].handle.Complete();
                    foreach (var e in l.entries)
                    {
                        if (e.prefab == draw.prefab)
                        {
                            e.mtxs = extractJobs[i].mtxs;
                            e.bounds = extractJobs[i].job.cellBounds;
                        }
                    }
                }
            }
        }
        */


        [BurstCompile]
        struct CellVisibilityJob : IJob
        {
            [ReadOnly] public int cellsPerSideX; // Number of cells along the X-axis
            [ReadOnly] public int cellsPerSideY; // Number of cells along the Y-axis
            [ReadOnly] public float cellWidth; // Width of each cell
            [ReadOnly] public float cellHeight; // Height of each cell
            [ReadOnly] public float3 cameraLocalPosition; // Camera position in local space
            [ReadOnly] public float viewDistance;
            [ReadOnly] public float unloadDistance;
            public NativeBitArray changed;


            public NativeBitArray cells;

            public void Execute()
            {
                for (int x = 0; x < cellsPerSideX; ++x)
                {
                    for (int y = 0; y < cellsPerSideY; ++y)
                    {
                        int index = y * cellsPerSideX + x;
                        float2 cellPosition = new float2(x * cellWidth, y * cellHeight);
                        float2 cellMin = cellPosition;
                        float2 cellMax = cellPosition + new float2(cellWidth, cellHeight);

                        // Find closest point on the cell to the camera
                        float2 closestPoint = new float2(
                            math.max(cellMin.x, math.min(cameraLocalPosition.x, cellMax.x)),
                            math.max(cellMin.y, math.min(cameraLocalPosition.z, cellMax.y))
                        );

                        // Calculate distance from camera to closest point on the cell
                        float distanceToEdge = math.distance(new float2(cameraLocalPosition.x, cameraLocalPosition.z), closestPoint);
                        bool closeEnough = distanceToEdge <= viewDistance;
                        if (cells.IsSet(index * 2) != closeEnough)
                            changed.Set(0, true);

                        cells.Set(index * 2, closeEnough);
                        cells.Set(index * 2 + 1, distanceToEdge >= unloadDistance);
                    }
                }
            }
        }


        public bool IsPotentiallyVisible(int i) { return cells.IsSet(i * 2); }
        public bool NeedUnload(int i) { return cells.IsSet(i * 2 + 1); }

        public NativeBitArray cells;

        string id;
        List<Draw> drawList = new List<Draw>();

        public CellLoader[] cellLoaders;

        public struct ParamSet
        {
            //public Bounds bounds;
            public DrawOptions drawOptions;
            public RenderParams renderParams;
        }

        public Dictionary<GameObject, ParamSet> paramSets = new Dictionary<GameObject, ParamSet>();
        Dictionary<GameObject, GraphicsBuffer> tempDraws = new Dictionary<GameObject, GraphicsBuffer>();
        Dictionary<GameObject, int> tempSizes = new Dictionary<GameObject, int>();

        public void Dispose()
        {
            UnregisterCells();
            if (cells.IsCreated)
                cells.Dispose();
            paramSets.Clear();
        }


        public void UnregisterCells()
        {
            if (cellLoaders != null)
            {
                for (int i = 0; i < cellLoaders.Length; ++i)
                {
                    var c = cellLoaders[i];
                    c.Dispose();
                }
                cellLoaders = null;
            }
            if (IndirectRenderer.hasInstance)
                IndirectRenderer.instance.Unregister(id);
        }

        int cellDimX;
        int cellDimY;
        Bounds worldCellBounds;
        public void SetupCells(int pcX, int pcY, Bounds systemWorldBounds, bool clearCells = false)
        {
            if (combineShader == null)
            {
                combineShader = Resources.Load<ComputeShader>("FoliageRendererCombineCells");
                combineKernal = combineShader.FindKernel("CSMain");
            }
            worldCellBounds = systemWorldBounds;
            cellDimX = pcX;
            cellDimY = pcY;
            int total = pcX * pcY;
            bool init = false;
            if (cellLoaders == null || cellLoaders.Length != total)
            {
                Dispose();
                init = true;
                cells = new NativeBitArray(total * 2, Allocator.Persistent, NativeArrayOptions.ClearMemory);
                cellLoaders = new CellLoader[total];
            }

            if (cells.IsCreated == false)
            {
                cells = new NativeBitArray(total * 2, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            }

            for (int y = 0; y < pcY; ++y)
            {
                for (int x = 0; x < pcX; ++x)
                {
                    int idx = y * pcY + x;
                    if (init)
                    {
                        cellLoaders[idx] = new CellLoader();
                    }
                    else if (clearCells)
                    {
                        cellLoaders[idx].Dispose();
                    }
                }
            }
        }

        ComputeShader combineShader;
        int combineKernal;
        static int _Output = Shader.PropertyToID("Output");
        static int[] Inputs = new int[8]
        {
            Shader.PropertyToID("Input"),
            Shader.PropertyToID("Input2"),
            Shader.PropertyToID("Input3"),
            Shader.PropertyToID("Input4"),
            Shader.PropertyToID("Input5"),
            Shader.PropertyToID("Input6"),
            Shader.PropertyToID("Input7"),
            Shader.PropertyToID("Input8"),
        };

        static int[] _Counts = new int[8]
        {
            Shader.PropertyToID("_Count"),
            Shader.PropertyToID("_Count2"),
            Shader.PropertyToID("_Count3"),
            Shader.PropertyToID("_Count4"),
            Shader.PropertyToID("_Count5"),
            Shader.PropertyToID("_Count6"),
            Shader.PropertyToID("_Count7"),
            Shader.PropertyToID("_Count8"),
        };

        public static int graphicsBufferBytes;

        // Timings on m2 mac, 500 meter visibility
        // 2.1ms total - approximately 30% faster in build
        // Combining Cell Draws (no dispatch)   : 0.88ms
        // Dispatching for above                : 0.59ms
        // Compute Array Sizes                  : 0.83ms (reduced, see below)
        // Compute Visibility                   : 0.07ms
        //
        // Findings
        // - Encapsulating the bounds is the bottleneck in compute array sizes. I tried moving this to a job
        // to run in a thread during combining cells, but the setup time was enough to nullify the gains, and
        // would be worse for smaller view sizes. Two possible optimizations. A) Just pass in entire bounds of
        // the cell area and let it be the render bounds. Penalizes small patches of unique details that are
        // only in one area, but maybe not a practical worry. B) Do some kind of smart search to find the extents.
        // Right now, it's encapsulating 1100 bounds in the demo scene. Currently doing A. 
        // - Dispatch for combining cell draws is expensive. This could be reworked to combine multiple cells in
        // a single dispatch, say 16, 8, 4, until you use the 1 version. This would reduce dispatch calls by a lot.
        // - Use of dictionary's is likely a factor. A restructure to cell loader such that it's one per game object
        // per cell might speed things up, allowing for each processing of all draws at once instead of iterating
        // over cells then internal draw lists. 
        Dictionary<GameObject, List<Draw>> flatDraws = new Dictionary<GameObject, List<Draw>>();
        public void CombineVisibleAndDraw(Vector3 camPos, float maxDistance)
        {
            Profiler.BeginSample("CombineVisibleAndDraw");
            IndirectRenderer.instance.Unregister(id);
            Profiler.BeginSample("Compute Array Sizes");
            // first compute final array size and bounds so we can alloc the buffers exactly.
            tempDraws.Clear();
            drawList.Clear();
            tempSizes.Clear();
            
            graphicsBufferBytes = 0;


            for (int index = 0; index < cellLoaders.Length; ++index)
            {
                var loader = cellLoaders[index];
                if (loader.loaded && IsPotentiallyVisible(index))
                {
                    for (int eidx = 0; eidx < loader.entries.Count; ++eidx)
                    {
                        var entry = loader.entries[eidx];
                        var prefab = entry.prefab;
                        if (!tempSizes.ContainsKey(prefab))
                        {
                            tempSizes[prefab] = entry.mtxBuffer.count;
                            //Profiler.BeginSample("Encapsulate Bounds");
                            //var p = paramSets[prefab];
                            //p.bounds = entry.bounds;
                            //paramSets[prefab] = p;
                            //Profiler.EndSample();
                        }
                        else
                        {
                            tempSizes[prefab] += entry.mtxBuffer.count;
                            //Profiler.BeginSample("Encapsulate Bounds");
                            //var p = paramSets[prefab];
                            //p.bounds.Encapsulate(entry.bounds);
                            //paramSets[prefab] = p;
                            //Profiler.EndSample();
                        }
                        if (!flatDraws.ContainsKey(prefab))
                        {
                            flatDraws[prefab] = new List<Draw>(256);
                        }
                        flatDraws[prefab].Add(entry);
                    }
                }
            }
            
            Profiler.EndSample();
            Profiler.BeginSample("Combining Cell Draws");
            foreach (var k in tempSizes.Keys)
            {
                int tempSize = tempSizes[k];
                var gb = new GraphicsBuffer(GraphicsBuffer.Target.Append, tempSize, 16 * 4);
                gb.SetCounterValue(0);
                graphicsBufferBytes += tempSize * 16;
                tempDraws[k] = gb;
                combineShader.SetBuffer(combineKernal, _Output, gb);

                var draws = flatDraws[k];
                int drawCount = draws.Count;
                int drawIdx = 0;
                combineShader.EnableKeyword("_EIGHT");

                while (drawCount > 8)
                {
                    int maxCount = 0;
                    for (int idx = 0; idx < 8; ++idx)
                    {
                        var entry = draws[drawIdx + idx];
                        int count = entry.mtxBuffer.count;
                        combineShader.SetBuffer(combineKernal, Inputs[idx], entry.mtxBuffer);
                        graphicsBufferBytes += count * 16;
                        combineShader.SetInt(_Counts[idx], count);
                        maxCount = math.max(count, maxCount);
                    }
                    Profiler.BeginSample("Dispatch");
                    combineShader.Dispatch(combineKernal, Mathf.CeilToInt(maxCount / 256.0f), 1, 1);
                    Profiler.EndSample();

                    drawIdx += 8;
                    drawCount -= 8;
                }
                combineShader.DisableKeyword("_EIGHT");
                while (drawCount > 0)
                {
                    var entry = draws[drawIdx];
                    int count = entry.mtxBuffer.count;

                    combineShader.SetBuffer(combineKernal, Inputs[0], entry.mtxBuffer);
                    graphicsBufferBytes += count * 16;
                    combineShader.SetInt(_Counts[0], count);
                    Profiler.BeginSample("Dispatch");
                    combineShader.Dispatch(combineKernal, Mathf.CeilToInt(count / 256.0f), 1, 1);
                    Profiler.EndSample();
                    drawIdx++;
                    drawCount--;
                }
            }

            foreach (var fd in flatDraws.Values)
            {
                fd.Clear();
            }

            Profiler.EndSample();

            
            Profiler.BeginSample("Add to Draw List");
            Bounds bounds = new Bounds(camPos, new Vector3(maxDistance, maxDistance, maxDistance));
            foreach (var prefab in tempDraws.Keys)
            {
                // because detail objects require us to put a child when LOD groups are present
                var prefabToUse = prefab;
                if (prefab.transform.parent != null)
                    prefabToUse = prefab.transform.parent.gameObject;
                var mtxBuffer = tempDraws[prefab];
                var paramSet = paramSets[prefab];
                Draw.AddToDrawList(prefabToUse, drawList, mtxBuffer, paramSet.renderParams, bounds, paramSet.drawOptions, Matrix4x4.identity);
            }
            Profiler.EndSample();

            IndirectRenderer.instance.Register(id, drawList);

            tempDraws.Clear();
            Profiler.EndSample();
        }

        public bool ComputeCellVisibility(Vector3 cameraWorldPosition, float maxVisibleDistance, float unloadDistance)
        {

            float cellWidth = worldCellBounds.size.x / cellDimX;
            float cellHeight = worldCellBounds.size.z / cellDimY;

            float extraDistance = math.max(cellWidth, cellHeight);
            if (unloadDistance < maxVisibleDistance + extraDistance)
                unloadDistance = maxVisibleDistance + extraDistance;

            maxVisibleDistance += extraDistance;
            NativeBitArray changedBit = new NativeBitArray(1, Allocator.TempJob);
            changedBit.Set(0, false);
            CellVisibilityJob job = new CellVisibilityJob()
            {
                cells = cells,
                cellsPerSideX = cellDimX,
                cellsPerSideY = cellDimY,
                cellWidth = cellWidth,
                cellHeight = cellHeight,
                cameraLocalPosition = cameraWorldPosition - (worldCellBounds.center - worldCellBounds.extents),
                viewDistance = maxVisibleDistance,
                unloadDistance = unloadDistance,
                changed = changedBit
            };
            
            job.Schedule().Complete();
            bool changed = changedBit.IsSet(0);
            changedBit.Dispose();
            return changed;
        }
    }
}
