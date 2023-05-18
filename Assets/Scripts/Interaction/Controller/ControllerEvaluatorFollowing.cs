namespace PathNav.Interaction
{
    using Events;
    using Input;
    using System;
    using UnityEngine;
    public class ControllerEvaluatorFollowing : MonoBehaviour
    {
        
        #region Controller Variables
        internal IController[] Controllers => ControllerManagement.controllers;
        #endregion
        
        #region Unity Methods
        private void OnEnable()
        {
            Enable();
        }
        private void OnDisable()
        {
            Disable();
        }
        #endregion
        
        #region Initialization
        private void Enable()
        {
            SubscribeToEvents();
        }

        private void Disable()
        {
            UnsubscribeToEvents();
        } 
        #endregion
        
        #region Manage Event Subscriptions
        private void SubscribeToEvents()
        {
            EventManager.Subscribe<ControllerEventArgs>(EventId.JoystickTouchStart, EvaluateJoystickTouchStart);
            //EventManager.Subscribe<ControllerEventArgs>(EventId.JoystickPoseUpdate, EvaluateJoystickPoseUpdate);
            EventManager.Subscribe<ControllerEventArgs>(EventId.JoystickTouchEnd,   EvaluateJoystickTouchEnd);
        }

        private void UnsubscribeToEvents()
        {
            EventManager.Subscribe<ControllerEventArgs>(EventId.JoystickTouchStart, EvaluateJoystickTouchStart);
            //EventManager.Subscribe<ControllerEventArgs>(EventId.JoystickPoseUpdate, EvaluateJoystickPoseUpdate);
            EventManager.Subscribe<ControllerEventArgs>(EventId.JoystickTouchEnd,   EvaluateJoystickTouchEnd);
        }
        #endregion
        
        private void EvaluateJoystickTouchStart(object obj, ControllerEventArgs args)
        {
            Debug.Log("EvaluateJoystickTouchStart");
            OnControllerEvaluatorFollowerEvent(EventId.StartSpeedUpdate, GetFollowerEvaluatorEventArgs(args.Controller));
        }
        
        private void EvaluateJoystickTouchEnd(object obj, ControllerEventArgs args)
        {
            OnControllerEvaluatorFollowerEvent(EventId.EndSpeedUpdate, GetFollowerEvaluatorEventArgs(args.Controller));
        }
        
        private void EvaluateJoystickPoseUpdate(object obj, ControllerEventArgs args)
        {
            OnControllerEvaluatorFollowerEvent(EventId.ChangeSpeed, GetFollowerEvaluatorEventArgs(args.Controller));
        }
        

        #region Emit Events
        private void OnControllerEvaluatorFollowerEvent(EventId id, FollowerEvaluatorEventArgs args)
        {
            EventManager.Publish(id, this, args);
        }

        private static FollowerEvaluatorEventArgs GetFollowerEvaluatorEventArgs(IController controller) => new(controller);
        #endregion
    }
    
    public class FollowerEvaluatorEventArgs : EventArgs
    {
        public FollowerEvaluatorEventArgs(IController c) => Controller = c;

        public IController Controller { get; }
        public int Sign => Controller.JoystickPose.y > 0 ? 1 : -1;
    }
}
