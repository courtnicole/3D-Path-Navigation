namespace PathNav.Interaction
{
    using Events;
    using System;
    using UnityEngine;

    public interface IPlaceable
    {
        void Place(EventId id, Transform t)
        {
            EventManager.Publish(id, this, GetPlacementEventArgs(t));
        }

        PlacementEventArgs GetPlacementEventArgs(Transform t) => new(t);
    }

    public class PlacementEventArgs : EventArgs
    {
        public PlacementEventArgs(Transform t)
        {
            Position = t.position;
            Heading  = t.forward;
            Rotation = t.rotation;
        }
        
        public Vector3 Position { get; }
        public Vector3 Heading { get; }
        public Quaternion Rotation { get; }
    }
}