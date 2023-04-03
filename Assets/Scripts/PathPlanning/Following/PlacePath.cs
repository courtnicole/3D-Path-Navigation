namespace PathNav.PathPlanning
{
    using Dreamteck.Splines;
    using System;
    using UnityEngine;

    public class PlacePath : MonoBehaviour
    {
        [SerializeField] private Vector3 targetPose;
        [SerializeField] private Vector3 targetRotation;
        [SerializeField] private Vector3 targetScale;

        [SerializeField] private SplineComputer targetSpline;
        [SerializeField] private SplineFollower follower;

        private SplineComputer _splineComputer;
        private Vector3 _modelPose;
        private Quaternion _modelRotation;
        private Vector3 _modelScale;
        private Matrix4x4 _pointToWorld;
        private Matrix4x4 _worldToTarget;

        private void Start()
        {
            if (SceneDataManager.Instance != null)
            {
                _splineComputer = SceneDataManager.Instance.GetSavedSplineComputer();
                _modelPose      = SceneDataManager.Instance.persistentData.ModelPose;
                _modelRotation  = Quaternion.Euler(SceneDataManager.Instance.persistentData.ModelRotation);
                _modelScale     = SceneDataManager.Instance.persistentData.ModelScale;
                
                _pointToWorld  = Matrix4x4.TRS(_modelPose, _modelRotation,                   _modelScale);
                _worldToTarget = Matrix4x4.TRS(targetPose, Quaternion.Euler(targetRotation), targetScale);
                
                SetupSpline();
            }
            else
            {
                throw new Exception("SceneDataManager is null!");
            }
        }

        private void SetupSpline()
        {
            SplinePoint[] points         = _splineComputer.GetPoints();
            var newPoints      = new SplinePoint[points.Length];
            var           pointPositions = new Vector3[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                Vector3 newPose = points[i].position;
                newPose.y         -= 0.965f;
                newPose           *= 40;
                pointPositions[i] =  newPose;

                SplinePoint pt = new()
                {
                    color    = Color.white,
                    normal   = Vector3.up, //points[i].normal,
                    size     = 0.01f,
                    tangent  = default, //points[i].tangent,
                    tangent2 = default, //points[i].tangent2,
                    position = pointPositions[i],
                };
                newPoints[i] = pt;
                
                Debug.Log(points[i].position + ", " + pointPositions[i]);
            }

            targetSpline.SetPoints(newPoints);
            follower.follow = true;
        }
    }
}