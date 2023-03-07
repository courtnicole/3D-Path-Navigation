namespace PathNav.PathPlanning
{
    using Patterns.FSM;

    public class SpatulaStrategyIdle<T> : IState<T> where T : SpatulaStrategy
    {
        #region Implementation of IState<T>
        public void Enter(T entity)
        {
            entity.ClearController();
        }
        public void UpdateLogic(T entity) { }
        public void UpdatePhysics(T entity) { }
        public void Exit(T entity) { }
        #endregion
    }
}
