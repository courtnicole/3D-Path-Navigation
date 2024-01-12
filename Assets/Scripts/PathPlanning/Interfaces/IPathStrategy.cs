namespace PathNav.PathPlanning
{
    using Input;
    using Interaction;
    using UnityEngine;
    using Patterns.Strategy;
    using System;

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
    
    public class PathStrategyEventArgs : EventArgs
    {
        public PathStrategyEventArgs(IPathStrategy strategy) => Strategy = strategy;
        public PathStrategyEventArgs(IController   controller) => Controller = controller;
        public IPathStrategy Strategy   { get; }
        public IController   Controller { get; }
    }
}
