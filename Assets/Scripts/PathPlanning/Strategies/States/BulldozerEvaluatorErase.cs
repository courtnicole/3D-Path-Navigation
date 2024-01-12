namespace PathNav.PathPlanning
{
    using Events;
    using Patterns.FSM;

    public class BulldozerEvaluatorErase<T> : IState<T> where T : BulldozerStrategy
    {
        #region Implementation of IState<T>
        public void Enter(T entity)
        {
            EventManager.Publish(EventId.EraseToggleOn, this, new PathStrategyEventArgs(entity.interactingController));
        }

        public void UpdateLogic(T entity)
        {
            if (!entity.ActiveSegment.CanErasePoint(ref entity.interactingController)) return;
            entity.ActiveSegment.RemovePoint();
        }

        public void UpdatePhysics(T entity) { }

        public void Exit(T entity)
        {
            EventManager.Publish(EventId.EraseToggleOff, this, new PathStrategyEventArgs(entity.interactingController));
        }
        #endregion
    }
    
}