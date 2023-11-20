namespace PathNav.ExperimentControl
{
    using Dreamteck.Splines;
    using Events;
    using Interaction;
    using SceneManagement;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class NavigationManager : MonoBehaviour
    {
        [SerializeField] private List<AudioClipMap> audioMap;
        [SerializeField] private SplineComputer targetSpline;
        [SerializeField] private GameObject discomfortScore;
        [SerializeField] private GameObject seq;
        
        private SplineComputer _splineComputer;
        private Vector3 _deltaTranslation;
        private float _deltaScale;
        
        
        private Trial _trial;
        private SceneDataFormat _sceneData;
        
        internal void Enable(Trial trial, SceneDataFormat sceneData)
        {
            _trial               = trial;
            _sceneData           = sceneData;

            SetSpline();
        }

        public void OnEndReached()
        {
            discomfortScore.SetActive(true);
        }

        private void SetSpline()
        {
            if (ExperimentDataManager.Instance != null)
            {
                _splineComputer = ExperimentDataManager.Instance.GetSavedSplineComputer();
                
                _deltaTranslation = ExperimentDataManager.Instance.GetSplineModel().Translation;
                _deltaScale       = ExperimentDataManager.Instance.GetSplineModel().Scale;

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
            EventManager.Publish(EventId.FollowPathReady, this, GetSceneControlEventArgs());
        }
        private static SceneControlEventArgs GetSceneControlEventArgs() => new();
    }
}
