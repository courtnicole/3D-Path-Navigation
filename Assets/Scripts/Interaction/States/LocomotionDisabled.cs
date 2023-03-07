namespace PathNav.Patterns.FSM
{
    using Interaction;

    public class LocomotionDisabled<T> : IState<T> where T : LocomotionEvaluator
    {
        #region Implementation of IState<T>
        public void Enter(T entity) { }

        public void UpdateLogic(T entity) { }

        public void UpdatePhysics(T entity) { }

        public void Exit(T entity) { }
        #endregion
    }
}