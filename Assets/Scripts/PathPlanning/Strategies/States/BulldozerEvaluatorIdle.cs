namespace PathNav.PathPlanning
{
    using Events;
    using Input;
    using Patterns.FSM;
    using UnityEngine;

    public class BulldozerEvaluatorIdle<T> : IState<T> where T : BulldozerStrategy
    {
        #region Implementation of IState<T>
        public void Enter(T entity) { }

        public void UpdateLogic(T entity)
        {
        }

        public void UpdatePhysics(T entity)
        {

        }

        public void Exit(T entity)
        {
            
        }
        #endregion
    }
}
