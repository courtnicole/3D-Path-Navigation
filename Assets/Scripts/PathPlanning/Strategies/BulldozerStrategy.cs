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

        public bool CanStartErasing
        {
            get
            {
                if (_state.CurrentState != _drawState) return false;

                return HasSegment && HasMultipleSegmentPoints;
            }
        }

        public bool CanStartDrawing => HasSegment && HasFirstSegmentPoint;

        public bool CanUpdatePath
        {
            get
            {
                if (_state.CurrentState != _idleState || _state.CurrentState != _eraseState) return false;

                return HasSegment && HasFirstSegmentPoint;
            }
        }
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

        private bool HasController => interactingController != null;
        private bool HasSegment => ActiveSegment            != null;
        private bool HasStartPosition => StartPosition      != Vector3.zero;

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

            if (_state.CurrentState == _eraseState) _state.ChangeState(_idleState);

            if (_state.CurrentState == _drawState) _state.ChangeState(_idleState);

            if (_state.CurrentState == _idleState) _state.ChangeState(_disabledState);
        }

        public void SubscribeToEvents()
        {
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.StartDrawingPath,     StartDrawingPath);
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.StopDrawingPath,      StopDrawingPath);
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.PathCreationComplete, FinishPath);
        }

        public void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.StartDrawingPath,     StartDrawingPath);
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.StopDrawingPath,      StopDrawingPath);
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.PathCreationComplete, FinishPath);
        }

        private void StartDrawingPath(object obj, ControllerEvaluatorEventArgs args)
        {
            if (!CanStartDrawing) return;
            SetController(args.Controller);
            StartDraw();
        }

        private void StopDrawingPath(object obj, ControllerEvaluatorEventArgs args)
        {
            StopDraw();
            ClearController();
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

        #region State Change Logic
        private void StartDraw()
        {
            if (_state.CurrentState == _disabledState) return;

            _state.ChangeState(_drawState);
        }

        private void StopDraw()
        {
            if (_state.CurrentState != _drawState) return;

            _state.ChangeState(_idleState);
        }

        internal void StartErase()
        {
            if (_state.CurrentState != _drawState) return;

            _state.ChangeState(_eraseState);
        }

        internal void StopErase()
        {
            if (_state.CurrentState != _eraseState) return;

            _state.ChangeState(_drawState);
        }
        
        #endregion
    }
}
