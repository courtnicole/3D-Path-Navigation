namespace PathNav.Interaction
{
    using UnityEngine;

    public interface ICast
    {
        Vector3 RayOrigin { get; }
        Vector3 RayDirection { get; }
        float MaxRaycastDistance { get; }
        RaycastHit HitResult { get; set; }
        
        Vector3 RayHitPoint { get; set; }
    }
}
