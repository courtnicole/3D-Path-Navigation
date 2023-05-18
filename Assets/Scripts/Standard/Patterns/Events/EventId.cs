namespace PathNav.Events
{
    using System;
    using PathPlanning;
    using Input;
    using Interaction;
    using PointVisualEventArgs = PathPlanning.PointVisualEventArgs;

    public enum EventId
    {
        #region Enable/Disable Evaluators
        [EventId(typeof(EventArgs))]
        EnableLocomotion,
        [EventId(typeof(EventArgs))]
        DisableLocomotion,
        #endregion

        #region Evaluators Enabled/Disabled
        [EventId(typeof(EventArgs))]
        LocomotionEvaluatorEnabled,
        [EventId(typeof(EventArgs))]
        LocomotionEvaluatorDisabled,
        #endregion

        #region ControllerEventArgs
        [EventId(typeof(ControllerEventArgs))]
        TriggerDown,
        [EventId(typeof(ControllerEventArgs))]
        TriggerUp,

        [EventId(typeof(ControllerEventArgs))]
        SystemClick,

        [EventId(typeof(ControllerEventArgs))]
        ButtonAClick,

        [EventId(typeof(ControllerEventArgs))]
        ButtonBClick,

        [EventId(typeof(ControllerEventArgs))]
        TouchpadTouchStart,
        [EventId(typeof(ControllerEventArgs))]
        TouchpadTouchUpdate,
        [EventId(typeof(ControllerEventArgs))]
        TouchpadTouchEnd,

        [EventId(typeof(ControllerEventArgs))]
        GripClick,

        [EventId(typeof(ControllerEventArgs))]
        JoystickClickStart,
        [EventId(typeof(ControllerEventArgs))]
        JoystickClickEnd,

        [EventId(typeof(ControllerEventArgs))]
        JoystickTouchStart,
        [EventId(typeof(ControllerEventArgs))]
        JoystickPoseUpdate,
        [EventId(typeof(ControllerEventArgs))]
        JoystickTouchEnd,
        #endregion

        #region RaycastResultEventArgs
        [EventId(typeof(RaycastResultEventArgs))]
        RaycastUpdated,
        #endregion

        #region LocomotionEvaluatorEventArgs
        [EventId(typeof(LocomotionEvaluatorEventArgs))]
        LocomotionStarted,
        [EventId(typeof(LocomotionEvaluatorEventArgs))]
        LocomotionUpdated,
        [EventId(typeof(LocomotionEvaluatorEventArgs))]
        LocomotionEnded, 
        #endregion

        #region Placement
        [EventId(typeof(PlacementEventArgs))]
        StartPointPlaced,
        #endregion

        #region ControllerEvaluator
        [EventId(typeof(ControllerEvaluatorEventArgs))]
        SetPathStrategy,

        [EventId(typeof(ControllerEvaluatorEventArgs))]
        BeginPlacingStartPoint,
        [EventId(typeof(ControllerEvaluatorEventArgs))]
        FinishPlacingStartPoint,

        [EventId(typeof(ControllerEvaluatorEventArgs))]
        StartDrawOrErasePath,
        [EventId(typeof(ControllerEvaluatorEventArgs))]
        StopDrawOrErasePath,
        [EventId(typeof(ControllerEvaluatorEventArgs))]
        StartErasingPath,
        [EventId(typeof(ControllerEvaluatorEventArgs))]
        StopErasingPath,
        
        [EventId(typeof(ControllerEvaluatorEventArgs))]
        PathCreationComplete,

        [EventId(typeof(ControllerEvaluatorEventArgs))]
        StartPlaceOrMovePoint,
        [EventId(typeof(ControllerEvaluatorEventArgs))]
        StopPlaceOrMovePoint,
        [EventId(typeof(ControllerEvaluatorEventArgs))]
        PlacePoint,
        [EventId(typeof(ControllerEvaluatorEventArgs))]
        StartMovingPoint,
        [EventId(typeof(ControllerEvaluatorEventArgs))]
        StopMovingPoint,
        #endregion

        #region PathStrategyEventArgs
        [EventId(typeof(PathStrategyEventArgs))]
        EraseStarted,
        [EventId(typeof(PathStrategyEventArgs))]
        EraseEnded, 
        #endregion

        #region NodeEventArgs
        [EventId(typeof(PointVisualEventArgs))]
        NodeCreated,
        [EventId(typeof(PointVisualEventArgs))]
        Node,
        [EventId(typeof(PointVisualEventArgs))]
        NodeConfigured,
        [EventId(typeof(PointVisualEventArgs))]
        NodeEnabled,
        [EventId(typeof(PointVisualEventArgs))]
        NodeDisabled,
        #endregion

        #region SegmentEventArgs
        [EventId(typeof(SegmentEventArgs))]
        SegmentCreated,
        [EventId(typeof(SegmentEventArgs))]
        SegmentConfigured,
        [EventId(typeof(SegmentEventArgs))]
        SegmentEnabled,
        [EventId(typeof(SegmentEventArgs))]
        SegmentDisabled, 
        [EventId(typeof(SegmentEventArgs))]
        SegmentComplete, 

        #endregion

        #region PointVisualEventArgs
        [EventId(typeof(PointVisualEventArgs))]
        PointVisualTriggered,
        [EventId(typeof(PointVisualEventArgs))]
        PointVisualUntriggered,
        #endregion
        
        #region PlacementPlaneArgs
        [EventId(typeof(PlacementPlaneEventArgs))]
        PlacementPlaneTriggered,
        [EventId(typeof(PlacementPlaneEventArgs))]
        PlacementPlaneUntriggered,
        #endregion
        
        #region SceneControlArgs
        [EventId(typeof(SceneControlEventArgs))]
        FollowPathReady,
        #endregion
        
        #region FollowerEvaluatorEventArgs
        [EventId(typeof(FollowerEvaluatorEventArgs))]
        ChangeSpeed,
        [EventId(typeof(FollowerEvaluatorEventArgs))]
        StartSpeedUpdate,
        [EventId(typeof(FollowerEvaluatorEventArgs))]
        EndSpeedUpdate,
        #endregion
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class EventIdAttribute : Attribute
    {
        public EventIdAttribute(Type argType) => ArgType = argType;

        public Type ArgType { get; }
    }
}