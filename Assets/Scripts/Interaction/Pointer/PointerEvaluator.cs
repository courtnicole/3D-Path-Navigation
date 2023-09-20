namespace PathNav.Interaction
{
    using Input;
    using UnityEngine;

    public class PointerEvaluator : MonoBehaviour, IRayCast
    {
        #region Class Variables
        [InterfaceType(typeof(IController))] [SerializeField]
        private Object controller;

        [SerializeField] private LayerMask mask;
        [SerializeField] [Range(0.1f, 15f)] private float maxRaycastDistance = 15f;
        [SerializeField] private LineRenderer lineRenderer;
        private IController Controller => controller as IController;

        private IRayCast _rayCast;
        private bool _result;
        private Vector3 LineOrigin => Controller.Position;
        
        private Vector3 _previousHitPoint;
        private Vector3 _previousOrigin;
        private Vector3 CurrentHitPoint => lineRenderer.GetPosition(1);
        #endregion

        #region Implementation of IRayCast
        public Vector3 RayOrigin => Controller.PointerPosition;
        public Vector3 RayDirection => Controller.PointerForward;
        public float MaxRaycastDistance => maxRaycastDistance;
        private RaycastHit _hitResult;

        public RaycastHit HitResult
        {
            get => _hitResult;
            set => _hitResult = value;
        }
        
        private Vector3 _rayHitPoint;

        public Vector3 RayHitPoint
        {
            get => _rayHitPoint;
            set => _rayHitPoint = value;
        } 
        #endregion
        private void Awake()
        {
            _rayCast = this;
        }
        
        private void OnEnable()
        {
            EnableLineRenderer();
        }

        private void EnableLineRenderer()
        {
            lineRenderer.enabled = true;
            //lineRenderer.startWidth = 0.01f;
            //lineRenderer.endWidth = 0.01f;
        }

        private void LateUpdate()
        {
            _previousHitPoint = RayHitPoint;
            _previousOrigin = RayOrigin;
            
            _result = _rayCast.Raycast(mask);
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (Vector3.Distance(_previousHitPoint, RayHitPoint) < 0.005f &&
                Vector3.Distance(_previousOrigin,   RayOrigin)   < 0.005f)  return;
            
            lineRenderer.SetPosition(0, RayOrigin);
            lineRenderer.SetPosition(1, RayHitPoint);
        }
    }
}