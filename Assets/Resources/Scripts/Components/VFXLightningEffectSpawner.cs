using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace OccaSoftware.Altos.Runtime
{
    /// <summary>
    /// This class is re-purposed from Unity's sample VFX Output Event Prefab Spawn class.
    /// <br>I made a variety of updates; it is now designed for the Altos lightning system.</br>
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(VisualEffect))]
    [AddComponentMenu("OccaSoftware/Altos/VFX Lightning Effect Spawner")]
    class LightningEffectSpawner : VFXOutputEventAbstractHandler
    {
        public override bool canExecuteInEditor => true;
        public uint instanceCount => AltosLight.MAX_ALTOS_LIGHT_COUNT;
        public GameObject prefabToSpawn => prefab;
        public bool parentInstances => doParentToCurrent;

#pragma warning disable 414, 649

        /// <summary>
        /// Object to spawn
        /// </summary>
        [
            SerializeField,
            Tooltip(
                "The prefab to enable upon event received. Prefabs are created as hidden and stored in a pool, upon enabling this behavior. Upon receiving an event a prefab from the pool is enabled and will be disabled when reaching its lifetime."
            )
        ]
        GameObject prefab;

        /// <summary>
        /// Treat position in local-space.
        /// </summary>
        private bool doParentToCurrent = true;
#pragma warning restore 414, 649

#if UNITY_EDITOR
        bool m_Dirty = true;
#endif

        static readonly GameObject[] k_EmptyGameObjects = new GameObject[0];
        static readonly float[] k_EmptyTimeToLive = new float[0];
        GameObject[] m_Instances = k_EmptyGameObjects;
        float[] m_TimesToLive = k_EmptyTimeToLive;

        void Update()
        {
            if (Application.isPlaying || (executeInEditor && canExecuteInEditor))
            {
                CheckAndRebuildInstances();

                var dt = Time.deltaTime;
                for (int i = 0; i < m_Instances.Length; i++)
                {
#if UNITY_EDITOR
                    //Reassign hide flag, "open prefab" could have resetted this hide flag.
                    UpdateHideFlag(m_Instances[i]);
#endif
                    // Negative infinity for non-time managed
                    if (m_TimesToLive[i] == float.NegativeInfinity)
                        continue;

                    // Else, manage time
                    if (m_TimesToLive[i] <= 0.0f && m_Instances[i].activeSelf)
                        m_Instances[i].SetActive(false);
                    else
                        m_TimesToLive[i] -= dt;
                }
            }
            else
            {
                DisposeInstances();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            foreach (var instance in m_Instances)
                instance.SetActive(false);
        }

        void OnDestroy()
        {
            DisposeInstances();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            m_Dirty = true;
        }
#endif

        void DisposeInstances()
        {
            foreach (var instance in m_Instances)
            {
                if (instance)
                {
                    if (Application.isPlaying)
                        Destroy(instance);
                    else
                        DestroyImmediate(instance);
                }
            }
            m_Instances = k_EmptyGameObjects;
            m_TimesToLive = k_EmptyTimeToLive;
        }

        static readonly int k_PositionID = Shader.PropertyToID("position");
        static readonly int k_LifetimeID = Shader.PropertyToID("lifetime");

        void UpdateHideFlag(GameObject instance)
        {
            instance.hideFlags = HideFlags.HideAndDontSave;
        }

        void CheckAndRebuildInstances()
        {
            bool rebuild = m_Instances.Length != instanceCount;
#if UNITY_EDITOR
            if (m_Dirty)
            {
                rebuild = true;
                m_Dirty = false;
            }
#endif
            if (rebuild)
            {
                DisposeInstances();
                if (prefab != null && instanceCount != 0)
                {
                    m_Instances = new GameObject[instanceCount];
                    m_TimesToLive = new float[instanceCount];
#if UNITY_EDITOR
                    var prefabAssetType = UnityEditor.PrefabUtility.GetPrefabAssetType(prefab);
#endif
                    for (int i = 0; i < m_Instances.Length; i++)
                    {
                        GameObject newInstance = null;
#if UNITY_EDITOR
                        if (prefabAssetType != UnityEditor.PrefabAssetType.NotAPrefab)
                            newInstance = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                        if (newInstance == null)
                            newInstance = Instantiate(prefab);
#else
                        newInstance = Instantiate(prefabToSpawn);
#endif
                        newInstance.name = $"{name} - #{i} - {prefab.name}";
                        newInstance.SetActive(false);
                        newInstance.transform.parent = doParentToCurrent ? transform : null;
                        UpdateHideFlag(newInstance);

                        m_Instances[i] = newInstance;
                        m_TimesToLive[i] = float.NegativeInfinity;
                    }
                }
            }
        }

        public override void OnVFXOutputEvent(VFXEventAttribute eventAttribute)
        {
            CheckAndRebuildInstances();

            int availableInstanceId = -1;
            for (int i = 0; i < m_Instances.Length; i++)
            {
                if (!m_Instances[i].activeSelf)
                {
                    availableInstanceId = i;
                    break;
                }
            }

            if (availableInstanceId != -1)
            {
                var availableInstance = m_Instances[availableInstanceId];
                availableInstance.SetActive(true);
                if (eventAttribute.HasVector3(k_PositionID))
                {
                    if (doParentToCurrent)
                    {
                        availableInstance.transform.localPosition = eventAttribute.GetVector3(k_PositionID);
                    }
                    else
                    {
                        availableInstance.transform.position = eventAttribute.GetVector3(k_PositionID);
                    }
                }

                if (eventAttribute.HasFloat(k_LifetimeID))
                {
                    m_TimesToLive[availableInstanceId] = eventAttribute.GetFloat(k_LifetimeID);
                }
                else
                {
                    m_TimesToLive[availableInstanceId] = float.NegativeInfinity;
                }
            }
        }
    }
}
