namespace PathNav.PathPlanning
{
    using System;

    public interface IPlacementPlane
    {
        public void Enable();
        public void Disable();
        public void OnTriggerEntered();
        public void OnTriggerExited();
        public bool HasCollidingController { get; }
        
        public bool IsActive { get; }
    }
    
    public class PlacementPlaneEventArgs : EventArgs
    {
        public PlacementPlaneEventArgs(IPlacementPlane placementPlane) => PlacementPlane = placementPlane;
        
        public IPlacementPlane PlacementPlane { get; }
    }
}
