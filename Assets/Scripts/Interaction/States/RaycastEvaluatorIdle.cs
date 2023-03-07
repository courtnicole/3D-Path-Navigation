namespace PathNav.Patterns.FSM
{
    using Interaction;

    public class RaycastEvaluatorIdle<T> : IState<T> where T : RaycastEvaluator
    {
        public void Enter(T entity) { }

        public void UpdateLogic(T entity)
        {
            if (entity.ProcessInteractableCandidate())
            {
                if (entity.ShouldHover)
                {
                    entity.Hover();
                    return;
                }
            }

            else if (entity.ProcessSpawnCandidate())
            {
                if (entity.ShouldAcquireSpawnPoint)
                {
                    entity.AcquireSpawnPoint();
                }
            }
        }

        public void UpdatePhysics(T entity) { }

        public void Exit(T entity) { }
    }
}
