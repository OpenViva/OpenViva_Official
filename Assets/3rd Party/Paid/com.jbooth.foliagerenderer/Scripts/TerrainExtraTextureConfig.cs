//////////////////////////////////////////////////////
// Foliage Renderer
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBooth.FoliageRendering
{
    [CreateAssetMenu(fileName = "ExtraTextureConfig", menuName = "FoliageRenderer/Terrain Extra Texture Config")]
    public class TerrainExtraTextureConfig : ScriptableObject
    {
        [Tooltip("This float property will be set as 1 when this feature is used")]
        public string activeProperty = "_TerrainAlbedoProvided";
        [Tooltip("Texture Propery name for current terrain")]
        public string propertyC = "_TerrainAlbedoC";
        [Tooltip("Texture Propery name for terrain left of current")]
        public string propertyL = "_TerrainAlbedoL";
        [Tooltip("Texture Propery name for terrain right of current")]
        public string propertyR = "_TerrainAlbedoR";
        [Tooltip("Texture Propery name for terrain up of current")]
        public string propertyU = "_TerrainAlbedoU";
        [Tooltip("Texture Propery name for terrain up and then left of current")]
        public string propertyUL = "_TerrainAlbedoUL";
        [Tooltip("Texture Propery name for terrain up and then right of current")]
        public string propertyUR = "_TerrainAlbedoUR";
        [Tooltip("Texture Propery name for terrain below current")]
        public string propertyB = "_TerrainAlbedoB";
        [Tooltip("Texture Propery name for terrain below and then left of current")]
        public string propertyBL = "_TerrainAlbedoBL";
        [Tooltip("Texture Propery name for terrain below and then right of current")]
        public string propertyBR = "_TerrainAlbedoBR";

    }
}
