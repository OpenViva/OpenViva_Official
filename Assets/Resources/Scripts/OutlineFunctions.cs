using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Viva
{

    public delegate IEnumerator OutlineCoroutine(Outline.Entry outlineEntry, Color color);


    public static class Outline
    {
        public class Entry
        {
            public Coroutine outlineCoroutine;
            private readonly List<Material> lastTargetMats = new List<Material>();
            public OutlineInstance outline;
            

            public Entry(Renderer renderer)
            {


                if (renderer == null) return;
                if (renderer.gameObject.TryGetComponent<OutlineInstance>(out outline))
                {
                    CancelDestroy(outline);
                }
                else
                {
                    outline = renderer.gameObject.AddComponent<OutlineInstance>();

                }
                if (outline == null) return;

                outline.OutlineMode = OutlineInstance.Mode.OutlineVisible;
            }

            public void SetOutline(Color color, float width)
            {
                if (outline == null) return;
                outline.OutlineColor = color;
                outline.OutlineWidth = width;

            }
        }

        private struct DestroyEntry
        {
            public Coroutine coroutine;
            public int id;

            public DestroyEntry(Coroutine _coroutine, int _id)
            {
                coroutine = _coroutine;
                id = _id;
            }
        }

        private static List<Entry> outlineEntries = new List<Entry>();
        private static List<DestroyEntry> destroyQueue = new List<DestroyEntry>();

        private static void QueueForDestroy(OutlineInstance outline)
        {
            if (outline == null) return;

            var instanceId = outline.GetInstanceID();
            var entry = new DestroyEntry(GameDirector.instance.StartCoroutine(DestroyOnUpdate(instanceId, outline)), instanceId);
            destroyQueue.Add(entry);
        }

        private static IEnumerator DestroyOnUpdate(int id, OutlineInstance outline)
        {
            yield return null;

            for (int i = destroyQueue.Count; i-- > 0;)
            {
                var entry = destroyQueue[i];
                if (entry.id == id)
                {
                    destroyQueue.RemoveAt(i);
                }
            }
            if (outline) GameObject.DestroyImmediate(outline);
        }

        private static void CancelDestroy(OutlineInstance outline)
        {
            if (outline == null) return;
            for (int i = destroyQueue.Count; i-- > 0;)
            {
                var entry = destroyQueue[i];
                if (entry.id == outline.GetInstanceID())
                {
                    destroyQueue.RemoveAt(i);
                    GameDirector.instance.StopCoroutine(entry.coroutine);
                }
            }
        }

        public static Outline.Entry StartOutlining(Renderer renderer, Color color, OutlineCoroutine outlineCoroutine)
        {
            if (renderer == null) return null;

            var entry = new Outline.Entry(renderer);
            if (outlineCoroutine != null) entry.outlineCoroutine = GameDirector.instance.StartCoroutine(outlineCoroutine(entry, color));
            outlineEntries.Add(entry);
            return entry;
        }

        public static void StopOutlining(Entry entry)
        {
            if (entry == null) return;

            if (entry.outlineCoroutine != null) GameDirector.instance.StopCoroutine(entry.outlineCoroutine);
            QueueForDestroy(entry.outline);

            outlineEntries.Remove(entry);
        }

        public static IEnumerator Flash(Outline.Entry entry, Color color)
        {
            float timer = 0;
            float duration = 0.75f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float ratio = 1.0f - Mathf.Clamp01(timer / duration);
                ratio = 1.0f - Mathf.Pow(ratio, 4);
                entry.SetOutline(color * ratio, ratio * 8);
                yield return null;
            }
            Outline.StopOutlining(entry);
        }

        public static IEnumerator Constant(Entry outlineEntry, Color color)
        {
            while (true)
            {
                outlineEntry.SetOutline(color, Mathf.Sin(Time.time * 8.0f) * 2 + 4.0f);
                yield return null;
            }
        }
    }

    public class OutlineInstance : MonoBehaviour
{
    private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();

    public enum Mode
    {
        OutlineAll,
        OutlineVisible,
        OutlineHidden,
        OutlineAndSilhouette,
        SilhouetteOnly
    }

    public Mode OutlineMode
    {
        get { return outlineMode; }
        set
        {
            outlineMode = value;
            needsUpdate = true;
        }
    }

    public Color OutlineColor
    {
        get { return outlineColor; }
        set
        {
            outlineColor = value;
            needsUpdate = true;
        }
    }

    public float OutlineWidth
    {
        get { return outlineWidth; }
        set
        {
            outlineWidth = value;
            needsUpdate = true;
        }
    }

    [Serializable]
    private class ListVector3
    {
        public List<Vector3> data;
    }

    [SerializeField]
    private Mode outlineMode;

    [SerializeField]
    private Color outlineColor = Color.white;

    [SerializeField, Range(0f, 10f)]
    private float outlineWidth = 2f;

    [Header("Optional")]

    [SerializeField, Tooltip("Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. "
    + "Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
    private bool precomputeOutline;

    [SerializeField, HideInInspector]
    private List<Mesh> bakeKeys = new List<Mesh>();

    [SerializeField, HideInInspector]
    private List<ListVector3> bakeValues = new List<ListVector3>();

    private Renderer[] renderers;
    private Material outlineMaskMaterial;
    private Material outlineFillMaterial;

    private bool needsUpdate;

    void Awake()
    {

        // Cache renderers
        renderers = GetComponentsInChildren<Renderer>();

        // Instantiate outline materials
        outlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineMask"));
        outlineFillMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineFill"));

        outlineMaskMaterial.name = "OutlineMask (Instance)";
        outlineFillMaterial.name = "OutlineFill (Instance)";

        // Retrieve or generate smooth normals
        LoadSmoothNormals();

        // Apply material properties immediately
        needsUpdate = true;
    }

    void OnEnable()
    {
        foreach (var renderer in renderers)
        {

            // Append outline shaders
            var materials = renderer.sharedMaterials.ToList();

            materials.Add(outlineMaskMaterial);
            materials.Add(outlineFillMaterial);

            renderer.materials = materials.ToArray();
        }
    }

    void OnValidate()
    {

        // Update material properties
        needsUpdate = true;

        // Clear cache when baking is disabled or corrupted
        if (!precomputeOutline && bakeKeys.Count != 0 || bakeKeys.Count != bakeValues.Count)
        {
            bakeKeys.Clear();
            bakeValues.Clear();
        }

        // Generate smooth normals when baking is enabled
        if (precomputeOutline && bakeKeys.Count == 0)
        {
            Bake();
        }
    }

    void Update()
    {
        if (needsUpdate)
        {
            needsUpdate = false;

            UpdateMaterialProperties();
        }
    }

    void OnDisable()
    {
        foreach (var renderer in renderers)
        {

            // Remove outline shaders
            var materials = renderer.sharedMaterials.ToList();

            var extraOutlineMask = materials.Find(m => m.name == "OutlineMask (Instance) (Instance)");
            if (extraOutlineMask != null)
            {
                materials.Remove(extraOutlineMask);
                Destroy(extraOutlineMask);
            }

            var extraOutlineFill = materials.Find(m => m.name == "OutlineFill (Instance) (Instance)");
            if (extraOutlineFill != null)
            {
                materials.Remove(extraOutlineFill);
                Destroy(extraOutlineFill);
            }


            materials.Remove(outlineMaskMaterial);
            materials.Remove(outlineFillMaterial);

            renderer.materials = materials.ToArray();
        }
    }

        void OnDestroy()
        {
            foreach (var renderer in renderers)
            {
                var materials = renderer.sharedMaterials.ToList();

                // Check for extra instance names and destroy them
                var extraOutlineMask = materials.Find(m => m.name == "OutlineMask (Instance) (Instance)");
                if (extraOutlineMask != null)
                {
                    materials.Remove(extraOutlineMask);
                    Destroy(extraOutlineMask);
                }

                var extraOutlineFill = materials.Find(m => m.name == "OutlineFill (Instance) (Instance)");
                if (extraOutlineFill != null)
                {
                    materials.Remove(extraOutlineFill);
                    Destroy(extraOutlineFill);
                }

                // Remove outline shaders
                if (materials.Contains(outlineMaskMaterial))
                {
                    materials.Remove(outlineMaskMaterial);
                    Destroy(outlineMaskMaterial);
                }

                if (materials.Contains(outlineFillMaterial))
                {
                    materials.Remove(outlineFillMaterial);
                    Destroy(outlineFillMaterial);
                }

                renderer.materials = materials.ToArray();
            }
        }

    void Bake()
    {

        // Generate smooth normals for each mesh
        var bakedMeshes = new HashSet<Mesh>();

        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {

            // Skip duplicates
            if (!bakedMeshes.Add(meshFilter.sharedMesh))
            {
                continue;
            }

            // Serialize smooth normals
            var smoothNormals = SmoothNormals(meshFilter.sharedMesh);

            bakeKeys.Add(meshFilter.sharedMesh);
            bakeValues.Add(new ListVector3() { data = smoothNormals });
        }
    }

    void LoadSmoothNormals()
    {

        // Retrieve or generate smooth normals
        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {

            // Skip if smooth normals have already been adopted
            if (!registeredMeshes.Add(meshFilter.sharedMesh))
            {
                continue;
            }

            // Retrieve or generate smooth normals
            var index = bakeKeys.IndexOf(meshFilter.sharedMesh);
            var smoothNormals = (index >= 0) ? bakeValues[index].data : SmoothNormals(meshFilter.sharedMesh);

            // Store smooth normals in UV3
            meshFilter.sharedMesh.SetUVs(3, smoothNormals);

            // Combine submeshes
            var renderer = meshFilter.GetComponent<Renderer>();

            if (renderer != null)
            {
                CombineSubmeshes(meshFilter.sharedMesh, renderer.sharedMaterials);
            }
        }

        // Clear UV3 on skinned mesh renderers
        foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
        {

            // Skip if UV3 has already been reset
            if (!registeredMeshes.Add(skinnedMeshRenderer.sharedMesh))
            {
                continue;
            }

            // Clear UV3
            skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];

            // Combine submeshes
            CombineSubmeshes(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer.sharedMaterials);
        }
    }

    List<Vector3> SmoothNormals(Mesh mesh)
    {

        // Group vertices by location
        var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);

        // Copy normals to a new list
        var smoothNormals = new List<Vector3>(mesh.normals);

        // Average normals for grouped vertices
        foreach (var group in groups)
        {

            // Skip single vertices
            if (group.Count() == 1)
            {
                continue;
            }

            // Calculate the average normal
            var smoothNormal = Vector3.zero;

            foreach (var pair in group)
            {
                smoothNormal += smoothNormals[pair.Value];
            }

            smoothNormal.Normalize();

            // Assign smooth normal to each vertex
            foreach (var pair in group)
            {
                smoothNormals[pair.Value] = smoothNormal;
            }
        }

        return smoothNormals;
    }

    void CombineSubmeshes(Mesh mesh, Material[] materials)
    {

        // Skip meshes with a single submesh
        if (mesh.subMeshCount == 1)
        {
            return;
        }

        // Skip if submesh count exceeds material count
        if (mesh.subMeshCount > materials.Length)
        {
            return;
        }

        // Append combined submesh
        mesh.subMeshCount++;
        mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
    }

    void UpdateMaterialProperties()
    {

        // Apply properties according to mode
        outlineFillMaterial.SetColor("_OutlineColor", outlineColor);

        switch (outlineMode)
        {
            case Mode.OutlineAll:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
                break;

            case Mode.OutlineVisible:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
                break;

            case Mode.OutlineHidden:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
                break;

            case Mode.OutlineAndSilhouette:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
                break;

            case Mode.SilhouetteOnly:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
                outlineFillMaterial.SetFloat("_OutlineWidth", 0f);
                break;
        }
    }
}
}