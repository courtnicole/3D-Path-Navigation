using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathNav.PathPlanning
{
    using Interaction;
    using System;

    public interface IPlacementPlane
    {
        public void OnTriggerEntered();
        public void OnTriggerExited();
        public bool HasCollidingController { get; }
    }
    
    public class PlacementPlaneEventArgs : EventArgs
    {
        public PlacementPlaneEventArgs(IPlacementPlane placementPlane) => PlacementPlane = placementPlane;
        
        public IPlacementPlane PlacementPlane { get; }
    }
}
