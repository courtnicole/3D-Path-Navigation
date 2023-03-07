namespace PathNav.Interaction
{
    using System;
    using Events;
    using Extensions;
    using UnityEngine;

    public interface IPathbendable 
    {
        UniqueId Id { get; }

        void PathbendStarted(Transform t)
        {
            //EventManager.Publish(EventId.PathbendStarted, this, GetPathbendEventArgs(t));
        }

        void PathbendUpdated(Transform t)
        {
            //EventManager.Publish(EventId.PathbendUpdated, this, GetPathbendEventArgs(t));
        }

        void PathbendEnded(Transform t)
        {
            //EventManager.Publish(EventId.PathbendEnded, this, GetPathbendEventArgs(t));
        }

        PathbendEventArgs GetPathbendEventArgs(Transform t) => new(t, Id);
    }

    public class PathbendEventArgs : EventArgs
    {
        public PathbendEventArgs(Transform t, UniqueId id)
        {
            Id        = id;
            Transform = t;
        }
        
        public UniqueId Id { get; }
        public Transform Transform { get; }
    }
}
