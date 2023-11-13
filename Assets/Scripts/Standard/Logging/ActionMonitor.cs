namespace PathNav.SceneManagement
{
    using DataLogging;
    using Events;
    using ExperimentControl;
    using Interaction;
    using PathPlanning;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using UnityEngine;

    public class ActionMonitor : MonoBehaviour
    {
        private int _totalActions;
        private int _editActions;
        private Stopwatch _taskTimer;
        private Stopwatch _actionTimer;
        private Stopwatch _editTimer;

        private int    UserId { get; set; }
        private string Scene  { get; set; }
        
        private string Block { get; set; }

        #region Logic
        public void Enable(int id, string activeScene, string block)
        {
            _taskTimer   = new Stopwatch();
            _editTimer   = new Stopwatch();
            _editActions = 0;
            UserId       = id;
            Scene        = activeScene;
            Block        = block;
            SubscribeToEvents();
        }

        public void Disable()
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
            _actionTimer.Start();
        }

        private void StopActionTimer()
        {
            _actionTimer.Stop();
        }

        private void StartEditTimer()
        {
            _editTimer.Start();
            _actionTimer.Start();
        }

        private void StopEditTimer()
        {
            _editTimer.Stop();
            _actionTimer.Stop();
        }
        public void RecordAction(string action, DateTime timestamp)
        {
            FileWriterInterface.RecordData(nameof(UserId),    UserId.ToString());
            FileWriterInterface.RecordData(nameof(Block),     Block);
            FileWriterInterface.RecordData(nameof(Scene),     Scene);
            FileWriterInterface.RecordData(nameof(action),    action);
            FileWriterInterface.RecordData(nameof(timestamp), timestamp.ToString(CultureInfo.InvariantCulture));

            FileWriterInterface.WriteRecordedData();
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
            _taskTimer.Start();
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
            _taskTimer.Stop();
            RecordAction("FinishPath", DateTime.Now);
        }
        #endregion

        public int                      GetEditActionCount()  => _editActions;
        public int                      GetTotalActionCount() => _totalActions;
        public TimeSpan                 GetTaskTime()         => _taskTimer.Elapsed;
        public TimeSpan                 GetActionTime()       => _actionTimer.Elapsed;
        public TimeSpan                 GetEditTime()         => _editTimer.Elapsed;
    }
}