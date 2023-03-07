namespace PathNav.Interaction
{
    using System;
    using System.Threading.Tasks;
    using Events;
    using Input;
    using Patterns.Factory;
    using Patterns.FSM;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    public class LocomotionEvaluator : MonoBehaviour, IActiveState
    {
        #region Local Variables
        [SerializeField] private AssetReferenceGameObject hintKey;
        [SerializeField] private LocomotionInfo locomotionInfo;

        [SerializeField] private Transform playerTransform;
        internal Transform PlayerTransform => playerTransform;

        private Factory _factory = new();

        private GameObject _hintPrefab;
        private GameObject _hintObject;
        private bool _hintConfigured;

        private Vector3 _hintOffset = new (0, 0, 0.05f);

        internal IController activeController;
        private bool HasController => activeController != null;
        #endregion

        #region Movement Variables
        internal float MaxVelocity => locomotionInfo.MaxVelocity;
        internal float MinVelocity => locomotionInfo.MinVelocity;
        internal float Acceleration => locomotionInfo.Acceleration;

        internal Vector2 TouchPose => activeController.TouchPose;
        #endregion

        #region State Variables
        private StateMachine<LocomotionEvaluator> _state = new();
        public bool IsActive => _state.CurrentState != _disabledState && _state.CurrentState != null;

        private LocomotionIdle<LocomotionEvaluator> _idleState = new();
        private LocomotionMove<LocomotionEvaluator> _moveState = new();
        private Disabled<LocomotionEvaluator> _disabledState = new();

        private bool HasHint => _hintObject.activeSelf;
        private bool HasLocomotion => _state.CurrentState == _moveState;
        
        private bool _hasLocomotionInput;

        private bool ShouldLocomote => HasHint         && _hasLocomotionInput && HasController;
        private bool ShouldUnlocomote => HasLocomotion && !_hasLocomotionInput;
        #endregion

        #region Unity Methods
        private async void OnEnable()
        {
            await Enable();
            SubscribeToEnableDisableEvents();
        }

        private void OnDisable()
        {
            UnsubscribeToEnableDisableEvents();
            Disable();
        }

        private void Update()
        {
            if (_state.CurrentState == _disabledState) return;

            _state.UpdateLogic();
        }

        private void LateUpdate()
        {
            if (_state.CurrentState == _disabledState) return;

            _state.UpdatePhysics();
        }
        #endregion

        #region Enable/Disable Methods
        private async Task Enable()
        {
            if (!_hintConfigured)
            {
                Task<bool> task = LoadHintPrefab();
                await task;

                if (task.Result)
                {
                    CreateHintPrefab();
                    _hintConfigured = true;
                }
            }

            if (!_state.IsConfigured) _state.Configure(this, _idleState);

            if (_state.CurrentState == _disabledState) _state.ChangeState(_idleState);

            OnLocomotionEnabled();
            SubscribeToLocomotionInputEvents();
        }

        private async Task<bool> LoadHintPrefab()
        {
            Task<GameObject> task    = _factory.LoadFromKeyAsync<GameObject>(hintKey);
            _hintPrefab = await task;
            return _hintPrefab is not null;
        }

        private void CreateHintPrefab()
        {
            _hintObject = Instantiate(_hintPrefab, transform);
            _hintObject.SetActive(false);
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
        internal void ShowHintVisual()
        {
            _hintObject.transform.parent        = activeController.Transform;
            _hintObject.transform.localRotation = Quaternion.identity;
            _hintObject.transform.localPosition = _hintOffset;
            _hintObject.SetActive(true);
        }

        internal void HideHintVisual()
        {
            _hintObject.SetActive(false);
            _hintObject.transform.parent   = transform;
            _hintObject.transform.rotation = Quaternion.identity;
            _hintObject.transform.position = Vector3.zero;
        }

        internal void Locomote()
        {
            if (_state.CurrentState != _idleState) return;

            _state.ChangeState(_moveState);
        }

        internal void Unlocomote()
        {
            if (_state.CurrentState != _moveState) return;

            _state.ChangeState(_idleState);
        }

        internal void SetActiveController(IController controller)
        {
            activeController = controller;
        }

        internal void ClearActiveController()
        {
            activeController = null;
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
        }

        private void UnsubscribeToEnableDisableEvents()
        {
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.EnableLocomotion,  EnableEvaluator);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.DisableLocomotion, DisableEvaluator);
        }

        private void SubscribeToLocomotionInputEvents()
        {
            EventManager.Subscribe<ControllerEventArgs>(EventId.TouchpadTouchStart, ShowHint);
            EventManager.Subscribe<ControllerEventArgs>(EventId.TouchpadTouchEnd,   HideHint);
            EventManager.Subscribe<ControllerEventArgs>(EventId.JoystickClickStart, StartLocomotion);
            EventManager.Subscribe<ControllerEventArgs>(EventId.JoystickClickEnd,   StopLocomotion);
        }

        private void UnsubscribeToLocomotionInputEvents()
        {
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.TouchpadTouchStart, ShowHint);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.TouchpadTouchEnd,   HideHint);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.JoystickClickStart, StartLocomotion);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.JoystickClickEnd,   StopLocomotion);
        }

        private void StartLocomotion(object sender, ControllerEventArgs args)
        {
            if (HasLocomotion) return;

            SetActiveController(args.Controller);
            _hasLocomotionInput = true;

            if (ShouldLocomote) Locomote();
        }

        private void StopLocomotion(object sender, ControllerEventArgs args)
        {
            _hasLocomotionInput = false;
            if (ShouldUnlocomote) Unlocomote();
        }

        private void ShowHint(object sender, ControllerEventArgs args)
        {
            if (HasLocomotion) return;

            SetActiveController(args.Controller);
            ShowHintVisual();
        }

        private void HideHint(object sender, ControllerEventArgs args)
        {
            HideHintVisual();
        }

        private async void EnableEvaluator(object sender, EventArgs controllerEventArgs)
        {
            await Enable();
        }

        private void DisableEvaluator(object sender, EventArgs controllerEventArgs) 
        {
            Disable();
        }
        #endregion
    }

    public class LocomotionEvaluatorEventArgs : EventArgs
    {

    }
}