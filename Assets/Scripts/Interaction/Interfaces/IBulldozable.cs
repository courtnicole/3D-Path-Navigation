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

        BulldozeEventArgs GetBulldozeEventArgs(Transform t) => new();
    }

    public class BulldozeEventArgs : EventArgs
    {
        public BulldozeEventArgs()
        {
        }
    }
}
