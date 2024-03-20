namespace PathNav.ExperimentControl
{
    using Dreamteck.Splines;
    using Events;
    using Interaction;
    using SceneManagement;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Animations;
    using UnityEngine.XR.OpenXR.Samples.ControllerSample;
    using Debug = UnityEngine.Debug;

    public class NavigationManager : MonoBehaviour
    {
        #region Variables
        [SerializeField] private List<AudioClipMap> audioMap;
        [SerializeField] private SplineComputer targetSpline;
        [SerializeField] private SplineFollower follower;
        [SerializeField] private SplineProjector projector;

        [SerializeField] private Transform teleportLocation;
        [SerializeField] private Teleporter teleporter;
        [SerializeField] private ParentConstraint parentConstraint;
        [SerializeField] private GameObject discomfortScore;
        [SerializeField] private GameObject seq;
        [SerializeField] private Overlay overlay;
        [SerializeField] private PointerEvaluator pointerLeft;
        [SerializeField] private PointerEvaluator pointerRight;
        [SerializeField] private NavigationEndPoint endPoint;
        [SerializeField] private Transform footVisualMarker;
        
        private SplineComputer _splineComputer;
        private SplinePoint[] _spline;
        private Vector3 _deltaTranslation;
        private float _deltaScale;
        private LocomotionDof _locomotionDof;

        private Stopwatch _taskTimerTotal;
        private bool _recordData;

        private int _userId, _blockId;
        private string _modelId, _methodId;
        #endregion

        #region Enable/Disable/Update
        internal void Enable()
        {
            _taskTimerTotal = new Stopwatch();
            _locomotionDof  = ExperimentDataManager.Instance.GetNavigationMethod();

            StartNavigation();
        }

        private void OnDisable()
        {
            UnsubscribeToEvents();
        }

        private void LateUpdate()
        {
            if (!_recordData) return;
            ExperimentDataLogger.Instance.RecordNavigationData(follower.followSpeed,
                                                               _locomotionDof == LocomotionDof.FourDoF ? follower.result.percent : projector.result.percent,
                                                               _locomotionDof == LocomotionDof.FourDoF ? follower.result.position : projector.result.position);
        }
        #endregion

        #region Event Subscription Management
        private void SubscribeToEvents()
        {
            EventManager.Subscribe<SceneControlEventArgs>(EventId.SeqComplete,             SeqComplete);
            EventManager.Subscribe<SceneControlEventArgs>(EventId.DiscomfortScoreComplete, DiscomfortComplete);
        }

        private void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<SceneControlEventArgs>(EventId.SeqComplete,             SeqComplete);
            EventManager.Unsubscribe<SceneControlEventArgs>(EventId.DiscomfortScoreComplete, DiscomfortComplete);
        }

        private static SceneControlEventArgs GetSceneControlEventArgs() => new();
        #endregion

        #region Event Callbacks
        private void SeqComplete(object sender, SceneControlEventArgs args)
        {
            seq.SetActive(false);
            EndNavigation();
        }

        private void DiscomfortComplete(object sender, SceneControlEventArgs args)
        {
            discomfortScore.SetActive(false);
            seq.SetActive(true);
        }

        public void OnEndReached()
        {
            if (_locomotionDof != LocomotionDof.FourDoF) return;

            NavigationComplete();
        }

        public void OnEndCollision()
        {
            if (_locomotionDof != LocomotionDof.SixDof) return;

            NavigationComplete();
        }

        private void NavigationComplete()
        {
            _taskTimerTotal.Stop();
            EventManager.Publish(EventId.SplineNavigationComplete, this, GetSceneControlEventArgs());
            ActionAssetEnabler actionController = FindObjectOfType<ActionAssetEnabler>();
            actionController.EnableUiInput();
            discomfortScore.SetActive(true);
            pointerLeft.Enable();
            pointerRight.Enable();
        }
        #endregion

        #region Logic
        private async void StartNavigation()
        {
            SetSpline();

            await Task.Delay(50);

            SubscribeToEvents();

            await Task.Delay(10);

            SetupNavigation();

            await Task.Delay(500);

            follower.followSpeed              = 0;
            parentConstraint.constraintActive = _locomotionDof == LocomotionDof.FourDoF;
            follower.follow                   = true; //_locomotionDof == LocomotionDof.FourDoF;

            if (_locomotionDof == LocomotionDof.SixDof)
            {
                pointerLeft.EnableLocomotion();
                pointerRight.EnableLocomotion();
            }

            overlay.FadeToClear();

            await Task.Delay(1000);

            _taskTimerTotal.Start();
            _recordData = true;
        }

        private async void EndNavigation()
        {
            await Task.Delay(50);

            overlay.FadeToBlack();

            await Task.Delay(500);
            
            ExperimentDataManager.Instance.NavigationComplete();
        }

        private void SetSpline()
        {
            if (ExperimentDataManager.Instance != null)
            {
                _spline           = ExperimentDataManager.Instance.GetSavedSpline();
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
            SplinePoint[] points         = _spline;
            var           newPoints      = new SplinePoint[points.Length];
            var           pointPositions = new Vector3[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                Vector3 newPose = points[i].position;
                newPose           += _deltaTranslation;
                newPose           /= _deltaScale;
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
        
        public void StopImmediately()
        {
            _recordData = false;
            overlay.FadeToBlackImmediate();
            _taskTimerTotal.Stop();

            EventManager.Publish(EventId.SplineNavigationComplete, this, GetSceneControlEventArgs());
            UnsubscribeToEvents();

            ExperimentDataLogger.Instance.RecordSurveyData("Discomfort", "10");
            ExperimentDataManager.Instance.EndExperimentImmediately();
        }

        private bool CheckTeleportation()
        {
            if (teleportLocation is not null)
            {
                if (teleporter is null)
                {
                    teleporter = FindObjectOfType<Teleporter>();

                    if (teleporter is null)
                    {
                        Debug.LogError("Teleporter not found in scene!");
                    }
                }
            }

            return (teleporter is not null) && (teleportLocation is not null);
        }

        private void SetupNavigation()
        {
            if (!CheckTeleportation()) return;

            float height = ExperimentDataManager.Instance.GetHeight();
            footVisualMarker.localPosition =  new Vector3(0, -height, 0);
            height                         *= 0.5f;
            SplineSample sample = follower.spline.Evaluate(0);
            teleportLocation.position = sample.position + new Vector3(0, height, 0);
            teleportLocation.forward  = sample.forward;
            teleporter.Teleport(teleportLocation);

            sample = follower.spline.Evaluate(follower.spline.pointCount - 1);
            endPoint.Place(sample.position);
        }
        #endregion
    }
}