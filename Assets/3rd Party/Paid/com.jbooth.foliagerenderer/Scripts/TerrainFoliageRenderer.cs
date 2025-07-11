//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEngine;

namespace JBooth.FoliageRendering
{
    [ExecuteAlways]
    [HelpURL("https://dicewrenchdesigns.com/category/assets/")]
    public class TerrainFoliageRenderer : MonoBehaviour
    {
        static TerrainFoliageRenderer sInstance;
        public static bool hasInstance { get { return sInstance != null; } }
        public static TerrainFoliageRenderer instance
        {
            get
            {
                if (sInstance == null)
                {
                    sInstance = FindObjectOfType<TerrainFoliageRenderer>();
                    if (sInstance == null)
                    {
                        var go = new GameObject("Foliage Renderer");
                        sInstance = go.AddComponent<TerrainFoliageRenderer>();
                    }
                }
                return sInstance;
            }
        }

        public Terrain[] terrains;


        public DrawOptions treeOptions = new DrawOptions() { useWorldYClip = true };
        public DrawOptions detailOptions = new DrawOptions()
        {
            distanceMode = DrawOptions.DistanceMode.UseQualitySettings,
            useWorldYClip = true,
            maxDrawDistance = 80,
            shadowDistance = 80,
            shadowDistanceMode = DrawOptions.DistanceMode.Manual,
        };


        public List<DetailOverrideOptions> detailOverrides = new List<DetailOverrideOptions>();
        public List<TreeOverrideOptions> treeOverrides = new List<TreeOverrideOptions>();

        public enum DetailCachingMode
        {
            AtLoad,
            Streaming
        }

        public DetailCachingMode detailCachingMode = DetailCachingMode.Streaming;

        // distance past max distance in which to start unloading cells. 
        public float detailCachingDistance = 256;

        public MaterialOptions materialOptions = new MaterialOptions();

        public static void EnsureExists()
        {
            if (sInstance == null)
            {
                sInstance = instance;
            }
        }

        [System.Serializable]
        public class TreeOverrideOptions : OverrideOptions
        {
            public TreePrototypeSerializable prototype;
        }

        [System.Serializable]
        public class DetailOverrideOptions : OverrideOptions
        {
            public DetailPrototypeSerializable prototype;
        }

        [System.Serializable]
        public class TerrainExtraTexture
        {
            public Terrain terrain;
            public Texture texture;
        }

        [System.Serializable]
        public class ExtraTextureOption
        {
            [Tooltip("The config which tells it how to set the textures on the shader")]
            public TerrainExtraTextureConfig config;

            public List<TerrainExtraTexture> textureEntries = new List<TerrainExtraTexture>();

            public Texture FindExtraTexture(Terrain t)
            {
                foreach (var te in textureEntries)
                {
                    if (te.terrain == t)
                        return te.texture;
                }
                return null;
            }
        }

        [System.Serializable]
        public class MaterialOptions
        {
            [Tooltip("Passes the height map from the terrain to the shaders on the terrain vegetation")]
            public bool supplyHeightmaps;
            [Tooltip("Passes the normal map from the terrain to the shaders on the terrain vegetation")]
            public bool supplyNormalmaps;

            public enum BaseMapRes
            {
                Off = 0,
                k128 = 128,
                k256 = 256,
                k512 = 512,
                k1024 = 1024
            }
            [Tooltip("Generates an albedo color map for use by shaders on the terrain vegetation")]
            public BaseMapRes albedoMapGeneration = BaseMapRes.Off;
            public List<ExtraTextureOption> extraTextureOptions = new List<ExtraTextureOption>();

            public void SetNeighborProperty(Terrain t, TerrainExtraTextureConfig config, MaterialPropertyBlock mpb, string propertyName)
            {
                if (t == null)
                    return;

                var options = TerrainFoliageRenderer.instance.materialOptions;
                
                foreach (var eto in options.extraTextureOptions)
                {
                    if (eto.config == config && eto.config != null)
                    {
                        foreach (var te in eto.textureEntries)
                        {
                            if (te.terrain == t)
                            {
                                mpb.SetTexture(propertyName, te.texture);
                            }
                        }
                    }
                }
            }
        }

        public void RefreshTerrains()
        {
            terrains = GameObject.FindObjectsByType<Terrain>(FindObjectsSortMode.None);
            foreach (var t in terrains)
            {
                var vp = t.GetComponent<TerrainFoliageProvider>();
                if (vp == null)
                {
                    t.gameObject.AddComponent<TerrainFoliageProvider>();
                }
            }
        }

        public void RefreshAllProviders()
        {
            RefreshTerrains();
            foreach (var t in terrains)
            {
                var vp = t.GetComponent<TerrainFoliageProvider>();
                if (vp != null)
                {
                    vp.Refresh();
                }
            }
        }

        private void OnEnable()
        {
            RefreshTerrains();
            foreach (var t in terrains)
            {
                var vp = t.GetComponent<TerrainFoliageProvider>();
                if (vp != null && vp.enabled == false)
                {
                    vp.enabled = true;
                }
            }
        }

        private void OnDisable()
        {
            RefreshTerrains();
            foreach (var t in terrains)
            {
                var vp = t.GetComponent<TerrainFoliageProvider>();
                if (vp != null && vp.enabled == true)
                {
                    vp.enabled = false;
                }
            }
        }

        public DetailOverrideOptions FindDetailOverrideOption(DetailPrototype prototype)
        {
            if (prototype == null)
                return null;
            foreach (var d in detailOverrides)
            {
                if (d.prototype.IsEqualToDetail(prototype))
                    return d;
            }
            return null;
        }

        public TreeOverrideOptions FindTreeOverrideOption(TreePrototype prototype)
        {
            if (prototype == null)
                return null;
            foreach (var d in treeOverrides)
            {
                if (d.prototype.IsEqualToTree(prototype))
                    return d;
            }
            return null;
        }
      
        public DrawOptions FindDetailDrawOption(DetailPrototype prototype)
        {
            if (detailOverrides == null || prototype == null)
                return detailOptions;
            foreach (var d in detailOverrides)
            {
                if (d.prototype.IsEqualToDetail(prototype))
                {
                    if (d.overrideDrawOptions)
                        return d.drawOptions;
                    else
                        return detailOptions;
                }
            }

            return detailOptions;
        }

        public float FindMaxDetailDrawDistance(Terrain t)
        {
            float max = t.detailObjectDistance;
            if (detailOptions.distanceMode == DrawOptions.DistanceMode.Manual &&
                    detailOptions.maxDrawDistance > max)
                max = detailOptions.maxDrawDistance;

            foreach (DetailOverrideOptions d in detailOverrides)
            {
                if (d.overrideDrawOptions)
                {
                    if (d.drawOptions.distanceMode == DrawOptions.DistanceMode.Manual &&
                            d.drawOptions.maxDrawDistance > max)
                        max = d.drawOptions.maxDrawDistance;
                }
            }
            return max;
        }

        public float FindMaxTreeDrawDistance(Terrain t)
        {
            float max = t.treeDistance;
            if(treeOptions.distanceMode == DrawOptions.DistanceMode.Manual &&
                treeOptions.maxDrawDistance > max)
                max = treeOptions.maxDrawDistance;

            foreach(var tree in treeOverrides)
            {
                if(tree.overrideDrawOptions)
                {
                    if (tree.drawOptions.distanceMode == DrawOptions.DistanceMode.Manual &&
                        tree.drawOptions.maxDrawDistance > max)
                        max = tree.drawOptions.maxDrawDistance;
                }
            }
            return max;
        }

        public DrawOptions FindTreeDrawOption(TreePrototype prototype)
        {
            if (treeOptions == null || prototype == null)
                return treeOptions;
            foreach (var d in treeOverrides)
            {
                if (d.prototype.IsEqualToTree(prototype))
                {
                    if (d.overrideDrawOptions)
                        return d.drawOptions;
                    else
                        return treeOptions;
                }
            }
            return treeOptions;
        }
    }

}