
using UnityEngine;

namespace JBooth.MicroSplat
{
   // Unity has some bug where the normal map you get from the terrain changes
   // instead of updates (likely it gets new'd instead of being reused), and
   // then suddenly it's not valid data anymore, and blending breaks.
   // I tried a number of ways to do this once, but all were unreliable in
   // some case, so now we just copy it every frame. Yuck. But beats an
   // extra copy of the normal map for every terrain in the scene. 
   [ExecuteInEditMode]
   public class MicroSplatUseInstanceNormal : MonoBehaviour
   {
      MicroSplatTerrain mst = null;
      void LateUpdate()
      {
         if (mst == null)
         {
            mst = GetComponent<MicroSplatTerrain>();
         }

         if (mst != null && mst.blendMatInstance != null)
         {
            mst.blendMatInstance.SetTexture("_PerPixelNormal", mst.terrain.normalmapTexture);

         }
      }
   }

}