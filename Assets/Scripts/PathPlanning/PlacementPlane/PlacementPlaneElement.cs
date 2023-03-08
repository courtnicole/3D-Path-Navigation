namespace PathNav.PathPlanning
{
    using Events;
    using UnityEngine;

    public class PlacementPlaneElement : MonoBehaviour, IPlacementPlane
    {
        #region Implementation of IPlacementPlane
        public bool HasCollidingController { get; private set; }

        public void OnTriggerEntered()
        {
            HasCollidingController = true;
            EventManager.Publish(EventId.PlacementPlaneTriggered, this, GetPlacementPlaneEventArgs());
        }

        public void OnTriggerExited()
        {
            HasCollidingController = false;
            EventManager.Publish(EventId.PlacementPlaneUntriggered, this, GetPlacementPlaneEventArgs());
        }
        #endregion
        
        private PlacementPlaneEventArgs GetPlacementPlaneEventArgs() => new(this);
    }
}