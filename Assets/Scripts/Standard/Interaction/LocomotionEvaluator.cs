namespace PathNav.Interaction
{
    using Dreamteck.Splines;
    using System;
    using Events;
    using ExperimentControl;
    using Input;
    using Patterns.FSM;
    using UnityEngine;

    public enum LocomotionDof
    {
        None,
        FourDoF,
        SixDof,
    }

    public class LocomotionEvaluator : MonoBehaviour, IActiveState
    {
        #region Local Variables
        [SerializeField] private LocomotionInfo locomotionInfo;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] internal SplineFollower follower;
        internal Transform  PlayerTransform   => playerTransform;
        internal Transform  CameraTransform   => cameraTransform;
        
        private IController _motionController;
        private IController _verticalController;

        internal LocomotionDof dof;

        
        #endregion

        #region Movement Variables
        internal float     MaxVelocity         => locomotionInfo.MaxVelocity;
        internal float     MinVelocity         => locomotionInfo.MinVelocity;
        internal float     Acceleration        => locomotionInfo.Acceleration;
        internal Vector2   InputPose           => _motionController.JoystickPose;
        internal Vector3   VerticalShift       => _verticalController.PointerForward;
        internal Transform VerticalControllerTransform => _verticalController.Transform;
        #endregion

        #region State Variables
        private StateMachine<LocomotionEvaluator> _state = new();
        public bool IsActive => _state.CurrentState != _disabledState && _state.CurrentState != null;

        private LocomotionIdle<LocomotionEvaluator> _idleState = new();
        private LocomotionMove<LocomotionEvaluator> _moveState = new();
        private Disabled<LocomotionEvaluator> _disabledState = new();

        private  bool HasLocomotion       => _state.CurrentState == _moveState;
        private  bool HasMotionController => _motionController   != null;
        internal bool ShouldUseVertical   => _verticalController != null && _hasVerticalInput;

        private bool _hasLocomotionInput;
        private bool _hasVerticalInput;

        private bool ShouldLocomote   => _hasLocomotionInput && HasMotionController;
        private bool ShouldUnlocomote => HasLocomotion       && !_hasLocomotionInput;
        #endregion

        #region Unity Methods
        private void OnEnable()
        {
            SubscribeToEnableDisableEvents();
        }

        private void OnDisable()
        {
            UnsubscribeToEnableDisableEvents();
            Disable();
        }

        private void Update()
        {
            if (!_state.IsConfigured) return;
            if (_state.CurrentState == _disabledState) return;

            _state.UpdateLogic();
            _state.UpdatePhysics();
        }
        #endregion
        
        #region Enable/Disable Methods
        private void Enable()
        {
            if (!_state.IsConfigured) _state.Configure(this, _idleState);

            if (_state.CurrentState == _disabledState) _state.ChangeState(_idleState);

            SubscribeToLocomotionInputEvents();
            OnLocomotionEnabled();
        }

        private void Disable()
        {
            UnsubscribeToLocomotionInputEvents();

            if (_state.CurrentState == _disabledState) return;

            if (_state.CurrentState == _moveState) _state.ChangeState(_idleState);

            if (_state.CurrentState == _idleState) _state.ChangeState(_disabledState);

            OnLocomotionDisabled();
        }
        #endregion

        #region State Management Methods
        private void Locomote()
        {
            if (_state.CurrentState != _idleState) return;

            _state.ChangeState(_moveState);
        }

        private void Unlocomote()
        {
            if (_state.CurrentState != _moveState) return;

            _state.ChangeState(_idleState);
        }

        private void SetActiveMotionController(IController controller)
        {
            _motionController = controller;
        }

        internal void ClearActiveMotionController()
        {
            _motionController = null;
        }
        
        private void SetActiveVerticalController(IController controller)
        {
            _verticalController = controller;
        }

        internal void ClearActiveVerticalController()
        {
            _verticalController = null;
        }
        #endregion

        #region Emitted Events
        internal void OnLocomotionStart()
        {
            EventManager.Publish(EventId.LocomotionStarted, this, GetLocomotionEvaluatorEventArgs());
        }

        internal void OnLocomotionUpdate()
        {
            EventManager.Publish(EventId.LocomotionUpdated, this, GetLocomotionEvaluatorEventArgs());
        }

        internal void OnLocomotionEnd()
        {
            EventManager.Publish(EventId.LocomotionEnded, this, GetLocomotionEvaluatorEventArgs());
        }

        private void OnLocomotionEnabled()
        {
            EventManager.Publish(EventId.LocomotionEvaluatorEnabled, this, EventArgs.Empty);
        }

        private void OnLocomotionDisabled()
        {
            EventManager.Publish(EventId.LocomotionEvaluatorDisabled, this, EventArgs.Empty);
        }

        private LocomotionEvaluatorEventArgs GetLocomotionEvaluatorEventArgs() => new();
        #endregion

        #region Received Events
        private void SubscribeToEnableDisableEvents()
        {
            EventManager.Subscribe<EventArgs>(EventId.EnableLocomotion,  EnableEvaluator);
            EventManager.Subscribe<EventArgs>(EventId.DisableLocomotion, DisableEvaluator);
            EventManager.Subscribe<SceneControlEventArgs>(EventId.SetLocomotionStrategy, SetStrategy);
        }

        private void UnsubscribeToEnableDisableEvents()
        {
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.EnableLocomotion,  EnableEvaluator);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.DisableLocomotion, DisableEvaluator);
            EventManager.Unsubscribe<SceneControlEventArgs>(EventId.SetLocomotionStrategy, SetStrategy);
        }

        private void SubscribeToLocomotionInputEvents()
        {
            EventManager.Subscribe<SceneControlEventArgs>(EventId.FollowPathReady, FollowPath);
            EventManager.Subscribe<ControllerEventArgs>(EventId.JoystickTouchStart, StartLocomotion);
            EventManager.Subscribe<ControllerEventArgs>(EventId.JoystickTouchEnd,   StopLocomotion);
            EventManager.Subscribe<ControllerEventArgs>(EventId.TriggerDown,        StartVertical);
            EventManager.Subscribe<ControllerEventArgs>(EventId.TriggerUp,          StopVertical);
        }

        private void UnsubscribeToLocomotionInputEvents()
        {
            EventManager.Unsubscribe<SceneControlEventArgs>(EventId.FollowPathReady, FollowPath);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.JoystickTouchStart, StartLocomotion);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.JoystickTouchEnd,   StopLocomotion);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.TriggerDown,        StartVertical);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.TriggerUp,          StopVertical);
        }

        private void FollowPath(object sender, SceneControlEventArgs args)
        {
            follower.followSpeed = 0;
        }

        private void StartLocomotion(object sender, ControllerEventArgs args)
        {
            if (HasLocomotion) return;

            SetActiveMotionController(args.Controller);
            _hasLocomotionInput = true;

            if (ShouldLocomote) Locomote();
        }

        private void StopLocomotion(object sender, ControllerEventArgs args)
        {
            _hasLocomotionInput = false;
            ClearActiveMotionController();
            if (ShouldUnlocomote) Unlocomote();
        }

        private void StartVertical(object sender, ControllerEventArgs args)
        {
            if (ShouldUseVertical) return;

            SetActiveVerticalController(args.Controller);
            _hasVerticalInput    = true;
        }

        private void StopVertical(object sender, ControllerEventArgs args)
        {
            _hasVerticalInput    = false;
            ClearActiveVerticalController();
        }

        private void EnableEvaluator(object sender, EventArgs args)
        {
            Enable();
        }

        private void DisableEvaluator(object sender, EventArgs args)
        {
            Disable();
        }

        private void SetStrategy(object sender, SceneControlEventArgs args)
        {
            dof = args.LocomotionDof;
        }
        #endregion
    }

    public class LocomotionEvaluatorEventArgs : EventArgs { }
}