namespace PathNav.PathPlanning
{
    using System;
    using Extensions;
    using UnityEngine;

    public interface IPointVisual : IPathElement
    {
        public void Move(Vector3 position);
    }

    public class PointVisualEventArgs : EventArgs
    {
        public PointVisualEventArgs(IPointVisual pointVisual)
        {
            Id = pointVisual.Id;
            Index = pointVisual.Index;
        }

        public UniqueId Id { get; }
        public int Index { get; }
    }
}
