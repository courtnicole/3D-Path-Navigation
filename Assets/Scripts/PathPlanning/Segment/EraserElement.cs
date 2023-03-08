namespace PathNav.PathPlanning
{
    using UnityEngine;

    public class EraserElement : MonoBehaviour, IEraser
    {
        [SerializeField] private Collider collider;
        private Bounds Bounds => collider.bounds;

        #region Implementation of IEraser
        public bool IsPointInside(Vector3 point) => Bounds.Contains(point);
        #endregion
    }
}