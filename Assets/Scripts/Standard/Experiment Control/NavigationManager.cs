namespace PathNav.ExperimentControl
{
    using CsvHelper;
    using CsvHelper.Configuration;
    using Dreamteck.Splines;
    using Events;
    using Interaction;
    using SceneManagement;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Animations;
    using UnityEngine.XR.OpenXR.Samples.ControllerSample;
    using Debug = UnityEngine.Debug;

    public class NavigationManager : MonoBehaviour
    {
        [SerializeField] private List<AudioClipMap> audioMap;
        [SerializeField] private SplineComputer targetSpline;
        [SerializeField] private SplineFollower follower;
        [SerializeField] private SplineProjector projector;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform teleportLocation;
        [SerializeField] private Teleporter teleporter;
        [SerializeField] private ParentConstraint parentConstraint;
        [SerializeField] private GameObject discomfortScore;
        [SerializeField] private GameObject seq;
        [SerializeField] private Overlay overlay;
        [SerializeField] private PointerEvaluator pointerLeft;
        [SerializeField] private PointerEvaluator pointerRight;
        [SerializeField] private NavigationEndPoint endPoint;

        private SplineComputer _splineComputer;
        private SplinePoint[] _spline;
        private Vector3 _deltaTranslation;
        private float _deltaScale;
        private LocomotionDof _locomotionDof;

        private Stopwatch _taskTimerTotal;
        private NavigationDataFormat _navigationData;
        private static string _logFile;
        private static readonly CsvConfiguration Config = new(CultureInfo.InvariantCulture);
        private bool _recordData;
        
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
            RecordData();
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
            ExperimentDataManager.Instance.RecordNavigationData(_taskTimerTotal.Elapsed);
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
            InitializeDataLogging();
            await Task.Delay(10);
            
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
            await Task.Delay(100);

            overlay.FadeToBlack();

            await Task.Delay(1500);

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
            
            ExperimentDataManager.Instance.RecordNavigationData(_taskTimerTotal.Elapsed);
            ExperimentDataManager.Instance.RecordDiscomfortScore(10);
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

            SplineSample sample = follower.spline.Evaluate(0);
            teleportLocation.position = sample.position + new Vector3(0, 1.5f, 0);
            teleportLocation.forward  = sample.forward;
            teleporter.Teleport(teleportLocation);
            
            sample = follower.spline.Evaluate(follower.spline.pointCount - 1);
            endPoint.Place(sample.position);
        }

        #endregion

        #region Data Logging
        private void InitializeDataLogging()
        {
            InitDataLog(ExperimentDataManager.Instance.GetLogDirectory(), ExperimentDataManager.Instance.GetNavigationLogFilePath());

            _navigationData = new NavigationDataFormat
            {
                ID       = ExperimentDataManager.Instance.GetId(),
                BLOCK_ID = ExperimentDataManager.Instance.GetBlock(),
                MODEL    = ExperimentDataManager.Instance.GetModel(),
                METHOD   = ExperimentDataManager.Instance.GetNavigationMethodString(),
            };
        }

        private void InitDataLog(string logDirectory, string filePath)
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            _logFile = filePath;

            if (File.Exists(_logFile))
            {
                return;
            }

            using StreamWriter streamWriter = new(_logFile);
            using CsvWriter    csvWriter    = new(streamWriter, Config);
            csvWriter.Context.RegisterClassMap<NavigationDataFormatMap>();
            csvWriter.WriteHeader<NavigationDataFormat>();
            csvWriter.NextRecord();
        }

        private void RecordData()
        {
            _navigationData.SPEED           = follower.followSpeed;
            _navigationData.SPLINE_POSITION = _locomotionDof == LocomotionDof.FourDoF ? follower.result.position.ToString("F3") : projector.result.position.ToString("F3");
            _navigationData.SPLINE_PERCENT  = _locomotionDof == LocomotionDof.FourDoF ? follower.result.percent : projector.result.percent;
            _navigationData.POSITION        = playerTransform.position.ToString("F3");
            _navigationData.ROTATION        = playerTransform.rotation.ToString("F3");
            _navigationData.TIMESTAMP       = DateTime.Now;

            using StreamWriter streamWriter = new(_logFile, true);
            using CsvWriter    csvWriter    = new(streamWriter, Config);
            csvWriter.Context.RegisterClassMap<SceneDataFormatMap>();
            csvWriter.WriteRecord(_navigationData);
            csvWriter.NextRecord();
        }
        #endregion
    }
}