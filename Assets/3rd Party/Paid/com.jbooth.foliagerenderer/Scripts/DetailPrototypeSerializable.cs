//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;


namespace JBooth.FoliageRendering
{
    // because unity are dicks about not serializing these normally
    // and using sealed class so I can't just make a serializable version
    [System.Serializable]
    public class DetailPrototypeSerializable
    {
        public GameObject prototype;
        public float alignToGround;
        public float density;
        public Color dryColor = Color.white;
        public Color healthyColor = Color.white;
        public float holeEdgePadding;
        public float minWidth;
        public float maxWidth;
        public float minHeight;
        public float maxHeight;
        public int noiseSeed;
        public float noiseSpread;
        public float positionJitter;
        public Texture2D prototypeTexture;
        public DetailRenderMode renderMode;
        public float targetCoverage;
        public bool useInstancing;
        public bool useDensityScaling;
        public bool usePrototypeMesh;

        public override int GetHashCode()
        {
            return System.HashCode.Combine(prototype);
        }

        
        public static bool operator ==(DetailPrototypeSerializable obj1, DetailPrototypeSerializable obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;
            if (ReferenceEquals(obj1, null))
                return false;
            if (ReferenceEquals(obj2, null))
                return false;
            return obj1.Equals(obj2);
        }

        public static bool operator !=(DetailPrototypeSerializable obj1, DetailPrototypeSerializable obj2) => !(obj1 == obj2);

        public override bool Equals(object obj) => Equals(obj as DetailPrototypeSerializable);

        
        public bool Equals(DetailPrototypeSerializable x)
        {
            if (ReferenceEquals(x, null))
                return false;
            if (ReferenceEquals(this, x))
                return true;
            bool r = true;
            r &= x.prototype == prototype;

    #if UNITY_2022_2_OR_NEWER
            r &= x.alignToGround == alignToGround;
            r &= x.targetCoverage == targetCoverage;
            r &= x.useDensityScaling == useDensityScaling;
            r &= x.positionJitter == positionJitter;
            r &= x.density == density;
    #endif
            r &= x.dryColor == dryColor;
            r &= x.healthyColor == healthyColor;
            r &= x.holeEdgePadding == holeEdgePadding;
            r &= x.maxHeight == maxHeight;
            r &= x.minHeight == minHeight;
            r &= x.maxWidth == maxWidth;
            r &= x.minWidth == minWidth;
            r &= x.noiseSeed == noiseSeed;
            r &= x.noiseSpread == noiseSpread;
            r &= x.prototypeTexture == prototypeTexture;
            r &= x.renderMode == renderMode;
            r &= x.useInstancing == useInstancing;
            r &= x.usePrototypeMesh == usePrototypeMesh;
            return r;
        }


        public int GetHashCode(DetailPrototypeSerializable t)
        {
            if (Object.ReferenceEquals(t, null)) return 0;

            int r = 0;
            r ^= prototype == null ? 0 : prototype.GetHashCode();

    #if UNITY_2022_2_OR_NEWER
            r ^= alignToGround.GetHashCode();
            r ^= targetCoverage.GetHashCode();
            r ^= useDensityScaling.GetHashCode();
            r ^= positionJitter.GetHashCode();
            r ^= density.GetHashCode();
    #endif
            r ^= dryColor.GetHashCode();
            r ^= healthyColor.GetHashCode();
            r ^= holeEdgePadding.GetHashCode();
            r ^= maxHeight.GetHashCode();
            r ^= minHeight.GetHashCode();
            r ^= maxWidth.GetHashCode();
            r ^= minWidth.GetHashCode();
            r ^= noiseSeed.GetHashCode();
            r ^= noiseSpread.GetHashCode();
            r ^= prototypeTexture == null ? 0 : prototypeTexture.GetHashCode();
            r ^= renderMode.GetHashCode();
            r ^= useInstancing.GetHashCode();
            r ^= usePrototypeMesh.GetHashCode();
            return r;
        }

        public bool IsValid()
        {
            if (usePrototypeMesh && prototype == null) return false;
            if (!usePrototypeMesh && prototypeTexture == null) return false;
            return true;
        }

        public DetailPrototypeSerializable()
        {
            ResetToMesh(false);
        }

        public void ResetToMesh(bool seed = true)
        {
            noiseSeed = 0;
            if (seed)
            {
                noiseSeed = UnityEngine.Random.Range(1, int.MaxValue);
            }
            useDensityScaling = true;
            usePrototypeMesh = true;
            noiseSpread = 1;
            useInstancing = true;
            minHeight = 1;
            minWidth = 1;
            maxHeight = 2;
            maxWidth = 2;
            density = 1;
            renderMode = DetailRenderMode.VertexLit;
        }

        public void ResetToTexture()
        {
            ResetToMesh();
            
            renderMode = DetailRenderMode.GrassBillboard;
            usePrototypeMesh = false;
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null && UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.terrainDetailGrassBillboardShader == null)
                renderMode = DetailRenderMode.Grass;
        }

        public DetailPrototypeSerializable(DetailPrototype d)
        {
            prototype = d.prototype;

    #if UNITY_2022_2_OR_NEWER
            alignToGround = d.alignToGround;
            positionJitter = d.positionJitter;
            targetCoverage = d.targetCoverage;
            useDensityScaling = d.useDensityScaling;
            density = d.density;
    #endif
            dryColor = d.dryColor;
            healthyColor = d.healthyColor;
            holeEdgePadding = d.holeEdgePadding;
            maxHeight = d.maxHeight;
            minHeight = d.minHeight;
            maxWidth = d.maxWidth;
            minWidth = d.minWidth;
            useInstancing = d.useInstancing;
            noiseSeed = d.noiseSeed;
            noiseSpread = d.noiseSpread;
            
            prototypeTexture = d.prototypeTexture;
            renderMode = d.renderMode;
            usePrototypeMesh = d.usePrototypeMesh;
        }
        public DetailPrototype GetPrototype()
        {
            var detail = new DetailPrototype();
            detail.prototype = prototype;

            if (prototype != null)
            {
                var lod = prototype.GetComponent<LODGroup>();
                if (lod != null)
                {
                    var lodTransform = lod.transform;
                    foreach (Transform child in lodTransform)
                    {
                        var renderer = child.GetComponent<Renderer>();
                        if (renderer != null && child.GetSiblingIndex() == 0)
                        {
                            detail.prototype = child.gameObject;
                        }
                    }

                }
            }

    #if UNITY_2022_2_OR_NEWER
            detail.alignToGround = alignToGround;
            detail.positionJitter = positionJitter;
            detail.targetCoverage = targetCoverage;
            detail.useDensityScaling = useDensityScaling;
            detail.density = density;
    #endif
            detail.dryColor = dryColor;
            detail.healthyColor = healthyColor;
            detail.holeEdgePadding = holeEdgePadding;
            detail.maxHeight = maxHeight;
            detail.minHeight = minHeight;
            detail.maxWidth = maxWidth;
            detail.minWidth = minWidth;
            detail.useInstancing = useInstancing;
            detail.noiseSeed = noiseSeed;
            detail.noiseSpread = noiseSpread;
            
            detail.prototypeTexture = prototypeTexture;
            detail.renderMode = renderMode;
            detail.usePrototypeMesh = usePrototypeMesh;

            if (detail.usePrototypeMesh == false)
            {
                detail.prototype = null;
                detail.useInstancing = false; // crashes unity if true
            }
            else
            {
                detail.prototypeTexture = null;
            }

            return detail;
        }

        public bool IsEqualToDetail(DetailPrototype detail)
        {
            bool r = true;
            r &= detail.prototype == prototype || detail.prototype?.transform.root?.gameObject == prototype;
    #if UNITY_2022_2_OR_NEWER
            r &= detail.alignToGround == alignToGround;
            r &= detail.targetCoverage == targetCoverage;
            r &= detail.useDensityScaling == useDensityScaling;
            r &= detail.positionJitter == positionJitter;
            r &= detail.density == density;
    #endif
            r &= detail.dryColor == dryColor;
            r &= detail.healthyColor == healthyColor;
            r &= detail.holeEdgePadding == holeEdgePadding;
            r &= detail.maxHeight == maxHeight;
            r &= detail.minHeight == minHeight;
            r &= detail.maxWidth == maxWidth;
            r &= detail.minWidth == minWidth;
            r &= detail.noiseSeed == noiseSeed;
            //r &= detail.useInstancing == useInstancing;
            r &= detail.noiseSpread == noiseSpread;
            r &= detail.prototypeTexture == prototypeTexture;
            r &= detail.renderMode == renderMode;
            
            r &= detail.usePrototypeMesh == usePrototypeMesh;
            return r;
        }
    }
}

