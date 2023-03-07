namespace PathNav.Interaction
{
    using UnityEngine;

    public interface ISphereCast : ICast
    {
        float Radius { get; }

        internal bool SphereCast(LayerMask mask)
        {
            bool result =  Physics.SphereCast(RayOrigin, Radius, RayDirection, out RaycastHit hitResult, MaxRaycastDistance, mask);
            HitResult = hitResult;
            return result;
        }
    }
}
