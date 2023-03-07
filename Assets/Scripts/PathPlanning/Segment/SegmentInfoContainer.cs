namespace PathNav.PathPlanning
{
    using Dreamteck.Splines;
    using UnityEngine;

    public class SegmentInfoContainer : MonoBehaviour
    {
        [SerializeField] private SegmentInfo segmentInfo;
        public SegmentInfo Info => segmentInfo;

        [SerializeField] private SplineComputer spline;
        public SplineComputer Spline => spline;
    }
}
