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

        private Vector3 RayHitPoint => _hitResult.point;
        #endregion

        private void Awake()
        {
            _rayCast = this;
        }

        private void LateUpdate()
        {
            _result = _rayCast.Raycast();
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            lineRenderer.SetPosition(0, LineOrigin);
            lineRenderer.SetPosition(1, RayHitPoint);
        }
    }
}