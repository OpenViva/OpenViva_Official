using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Viva.Util;

namespace Viva
{

    public enum SpeechBubble
    {
        EXCLAMATION,
        INTERROGATION,
        FULL
    }

    public partial class GameDirector : MonoBehaviour
    {


        public static Player player { get; private set; }
        private static Set<Character> m_characters = new Set<Character>();
        public static Set<Character> characters { get { return m_characters; } }

        [SerializeField]
        private Player m_player;
        [SerializeField]
        private GameObject companionPrefab;
        [SerializeField]
        private Transform companionPool;
        [SerializeField]
        public Transform companionRespawnPoint;
        [SerializeField]
        private PoseCache m_companionBasePose;
        public PoseCache companionBasePose { get { return m_companionBasePose; } }
        [FormerlySerializedAs("m_allLoliSettings")] [SerializeField]
        private CompanionSettings m_AllCompanionSettings;
        public CompanionSettings companionSettings { get { return m_AllCompanionSettings; } }
        [SerializeField]
        private Texture2D[] speechBubbleTextures = new Texture2D[System.Enum.GetValues(typeof(SpeechBubble)).Length];
        public Material vivaModelMaterial;
        [SerializeField]
        private PhysicMaterial m_stickyPhysicsMaterial;
        public PhysicMaterial stickyPhysicsMaterial { get { return m_stickyPhysicsMaterial; } }
        [SerializeField]
        private PhysicMaterial m_slipperyPhysicsMaterial;
        public PhysicMaterial slipperyPhysicsMaterial { get { return m_slipperyPhysicsMaterial; } }


        public Texture2D GetSpeechBubbleTexture(SpeechBubble bubble)
        {
            return speechBubbleTextures[(int)bubble];
        }


        public Companion GetCompanionFromPool()
        {
            if (companionPool.childCount == 0)
            {
                var container = GameObject.Instantiate(companionPrefab, Vector3.zero, Quaternion.identity);
                var newLoli = container.GetComponent<Companion>();
                return newLoli;
            }
            var result = companionPool.GetChild(0);
            result.transform.SetParent(null, true);
            return result.GetComponent<Companion>(); ;
        }


        public T FindClosestCharacter<T>(Vector3 point, float radius)
        {
            radius *= radius;
            T result = default(T);
            for (int i = 0; i < m_characters.objects.Count; i++)
            {
                Character candidate = m_characters.objects[i];
                float sqDist = Vector3.SqrMagnitude(candidate.head.position - point);
                if (sqDist < radius && candidate.GetType() == typeof(T))
                {
                    try
                    {
                        result = (T)System.Convert.ChangeType(candidate, typeof(T));
                        radius = sqDist;
                    }
                    catch
                    {
                        Debug.LogError("ERROR Object could not be cast!");
                    }
                }
            }
            return result;
        }


        public Player FindNearbyPlayer(Vector3 position, float distance)
        {

            Character nearby = GameDirector.instance.FindClosestCharacter<Player>(
                position,
                distance
            );
            return nearby as Player;
        }

        public Companion FindNearbyLoli(Vector3 position, float distance)
        {

            Character nearby = GameDirector.instance.FindClosestCharacter<Companion>(
                position,
                distance
            );
            return nearby as Companion;
        }

        public Companion FindClosestGeneralDirectionLoli(Transform source, float maxBearing = 45.0f)
        {
            float least = Mathf.Infinity;
            Companion result = null;
            for (int i = 0; i < characters.objects.Count; i++)
            {
                Companion companion = characters.objects[i] as Companion;
                if (companion == null)
                {
                    continue;
                }
                if (!companion.CanSeePoint(source.position))
                {
                    continue;
                }
                Vector3 headPos = companion.head.position;
                float bearing = Tools.Bearing(source, headPos);
                if (bearing > maxBearing)
                {
                    continue;
                }
                float sqDist = Vector3.SqrMagnitude(headPos - source.position);
                if (sqDist < least)
                {
                    least = sqDist;
                    result = companion;
                }
            }
            return result;
        }

        public List<Companion> FindGeneralDirectionLolis(Transform source, float maxBearing = 45.0f, float maxDist = 15.0f)
        {
            maxDist *= maxDist;
            List<Companion> result = new List<Companion>();
            for (int i = 0; i < characters.objects.Count; i++)
            {
                Companion companion = characters.objects[i] as Companion;
                if (companion == null)
                {
                    continue;
                }
                if (!companion.CanSeePoint(source.position))
                {
                    continue;
                }
                Vector3 headPos = companion.head.position;
                float bearing = Tools.Bearing(source, headPos);
                if (bearing > maxBearing)
                {
                    continue;
                }
                float sqDist = Vector3.SqrMagnitude(headPos - source.position);
                if (sqDist < maxDist)
                {
                    result.Add(companion);
                }
            }
            return result;
        }

        public List<Character> FindCharactersInSphere(int typeMask, Vector3 point, float radius)
        {
            radius *= radius;
            List<Character> candidates = new List<Character>();
            for (int i = 0; i < m_characters.objects.Count; i++)
            {
                Character candidate = m_characters.objects[i];
                float sqDist = Vector3.SqrMagnitude(candidate.head.position - point);
                if (sqDist < radius && ((int)candidate.characterType & typeMask) != 0)
                {
                    candidates.Add(candidate);
                }
            }
            return candidates;
        }

        public Item FindPickupItemForCharacter(Character source, Vector3 position, float radius, Item.Type preference = Item.Type.NONE)
        {

            Collider[] objects = Physics.OverlapSphere(position, radius, WorldUtil.visionMask, QueryTriggerInteraction.Collide);
            Item result = null;
            float leastSqDist = Mathf.Infinity;
            for (int i = 0; i < objects.Length; i++)
            {

                Collider collider = objects[i];
                // Item item = Tools.SearchTransformFamily<Item>( collider.transform );
                Item item = collider.transform.GetComponent<Item>();
                if (item == null)
                {
                    continue;
                }
                //ignore self body items
                if (!item.settings.allowChangeOwner && item.mainOwner == source)
                {
                    continue;
                }
                if (item.hasAnyAttributes((int)Item.Attributes.DISABLE_PICKUP))
                {
                    continue;
                }
                if (item.settings.itemType == preference)
                {
                    return item;
                }
                float sqDist = Vector3.SqrMagnitude(position - collider.bounds.center);
                if (sqDist < leastSqDist)
                {
                    result = item;
                    leastSqDist = sqDist;
                }
            }
            return result;
        }
    }

}