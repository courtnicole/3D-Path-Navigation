namespace PathNav.Interaction
{
    using System;
    using System.Collections.Generic;
    using Events;
    using Extensions;
    using Input;
    using Patterns.FSM;
    using UnityEngine;

    public enum RaycastState
    {
        Idle,
        Hover,
        Select,
        CanSpawn,
        Disabled,
    }

    public class RaycastEvaluator : MonoBehaviour, IRaycastResultProvider, ISpawnPointProvider, IActiveState, IRayCast, ISphereCast, IBoundsCast
    {
        [SerializeField] [InterfaceType(typeof(IController))]
        private UnityEngine.Object controller;

        public IController Controller => controller as IController;

        private ControllerInfo ControllerInfo => Controller.ControllerInfo;

        public UniqueId Id { get; private set; }

        #region Raycast Variables
        [SerializeField] private LayerMaskInfo maskInfo;
        [SerializeField] private RaycastInfo raycastInfo;

        private IRayCast _raycast;
        private ISphereCast _spherecast;
        private IBoundsCast _boundsCast;
        //public event Action<RaycastResultEventArgs> RaycastUpdated = delegate { };

        public Bounds BoundingVolume => maskInfo.BoundingVolume;
        public RaycastHit HitResult
        {
            get => _hitResult;
            set => _hitResult = value;
        }
        private RaycastHit _hitResult;
        private SurfaceHit _surfaceHit;
        public ISurfaceHit SurfaceHit => _surfaceHit;

        public float MaxRaycastDistance => raycastInfo.MaxRaycastDistance;
        public float Radius => raycastInfo.RaycastSphereSize;
        public Vector3 RayOrigin => Controller.Position;
        public Vector3 RayDirection => Controller.Transform.forward + Controller.Transform.up * ControllerInfo.LaserAngleOffsetRadians;
        public Vector3 RayHitPoint => _hitResult.point;
        #endregion

        #region Interactable Variables
        internal IInteractable Candidate { get; set; }
        internal IInteractable Interactable { get; set; }
        internal IInteractable Selected { get; set; }

        internal bool HasCandidate => Candidate       != null;
        internal bool HasInteractable => Interactable != null;
        internal bool HasSelected => Selected         != null;
        #endregion

        #region Spawn Point Variables
        internal ISpawnPointProvider spawnPointProvider;
        public ISpawnPoint SpawnPointCandidate { get; set; }
        public ISpawnPoint SpawnPoint { get; set; }
        #endregion

        #region State Variables
        private StateMachine<RaycastEvaluator> _state = new();

        public bool IsActive => _state.CurrentState != _disabledState && _state.CurrentState != null;

        public bool PreviouslyIdle => _previousState == RaycastState.Idle;

        private RaycastState State
        {
            get
            {
                if (_state.CurrentState == _idleState) return RaycastState.Idle;

                if (_state.CurrentState == _hoverState) return RaycastState.Hover;

                if (_state.CurrentState == _selectState) return RaycastState.Select;

                if (_state.CurrentState == _canSpawnState) return RaycastState.CanSpawn;

                return RaycastState.Disabled;
            }
        }

        private RaycastState _currentState;
        private RaycastState _previousState;

        internal Queue<bool> selectorQueue = new();
        internal bool QueuedSelect => selectorQueue.Count   > 0 && selectorQueue.Peek();
        internal bool QueuedUnselect => selectorQueue.Count > 0 && !selectorQueue.Peek();

        private RaycastEvaluatorHover<RaycastEvaluator> _hoverState = new();
        private RaycastEvaluatorIdle<RaycastEvaluator> _idleState = new();
        private RaycastEvaluatorCanSpawn<RaycastEvaluator> _canSpawnState = new();
        private Disabled<RaycastEvaluator> _disabledState = new();
        private RaycastEvaluatorSelect<RaycastEvaluator> _selectState = new();

        public bool ShouldHover
        {
            get
            {
                if (_state.CurrentState != _idleState) return false;

                return HasCandidate || QueuedSelect;
            }
        }

        public bool ShouldUnhover
        {
            get
            {
                if (_state.CurrentState != _hoverState) return false;

                return Interactable != Candidate || Candidate == null;
            }
        }

        public bool ShouldSelect
        {
            get
            {
                if (_state.CurrentState != _hoverState) return false;

                return Candidate == Interactable && QueuedSelect;
            }
        }

        public bool ShouldUnselect => _state.CurrentState == _selectState && QueuedUnselect;

        public bool ShouldAcquireSpawnPoint
        {
            get
            {
                if (_state.CurrentState != _idleState) return false;

                return (this as ISpawnPointProvider).HasSpawnCandidate;
            }
        }

        public bool ShouldClearSpawnPoint
        {
            get
            {
                if (_state.CurrentState != _canSpawnState) return false;

                return SpawnPointCandidate == null;
            }
        }

        public bool ShouldSelectSpawnPoint
        {
            get
            {
                if (_state.CurrentState != _canSpawnState) return false;

                return (this as ISpawnPointProvider).HasSpawnPoint && QueuedSelect;
            }
        }
        #endregion

        #region Unity Methods
        private void Awake()
        {
            Id                = UniqueId.Generate();
            spawnPointProvider = this;
            _raycast           = this;
            _spherecast        = this;
        }

        private void OnEnable()
        {
            SubscribeToEvaluatorEvents();
            Enable();
        }

        private void OnDisable()
        {
            UnsubscribeToEvaluatorEvents();
            Disable();
        }

        private void Enable()
        {
            if (!_state.IsConfigured) _state.Configure(this, _idleState);

            if (_state.CurrentState == _disabledState) _state.ChangeState(_idleState);

            _currentState  = State;
            _previousState = State;

            selectorQueue.Clear();

            OnEvaluatorEvent(EventId.RaycastEvaluatorEnabled);
        }

        private void Disable()
        {
            if (_state.CurrentState == _disabledState)
            {
                OnEvaluatorEvent(EventId.RaycastEvaluatorDisabled);
                return;
            }

            if (_state.CurrentState == _selectState) _state.ChangeState(_hoverState);

            if (_state.CurrentState == _hoverState) _state.ChangeState(_idleState);

            if (_state.CurrentState == _idleState) _state.ChangeState(_disabledState);

            _currentState  = State;
            _previousState = State;

            OnEvaluatorEvent(EventId.RaycastEvaluatorDisabled);
        }
        #endregion

        #region State Management
        private void Update()
        {
            if (_state.CurrentState == _disabledState) return;

            _state.UpdateLogic();

            OnRaycastEvent();

            _previousState = _currentState;
            _currentState  = State;

            selectorQueue.Clear();
        }

        internal void Hover()
        {
            if (_state.CurrentState != _idleState) return;

            _state.ChangeState(_hoverState);
        }

        internal void Unhover()
        {
            if (_state.CurrentState != _hoverState) return;

            _state.ChangeState(_idleState);
        }

        public void AcquireSpawnPoint()
        {
            if (_state.CurrentState != _idleState) return;

            _state.ChangeState(_canSpawnState);
        }

        public void ClearSpawnPoint()
        {
            if (_state.CurrentState != _canSpawnState) return;

            _state.ChangeState(_idleState);
        }

        internal void SelectSpawnPoint()
        {
            if (_state.CurrentState != _canSpawnState) return;

            while (QueuedSelect)
            {
                selectorQueue.Dequeue();
            }

            OnSpawnPointEvent(EventId.SpawnPointSelected, SpawnPoint);
            _state.ChangeState(_idleState);
        }

        internal void Select()
        {
            if (_state.CurrentState != _hoverState) return;

            _state.ChangeState(_selectState);
        }

        internal void Unselect()
        {
            if (_state.CurrentState != _selectState) return;

            _state.ChangeState(_hoverState);
        }
        #endregion

        #region Process Candidates
        internal bool ProcessInteractableCandidate()
        {
            Candidate = null;
            Candidate = ComputeInteractableCandidate();
            return Candidate != null;
        }

        internal bool ProcessSpawnCandidate()
        {
            SpawnPointCandidate = null;
            SpawnPointCandidate = ComputeSpawnPoint();
            return SpawnPointCandidate != null;
        }

        private IInteractable ComputeInteractableCandidate()
        {
            _surfaceHit = new SurfaceHit();

            if (_spherecast.SphereCast(maskInfo.InteractableMask))
                _surfaceHit = new SurfaceHit(_hitResult, _hitResult.collider.gameObject.GetComponent<InteractableElement>());

            return _surfaceHit.Interactable;
        }

        public ISpawnPoint ComputeSpawnPoint()
        {
            _surfaceHit = new SurfaceHit();

            if (_raycast.Raycast(maskInfo.SpawnMask)) 
                _surfaceHit = new SurfaceHit(_hitResult, new SpawnPointElement(_hitResult.point));

            return _surfaceHit.SpawnPoint;
        }
        #endregion

        #region Received Events
        private void SubscribeToEvaluatorEvents()
        {
            EventManager.Subscribe<ControllerEventArgs>(EventId.TriggerDown, DoSelected);
            EventManager.Subscribe<ControllerEventArgs>(EventId.GripEnd,     DoUnselected);
        }

        private void UnsubscribeToEvaluatorEvents()
        {
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.TriggerDown, DoSelected);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.GripEnd,     DoUnselected);
        }

        private void DoSelected(object sender, ControllerEventArgs controllerEventArgs)
        {
            if (sender != Controller) return;
            selectorQueue.Enqueue(true);
        }

        private void DoUnselected(object sender, ControllerEventArgs controllerEventArgs)
        {
            if (sender != Controller) return;
            selectorQueue.Enqueue(false);
        }
        #endregion

        #region Emitted Events
        private void OnSpawnPointEvent(EventId id, ISpawnPoint spawnPoint)
        {
            EventManager.Publish(id, this, GetSpawnPointEventArgs(spawnPoint));
        }

        internal void OnInteractableEvent(EventId id, IInteractable interactable)
        {
            EventManager.Publish(id, this, GetRaycastEvaluatorEventArgs(interactable));
        }

        internal void OnEvaluatorEvent(EventId id)
        {
            EventManager.Publish(id, this, EventArgs.Empty);
        }

        internal void OnRaycastEvent()
        {
            EventManager.Publish(EventId.RaycastUpdated, this, GetRaycastResultEventArgs());
        }
        #endregion

        #region EventArgs
        private RaycastEvaluatorEventArgs GetRaycastEvaluatorEventArgs(IInteractable interactable) => new(interactable, Controller);

        private RaycastResultEventArgs GetRaycastResultEventArgs() => new(this, Id);

        private SpawnPointEventArgs GetSpawnPointEventArgs(ISpawnPoint spawnPoint) => new(spawnPoint, Controller);
        #endregion
    }

    public class RaycastEvaluatorEventArgs : EventArgs
    {
        public RaycastEvaluatorEventArgs(IInteractable interactable, IController controller)
        {
            Interactable  = interactable;
            Controller    = controller;
        }
        
        public IInteractable Interactable { get; }
        public IController Controller { get; }
    }

    public class RaycastResultEventArgs : EventArgs
    {
        public RaycastResultEventArgs(IRaycastResultProvider result, UniqueId id)
        {
            Result        = result;
            Id            = id;
        }

        public IRaycastResultProvider Result { get; }
        public UniqueId Id { get; set; }
    }
}