namespace PathNav.Interaction
{
    using Extensions;
    using Input;
    using UnityEngine;

    public interface IRaycastResultProvider
    {
        public UniqueId Id { get; }
        public ISurfaceHit SurfaceHit { get; }
        public IController Controller { get; }
        
        public Vector3 RayOrigin { get; }
        public Vector3 RayDirection { get; }
        public Vector3 RayHitPoint { get; }
    }
}
