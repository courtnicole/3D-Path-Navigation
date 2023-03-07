namespace PathNav.Extensions
{
    using PathPlanning;
    using UnityEngine;
    using UnityEngine.ResourceManagement.ResourceProviders;

    public static class Utility 
    {
        public static bool GetCameraTransform(out Transform cameraTransform)
        {
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
                return true;
            }
            if (GameObject.FindGameObjectWithTag("MainCamera") != null)
            {
                cameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
                return true;
            }
            cameraTransform = null;
            return false;
        }

        public static bool GetCameraRigTransform(out Transform cameraTransform)
        {
            if (GameObject.FindGameObjectWithTag("CameraRig") != null)
            {
                cameraTransform = GameObject.FindGameObjectWithTag("CameraRig").transform;
                return true;
            }
            cameraTransform = null;
            return false;
        }

        public static float Mod(float a, float b) => a - b * Mathf.Floor(a / b);

        public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n) => Mathf.Atan2(Vector3.Dot(n, Vector3.Cross(v1, v2)), Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;

        public static float NormalizeAngle(this float angle)
        {
            angle %= 360f;
            if (angle < 0)
            {
                angle += 360.0f;
            }
            return angle;
        }

        public static float AxisAngle(this Vector2 axis)
        {
            float angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg;
            return 90 - angle;
        }

        public static InstantiationParameters Parameterize(Vector3 position, Transform parent) => new(position, Quaternion.identity, parent);

        public static InstantiationParameters Parameterize(Vector3 position, Quaternion rotation, Transform parent) => new(position, rotation, parent);

        public static InstantiationParameters Parameterize(Vector3 position) => new(position, Quaternion.identity, null);

        public static InstantiationParameters Parameterize(Transform parent, bool useWorldSpace = true) => new(parent, useWorldSpace);
    }
}
