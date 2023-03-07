namespace PathNav.Interaction
{
    using Events;
    using Input;
    using UnityEngine;

    public class SpatulaInteractor : MonoBehaviour
    {
        #region Controller Variables
        internal IController[] Controllers => ControllerManagement.controllers;
        internal IController interactingController;
        private void SetController(IController controller) => interactingController = controller;
        internal void ClearController() => interactingController = null;
        #endregion

        #region Start Point Variables
        private GameObject _startObject;
        private IAttachable _attachable;
        private IPlaceable _placeable;

        private bool _startPointPlaced; 
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
            EventManager.Subscribe<ControllerEventArgs>(EventId.TriggerDown, EvaluateStartPlaceOrMovePoint);
            EventManager.Subscribe<ControllerEventArgs>(EventId.TriggerUp, EvaluateStopPlaceOrMovePoint);
        }

        private void UnsubscribeToEvaluatorEvents()
        {
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.TriggerDown, EvaluateStartPlaceOrMovePoint);
            EventManager.Unsubscribe<ControllerEventArgs>(EventId.TriggerUp,   EvaluateStopPlaceOrMovePoint);
        }
        #endregion

        #region Emit Events
        private void OnControllerEvaluatorEvent(EventId id, ControllerEvaluatorEventArgs args)
        {
            EventManager.Publish(id, this, GetControllerEvaluatorEventArgs(args.Controller));
        }

        private ControllerEvaluatorEventArgs GetControllerEvaluatorEventArgs(IController controller) => new(controller, PathStrategy.Spatula);
        #endregion

        #region Received Events
        private void EvaluateStartPlaceOrMovePoint(object obj, ControllerEventArgs args)
        {
            if (!_startPointPlaced) return;

            OnControllerEvaluatorEvent(EventId.StartPlaceOrMovePoint, GetControllerEvaluatorEventArgs(args.Controller));
        }

        private void EvaluateStopPlaceOrMovePoint(object obj, ControllerEventArgs args)
        {
            if (!_startPointPlaced) return;

            OnControllerEvaluatorEvent(EventId.StopPlaceOrMovePoint, GetControllerEvaluatorEventArgs(args.Controller));
        }
        #endregion
    }
}
