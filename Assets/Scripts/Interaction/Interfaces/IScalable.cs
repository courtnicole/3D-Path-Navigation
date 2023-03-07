namespace PathNav.Interaction
{
    using System;
    using Events;
    using Extensions;
    using UnityEngine;

    public interface IScalable
    {
        UniqueId Id { get; }

        void ScaleStarted(Transform t)
        {
            //EventManager.Publish(EventId.ScaleStarted, this, GetScaleEventArgs(t));
        }

        void ScaleUpdated(Transform t)
        {
            //EventManager.Publish(EventId.ScaleUpdated, this, GetScaleEventArgs(t));
        }

        void ScaleEnded(Transform t)
        {
            //EventManager.Publish(EventId.ScaleEnded, this, GetScaleEventArgs(t));
        }

        ScaleEventArgs GetScaleEventArgs(Transform t) => new(t, Id);
    }

    public class ScaleEventArgs : EventArgs
    {
        public ScaleEventArgs(Transform t, UniqueId id)
        {
            Id    = id;
            Scale = t.localScale;
        }

        public Vector3 Scale { get; }
        public UniqueId Id { get; }
    }
}