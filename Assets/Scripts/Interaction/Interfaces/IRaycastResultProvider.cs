namespace PathNav.Interaction
{
    using Extensions;
    using Input;
    using System;
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
    
    public class RaycastResultEventArgs : EventArgs
    {
        public RaycastResultEventArgs(IRaycastResultProvider result, UniqueId id)
        {
            Result = result;
            Id     = id;
        }

        public IRaycastResultProvider Result { get; }
        public UniqueId Id { get; set; }
    }
}
