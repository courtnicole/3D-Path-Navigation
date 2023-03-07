namespace PathNav.Interaction
{
    using System;
    using Events;
    using Extensions;
    using UnityEngine;

    public interface IBulldozable 
    {
        UniqueId Id { get; }

        void DrawStarted(Transform t)
        {
            EventManager.Publish(EventId.DrawStarted, this, GetBulldozeEventArgs(t));
        }

        void DrawUpdated(Transform t)
        {
            EventManager.Publish(EventId.DrawUpdated, this, GetBulldozeEventArgs(t));
        }

        void DrawEnded(Transform t)
        {
            EventManager.Publish(EventId.DrawEnded, this, GetBulldozeEventArgs(t));
        }

        void EraseStarted(Transform t)
        {
            EventManager.Publish(EventId.DrawStarted, this, GetBulldozeEventArgs(t));
        }

        void EraseUpdated(Transform t)
        {
            EventManager.Publish(EventId.DrawUpdated, this, GetBulldozeEventArgs(t));
        }

        void EraseEnded(Transform t)
        {
            EventManager.Publish(EventId.DrawEnded, this, GetBulldozeEventArgs(t));
        }

        BulldozeEventArgs GetBulldozeEventArgs(Transform t) => new(t, Id);
    }

    public class BulldozeEventArgs : EventArgs
    {
        public BulldozeEventArgs(Transform t, UniqueId id)
        {
            Id    = id;
            Transform = t;
        }
        
        public UniqueId Id { get; }
        public Transform Transform { get; }
    }
}
