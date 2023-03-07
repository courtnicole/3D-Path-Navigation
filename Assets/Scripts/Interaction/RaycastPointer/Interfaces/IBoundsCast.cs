namespace PathNav.Interaction
{
    using UnityEngine;

    public interface IBoundsCast : ICast
    {
        Bounds BoundingVolume { get; }

        internal bool BoundingVolumeHit() => BoundingVolume.IntersectRay(new Ray(RayOrigin, RayDirection));

        internal bool IsInBoundingVolume(Vector3 point) => BoundingVolume.Contains(point);
    }
}
