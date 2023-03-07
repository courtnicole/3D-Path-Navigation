namespace PathNav.Interaction
{
    using Events;
    using System;
    using UnityEngine;

    public interface ICollidable
    {
        public Collider ColliderElement { get; }

        public void TriggerEntered(EventId id, Collider c)
        {
            EventManager.Publish(id, this, GetCollisionEventArgs(c));
        }

        CollisionEventArgs GetCollisionEventArgs(Collider c) => new(c);
    }

    public class CollisionEventArgs : EventArgs
    {
        public CollisionEventArgs(Collider c) => ObjectHit = c;

        public Collider ObjectHit { get; }
    }
}