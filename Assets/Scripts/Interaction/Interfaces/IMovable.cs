namespace PathNav.Interaction
{
    using System;
    using Events;
    using Extensions;
    using UnityEngine;

    public interface IMovable
    {
        UniqueId Id { get; }
        void MoveStarted(Transform t)
        {
            EventManager.Publish(EventId.MoveStarted, this, GetMoveEventArgs(t));
        }

        void MoveUpdated(Transform t)
        {
            EventManager.Publish(EventId.MoveUpdated, this, GetMoveEventArgs(t));
        }

        void MoveEnded(Transform t)
        {
            EventManager.Publish(EventId.MoveEnded, this, GetMoveEventArgs(t));
        }

        MoveEventArgs GetMoveEventArgs(Transform t) => new(t, Id);
    }

    public class MoveEventArgs : EventArgs
    {
        public MoveEventArgs(Transform t, UniqueId id)
        {
            Id    = id;
            Position = t.position;
        }

        public Vector3 Position { get; }
        public UniqueId Id { get; }
    }
}