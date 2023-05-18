namespace PathNav
{
    using Patterns.FSM;

    public class Disabled<T> : IState<T> where T : class
    {
        #region Implementation of IState<T>
        public void Enter(T entity) { }

        public void UpdateLogic(T entity) { }

        public void UpdatePhysics(T entity) { }

        public void Exit(T entity) { }
        #endregion
    }
}