namespace PathNav.Interaction
{
    using UnityEngine;

    public interface IRayCast : ICast
    {
        internal bool Raycast(LayerMask mask)
        {
            bool result = Physics.Raycast(RayOrigin, RayDirection, out RaycastHit hitResult, MaxRaycastDistance, mask);
            HitResult = hitResult;
            return result;
        }

        internal bool Raycast()
        {
            bool result = Physics.Raycast(RayOrigin, RayDirection, out RaycastHit hitResult, MaxRaycastDistance);
            HitResult = hitResult;
            return result;
        }
    }
}
