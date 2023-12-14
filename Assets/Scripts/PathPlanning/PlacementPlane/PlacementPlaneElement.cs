namespace PathNav.PathPlanning
{
    using Events;
    using UnityEngine;

    public class PlacementPlaneElement : MonoBehaviour, IPlacementPlane
    {
        #region Implementation of IPlacementPlane
        public bool HasCollidingController { get; private set; }
        public bool IsActive               { get; private set;}

        public void Enable()
        {
            IsActive = true;
            gameObject.SetActive(true);
        }
        
        public void Disable()
        {
            IsActive = false;
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            IsActive = false;
        }

        public void OnTriggerEntered()
        {
            if(!IsActive) return;
            HasCollidingController = true;
            EventManager.Publish(EventId.PlacementPlaneTriggered, this, GetPlacementPlaneEventArgs());
        }

        public void OnTriggerExited()
        {
            if(!IsActive) return;
            HasCollidingController = false;
            EventManager.Publish(EventId.PlacementPlaneUntriggered, this, GetPlacementPlaneEventArgs());
        }
        #endregion
        
        private PlacementPlaneEventArgs GetPlacementPlaneEventArgs() => new(this);
    }
}