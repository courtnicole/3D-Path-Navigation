namespace PathNav.ExperimentControl
{
    using Events;
    using Interaction;
    using PathPlanning;
    using System;
    using UnityEngine;

    public class CreationActionMonitor : MonoBehaviour
    {
        private bool _actionInProgress;
        private string _currentAction;
        private string _actionType;
        private long _startTime;
        private long _totalTime;
        #region Logic
        public void Enable()
        {
            SubscribeToEvents();
        }
        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        #endregion

        #region Event Subscription Management
        private void SubscribeToEvents()
        {
            EventManager.Subscribe<SceneControlEventArgs>(EventId.RegisterStartPoint, RegisterStartPoint);

            EventManager.Subscribe<PathStrategyEventArgs>(EventId.DrawStarted, DrawStarted);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.DrawEnded, DrawEnded);

            EventManager.Subscribe<PathStrategyEventArgs>(EventId.PointPlaced, PointPlaced);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.PointDeleted, PointDeleted);

            EventManager.Subscribe<PathStrategyEventArgs>(EventId.EraseToggleOn, EraseStarted);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.EraseToggleOff, EraseEnded);

            EventManager.Subscribe<PathStrategyEventArgs>(EventId.MoveStarted, MoveStarted);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.MoveEnded, MoveEnded);

            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.PathCreationComplete, FinishPath);
        }

        private void UnsubscribeFromEvents()
        {
            EventManager.Unsubscribe<SceneControlEventArgs>(EventId.RegisterStartPoint, RegisterStartPoint);

            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.DrawStarted, DrawStarted);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.DrawEnded, DrawEnded);

            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.PointPlaced, PointPlaced);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.PointDeleted, PointDeleted);

            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.EraseToggleOn, EraseStarted);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.EraseToggleOff, EraseEnded);

            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.MoveStarted, MoveStarted);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.MoveEnded, MoveEnded);

            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.PathCreationComplete, FinishPath);
        }
        #endregion

        #region Event Callbacks
        private void EraseStarted(object sender, PathStrategyEventArgs args)
        {
            _currentAction = "Erase";
            _actionType = "Edit";
            _startTime = DateTime.Now.Millisecond;
            _actionInProgress = true;
        }

        private void EraseEnded(object sender, PathStrategyEventArgs args)
        {
            if (!_actionInProgress) return;
            _actionInProgress = false;
            _totalTime = DateTime.Now.Millisecond - _startTime;
            ExperimentDataLogger.Instance.RecordCreationData(_actionType, _currentAction, _totalTime.ToString());
        }

        private void MoveStarted(object sender, PathStrategyEventArgs args)
        {
            _currentAction = "Move";
            _actionType = "Edit";
            _startTime = DateTime.Now.Millisecond;
            _actionInProgress = true;
        }

        private void MoveEnded(object sender, PathStrategyEventArgs args)
        {
            if (!_actionInProgress) return;
            _actionInProgress = false;
            _totalTime = DateTime.Now.Millisecond - _startTime;
            ExperimentDataLogger.Instance.RecordCreationData(_actionType, _currentAction, _totalTime.ToString());
        }

        private void DrawStarted(object sender, PathStrategyEventArgs args)
        {
            _actionInProgress = true;
            _currentAction = "Draw";
            _actionType = "Create";
            _startTime = DateTime.Now.Millisecond;
        }

        private void DrawEnded(object sender, PathStrategyEventArgs args)
        {
            if (!_actionInProgress) return;
            _actionInProgress = false;
            _totalTime = DateTime.Now.Millisecond - _startTime;
            ExperimentDataLogger.Instance.RecordCreationData(_actionType, _currentAction, _totalTime.ToString());
        }

        private void PointPlaced(object sender, PathStrategyEventArgs args)
        {
            _currentAction = "PointPlaced";
            _actionType = "Create";
            ExperimentDataLogger.Instance.RecordCreationData(_actionType, _currentAction, "N/A");
        }

        private void PointDeleted(object sender, PathStrategyEventArgs args)
        {
            _currentAction = "PointDeleted";
            _actionType = "Edit";
            ExperimentDataLogger.Instance.RecordCreationData(_actionType, _currentAction, "N/A");
        }

        private void RegisterStartPoint(object sender, SceneControlEventArgs args)
        {
            ExperimentDataLogger.Instance.RecordCreationData("StartPointRegistered", "Start", "N/A");
        }

        private void FinishPath(object sender, ControllerEvaluatorEventArgs args)
        {
            ExperimentDataLogger.Instance.RecordCreationData("FinishPath", "End", "N/A");
        }
        #endregion
    }
}