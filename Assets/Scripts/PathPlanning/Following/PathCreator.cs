namespace PathNav
{
    using Dreamteck.Splines;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PathCreator : MonoBehaviour
    {
        [SerializeField] private SplineComputer spline;

        private Vector3 _modelPose;
        private Quaternion _modelRotation;
        private Vector3 _modelScale;

        private void GetSpline()
        {
            if (SceneDataManager.Instance == null) return;

            SplineComputer newSpline = SceneDataManager.Instance.GetSavedSplineComputer();
            _modelPose     = SceneDataManager.Instance.persistentData.ModelPose;
            _modelRotation = SceneDataManager.Instance.persistentData.ModelRotation;
            _modelScale    = SceneDataManager.Instance.persistentData.ModelScale;

            SplinePoint[] points = spline.GetPoints();
        }
        
        //TODO: Verify localToWorld splinePoint transformation
        private void TransformSplinePoints(SplinePoint[] points) { }
    }
}