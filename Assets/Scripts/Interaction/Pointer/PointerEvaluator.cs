namespace PathNav.Interaction
{
    using Input;
    using UnityEngine;

    public class PointerEvaluator : MonoBehaviour, IRayCast
    {
        #region Class Variables
        [InterfaceType(typeof(IController))] [SerializeField]
        private Object controller;

        [SerializeField] private bool debug;
        [SerializeField] private LayerMask mask;
        [SerializeField] [Range(0.1f, 15f)] private float maxRaycastDistance = 15f;
        [SerializeField] private LineRenderer lineRenderer;
        private IController Controller => controller as IController;

        private IRayCast _rayCast;
        private bool _result;
        private bool _enabled;
        private bool _isLocomotion;

        private Vector3 _previousHitPoint;
        private Vector3 _previousOrigin;
        private Vector3 CurrentHitPoint => lineRenderer.GetPosition(1);
        #endregion

        #region Implementation of IRayCast
        public Vector3 RayOrigin    => Controller.PointerPosition;
        public Vector3 RayDirection => Controller.PointerForward;

        public float MaxRaycastDistance => _isLocomotion ? _locomotionMaxRaycastDistance : maxRaycastDistance;

        private const float _locomotionMaxRaycastDistance = 0.25f;

        public RaycastHit HitResult { get; set; }

        public Vector3 RayHitPoint { get; set; }
        #endregion

        private void Awake()
        {
            _rayCast             = this;
            _enabled             = debug ? true : false;
            lineRenderer.enabled = debug ? true : false;
        }

        public void Enable()
        {
            _isLocomotion           = false;
            _enabled                = true;
            EnableLineRenderer();
        }

        public void EnableLocomotion()
        {
            _enabled                = true;
            _isLocomotion           = true;
            EnableLineRenderer();
        }

        private void EnableLineRenderer()
        {
            lineRenderer.startWidth = _isLocomotion ? 0.005f : 0.015f;
            lineRenderer.endWidth   = _isLocomotion ? 0.005f : 0.015f;
            lineRenderer.enabled    = true;
        }

        private void LateUpdate()
        {
            if (!_enabled) return;
            _previousHitPoint = RayHitPoint;
            _previousOrigin   = RayOrigin;

            _result = _rayCast.Raycast(mask);
            UpdateVisualRaycast();
        }

        private void UpdateVisualRaycast()
        {
            if (!_enabled) return;
            if (Vector3.Distance(_previousHitPoint, RayHitPoint) < 0.0015f && Vector3.Distance(_previousOrigin, RayOrigin) < 0.0015f) return;

            lineRenderer.SetPosition(0, RayOrigin);
            lineRenderer.SetPosition(1, RayHitPoint);
        }
    }
}