namespace PathNav.PathPlanning
{
    using Events;
    using Input;
    using Patterns.FSM;

    public class BulldozerEvaluatorIdle<T> : IState<T> where T : BulldozerStrategy
    {
        #region Implementation of IState<T>
        public void Enter(T entity) { }

        public void UpdateLogic(T entity)
        {
        }

        public void UpdatePhysics(T entity)
        {
            if (entity.ActiveSegment.CanErasePoint(ref entity.Controllers[0]))
            {
                if (entity.CanStartErasing)
                {
                    EventManager.Publish(EventId.CanErase, this, new PathStrategyEventArgs(entity));
                }
            }
            else if (entity.ActiveSegment.CanErasePoint(ref entity.Controllers[1]))
            {
                if (entity.CanStartErasing)
                {
                    EventManager.Publish(EventId.CanErase, this, new PathStrategyEventArgs(entity));
                }
            }
            else
            {
                EventManager.Publish(EventId.CannotErase, this, new PathStrategyEventArgs(entity));
            }
        }
        public void Exit(T entity) { }
        #endregion
    }
}
