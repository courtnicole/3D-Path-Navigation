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

    public class NavigationManager : MonoBehaviour
    {
        [SerializeField] private List<AudioClipMap> audioMap;
        [SerializeField] private SplineComputer targetSpline;
        [SerializeField] private SplineFollower follower;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private GameObject discomfortScore;
        [SerializeField] private GameObject seq;
        [SerializeField] private Overlay overlay;
        [SerializeField] private PointerEvaluator pointerLeft;
        [SerializeField] private PointerEvaluator pointerRight;

        private SplineComputer _splineComputer;
        private Vector3 _deltaTranslation;
        private float _deltaScale;

        private Stopwatch _taskTimerTotal;
        private NavigationDataFormat _navigationData;
        private static string _logFile;
        private static readonly CsvConfiguration Config = new(CultureInfo.InvariantCulture);
        private bool _recordData;

        #region Enable/Disable/Update
        internal void Enable()
        {
            _taskTimerTotal = new Stopwatch();
            _taskTimerTotal.Start();

            _recordData = true;
            
            InitializeDataLogging();
            SetSpline();
            SubscribeToEvents();
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
            _taskTimerTotal.Stop();
            EventManager.Publish(EventId.SplineNavigationComplete, this, GetSceneControlEventArgs());
            ExperimentDataManager.Instance.RecordNavigationData(_taskTimerTotal.Elapsed);
            discomfortScore.SetActive(true);
            pointerLeft.Enable();
            pointerRight.Enable();
        }
        #endregion

        #region Logic
        private async void StartNavigation()
        {
            await Task.Delay(600);

            overlay.FadeToClear();
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
                _splineComputer   = ExperimentDataManager.Instance.GetSavedSplineComputer();
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

        public void StopImmediately()
        {
            
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

        private static void InitDataLog(string logDirectory, string filePath)
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            _logFile = filePath;
            if (File.Exists(_logFile)) return;

            using StreamWriter streamWriter = new(_logFile);
            using CsvWriter    csvWriter    = new(streamWriter, Config);
            csvWriter.Context.RegisterClassMap<NavigationDataFormatMap>();
            csvWriter.WriteHeader<NavigationDataFormat>();
            csvWriter.NextRecord();
        }

        private void RecordData()
        {
            _navigationData.SPEED           = follower.followSpeed;
            _navigationData.SPLINE_POSITION = follower.result.position.ToString("F3");
            _navigationData.SPLINE_PERCENT  = follower.result.percent;
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