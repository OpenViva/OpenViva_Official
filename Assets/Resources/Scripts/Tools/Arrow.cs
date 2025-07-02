using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Viva.Util
{
    public class Arrow : MonoBehaviour
    {
        public float arrowHeadLength = 0.25f;
        public float arrowHeadAngle = 20.0f;
        public float arrowLineLength = 1.0f;

        void OnDrawGizmos()
        {
            Vector3 direction = transform.forward * arrowLineLength;
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(transform.position + direction, right * arrowHeadLength);
            Gizmos.DrawRay(transform.position + direction, left * arrowHeadLength);
        }
    }
}