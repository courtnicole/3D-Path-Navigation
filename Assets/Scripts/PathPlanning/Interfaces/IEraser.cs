namespace PathNav.PathPlanning
{
    using UnityEngine;

    public interface IEraser
    {
        public bool IsPointInside(Vector3 point);
    }
}