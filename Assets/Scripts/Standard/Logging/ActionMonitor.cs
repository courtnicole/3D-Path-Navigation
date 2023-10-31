namespace PathNav.SceneManagement
{
    using Events;
    using Interaction;
    using PathPlanning;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using UnityEngine;

    public class ActionMonitor : MonoBehaviour
    {
        private int _totalActions;
        private int _editActions;
        private Stopwatch _taskTimer;
        private Stopwatch _actionTimer;
        private Stopwatch _editTimer;
        private List<(string, DateTime)> _actionList;

        #region Logic
        public void Enable()
        {
            _taskTimer   = new Stopwatch();
            _editTimer   = new Stopwatch();
            _editActions = 0;
            _actionList  = new List<(string, DateTime)>();
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
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.EraseStarted, EraseStarted);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.MoveStarted,  MoveStarted);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.EraseEnded,   EraseEnded);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.MoveEnded,    MoveEnded);
        }
        #endregion

        #region Event Callbacks
        // Increment Action counts on Event start to avoid "losing" an action if there is a timeout/cutout. 
        // Do not count "Finishing path" as an action
        private void EraseStarted(object sender, PathStrategyEventArgs args)
        {
            StartEditTimer();
            IncrementEditActions();
            _actionList.Add(("EraseStart", DateTime.Now));
        }

        private void EraseEnded(object sender, PathStrategyEventArgs args)
        {
            StopEditTimer();
            _actionList.Add(("EraseEnd", DateTime.Now));
        }

        private void MoveStarted(object sender, PathStrategyEventArgs args)
        {
            StartEditTimer();
            IncrementEditActions();
            _actionList.Add(("MoveStart", DateTime.Now));
        }

        private void MoveEnded(object sender, PathStrategyEventArgs args)
        {
            StopEditTimer();
            _actionList.Add(("MoveEnd", DateTime.Now));
        }

        private void DrawStarted(object sender, PathStrategyEventArgs args)
        {
            StartActionTimer();
            IncrementTotalActions();
            _actionList.Add(("DrawStart", DateTime.Now));
        }

        private void DrawEnded(object sender, PathStrategyEventArgs args)
        {
            _actionList.Add(("DrawEnd", DateTime.Now));
        }

        private void PointPlaced(object sender, PathStrategyEventArgs args)
        {
            IncrementTotalActions();
            _actionList.Add(("PointPlaced", DateTime.Now));
        }

        private void BeginPlacingStartPoint(object sender, ControllerEvaluatorEventArgs args)
        {
            _taskTimer.Start();
            StartActionTimer();
            IncrementTotalActions();
            _actionList.Add(("BeginStartPointPlacement", DateTime.Now));
        }

        private void FinishPlacingStartPoint(object sender, ControllerEvaluatorEventArgs args)
        {
            StopActionTimer();
            _actionList.Add(("FinishStartPointPlacement", DateTime.Now));
        }

        private void FinishPath(object sender, ControllerEvaluatorEventArgs args)
        {
            _taskTimer.Stop();
            _actionList.Add(("FinishPath", DateTime.Now));
        }
        #endregion

        public int                      GetEditActionCount()  => _editActions;
        public int                      GetTotalActionCount() => _totalActions;
        public List<(string, DateTime)> GetEditInfo()         => _actionList;
        public TimeSpan                 GetTaskTime()         => _taskTimer.Elapsed;
        public TimeSpan                 GetActionTime()       => _actionTimer.Elapsed;
        public TimeSpan                 GetEditTime()         => _editTimer.Elapsed;
    }
}