namespace PathNav.Interaction
{
    using Events;
    using Extensions;
    using Input;
    using UnityEngine;

    public class SpatulaWand : MonoBehaviour, IRaycastResultProvider, IRayCast
    {
        [SerializeField] [InterfaceType(typeof(IController))]
        private Object controller;
        public IController Controller => controller as IController;
        private ControllerInfo ControllerInfo => Controller.ControllerInfo;
        
        [SerializeField] private LayerMaskInfo maskInfo;
        [SerializeField] private RaycastInfo raycastInfo;

        private IRayCast _raycast;
        
        public float MaxRaycastDistance => raycastInfo.MaxRaycastDistance;

        public Vector3 RayOrigin => Controller.Position;
        public Vector3 RayDirection => Controller.Transform.forward + Controller.Transform.up * ControllerInfo.LaserAngleOffsetRadians;
        
        private RaycastHit _hitResult;
        public RaycastHit HitResult
        {
            get => _hitResult;
            set => _hitResult = value;
        }
        public Vector3 RayHitPoint => _hitResult.point;

        private SurfaceHit _surfaceHit;
        public ISurfaceHit SurfaceHit => _surfaceHit;

        public UniqueId Id { get; private set; }

        #region Unity Methods
        private void Awake()
        {
            _raycast    = this;
            Id          = UniqueId.Generate();
            _surfaceHit = new SurfaceHit();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeToEvents();
        }

        private void Update()
        {
            UpdateRaycast();
        }
        #endregion

        #region Manage Event Subscriptions
        private void SubscribeToEvents()
        {
            //EventManager.Subscribe<ControllerEventArgs>(EventId.JoystickTouchUpdate, ChangeRayLength);
        }

        private void UnsubscribeToEvents()
        {
            //EventManager.Unsubscribe<ControllerEventArgs>(EventId.JoystickTouchUpdate, ChangeRayLength);
        }
        #endregion
        
        #region Logic

        private void UpdateRaycast()
        { 
            ComputeSpawnPoint();
            EventManager.Publish(EventId.RaycastUpdated, this, GetRaycastResultEventArgs());
        }

        public void ComputeSpawnPoint()
        {
            _surfaceHit = new SurfaceHit();

            _surfaceHit = _raycast.Raycast(maskInfo.SpawnMask) 
                ? new SurfaceHit(_hitResult, new SpawnPointElement(_hitResult.point)) 
                : new SurfaceHit(MaxRaycastDistance);
        }

        private RaycastResultEventArgs GetRaycastResultEventArgs() => new(this, Id);
        #endregion
    }
}
