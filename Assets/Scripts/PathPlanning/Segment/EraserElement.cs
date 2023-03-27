namespace PathNav.PathPlanning
{
    using UnityEngine;

    public class EraserElement : MonoBehaviour, IEraser
    {
        [SerializeField] private Collider controllerCollider;
        private Bounds Bounds => controllerCollider.bounds;

        #region Implementation of IEraser
        public bool IsPointInside(Vector3 point) => Bounds.Contains(point);
        #endregion
    }
}