namespace PathNav.Interaction
{
    using Events;
    using Extensions;
    using Input;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class SpatulaDepthPlane : MonoBehaviour, IRaycastResultProvider
    {
        [SerializeField] [InterfaceType(typeof(IController))]
        private Object controller;

        public IController Controller => controller as IController;

        private ControllerInfo ControllerInfo => Controller.ControllerInfo;
        
        private RaycastHit _hitResult;

        public ISurfaceHit SurfaceHit { get; private set; }
        private float _rayLength = 0.25f;
        public Vector3 RayOrigin => Controller.Position;
        public Vector3 RayDirection => Controller.Transform.forward + Controller.Transform.up * ControllerInfo.LaserAngleOffsetRadians;
        public Vector3 RayHitPoint => _hitResult.point;

        public UniqueId Id { get; private set; }

        #region Unity Methods
        private void Awake()
        {
            Id         = UniqueId.Generate();
            SurfaceHit = new SurfaceHit(_rayLength);
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
            EventManager.Subscribe<ControllerEventArgs>(EventId.JoystickPoseUpdate, ChangeRayLength);
        }

        private void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.JoystickPoseUpdate, ChangeRayLength);
        }
        #endregion

        #region Received Events
        private void ChangeRayLength(object sender, ControllerEventArgs args)
        {
            if (sender != Controller) return;

            float value = 0.0025f * args.Controller.JoystickPose.y;
            _rayLength += value;

            if (_rayLength < 0.05f) _rayLength = 0.05f;
        }
        #endregion

        #region Logic
        private void UpdateRaycast()
        {
            SurfaceHit = new SurfaceHit(_rayLength);
            EventManager.Publish(EventId.RaycastUpdated, this, GetRaycastResultEventArgs());
        }

        private RaycastResultEventArgs GetRaycastResultEventArgs() => new(this, Id);
        #endregion
    }
    

}
