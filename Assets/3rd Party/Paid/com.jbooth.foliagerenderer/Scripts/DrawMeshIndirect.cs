//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System;
using UnityEngine;

namespace JBooth.FoliageRendering
{
    /// <summary>
    /// Indirect draw data for all LODs
    /// </summary>
    public class DrawMeshIndirect : IDisposable
    {
        public DrawMeshIndirectLOD lod0;
        public DrawMeshIndirectLOD lod1;
        public DrawMeshIndirectLOD lod2;
        public DrawMeshIndirectLOD lod3;
        public GraphicsBuffer allInstances;
        public bool allInstancesPooled;
        public int allInstanceCount;
        public GraphicsBuffer packedInstances; // uses same count as all instances
        public bool useCrossFade;
        public Vector4 lodScreenHeights;
        public Vector4 lodTransitionWidth;
        public DrawOptions options;
        public Bounds bounds;

        public void Dispose()
        {
            if (allInstances != null) 
                allInstances.Dispose();
            if (packedInstances != null) 
                IndirectRenderer.GraphicsBufferPool.ReturnBuffer(packedInstances);
            if (lod0 != null) 
                IndirectRenderer.PoolDrawMeshIndirectLOD.ReturnObject(lod0);
            if (lod1 != null) 
                IndirectRenderer.PoolDrawMeshIndirectLOD.ReturnObject(lod1);
            if (lod2 != null) 
                IndirectRenderer.PoolDrawMeshIndirectLOD.ReturnObject(lod2);
            if (lod3 != null) 
                IndirectRenderer.PoolDrawMeshIndirectLOD.ReturnObject(lod3);
            allInstances = null;
            lod0 = null;
            lod1 = null;
            lod2 = null;
            lod3 = null;
            allInstanceCount = 0;
        }
    }
}