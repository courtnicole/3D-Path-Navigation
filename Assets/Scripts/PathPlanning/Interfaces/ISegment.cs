namespace PathNav.PathPlanning
{
    using System;
    using UnityEngine;
    using Dreamteck.Splines;
    using Input;
    using Interaction;

    public interface ISegment : IPathElement
    {
        SplineComputer Spline { get; }
        int CurrentPointCount { get; }
        
        int SelectedPointVisualIndex { get; }
        int SelectedSegmentIndex { get; }

        void AddFirstPoint(Vector3 newPosition, Vector3 heading);
        void AddPoint(Vector3 position);
        void MovePoint(int pointIndex, Vector3 newPosition);
        void RemovePoint();
        void RemovePoint(int pointIndex);
        void ConfigureNodeVisuals(PathStrategy strategy);
        bool IsCloseToPoint(out int pointIndex);
        public bool CanErasePoint(ref IController controller);
        void SaveSpline();
    }

    public class SegmentEventArgs : EventArgs
    {
        public SegmentEventArgs(ISegment segment) => Segment = segment;

        public ISegment Segment { get; }
    }
}
