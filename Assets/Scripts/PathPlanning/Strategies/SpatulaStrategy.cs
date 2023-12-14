namespace PathNav.PathPlanning
{
    using Events;
    using Input;
    using Interaction;
    using Patterns.FSM;
    using UnityEngine;

    public class SpatulaStrategy : IPathStrategy
    {
        public SpatulaStrategy(IPlacementPlane placementPlane) => _placementPlane = placementPlane;

        #region State Variables
        private StateMachine<SpatulaStrategy> _state = new();

        public bool IsActive => _state.CurrentState != _disabledState && _state.CurrentState != null;

        private Disabled<SpatulaStrategy> _disabledState = new();
        private SpatulaStrategyIdle<SpatulaStrategy> _idleState = new();
        private SpatulaStrategyCreatePoints<SpatulaStrategy> _createPointState = new();
        private SpatulaStrategyMovePoints<SpatulaStrategy> _movePointState = new();
        private SpatulaStrategyDeletePoint<SpatulaStrategy> _deletePointState = new();

        private bool CanMoveOrDeletePoint  => HasSegment && HasFirstSegmentPoint;
        private bool CanPlacePoint => HasSegment && HasFirstSegmentPoint && HasValidPlane;
        #endregion

        #region Controller Variables
        internal IController[] Controllers => ControllerManagement.controllers;
        internal IController interactingController;
        private void SetController(IController controller) => interactingController = controller;
        internal void ClearController() => interactingController = null;
        #endregion

        #region Segment and Path Variables
        internal Vector3 lastHandPosition;
        internal const float minimumDelta = 0.025f;

        internal int PointIndexToMoveOrDelete => ActiveSegment.SelectedPointVisualIndex;
        private IPlacementPlane _placementPlane;

        private bool HasController => interactingController                      != null;
        private bool HasSegment => ActiveSegment                                 != null;
        private bool HasValidPlane => _placementPlane?.HasCollidingController    == true;
        private bool HasStartPosition => StartPosition                           != Vector3.zero;
        private bool HasFirstSegmentPoint => ActiveSegment.CurrentPointCount     > 0;
        private bool HasMultipleSegmentPoints => ActiveSegment.CurrentPointCount > 1;

        private bool HasValidPointTarget => ActiveSegment.SelectedPointVisualIndex > -1;
        #endregion

        #region Implementation of IPathStrategy
        public ISegment ActiveSegment { get; set; }
        public Vector3 StartPosition { get; set; }
        public Vector3 StartHeading { get; set; }

        public void Enable()
        {
            if (!_state.IsConfigured) _state.Configure(this, _idleState);
            _placementPlane.Enable();
            if (_state.CurrentState == _disabledState) _state.ChangeState(_idleState);
        }

        public void Run()
        {
            if (_state.CurrentState == _disabledState) return;

            _state.UpdateLogic();
        }

        public void Disable()
        {
            if (_state.CurrentState == _createPointState) _state.ChangeState(_idleState);

            if (_state.CurrentState == _movePointState) _state.ChangeState(_idleState);

            if (_state.CurrentState == _idleState) _state.ChangeState(_disabledState);

            if(_placementPlane.IsActive) _placementPlane?.Disable();
            
            UnsubscribeToEvents();
        }

        public void SubscribeToEvents()
        {
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.StartPlaceOrMovePoint, EvaluateStartPlaceOrMovePoint);
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.StopPlaceOrMovePoint,  EvaluateStopPlaceOrMovePoint);
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.RemovePoint,           EvaluateRemovePoint);
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.PathCreationComplete,  FinishPath);
        }

        public void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.StartPlaceOrMovePoint, EvaluateStartPlaceOrMovePoint);
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.StopPlaceOrMovePoint,  EvaluateStopPlaceOrMovePoint);
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.RemovePoint,           EvaluateRemovePoint);
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.PathCreationComplete,  FinishPath);
        }

        
        #endregion

        #region Received Events
        private void EvaluateStartPlaceOrMovePoint(object sender, ControllerEvaluatorEventArgs args)
        {
            SetController(args.Controller);

            if (HasValidPointTarget)
            {
                if (!CanMoveOrDeletePoint) return;

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
            ClearController();

            if (_state.CurrentState != _movePointState) return;

            StopMovePoint();
        }
        
        private void FinishPath(object obj, ControllerEvaluatorEventArgs args)
        {
            if (_state.CurrentState == _disabledState) return;
            
            if (_state.CurrentState == _createPointState) _state.ChangeState(_idleState);

            if (_state.CurrentState == _movePointState) _state.ChangeState(_idleState);

            if (_state.CurrentState != _idleState)
            {
                throw new System.Exception("SpatulaStrategy: FinishPath called while not in idle state");
            }

            ActiveSegment.SaveSpline();
            ClearController();
            Disable();
        }
        private void EvaluateRemovePoint(object sender, ControllerEvaluatorEventArgs args)
        {
            SetController(args.Controller);
            if (!HasValidPointTarget) return;
            if (!CanMoveOrDeletePoint) return;

            StartDeletePoint();
        }
        #endregion

        #region Logic
        private void StartCreatePoint()
        {
            if (_state.CurrentState == _disabledState) return;

            _state.ChangeState(_createPointState);
        }

        internal void StopCreatePoint()
        {
            if (_state.CurrentState != _createPointState) return;

            _state.ChangeState(_idleState);
        }
        
        private void StartDeletePoint()
        {
            if (_state.CurrentState == _disabledState) return;

            _state.ChangeState(_deletePointState);
        }

        internal void StopDeletePoint()
        {
            if (_state.CurrentState != _deletePointState) return;

            _state.ChangeState(_idleState);
            ClearController();
        }


        private void StartMovePoint()
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