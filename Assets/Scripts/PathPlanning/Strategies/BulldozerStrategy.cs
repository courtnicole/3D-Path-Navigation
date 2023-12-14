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
        private void ClearController()                     => interactingController = null;
        #endregion

        #region Segment and Path Variables
        internal Vector3 lastHandPosition;
        internal const float minimumDelta = 0.025f;
        internal int TargetPointIndex => ActiveSegment.SelectedPointVisualIndex;

        private bool HasSegment => ActiveSegment != null;

        private bool HasFirstSegmentPoint     => ActiveSegment.CurrentPointCount > 1;
        private bool HasMultipleSegmentPoints => ActiveSegment.CurrentPointCount > 2;

        private bool HasValidEraseTarget => ActiveSegment.SelectedSegmentIndex != -1; //== ActiveSegment.CurrentPointCount - 1;
        #endregion

        #region Implementation of IPathStrategy
        public ISegment ActiveSegment { get; set; }
        public Vector3  StartPosition { get; set; }
        public Vector3  StartHeading  { get; set; }

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
            
            UnsubscribeToEvents();
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

            Disable();
        }
        #endregion

        #region Events
        public void SubscribeToEvents()
        {
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.StartDrawPath,        EvaluateStartDrawPath);
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.StopDrawPath,         EvaluateStopDrawPath);
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.StartErasePath,       EvaluateStartErasePath);
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.StopErasePath,        EvaluateStopErasePath);
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.PathCreationComplete, FinishPath);
        }

        public void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.StartDrawPath,        EvaluateStartDrawPath);
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.StopDrawPath,         EvaluateStopDrawPath);
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.StartErasePath,       EvaluateStartErasePath);
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.StopErasePath,        EvaluateStopErasePath);
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.PathCreationComplete, FinishPath);
        }
        #endregion

        #region Logic
        private void EvaluateStartErasePath(object obj, ControllerEvaluatorEventArgs args)
        {
            SetController(args.Controller);
            if (!ActiveSegment.CanErasePoint(ref interactingController)) return;
            if (!CanStartErasing) return;

            StartErase();
        }

        private void EvaluateStopErasePath(object obj, ControllerEvaluatorEventArgs args)
        {
            StopErase();
            ClearController();
        }

        private void EvaluateStartDrawPath(object obj, ControllerEvaluatorEventArgs args)
        {
            SetController(args.Controller);
            if (!CanStartDrawing) return;
            StartDraw();
        }

        private void EvaluateStopDrawPath(object obj, ControllerEvaluatorEventArgs args)
        {
            StopDraw();
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