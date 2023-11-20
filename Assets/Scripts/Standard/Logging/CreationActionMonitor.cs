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
        public void Enable(int id, string model, int block, string method, string logDirectory, string logFilePath)
        {
            _taskTimerTotal   = new Stopwatch();
            _taskTimerCreate   = new Stopwatch();
            _editActions = 0;
            
            InitDataLog(logDirectory, logFilePath);
            
            _creationData          = new CreationDataFormat
            {
                ID       = id,
                BLOCK_ID = block,
                MODEL    = model,
                METHOD   = method,
            };
            
            SubscribeToEvents();
        }

        private static void InitDataLog(string logDirectory, string filePath)
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            _logFile = filePath;
            if (File.Exists(_logFile)) return;
            using StreamWriter    streamWriter = new (_logFile);
            using CsvWriter csvWriter    = new (streamWriter, Config);
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

        private void StartActionTimer()
        {
            _taskTimerEdits.Start();
        }

        private void StopActionTimer()
        {
            _taskTimerEdits.Stop();
        }

        private void StartEditTimer()
        {
            _taskTimerCreate.Start();
            _taskTimerEdits.Start();
        }

        private void StopEditTimer()
        {
            _taskTimerCreate.Stop();
            _taskTimerEdits.Stop();
        }
        public void RecordAction(string action, DateTime timestamp)
        {
            _creationData.ACTION    = action;
            _creationData.TIMESTAMP = timestamp;
            using StreamWriter streamWriter = new (_logFile, true);
            using CsvWriter    csvWriter    = new (streamWriter, Config);
            csvWriter.Context.RegisterClassMap<SceneDataFormatMap>();
            csvWriter.WriteRecord(_creationData);
            csvWriter.NextRecord();
        }

        public void RecordAllActions()
        {
            ExperimentDataManager.Instance.RecordActionData(_totalActions, _editActions, _taskTimerTotal.Elapsed, _taskTimerEdits.Elapsed, _taskTimerCreate.Elapsed);
        }
        
        #endregion

        #region Event Subscription Management
        private void SubscribeToEvents()
        {
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.BeginPlacingStartPoint,  BeginPlacingStartPoint);
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.FinishPlacingStartPoint, FinishPlacingStartPoint);

            EventManager.Subscribe<PathStrategyEventArgs>(EventId.DrawStarted, DrawStarted);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.DrawEnded,   DrawEnded);

            EventManager.Subscribe<PathStrategyEventArgs>(EventId.PointPlaced, PointPlaced);

            EventManager.Subscribe<PathStrategyEventArgs>(EventId.EraseStarted, EraseStarted);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.MoveStarted,  MoveStarted);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.EraseEnded,   EraseEnded);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.MoveEnded,    MoveEnded);

            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.PathCreationComplete, FinishPath);
        }

        private void UnsubscribeFromEvents()
        {
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.BeginPlacingStartPoint,  BeginPlacingStartPoint);
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.FinishPlacingStartPoint, FinishPlacingStartPoint);

            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.DrawStarted, DrawStarted);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.DrawEnded,   DrawEnded);

            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.PointPlaced, PointPlaced);

            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.EraseStarted, EraseStarted);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.MoveStarted,  MoveStarted);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.EraseEnded,   EraseEnded);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.MoveEnded,    MoveEnded);

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
            StartActionTimer();
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

        private void BeginPlacingStartPoint(object sender, ControllerEvaluatorEventArgs args)
        {
            _taskTimerTotal.Start();
            StartActionTimer();
            IncrementTotalActions();
            RecordAction("BeginStartPointPlacement", DateTime.Now);
        }

        private void FinishPlacingStartPoint(object sender, ControllerEvaluatorEventArgs args)
        {
            StopActionTimer();
            RecordAction("FinishStartPointPlacement", DateTime.Now);
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