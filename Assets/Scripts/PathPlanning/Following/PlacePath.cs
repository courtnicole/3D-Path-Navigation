namespace PathNav.PathPlanning
{
    using Dreamteck.Splines;
    using Events;
    using ExperimentControl;
    using System;
    using UnityEngine;

    public class PlacePath : MonoBehaviour
    {
        [SerializeField] private SplineComputer targetSpline;

        private SplineComputer _splineComputer;
        private Vector3 _deltaTranslation;
        private float _deltaScale;

        private void Start()
        {
            if (SceneDataManager.Instance != null)
            {
                _splineComputer   = SceneDataManager.Instance.GetSavedSplineComputer();
                _deltaTranslation = SceneDataManager.Instance.persistentData.DeltaTranslation;
                _deltaScale       = SceneDataManager.Instance.persistentData.DeltaScale;

                SetupSpline();
            }
            else
            {
                throw new Exception("SceneDataManager is null!");
            }
        }

        private void EmitEvent(EventId id)
        {
            EventManager.Publish(id, this, GetSceneControlEventArgs());
        }

        private static SceneControlEventArgs GetSceneControlEventArgs() => new();

        private void SetupSpline()
        {
            SplinePoint[] points         = _splineComputer.GetPoints();
            var           newPoints      = new SplinePoint[points.Length];
            var           pointPositions = new Vector3[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                Vector3 newPose = points[i].position;
                newPose           += _deltaTranslation;
                newPose           *= _deltaScale;
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
            }

            targetSpline.SetPoints(newPoints);
            EmitEvent(EventId.FollowPathReady);
        }
    }
}