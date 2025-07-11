//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace JBooth.FoliageRendering
{
    /// <summary>
    /// Holds the <see cref="DrawMesh"/> Data for a single
    /// LOD worth of draws.
    /// </summary>
    public class DrawMeshIndirectLOD : IDisposable
    {
        public List<DrawMesh> drawMesh = new List<DrawMesh>();
        public GraphicsBuffer culledInstances;
        public GraphicsBuffer argsBuffer;
        public GraphicsBuffer shadowInstances;
        public GraphicsBuffer shadowArgsBuffer;

        static int _ArgsBuffer = Shader.PropertyToID("_ArgsBuffer");
        ComputeShader singlePassStereo;

        public void Dispose()
        {
            drawMesh.Clear();
            if (culledInstances != null) 
                IndirectRenderer.GraphicsBufferPool.ReturnBuffer(culledInstances);
            if (argsBuffer != null) 
                IndirectRenderer.GraphicsBufferPool.ReturnBuffer(argsBuffer);
            if (shadowInstances != null) 
                IndirectRenderer.GraphicsBufferPool.ReturnBuffer(shadowInstances);
            if (shadowArgsBuffer != null) 
                IndirectRenderer.GraphicsBufferPool.ReturnBuffer(shadowArgsBuffer);
            culledInstances = null;
            argsBuffer = null;
            shadowInstances = null;
            shadowArgsBuffer = null;
        }

        private void SetupArgsBuffer(Mesh mesh, GraphicsBuffer argsBuffer)
        {
            var args = IndirectRenderer.GetIndirectArgs(mesh.subMeshCount);

            for (int i = 0; i < mesh.subMeshCount; ++i)
            {
                args[i].instanceCount = 0;
                args[i].indexCountPerInstance = mesh.GetIndexCount(i);
                args[i].startIndex = mesh.GetIndexStart(i);
                args[i].startInstance = 0;
                args[i].baseVertexIndex = mesh.GetBaseVertex(i);
            }

            argsBuffer.SetData(args);
        }

        public void Draw(GraphicsBuffer packedBuffer)
        {
            foreach (var d in drawMesh)
            {
                if (d.mesh == null || d.materials == null || d.materials.Length == 0)
                    continue;

                SetupArgsBuffer(d.mesh, argsBuffer);
                if (shadowInstances != null)
                {
                    SetupArgsBuffer(d.mesh, shadowArgsBuffer);
                }
                CopyArgsBufferCount(d, IndirectRenderer.doPostCullingCapture);
                d.renderParams.matProps.SetBuffer(IndirectTargetDataFoliage.GetPropertyID(), culledInstances);
                if (packedBuffer != null)
                {
                    d.renderParams.matProps.SetBuffer(IndirectTargetDataFoliage.GetAllBufferID(), packedBuffer);
                }
                d.renderParams.shadowCastingMode = ShadowCastingMode.Off;
                d.renderParams.motionVectorMode = MotionVectorGenerationMode.Camera;

                for (int i = 0; i < d.mesh.subMeshCount; ++i)
                {
                    d.renderParams.material = d.materials[i];
                    IndirectRenderer.drawCalls++;
                    Graphics.RenderMeshIndirect(d.renderParams, d.mesh, argsBuffer, 1, i);
                }

                if (shadowInstances != null)
                {
                    d.renderParams.matProps.SetBuffer(IndirectTargetDataFoliage.GetPropertyID(), shadowInstances);
                    d.renderParams.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                    for (int i = 0; i < d.mesh.subMeshCount; ++i)
                    {
                        d.renderParams.material = d.materials[i];
                        IndirectRenderer.drawCalls++;
                        Graphics.RenderMeshIndirect(d.renderParams, d.mesh, shadowArgsBuffer, 1, i);
                    }
                }
            }
        }

        private uint GetArgsInstanceCount(GraphicsBuffer args)
        {
            var a = new GraphicsBuffer.IndirectDrawIndexedArgs[args.count];
            args.GetData(a);
            return a[0].instanceCount;
        }

        
        public void CopyArgsBufferCount(DrawMesh d, bool captureCount)
        {
            for (int i = 0; i < d.mesh.subMeshCount; ++i)
            {
                GraphicsBuffer.CopyCount(culledInstances, argsBuffer, sizeof(uint) * i * 5 + sizeof(uint));

                if (shadowInstances != null)
                {
                    GraphicsBuffer.CopyCount(shadowInstances, shadowArgsBuffer, sizeof(uint) * i * 5 + sizeof(uint));
                }
            }
            if (captureCount == true)
            {
                IndirectRenderer.postCullingInstanceCount += (int)GetArgsInstanceCount(argsBuffer);
                if (shadowInstances != null)
                {
                    IndirectRenderer.postCullingShadowCount += (int)GetArgsInstanceCount(shadowArgsBuffer);
                }
            }

            if (XRSettings.eyeTextureDesc.vrUsage == VRTextureUsage.TwoEyes)
            {
                if (singlePassStereo == null)
                {
                    singlePassStereo = Resources.Load<ComputeShader>("FoliageRendererSinglePassVR");
                    if (singlePassStereo == null)
                    {
                        Debug.LogError("Cannot find single pass VR compute shader");
                    }
                }
                int kernel = singlePassStereo.FindKernel("CSMain");
                singlePassStereo.SetBuffer(kernel, _ArgsBuffer, argsBuffer);
                singlePassStereo.Dispatch(kernel, 1, 1, 1);
                if (shadowInstances != null)
                {
                    singlePassStereo.SetBuffer(kernel, _ArgsBuffer, shadowArgsBuffer);
                    singlePassStereo.Dispatch(kernel, 1, 1, 1);
                }
            }
        }
    }


}