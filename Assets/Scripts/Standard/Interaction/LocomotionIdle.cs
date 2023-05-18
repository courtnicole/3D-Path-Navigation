namespace PathNav.Patterns.FSM
{
    using Interaction;

    public class LocomotionIdle<T> : IState<T> where T : LocomotionEvaluator
    {
        public void Enter(T entity)
        {
        }

        public void UpdateLogic(T entity)
        {
        }

        public void UpdatePhysics(T entity)
        {
        }

        public void Exit(T entity)
        {
        }
    }
}
