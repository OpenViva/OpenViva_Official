
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;


namespace nTools.PrefabPainter
{
    //
    // class PrefabPainter
    //

    public partial class PrefabPainter : EditorWindow
    {

        public struct RaycastInfo
        {
            public Ray ray;
            public bool isHitTargetLayer;
            public bool isHitMaskedLayer;
            public Vector3 point;
            public Vector3 normal;
            public Vector3 localPoint;
            public Vector3 localNormal;
            public float distance;
            public GameObject hitObject;

            public bool isHit { get { return isHitTargetLayer || isHitMaskedLayer; } }

            public bool IntersectsHitPlane(Ray ray, out Vector3 hitPoint)
            {
                float rayDistance;
                Plane plane = new Plane(normal, point);
                if (plane.Raycast(ray, out rayDistance))
                {
                    hitPoint = ray.GetPoint(rayDistance);
                    return true;
                }
                hitPoint = Vector3.zero;
                return false;
            }
        }


        //
        // class Octree
        //
        public class Octree
        {
            class OctreeObject
            {
                public GameObject gameObject;
                public int layer;

                public Mesh mesh;
                public Renderer renderer;
                public Collider collider;
                public RectTransform rectTransform;
                public Vector3[] cornerPoints = new Vector3[4];
                public Bounds bounds;

                public int raycastOpID;
            }

            struct RaycastData
            {
                public Ray ray;
                public int layersMask;
                public int ignoreLayersMask;
                public List<GameObject> objectList;

                public RaycastInfo raycastInfo;
                public GameObject gameObject;
            }

            struct SortedObject
            {
                public OctreeObject obj;
                public float boundsDistance;
            }

            class Node
            {
                public Node[] childs = new Node[8];
                public Bounds bounds;
                public List<OctreeObject> objects = null;

                const float kNodesOverlapSize = 0.001f; // fixes situations where ray lies between nodes

                public Node(int depth)
                {
                    if (depth <= 0)
                        return;

                    for (int i = 0; i < 8; i++)
                        childs[i] = new Node(depth - 1);
                }

                public void Resize(Bounds treeBounds)
                {
                    bounds = treeBounds;

                    if (childs[0] == null)
                        return;

                    Vector3 center = treeBounds.center;
                    Vector3 offset = treeBounds.extents * 0.5f;
                    Vector3 childSize = treeBounds.extents + new Vector3(kNodesOverlapSize, kNodesOverlapSize, kNodesOverlapSize); ;

                    childs[0].Resize(new Bounds(new Vector3(center.x + offset.x, center.y + offset.y, center.z + offset.z), childSize));
                    childs[1].Resize(new Bounds(new Vector3(center.x + offset.x, center.y + offset.y, center.z - offset.z), childSize));
                    childs[2].Resize(new Bounds(new Vector3(center.x + offset.x, center.y - offset.y, center.z + offset.z), childSize));
                    childs[3].Resize(new Bounds(new Vector3(center.x + offset.x, center.y - offset.y, center.z - offset.z), childSize));
                    childs[4].Resize(new Bounds(new Vector3(center.x - offset.x, center.y + offset.y, center.z + offset.z), childSize));
                    childs[5].Resize(new Bounds(new Vector3(center.x - offset.x, center.y + offset.y, center.z - offset.z), childSize));
                    childs[6].Resize(new Bounds(new Vector3(center.x - offset.x, center.y - offset.y, center.z + offset.z), childSize));
                    childs[7].Resize(new Bounds(new Vector3(center.x - offset.x, center.y - offset.y, center.z - offset.z), childSize));
                }

                public void Cleanup()
                {
                    if (objects != null)
                        objects.Clear();

                    if (childs[0] != null)
                    {
                        for (int i = 0; i < 8; i++)
                            childs[i].Cleanup();
                    }
                }

                public bool RemoveGameObject(GameObject gameObject)
                {
                    if (objects != null)
                    {
                        objects.RemoveAll((o) => o.gameObject == gameObject);
                    }

                    if (childs[0] != null)
                    {
                        for (int i = 0; i < 8; i++)
                            if (childs[i].RemoveGameObject(gameObject))
                                return true;
                    }
                    return false;
                }


                public void AddObject(OctreeObject obj)
                {
                    if (bounds.Intersects(obj.bounds))
                    {
                        if (childs[0] == null)
                        {
                            if (objects == null)
                                objects = new List<OctreeObject>(16);
                            objects.Add(obj);
                        }
                        else
                        {
                            for (int i = 0; i < 8; i++)
                                childs[i].AddObject(obj);
                        }
                    }
                }
            }

            Node m_Tree;
            List<OctreeObject> m_DynamicObjects = null;
            List<SortedObject> m_SortedObjects = new List<SortedObject>();

            int s_RaycastOpID = 0;

            public int raycastCounter = 0;
            public int intersectRayMeshCounter = 0;


            delegate bool HandleUtility_IntersectRayMesh(Ray ray, Mesh mesh, Matrix4x4 matrix, out UnityEngine.RaycastHit raycastHit);
            static HandleUtility_IntersectRayMesh IntersectRayMesh = null;

            // Static Constructor
            static Octree()
            {
                MethodInfo methodIntersectRayMesh = typeof(HandleUtility).GetMethod("IntersectRayMesh", BindingFlags.Static | BindingFlags.NonPublic);

                if (methodIntersectRayMesh != null)
                {
                    IntersectRayMesh = delegate (Ray ray, Mesh mesh, Matrix4x4 matrix, out UnityEngine.RaycastHit raycastHit)
                    {
                        object[] parameters = new object[] { ray, mesh, matrix, null };
                        bool result = (bool)methodIntersectRayMesh.Invoke(null, parameters);
                        raycastHit = (UnityEngine.RaycastHit)parameters[3];
                        return result;
                    };
                }

            }

            public Octree(int depth)
            {
                m_Tree = new Node(depth);
            }

            public void AddDynamicObject(GameObject gameObject, bool useAdditionalVertexStreams)
            {
                Utility.ForAllInHierarchy(gameObject, (go) =>
                {
                    OctreeObject octreeObject = MakeOctreeObject(go, useAdditionalVertexStreams);

                    if (octreeObject != null)
                    {
                        if (m_DynamicObjects == null)
                            m_DynamicObjects = new List<OctreeObject>(16);

                        m_DynamicObjects.Add(octreeObject);
                    }
                });
            }


            OctreeObject MakeOctreeObject(GameObject gameObject, bool useAdditionalVertexStreams)
            {
                if (!gameObject.activeInHierarchy)
                    return null;

                Renderer renderer = gameObject.GetComponent<Renderer>();
                MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
                Collider collider;
                RectTransform rectTransform;

                if(renderer != null && renderer.enabled && renderer is SkinnedMeshRenderer)
                {
                    OctreeObject obj = new OctreeObject();

                    obj.renderer = renderer;
                    obj.bounds = renderer.bounds;
                    obj.layer = 1 << gameObject.layer;
                    obj.raycastOpID = s_RaycastOpID;

                    obj.gameObject = gameObject;

                    SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
                    obj.mesh = skinnedMeshRenderer.sharedMesh;

                    if (obj.mesh == null)
                        return null;

                    return obj;
                }
                else
                if (renderer != null && renderer.enabled && renderer is SpriteRenderer)
                {
                    OctreeObject obj = new OctreeObject();

                    obj.renderer = renderer;
                    obj.bounds = renderer.bounds;
                    obj.layer = 1 << gameObject.layer;
                    obj.raycastOpID = s_RaycastOpID;

                    obj.gameObject = gameObject;

                    return obj;
                }
                else
                if (renderer != null && renderer.enabled &&
                    meshFilter != null && meshFilter.sharedMesh != null)
                {
                    OctreeObject obj = new OctreeObject();

                    obj.renderer = renderer;
                    obj.bounds = renderer.bounds;
                    obj.layer = 1 << gameObject.layer;
                    obj.raycastOpID = s_RaycastOpID;

                    obj.gameObject = gameObject;

                    MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

                    if (useAdditionalVertexStreams && meshRenderer != null && meshRenderer.additionalVertexStreams != null)
                        obj.mesh = meshRenderer.additionalVertexStreams;
                    else
                        obj.mesh = meshFilter.sharedMesh;

                    return obj;
                }
                else if ((collider = gameObject.GetComponent<Collider>()) != null && collider.enabled)
                {
                    OctreeObject obj = new OctreeObject();

                    obj.collider = collider;
                    obj.bounds = collider.bounds;
                    obj.layer = 1 << gameObject.layer;
                    obj.raycastOpID = s_RaycastOpID;

                    obj.gameObject = gameObject;

                    return obj;
                }
                else if ((rectTransform = gameObject.GetComponent<RectTransform>()) != null)
                {
                    OctreeObject obj = new OctreeObject();

                    rectTransform.GetWorldCorners(obj.cornerPoints);

                    Bounds bounds = new Bounds(obj.cornerPoints[0], Vector3.zero);
                    bounds.Encapsulate(obj.cornerPoints[1]);
                    bounds.Encapsulate(obj.cornerPoints[2]);
                    bounds.Encapsulate(obj.cornerPoints[3]);

                    obj.rectTransform = rectTransform;

                    obj.bounds = bounds;
                    obj.layer = 1 << gameObject.layer;
                    obj.raycastOpID = s_RaycastOpID;

                    obj.gameObject = gameObject;

                    return obj;
                }

                return null;
            }

            public void Populate(GameObject[] sceneObjects, bool useAdditionalVertexStreams)
            {
                Cleanup();

                Bounds worldBounds = new Bounds(Vector3.zero, Vector3.zero);
                List<OctreeObject> raycastObjects = new List<OctreeObject>(sceneObjects.Length);

                for (int i = 0; i < sceneObjects.Length; i++)
                {
                    if (!sceneObjects[i].activeInHierarchy)
                        continue;

                    OctreeObject octreeObject = MakeOctreeObject(sceneObjects[i], useAdditionalVertexStreams);
                    if (octreeObject != null)
                    {
                        worldBounds.Encapsulate(octreeObject.bounds);
                        raycastObjects.Add(octreeObject);
                    }
                }

                m_Tree.Resize(worldBounds);

                for (int i = 0; i < raycastObjects.Count; i++)
                    m_Tree.AddObject(raycastObjects[i]);
            }



            public void Cleanup()
            {
                if (m_DynamicObjects != null)
                    m_DynamicObjects.Clear();

                m_Tree.Cleanup();
            }


            public void RemoveGameObject(GameObject gameObject)
            {
                List<GameObject> objectsList = new List<GameObject>(16);

                Utility.ForAllInHierarchy(gameObject, (go) => { objectsList.Add(go); });

                if (m_DynamicObjects != null)
                {
                    foreach (GameObject go in objectsList)
                        m_DynamicObjects.RemoveAll((o) => o.gameObject == go);
                }

                foreach (GameObject go in objectsList)
                    m_Tree.RemoveGameObject(go);
            }


            static bool RaycastTriangle(Ray ray, Vector3 p1, Vector3 p2, Vector3 p3, out float u, out float v, out float t)
            {
                Vector3 e1 = p2 - p1;
                Vector3 e2 = p3 - p1;

                Vector3 pvec = Vector3.Cross(ray.direction, e2);
                float det = Vector3.Dot(e1, pvec);

                u = v = t = 0;

                if (det == 0.0f)
                    return false;

                float invDet = 1.0f / det;
                Vector3 tvec = ray.origin - p1;

                u = invDet * Vector3.Dot(tvec, pvec);
                if (u < 0.0f || u > 1.0f)
                    return false;

                Vector3 qvec = Vector3.Cross(tvec, e1);
                v = invDet * Vector3.Dot(qvec, ray.direction);
                if (v < 0.0f || u + v > 1.0f)
                    return false;

                t = Vector3.Dot(e2, qvec) * invDet;

                if (t > Mathf.Epsilon)
                    return true;

                return false;
            }

            public static bool IntersectRayMeshEx(Ray ray, Mesh mesh, Matrix4x4 matrix, out RaycastInfo raycastInfo)
            {
                raycastInfo = default(RaycastInfo);
                raycastInfo.isHitTargetLayer = false;
                raycastInfo.distance = Mathf.Infinity;
                raycastInfo.ray = ray;

                UnityEngine.RaycastHit unityRaycastHit = default(UnityEngine.RaycastHit);


                if (IntersectRayMesh != null &&
                    IntersectRayMesh(ray, mesh, matrix, out unityRaycastHit))
                {
                    raycastInfo.isHitTargetLayer = true;
                    raycastInfo.ray = ray;
                    raycastInfo.distance = unityRaycastHit.distance;

                    raycastInfo.localNormal = matrix.inverse.MultiplyVector(unityRaycastHit.normal).normalized;
                    raycastInfo.localPoint = matrix.inverse.MultiplyPoint(unityRaycastHit.point);

                    raycastInfo.point = unityRaycastHit.point;
                    raycastInfo.normal = matrix.inverse.transpose.MultiplyVector(raycastInfo.localNormal).normalized;

                    return true;
                }

                return false;
            }


            void Raycast(ref RaycastData raycastData, Node node)
            {
                float distance;

                // Raycast node bounds
                if (!node.bounds.IntersectRay(raycastData.ray, out distance))
                    return;

                // Raycast childs
                if (node.childs[0] != null)
                {
                    for (int i = 0; i < 8; i++)
                        Raycast(ref raycastData, node.childs[i]);
                }

                // no objects in node
                if (node.objects == null)
                    return;


                // raycast all objects in leaf
                for (int i = 0; i < node.objects.Count; i++)
                {
                    OctreeObject obj = node.objects[i];

                    if (obj.gameObject == null) // object removed
                        continue;

                    // skip raycasted in current raycast operation
                    if (obj.raycastOpID == s_RaycastOpID)
                        continue;

                    obj.raycastOpID = s_RaycastOpID;

                    // through ignore layers
                    if ((obj.layer & raycastData.ignoreLayersMask) != 0)
                        continue;

                    bool isObjectInList = raycastData.objectList != null && raycastData.objectList.Contains(obj.gameObject);

                    if (raycastData.objectList != null && !isObjectInList)
                        continue;

                    // check object bounds and distance
                    if (!obj.bounds.IntersectRay(raycastData.ray, out distance))
                        continue;

                    SortedObject raycastObject;
                    raycastObject.obj = obj;
                    raycastObject.boundsDistance = distance;
                    m_SortedObjects.Add(raycastObject);
                }

            }

            void RaycastDynamic(ref RaycastData raycastData)
            {
                float distance;

                if (m_DynamicObjects == null)
                    return;

                for (int i = 0; i < m_DynamicObjects.Count; i++)
                {
                    OctreeObject obj = m_DynamicObjects[i];

                    if (obj.gameObject == null) // object removed
                        continue;

                    // through ignore layers
                    if ((obj.layer & raycastData.ignoreLayersMask) != 0)
                        continue;

                    bool isObjectInList = raycastData.objectList != null && raycastData.objectList.Contains(obj.gameObject);

                    if (raycastData.objectList != null && !isObjectInList)
                        continue;

                    Bounds bounds;

                    if (obj.renderer != null)
                        bounds = obj.renderer.bounds;
                    else if (obj.collider != null)
                        bounds = obj.collider.bounds;
                    else if (obj.rectTransform != null)
                    {
                        obj.rectTransform.GetWorldCorners(obj.cornerPoints);
                        bounds = new Bounds(obj.cornerPoints[0], Vector3.zero);
                        bounds.Encapsulate(obj.cornerPoints[1]);
                        bounds.Encapsulate(obj.cornerPoints[2]);
                        bounds.Encapsulate(obj.cornerPoints[3]);
                    }
                    else
                        continue;

                    // check object bounds and distance
                    if (!bounds.IntersectRay(raycastData.ray, out distance))
                        continue;

                    SortedObject raycastObject;
                    raycastObject.obj = obj;
                    raycastObject.boundsDistance = distance;
                    m_SortedObjects.Add(raycastObject);
                }
            }

            void SortedRaycast(ref RaycastData raycastData)
            {
                for(int i = 0; i < m_SortedObjects.Count; i++)
                {
                    OctreeObject obj = m_SortedObjects[i].obj;

                    // check bounds distance
                    if (m_SortedObjects[i].boundsDistance > raycastData.raycastInfo.distance)
                        return;

                    RaycastInfo raycastInfo = default(RaycastInfo);
                    if (obj.mesh != null)
                    {
                        if (IntersectRayMeshEx(raycastData.ray, obj.mesh, obj.gameObject.transform.localToWorldMatrix, out raycastInfo))
                        {
                            if (raycastInfo.distance < raycastData.raycastInfo.distance)
                            {
                                raycastData.gameObject = obj.gameObject;
                                raycastData.raycastInfo = raycastInfo;
                            }
                        }

                        intersectRayMeshCounter++;
                    }
                    else if (obj.collider != null)
                    {
                        UnityEngine.RaycastHit unityRaycastHit;

                        // NOTE: do not use Mathf.Infinity, strange bug
                        if (obj.collider.Raycast(raycastData.ray, out unityRaycastHit, 100000.0f))
                        {
                            if (unityRaycastHit.distance < raycastData.raycastInfo.distance)
                            {
                                raycastInfo.ray = raycastData.ray;
                                raycastInfo.isHitTargetLayer = true;
                                raycastInfo.isHitMaskedLayer = false;
                                raycastInfo.distance = unityRaycastHit.distance;
                                raycastInfo.point = unityRaycastHit.point;
                                raycastInfo.normal = unityRaycastHit.normal;

                                raycastData.gameObject = obj.gameObject;
                                raycastData.raycastInfo = raycastInfo;
                            }
                        }
                    }
                    else if (obj.rectTransform != null)
                    {
                        float u = 0, v = 0, t = 0;

                        if (RaycastTriangle(raycastData.ray, obj.cornerPoints[0], obj.cornerPoints[1], obj.cornerPoints[2], out u, out v, out t) ||
                           RaycastTriangle(raycastData.ray, obj.cornerPoints[0], obj.cornerPoints[2], obj.cornerPoints[3], out u, out v, out t))
                        {
                            if (t < raycastData.raycastInfo.distance)
                            {
                                raycastInfo.ray = raycastData.ray;
                                raycastInfo.isHitTargetLayer = true;
                                raycastInfo.isHitMaskedLayer = false;
                                raycastInfo.distance = t;
                                raycastInfo.point = raycastData.ray.GetPoint(t);
                                raycastInfo.normal = (new Plane(obj.cornerPoints[0], obj.cornerPoints[1], obj.cornerPoints[2])).normal;

                                raycastData.gameObject = obj.gameObject;
                                raycastData.raycastInfo = raycastInfo;
                            }
                        }
                    }

                    bool isMasked = (obj.layer & raycastData.layersMask) == 0;

                    if (raycastInfo.isHitTargetLayer && isMasked && raycastData.objectList == null)
                    {
                        raycastData.raycastInfo.isHitTargetLayer = false;
                        raycastData.raycastInfo.isHitMaskedLayer = true;
                    }
                }
            }

            public bool Raycast(Ray ray, out RaycastInfo raycastInfo, int layersMask, int ignoreLayersMask, List<GameObject> objectList)
            {
                s_RaycastOpID++;
                raycastCounter++;

                RaycastData raycastData = new RaycastData();
                raycastData.raycastInfo = new RaycastInfo();
                raycastData.ray = ray;
                raycastData.layersMask = layersMask;
                raycastData.objectList = objectList;
                raycastData.ignoreLayersMask = ignoreLayersMask;
                raycastData.raycastInfo.distance = float.PositiveInfinity;
                raycastData.gameObject = null;


                Raycast(ref raycastData, m_Tree);
                RaycastDynamic(ref raycastData);

                m_SortedObjects.Sort(delegate (SortedObject x, SortedObject y)
                {
                    if(x.boundsDistance < y.boundsDistance)
                        return -1;
                    return 1;
                });

                SortedRaycast(ref raycastData);

                m_SortedObjects.Clear();

                if (raycastData.gameObject != null)
                {
                    raycastInfo = raycastData.raycastInfo;
                    raycastInfo.hitObject = raycastData.gameObject;

                    // if we hit backwards - reverse normal
                    if (Vector3.Dot(ray.direction, raycastInfo.normal) > 0.0f)
                    {
                        raycastInfo.normal = -raycastInfo.normal;
                        raycastInfo.localNormal = -raycastInfo.localNormal;
                    }

                    return true;
                }


                raycastInfo = new RaycastInfo();
                return false;
            }


            struct IntersectSphereData
            {
                public Vector3 spherePoint;
                public float sphereRadiusSq;
                public Func<GameObject, bool> func;
            }


            bool IntersectSphere(ref IntersectSphereData data, Node node)
            {
                if (node.bounds.SqrDistance(data.spherePoint) > data.sphereRadiusSq)
                    return true;

                // Raycast childs
                if (node.childs[0] != null)
                {
                    for (int i = 0; i < 8; i++)
                        if (!IntersectSphere(ref data, node.childs[i]))
                            return false;
                }

                // no objects in leaf
                if (node.objects == null)
                    return true;

                for (int i = 0; i < node.objects.Count; i++)
                {
                    OctreeObject obj = node.objects[i];

                    if (obj.gameObject == null) // object removed
                        continue;

                    // skip raycasted in current raycast operation
                    if (obj.raycastOpID == s_RaycastOpID)
                        continue;

                    obj.raycastOpID = s_RaycastOpID;

                    if (obj.bounds.SqrDistance(data.spherePoint) < data.sphereRadiusSq)
                    {
                        if (!data.func.Invoke(obj.gameObject))
                            return false;
                    }

                }

                return true;
            }

            void IntersectSphereDynamic(ref IntersectSphereData data)
            {
                if (m_DynamicObjects == null)
                    return;

                for (int i = 0; i < m_DynamicObjects.Count; i++)
                {
                    OctreeObject obj = m_DynamicObjects[i];

                    if (obj.gameObject == null) // object removed
                        continue;

                    Bounds bounds;

                    if (obj.renderer != null)
                        bounds = obj.renderer.bounds;
                    else if (obj.collider != null)
                        bounds = obj.collider.bounds;
                    else if (obj.rectTransform != null)
                    {
                        obj.rectTransform.GetWorldCorners(obj.cornerPoints);
                        bounds = new Bounds(obj.cornerPoints[0], Vector3.zero);
                        bounds.Encapsulate(obj.cornerPoints[1]);
                        bounds.Encapsulate(obj.cornerPoints[2]);
                        bounds.Encapsulate(obj.cornerPoints[3]);
                    }
                    else
                        continue;

                    if (bounds.SqrDistance(data.spherePoint) < data.sphereRadiusSq)
                    {
                        if (!data.func.Invoke(obj.gameObject))
                            return;
                    }
                }
            }

            public void IntersectSphere(Vector3 point, float radius, Func<GameObject, bool> func)
            {
                if (func == null)
                    return;

                s_RaycastOpID++;

                IntersectSphereData data = new IntersectSphereData();
                data.spherePoint = point;
                data.sphereRadiusSq = radius * radius;
                data.func = func;

                IntersectSphere(ref data, m_Tree);
                IntersectSphereDynamic(ref data);
            }


            struct IntersectBoundsData
            {
                public Bounds bounds;
                public Func<GameObject, bool> func;
            }

            bool IntersectBounds(ref IntersectBoundsData data, Node node)
            {
                if (!node.bounds.Intersects(data.bounds))
                    return true;

                // Raycast childs
                if (node.childs[0] != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (!IntersectBounds(ref data, node.childs[i]))
                            return false;
                    }
                }

                // no objects in leaf
                if (node.objects == null)
                    return true;

                for (int i = 0; i < node.objects.Count; i++)
                {
                    OctreeObject obj = node.objects[i];

                    if (obj.gameObject == null) // object removed
                        continue;

                    // skip raycasted in current raycast operation
                    if (obj.raycastOpID == s_RaycastOpID)
                        continue;

                    obj.raycastOpID = s_RaycastOpID;

                    if (obj.bounds.Intersects(data.bounds))
                    {
                        if (!data.func.Invoke(obj.gameObject))
                            return false;
                    }
                }

                return true;
            }

            void IntersectBoundsDynamic(ref IntersectBoundsData data)
            {
                if (m_DynamicObjects == null)
                    return;

                for (int i = 0; i < m_DynamicObjects.Count; i++)
                {
                    OctreeObject obj = m_DynamicObjects[i];

                    if (obj.gameObject == null) // object removed
                        continue;

                    Bounds bounds;

                    if (obj.renderer != null)
                        bounds = obj.renderer.bounds;
                    else if (obj.collider != null)
                        bounds = obj.collider.bounds;
                    else if (obj.rectTransform != null)
                    {
                        obj.rectTransform.GetWorldCorners(obj.cornerPoints);
                        bounds = new Bounds(obj.cornerPoints[0], Vector3.zero);
                        bounds.Encapsulate(obj.cornerPoints[1]);
                        bounds.Encapsulate(obj.cornerPoints[2]);
                        bounds.Encapsulate(obj.cornerPoints[3]);
                    }
                    else
                        continue;

                    if (bounds.Intersects(data.bounds))
                    {
                        if (!data.func.Invoke(obj.gameObject))
                            return;
                    }
                }
            }

            public void IntersectBounds(Bounds bounds, Func<GameObject, bool> func)
            {
                if (func == null)
                    return;

                s_RaycastOpID++;

                IntersectBoundsData data = new IntersectBoundsData();
                data.bounds = bounds;
                data.func = func;

                IntersectBounds(ref data, m_Tree);
                IntersectBoundsDynamic(ref data);
            }

        } // class Octree




        public static class Utility
        {
            public static bool IsVector2Equal(Vector2 a, Vector2 b, float epsilon = 0.001f)
            {
                return Mathf.Abs(a.x - b.x) < epsilon && Mathf.Abs(a.y - b.y) < epsilon;
            }

            public static bool IsVector3Equal(Vector3 a, Vector3 b, float epsilon = 0.001f)
            {
                return Mathf.Abs(a.x - b.x) < epsilon && Mathf.Abs(a.y - b.y) < epsilon && Mathf.Abs(a.z - b.z) < epsilon;
            }

            public static Vector3 RoundVector(Vector3 v, int digits)
            {
                return new Vector3((float)Math.Round(v.x, digits), (float)Math.Round(v.y, digits), (float)Math.Round(v.z, digits));
            }

            public static float Round(float v, int digits)
            {
                return (float)Math.Round(v, digits);
            }

            public static float ToAngle360(float angle)
            {
                angle %= 360;
                return angle < 0 ? 360 + angle : angle;
            }

            public static Vector3 ToAngle360(Vector3 euler)
            {
                return new Vector3(ToAngle360(euler.x), ToAngle360(euler.y), ToAngle360(euler.z));
            }

            public static string TruncateString(string str, GUIStyle style, int maxWidth)
            {
                GUIContent ellipsis = new GUIContent("...");
                string shortStr = "";

                float ellipsisSize = style.CalcSize(ellipsis).x;
                GUIContent textContent = new GUIContent("");

                char[] charArray = str.ToCharArray();
                for (int i = 0; i < charArray.Length; i++)
                {
                    textContent.text += charArray[i];

                    float size = style.CalcSize(textContent).x;

                    if (size > maxWidth - ellipsisSize)
                    {
                        shortStr += ellipsis.text;
                        break;
                    }

                    shortStr += charArray[i];
                }

                return shortStr;
            }

            public static void ForAllInHierarchy(GameObject gameObject, Action<GameObject> action)
            {
                action(gameObject);

                for (int i = 0; i < gameObject.transform.childCount; i++)
                    ForAllInHierarchy(gameObject.transform.GetChild(i).gameObject, action);
            }

            public static void MarkActiveSceneDirty()
            {
                UnityEngine.SceneManagement.Scene activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
            }


        } // class Utility





        #region Variables

        enum PaintTool
        {
            None = -1,
            Brush = 0,
            Pin = 1,
            Place = 2,
            Throw = 3,
            Erase = 4,
            Select = 5,
            Move = 6,
            Modify = 7,
            Orient = 8,
            Settings = 9,

            // Special Tools
            PickObject = 20,
        }

        PaintTool _LastTool = PaintTool.Brush;
        PaintTool m_LastTool
        {
            get { return _LastTool; }
            set
            {
                if (value != PaintTool.None && value != PaintTool.Settings && value != PaintTool.PickObject)
                    _LastTool = value;
            }
        }
        PaintTool _CurrentTool = PaintTool.None;
        PaintTool m_CurrentTool
        {
            get { return _CurrentTool; }
            set
            {
                if (_CurrentTool == value)
                    return;

                OnToolDisabled(_CurrentTool);

                _CurrentTool = value;
                m_LastTool = value;

                OnToolEnabled(_CurrentTool);
            }
        }






        class PlacedObjectInfo
        {
            public RaycastInfo raycastInfo;
            public GameObject pivotObject;
            public GameObject gameObject;
            public Bounds localBounds;
            public Brush brush;
            public int prefabSlot;
        }

        class BrushTool
        {
            public RaycastInfo raycastInfo;
            public RaycastInfo prevRaycast;
            public float dragDistance;
            public Vector3 strokeDirection;
            public Vector3 strokeDirectionRefPoint;
            public PlacedObjectInfo lastPlacedObjectInfo;
            public Vector3 firstNormal;
        }

        class ThrowTool
        {
            public RaycastInfo raycastInfo;
            public RaycastInfo prevRaycast;
            public float dragDistance;

            public List<Rigidbody> m_rigidbodys;
            public List<Rigidbody2D> m_rigidbody2Ds;

            public bool noRigidBody;
            
            #if UNITY_2022_2_OR_NEWER
            public SimulationMode lastSimulationMode;
            #endif
        }

        class PinTool
        {
            public PlacedObjectInfo placedObjectInfo;

            public float scaleFactor;
            public float angle;
            public float radius;

            public Vector3 point;
            public Vector3 right;
            public Vector3 upwards;
            public Vector3 forward;

            public bool IntersectsHitPlane(Ray ray, out Vector3 hitPoint)
            {
                float rayDistance;
                Plane plane = new Plane(upwards, point);
                if (plane.Raycast(ray, out rayDistance))
                {
                    hitPoint = ray.GetPoint(rayDistance);
                    return true;
                }
                hitPoint = Vector3.zero;
                return false;
            }
        }

        class PlaceTool
        {
            public PlacedObjectInfo placedObjectInfo;
            public RaycastInfo raycastInfo;

            public Vector3 right;
            public Vector3 upwards;
            public Vector3 forward;

        }

        class EraseTool
        {
            public List<GameObject> prefabList = new List<GameObject>();
        }

        class SelectionTool
        {
            public List<GameObject> prefabList = new List<GameObject>();
            public List<GameObject> selectedObjects = new List<GameObject>();
        }

        class ModifyTool
        {
            public struct ModifyInfo
            {
                public Vector3 pivot;
                public Vector3 initialPosition;
                public Quaternion initialRotation;
                public Vector3 initialScale;
                public Vector3 initialUp;

                public Vector3 randomRotation;
                public float randomScale;
                public float currentScale;

                public int lastUpdate;
            }

            public List<GameObject> prefabList = new List<GameObject>();
            public Dictionary<GameObject, ModifyInfo> modifiedObjects = new Dictionary<GameObject, ModifyInfo>();
            public int updateTicks;
        }


        class OrientTool
        {
            public struct ObjectInfo
            {
                public Vector3 initialPosition;
                public Quaternion initialRotation;
                public Vector3 pivot;
                public Vector3 initialUp;
                public GameObject prefabRoot;
            }

            public List<ObjectInfo> objects = new List<ObjectInfo>();
            public Vector3 objectsCenter;
        }

        class MoveTool
        {
            public struct ObjectInfo
            {
                public Vector3 initialPosition;
                public Quaternion initialRotation;
                public Vector3 pivot;
                public Vector3 initialUp;
                public Vector3 initialForward;
                public GameObject prefabRoot;
            }

            public float handleDiskSize;
            public Vector3 dragStart;
            public RaycastInfo lastHitRaycast;


            public List<ObjectInfo> objects = new List<ObjectInfo>();
            public Vector3 initialObjectsCenter;
            public Vector3 objectsCenter;
        }

        class Grid
        {
            public const int kSize = 10;
            public const float kDeadZoneSize = 1.2f;

            public RaycastInfo originRaycastInfo;
            public Vector3 visualOrigin;
            public bool inDeadZone = false;

            /*public struct Point
            {
                public bool hit;
                public Vector3 point;
            }

            public Point[,] points = new Point[kSize, kSize];*/
        }


        // Tools data
        BrushTool m_BrushTool = new BrushTool();
        PinTool m_PinTool = new PinTool();
        PlaceTool m_PlaceTool = new PlaceTool();
        EraseTool m_EraseTool = new EraseTool();
        SelectionTool m_SelectionTool = new SelectionTool();
        ModifyTool m_ModifyTool = new ModifyTool();
        OrientTool m_OrientTool = new OrientTool();
        MoveTool m_MoveTool = new MoveTool();
        ThrowTool m_ThrowTool = new ThrowTool();
        Grid m_Grid = new Grid();

        Action<RaycastInfo> m_OnPickObjectAction = null;
        string m_OnPickObjectMessage = "";


        const int kOctreeDepth = 4;
        Octree m_Octree = null;

        RaycastInfo m_CurrentRaycast;

        Stage m_CurrentStage;
        Scene m_CurrentStageScene;

        float m_LastPhysicsUpdateTime = 0;

        //float m_EditorUpdateLastTime;
        //float m_EditorUpdateTimeDelta;

        static int s_BrushToolHash = "nTools.PrefabPainter.BrushTool".GetHashCode();
        static int s_PinToolHash = "nTools.PrefabPainter.PinTool".GetHashCode();
        static int s_ThrowToolHash = "nTools.PrefabPainter.ThrowTool".GetHashCode();
        static int s_PlaceToolHash = "nTools.PrefabPainter.PlaceTool".GetHashCode();
        static int s_EraseToolHash = "nTools.PrefabPainter.EraseTool".GetHashCode();
        static int s_SelectToolHash = "nTools.PrefabPainter.SelectTool".GetHashCode();
        static int s_ModifyToolHash = "nTools.PrefabPainter.ModifyTool".GetHashCode();
        static int s_OrientToolHash = "nTools.PrefabPainter.OrientTool".GetHashCode();
        static int s_MoveToolHash = "nTools.PrefabPainter.SlideTool".GetHashCode();
        static int s_PickObjectToolHash = "nTools.PrefabPainter.PickObjectTool".GetHashCode();
        static PrefabPainter s_ActiveWindow;


        // Database
        PrefabPainterSettings m_Settings;
        PrefabPainterSceneSettings m_SceneSettings;


        string m_WorkDirectoryPath = null;
        const string kGUIDirectoryName = "GUI";
        const string kSettingsDirectoryName = "Settings";
        const string kPresetsDirectoryName = "Presets";
        const string kSettingsFileName = "settings.asset";
        const string kDefaultSettingsFileName = "defaultSettings.asset";
        const string kSettingsObjectName = "PrefabPainterSceneSettings";


        //
        // Selected objects
        UnityEngine.Object[] _SelectedObjects = null;
        List<GameObject> _SelectedGameObjects = null;

        List<GameObject> m_SelectedGameObjects
        {
            get { return _SelectedGameObjects; }
        }

        UnityEngine.Object[] m_SelectedObjects
        {
            get
            {
                return _SelectedObjects;
            }

            set
            {
                _SelectedObjects = value;

                if (_SelectedObjects == null)
                {
                    _SelectedGameObjects = null;
                }
                else
                {
                    _SelectedGameObjects = new List<GameObject>(_SelectedObjects.Length);

                    foreach (UnityEngine.Object obj in _SelectedObjects)
                    {
                        if (obj is GameObject && ((GameObject)obj).activeInHierarchy)
                        {
                            _SelectedGameObjects.Add((GameObject)obj);
                        }
                    }
                }
            }
        }

        #endregion // Variables



        void RegisterCreatedObjectUndo(UnityEngine.Object objectToUndo, string name)
        {
            if (!m_Settings.disableUndo)
            {
                UnityEditor.Undo.RegisterCreatedObjectUndo(objectToUndo, name);
            }
        }

        void DestroyObjectImmediate(UnityEngine.Object objectToUndo)
        {
            if (!m_Settings.disableUndo)
            {
                UnityEditor.Undo.DestroyObjectImmediate(objectToUndo);
            }
            else
            {
                GameObject.DestroyImmediate(objectToUndo);
            }
        }


        #region Initialization

        // Unity Editor Menu Item
        [MenuItem("Window/nTools/Prefab Painter")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            PrefabPainter window = (PrefabPainter)EditorWindow.GetWindow(typeof(PrefabPainter));
            window.ShowUtility();
        }


        public string GetWorkDirectory()
        {
            if (m_WorkDirectoryPath != null)
                return m_WorkDirectoryPath;

            MonoScript ownerScript;
            string ownerPath;

            // work dir based on PrefabPainter.cs script path.
            // get PrefabPainter.cs script
            if ((ownerScript = MonoScript.FromScriptableObject(this)) != null)
            {
                // get .../PrefabPainter/Scripts/Editor/PrefabPainter.cs
                if ((ownerPath = AssetDatabase.GetAssetPath(ownerScript)) != null)
                {
                    // get .../PrefabPainter/Scripts/Editor/
                    ownerPath = Path.GetDirectoryName(ownerPath);
                    // get .../PrefabPainter/Scripts/
                    ownerPath = Path.GetDirectoryName(ownerPath);
                    // get .../PrefabPainter/
                    m_WorkDirectoryPath = Path.GetDirectoryName(ownerPath);
                }
            }

            return m_WorkDirectoryPath;
        }

        public string GetGUIDirectory()
        {
            string directory = Path.Combine(GetWorkDirectory(), kGUIDirectoryName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }
            return directory;
        }

        public string GetSettingsDirectory()
        {
            string directory = Path.Combine(GetWorkDirectory(), kSettingsDirectoryName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }
            return directory;
        }

        public string GetPresetsDirectory()
        {
            string directory = Path.Combine(GetWorkDirectory(), kPresetsDirectoryName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }
            return directory;
        }


        void LoadSceneSettings()
        {
            GameObject gameObject = GameObject.Find(kSettingsObjectName);
            if (gameObject == null)
            {
                gameObject = new GameObject(kSettingsObjectName);
                Utility.MarkActiveSceneDirty();
            }

            m_SceneSettings = gameObject.GetComponent<PrefabPainterSceneSettings>();
            if (m_SceneSettings == null)
            {
                m_SceneSettings = gameObject.AddComponent<PrefabPainterSceneSettings>();
                Utility.MarkActiveSceneDirty();
            }

            HideFlags hideFlags = m_Settings.hideSceneSettingsObject ? (HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSaveInBuild) : (HideFlags.DontSaveInBuild);

            if (gameObject.hideFlags != hideFlags)
            {
                gameObject.hideFlags = hideFlags;
                Utility.MarkActiveSceneDirty();
                EditorApplication.RepaintHierarchyWindow();
            }
        }



        PrefabPainterSettings LoadSettings()
        {
            string settingsDirectoryPath = GetSettingsDirectory();

            // Try load settings asset
            PrefabPainterSettings settings = AssetDatabase.LoadAssetAtPath(Path.Combine(settingsDirectoryPath, kSettingsFileName),
                typeof(PrefabPainterSettings)) as PrefabPainterSettings;
            if (settings == null)
            {
                // if no settings file, try load default settings file
                settings = AssetDatabase.LoadAssetAtPath(Path.Combine(settingsDirectoryPath, kDefaultSettingsFileName),
                    typeof(PrefabPainterSettings)) as PrefabPainterSettings;
                if (settings != null)
                {
                    // Duplicate
                    settings = Instantiate(settings);

                    // Save as settingsFileName
                    AssetDatabase.CreateAsset(settings, Path.Combine(settingsDirectoryPath, kSettingsFileName));
                }
                else
                // if no default settings file - create new instance
                {
                    settings = ScriptableObject.CreateInstance<PrefabPainterSettings>();

                    // Save as settingsFileName
                    AssetDatabase.CreateAsset(settings, Path.Combine(settingsDirectoryPath, kSettingsFileName));
                }
            }

            PrefabPainterSettings.current = settings;

            return settings;
        }




        void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;

            s_ActiveWindow = this;


            m_Settings = LoadSettings();
            LoadSceneSettings();


            m_CurrentTool = PaintTool.None;

            // Initialize Octree
            m_Octree = new Octree(kOctreeDepth);


            OnInitGUI();

            //m_EditorUpdateLastTime = Time.realtimeSinceStartup;


            // Setup callbacks
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
            EditorApplication.update += EditorApplicationUpdateCallback;
            UnityEditor.Undo.undoRedoPerformed += UndoRedoPerformedCallback;
            EditorApplication.modifierKeysChanged += ModifierKeysChangedCallback;

            /*{
                FieldInfo globalEventHandlerFiledInfo = typeof(EditorApplication).GetField("globalEventHandler",
                                                            BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                if(globalEventHandlerFiledInfo != null)
                {
                    EditorApplication.CallbackFunction callback = (EditorApplication.CallbackFunction)globalEventHandlerFiledInfo.GetValue(null);
                    callback += GlobalEventHandler;
                    globalEventHandlerFiledInfo.SetValue(null, (object)callback);
                }
            }*/

        }



        void OnDisable()
        {
            m_CurrentTool = PaintTool.None;

            OnCleanupGUI();

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
            EditorApplication.update -= EditorApplicationUpdateCallback;
            UnityEditor.Undo.undoRedoPerformed -= UndoRedoPerformedCallback;
            EditorApplication.modifierKeysChanged -= ModifierKeysChangedCallback;

            s_ActiveWindow = null;

            EditorUtility.SetDirty(m_Settings);
        }


        void GlobalEventHandler()
        {
            Event e = Event.current;

            /*
            if (m_Settings.enableToolsShortcuts && e.isKey &&
#if UNITY_EDITOR_OSX
            IsModifierDown(EventModifiers.Command)
#else
            IsModifierDown(EventModifiers.Control)
#endif
                )
            {
                switch (e.keyCode)
                {
                case KeyCode.B:
                    m_CurrentTool = PaintTool.Brush;
                    Repaint();
                    e.Use();
                    break;
                case KeyCode.P:
                    m_CurrentTool = PaintTool.Pin;
                    Repaint();
                    e.Use();
                    break;
                case KeyCode.L:
                    m_CurrentTool = PaintTool.Place;
                    Repaint();
                    e.Use();
                    break;
                case KeyCode.A:
                    m_CurrentTool = PaintTool.Erase;
                    Repaint();
                    e.Use();
                    break;
                case KeyCode.S:
                    m_CurrentTool = PaintTool.Select;
                    Repaint();
                    e.Use();
                    break;
                case KeyCode.M:
                    m_CurrentTool = PaintTool.Modify;
                    Repaint();
                    e.Use();
                    break;
                }
            }*/
        }

        void UndoRedoPerformedCallback()
        {
            switch (m_CurrentTool)
            {
            case PaintTool.Move:
                {
                    MoveToolReloadSelection();
                }
                break;
            }

            Repaint();
        }



        void EditorApplicationUpdateCallback()
        {
            //m_EditorUpdateTimeDelta = Time.realtimeSinceStartup - m_EditorUpdateLastTime;
            //m_EditorUpdateLastTime = Time.realtimeSinceStartup;

            if (Tools.current != Tool.None && (m_CurrentTool != PaintTool.None && m_CurrentTool != PaintTool.Settings))
            {
                m_CurrentTool = PaintTool.None;
                Repaint();
            }


            if (Time.realtimeSinceStartup - m_LastUIRepaintTime > kUIRepaintInterval)
            {
                m_LastUIRepaintTime = Time.realtimeSinceStartup;
                Repaint();
            }

            if(m_CurrentTool == PaintTool.Throw)
            {
                if (Time.realtimeSinceStartup - m_LastPhysicsUpdateTime > m_Settings.throwPhysicsTimeStep)
                {
                    float timeElapsed = Time.realtimeSinceStartup - m_LastPhysicsUpdateTime;
                    m_LastPhysicsUpdateTime = Time.realtimeSinceStartup;

                    int steps = 0;
                    while (timeElapsed > m_Settings.throwPhysicsTimeStep && steps < 20)
                    {
                        Physics.Simulate(m_Settings.throwPhysicsTimeStep);
                        timeElapsed -= m_Settings.throwPhysicsTimeStep;
                        steps++;
                    }

                    SceneView.RepaintAll();
                }
            }

            // Lose scene settings - lose scene
            if (m_SceneSettings == null)
            {
                m_CurrentTool = PaintTool.None;
                m_SelectedObjects = null;
                m_Octree.Cleanup();

                LoadSceneSettings();
                Repaint();
            }
        }



        void ModifierKeysChangedCallback()
        {
            Repaint();
        }


        void OnSelectionChange()
        {
            switch(m_CurrentTool)
            {
            case PaintTool.Move:
                {
                    MoveToolReloadSelection();
                }
                break;
            }
        }

        #endregion // Initialization




        #region Deprecated stuff


#if UNITY_2018_2_OR_NEWER
        public static UnityEngine.Object GetCorrespondingObjectFromSource(UnityEngine.Object source)
        {
            return PrefabUtility.GetCorrespondingObjectFromSource(source);
        }
#else
        public static UnityEngine.Object GetCorrespondingObjectFromSource(UnityEngine.Object source)
        {
            return PrefabUtility.GetPrefabParent(source);
        }
#endif

#if (UNITY_2018_3_OR_NEWER)
        public static bool IsAcceptablePrefab(UnityEngine.Object obj)
        {
            if (obj != null && obj is GameObject)
            {
                PrefabAssetType type = PrefabUtility.GetPrefabAssetType(obj);
                if (type == PrefabAssetType.Regular || type == PrefabAssetType.Variant || type == PrefabAssetType.Model)
                {
                    return AssetDatabase.Contains(obj);
                }
            }
            return false;
        }

        public static GameObject GetPrefabRoot(GameObject gameObject)
        {
            if(PrefabUtility.GetPrefabAssetType(gameObject) == PrefabAssetType.NotAPrefab)
            {
                return gameObject;
            }

            return PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
        }
#else
        public static bool IsAcceptablePrefab(UnityEngine.Object obj)
        {
            return obj != null &&
                   obj is GameObject &&
                   PrefabUtility.GetPrefabType(obj as GameObject) != PrefabType.None &&
                   AssetDatabase.Contains(obj);
        }

        public static GameObject GetPrefabRoot(GameObject gameObject)
        {
            return PrefabUtility.FindPrefabRoot(gameObject);
        }
#endif


        #endregion // Deprecated stuff






        #region Object Placement

        bool Raycast(Ray ray, out RaycastInfo raycastInfo, int layersMask, int ignoreLayersMask)
        {
            raycastInfo = new RaycastInfo();

            if (m_Octree == null)
                return false;

            if (m_Settings.paintOnSelected && (m_CurrentTool == PaintTool.Brush || m_CurrentTool == PaintTool.Pin || m_CurrentTool == PaintTool.Place))
            {
                if (m_SelectedObjects == null)
                    return false;

                return m_Octree.Raycast(ray, out raycastInfo, ~0, 0, _SelectedGameObjects);
            }

            return m_Octree.Raycast(ray, out raycastInfo, layersMask, ignoreLayersMask, null);
        }


        Quaternion OrientObject(OrientationMode orientationMode, Vector3 normal, bool isRectTransform)
        {
            Vector3 right;
            Vector3 forward;
            Vector3 upwards;

            switch (orientationMode)
            {
            default:
            case OrientationMode.SurfaceNormal:
            case OrientationMode.SurfaceNormalNegative:
                {
                    upwards = normal;

                    if (orientationMode == OrientationMode.SurfaceNormalNegative)
                        upwards = -upwards;

                    GetRightForward(upwards, out right, out forward);
                    if (isRectTransform)
                    {
                        upwards = forward;
                        forward = -normal;
                    }
                }
                break;
            case OrientationMode.X:
            case OrientationMode.XNegative:
                {
                    upwards = new Vector3(1, 0, 0);
                    if (orientationMode == OrientationMode.XNegative)
                        upwards = -upwards;
                    GetRightForward(upwards, out right, out forward);

                }
                break;
            case OrientationMode.Y:
            case OrientationMode.YNegative:
                {
                    upwards = new Vector3(0, 1, 0);
                    if (orientationMode == OrientationMode.YNegative)
                        upwards = -upwards;
                    GetRightForward(upwards, out right, out forward);

                }
                break;
            case OrientationMode.Z:
            case OrientationMode.ZNegative:
                {
                    upwards = new Vector3(0, 0, 1);
                    if (orientationMode == OrientationMode.ZNegative)
                        upwards = -upwards;
                    GetRightForward(upwards, out right, out forward);
                }
                break;
            }

            return Quaternion.LookRotation(forward, upwards);
        }

        void OrientObject(PlacedObjectInfo placedObjectInfo, Vector3 euler)
        {
            Transform transform = placedObjectInfo.pivotObject.transform;
            bool isRectTransform = placedObjectInfo.gameObject.transform is RectTransform;
            Vector3 normal = placedObjectInfo.raycastInfo.normal;
            BrushSettings brushSettings = placedObjectInfo.brush.settings;
            int prefabSlot = placedObjectInfo.prefabSlot;

            if (brushSettings.multibrushEnabled)
                euler += brushSettings.multibrushSlots[prefabSlot].rotation;

            Quaternion placeOrientation = OrientObject(brushSettings.orientationMode, normal, isRectTransform);
            transform.rotation = placeOrientation * Quaternion.Euler(euler);
        }

        void BrushModeOrientObject(PlacedObjectInfo placedObjectInfo)
        {
            Transform transform = placedObjectInfo.pivotObject.transform;
            bool isRectTransform = placedObjectInfo.gameObject.transform is RectTransform;
            Vector3 normal = placedObjectInfo.raycastInfo.normal;
            BrushSettings brushSettings = placedObjectInfo.brush.settings;
            int prefabSlot = placedObjectInfo.prefabSlot;

            Quaternion placeOrientation = Quaternion.identity;
            Quaternion randomRotation = Quaternion.identity;
            Vector3 rotation = Vector3.zero;


            // Place orientation
            {
                Vector3 right;
                Vector3 forward;
                Vector3 upwards;

                switch (brushSettings.orientationMode)
                {
                default:
                case OrientationMode.SurfaceNormal:
                case OrientationMode.SurfaceNormalNegative:
                    {
                        upwards = normal;

                        if (brushSettings.orientationMode == OrientationMode.SurfaceNormalNegative)
                            upwards = -upwards;

                        GetRightForward(upwards, out right, out forward);
                        if (isRectTransform)
                        {
                            upwards = forward;
                            forward = -normal;
                        }
                    }
                    break;
                case OrientationMode.X:
                case OrientationMode.XNegative:
                    {
                        upwards = new Vector3(1, 0, 0);
                        if (brushSettings.orientationMode == OrientationMode.XNegative)
                            upwards = -upwards;
                        GetRightForward(upwards, out right, out forward);

                    }
                    break;
                case OrientationMode.Y:
                case OrientationMode.YNegative:
                    {
                        upwards = new Vector3(0, 1, 0);
                        if (brushSettings.orientationMode == OrientationMode.YNegative)
                            upwards = -upwards;
                        GetRightForward(upwards, out right, out forward);

                    }
                    break;
                case OrientationMode.Z:
                case OrientationMode.ZNegative:
                    {
                        upwards = new Vector3(0, 0, 1);
                        if (brushSettings.orientationMode == OrientationMode.ZNegative)
                            upwards = -upwards;
                        GetRightForward(upwards, out right, out forward);
                    }
                    break;
                }

                if (brushSettings.alongBrushStroke)
                {
                    Vector3 strokeForward;

                    if (isRectTransform)
                        strokeForward = Vector3.Cross(-upwards, m_BrushTool.strokeDirection);
                    else
                        strokeForward = Vector3.Cross(m_BrushTool.strokeDirection, upwards);

                    if (strokeForward.magnitude > 0.001f)
                        forward = strokeForward;
                }

                placeOrientation = Quaternion.LookRotation(forward, upwards);
            }


            // Random rotation
            Vector3 randomVector = UnityEngine.Random.insideUnitSphere * 0.5f;
            randomRotation = Quaternion.Euler(new Vector3(brushSettings.randomizeOrientationX * 3.6f * randomVector.x,
                brushSettings.randomizeOrientationY * 3.6f * randomVector.y,
                brushSettings.randomizeOrientationZ * 3.6f * randomVector.z));


            rotation = brushSettings.rotation;

            if (brushSettings.multibrushEnabled)
                rotation += brushSettings.multibrushSlots[prefabSlot].rotation;

            transform.rotation = placeOrientation * (randomRotation * Quaternion.Euler(rotation));
        }


        Vector3 GetObjectPivot(GameObject gameObject, PivotMode pivotMode)
        {
            Vector3 pivot;
            Bounds bounds;

            switch (pivotMode)
            {
            case PivotMode.BoundsTopCenter:
                if(!GetObjectLocalBounds(gameObject, out bounds))
                    return gameObject.transform.position;

                pivot = bounds.center + new Vector3(0f, bounds.extents.y, 0f);
                pivot = gameObject.transform.localToWorldMatrix.MultiplyPoint(pivot);
                break;
            case PivotMode.BoundsCenter:
                if (!GetObjectLocalBounds(gameObject, out bounds))
                    return gameObject.transform.position;
                pivot = bounds.center;
                pivot = gameObject.transform.localToWorldMatrix.MultiplyPoint(pivot);
                break;
            case PivotMode.BoundsBottomCenter:
                if (!GetObjectLocalBounds(gameObject, out bounds))
                    return gameObject.transform.position;
                pivot = bounds.center - new Vector3(0f, bounds.extents.y, 0f);
                pivot = gameObject.transform.localToWorldMatrix.MultiplyPoint(pivot);
                break;
            case PivotMode.WorldBoundsTopCenter:
                if (!GetObjectWorldBounds(gameObject, out bounds))
                    return gameObject.transform.position;
                pivot = bounds.center + new Vector3(0f, bounds.extents.y, 0f);
                break;
            case PivotMode.WorldBoundsCenter:
                if (!GetObjectWorldBounds(gameObject, out bounds))
                    return gameObject.transform.position;
                pivot = bounds.center;
                break;
            case PivotMode.WorldBoundsBottomCenter:
                if (!GetObjectWorldBounds(gameObject, out bounds))
                    return gameObject.transform.position;
                pivot = bounds.center - new Vector3(0f, bounds.extents.y, 0f);
                break;
            default:
                pivot = gameObject.transform.position;
                break;
            }

            return pivot;
        }


        void PositionObject(PlacedObjectInfo info)
        {
            Transform transform = info.pivotObject.transform;
            BrushSettings brushSettings = info.brush.settings;

            //float surfaceOffset = brushSettings.surfaceOffsetMin + UnityEngine.Random.value * (brushSettings.surfaceOffsetMax - brushSettings.surfaceOffsetMin);

            Vector3 pivot = GetObjectPivot(info.pivotObject, info.brush.settings.multibrushSlots[info.prefabSlot].pivotMode);

            transform.position = info.raycastInfo.point
                - (pivot - transform.position)
                + brushSettings.surfaceOffset * info.raycastInfo.normal;

            if (brushSettings.multibrushEnabled)
            {
                Vector3 offset = brushSettings.multibrushSlots[info.prefabSlot].position;
                transform.position = transform.position + transform.right * offset.x + transform.up * offset.y + transform.forward * offset.z;
            }
        }



        void ScaleObject(PlacedObjectInfo info)
        {
            Transform transform = info.pivotObject.transform;
            BrushSettings brushSettings = info.brush.settings;
            Vector3 randomVector = UnityEngine.Random.insideUnitSphere;
            Vector3 scale;

            randomVector = new Vector3(Mathf.Abs(randomVector.x), Mathf.Abs(randomVector.y), Mathf.Abs(randomVector.z));

            if (brushSettings.scaleMode == AxisMode.Uniform)
            {
                float scaleValue = brushSettings.scaleUniformMin + randomVector.x * (brushSettings.scaleUniformMax - brushSettings.scaleUniformMin);
                scale = new Vector3(scaleValue, scaleValue, scaleValue);
            }
            else
            {
                scale = new Vector3(brushSettings.scalePerAxisMin.x + randomVector.x * (brushSettings.scalePerAxisMax.x - brushSettings.scalePerAxisMin.x),
                    brushSettings.scalePerAxisMin.y + randomVector.y * (brushSettings.scalePerAxisMax.y - brushSettings.scalePerAxisMin.y),
                    brushSettings.scalePerAxisMin.z + randomVector.z * (brushSettings.scalePerAxisMax.z - brushSettings.scalePerAxisMin.z));
            }

            if (brushSettings.multibrushEnabled)
            {
                scale.x = brushSettings.multibrushSlots[info.prefabSlot].scale.x * scale.x;
                scale.y = brushSettings.multibrushSlots[info.prefabSlot].scale.y * scale.y;
                scale.z = brushSettings.multibrushSlots[info.prefabSlot].scale.z * scale.z;
            }

            transform.localScale = scale;
        }



        float SlopeAngle(Brush brush, Vector3 surfaceNormal)
        {
            Vector3 refVector = Vector3.up;
            switch (brush.settings.slopeVector)
            {
            case SlopeVector.X:
                refVector = new Vector3(1, 0, 0);
                break;
            case SlopeVector.Y:
                refVector = new Vector3(0, 1, 0);
                break;
            case SlopeVector.Z:
                refVector = new Vector3(0, 0, 1);
                break;
            case SlopeVector.View:
                if (Camera.current != null)
                    refVector = -Camera.current.transform.forward;
                break;
            case SlopeVector.FirstNormal:
                refVector = m_BrushTool.firstNormal;
                break;
            case SlopeVector.Custom:
                refVector = brush.settings.slopeVectorCustom.normalized;
                break;
            }

            if (brush.settings.slopeVectorFlip)
                refVector = -refVector;

            return Mathf.Acos(Mathf.Clamp01(Vector3.Dot(surfaceNormal, refVector))) * Mathf.Rad2Deg;
        }


        bool SlopeFilter(Brush brush, Vector3 surfaceNormal)
        {
            // Only in brush tool
            if (m_CurrentTool != PaintTool.Brush)
                return true;

            if (!brush.settings.slopeEnabled)
                return true;

            float angle = SlopeAngle(brush, surfaceNormal);

            return angle >= brush.settings.slopeAngleMin && angle <= brush.settings.slopeAngleMax;
        }




        bool CheckOverlap(Brush brush, BrushSettings brushSettings, Bounds bounds)
        {
            bool overlaps = false;
            float distance = brushSettings.brushOverlapDistance;
            int checkLayers = brushSettings.brushOverlapCheckLayers.value;
            List<GameObject> prefabList = null;


            if(brushSettings.brushOverlapCheckObjects == OverlapCheckObjects.SameObjects)
            {
                prefabList = new List<GameObject>(BrushSettings.kNumMultibrushSlots);

                if(brushSettings.multibrushEnabled)
                {
                    for(int i = 0; i < brush.prefabSlots.Length; i++)
                    {
                        if(brush.prefabSlots[i].gameObject != null)
                        {
                            prefabList.Add(brush.prefabSlots[i].gameObject);
                        }
                    }
                }
                else
                {
                    GameObject prefab = brush.GetFirstAssociatedPrefab();
                    if(prefab != null)
                        prefabList.Add(prefab);
                }
            }


            if(brushSettings.brushOverlapCheckMode == OverlapCheckMode.Distance)
            {
                m_Octree.IntersectSphere(bounds.center, distance, (go) =>
                    {
                        if(go == null)
                        {
                            return true;
                        }

                        switch(brushSettings.brushOverlapCheckObjects)
                        {
                        case OverlapCheckObjects.SameObjects:
                            {
                                GameObject prefabRoot = GetPrefabRoot(go);
                                if(prefabRoot == null)
                                    return true;

                                if(!prefabList.Contains(GetCorrespondingObjectFromSource(prefabRoot) as GameObject))
                                    return true;
                            }
                            break;
                        case OverlapCheckObjects.SamePlaceLayer:
                            if(go.layer != m_Settings.prefabPlaceLayer)
                                return true;
                            break;
                        case OverlapCheckObjects.OtherLayers:
                            if(((1 << go.layer) & checkLayers) == 0)
                                return true;
                            break;
                        }

                        if((go.transform.position - bounds.center).magnitude < distance)
                        {
                            overlaps = true;
                            return false;
                        }

                        return true;
                    }
                );
            }
            else if (brushSettings.brushOverlapCheckMode == OverlapCheckMode.Bounds)
            {
                m_Octree.IntersectBounds(bounds,
                    (go) =>
                    {
                        if(go == null)
                        {
                            return true;
                        }

                        switch(brushSettings.brushOverlapCheckObjects)
                        {
                        case OverlapCheckObjects.SameObjects:
                            {
                                GameObject prefabRoot = GetPrefabRoot(go);
                                if (prefabRoot == null)
                                    return true;

                                if(!prefabList.Contains(GetCorrespondingObjectFromSource(prefabRoot) as GameObject))
                                    return true;
                            }
                            break;
                        case OverlapCheckObjects.SamePlaceLayer:
                            if(go.layer != m_Settings.prefabPlaceLayer)
                                return true;
                            break;
                        case OverlapCheckObjects.OtherLayers:
                            if(((1 << go.layer) & checkLayers) == 0)
                                return true;
                            break;
                        }

                        overlaps = true;
                        return false;
                    });
            }

            return overlaps;
        }



        PlacedObjectInfo PlaceObject(RaycastInfo raycastInfo, Brush brush)
        {
            int prefabSlot = brush.GetPrefabSlotForPlace();
            if (prefabSlot == -1 || brush.prefabSlots[prefabSlot].gameObject == null)
                return null;

            GameObject gameObject = PrefabUtility.InstantiatePrefab(brush.prefabSlots[prefabSlot].gameObject) as GameObject;
            if (gameObject == null)
                return null;


            GameObject pivotObject = new GameObject("TemporaryObjectPivot");
            if (pivotObject == null)
                return null;

            if(StageUtility.GetCurrentStage() != StageUtility.GetMainStage())
                StageUtility.PlaceGameObjectInCurrentStage(pivotObject);

            pivotObject.hideFlags = HideFlags.HideInHierarchy;
            pivotObject.transform.position = Vector3.zero;
            pivotObject.transform.rotation = Quaternion.identity;
            pivotObject.transform.localScale = Vector3.one;


            gameObject.transform.position = Vector3.zero;

            if (brush.settings.orientationTransformMode == TransformMode.Absolute)
                gameObject.transform.rotation = Quaternion.identity;

            if (brush.settings.scaleTransformMode == TransformMode.Absolute)
                gameObject.transform.localScale = Vector3.one;


            gameObject.transform.SetParent(pivotObject.transform, true);

            if (m_Settings.overwritePrefabLayer)
            {
                Utility.ForAllInHierarchy(pivotObject, go => { go.layer = m_Settings.prefabPlaceLayer; });
            }


            PlacedObjectInfo placedObjectInfo = new PlacedObjectInfo();
            placedObjectInfo.raycastInfo = raycastInfo;
            placedObjectInfo.gameObject = gameObject;
            placedObjectInfo.pivotObject = pivotObject;
            placedObjectInfo.brush = brush;
            placedObjectInfo.prefabSlot = prefabSlot;

            //
            GetObjectWorldBounds(gameObject, out placedObjectInfo.localBounds);

            return placedObjectInfo;
        }



        PlacedObjectInfo BrushModePlaceObject(RaycastInfo raycastInfo, Brush brush)
        {
            if (!SlopeFilter(brush, raycastInfo.normal))
                return null;

            PlacedObjectInfo placedObjectInfo = PlaceObject(raycastInfo, brush);
            if (placedObjectInfo == null)
                return null;


            BrushModeOrientObject(placedObjectInfo);
            ScaleObject(placedObjectInfo);
            PositionObject(placedObjectInfo);

            if (brush.settings.brushOverlapCheckMode != OverlapCheckMode.None)
            {
                Bounds bounds;

                if (GetObjectWorldBounds(placedObjectInfo.pivotObject, out bounds) && CheckOverlap(brush, brush.settings, bounds))
                {
                    GameObject.DestroyImmediate(placedObjectInfo.gameObject);
                    GameObject.DestroyImmediate(placedObjectInfo.pivotObject);
                    return null;
                }
            }


            m_Octree.AddDynamicObject(placedObjectInfo.gameObject, m_Settings.useAdditionalVertexStreams);

            placedObjectInfo.brush.PrepareNextPrefabForPlace();

            return placedObjectInfo;
        }



        void BrushModeFinishPlaceObject(PlacedObjectInfo placedObjectInfo)
        {
            placedObjectInfo.gameObject.transform.SetParent(null, true);
            placedObjectInfo.gameObject.hideFlags = HideFlags.None;

            // Round position
            placedObjectInfo.gameObject.transform.position =
                Utility.RoundVector(placedObjectInfo.gameObject.transform.position, 3);

            GameObject.DestroyImmediate(placedObjectInfo.pivotObject);
            placedObjectInfo.pivotObject = null;

            ParentObject(placedObjectInfo);

            RegisterCreatedObjectUndo(placedObjectInfo.gameObject, "PP: Place Objects");
        }


        void FinishPlaceObject(PlacedObjectInfo placedObjectInfo)
        {
            placedObjectInfo.gameObject.transform.SetParent(null, true);

            if(StageUtility.GetCurrentStage() != StageUtility.GetMainStage())
                StageUtility.PlaceGameObjectInCurrentStage(placedObjectInfo.gameObject);

            // Round position
            placedObjectInfo.gameObject.transform.position =
                Utility.RoundVector(placedObjectInfo.gameObject.transform.position, 3);

            GameObject.DestroyImmediate(placedObjectInfo.pivotObject);
            placedObjectInfo.pivotObject = null;

            ParentObject(placedObjectInfo);

            m_Octree.AddDynamicObject(placedObjectInfo.gameObject, m_Settings.useAdditionalVertexStreams);

            RegisterCreatedObjectUndo(placedObjectInfo.gameObject, "PP: Place Objects");

            placedObjectInfo.brush.PrepareNextPrefabForPlace();
        }


        void DestroyObject(PlacedObjectInfo placedObjectInfo)
        {
            GameObject.DestroyImmediate(placedObjectInfo.gameObject);
            GameObject.DestroyImmediate(placedObjectInfo.pivotObject);
        }



        void ParentObject(PlacedObjectInfo placedObjectInfo)
        {
            ParentObject(placedObjectInfo.gameObject, placedObjectInfo.raycastInfo.hitObject,
                placedObjectInfo.brush.name);
        }

        void ParentObject(GameObject gameObject, GameObject hitObject, string brushName)
        {
            GameObject placeLocation = null;

            switch(m_Settings.placeUnder)
            {
            case Placement.HitObject:
                placeLocation = hitObject;
                break;
            case Placement.CustomObject:
                if(StageUtility.GetCurrentStage() == StageUtility.GetMainStage())
                    placeLocation = m_SceneSettings.parentForPrefabs;
                break;
            default: case Placement.World:
                break;
            }


            // Group Prefabs
            // find group object by name
            if(m_Settings.groupPrefabs)
            {
                Transform group = null;
                string groupName = brushName + m_Settings.groupName;

                if (placeLocation != null)
                {
                    group = placeLocation.transform.Find(groupName);
                }
                else
                {
                    GameObject[] sceneRoots = m_CurrentStageScene.GetRootGameObjects();

                    if (StageUtility.GetCurrentStage() != StageUtility.GetMainStage() && sceneRoots.Length > 0)
                    {
                        for (int i = 0; i < sceneRoots[0].transform.childCount; i++)
                        {
                            if (sceneRoots[0].transform.GetChild(i).name == groupName)
                            {
                                group = sceneRoots[0].transform.GetChild(i).transform;
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (GameObject root in sceneRoots)
                        {
                            if (root.name == groupName)
                            {
                                group = root.transform;
                                break;
                            }
                        }
                    }
                }

                if (group == null)
                {
                    GameObject newGroupForPrefabs = new GameObject(groupName);
                    if (placeLocation != null)
                        newGroupForPrefabs.transform.parent = placeLocation.transform;
                    group = newGroupForPrefabs.transform;
                }

                group.gameObject.layer = m_Settings.prefabPlaceLayer;
                placeLocation = group.gameObject;
            }

            if (gameObject != null && placeLocation != null && placeLocation.transform != null)
            {
                gameObject.transform.SetParent(placeLocation.transform, true);

                if(StageUtility.GetCurrentStage() != StageUtility.GetMainStage())
                    StageUtility.PlaceGameObjectInCurrentStage(placeLocation);
            }
        }



        void GetRightForward(Vector3 up, out Vector3 right, out Vector3 forward)
        {
            switch (m_Settings.surfaceCoords)
            {
            default:
            case SurfaceCoords.AroundX:
                forward = Vector3.Cross(Vector3.right, up).normalized;
                if (forward.magnitude < 0.001f)
                    forward = Vector3.forward;

                right = Vector3.Cross(up, forward).normalized;
                break;
            case SurfaceCoords.AroundY:
                right = Vector3.Cross(up, Vector3.up).normalized;
                if (right.magnitude < 0.001f)
                    right = Vector3.right;

                forward = Vector3.Cross(right, up).normalized;
                break;
            case SurfaceCoords.AroundZ:
                right = Vector3.Cross(up, Vector3.forward).normalized;
                if (right.magnitude < 0.001f)
                    right = Vector3.right;

                forward = Vector3.Cross(right, up).normalized;
                break;
            }
        }

        void GetOrientation(Vector3 surfaceNormal, OrientationMode mode, out Vector3 upwards, out Vector3 right, out Vector3 forward)
        {
            switch (mode)
            {
            case OrientationMode.SurfaceNormal:
                upwards = surfaceNormal;
                break;
            case OrientationMode.SurfaceNormalNegative:
                upwards = -surfaceNormal;
                break;
            case OrientationMode.X:
                upwards = new Vector3(1, 0, 0);
                break;
            case OrientationMode.XNegative:
                upwards = new Vector3(-1, 0, 0);
                break;
            default:
            case OrientationMode.Y:
                upwards = new Vector3(0, 1, 0);
                break;
            case OrientationMode.YNegative:
                upwards = new Vector3(0, -1, 0);
                break;
            case OrientationMode.Z:
                upwards = new Vector3(0, 0, 1);
                break;
            case OrientationMode.ZNegative:
                upwards = new Vector3(0, 0, -1);
                break;
            }

            GetRightForward(upwards, out right, out forward);
        }

        bool GetObjectWorldBounds(GameObject gameObject, out Bounds bounds)
        {
            Bounds worldBounds = new Bounds();
            bool found = false;

            Utility.ForAllInHierarchy(gameObject, (go) =>
            {
                if (!go.activeInHierarchy)
                    return;

                Renderer renderer = go.GetComponent<Renderer>();
                SkinnedMeshRenderer skinnedMeshRenderer;
                RectTransform rectTransform;

                if (renderer != null)
                {
                    if (!found)
                    {
                        worldBounds = renderer.bounds;
                        found = true;
                    }
                    else
                    {
                        worldBounds.Encapsulate(renderer.bounds);
                    }
                }
                else if ((skinnedMeshRenderer = go.GetComponent<SkinnedMeshRenderer>()) != null)
                {
                    if (!found)
                    {
                        worldBounds = skinnedMeshRenderer.bounds;
                        found = true;
                    }
                    else
                    {
                        worldBounds.Encapsulate(skinnedMeshRenderer.bounds);
                    }
                }
                else if ((rectTransform = go.GetComponent<RectTransform>()) != null)
                {
                    Vector3[] fourCorners = new Vector3[4];
                    rectTransform.GetWorldCorners(fourCorners);
                    Bounds rectBounds = new Bounds();

                    rectBounds.center = fourCorners[0];
                    rectBounds.Encapsulate(fourCorners[1]);
                    rectBounds.Encapsulate(fourCorners[2]);
                    rectBounds.Encapsulate(fourCorners[3]);

                    if (!found)
                    {
                        worldBounds = rectBounds;
                        found = true;
                    }
                    else
                    {
                        worldBounds.Encapsulate(rectBounds);
                    }
                }
            });

            if (!found)
                bounds = new Bounds(gameObject.transform.position, Vector3.one);
            else
                bounds = worldBounds;

            return found;
        }


        bool GetObjectLocalBounds(GameObject gameObject, out Bounds bounds)
        {
            Transform transform = gameObject.transform;

            Vector3 savedPosition = transform.position;
            Quaternion savedRotation = transform.rotation;
            Vector3 savedLocalScale = transform.localScale;

            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            bool found = GetObjectWorldBounds(gameObject, out bounds);

            transform.position = savedPosition;
            transform.rotation = savedRotation;
            transform.localScale = savedLocalScale;

            return found;
        }


        public static float PointLineDistance(Vector3 point, Vector3 linePoint, Vector3 lineDirection)
        {
            float q = (Vector3.Dot(lineDirection, (point - linePoint))) / (lineDirection.sqrMagnitude + Mathf.Epsilon);
            return (point - (linePoint + q * lineDirection)).magnitude;

        }

        float GetObjectScaleFactor(GameObject gameObject, RaycastInfo raycast)
        {
            Bounds bounds;

            GetObjectWorldBounds(gameObject, out bounds);

            bounds.center = Vector3.zero;

            Vector3 localScale = gameObject.transform.localScale;
            float size = Vector3.ProjectOnPlane(bounds.extents, raycast.normal).magnitude;

            size *= 2.0f;

            if (size != 0.0f)
                size = 1.0f / size;
            else
                size = 1.0f;

            return localScale.x * size;
        }

        float GetObjectScaleFactor(GameObject gameObject)
        {
            Bounds bounds;

            GetObjectWorldBounds(gameObject, out bounds);

            Vector3 localScale = gameObject.transform.localScale;
            Vector3 point = gameObject.transform.position;
            Vector3 direction = gameObject.transform.rotation * Vector3.up;

            float size = 0f;
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            size = Mathf.Max(PointLineDistance(new Vector3(min.x, min.y, min.z), point, direction), size);
            size = Mathf.Max(PointLineDistance(new Vector3(min.x, min.y, max.z), point, direction), size);
            size = Mathf.Max(PointLineDistance(new Vector3(min.x, max.y, min.z), point, direction), size);
            size = Mathf.Max(PointLineDistance(new Vector3(min.x, max.y, max.z), point, direction), size);
            size = Mathf.Max(PointLineDistance(new Vector3(max.x, min.y, min.z), point, direction), size);
            size = Mathf.Max(PointLineDistance(new Vector3(max.x, min.y, max.z), point, direction), size);
            size = Mathf.Max(PointLineDistance(new Vector3(max.x, max.y, min.z), point, direction), size);
            size = Mathf.Max(PointLineDistance(new Vector3(max.x, max.y, max.z), point, direction), size);

            size *= 2.0f;

            if (size != 0.0f)
               size = 1.0f / size;
            else
               size = 1.0f;

            return localScale.x * size;
        }
#endregion // Object Placement




#region Scene UI


        void DrawBrushHandles(RaycastInfo hit, Brush brush)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (brush.settings.gridEnabled)
            {
                Handles.color = m_Settings.handlesColor;
                Handles.CircleHandleCap(1, hit.point, Quaternion.LookRotation(hit.normal), Mathf.Min(brush.settings.gridStep.x, brush.settings.gridStep.y) * 0.5f, EventType.Repaint);

                Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.1f);
                Handles.DrawSolidDisc(hit.point, hit.normal, Mathf.Min(brush.settings.gridStep.x, brush.settings.gridStep.y) * 0.5f);

            }
            else {
                Handles.color = m_Settings.handlesColor;
                Handles.CircleHandleCap(1, hit.point, Quaternion.LookRotation(hit.normal), brush.settings.brushRadius, EventType.Repaint);

                Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.1f);
                Handles.DrawSolidDisc(hit.point, hit.normal, brush.settings.brushRadius);
            }

            Vector3 upwards, forward, right;
            GetOrientation(hit.normal, brush.settings.orientationMode, out upwards, out right, out forward);

            Handles.color = m_Settings.handlesColor;
            Styles.handlesBoldTextStyle.normal.textColor = m_Settings.handlesColor;
            Handles.Label(hit.point, "    Br", Styles.handlesBoldTextStyle);

            DrawXYZCross(hit, upwards, right, forward);
        }

        void DrawEraseHandles(RaycastInfo hit, Brush brush)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.1f);
            Handles.DrawSolidDisc(hit.point, hit.normal, m_Settings.eraseBrushRadius);
            Handles.color = m_Settings.handlesColor;
            Handles.CircleHandleCap(1, hit.point, Quaternion.LookRotation (hit.normal), m_Settings.eraseBrushRadius, EventType.Repaint);

            Handles.color = m_Settings.handlesColor;
            Styles.handlesBoldTextStyle.normal.textColor = m_Settings.handlesColor;
            Handles.Label(hit.point, "    Er", Styles.handlesBoldTextStyle);

            Vector3 forward, right;
            GetRightForward(hit.normal, out right, out forward);

            float handleSize = HandleUtility.GetHandleSize (hit.point) * 0.4f;
            Handles.color = m_Settings.handlesColor;
            Handles.DrawLine(hit.point + right * handleSize, hit.point + right * -handleSize);
            Handles.DrawLine(hit.point + forward * handleSize, hit.point + forward * -handleSize);
        }

        void DrawSelectHandles(RaycastInfo hit, Brush brush)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.1f);
            Handles.DrawSolidDisc(hit.point, hit.normal, m_Settings.selectBrushRadius);
            Handles.color = m_Settings.handlesColor;
            Handles.CircleHandleCap(1, hit.point, Quaternion.LookRotation (hit.normal), m_Settings.selectBrushRadius, EventType.Repaint);

            Handles.color = m_Settings.handlesColor;
            Styles.handlesBoldTextStyle.normal.textColor = m_Settings.handlesColor;
            if(Event.current.shift)
                Handles.Label(hit.point, "    Sl+", Styles.handlesBoldTextStyle);
            else if (Event.current.control)
                Handles.Label(hit.point, "    Sl-", Styles.handlesBoldTextStyle);
            else
                Handles.Label(hit.point, "    Sl", Styles.handlesBoldTextStyle);

            Vector3 forward, right;
            GetRightForward(hit.normal, out right, out forward);

            float handleSize = HandleUtility.GetHandleSize (hit.point) * 0.4f;
            Handles.color = m_Settings.handlesColor;
            Handles.DrawLine(hit.point + right * handleSize, hit.point + right * -handleSize);
            Handles.DrawLine(hit.point + forward * handleSize, hit.point + forward * -handleSize);
        }


        void DrawModifyHandles(RaycastInfo hit, Brush brush)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.1f);
            Handles.DrawSolidDisc(hit.point, hit.normal, m_Settings.modifyBrushRadius);
            Handles.color = m_Settings.handlesColor;
            Handles.CircleHandleCap(1, hit.point, Quaternion.LookRotation(hit.normal), m_Settings.modifyBrushRadius, EventType.Repaint);

            Handles.color = m_Settings.handlesColor;
            Styles.handlesBoldTextStyle.normal.textColor = m_Settings.handlesColor;
            Handles.Label(hit.point, "    Md", Styles.handlesBoldTextStyle);

            Vector3 forward, right;
            GetRightForward(hit.normal, out right, out forward);

            float handleSize = HandleUtility.GetHandleSize(hit.point) * 0.4f;
            Handles.color = m_Settings.handlesColor;
            Handles.DrawLine(hit.point + right * handleSize, hit.point + right * -handleSize);
            Handles.DrawLine(hit.point + forward * handleSize, hit.point + forward * -handleSize);
        }

        void DrawOrientToolHandles(RaycastInfo hit, bool isHot)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Handles.color = m_Settings.handlesColor;
            Styles.handlesBoldTextStyle.normal.textColor = m_Settings.handlesColor;
            Handles.Label(hit.point, "    Or", Styles.handlesBoldTextStyle);

            Vector3 forward, right;
            GetRightForward(hit.normal, out right, out forward);

            float handleSize = HandleUtility.GetHandleSize(hit.point) * 0.4f;
            Handles.color = m_Settings.handlesColor;
            Handles.DrawLine(hit.point + right * handleSize, hit.point + right * -handleSize);
            Handles.DrawLine(hit.point + forward * handleSize, hit.point + forward * -handleSize);

            if (isHot && hit.isHit)
            {
                if (m_Settings.orientSameDirection)
                {
                    Handles.DrawLine(hit.point, m_OrientTool.objectsCenter);
                }
                else
                {
                    foreach (OrientTool.ObjectInfo objectInfo in m_OrientTool.objects)
                    {
                        Handles.DrawLine(hit.point, objectInfo.pivot);
                    }
                }
            }
        }

        void DrawMoveToolHandles(bool isHot)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Handles.color = m_Settings.handlesColor;
            Styles.handlesBoldTextStyle.normal.textColor = m_Settings.handlesColor;
            Handles.Label(m_MoveTool.objectsCenter, "    Sld", Styles.handlesBoldTextStyle);

            if (m_MoveTool.objects.Count > 0)
            {
                m_MoveTool.handleDiskSize = HandleUtility.GetHandleSize(m_MoveTool.objectsCenter) * 0.3f;
                Vector3 normal = (m_MoveTool.objectsCenter - Camera.current.transform.position).normalized;

                Handles.color = new Color(m_Settings.handlesColor.r, m_Settings.handlesColor.g, m_Settings.handlesColor.b, 0.2f);
                Handles.DrawSolidDisc(m_MoveTool.objectsCenter, normal, m_MoveTool.handleDiskSize);

                Handles.color = m_Settings.handlesColor;
                Handles.DrawWireDisc(m_MoveTool.objectsCenter, normal, m_MoveTool.handleDiskSize);

            }
        }

        void DrawMaskedHandles(RaycastInfo hit, Brush brush)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            float handleSize = HandleUtility.GetHandleSize (hit.point) * 0.2f;

            Vector3 forward, right;
            GetRightForward(hit.normal, out right, out forward);

            Handles.color = m_Settings.handlesColor;
            Styles.handlesTextStyle.normal.textColor = m_Settings.handlesColor;
            Handles.CircleHandleCap(1, hit.point, Quaternion.LookRotation (hit.normal), handleSize * 0.5f, EventType.Repaint);
            Handles.DrawLine(hit.point + right * handleSize, hit.point + right * -handleSize);
            Handles.DrawLine(hit.point + forward * handleSize, hit.point + forward * -handleSize);
            if (hit.hitObject != null)
                Handles.Label(hit.point, "      Layer: " + LayerMask.LayerToName(hit.hitObject.layer), Styles.handlesTextStyle);
        }

        void DrawErrorHandles(RaycastInfo hit, string message)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            float handleSize = HandleUtility.GetHandleSize (hit.point) * 0.2f;

            Vector3 forward, right;
            GetRightForward(hit.normal, out right, out forward);

            Handles.color = m_Settings.handlesColor;
            Handles.CircleHandleCap(1, hit.point, Quaternion.LookRotation (hit.normal), handleSize * 0.5f, EventType.Repaint);
            Handles.DrawLine(hit.point + right * handleSize, hit.point + right * -handleSize);
            Handles.DrawLine(hit.point + forward * handleSize, hit.point + forward * -handleSize);
            Handles.Label(hit.point, "      " +  message, Styles.handlesTextStyle);
        }


        void DrawPickObjectHandles(RaycastInfo hit)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Vector3 forward, right;
            GetRightForward(hit.normal, out right, out forward);

            float handleSize = HandleUtility.GetHandleSize(hit.point) * 0.3f;

            Handles.color = Color.green;
            Handles.ArrowHandleCap(0, hit.point, Quaternion.LookRotation(hit.normal, right), handleSize * 2f, EventType.Repaint);
            Handles.color = Color.red;
            Handles.DrawAAPolyLine(3, hit.point + right * handleSize, hit.point + right * -handleSize);
            Handles.color = Color.blue;
            Handles.DrawAAPolyLine(3, hit.point + forward * handleSize, hit.point + forward * -handleSize);
            Handles.color = Color.red;
            Handles.Label(hit.point, "      " + m_OnPickObjectMessage, Styles.handlesTextStyle);
        }


        void DrawPinToolHandles(Brush brush)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Vector3 upwards;
            Vector3 right;
            Vector3 forward;

            GetOrientation(m_CurrentRaycast.normal, brush.settings.orientationMode,
                out upwards, out right, out forward);

            if (m_PinTool.placedObjectInfo == null)
            {
                Handles.color = m_Settings.handlesColor;
                Styles.handlesBoldTextStyle.normal.textColor = m_Settings.handlesColor;
                Handles.Label(m_CurrentRaycast.point, "    Pi", Styles.handlesBoldTextStyle);

                DrawXYZCross(m_CurrentRaycast, upwards, right, forward);

                Handles.color = m_Settings.handlesColor;
                Handles.CircleHandleCap(1, m_CurrentRaycast.point, Quaternion.LookRotation(upwards),
                    HandleUtility.GetHandleSize(m_CurrentRaycast.point) * 0.2f, EventType.Repaint);
                return;
            }

            Handles.color = m_Settings.handlesColor;
            Handles.CircleHandleCap(1, m_PinTool.placedObjectInfo.raycastInfo.point, Quaternion.LookRotation (upwards), m_PinTool.radius, EventType.Repaint);
            Handles.DrawDottedLine(m_PinTool.placedObjectInfo.raycastInfo.point, m_PinTool.point, 4.0f);
            Handles.DrawDottedLine(m_PinTool.placedObjectInfo.raycastInfo.point, m_PinTool.placedObjectInfo.raycastInfo.point + forward * m_PinTool.radius, 4.0f);

            Handles.color = new Color(0, 0, 0, 0.5f);
            Handles.DrawSolidArc(m_PinTool.placedObjectInfo.raycastInfo.point, upwards, forward, m_PinTool.angle, m_PinTool.radius);

            Handles.color = m_Settings.handlesColor;
            Styles.handlesBoldTextStyle.normal.textColor = m_Settings.handlesColor;
            Handles.Label(m_PinTool.point, "    Angle: " + (m_PinTool.angle).ToString("F2"), Styles.handlesBoldTextStyle);
            float sScale = m_PinTool.placedObjectInfo.pivotObject.transform.localScale.x;
            Handles.Label(m_PinTool.point, "\n    Scale: " + sScale.ToString("F2"), Styles.handlesBoldTextStyle);

            DrawXYZCross(m_CurrentRaycast, upwards, right, forward);
        }

        void DrawPlaceToolHandles(Brush brush)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Vector3 forward, right;
            if (m_PlaceTool.placedObjectInfo == null)
            {
                Handles.color = m_Settings.handlesColor;
                Styles.handlesBoldTextStyle.normal.textColor = m_Settings.handlesColor;
                Handles.Label(m_CurrentRaycast.point, "    Pl", Styles.handlesBoldTextStyle);

                GetRightForward(m_CurrentRaycast.normal, out right, out forward);
                DrawXYZCross(m_CurrentRaycast, m_CurrentRaycast.normal, right, forward);
                return;
            }


            Handles.color = m_Settings.handlesColor;
            Handles.matrix = m_PlaceTool.placedObjectInfo.gameObject.transform.localToWorldMatrix;
            Handles.DrawWireCube(m_PlaceTool.placedObjectInfo.localBounds.center, m_PlaceTool.placedObjectInfo.localBounds.size);
            Handles.matrix = Matrix4x4.identity;

            Handles.color = m_Settings.handlesColor;
            Styles.handlesBoldTextStyle.normal.textColor = m_Settings.handlesColor;
            Handles.Label(m_PlaceTool.raycastInfo.point, "    Rotation: X: " + (brush.settings.placeEulerAngles.x).ToString("F2")
                + " Y: " + (brush.settings.placeEulerAngles.y).ToString("F1")
                + " Z: " + (brush.settings.placeEulerAngles.z).ToString("F1"), Styles.handlesBoldTextStyle);

            //if (brush.settings.placeScale < 0.1f)
                Handles.Label(m_PlaceTool.raycastInfo.point, "\n    Scale: " + brush.settings.placeScale.ToString("F2"), Styles.handlesBoldTextStyle);

            GetRightForward(m_CurrentRaycast.normal, out right, out forward);
            DrawXYZCross(m_CurrentRaycast, m_CurrentRaycast.normal, right, forward);
        }


        void DrawDropHandles(RaycastInfo hit, Brush brush)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Handles.color = m_Settings.handlesColor;
            Handles.CircleHandleCap(1, hit.point, Quaternion.LookRotation(hit.normal), brush.settings.throwRadius, EventType.Repaint);

            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.1f);
            Handles.DrawSolidDisc(hit.point, hit.normal, brush.settings.throwRadius);

            Handles.color = m_Settings.handlesColor;
            Styles.handlesBoldTextStyle.normal.textColor = m_Settings.handlesColor;
            Handles.Label(hit.point, "    Dr", Styles.handlesBoldTextStyle);

            Vector3 forward, right;
            GetRightForward(hit.normal, out right, out forward);

            float handleSize = HandleUtility.GetHandleSize(hit.point) * 0.4f;
            Handles.color = m_Settings.handlesColor;
            Handles.DrawLine(hit.point + right * handleSize, hit.point + right * -handleSize);
            Handles.DrawLine(hit.point + forward * handleSize, hit.point + forward * -handleSize);

            if (m_ThrowTool.noRigidBody)
            {
                Handles.Label(hit.point, "\n\n\n WARNING: Some prefabs without Rigidbody", Styles.handlesTextStyle);
            }
        }

        void DrawXYZCross(RaycastInfo hit, Vector3 upwards, Vector3 right, Vector3 forward)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            float lineWidth = 4f;
            float handleSize = HandleUtility.GetHandleSize(hit.point) * 0.5f;

            Handles.color = Color.red;
            Handles.DrawAAPolyLine(lineWidth, hit.point + right * handleSize, hit.point + right * -handleSize * 0.2f);
            Handles.color = Color.green;
            Handles.DrawAAPolyLine(lineWidth, hit.point + upwards * handleSize, hit.point + upwards * -handleSize * 0.2f);
            Handles.color = Color.blue;
            Handles.DrawAAPolyLine(lineWidth, hit.point + forward * handleSize, hit.point + forward * -handleSize * 0.2f);
        }


        Vector3 GetGridNormalVector(Brush brush)
        {
            switch(brush.settings.gridPlane)
            {
            case GridPlane.XY:
                return new Vector3(0, 0, 1);
            case GridPlane.XZ:
                return new Vector3(0, 1, 0);
            case GridPlane.YZ:
                return new Vector3(1, 0, 0);
            }

            if(brush.settings.gridNormal.magnitude < 0.001f)
                return new Vector3(0, 1, 0);

            return brush.settings.gridNormal.normalized;
        }



        void DrawGrid(Brush brush)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (!m_Grid.originRaycastInfo.isHit)
                return;


            const int kSize     = Grid.kSize;
            int halfSize        = kSize / 2;
            float halfSizeInv   = 1.0f / halfSize;
            Vector3 gridStep    = new Vector3(brush.settings.gridStep.x, brush.settings.gridStep.y, 1);
            Vector3 gridNormal  = GetGridNormalVector(brush);
            float gridBaseAlpha = 1.0f;
            //float raycastHeight = m_Settings.gridRaycastHeight;

            Matrix4x4 gridMatrix = Matrix4x4.TRS(m_Grid.visualOrigin, Quaternion.AngleAxis(brush.settings.gridAngle, gridNormal) * Quaternion.LookRotation(gridNormal), gridStep);

            Vector3 localHitPoint = gridMatrix.inverse.MultiplyPoint(m_CurrentRaycast.point);


            /*for(int ix = 0; ix < kSize; ix++)
            {
                for (int iy = 0; iy < kSize; iy++)
                {
                    Vector3 localPoint = new Vector3(ix - halfSize, iy - halfSize, 0);

                    Vector3 point = gridMatrix.MultiplyPoint(localPoint);

                    Ray ray = m_Grid.originRaycastInfo.ray;
                    ray.origin = point + ray.direction * -raycastHeight;

                    Handles.color = m_Settings.handlesColor;

                    if (m_Octree.Raycast(ray, out RaycastInfo raycastInfo, m_Settings.paintLayers.value, m_Settings.ignoreLayers.value, null))
                    {
                        m_Grid.points[ix, iy].point = raycastInfo.point;
                        m_Grid.points[ix, iy].hit = true;

                        Handles.DotHandleCap(0, raycastInfo.point, Quaternion.identity, HandleUtility.GetHandleSize(raycastInfo.point) * 0.03f, EventType.Repaint);
                    }
                    else
                    {
                        m_Grid.points[ix, iy].hit = false;
                    }
                }
            }

            for (int ix = 0; ix < kSize; ix++)
            {
                for (int iy = 0; iy < kSize-1; iy++)
                {
                    if (!m_Grid.points[ix, iy].hit || !m_Grid.points[ix, iy + 1].hit)
                        continue;

                    GL.Begin(GL.LINES);
                    GL.Color(new Color(0, 0, 0, 0.9f));
                    GL.Vertex(m_Grid.points[ix, iy].point);
                    GL.Vertex(m_Grid.points[ix, iy+1].point);
                    GL.End();
                }

            }

            for (int iy = 0; iy < kSize; iy++)
            {
                for (int ix = 0; ix < kSize - 1; ix++)
                {
                    if (!m_Grid.points[ix, iy].hit || !m_Grid.points[ix+1, iy].hit)
                        continue;

                    GL.Begin(GL.LINES);
                    GL.Color(new Color(0, 0, 0, 0.9f));
                    GL.Vertex(m_Grid.points[ix, iy].point);
                    GL.Vertex(m_Grid.points[ix+1, iy].point);
                    GL.End();
                }

            }*/




            for (int x = -halfSize; x <= halfSize; x++)
            {
                for (int y = -halfSize; y <= halfSize; y++)
                {
                    Vector3 localPoint = new Vector3(x, y, 0);

                    Vector3 point = gridMatrix.MultiplyPoint(localPoint);

                    float alpha = gridBaseAlpha * Mathf.Clamp01((1.0f - (localPoint - localHitPoint).magnitude * halfSizeInv)) * 1.0f + 0.15f;
                    Handles.color = new Color(1, 1, 1, alpha) * m_Settings.handlesColor;
                    Handles.DotHandleCap(0, point, Quaternion.identity, HandleUtility.GetHandleSize(point) * 0.03f, EventType.Repaint);

                }
            }


            halfSize += 1;
            for(int x = -(halfSize-1); x < halfSize; x++)
            {
                Vector3 point1 = new Vector3(x, -halfSize, 0);
                Vector3 point2 = new Vector3(x, 0, 0);
                Vector3 point3 = new Vector3(x, halfSize, 0);

                Vector3 point4 = new Vector3(-halfSize, x, 0);
                Vector3 point5 = new Vector3(0, x, 0);
                Vector3 point6 = new Vector3(halfSize, x, 0);

                GL.PushMatrix();
                GL.MultMatrix(gridMatrix);
                GL.Begin(GL.LINES);

                GL.Color(new Color(0, 0, 0, gridBaseAlpha * Mathf.Clamp01(1.0f - (point1 - localHitPoint).magnitude * halfSizeInv)));
                GL.Vertex(point1);
                GL.Color(new Color(0, 0, 0, gridBaseAlpha * Mathf.Clamp01(1.0f - (point2 - localHitPoint).magnitude * halfSizeInv)));
                GL.Vertex(point2);
                GL.Vertex(point2);
                GL.Color(new Color(0, 0, 0, gridBaseAlpha * Mathf.Clamp01(1.0f - (point3 - localHitPoint).magnitude * halfSizeInv)));
                GL.Vertex(point3);


                GL.Color(new Color(0, 0, 0, gridBaseAlpha * Mathf.Clamp01(1.0f - (point4 - localHitPoint).magnitude * halfSizeInv)));
                GL.Vertex(point4);
                GL.Color(new Color(0, 0, 0, gridBaseAlpha * Mathf.Clamp01(1.0f - (point5 - localHitPoint).magnitude * halfSizeInv)));
                GL.Vertex(point5);
                GL.Vertex(point5);
                GL.Color(new Color(0, 0, 0, gridBaseAlpha * Mathf.Clamp01(1.0f - (point6 - localHitPoint).magnitude * halfSizeInv)));
                GL.Vertex(point6);


                GL.End();
                GL.PopMatrix();
            }

            if(!m_Grid.inDeadZone)
            {
                // Draw origin point
                Handles.color = m_Settings.handlesColor;
                Handles.DotHandleCap(0, m_Grid.originRaycastInfo.point, Quaternion.identity, HandleUtility.GetHandleSize (m_Grid.originRaycastInfo.point) * 0.05f, EventType.Repaint);
                Handles.color = Color.white;
                Handles.DotHandleCap(0, m_Grid.originRaycastInfo.point, Quaternion.identity, HandleUtility.GetHandleSize (m_Grid.originRaycastInfo.point) * 0.03f, EventType.Repaint);

                Handles.Label(m_CurrentRaycast.point, "\n\n     P: " + m_Grid.originRaycastInfo.point, Styles.handlesTextStyle);
            }

        }




        void UpdateGrid(Brush brush)
        {
            Vector3 gridOrigin      = new Vector3(brush.settings.gridOrigin.x, brush.settings.gridOrigin.y, 0);
            Vector3 gridStep        = new Vector3(brush.settings.gridStep.x, brush.settings.gridStep.y, 1);
            Vector3 gridNormal      = GetGridNormalVector(brush);
            float   gridAngle       = brush.settings.gridAngle;
            Vector3 hitPoint        = m_CurrentRaycast.point;
            float   raycastHeight   = m_Settings.gridRaycastHeight;


            if(!m_CurrentRaycast.isHit)
            {
                m_Grid.originRaycastInfo = new RaycastInfo();
                return;
            }


            Matrix4x4 gridMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(gridAngle, gridNormal) * Quaternion.LookRotation(gridNormal), Vector3.one)
                                 * Matrix4x4.TRS(gridOrigin, Quaternion.identity, gridStep);

            // transform point to grid space
            Vector3 gridSpacePoint = gridMatrix.inverse.MultiplyPoint(hitPoint);


            m_Grid.inDeadZone = (new Vector2(Mathf.Round(gridSpacePoint.x), Mathf.Round(gridSpacePoint.y)) - new Vector2(gridSpacePoint.x, gridSpacePoint.y)).magnitude > Grid.kDeadZoneSize;


            // round point values
            gridSpacePoint = new Vector3(Mathf.Round(gridSpacePoint.x), Mathf.Round(gridSpacePoint.y), gridSpacePoint.z);

            // transform point back to world space
            Vector3 snappedHitPoint = gridMatrix.MultiplyPoint(gridSpacePoint);


            Ray ray;

            // offset raycast
            if(Vector3.Dot(m_CurrentRaycast.normal, gridNormal) > 0f)
            {
                snappedHitPoint += gridNormal * raycastHeight;
                ray = new Ray(snappedHitPoint, -gridNormal);
            }
            else
            {
                snappedHitPoint -= gridNormal * raycastHeight;
                ray = new Ray(snappedHitPoint, gridNormal);
            }

            Raycast(ray, out m_Grid.originRaycastInfo, m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);

            // if no hit - try slightly shift the ray
            // fixes situations where ray just touch objects
            if(!m_Grid.originRaycastInfo.isHit)
            {
                for(int i = 0; i < 3; i++)
                {
                    Ray shiftedRay = new Ray(ray.origin + UnityEngine.Random.onUnitSphere * 0.001f, ray.direction);
                    Raycast(shiftedRay, out m_Grid.originRaycastInfo, m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);
                    if(m_Grid.originRaycastInfo.isHit)
                        break;
                }
            }


            RaycastInfo visualRaycast = new RaycastInfo();
                Raycast(m_Grid.originRaycastInfo.ray, out visualRaycast, m_Settings.paintLayers.value, ~m_Settings.paintLayers.value);
                m_Grid.visualOrigin = visualRaycast.point;
        }



        void OnToolEnabled(PaintTool tool)
        {
            GameObject[] sceneObjects;
            m_CurrentStage = StageUtility.GetCurrentStage();

            if (m_CurrentStage == StageUtility.GetMainStage())
            {
                m_CurrentStageScene = SceneManager.GetActiveScene();
                #if UNITY_2023_1_OR_NEWER
                sceneObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                #else
                sceneObjects = GameObject.FindObjectsOfType<GameObject>(); 
                #endif
            }
            else if(m_CurrentStage is PreviewSceneStage previewSceneStage)
            {
                m_CurrentStageScene = previewSceneStage.scene;

                List<GameObject> objects = new List<GameObject>();
                foreach (GameObject root in m_CurrentStageScene.GetRootGameObjects())
                    Utility.ForAllInHierarchy(root, o => { objects.Add(o); });

                sceneObjects = objects.ToArray();
            }
            else
            {
                m_CurrentStageScene = SceneManager.GetActiveScene();

                List<GameObject> objects = new List<GameObject>();
                sceneObjects = objects.ToArray();
            }


            switch (tool)
            {
            case PaintTool.Brush:
            case PaintTool.Pin:
            case PaintTool.Place:
            case PaintTool.Throw:
            case PaintTool.Erase:
            case PaintTool.Modify:
            case PaintTool.PickObject:
                {
                    Tools.current = Tool.None;

                    m_SelectedObjects = Selection.objects;

                    Selection.objects = new UnityEngine.Object[0];

                    m_Octree.Populate(sceneObjects, m_Settings.useAdditionalVertexStreams);

                    if (tool == PaintTool.Throw)
                    {
                        m_LastPhysicsUpdateTime = Time.realtimeSinceStartup;
                        #if UNITY_2022_2_OR_NEWER
                        m_ThrowTool.lastSimulationMode = Physics.simulationMode;
                        Physics.simulationMode = SimulationMode.Script;
                        #else
                        Physics.autoSimulation = false;
                        #endif

                        m_ThrowTool.m_rigidbodys ??= new List<Rigidbody>();
                        m_ThrowTool.m_rigidbody2Ds ??= new List<Rigidbody2D>();

                        m_ThrowTool.m_rigidbodys.Clear();
                        m_ThrowTool.m_rigidbody2Ds.Clear();

                        foreach (var root in m_CurrentStageScene.GetRootGameObjects())
                        {
                            Utility.ForAllInHierarchy(root, gameObject =>
                            {
                                Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
                                Rigidbody2D rigidbody2D = gameObject.GetComponent<Rigidbody2D>();

                                if (rigidbody != null && rigidbody.isKinematic == false)
                                {
                                    m_ThrowTool.m_rigidbodys.Add(rigidbody);
                                    rigidbody.isKinematic = true;
                                    rigidbody.Sleep();
                                }

                                if (rigidbody2D != null && rigidbody2D.isKinematic == false)
                                {
                                    m_ThrowTool.m_rigidbody2Ds.Add(rigidbody2D);
                                    rigidbody2D.isKinematic = true;
                                    rigidbody2D.Sleep();
                                }
                            });
                        }
                    }
                }
                break;
            case PaintTool.Orient:
                {
                    Tools.current = Tool.None;
                    m_Octree.Populate(sceneObjects, m_Settings.useAdditionalVertexStreams);

                    m_SelectedObjects = null;
                }
                break;
            case PaintTool.Move:
                {
                    Tools.current = Tool.None;
                    m_Octree.Populate(sceneObjects, m_Settings.useAdditionalVertexStreams);

                    m_SelectedObjects = null;

                    MoveToolReloadSelection();
                }
                break;
            case PaintTool.Select:
                {
                    Tools.current = Tool.None;
                    m_Octree.Populate(sceneObjects, m_Settings.useAdditionalVertexStreams);

                    m_SelectedObjects = null;
                    m_SelectionTool.selectedObjects.Clear();
                    m_SelectionTool.selectedObjects.AddRange(Selection.gameObjects);
                }
                break;
            }
        }


        void OnToolDisabled(PaintTool tool)
        {
            if (m_SelectedObjects != null && Selection.objects.Length == 0)
            {
                Selection.objects = m_SelectedObjects;
            }

            m_SelectedObjects = null;

            m_Octree.Cleanup();

            switch (tool)
            {
            case PaintTool.Pin:
                if (m_PinTool.placedObjectInfo != null)
                {
                    DestroyObject(m_PinTool.placedObjectInfo);
                    m_PinTool.placedObjectInfo = null;
                }
                break;
            case PaintTool.Place:
                if(m_PlaceTool.placedObjectInfo != null)
                {
                    DestroyObject(m_PlaceTool.placedObjectInfo);
                    m_PlaceTool.placedObjectInfo = null;
                }
                break;
            case PaintTool.Throw:
                {
                    foreach (var rigidbody in m_ThrowTool.m_rigidbodys.Where(rigidbody => rigidbody != null))
                    {
                        rigidbody.isKinematic = false;
                    }

                    foreach (var rigidbody2D in m_ThrowTool.m_rigidbody2Ds.Where(rigidbody2D => rigidbody2D != null))
                    {
                        rigidbody2D.isKinematic = false;
                    }

                    #if UNITY_2022_2_OR_NEWER
                    Physics.simulationMode = m_ThrowTool.lastSimulationMode;
                    #else
                    Physics.autoSimulation = true;
                    #endif
                }
                break;
            case PaintTool.Move:
                {
                    m_MoveTool.objects.Clear();
                }
                break;
            }
        }


        void PickObject(string message, Action<RaycastInfo> action)
        {
            m_OnPickObjectAction = action;
            m_OnPickObjectMessage = message;

            Tools.current = Tool.None;
            m_CurrentTool = PaintTool.PickObject;
        }




        void OnSceneGUI(SceneView sceneView)
        {
            if (m_CurrentTool == PaintTool.None)
                return;


            // Stage changed during paint
            if(StageUtility.GetCurrentStage() != StageUtility.GetMainStage() &&
               StageUtility.GetCurrentStage() != m_CurrentStage)
            {
                m_CurrentTool = PaintTool.None;
            }


#if PP_DEBUG
            if(m_Octree.raycastCounter > 2)
            {
                Debug.Log("raycastCounter = " + m_Octree.raycastCounter);
            }
            m_Octree.raycastCounter = 0;
#endif

            // if any object selected - abort paint
            if (Selection.objects.Length > 0)
            {
                switch(m_CurrentTool)
                {
                case PaintTool.Brush:
                case PaintTool.Pin:
                case PaintTool.Place:
                case PaintTool.Erase:
                case PaintTool.Modify:
                    {
                        m_CurrentTool = PaintTool.None;
                        m_SelectedObjects = null;
                        Repaint();
                    }
                    break;
                }

            }

            Color handlesColor = Handles.color;
            Matrix4x4 handlesMatrix = Handles.matrix;


            switch (m_CurrentTool)
            {
            case PaintTool.Brush:
                DoBrushTool();
                break;
            case PaintTool.Pin:
                DoPinTool();
                break;
            case PaintTool.Place:
                DoPlaceTool();
                break;
            case PaintTool.Throw:
                DoThrowTool();
                break;
            case PaintTool.Erase:
                DoEraseTool();
                break;
            case PaintTool.Select:
                DoSelectTool();
                break;
            case PaintTool.Modify:
                DoModifyTool();
                break;
            case PaintTool.Orient:
                DoOrientTool();
                break;
            case PaintTool.Move:
                DoMoveTool();
                break;
            case PaintTool.PickObject:
                DoPickObject();
                break;
            }

            HandleKeyboardEvents();

            Handles.color = handlesColor;
            Handles.matrix = handlesMatrix;
        }



        void DoPickObject()
        {
            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(s_PickObjectToolHash, FocusType.Passive);

            switch (e.GetTypeForControl(controlID))
            {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                                m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);
                    if (m_OnPickObjectAction != null)
                    {
                        m_OnPickObjectAction(m_CurrentRaycast);
                        m_OnPickObjectAction = null;
                    }

                    m_OnPickObjectMessage = "";
                    m_CurrentTool = PaintTool.None;
                }
                break;
            case EventType.MouseMove:
                Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                                m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);
                e.Use();
                break;
            case EventType.Repaint:
                if(m_CurrentRaycast.isHit) {
                    DrawPickObjectHandles(m_CurrentRaycast);
                }
                break;
            case EventType.Layout:
                HandleUtility.AddDefaultControl(controlID);
                break;
            }
        }



        void DoBrushTool()
        {
            bool hasMultipleSelectedBrushes = m_Settings.GetActiveTab().HasMultipleSelectedBrushes();
            Brush brush = m_Settings.GetActiveTab().GetFirstSelectedBrush();

            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(s_BrushToolHash, FocusType.Passive);

            switch (e.GetTypeForControl(controlID))
            {
            case EventType.MouseDown:
                if (e.button == 0 && !e.alt && brush != null && !hasMultipleSelectedBrushes)
                {
                    Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                               m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);


                    if (brush.settings.gridEnabled)
                    {
                        UpdateGrid(brush);

                        m_BrushTool.raycastInfo = m_Grid.originRaycastInfo;
                    }
                    else
                    {
                        m_BrushTool.raycastInfo = m_CurrentRaycast;
                    }

                    m_BrushTool.dragDistance = 0;
                    m_BrushTool.strokeDirection = new Vector3(1, 0, 0);
                    m_BrushTool.lastPlacedObjectInfo = null;
                    m_BrushTool.firstNormal = new Vector3(0, 0, 0);

                    brush.BeginStroke();

                    if(brush.settings.gridEnabled)
                    {
                        PlacedObjectInfo placedObjectInfo = BrushModePlaceObject(m_BrushTool.raycastInfo, brush);
                        if (placedObjectInfo != null)
                        {
                            m_BrushTool.lastPlacedObjectInfo = placedObjectInfo;
                            m_BrushTool.strokeDirectionRefPoint = m_CurrentRaycast.point;
                        }
                    }
                    else
                    if (m_BrushTool.raycastInfo.isHit)
                    {
                        RaycastInfo raycastInfo = new RaycastInfo();

                        float brushRadius = brush.settings.brushRadius;
                        Vector3 randomPoint = m_CurrentRaycast.point + Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, m_CurrentRaycast.normal) * brushRadius;

                        Raycast(WorldPointToRay(randomPoint), out raycastInfo, m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);

                        if(raycastInfo.isHitTargetLayer)
                        {
                            PlacedObjectInfo placedObjectInfo = BrushModePlaceObject(raycastInfo, brush);
                            if (placedObjectInfo != null)
                            {
                                m_BrushTool.lastPlacedObjectInfo = placedObjectInfo;
                                m_BrushTool.strokeDirectionRefPoint = m_CurrentRaycast.point;
                            }
                        }
                    }

                    GUIUtility.hotControl = controlID;
                    e.Use();
                }
                break;
            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlID && e.button == 0 && brush != null && !hasMultipleSelectedBrushes)
                {
                    Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                               m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);


                    if (brush.settings.gridEnabled)
                    {
                        UpdateGrid(brush);

                        m_BrushTool.prevRaycast = m_BrushTool.raycastInfo;
                        m_BrushTool.raycastInfo = m_Grid.originRaycastInfo;
                    }
                    else
                    {
                        m_BrushTool.prevRaycast = m_BrushTool.raycastInfo;
                        m_BrushTool.raycastInfo = m_CurrentRaycast;
                    }

                    // loose surface - break stroke - don't orient last object
                    if (!m_BrushTool.raycastInfo.isHit || !m_BrushTool.prevRaycast.isHit)
                    {
                        if (m_BrushTool.lastPlacedObjectInfo != null)
                        {
                            BrushModeFinishPlaceObject(m_BrushTool.lastPlacedObjectInfo);
                            m_BrushTool.lastPlacedObjectInfo = null;
                        }
                    }

                    if (m_BrushTool.firstNormal.magnitude < 0.1f && m_BrushTool.raycastInfo.isHit)
                    {
                        m_BrushTool.firstNormal = m_BrushTool.raycastInfo.normal;
                    }

                    if (m_BrushTool.raycastInfo.isHit || m_BrushTool.prevRaycast.isHit)
                    {
                        Vector3 hitPoint = m_BrushTool.raycastInfo.point;
                        Vector3 lastHitPoint = m_BrushTool.prevRaycast.point;
                        Vector3 hitNormal = m_BrushTool.raycastInfo.isHit ? m_BrushTool.raycastInfo.normal : m_BrushTool.prevRaycast.normal;

                        bool isTwoPoints = true;

                        // predict point
                        if (!m_BrushTool.raycastInfo.isHit)
                        {
                            if (!m_BrushTool.prevRaycast.IntersectsHitPlane(m_BrushTool.raycastInfo.ray, out hitPoint))
                                isTwoPoints = false;
                        }

                        // predict point
                        if (!m_BrushTool.prevRaycast.isHit)
                        {
                            if (!m_BrushTool.raycastInfo.IntersectsHitPlane(m_BrushTool.prevRaycast.ray, out lastHitPoint))
                                isTwoPoints = false;
                        }


                        if (brush.settings.gridEnabled)
                        {
                            if (m_BrushTool.raycastInfo.isHitTargetLayer && !m_Grid.inDeadZone &&
                                (m_BrushTool.lastPlacedObjectInfo == null || !Utility.IsVector3Equal(hitPoint, m_BrushTool.lastPlacedObjectInfo.raycastInfo.point)))
                            {
                                PlacedObjectInfo placedObjectInfo = BrushModePlaceObject(m_BrushTool.raycastInfo, brush);
                                if (placedObjectInfo != null)
                                {
                                    if (m_BrushTool.lastPlacedObjectInfo != null)
                                        m_BrushTool.strokeDirection = (placedObjectInfo.raycastInfo.point - m_BrushTool.lastPlacedObjectInfo.raycastInfo.point).normalized;

                                    // re-orient last object along stroke
                                    if (brush.settings.alongBrushStroke && m_BrushTool.lastPlacedObjectInfo != null)
                                    {
                                        BrushModeOrientObject(m_BrushTool.lastPlacedObjectInfo);
                                        PositionObject(m_BrushTool.lastPlacedObjectInfo);
                                    }

                                    if (m_BrushTool.lastPlacedObjectInfo != null)
                                    {
                                        BrushModeFinishPlaceObject(m_BrushTool.lastPlacedObjectInfo);
                                        m_BrushTool.lastPlacedObjectInfo = null;
                                    }
                                    m_BrushTool.lastPlacedObjectInfo = placedObjectInfo;
                                    m_BrushTool.strokeDirectionRefPoint = hitPoint;
                                }
                            }
                        }
                        else
                        {
                            if (isTwoPoints && !Utility.IsVector3Equal(hitPoint, lastHitPoint))
                            {
                                float brushRadius = brush.settings.brushRadius;
                                float brushSpacing = Mathf.Max(0.01f, brush.settings.brushSpacing);
                                Vector3 moveVector = (hitPoint - lastHitPoint);
                                float moveLenght = moveVector.magnitude;
                                Vector3 moveDirection = moveVector.normalized;


                                m_BrushTool.strokeDirection = (hitPoint - m_BrushTool.strokeDirectionRefPoint).normalized;


                                // re-orient last object along stroke
                                if (brush.settings.alongBrushStroke && m_BrushTool.lastPlacedObjectInfo != null)
                                {
                                    BrushModeOrientObject(m_BrushTool.lastPlacedObjectInfo);
                                    PositionObject(m_BrushTool.lastPlacedObjectInfo);
                                }


                                if (m_BrushTool.dragDistance + moveLenght >= brushSpacing)
                                {
                                    float d = brushSpacing - m_BrushTool.dragDistance;
                                    Vector3 drawPoint = lastHitPoint + moveDirection * d;
                                    m_BrushTool.dragDistance = 0;
                                    moveLenght -= d;

                                    Vector3 randomPoint = drawPoint + Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, hitNormal) * brushRadius;

                                    RaycastInfo raycastInfo = new RaycastInfo();
                                    Raycast(WorldPointToRay(randomPoint), out raycastInfo, m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);

                                    if (raycastInfo.isHitTargetLayer)
                                    {
                                        PlacedObjectInfo placedObjectInfo = BrushModePlaceObject(raycastInfo, brush);
                                        if (placedObjectInfo != null)
                                        {
                                            if (m_BrushTool.lastPlacedObjectInfo != null)
                                            {
                                                BrushModeFinishPlaceObject(m_BrushTool.lastPlacedObjectInfo);
                                                m_BrushTool.lastPlacedObjectInfo = null;
                                            }
                                            m_BrushTool.lastPlacedObjectInfo = placedObjectInfo;
                                            m_BrushTool.strokeDirectionRefPoint = hitPoint;
                                        }
                                    }

                                    while (moveLenght >= brushSpacing)
                                    {
                                        moveLenght -= brushSpacing;
                                        drawPoint += moveDirection * brushSpacing;

                                        randomPoint = drawPoint + Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, hitNormal) * brushRadius;

                                        Raycast(WorldPointToRay(randomPoint), out raycastInfo, m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);

                                        if (raycastInfo.isHitTargetLayer)
                                        {
                                            PlacedObjectInfo placedObjectInfo = BrushModePlaceObject(raycastInfo, brush);
                                            if (placedObjectInfo != null)
                                            {
                                                if (m_BrushTool.lastPlacedObjectInfo != null)
                                                {
                                                    BrushModeFinishPlaceObject(m_BrushTool.lastPlacedObjectInfo);
                                                    m_BrushTool.lastPlacedObjectInfo = null;
                                                }
                                                m_BrushTool.lastPlacedObjectInfo = placedObjectInfo;
                                                m_BrushTool.strokeDirectionRefPoint = hitPoint;
                                            }
                                        }
                                    }
                                }

                                m_BrushTool.dragDistance += moveLenght;
                            }
                        }
                    }



                    e.Use();
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && e.button == 0)
                {
                    brush.EndStroke();

                    if (m_BrushTool.lastPlacedObjectInfo != null)
                    {
                        BrushModeFinishPlaceObject(m_BrushTool.lastPlacedObjectInfo);
                        m_BrushTool.lastPlacedObjectInfo = null;
                    }

                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;
            case EventType.MouseMove:
                {
                    Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                               m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);

                    // for slope handles info
                    m_BrushTool.firstNormal = m_CurrentRaycast.normal;

                    if (brush != null && brush.settings.gridEnabled)
                        UpdateGrid(brush);

                    e.Use();
                }
                break;
            case EventType.Repaint:
                if (m_CurrentRaycast.isHit)
                {
                    if (brush == null)
                    {
                        DrawErrorHandles(m_CurrentRaycast, Strings.selectBrush);
                    }
                    else
                    if (hasMultipleSelectedBrushes)
                    {
                        DrawErrorHandles(m_CurrentRaycast, Strings.multiSelBrush);
                    }
                    else
                    if (m_CurrentRaycast.isHitMaskedLayer)
                    {
                        DrawMaskedHandles(m_CurrentRaycast, brush);
                    }
                    else
                    {
                        if (brush.settings.gridEnabled)
                            DrawGrid(brush);
                        DrawBrushHandles(m_CurrentRaycast, brush);
                    }
                }
                break;
            case EventType.Layout:
                HandleUtility.AddDefaultControl(controlID);
                break;
            case EventType.KeyDown:
                switch (e.keyCode)
                {
                case KeyCode.F:
                    // F key - Frame camera on brush hit point
                    if (IsModifierDown(EventModifiers.None) && m_CurrentRaycast.isHit)
                    {
                        if(brush != null && !brush.settings.gridEnabled)
                            SceneView.lastActiveSceneView.LookAt(m_CurrentRaycast.point, SceneView.lastActiveSceneView.rotation, brush.settings.brushRadius * 25f);
                        else
                            SceneView.lastActiveSceneView.LookAt(m_CurrentRaycast.point, SceneView.lastActiveSceneView.rotation, Mathf.Max(brush.settings.gridStep.x, brush.settings.gridStep.y) * 10f);
                        e.Use();
                    }
                    break;
                }
                break;
            }
        }


        void DoPinTool()
        {
            bool hasMultipleSelectedBrushes = m_Settings.GetActiveTab().HasMultipleSelectedBrushes();
            Brush brush = m_Settings.GetActiveTab().GetFirstSelectedBrush();

            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(s_PinToolHash, FocusType.Passive);

            switch (e.GetTypeForControl(controlID))
            {
            case EventType.MouseDown:
                if (e.button == 0 && !e.alt && brush != null && !hasMultipleSelectedBrushes)
                {
                    Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                            m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);

                    if (brush.settings.gridEnabled)
                    {
                        m_CurrentRaycast = m_Grid.originRaycastInfo;
                    }

                    if (m_CurrentRaycast.isHitTargetLayer)
                    {
                        brush.BeginStroke();

                        m_PinTool.placedObjectInfo = PlaceObject(m_CurrentRaycast, brush);
                        if (m_PinTool.placedObjectInfo == null)
                            return;

                        brush.EndStroke();

                        m_PinTool.point = m_CurrentRaycast.point;
                        GetOrientation(m_CurrentRaycast.normal, brush.settings.orientationMode, out m_PinTool.upwards, out m_PinTool.right, out m_PinTool.forward);



                        if (brush.settings.pinFixedRotation)
                            OrientObject(m_PinTool.placedObjectInfo, brush.settings.pinFixedRotationValue);
                        else
                            OrientObject(m_PinTool.placedObjectInfo, new Vector3(0, m_PinTool.angle, 0));


                        m_PinTool.scaleFactor = GetObjectScaleFactor(m_PinTool.placedObjectInfo.pivotObject, m_CurrentRaycast);


                        if (brush.settings.pinFixedScale) {
                            m_PinTool.placedObjectInfo.pivotObject.transform.localScale = brush.settings.pinFixedScaleValue;
                        }
                        else
                            m_PinTool.placedObjectInfo.pivotObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                        m_PinTool.radius = 0f;
                    }

                    GUIUtility.hotControl = controlID;
                    e.Use();
                }
                break;
            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlID && e.button == 0 && m_PinTool.placedObjectInfo != null)
                {
                    if (m_PinTool.IntersectsHitPlane(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_PinTool.point))
                    {
                        Vector3 vector = m_PinTool.point - m_PinTool.placedObjectInfo.raycastInfo.point;
                        float vectorLength = vector.magnitude;

                        if (vectorLength < 0.01f)
                        {
                            vector = Vector3.up * 0.01f;
                            vectorLength = 0.01f;
                        }

                        m_PinTool.angle = Vector3.Angle(m_PinTool.forward, vector.normalized);
                        if (Vector3.Dot(vector.normalized, m_PinTool.right) < 0.0f)
                            m_PinTool.angle = -m_PinTool.angle;


                        bool snapScale = e.shift ? !m_Settings.pinSnapScale : m_Settings.pinSnapScale;
                        bool snapRotation = e.control ? !m_Settings.pinSnapRotation : m_Settings.pinSnapRotation;

                        // snap angle
                        if (snapRotation && m_Settings.pinSnapRotationValue > 0)
                            m_PinTool.angle = Mathf.Round(m_PinTool.angle / m_Settings.pinSnapRotationValue) * m_Settings.pinSnapRotationValue;


                        float scale = 2.0f * vectorLength * m_PinTool.scaleFactor;

                        // snap scale
                        if (snapScale && m_Settings.pinSnapScaleValue > 0)
                        {
                            scale = Mathf.Round(scale / m_Settings.pinSnapScaleValue) * m_Settings.pinSnapScaleValue;
                            scale = Mathf.Max(scale, 0.01f);
                        }

                        m_PinTool.radius = scale / (m_PinTool.scaleFactor * 2f);

                        if (!brush.settings.pinFixedRotation)
                            OrientObject(m_PinTool.placedObjectInfo, new Vector3(0, m_PinTool.angle, 0));

                        if (!brush.settings.pinFixedScale)
                        {
                            m_PinTool.placedObjectInfo.pivotObject.transform.localScale = new Vector3(scale, scale, scale);
                        }

                        PivotMode pivotMode = brush.settings.multibrushSlots[m_PinTool.placedObjectInfo.prefabSlot].pivotMode;

                        switch(pivotMode)
                        {
                        case PivotMode.WorldBoundsBottomCenter: pivotMode = PivotMode.BoundsBottomCenter; break;
                        case PivotMode.WorldBoundsCenter: pivotMode = PivotMode.BoundsCenter; break;
                        case PivotMode.WorldBoundsTopCenter: pivotMode = PivotMode.BoundsTopCenter; break;
                        }

                        Vector3 pivot = GetObjectPivot(m_PinTool.placedObjectInfo.pivotObject, pivotMode);

                        m_PinTool.placedObjectInfo.pivotObject.transform.position = m_PinTool.placedObjectInfo.raycastInfo.point
                            - (pivot - m_PinTool.placedObjectInfo.pivotObject.transform.position)
                            + brush.settings.surfaceOffset * m_PinTool.placedObjectInfo.raycastInfo.normal;
                    }

                    e.Use();
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && e.button == 0)
                {
                    if (m_PinTool.placedObjectInfo != null)
                    {
                        FinishPlaceObject(m_PinTool.placedObjectInfo);

                        if (brush.settings.pinFixedScale == false)
                        {
                            Vector2 placeSrceenPoint = HandleUtility.WorldToGUIPoint(m_PinTool.placedObjectInfo.raycastInfo.point);

                            if ((e.mousePosition - placeSrceenPoint).magnitude < 5f)
                            {
                                GameObject.DestroyImmediate(m_PinTool.placedObjectInfo.gameObject);
                            }
                        }

                        m_PinTool.placedObjectInfo = null;
                    }

                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;
            case EventType.MouseMove:
                {
                    Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                               m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);

                    if (brush != null && brush.settings.gridEnabled)
                        UpdateGrid(brush);

                    e.Use();
                }
                break;
            case EventType.Repaint:
                if (m_CurrentRaycast.isHit)
                {
                    if (brush == null)
                    {
                        DrawErrorHandles(m_CurrentRaycast, Strings.selectBrush);
                    }
                    else
                    if (hasMultipleSelectedBrushes)
                    {
                        DrawErrorHandles(m_CurrentRaycast, Strings.multiSelBrush);
                    }
                    else
                    if (m_CurrentRaycast.isHitMaskedLayer)
                    {
                        DrawMaskedHandles(m_CurrentRaycast, brush);
                    }
                    else
                    {
                        if (brush.settings.gridEnabled)
                            DrawGrid(brush);

                        DrawPinToolHandles(brush);
                    }
                }
                break;
            case EventType.Layout:
                HandleUtility.AddDefaultControl(controlID);
                break;
            case EventType.KeyDown:
                switch (e.keyCode)
                {
                case KeyCode.F:
                    // F key - Frame camera on brush hit point
                    if (IsModifierDown(EventModifiers.None) && m_CurrentRaycast.isHit)
                    {
                        SceneView.lastActiveSceneView.LookAt(m_CurrentRaycast.point);
                        e.Use();
                    }
                    break;
                }
                break;
            }
        }


        void DoPlaceTool()
        {
            bool hasMultipleSelectedBrushes = m_Settings.GetActiveTab().HasMultipleSelectedBrushes();
            Brush brush = m_Settings.GetActiveTab().GetFirstSelectedBrush();

            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(s_PlaceToolHash, FocusType.Passive);

            switch (e.GetTypeForControl(controlID))
            {
            case EventType.MouseDown:
                if (e.button == 0 && !e.alt && brush != null && !hasMultipleSelectedBrushes && m_PlaceTool.placedObjectInfo != null)
                {
                    GetOrientation(m_CurrentRaycast.normal, brush.settings.orientationMode, out m_PlaceTool.upwards, out m_PlaceTool.right, out m_PlaceTool.forward);

                    GUIUtility.hotControl = controlID;
                    e.Use();
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && e.button == 0)
                {
                    if (m_PlaceTool.raycastInfo.isHitTargetLayer)
                    {
                        FinishPlaceObject(m_PlaceTool.placedObjectInfo);
                        m_PlaceTool.placedObjectInfo = null;
                    }

                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;
            case EventType.MouseMove:
                Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                               m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);

                if (brush != null && !hasMultipleSelectedBrushes && brush.settings.gridEnabled)
                {
                    UpdateGrid(brush);

                    m_PlaceTool.raycastInfo = m_Grid.originRaycastInfo;
                }
                else
                {
                    m_PlaceTool.raycastInfo = m_CurrentRaycast;
                }


                e.Use();
                break;
            case EventType.Repaint:
                if (m_PlaceTool.raycastInfo.isHit)
                {
                    if (m_PlaceTool.placedObjectInfo != null &&
                        (brush == null || hasMultipleSelectedBrushes || m_PlaceTool.placedObjectInfo.brush != brush))
                    {
                        DestroyObject(m_PlaceTool.placedObjectInfo);
                        m_PlaceTool.placedObjectInfo = null;
                    }

                    if (brush != null && !hasMultipleSelectedBrushes)
                    {
                        // Create preview object
                        if (m_PlaceTool.placedObjectInfo == null && m_PlaceTool.raycastInfo.isHit)
                        {
                            brush.BeginStroke();
                            m_PlaceTool.placedObjectInfo = PlaceObject(m_PlaceTool.raycastInfo, brush);
                            brush.EndStroke();
                        }
                    }

                    // Update object transform
                    if (m_PlaceTool.placedObjectInfo != null)
                    {
                        m_PlaceTool.placedObjectInfo.raycastInfo = m_PlaceTool.raycastInfo;

                        if (brush.settings.placeScale <= 0f)
                            brush.settings.placeScale = 0.1f;

                        OrientObject(m_PlaceTool.placedObjectInfo, brush.settings.placeEulerAngles);
                        m_PlaceTool.placedObjectInfo.pivotObject.transform.localScale = new Vector3(brush.settings.placeScale, brush.settings.placeScale, brush.settings.placeScale);

                        Vector3 pivot = GetObjectPivot(m_PlaceTool.placedObjectInfo.pivotObject, brush.settings.multibrushSlots[m_PlaceTool.placedObjectInfo.prefabSlot].pivotMode);

                        m_PlaceTool.placedObjectInfo.pivotObject.transform.position = m_PlaceTool.placedObjectInfo.raycastInfo.point
                            - (pivot - m_PlaceTool.placedObjectInfo.pivotObject.transform.position)
                            + brush.settings.surfaceOffset * m_PlaceTool.placedObjectInfo.raycastInfo.normal;


                        m_PlaceTool.placedObjectInfo.pivotObject.transform.position =
                            Utility.RoundVector(m_PlaceTool.placedObjectInfo.pivotObject.transform.position, 3);
                    }
                }

                if (m_CurrentRaycast.isHit)
                {

                    if (brush == null)
                    {
                        DrawErrorHandles(m_CurrentRaycast, Strings.selectBrush);
                    }
                    else
                    if (hasMultipleSelectedBrushes)
                    {
                        DrawErrorHandles(m_CurrentRaycast, Strings.multiSelBrush);
                    }
                    else
                    if (m_CurrentRaycast.isHitMaskedLayer)
                    {
                        DrawMaskedHandles(m_CurrentRaycast, brush);
                    }
                    else
                    {
                        if (brush.settings.gridEnabled)
                            DrawGrid(brush);

                        DrawPlaceToolHandles(brush);
                    }
                }
                break;
            case EventType.Layout:
                HandleUtility.AddDefaultControl(controlID);
                break;
            case EventType.KeyDown:
                switch (e.keyCode)
                {
                case KeyCode.F:
                    // F key - Frame camera on brush hit point
                    if (IsModifierDown(EventModifiers.None) && m_CurrentRaycast.isHit)
                    {
                        if (m_PlaceTool.placedObjectInfo != null)
                        {
                            Bounds bounds;
                            GetObjectWorldBounds(m_PlaceTool.placedObjectInfo.pivotObject, out bounds);

                            if (m_CurrentRaycast.isHit)
                                bounds.Encapsulate(m_CurrentRaycast.point);

                            if(bounds.size.magnitude > Mathf.Epsilon)
                                SceneView.lastActiveSceneView.LookAt(bounds.center, SceneView.lastActiveSceneView.rotation, bounds.size.magnitude * 6f);
                            else
                                SceneView.lastActiveSceneView.LookAt(bounds.center);
                        }
                        else
                            SceneView.lastActiveSceneView.LookAt(m_CurrentRaycast.point);

                        e.Use();
                    }
                    break;
                }
                break;
            }
        }



        void ThrowObject(RaycastInfo raycastInfo, Brush brush)
        {
            int prefabSlot = brush.GetPrefabSlotForPlace();
            brush.PrepareNextPrefabForPlace();

            if (prefabSlot == -1 || brush.prefabSlots[prefabSlot].gameObject == null)
                return;

            GameObject gameObject = PrefabUtility.InstantiatePrefab(brush.prefabSlots[prefabSlot].gameObject) as GameObject;
            if (gameObject == null)
                return;

            Undo.RegisterCreatedObjectUndo(gameObject, "PP: Drop Object(s)");

            if (m_Settings.overwritePrefabLayer) {
                Utility.ForAllInHierarchy(gameObject, go => { go.layer = m_Settings.prefabPlaceLayer; });
            }

            gameObject.transform.position = raycastInfo.point + new Vector3(0, brush.settings.throwHeight, 0);

            Vector3 randomVector = UnityEngine.Random.insideUnitSphere * 0.5f;
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(
                brush.settings.throwRandomRotation.x * 3.6f * randomVector.x,
                brush.settings.throwRandomRotation.y * 3.6f * randomVector.y,
                brush.settings.throwRandomRotation.z * 3.6f * randomVector.z));

            gameObject.transform.localScale = Vector3.one * UnityEngine.Random.Range(
                                                  brush.settings.throwScaleMinMax.x,
                                                  brush.settings.throwScaleMinMax.y);

            ParentObject(gameObject, raycastInfo.hitObject, brush.name);

            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.AddForce(Vector3.down * brush.settings.throwVelocity, ForceMode.Impulse);
            }

        }


        void DoThrowTool()
        {
            bool hasMultipleSelectedBrushes = m_Settings.GetActiveTab().HasMultipleSelectedBrushes();
            Brush brush = m_Settings.GetActiveTab().GetFirstSelectedBrush();

            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(s_ThrowToolHash, FocusType.Passive);

            switch (e.GetTypeForControl(controlID))
            {
            case EventType.MouseDown:
                if (e.button == 0 && !e.alt && brush != null && !hasMultipleSelectedBrushes)
                {
                    m_ThrowTool.raycastInfo = m_CurrentRaycast;
                    m_ThrowTool.dragDistance = 0;

                    brush.BeginStroke();

                    if (m_ThrowTool.raycastInfo.isHit)
                    {
                        RaycastInfo raycastInfo = new RaycastInfo();
                        Vector3 randomPoint = m_CurrentRaycast.point
                                              + Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, m_CurrentRaycast.normal)
                                              * brush.settings.throwRadius;;

                        Raycast(WorldPointToRay(randomPoint), out raycastInfo, m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);

                        if(raycastInfo.isHitTargetLayer)
                        {
                            ThrowObject(raycastInfo, brush);
                        }
                    }

                    GUIUtility.hotControl = controlID;
                    e.Use();
                }

                // Freeze
                if (e.button == 1 && !e.alt)
                {
                    foreach (var root in m_CurrentStageScene.GetRootGameObjects())
                        Utility.ForAllInHierarchy(root, obj =>
                        {
                            var rigidbody = obj.GetComponent<Rigidbody>();
                            var rigidbody2D = obj.GetComponent<Rigidbody2D>();
                            if(rigidbody != null) rigidbody.Sleep();
                            if(rigidbody2D != null) rigidbody2D.Sleep();
                        });
                    e.Use();
                }

                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && e.button == 0)
                {
                    brush.EndStroke();

                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;
            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlID && e.button == 0 && brush != null && !hasMultipleSelectedBrushes)
                {
                    Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                               m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);

                    m_ThrowTool.prevRaycast = m_ThrowTool.raycastInfo;
                    m_ThrowTool.raycastInfo = m_CurrentRaycast;

                    if (!m_ThrowTool.raycastInfo.isHit && m_ThrowTool.prevRaycast.isHit)
                        break;

                    Vector3 hitPoint = m_ThrowTool.raycastInfo.point;
                    Vector3 lastHitPoint = m_ThrowTool.prevRaycast.point;
                    Vector3 hitNormal = m_ThrowTool.raycastInfo.isHit ? m_ThrowTool.raycastInfo.normal : m_ThrowTool.prevRaycast.normal;

                    bool isTwoPoints = true;

                    if (!m_ThrowTool.raycastInfo.isHit) {
                        if (!m_ThrowTool.prevRaycast.IntersectsHitPlane(m_ThrowTool.raycastInfo.ray, out hitPoint))
                            isTwoPoints = false;
                    }

                    if (!m_ThrowTool.prevRaycast.isHit) {
                        if (!m_ThrowTool.raycastInfo.IntersectsHitPlane(m_ThrowTool.prevRaycast.ray, out lastHitPoint))
                            isTwoPoints = false;
                    }

                    if (!isTwoPoints || Utility.IsVector3Equal(hitPoint, lastHitPoint))
                        break;

                    float throwRadius = brush.settings.throwRadius;
                    float throwSpacing = Mathf.Max(0.01f, brush.settings.throwSpacing);
                    Vector3 moveVector = (hitPoint - lastHitPoint);
                    float moveLenght = moveVector.magnitude;
                    Vector3 moveDirection = moveVector.normalized;


                    if (m_ThrowTool.dragDistance + moveLenght >= throwSpacing)
                    {
                        float d = throwSpacing - m_ThrowTool.dragDistance;
                        Vector3 drawPoint = lastHitPoint + moveDirection * d;
                        m_ThrowTool.dragDistance = 0;
                        moveLenght -= d;

                        Vector3 randomPoint = drawPoint + Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, hitNormal) * throwRadius;

                        RaycastInfo raycastInfo = new RaycastInfo();
                        Raycast(WorldPointToRay(randomPoint), out raycastInfo, m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);

                        if (raycastInfo.isHitTargetLayer)
                        {
                            ThrowObject(raycastInfo, brush);
                        }

                        while (moveLenght >= throwSpacing)
                        {
                            moveLenght -= throwSpacing;
                            drawPoint += moveDirection * throwSpacing;

                            randomPoint = drawPoint + Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, hitNormal) * throwRadius;

                            Raycast(WorldPointToRay(randomPoint), out raycastInfo, m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);

                            if (raycastInfo.isHitTargetLayer)
                            {
                                ThrowObject(raycastInfo, brush);
                            }
                        }
                    }

                    m_ThrowTool.dragDistance += moveLenght;


                    e.Use();
                }
                break;
            case EventType.MouseMove:
                Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                               m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);

                m_ThrowTool.raycastInfo = m_CurrentRaycast;

                e.Use();
                    break;
            case EventType.Repaint:
                if (m_PlaceTool.raycastInfo.isHit)
                {
                }

                if (m_CurrentRaycast.isHit)
                {
                    if (brush == null)
                    {
                        DrawErrorHandles(m_CurrentRaycast, Strings.selectBrush);
                    }
                    else
                    if (hasMultipleSelectedBrushes)
                    {
                        DrawErrorHandles(m_CurrentRaycast, Strings.multiSelBrush);
                    }
                    else
                    if (m_CurrentRaycast.isHitMaskedLayer)
                    {
                        DrawMaskedHandles(m_CurrentRaycast, brush);
                    }
                    else
                    {
                        DrawDropHandles(m_CurrentRaycast, brush);
                    }
                }
                break;
            case EventType.Layout:
                HandleUtility.AddDefaultControl(controlID);
                break;
            case EventType.KeyDown:
                switch (e.keyCode)
                {
                case KeyCode.F:
                    // F key - Frame camera on brush hit point
                    if (IsModifierDown(EventModifiers.None) && m_CurrentRaycast.isHit)
                    {
                        SceneView.lastActiveSceneView.LookAt(m_CurrentRaycast.point, SceneView.lastActiveSceneView.rotation,
                            brush.settings.throwRadius * 25f);

                        e.Use();
                    }
                    break;
                }
                break;
            }
        }


        void DoEraseTool()
        {
            Brush brush = m_Settings.GetActiveTab().GetFirstSelectedBrush();

            int controlID = GUIUtility.GetControlID(s_EraseToolHash, FocusType.Passive);
            Event e = Event.current;
            EventType eventType = e.GetTypeForControl(controlID);

            switch (eventType)
            {
            case EventType.MouseDown:
            case EventType.MouseDrag:

                if (eventType == EventType.MouseDown && e.button == 0 && !e.alt)
                {
                    // on MouseDown make list of selected prefabs
                    if (!m_Settings.eraseByLayer && brush != null)
                    {
                        m_EraseTool.prefabList.Clear();

                        m_Settings.GetActiveTab().brushes.ForEach((b) => {
                            if (b.selected)
                            {
                                if (b.settings.multibrushEnabled)
                                {
                                    for (int i = 0; i < b.prefabSlots.Length; i++)
                                    {
                                        if (b.prefabSlots[i].gameObject != null && b.settings.multibrushSlots[i].enabled)
                                        {
                                            m_EraseTool.prefabList.Add(b.prefabSlots[i].gameObject);
                                        }
                                    }
                                }
                                else
                                {
                                    GameObject prefab = b.GetFirstAssociatedPrefab();
                                    if (prefab != null)
                                        m_EraseTool.prefabList.Add(prefab);
                                }
                            }
                        });
                    }

                    GUIUtility.hotControl = controlID;
                }


                if (GUIUtility.hotControl == controlID)
                {
                    Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                               ~0, m_Settings.ignoreLayers.value);

                    m_Octree.IntersectSphere(m_CurrentRaycast.point, m_Settings.eraseBrushRadius, (go) =>
                    {
                        if (go == null)
                            return true;

                        GameObject prefabRoot = GetPrefabRoot(go);
                        if (prefabRoot == null)
                            return true;

                        if (m_Settings.eraseByLayer)
                        {
                            if (((1 << prefabRoot.layer) & m_Settings.eraseLayers.value) != 0)
                                DestroyObjectImmediate(prefabRoot);
                        }
                        else if (m_EraseTool.prefabList.Contains(GetCorrespondingObjectFromSource(prefabRoot) as GameObject))
                        {
                            DestroyObjectImmediate(prefabRoot);
                        }

                        return true;
                    });

                    e.Use();
                }


                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && e.button == 0)
                {
                    m_EraseTool.prefabList.Clear();

                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;
            case EventType.MouseMove:
                {
                    Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                               ~0, m_Settings.ignoreLayers.value);

                    e.Use();
                }
                break;
            case EventType.Repaint:
                if (m_CurrentRaycast.isHit)
                {
                    if (!m_Settings.eraseByLayer && brush == null)
                        DrawErrorHandles(m_CurrentRaycast, Strings.selectBrush);
                    else
                        DrawEraseHandles(m_CurrentRaycast, brush);
                }
                break;
            case EventType.Layout:
                HandleUtility.AddDefaultControl(controlID);
                break;
            case EventType.KeyDown:
                switch (e.keyCode)
                {
                case KeyCode.F:
                    // F key - Frame camera on brush hit point
                    if (IsModifierDown(EventModifiers.None) && m_CurrentRaycast.isHit)
                    {
                        SceneView.lastActiveSceneView.LookAt(m_CurrentRaycast.point, SceneView.lastActiveSceneView.rotation, m_Settings.eraseBrushRadius * 25f);
                        e.Use();
                    }
                    break;
                }
                break;
            }
        }


        void DoSelectTool()
        {
            Brush brush = m_Settings.GetActiveTab().GetFirstSelectedBrush();
            bool selectionChanged = false;

            int controlID = GUIUtility.GetControlID(s_SelectToolHash, FocusType.Passive);
            Event e = Event.current;
            EventType eventType = e.GetTypeForControl(controlID);

            switch (eventType)
            {
            case EventType.MouseDown:
            case EventType.MouseDrag:

                SelectMode selectMode;
                if (e.shift)
                    selectMode = SelectMode.Add;
                else if (e.control)
                    selectMode = SelectMode.Substract;
                else
                    selectMode = SelectMode.Replace;


                if (eventType == EventType.MouseDown && e.button == 0 && !e.alt)
                {
                    switch (selectMode)
                    {
                    case SelectMode.Replace:
                        m_SelectionTool.selectedObjects.Clear();
                        selectionChanged = true;
                        break;
                    default:
                        m_SelectionTool.selectedObjects.Clear();
                        m_SelectionTool.selectedObjects.AddRange(Selection.gameObjects);
                        break;
                    }

                    // on MouseDown make list of selected prefabs
                    if (!m_Settings.selectByLayer && brush != null)
                    {
                        m_SelectionTool.prefabList.Clear();

                        // on MouseDown make list of selected prefabs
                        m_Settings.GetActiveTab().brushes.ForEach((b) => {
                            if (b.selected)
                            {
                                if (b.settings.multibrushEnabled)
                                {
                                    for (int i = 0; i < b.prefabSlots.Length; i++)
                                    {
                                        if (b.prefabSlots[i].gameObject != null && b.settings.multibrushSlots[i].enabled)
                                        {
                                            m_SelectionTool.prefabList.Add(b.prefabSlots[i].gameObject);
                                        }
                                    }
                                }
                                else
                                {
                                    GameObject prefab = b.GetFirstAssociatedPrefab();
                                    if (prefab != null)
                                        m_SelectionTool.prefabList.Add(prefab);
                                }
                            }
                        });
                    }

                    GUIUtility.hotControl = controlID;
                }

                if (GUIUtility.hotControl == controlID)
                {
                    Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                               ~0, m_Settings.ignoreLayers.value);

                    m_Octree.IntersectSphere(m_CurrentRaycast.point, m_Settings.selectBrushRadius, (go) =>
                    {
                        if (go == null)
                            return true;

                        GameObject prefabRoot = GetPrefabRoot(go);
                        if (prefabRoot == null)
                            return true;

                        if (m_Settings.selectByLayer)
                        {
                            if (((1 << prefabRoot.layer) & m_Settings.selectLayers.value) == 0)
                                return true;
                        }
                        else if (!m_SelectionTool.prefabList.Contains(GetCorrespondingObjectFromSource(prefabRoot) as GameObject))
                            return true;

                        switch (selectMode)
                        {
                        case SelectMode.Replace:
                        case SelectMode.Add:
                            m_SelectionTool.selectedObjects.Add(prefabRoot);
                            selectionChanged = true;
                            break;
                        case SelectMode.Substract:
                            if (m_SelectionTool.selectedObjects.Contains(prefabRoot))
                            {
                                m_SelectionTool.selectedObjects.Remove(prefabRoot);
                                selectionChanged = true;
                            }
                            break;
                        }
                        return true;
                    });

                    if (selectionChanged)
                        Selection.objects = m_SelectionTool.selectedObjects.ToArray();

                    e.Use();
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && e.button == 0)
                {
                    m_SelectionTool.prefabList.Clear();

                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;
            case EventType.MouseMove:
                {
                    Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                               ~0, m_Settings.ignoreLayers.value);
                    e.Use();
                }
                break;
            case EventType.Repaint:
                if (m_CurrentRaycast.isHit)
                {
                    if (!m_Settings.selectByLayer && brush == null)
                        DrawErrorHandles(m_CurrentRaycast, Strings.selectBrush);
                    else
                        DrawSelectHandles(m_CurrentRaycast, brush);
                }
                break;
            case EventType.Layout:
                HandleUtility.AddDefaultControl(controlID);
                break;
            case EventType.KeyDown:
                switch (e.keyCode)
                {
                case KeyCode.F:
                    // F key - Frame camera on brush hit point
                    if (IsModifierDown(EventModifiers.None) && m_CurrentRaycast.isHit)
                    {
                        SceneView.lastActiveSceneView.LookAt(m_CurrentRaycast.point, SceneView.lastActiveSceneView.rotation, m_Settings.selectBrushRadius * 25f);
                        e.Use();
                    }
                    break;
                }
                break;
            }
        }




        void DoModifyTool()
        {
            Brush brush = m_Settings.GetActiveTab().GetFirstSelectedBrush();

            int controlID = GUIUtility.GetControlID(s_ModifyToolHash, FocusType.Passive);
            Event e = Event.current;
            EventType eventType = e.GetTypeForControl(controlID);

            switch (eventType)
            {
            case EventType.MouseDown:
            case EventType.MouseDrag:

                Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                               ~0, m_Settings.ignoreLayers.value);


                if (eventType == EventType.MouseDown && e.button == 0 && !e.alt)
                {
                    // on MouseDown make list of selected prefabs
                    if (!m_Settings.modifyByLayer && brush != null)
                    {
                        m_ModifyTool.prefabList.Clear();

                        m_Settings.GetActiveTab().brushes.ForEach((b) => {
                            if (b.selected)
                            {
                                if (b.settings.multibrushEnabled)
                                {
                                    for (int i = 0; i < b.prefabSlots.Length; i++)
                                    {
                                        if (b.prefabSlots[i].gameObject != null && b.settings.multibrushSlots[i].enabled)
                                        {
                                            m_ModifyTool.prefabList.Add(b.prefabSlots[i].gameObject);
                                        }
                                    }
                                }
                                else
                                {
                                    GameObject prefab = b.GetFirstAssociatedPrefab();
                                    if (prefab != null)
                                        m_ModifyTool.prefabList.Add(prefab);
                                }
                            }
                        });
                    }

                    m_ModifyTool.updateTicks = 0;

                    GUIUtility.hotControl = controlID;
                    e.Use();
                }


                if (eventType == EventType.MouseDrag && e.button == 0 && !e.alt && GUIUtility.hotControl == controlID)
                {
                    ModifyTool.ModifyInfo modifyInfo = new ModifyTool.ModifyInfo();

                    m_ModifyTool.updateTicks++;

                    m_Octree.IntersectSphere(m_CurrentRaycast.point, m_Settings.modifyBrushRadius, (go) =>
                    {
                        if (go == null)
                            return true;

                        GameObject prefabRoot = GetPrefabRoot(go);
                        if (prefabRoot == null)
                            return true;

                        if (m_Settings.modifyByLayer)
                        {
                            if (((1 << prefabRoot.layer) & m_Settings.modifyLayers.value) == 0)
                                return true;
                        }
                        else
                        {
                            if (!m_ModifyTool.prefabList.Contains(GetCorrespondingObjectFromSource(prefabRoot) as GameObject))
                                return true;
                        }

                        if (!m_ModifyTool.modifiedObjects.TryGetValue(prefabRoot, out modifyInfo))
                        {
                            modifyInfo.pivot = GetObjectPivot(prefabRoot, m_Settings.modifyPivotMode);
                            modifyInfo.initialPosition = prefabRoot.transform.position;
                            modifyInfo.initialRotation = prefabRoot.transform.rotation;
                            modifyInfo.initialScale = prefabRoot.transform.localScale;
                            modifyInfo.initialUp = prefabRoot.transform.up;

                            modifyInfo.randomScale = 1f - UnityEngine.Random.value * (m_Settings.modifyScaleRandomize * 0.01f);

                            Vector3 randomVector = UnityEngine.Random.insideUnitSphere;

                            modifyInfo.randomRotation = new Vector3(
                                m_Settings.modifyRandomRotationValues.x * randomVector.x,
                                m_Settings.modifyRandomRotationValues.y * randomVector.y,
                                m_Settings.modifyRandomRotationValues.z * randomVector.z);

                            modifyInfo.currentScale = 1.0f;

                            Undo.RegisterCompleteObjectUndo(prefabRoot.transform, "Modify");

                            m_Octree.RemoveGameObject(prefabRoot);
                            m_Octree.AddDynamicObject(prefabRoot, m_Settings.useAdditionalVertexStreams);
                        }



                        if (modifyInfo.lastUpdate != m_ModifyTool.updateTicks)
                        {
                            modifyInfo.lastUpdate = m_ModifyTool.updateTicks;

                            Transform transform = prefabRoot.transform;

                            float moveLenght = e.delta.magnitude;
                            float distance = Mathf.Min((m_CurrentRaycast.point - modifyInfo.pivot).magnitude, m_Settings.modifyBrushRadius);
                            float falloff = 2f - distance / Mathf.Max(m_Settings.modifyBrushRadius, 0.001f);
                            float strength = m_Settings.modifyStrength * moveLenght;

                            modifyInfo.currentScale += m_Settings.modifyScale * strength * falloff * modifyInfo.randomScale * 0.005f;


                            Quaternion qRotation;
                            if(m_Settings.modifyRandomRotation)
                            {
                                qRotation = transform.rotation * Quaternion.Euler(modifyInfo.randomRotation * falloff * strength * 0.1f);
                            }
                            else
                            {
                                qRotation = transform.rotation * Quaternion.Euler(m_Settings.modifyRotationValues * falloff * strength * 0.1f);
                            }

                            Quaternion qPosition = qRotation * Quaternion.Inverse(modifyInfo.initialRotation);

                            transform.rotation = qRotation;
                            transform.position = modifyInfo.pivot + modifyInfo.currentScale * (qPosition * (modifyInfo.initialPosition - modifyInfo.pivot));
                            transform.localScale = modifyInfo.initialScale * modifyInfo.currentScale;
                        }


                        m_ModifyTool.modifiedObjects[prefabRoot] = modifyInfo;

                        return true;
                    });

                    e.Use();
                }

                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && e.button == 0)
                {
                    m_ModifyTool.prefabList.Clear();
                    m_ModifyTool.modifiedObjects.Clear();

                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;
            case EventType.MouseMove:
                {
                    Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                               ~0, m_Settings.ignoreLayers.value);

                    e.Use();
                }
                break;
            case EventType.Repaint:
                if (m_CurrentRaycast.isHit)
                {
                    if (!m_Settings.modifyByLayer && brush == null)
                        DrawErrorHandles(m_CurrentRaycast, Strings.selectBrush);
                    else
                        DrawModifyHandles(m_CurrentRaycast, brush);
                }
                break;
            case EventType.Layout:
                HandleUtility.AddDefaultControl(controlID);
                break;
            case EventType.KeyDown:
                switch (e.keyCode)
                {
                case KeyCode.F:
                    // F key - Frame camera on brush hit point
                    if (IsModifierDown(EventModifiers.None) && m_CurrentRaycast.isHit)
                    {
                        SceneView.lastActiveSceneView.LookAt(m_CurrentRaycast.point, SceneView.lastActiveSceneView.rotation, m_Settings.modifyBrushRadius * 10f);
                        e.Use();
                    }
                    break;
                }
                break;
            }
        }



        void DoOrientTool()
        {
            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(s_OrientToolHash, FocusType.Passive);
            EventType eventType = e.GetTypeForControl(controlID);

            switch (eventType)
            {
            case EventType.MouseDown:
            case EventType.MouseDrag:

                Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                               m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);

                if (eventType == EventType.MouseDown && e.button == 0 && !e.alt)
                {
                    GameObject[] selectedObjects = Selection.gameObjects;
                    for(int i = 0; i < selectedObjects.Length; i++)
                    {
                        GameObject gameObject = selectedObjects[i];

                        if (GetPrefabRoot(gameObject) == null)
                            continue;

                        OrientTool.ObjectInfo objectInfo = new OrientTool.ObjectInfo();
                        objectInfo.prefabRoot = gameObject;
                        objectInfo.initialPosition = gameObject.transform.position;
                        objectInfo.initialRotation = gameObject.transform.rotation;
                        objectInfo.initialUp = gameObject.transform.up;
                        objectInfo.pivot = GetObjectPivot(gameObject, m_Settings.orientPivotMode);

                        if (i == 0)
                            m_OrientTool.objectsCenter = objectInfo.pivot;
                        else
                            m_OrientTool.objectsCenter = (m_OrientTool.objectsCenter + objectInfo.pivot) * 0.5f;

                        m_OrientTool.objects.Add(objectInfo);
                    }

                    GUIUtility.hotControl = controlID;
                    e.Use();
                }


                if (e.button == 0 && !e.alt && GUIUtility.hotControl == controlID)
                {
                    if (m_CurrentRaycast.isHit)
                    {
                        foreach (OrientTool.ObjectInfo objectInfo in m_OrientTool.objects)
                        {
                            Transform transform = objectInfo.prefabRoot.transform;
                            Vector3 forward;

                            if(m_Settings.orientSameDirection)
                                forward = (m_CurrentRaycast.point - m_OrientTool.objectsCenter).normalized;
                            else
                                forward = (m_CurrentRaycast.point - objectInfo.pivot).normalized;

                            if (m_Settings.orientFlipDirection)
                                forward = -forward;

                            if (m_Settings.orientLockUp)
                                forward = Vector3.ProjectOnPlane(forward, objectInfo.initialUp).normalized;

                            Quaternion quaternion = Quaternion.LookRotation(forward, objectInfo.initialUp) * Quaternion.Euler(m_Settings.orientRotation);
                            Quaternion qPos = quaternion * Quaternion.Inverse(objectInfo.initialRotation);

                            Undo.RegisterCompleteObjectUndo(transform, "Orient Object(s)");

                            transform.position = objectInfo.pivot + qPos * (objectInfo.initialPosition - objectInfo.pivot);
                            transform.rotation = quaternion;
                        }
                    }

                    e.Use();
                }

                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && e.button == 0)
                {
                    m_OrientTool.objects.Clear();
                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;
            case EventType.MouseMove:
                Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                               m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);


                e.Use();
                break;
            case EventType.Repaint:
                if (m_CurrentRaycast.isHit)
                {
                    if(GUIUtility.hotControl == controlID)
                    {
                        if (m_OrientTool.objects.Count > 0)
                            DrawOrientToolHandles(m_CurrentRaycast, true);
                        else
                            DrawErrorHandles(m_CurrentRaycast, "Select objects");
                    }
                    else
                        DrawOrientToolHandles(m_CurrentRaycast, false);

                }
                break;
            case EventType.Layout:
                HandleUtility.AddDefaultControl(controlID);
                break;
            }
        }


        void MoveToolReloadSelection()
        {
            m_MoveTool.objects.Clear();

            GameObject[] selectedObjects = Selection.gameObjects;
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                GameObject gameObject = selectedObjects[i];

                if (GetPrefabRoot(gameObject) == null)
                    continue;

                MoveTool.ObjectInfo objectInfo = new MoveTool.ObjectInfo();
                objectInfo.prefabRoot = gameObject;
                objectInfo.initialPosition = gameObject.transform.position;
                objectInfo.initialRotation = gameObject.transform.rotation;
                objectInfo.initialUp = gameObject.transform.up;
                objectInfo.initialForward = gameObject.transform.forward;
                objectInfo.pivot = GetObjectPivot(gameObject, m_Settings.movePivotMode);

                if (i == 0)
                {
                    m_MoveTool.initialObjectsCenter = GetObjectPivot(gameObject, PivotMode.WorldBoundsCenter);
                }
                else
                {
                    m_MoveTool.initialObjectsCenter = (m_MoveTool.initialObjectsCenter + GetObjectPivot(gameObject, PivotMode.WorldBoundsCenter)) * 0.5f;
                }

                m_MoveTool.objects.Add(objectInfo);
            }

            m_MoveTool.objectsCenter = m_MoveTool.initialObjectsCenter;

        }



        void DoMoveTool()
        {
            Event e = Event.current;

            int controlID = GUIUtility.GetControlID(s_MoveToolHash, FocusType.Passive);
            EventType eventType = e.GetTypeForControl(controlID);

            switch (eventType)
            {
            case EventType.MouseDown:
            case EventType.MouseDrag:

                Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                               m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);

                if (m_CurrentRaycast.isHit)
                    m_MoveTool.lastHitRaycast = m_CurrentRaycast;

                if (eventType == EventType.MouseDown && e.button == 0 && !e.alt && m_MoveTool.objects.Count > 0)
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                    if (PointLineDistance(m_MoveTool.objectsCenter, ray.origin, ray.direction) < m_MoveTool.handleDiskSize)
                    {
                        if (m_CurrentRaycast.isHit)
                        {
                            m_MoveTool.dragStart = m_CurrentRaycast.point;
                            GUIUtility.hotControl = controlID;
                            e.Use();
                        }
                        else
                        {
                            Plane diskHandlePlane = new Plane(-ray.direction, m_MoveTool.objectsCenter);

                            float enter;

                            if (diskHandlePlane.Raycast(ray, out enter))
                            {
                                m_MoveTool.dragStart = ray.GetPoint(enter);
                                GUIUtility.hotControl = controlID;
                                e.Use();
                            }
                        }
                    }
                }


                if (e.button == 0 && !e.alt && GUIUtility.hotControl == controlID)
                {
                    Vector3 hitPoint;

                    if (m_CurrentRaycast.isHit)
                        hitPoint = m_CurrentRaycast.point;
                    else
                    {
                        if (m_MoveTool.lastHitRaycast.isHit)
                            m_MoveTool.lastHitRaycast.IntersectsHitPlane(HandleUtility.GUIPointToWorldRay(e.mousePosition), out hitPoint);
                        else
                            hitPoint = m_MoveTool.dragStart;
                    }


                    Vector3 move = hitPoint - m_MoveTool.dragStart;
                    m_MoveTool.objectsCenter = m_MoveTool.initialObjectsCenter + move;


                    int objCount = m_MoveTool.objects.Count;
                    for (int i = 0; i < objCount; i++)
                    {
                        MoveTool.ObjectInfo objectInfo = m_MoveTool.objects[i];
                        Transform transform = objectInfo.prefabRoot.transform;

                        Vector3 surfaceNormal = transform.up;
                        Vector3 newpos = objectInfo.pivot + move;
                        Ray ray = WorldPointToRay(newpos);

                        RaycastInfo raycast;
                        if (Raycast(ray, out raycast, m_Settings.paintLayers.value, m_Settings.ignoreLayers.value))
                        {
                            newpos = raycast.point;
                            surfaceNormal = raycast.normal;
                        }
                        else
                        {
                            if (m_MoveTool.lastHitRaycast.isHit && m_MoveTool.lastHitRaycast.IntersectsHitPlane(ray, out hitPoint))
                            {
                                newpos = hitPoint;
                                surfaceNormal = m_MoveTool.lastHitRaycast.normal;
                            }
                            else
                                newpos = transform.position;
                        }

                        Undo.RegisterCompleteObjectUndo(transform, "Move Object(s)");

                        newpos = newpos + surfaceNormal.normalized * m_Settings.moveSurfaceOffset;

                        Quaternion rotation = objectInfo.initialRotation;

                        if (!m_Settings.moveLockUp)
                        {
                            Vector3 upwards;
                            switch (m_Settings.moveOrientationMode)
                            {
                            default: case OrientationMode.SurfaceNormal: upwards = surfaceNormal; break;
                            case OrientationMode.SurfaceNormalNegative: upwards = -surfaceNormal; break;
                            case OrientationMode.X: upwards = new Vector3(1, 0, 0); break;
                            case OrientationMode.XNegative: upwards = new Vector3(-1, 0, 0); break;
                            case OrientationMode.Y: upwards = new Vector3(0, 1, 0); break;
                            case OrientationMode.YNegative: upwards = new Vector3(0, -1, 0); break;
                            case OrientationMode.Z: upwards = new Vector3(0, 0, 1); break;
                            case OrientationMode.ZNegative: upwards = new Vector3(0, 0, -1); break;
                            }

                            Vector3 right = Vector3.Cross(objectInfo.initialForward, upwards);
                            Vector3 forward = Vector3.Cross(upwards, right);

                            rotation = Quaternion.LookRotation(forward, upwards);
                        }

                        Quaternion qPos = rotation * Quaternion.Inverse(objectInfo.initialRotation);

                        transform.rotation = rotation;
                        transform.position = newpos +  qPos * (objectInfo.initialPosition - objectInfo.pivot);
                    }

                    e.Use();
                }

                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && e.button == 0)
                {
                    MoveToolReloadSelection();

                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;
            case EventType.MouseMove:
                Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out m_CurrentRaycast,
                                               m_Settings.paintLayers.value, m_Settings.ignoreLayers.value);

                e.Use();
                break;
            case EventType.Repaint:
                if (m_MoveTool.objects.Count > 0)
                    DrawMoveToolHandles(GUIUtility.hotControl == controlID);
                break;
            case EventType.Layout:
                if (m_MoveTool.objects.Count > 0)
                    HandleUtility.AddControl(controlID, HandleUtility.DistanceToDisc(m_MoveTool.objectsCenter, -HandleUtility.GUIPointToWorldRay(e.mousePosition).direction, m_MoveTool.handleDiskSize));
                break;
            }
        }


        Ray WorldPointToRay(Vector3 worldSpacePoint)
        {
            if(Camera.current == null)
                return new Ray();
            return new Ray(Camera.current.transform.position, (worldSpacePoint - Camera.current.transform.position).normalized);
        }


#endregion // Scene UI

    } // class PrefabPainter


} // namespace nTools.PrefabPainter


