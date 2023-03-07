namespace PathNav.PathPlanning
{
    using Events;
    using Input;
    using Interaction;
    using Patterns.FSM;
    using UnityEngine;

    public class SpatulaStrategy : IPathStrategy
    {
        #region State Variables
        private StateMachine<SpatulaStrategy> _state = new();

        public bool IsActive => _state.CurrentState != _disabledState && _state.CurrentState != null;

        private Disabled<SpatulaStrategy> _disabledState = new();
        private SpatulaStrategyIdle<SpatulaStrategy> _idleState = new();
        private SpatulaStrategyCreatePoints<SpatulaStrategy> _createPointState = new();
        private SpatulaStrategyMovePoints<SpatulaStrategy> _movePointState = new();

        public bool CanMovePoint => HasSegment && HasMultipleSegmentPoints;
        public bool CanPlacePoint => HasSegment   && HasFirstSegmentPoint;
        #endregion

        #region Controller Variables
        internal IController[] Controllers => ControllerManagement.controllers;
        internal IController interactingController;
        private void SetController(IController controller) => interactingController = controller;
        internal void ClearController() => interactingController = null;
        #endregion

        #region Segment and Path Variables
        internal Vector3 lastHandPosition;
        internal float minimumDelta = 0.025f;

        private const float _maxDistance = 0.08f;
        internal int pointIndexToMove;

        private bool HasController => interactingController                      != null;
        private bool HasSegment => ActiveSegment                                 != null;
        private bool HasStartPosition => StartPosition                           != Vector3.zero;
        private bool HasFirstSegmentPoint => ActiveSegment.CurrentPointCount     > 0;
        private bool HasMultipleSegmentPoints => ActiveSegment.CurrentPointCount > 1;
        #endregion
       
        #region Implementation of IPathStrategy
        public ISegment ActiveSegment { get; set; }
        public Vector3 StartPosition { get; set; }
        public Vector3 StartHeading { get; set; }

        public void Enable()
        {
            if (!_state.IsConfigured) _state.Configure(this, _idleState);
            if (_state.CurrentState == _disabledState) _state.ChangeState(_idleState);
        }

        public void Run()
        {
            if (_state.CurrentState == _disabledState) return;
            
            _state.UpdateLogic();
        }

        public void Disable()
        {
            if (_state.CurrentState == _disabledState) return;

            if (_state.CurrentState == _createPointState) _state.ChangeState(_idleState);

            if (_state.CurrentState == _movePointState) _state.ChangeState(_idleState);

            if (_state.CurrentState == _idleState) _state.ChangeState(_disabledState);
        }

        public void SubscribeToEvents()
        {
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.StartPlaceOrMovePoint, EvaluateStartPlaceOrMovePoint);
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.StopPlaceOrMovePoint,  EvaluateStopPlaceOrMovePoint);
            //EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.PlacePoint,       PlacePoint);
            //EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.StartMovingPoint, StartMovingPoint);
            //EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.StopMovingPoint,  StopMovingPoint);
        }

        public void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.StartPlaceOrMovePoint, EvaluateStartPlaceOrMovePoint);
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.StopPlaceOrMovePoint,  EvaluateStopPlaceOrMovePoint);
            //EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.PlacePoint, PlacePoint);
            //EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.StartMovingPoint, StartMovingPoint);
            //EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.StopMovingPoint,  StopMovingPoint);
        }
        #endregion

        #region Received Events
        private void EvaluateStartPlaceOrMovePoint(object sender, ControllerEvaluatorEventArgs args)
        {
            SetController(args.Controller);

            if (ActiveSegment.IsCloseToPoint(out pointIndexToMove))
            {
                if (!CanMovePoint) return;
                StartMovePoint();
            }
            else
            {
                if (!CanPlacePoint) return;
                StartCreatePoint();
            }
        }

        private void EvaluateStopPlaceOrMovePoint(object sender, ControllerEvaluatorEventArgs args)
        {
            SetController(args.Controller);

            if (_state.CurrentState != _movePointState) return;
            
            StopMovePoint();
        }
        #endregion

        #region Logic
        internal void StartCreatePoint()
        {
            if (_state.CurrentState == _disabledState) return;

            _state.ChangeState(_createPointState);
        }

        internal void StopCreatePoint()
        {
            if (_state.CurrentState != _createPointState) return;

            _state.ChangeState(_idleState);
        }

        internal void StartMovePoint()
        {
            if (_state.CurrentState != _idleState) return;
            
            _state.ChangeState(_movePointState);
        }

        internal void StopMovePoint()
        {
            if (_state.CurrentState != _movePointState) return;

            _state.ChangeState(_idleState);
        }
        #endregion
    }
}
