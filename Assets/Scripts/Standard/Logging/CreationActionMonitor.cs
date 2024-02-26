namespace PathNav.SceneManagement
{
    using CsvHelper;
    using CsvHelper.Configuration;
    using Events;
    using ExperimentControl;
    using Interaction;
    using PathPlanning;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using UnityEngine;

    public class CreationActionMonitor : MonoBehaviour
    {
        private int _totalActions;
        private int _editActions;
        private Stopwatch _taskTimerTotal;
        private Stopwatch _taskTimerEdits;
        private Stopwatch _taskTimerCreate;

        private CreationDataFormat _creationData;
        private static string _logFile;
        private static readonly CsvConfiguration Config = new(CultureInfo.InvariantCulture);

        #region Logic
        public void Enable()
        {
            _taskTimerTotal  = new Stopwatch();
            _taskTimerEdits  = new Stopwatch();
            _taskTimerCreate = new Stopwatch();
            _totalActions    = 0;
            _editActions     = 0;

            InitializeDataLogging();
            SubscribeToEvents();
        }

        private void InitializeDataLogging()
        {
            InitDataLog(ExperimentDataManager.Instance.GetLogDirectory(), ExperimentDataManager.Instance.GetActionLogFilePath());

            _creationData = new CreationDataFormat
            {
                ID       = ExperimentDataManager.Instance.GetId(),
                BLOCK_ID = ExperimentDataManager.Instance.GetBlock(),
                MODEL    = ExperimentDataManager.Instance.GetModel(),
                METHOD   = ExperimentDataManager.Instance.GetCreationMethodString(),
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
            csvWriter.Context.RegisterClassMap<CreationDataFormatMap>();
            csvWriter.WriteHeader<CreationDataFormat>();
            csvWriter.NextRecord();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void IncrementEditActions()
        {
            _editActions++;
            _totalActions++;
        }

        private void IncrementTotalActions()
        {
            _totalActions++;
        }

        private void StartCreateTimer()
        {
            _taskTimerCreate.Start();
        }

        private void StopCreateTimer()
        {
            _taskTimerCreate.Stop();
        }

        private void StartEditTimer()
        {
            _taskTimerCreate.Stop();
            _taskTimerEdits.Start();
        }

        private void StopEditTimer()
        {
            _taskTimerEdits.Stop();
            _taskTimerCreate.Start();
        }

        public void RecordAction(string action, DateTime timestamp)
        {
            _creationData.ACTION    = action;
            _creationData.TIMESTAMP = timestamp;
            using StreamWriter streamWriter = new(_logFile, true);
            using CsvWriter    csvWriter    = new(streamWriter, Config);
            csvWriter.Context.RegisterClassMap<SceneDataFormatMap>();
            csvWriter.WriteRecord(_creationData);
            csvWriter.NextRecord();
        }

        public void RecordAllActions()
        {
            ExperimentDataManager.Instance.RecordActionData(_totalActions,
                                                            _editActions,
                                                            _taskTimerTotal.Elapsed,
                                                            _taskTimerEdits.Elapsed,
                                                            _taskTimerCreate.Elapsed);
        }
        #endregion

        #region Event Subscription Management
        private void SubscribeToEvents()
        {
            EventManager.Subscribe<SceneControlEventArgs>(EventId.RegisterStartPoint,  RegisterStartPoint);

            EventManager.Subscribe<PathStrategyEventArgs>(EventId.DrawStarted, DrawStarted);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.DrawEnded,   DrawEnded);

            EventManager.Subscribe<PathStrategyEventArgs>(EventId.PointPlaced,  PointPlaced);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.PointDeleted, PointDeleted);

            EventManager.Subscribe<PathStrategyEventArgs>(EventId.EraseToggleOn, EraseStarted);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.EraseToggleOff,   EraseEnded);

            EventManager.Subscribe<PathStrategyEventArgs>(EventId.MoveStarted, MoveStarted);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.MoveEnded,   MoveEnded);

            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.PathCreationComplete, FinishPath);
        }

        private void UnsubscribeFromEvents()
        {
            EventManager.Unsubscribe<SceneControlEventArgs>(EventId.RegisterStartPoint,  RegisterStartPoint);

            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.DrawStarted, DrawStarted);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.DrawEnded,   DrawEnded);

            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.PointPlaced,  PointPlaced);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.PointDeleted, PointDeleted);

            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.EraseToggleOn, EraseStarted);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.EraseToggleOff,   EraseEnded);

            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.MoveStarted, MoveStarted);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.MoveEnded,   MoveEnded);

            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.PathCreationComplete, FinishPath);
        }
        #endregion

        #region Event Callbacks
        // Increment Action counts on Event start to avoid "losing" an action if there is a timeout/cutout. 
        // Do not count "Finishing path" as an action
        private void EraseStarted(object sender, PathStrategyEventArgs args)
        {
            StartEditTimer();
            IncrementEditActions();
            RecordAction("EraseStart", DateTime.Now);
        }

        private void EraseEnded(object sender, PathStrategyEventArgs args)
        {
            StopEditTimer();
            RecordAction("EraseEnd", DateTime.Now);
        }

        private void MoveStarted(object sender, PathStrategyEventArgs args)
        {
            StartEditTimer();
            IncrementEditActions();
            RecordAction("MoveStart", DateTime.Now);
        }

        private void MoveEnded(object sender, PathStrategyEventArgs args)
        {
            StopEditTimer();
            RecordAction("MoveEnd", DateTime.Now);
        }

        private void DrawStarted(object sender, PathStrategyEventArgs args)
        {
            StartCreateTimer();
            IncrementTotalActions();
            RecordAction("DrawStart", DateTime.Now);
        }

        private void DrawEnded(object sender, PathStrategyEventArgs args)
        {
            RecordAction("DrawEnd", DateTime.Now);
        }

        private void PointPlaced(object sender, PathStrategyEventArgs args)
        {
            IncrementTotalActions();
            RecordAction("PointPlaced", DateTime.Now);
        }

        private void PointDeleted(object sender, PathStrategyEventArgs args)
        {
            IncrementTotalActions();
            RecordAction("PointDeleted", DateTime.Now);
        }

        private void RegisterStartPoint(object sender, SceneControlEventArgs args)
        {
            _taskTimerTotal.Start();
            StartCreateTimer();
            RecordAction("StartPointRegistered", DateTime.Now);
        }

        private void FinishPath(object sender, ControllerEvaluatorEventArgs args)
        {
            _taskTimerTotal.Stop();
            RecordAction("FinishPath", DateTime.Now);
            RecordAllActions();
        }
        #endregion
    }
}