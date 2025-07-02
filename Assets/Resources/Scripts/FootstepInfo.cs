using System.Linq;
using UnityEngine;



namespace Viva
{


    public class FootstepInfo : MonoBehaviour
    {

        public enum FootStepType
        {
            DIRT,   //default
            WOOD,
            TILE,
            WATER,
            CARPET,
            SAND,
            STONE,
            STONE_WET
        }

        [SerializeField]
        private SoundSet[] m_sounds = new SoundSet[System.Enum.GetValues(typeof(FootstepInfo.FootStepType)).Length];
        public SoundSet[] sounds { get { return m_sounds; } }

        [HideInInspector]
        public Vector3 lastFloorPos = Vector2.zero;
        private FootStepType mCurrentFootStepType = FootStepType.DIRT;
        public FootStepType CurrentFootStepType { get { return mCurrentFootStepType; } }
        private int[] footstepRegionCount = new int[System.Enum.GetValues(typeof(FootStepType)).Length];


        public void SetFootStepTypeBasedOnCollider(Collider collider)
        {
            Debug.Log($"FootStep Mat");
            if(collider != null)
                if (collider.material != null)
                {
                    var groundMatName = collider.material.name;
                    groundMatName = groundMatName.Replace(" (Instance)","");
                    
                    if (System.Enum.TryParse(groundMatName.ToUpper(), out FootStepType f))
                    {
                        mCurrentFootStepType = f;
                        return;
                    }
                }
            
            mCurrentFootStepType = FootStepType.DIRT; //default to this if nothing is found
        }
        
        public void SetFootStepTypeBasedOnTerrain(Terrain terrain, Vector3 position)
        {
            TerrainData terrainData = terrain.terrainData;
            Vector3 terrainPosition = terrain.transform.position;

            int mapX = Mathf.RoundToInt(((position.x - terrainPosition.x) / terrainData.size.x) * terrainData.alphamapWidth);
            int mapZ = Mathf.RoundToInt(((position.z - terrainPosition.z) / terrainData.size.z) * terrainData.alphamapHeight);

            float[] weights = GetTextureMix(terrainData, mapX, mapZ);
            int textureIndex = GetMainTextureIndex(weights);

            switch (textureIndex)
            {
                case 0:
                    mCurrentFootStepType = FootStepType.DIRT;
                    break;
                case 1:
                    mCurrentFootStepType = FootStepType.STONE;
                    break;
                case 2:
                    mCurrentFootStepType = FootStepType.DIRT;
                    break;
                case 3:
                    mCurrentFootStepType = FootStepType.DIRT;
                    break;
                case 4:
                    mCurrentFootStepType = FootStepType.SAND;
                    break;

                default:
                    // Do nothing
                    break;
            }
        }

        private float[] GetTextureMix(TerrainData terrainData, int mapX, int mapZ)
        {
            float[,,] splatMapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);
            float[] weights = new float[splatMapData.GetUpperBound(2) + 1];

            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = splatMapData[0, 0, i];
            }

            return weights;
        }

        private int GetMainTextureIndex(float[] mix)
        {
            int maxIndex = 0;
            float maxMix = 0;

            for (int i = 0; i < mix.Length; i++)
            {
                if (mix[i] > maxMix)
                {
                    maxIndex = i;
                    maxMix = mix[i];
                }
            }

            return maxIndex;
        }

        public void AddtoFootstepRegion(FootStepType footstep)
        {
            footstepRegionCount[(int)footstep]++;
            UpdateCurrentFootstepType();
        }

        public void RemoveFromFootstepRegion(FootStepType footstep)
        {
            footstepRegionCount[(int)footstep]--;
            if (footstepRegionCount[(int)footstep] < 0)
            {
                footstepRegionCount[(int)footstep] = 0;
            }
            UpdateCurrentFootstepType();
        }

        private void UpdateCurrentFootstepType()
        {
            for (int i = 0; i < footstepRegionCount.Length; i++)
            {
                if (footstepRegionCount[i] > 0)
                {
                    mCurrentFootStepType = (FootStepType)i;
                    return;
                }
            }
            mCurrentFootStepType = FootStepType.DIRT; // Default footstep type
        }

        public bool IsAnyFootstepRegionActive()
        {
            return footstepRegionCount.Any(count => count > 0);
        }

        public void SetFootStepTypeFromRegions()
        {
            for (int i = 0; i < footstepRegionCount.Length; i++)
            {
                if (footstepRegionCount[i] > 0)
                {
                    mCurrentFootStepType = (FootStepType)i;
                    return;
                }
            }
            mCurrentFootStepType = FootStepType.DIRT; // Default to this if nothing is found
        }
    }

}