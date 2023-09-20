namespace PathNav.SceneManagement
{
    using Events;
    using Interaction;
    using PathPlanning;
    using System;
    using System.Diagnostics;
    using UnityEngine;
    public class Timing : MonoBehaviour
    {
        private Stopwatch _taskTimer;
        private Stopwatch _editTimer;

        public void Enable()
        {
            _taskTimer = new Stopwatch();
            _editTimer = new Stopwatch();
            SubscribeToEvents();
        }
        
        public void Disable()
        {
            UnsubscribeFromEvents();
        }
        
        private void SubscribeToEvents()
        {
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.BeginPlacingStartPoint, TaskStarted);
            
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.EraseStarted, EditStarted);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.MoveStarted,  EditStarted);
            
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.EraseEnded, EditStopped);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.MoveEnded,  EditStopped);
            
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.PathCreationComplete, TaskFinished);
        }

        private void UnsubscribeFromEvents()
        {
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.BeginPlacingStartPoint, TaskStarted);
            
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.EraseStarted, EditStarted);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.MoveStarted,  EditStarted);
            
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.EraseEnded, EditStopped);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.MoveEnded,  EditStopped);
            
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.PathCreationComplete, TaskFinished);
        }
        
        private void TaskStarted(object sender, ControllerEvaluatorEventArgs args)
        {
            StartTaskTimer();
        }

        private void TaskFinished(object sender, ControllerEvaluatorEventArgs args)
        {
            StopTaskTimer();
        }

        private void EditStarted(object sender, PathStrategyEventArgs args)
        {
            StartEditTimer();
        }
        
        private void EditStopped(object sender, PathStrategyEventArgs args)
        {
            StopEditTimer();
        }
        
        private void Reset()
        {
            _taskTimer.Reset();
            _editTimer.Reset();
        }
        
        private void StartTaskTimer()
        {
            _taskTimer.Start();
        }
        
        private void StopTaskTimer()
        {
            _taskTimer.Stop();
        }
        private void StartEditTimer()
        {
            _editTimer.Start();
        }
        private void StopEditTimer()
        {
            _editTimer.Stop();
        }

        public TimeSpan GetTaskTime() => _taskTimer.Elapsed;
        public TimeSpan GetEditTime() => _editTimer.Elapsed;
    }
}
