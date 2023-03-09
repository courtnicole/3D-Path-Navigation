namespace PathNav.Interaction
{
    using Events;
    using Input;
    using System;
    using UnityEngine;

    public enum PathStrategy
    {
        Bulldozer,
        Spatula,
    }

    public class ControllerEvaluator : MonoBehaviour
    {
        [SerializeField] private PathStrategy pathStrategy;

        private bool _startPointPlaced;

        #region Controller Variables
        internal IController[] Controllers => ControllerManagement.controllers;
        internal IController interactingController;
        private void SetController(IController controller) => interactingController = controller;
        internal void ClearController() => interactingController = null;
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
            OnControllerEvaluatorEvent(EventId.SetPathStrategy, GetControllerEvaluatorEventArgs(null));
            SubscribeToEvaluatorEvents();
        }

        private void Disable()
        {
            UnsubscribeToEvaluatorEvents();
        } 
        #endregion

        #region Manage Event Subscriptions
        private void SubscribeToEvaluatorEvents()
        {
            EventManager.Subscribe<PlacementEventArgs>(EventId.StartPointPlaced, StartPointPlaced);
            EventManager.Subscribe<ControllerEventArgs>(EventId.TriggerDown,       EvaluateTriggerInputDown);
            EventManager.Subscribe<ControllerEventArgs>(EventId.TriggerUp,         EvaluateTriggerInputUp);
            EventManager.Subscribe<ControllerEventArgs>(EventId.ButtonAClickStart, EvaluateButtonAInput);
        }

        private void UnsubscribeToEvaluatorEvents()
        {
            EventManager.Unsubscribe<PlacementEventArgs>(EventId.StartPointPlaced, StartPointPlaced);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.TriggerDown,       EvaluateTriggerInputDown);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.TriggerUp,         EvaluateTriggerInputUp);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.ButtonAClickStart, EvaluateButtonAInput);
        }
        #endregion

        #region Emit Events
        private void OnControllerEvaluatorEvent(EventId id, ControllerEvaluatorEventArgs args)
        {
            EventManager.Publish(id, this, GetControllerEvaluatorEventArgs(args.Controller));
        }

        private ControllerEvaluatorEventArgs GetControllerEvaluatorEventArgs(IController controller) => new(controller, pathStrategy);
        #endregion

        #region Event Callbacks
        private void StartPointPlaced(object sender, PlacementEventArgs args)
        {
            _startPointPlaced = true;
        }

        private void EvaluateTriggerInputDown(object obj, ControllerEventArgs args)
        {
            if (!_startPointPlaced)
            {
                OnControllerEvaluatorEvent(EventId.BeginPlacingStartPoint, GetControllerEvaluatorEventArgs(args.Controller));
            }
            else
            {
                switch (pathStrategy)
                {
                    case PathStrategy.Bulldozer:
                        EvaluateStartDrawingPath(obj, args);
                        break;
                    case PathStrategy.Spatula:
                        EvaluateStartPlaceOrMovePoint(obj, args);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void EvaluateTriggerInputUp(object obj, ControllerEventArgs args)
        {
            if (!_startPointPlaced)
            {
                OnControllerEvaluatorEvent(EventId.FinishPlacingStartPoint, GetControllerEvaluatorEventArgs(args.Controller));
            }
            else
            {
                switch (pathStrategy)
                {
                    case PathStrategy.Bulldozer:
                        EvaluateStopDrawingPath(obj, args);
                        break;
                    case PathStrategy.Spatula:
                        EvaluateStopPlaceOrMovePoint(obj, args);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void EvaluateButtonAInput(object obj, ControllerEventArgs args)
        {
            //check if there's a node in our collider. If yes, remove it. otherwise, ignore input. 
            
        }

        private void EvaluateStartDrawingPath(object obj, ControllerEventArgs args)
        {
            OnControllerEvaluatorEvent(EventId.StartDrawingPath, GetControllerEvaluatorEventArgs(args.Controller));
        }

        private void EvaluateStopDrawingPath(object obj, ControllerEventArgs args)
        {
            OnControllerEvaluatorEvent(EventId.StopDrawingPath, GetControllerEvaluatorEventArgs(args.Controller));
        }

        private void EvaluateStartPlaceOrMovePoint(object obj, ControllerEventArgs args)
        {
            OnControllerEvaluatorEvent(EventId.StartPlaceOrMovePoint, GetControllerEvaluatorEventArgs(args.Controller));
        }

        private void EvaluateStopPlaceOrMovePoint(object obj, ControllerEventArgs args)
        {
            OnControllerEvaluatorEvent(EventId.StopPlaceOrMovePoint, GetControllerEvaluatorEventArgs(args.Controller));
        }
        #endregion
    }

    public class ControllerEvaluatorEventArgs : EventArgs
    {
        public ControllerEvaluatorEventArgs(IController c, PathStrategy strategy)
        {
            Controller = c;
            Strategy = strategy;
        }

        public IController Controller { get; }
        public PathStrategy Strategy { get; }
    }
}