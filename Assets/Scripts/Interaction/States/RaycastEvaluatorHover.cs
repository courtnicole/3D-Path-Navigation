namespace PathNav.Patterns.FSM
{
    using Events;
    using Interaction;

    public class RaycastEvaluatorHover<T> : IState<T> where T : RaycastEvaluator
    {
        public void Enter(T entity)
        {
            SetInteractable(entity);
        }

        public void UpdateLogic(T entity)
        {
            if (!entity.PreviouslyIdle)
            {
                entity.ProcessInteractableCandidate();
            }

            if (entity.ShouldSelect)
            {
                entity.Select();
                return;
            }

            if (entity.ShouldUnhover)
            {
                entity.Unhover();
                return;
            }
        }

        public void UpdatePhysics(T entity) { }

        public void Exit(T entity)
        {
            UnsetInteractable(entity);
        }

        private void SetInteractable(T entity)
        {
            if (entity.Interactable == entity.Candidate)
            {
                return;
            }

            UnsetInteractable(entity);
            entity.Interactable = entity.Candidate;
            entity.Candidate.AddInteractor(entity.Controller);
            entity.Candidate.OnHover();
            entity.OnInteractableEvent(EventId.RaycastInteractableSet, entity.Candidate);
        }

        private void UnsetInteractable(T entity)
        {
            IInteractable interactable = entity.Interactable;

            if (interactable == null)
            {
                return;
            }

            entity.Interactable = null;
            interactable.RemoveInteractor(entity.Controller);
            interactable.OnUnhover();
            entity.OnInteractableEvent(EventId.RaycastInteractableUnset, interactable);
        }
    }
}