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
        private IController _interactingController;
        private void SetController(IController controller) => _interactingController = controller;
        internal void ClearController() => _interactingController = null;
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
            EventManager.Subscribe<ControllerEventArgs>(EventId.JoystickPoseUpdate, EvaluateJoystickPoseUpdate);
            EventManager.Subscribe<ControllerEventArgs>(EventId.JoystickTouchEnd,   EvaluateJoystickTouchEnd);
        }

        private void UnsubscribeToEvents()
        {
            EventManager.Subscribe<ControllerEventArgs>(EventId.JoystickTouchStart, EvaluateJoystickTouchStart);
            EventManager.Subscribe<ControllerEventArgs>(EventId.JoystickPoseUpdate, EvaluateJoystickPoseUpdate);
            EventManager.Subscribe<ControllerEventArgs>(EventId.JoystickTouchEnd,   EvaluateJoystickTouchEnd);
        }
        #endregion
        
        private void EvaluateJoystickTouchEnd(object obj, ControllerEventArgs args)
        {
            
        }

        private void EvaluateJoystickPoseUpdate(object obj, ControllerEventArgs args)
        {
            
        }

        private void EvaluateJoystickTouchStart(object obj, ControllerEventArgs args)
        {
            
        }

        #region Emit Events
        private void OnControllerEvaluatorEvent(EventId id, FollowerEvaluatorEventArgs args)
        {
            EventManager.Publish(id, this, GetControllerEvaluatorEventArgs(args.Controller));
        }

        private static FollowerEvaluatorEventArgs GetControllerEvaluatorEventArgs(IController controller) => new(controller);
        #endregion
        
        
    }
    
    public class FollowerEvaluatorEventArgs : EventArgs
    {
        public FollowerEvaluatorEventArgs(IController c) => Controller = c;

        public IController Controller { get; }
    }
}
