namespace PathNav.Events
{
    using System;
    using PathPlanning;
    using Input;
    using Interaction;
    using PointVisualEventArgs = PathPlanning.PointVisualEventArgs;

    public enum EventId
    {
        #region Button Events
        [EventId(typeof(MenuClickEventArgs))]
        ButtonScaleClicked,
        [EventId(typeof(MenuClickEventArgs))]
        MoveNodeSelected,
        [EventId(typeof(MenuClickEventArgs))]
        DeleteNodeSelected,

        [EventId(typeof(MenuClickEventArgs))]
        ButtonPathbendClicked, 
        [EventId(typeof(MenuClickEventArgs))]
        ButtonBulldozeClicked, 
        [EventId(typeof(MenuClickEventArgs))]
        ButtonInsertNodeClicked, 
        #endregion

        #region Enable/Disable Evaluators
        [EventId(typeof(EventArgs))]
        EnableLocomotion,
        [EventId(typeof(EventArgs))]
        DisableLocomotion,
        [EventId(typeof(EventArgs))]
        EnableRaycastPointer,
        [EventId(typeof(EventArgs))]
        DisableRaycastPointer,
        [EventId(typeof(EventArgs))]
        EnableRadialMenu,
        [EventId(typeof(EventArgs))]
        DisableRadialMenu,
        #endregion

        #region Evaluators Enabled/Disabled
        [EventId(typeof(EventArgs))]
        LocomotionEvaluatorEnabled,
        [EventId(typeof(EventArgs))]
        LocomotionEvaluatorDisabled,
        [EventId(typeof(EventArgs))]
        RaycastEvaluatorEnabled,
        [EventId(typeof(EventArgs))]
        RaycastEvaluatorDisabled,
        [EventId(typeof(EventArgs))]
        RadialMenuEnabled,
        [EventId(typeof(EventArgs))]
        RadialMenuDisabled, 
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

        #region RaycastEvaluatorEventArgs
        [EventId(typeof(RaycastEvaluatorEventArgs))]
        RaycastInteractableSet,
        [EventId(typeof(RaycastEvaluatorEventArgs))]
        RaycastInteractableUnset,
        [EventId(typeof(RaycastEvaluatorEventArgs))]
        RaycastInteractableSelected,
        [EventId(typeof(RaycastEvaluatorEventArgs))]
        RaycastInteractableUnselected,

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

        [EventId(typeof(SpawnPointEventArgs))]
        SpawnPointSelected,

        #region MoveEventArgs
        [EventId(typeof(MoveEventArgs))]
        MoveStarted,
        [EventId(typeof(MoveEventArgs))]
        MoveUpdated,
        [EventId(typeof(MoveEventArgs))]
        MoveEnded,
        #endregion

        #region Placement
        [EventId(typeof(PlacementEventArgs))]
        StartPointPlaced,
        #endregion

        #region CollisionEventArgs
        [EventId(typeof(CollisionEventArgs))]
        WandCursorTriggerEntered,
        #endregion

        #region ControllerEvaluator
        [EventId(typeof(ControllerEvaluatorEventArgs))]
        SetPathStrategy,

        [EventId(typeof(ControllerEvaluatorEventArgs))]
        BeginPlacingStartPoint,
        [EventId(typeof(ControllerEvaluatorEventArgs))]
        FinishPlacingStartPoint,

        [EventId(typeof(ControllerEvaluatorEventArgs))]
        StartDrawingPath,
        [EventId(typeof(ControllerEvaluatorEventArgs))]
        StopDrawingPath,
        [EventId(typeof(ControllerEvaluatorEventArgs))]
        StartErasingPath,
        [EventId(typeof(ControllerEvaluatorEventArgs))]
        StopErasingPath,

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

        #region BulldozeEventArgs
        [EventId(typeof(BulldozeEventArgs))]
        DrawStarted,
        [EventId(typeof(BulldozeEventArgs))]
        DrawUpdated,
        [EventId(typeof(BulldozeEventArgs))]
        DrawEnded, 

        [EventId(typeof(BulldozeEventArgs))]
        EraseStarted,
        [EventId(typeof(BulldozeEventArgs))]
        EraseUpdated,
        [EventId(typeof(BulldozeEventArgs))]
        EraseEnded, 
        #endregion

        #region NodeEventArgs
        [EventId(typeof(PointVisualEventArgs))]
        NodeCreated,
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
        
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class EventIdAttribute : Attribute
    {
        public EventIdAttribute(Type argType) => ArgType = argType;

        public Type ArgType { get; }
    }
}