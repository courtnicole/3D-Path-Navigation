namespace PathNav.Interaction
{
    using Events;
    using ExperimentControl;
    using Input;
    using PathPlanning;
    using System;
    using UnityEngine;



    public class ControllerEvaluator : MonoBehaviour
    {
        private PathStrategy _pathStrategy;
        private bool _pathStrategySet;
        private bool _startPointPlaced;

        #region Controller Variables
        internal IController[] Controllers => ControllerManagement.controllers;
        private IController _interactingController;
        private  void SetController(IController controller) => _interactingController = controller;
        internal void ClearController()                     => _interactingController = null;
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
            EventManager.Subscribe<SceneControlEventArgs>(EventId.SetPathStrategy, SetPathStrategy);
            EventManager.Subscribe<PlacementEventArgs>(EventId.StartPointPlaced, StartPointPlaced);
            EventManager.Subscribe<ControllerEventArgs>(EventId.TriggerDown,      EvaluateTriggerInputDown);
            EventManager.Subscribe<ControllerEventArgs>(EventId.TriggerUp,        EvaluateTriggerInputUp);
            EventManager.Subscribe<ControllerEventArgs>(EventId.ButtonAClickDown, EvaluateButtonAInputDown);
            EventManager.Subscribe<ControllerEventArgs>(EventId.ButtonAClickUp, EvaluateButtonAInputUp);
            EventManager.Subscribe<ControllerEventArgs>(EventId.ButtonBClick,     EvaluateButtonBInput);
        }

        private void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<SceneControlEventArgs>(EventId.SetPathStrategy, SetPathStrategy);
            EventManager.Unsubscribe<PlacementEventArgs>(EventId.StartPointPlaced, StartPointPlaced);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.TriggerDown,      EvaluateTriggerInputDown);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.TriggerUp,        EvaluateTriggerInputUp);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.ButtonAClickDown, EvaluateButtonAInputDown);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.ButtonAClickUp,   EvaluateButtonAInputUp);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.ButtonBClick,     EvaluateButtonBInput);
        }
        #endregion

        #region Emit Events
        private void OnControllerEvaluatorEvent(EventId id, ControllerEvaluatorEventArgs args)
        {
            EventManager.Publish(id, this, GetControllerEvaluatorEventArgs(args.Controller));
        }

        private static ControllerEvaluatorEventArgs GetControllerEvaluatorEventArgs(IController controller) => new(controller);
        #endregion

        #region Event Callbacks
        private void SetPathStrategy(object sender, SceneControlEventArgs args)
        {
            _pathStrategy    = args.Strategy;
            _pathStrategySet = true;
        }

        private void StartPointPlaced(object sender, PlacementEventArgs args)
        {
            _startPointPlaced = true;
        }

        private void EvaluateTriggerInputDown(object sender, ControllerEventArgs args)
        {
            if (!_pathStrategySet) return;
            
            if (!_startPointPlaced)
            {
                OnControllerEvaluatorEvent(EventId.BeginPlacingStartPoint, GetControllerEvaluatorEventArgs(args.Controller));
            }
            else
            {
                switch (_pathStrategy)
                {
                    case PathStrategy.Bulldozer:
                        OnControllerEvaluatorEvent(EventId.StartDrawPath, GetControllerEvaluatorEventArgs(args.Controller));
                        break;
                    case PathStrategy.Spatula:
                        OnControllerEvaluatorEvent(EventId.StartPlaceOrMovePoint, GetControllerEvaluatorEventArgs(args.Controller));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void EvaluateTriggerInputUp(object sender, ControllerEventArgs args)
        {
            if (!_startPointPlaced)
            {
                OnControllerEvaluatorEvent(EventId.FinishPlacingStartPoint, GetControllerEvaluatorEventArgs(args.Controller));
            }
            else
            {
                switch (_pathStrategy)
                {
                    case PathStrategy.Bulldozer:
                        OnControllerEvaluatorEvent(EventId.StopDrawPath, GetControllerEvaluatorEventArgs(args.Controller));
                        break;
                    case PathStrategy.Spatula:
                        OnControllerEvaluatorEvent(EventId.StopPlaceOrMovePoint, GetControllerEvaluatorEventArgs(args.Controller));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void EvaluateButtonAInputDown(object sender, ControllerEventArgs args)
        {
            if (!_pathStrategySet) return;
            if (!_startPointPlaced) return;
            switch (_pathStrategy)
            {
                case PathStrategy.Bulldozer:
                    OnControllerEvaluatorEvent(EventId.StartErasePath, GetControllerEvaluatorEventArgs(args.Controller));
                    break;
                case PathStrategy.Spatula:
                    OnControllerEvaluatorEvent(EventId.RemovePoint, GetControllerEvaluatorEventArgs(args.Controller));
                    break;
                case PathStrategy.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
        
        private void EvaluateButtonAInputUp(object sender, ControllerEventArgs args)
        {
            if (!_pathStrategySet) return;
            if (!_startPointPlaced) return;
            switch (_pathStrategy)
            {
                case PathStrategy.Bulldozer:
                    OnControllerEvaluatorEvent(EventId.StopErasePath, GetControllerEvaluatorEventArgs(args.Controller));
                    break;
                case PathStrategy.Spatula:
                    break;
                case PathStrategy.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
        
        private void EvaluateButtonBInput(object sender, ControllerEventArgs args)
        {
            OnControllerEvaluatorEvent(EventId.PathCreationComplete, GetControllerEvaluatorEventArgs(args.Controller));
        }
        #endregion
    }

    public class ControllerEvaluatorEventArgs : EventArgs
    {
        public ControllerEvaluatorEventArgs(IController c) => Controller = c;

        public IController Controller { get; }
    }
}