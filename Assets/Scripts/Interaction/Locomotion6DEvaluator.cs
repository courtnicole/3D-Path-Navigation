namespace PathNav.Interaction
{
    using Events;
    using UnityEngine;
    using Input;
    using System;

    public class Locomotion6DEvaluator : MonoBehaviour
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
            EventManager.Subscribe<ControllerEventArgs>(EventId.JoystickTouchEnd,   EvaluateJoystickTouchEnd);
            EventManager.Subscribe<ControllerEventArgs>(EventId.ButtonAClick,       EvaluateButtonAClick);
            EventManager.Subscribe<ControllerEventArgs>(EventId.ButtonBClick,       EvaluateButtonBClick);
        }

        private void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.JoystickTouchStart, EvaluateJoystickTouchStart);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.JoystickTouchEnd,   EvaluateJoystickTouchEnd);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.ButtonAClick,       EvaluateButtonAClick);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.ButtonBClick,       EvaluateButtonBClick);
        }
        #endregion

        private void EvaluateButtonAClick(object obj, ControllerEventArgs args)
        {
            OnLocomotion6DEvaluatorEvent(EventId.VerticalUpdate, GetLocomotion6DEvaluatorEventArgs(args.Controller, -1));
        }
        
        private void EvaluateButtonBClick(object obj, ControllerEventArgs args)
        {
            OnLocomotion6DEvaluatorEvent(EventId.VerticalUpdate, GetLocomotion6DEvaluatorEventArgs(args.Controller, 1));
        }
        
        private void EvaluateJoystickTouchStart(object obj, ControllerEventArgs args)
        {
            OnLocomotion6DEvaluatorEvent(EventId.StartHorizontalUpdate, GetLocomotion6DEvaluatorEventArgs(args.Controller));
        }
        
        private void EvaluateJoystickTouchEnd(object obj, ControllerEventArgs args)
        {
            OnLocomotion6DEvaluatorEvent(EventId.EndHorizontalUpdate, GetLocomotion6DEvaluatorEventArgs(args.Controller));
        }
        
        #region Emit Events
        private void OnLocomotion6DEvaluatorEvent(EventId id, Locomotion6DEvaluatorArgs args)
        {
            EventManager.Publish(id, this, args);
        }

        private static Locomotion6DEvaluatorArgs GetLocomotion6DEvaluatorEventArgs(IController controller, int direction) => new(controller, direction);
        
        private static Locomotion6DEvaluatorArgs GetLocomotion6DEvaluatorEventArgs(IController controller, bool enabled) => new(controller, enabled);
        private static Locomotion6DEvaluatorArgs GetLocomotion6DEvaluatorEventArgs(IController controller) => new(controller);
        #endregion
    }
    
    public class Locomotion6DEvaluatorArgs : EventArgs
    {
        public Locomotion6DEvaluatorArgs(IController c, bool enableLocomotion)
        {
            Controller       = c;
            EnableLocomotion = enableLocomotion;
            Direction        = 0;
        }
        
        public Locomotion6DEvaluatorArgs(IController c)
        {
            Controller       = c;
            EnableLocomotion = true;
            Direction        = 0;
        }
        
        public Locomotion6DEvaluatorArgs(IController c, int direction)
        {
            Controller       = c;
            EnableLocomotion = true;
            Direction        = direction;
        }

        public IController Controller       { get; }
        public bool        EnableLocomotion { get; }
        public int         Direction        { get; }
    }
}
