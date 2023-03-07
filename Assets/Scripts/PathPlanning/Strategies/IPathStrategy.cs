namespace PathNav.PathPlanning
{
    using UnityEngine;
    using Patterns.Strategy;

    public interface IPathStrategy : IStrategy
    { 
        ISegment ActiveSegment { get; set; } 
        Vector3 StartPosition { get; set; }
        Vector3 StartHeading { get; set; }

        void SetStartPosition(Vector3 startPose, Vector3 startHeading)
        {
            StartPosition = startPose;
            StartHeading  = startHeading;
        }
        void SetActiveSegment(ISegment activeSegment)
        {
            ActiveSegment = activeSegment;
        }

        void AddFirstPoint()
        {
            ActiveSegment.AddFirstPoint(StartPosition, StartHeading);
        }

        void Enable();
        void Disable();

        void SubscribeToEvents();
        void UnsubscribeToEvents();
    }
}
