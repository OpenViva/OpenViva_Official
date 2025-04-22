using System;
using System.Collections.Generic;
using UnityEngine;
using Viva.Util;


namespace Viva
{


    public class ManekiNeko : MonoBehaviour
    {

        [SerializeField]
        private MeshRenderer meshRenderer;
        [SerializeField]
        private Rigidbody paw;
        [SerializeField]
        private AudioClip tickSound;
        [SerializeField]
        private AudioClip tockSound;
        public float clockSpeed = 1.0f;
        [SerializeField]
        private Texture2D eyeEmissionTex;

        private static int pupilRightID = Shader.PropertyToID("_PupilRight");
        private static int pupilUpID = Shader.PropertyToID("_PupilUp");
        private static int pupilShrinkID = Shader.PropertyToID("_PupilShrink");
        public bool lookMode = false;

        private float tickTock = 0.0f;
        private int tickTockSecond = 0;
        private Material eyeMaterial
        {
            get
            {
                if (meshRenderer.materials.Length >= 2)
                {
                    return meshRenderer.materials[1];
                }
                else
                {
                    return null;
                }
            }
        }


        public void SetLookMode(bool on)
        {
            lookMode = on;
            enabled = !on;

            var targetMaterial = eyeMaterial;
            if (lookMode)
            {
                targetMaterial.SetTexture("_Emission", null);
            }
            else
            {
                targetMaterial.SetTexture("_Emission", eyeEmissionTex);
            }
        }

        private void Update()
        {
            UpdateLookAt();
        }

        private void FixedUpdate()
        {
            if (lookMode)
            {
                return;
            }

            tickTock += Time.deltaTime * clockSpeed;
            int newTickTockSecond = (int)tickTock;
            if (tickTockSecond != newTickTockSecond)
            {
                tickTockSecond = newTickTockSecond;

                var handle = SoundManager.main.RequestHandle(transform.localPosition, transform);
                handle.PlayOneShot(tickTockSecond % 1 == 0 ? tickSound : tockSound);
                handle.maxDistance = 12.0f;
                handle.volume = 0.4f;
                TwitchHand();
            }
            if (meshRenderer.materials.Length >= 2)
            {
                var targetMaterial = meshRenderer.materials[1];
                float lerp = Mathf.Abs(0.5f - tickTock * 0.5f % 1.0f) * 2.0f;
                float eyes = Mathf.LerpUnclamped(-0.6f, 0.6f, Tools.EaseInOutQuad(lerp));
                targetMaterial.SetFloat(pupilRightID, eyes);
                targetMaterial.SetFloat(pupilUpID, 0.0f);
            }
        }

        public void UpdateLookAt()
        {
            if (GameDirector.instance == null) return;
            
            if (!lookMode)
            {
                return;
            }
            //float leastSqDist = Mathf.Infinity;
            // for (int i = visibleItems.Count; i-- > 0;)
            // {
            //
            //     var item = visibleItems[i];
            //     if (item == null)
            //     {
            //         //visibleItems.RemoveAt(i);
            //         continue;
            //     }
            //     Vector3 localPos = transform.InverseTransformPoint(item.transform.position);
            //     if (localPos.z < 0.0f)
            //     {
            //         continue;
            //     }
            //     var sqDist = Vector3.Dot(localPos, localPos);
            //     if (sqDist < leastSqDist)
            //     {
            //         leastSqDist = sqDist;
            //         target = item;
            //     }
            // }
            
            // float scalingFactor = 0.4f / 180.0f;
            // Debug.Log(Tools.Bearing(transform, target.transform.position) * scalingFactor);
            // float right = Mathf.Clamp(Tools.Bearing(transform, target.transform.position) * scalingFactor, -0.4f, 0.4f);
            // Vector3 local = transform.InverseTransformPoint(target.transform.position);
            // float up = Mathf.Clamp(local.y / local.z, -0.7f, 0.7f) * 0.5f;
            //
            // var targetMaterial = eyeMaterial;
            // targetMaterial.SetFloat(pupilRightID, right);
            // targetMaterial.SetFloat(pupilUpID, -up);
            
            float FOV = 90f;
            
            Vector3 directionToPlayer = (GameDirector.player.head.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if (angleToPlayer <= FOV / 2) {
                float bearing = Tools.Bearing(transform, GameDirector.player.head.transform.position);

                float scalingFactor = 0.4f / (FOV / 2);
                float right = Mathf.Clamp(bearing * scalingFactor, -0.4f, 0.4f);
                Vector3 local = transform.InverseTransformPoint(GameDirector.player.head.transform.position);
                float up = Mathf.Clamp(local.y / local.z, -0.2f, 0.08f) * 0.5f;
                
                eyeMaterial.SetFloat(pupilRightID, right);
                eyeMaterial.SetFloat(pupilUpID, -up);
                eyeMaterial.SetFloat(pupilShrinkID, 1.3f);
            }

            // if (Random.value > 0.5f)
            // {
            //     targetMaterial.SetFloat(pupilShrinkID, 1.0f);
            // }
            // else
            // {
            //     targetMaterial.SetFloat(pupilShrinkID, 3.0f);
            // }
            //TwitchHand();
        }

        private void TwitchHand()
        {
            paw.AddRelativeTorque(Vector3.forward * 1000.0f, ForceMode.VelocityChange);
        }
    }

}