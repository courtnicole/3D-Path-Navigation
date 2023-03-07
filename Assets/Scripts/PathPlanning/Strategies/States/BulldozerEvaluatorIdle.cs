namespace PathNav.PathPlanning
{
    using Patterns.FSM;

    public class BulldozerEvaluatorIdle<T> : IState<T> where T : BulldozerStrategy
    {
        #region Implementation of IState<T>
        public void Enter(T entity) { }
        public void UpdateLogic(T entity) { }
        public void UpdatePhysics(T entity) { }
        public void Exit(T entity) { }
        #endregion
    }
}
