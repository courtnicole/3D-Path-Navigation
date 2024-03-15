namespace PathNav.Extensions
{
    using UnityEngine;

    public static class Vector3Extensions
    {
        public static Vector3 FlattenY(this Vector3 v) => new Vector3(v.x, 0, v.z);
    }
}
