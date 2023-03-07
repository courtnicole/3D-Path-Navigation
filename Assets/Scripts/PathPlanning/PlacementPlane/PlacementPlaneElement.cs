namespace PathNav.PathPlanning
{
    using Events;
    using UnityEngine;
    using Interaction;

    public class PlacementPlaneElement : MonoBehaviour, IPlacementPlane
    {
        private bool _hasValidCollision;

        #region Implementation of IPlacementPlane
        public bool HasCollidingController => _hasValidCollision;

        public void OnTriggerEntered()
        {
            _hasValidCollision = true;
            EventManager.Publish(EventId.PlacementPlaneTriggered, this, GetPlacementPlaneEventArgs());
        }

        public void OnTriggerExited()
        {
            _hasValidCollision = false;
            EventManager.Publish(EventId.PlacementPlaneUntriggered, this, GetPlacementPlaneEventArgs());
        }
        #endregion
        
        private PlacementPlaneEventArgs GetPlacementPlaneEventArgs() => new(this);
    }
}