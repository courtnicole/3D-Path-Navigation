namespace PathNav.Patterns.FSM
{
    using Events;
    using Interaction;

    public class RaycastEvaluatorSelect<T> : IState<T> where T : RaycastEvaluator
    {
        public void Enter(T entity)
        {
            while (entity.QueuedSelect)
            {
                entity.selectorQueue.Dequeue();
            }

            if (entity.Interactable != null) SelectInteractable(entity);
        }

        public void UpdateLogic(T entity)
        {
            if (entity.ShouldUnselect) entity.Unselect();
        }

        public void UpdatePhysics(T entity) { }

        public void Exit(T entity)
        {
            while (entity.QueuedUnselect)
            {
                entity.selectorQueue.Dequeue();
            }

            UnselectInteractable(entity);
        }

        private void SelectInteractable(T entity)
        {
            entity.Unselect();

            UnselectInteractable(entity);
            entity.Selected = entity.Interactable;
            entity.Interactable.AddSelectingInteractor(entity.Controller);
            entity.OnInteractableEvent(EventId.RaycastInteractableSelected, entity.Interactable);
        }

        private void UnselectInteractable(T entity)
        {
            IInteractable interactable = entity.Selected;

            if (interactable == null) return;

            entity.Selected = null;
            interactable.RemoveSelectingInteractor(entity.Controller);
            entity.OnInteractableEvent(EventId.RaycastInteractableUnselected, interactable);
        }
    }
}