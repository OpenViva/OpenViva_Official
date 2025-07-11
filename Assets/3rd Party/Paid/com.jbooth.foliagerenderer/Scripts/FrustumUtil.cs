//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System;
using System.Reflection;
using UnityEngine;

namespace JBooth.FoliageRendering
{
    /// <summary>
    /// Utility class for extracting sealed Unity Internal function data...
    /// </summary>
    public static class FrustumUtil
    {
        public static Plane[] frustumPlanes = new Plane[6];

        private static readonly Action<Plane[], Matrix4x4> InternalExtractPlanes =
            (Action<Plane[], Matrix4x4>)Delegate.CreateDelegate(
                typeof(Action<Plane[], Matrix4x4>),
                // ReSharper disable once AssignNullToNotNullAttribute
                typeof(GeometryUtility).GetMethod("Internal_ExtractPlanes",
                    BindingFlags.Static | BindingFlags.NonPublic));

        public static void CalculateFrustrumPlanes(Camera camera)
        {
            InternalExtractPlanes(frustumPlanes, camera.projectionMatrix * camera.worldToCameraMatrix);
        }
    }
}