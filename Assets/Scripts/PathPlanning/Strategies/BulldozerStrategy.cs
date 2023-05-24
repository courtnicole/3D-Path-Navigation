namespace PathNav.PathPlanning
{
    using Events;
    using Input;
    using Interaction;
    using Patterns.FSM;
    using UnityEngine;

    public class BulldozerStrategy : IPathStrategy
    {
        #region State Variables
        private StateMachine<BulldozerStrategy> _state = new();

        public bool IsActive => _state.CurrentState != _disabledState && _state.CurrentState != null;

        private Disabled<BulldozerStrategy> _disabledState = new();
        private BulldozerEvaluatorIdle<BulldozerStrategy> _idleState = new();
        private BulldozerEvaluatorDraw<BulldozerStrategy> _drawState = new();
        private BulldozerEvaluatorErase<BulldozerStrategy> _eraseState = new();

        internal bool CanStartErasing
        {
            get
            {
                if (_state.CurrentState != _idleState) return false;

                return HasSegment && HasMultipleSegmentPoints && HasValidEraseTarget;
            }
        }

        internal bool CanStartDrawing => HasSegment && HasFirstSegmentPoint;
        #endregion

        #region Controller Variables
        internal IController[] Controllers => ControllerManagement.controllers;
        internal IController interactingController;
        private void SetController(IController controller) => interactingController = controller;
        private void ClearController() => interactingController = null;
        #endregion

        #region Segment and Path Variables
        internal Vector3 lastHandPosition;
        internal const float minimumDelta = 0.025f;
        internal int TargetPointIndex => ActiveSegment.SelectedPointVisualIndex;

        private bool HasSegment => ActiveSegment != null;

        private bool HasFirstSegmentPoint => ActiveSegment.CurrentPointCount     > 1;
        private bool HasMultipleSegmentPoints => ActiveSegment.CurrentPointCount > 2;

        private bool HasValidEraseTarget => ActiveSegment.SelectedSegmentIndex == ActiveSegment.CurrentPointCount - 1;
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

            if (_state.CurrentState == _eraseState) _state.ChangeState(_idleState);

            if (_state.CurrentState == _drawState) _state.ChangeState(_idleState);

            if (_state.CurrentState == _idleState) _state.ChangeState(_disabledState);
        }

        private void FinishPath(object obj, ControllerEvaluatorEventArgs args)
        {
            if (_state.CurrentState == _eraseState) _state.ChangeState(_idleState);

            if (_state.CurrentState == _drawState) _state.ChangeState(_idleState);

            if (_state.CurrentState != _idleState)
            {
                throw new System.Exception("BulldozerStrategy: FinishPath called while not in idle state");
            }

            ActiveSegment.SaveSpline();
        }
        #endregion

        #region Events
        public void SubscribeToEvents()
        {
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.StartDrawOrErasePath, EvaluateStartDrawOrErasePath);
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.StopDrawOrErasePath,  EvaluateStopDrawOrErasePath);
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.PathCreationComplete, FinishPath);
        }

        public void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.StartDrawOrErasePath, EvaluateStartDrawOrErasePath);
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.StopDrawOrErasePath,  EvaluateStopDrawOrErasePath);
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.PathCreationComplete, FinishPath);
        }
        #endregion

        #region Logic
        private void EvaluateStartDrawOrErasePath(object obj, ControllerEvaluatorEventArgs args)
        {
            SetController(args.Controller);

            if (ActiveSegment.CanErasePoint(ref interactingController))
            {
                if (!CanStartErasing) return;
                StartErase();
            }
            else
            {
                if (!CanStartDrawing) return;

                StartDraw();
            }
        }

        private void EvaluateStopDrawOrErasePath(object obj, ControllerEvaluatorEventArgs args)
        {
            if (_state.CurrentState == _eraseState)
            {
                StopErase();
            }
            else if (_state.CurrentState == _drawState)
            {
                StopDraw();
            }

            ClearController();
        }
        #endregion

        #region State Change Logic
        private void StartDraw()
        {
            if (_state.CurrentState != _idleState) return;

            _state.ChangeState(_drawState);
        }

        private void StopDraw()
        {
            if (_state.CurrentState != _drawState) return;

            _state.ChangeState(_idleState);
        }

        private void StartErase()
        {
            if (_state.CurrentState != _idleState) return;

            _state.ChangeState(_eraseState);
        }

        private void StopErase()
        {
            if (_state.CurrentState != _eraseState) return;

            _state.ChangeState(_idleState);
        }
        #endregion
    }
}