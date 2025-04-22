using UnityEngine;

namespace OccaSoftware.Altos.Runtime
{
    internal static class MatrixHandler
    {
        public static Matrix4x4 SetupViewMatrix(Vector3 cameraPosition, Vector3 cameraForward, float zFar, Vector3 cameraUp)
        {
            Matrix4x4 lookMatrix = Matrix4x4.LookAt(cameraPosition, cameraPosition + cameraForward * zFar, cameraUp);
            Matrix4x4 scaleMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
            Matrix4x4 viewMatrix = scaleMatrix * lookMatrix.inverse;
            return viewMatrix;
        }

        public static Matrix4x4 SetupProjectionMatrix(float halfWidth, float zFar)
        {
            float s = halfWidth;
            Matrix4x4 proj = Matrix4x4.Ortho(-s, s, -s, s, 30, zFar);
            return proj;
        }

        public static Matrix4x4 ConvertToWorldToShadowMatrix(Matrix4x4 projectionMatrix, Matrix4x4 viewMatrix)
        {
            var scaleOffset = Matrix4x4.identity;
            scaleOffset.m00 = scaleOffset.m11 = scaleOffset.m22 = 0.5f;
            scaleOffset.m03 = scaleOffset.m13 = scaleOffset.m23 = 0.5f;
            return scaleOffset * (projectionMatrix * viewMatrix);
        }
    }
}
