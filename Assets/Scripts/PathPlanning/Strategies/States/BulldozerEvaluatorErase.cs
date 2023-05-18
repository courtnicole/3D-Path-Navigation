namespace PathNav.PathPlanning
{
    using Events;
    using Interaction;
    using Patterns.FSM;
    using UnityEngine;

    public class BulldozerEvaluatorErase<T> : IState<T> where T : BulldozerStrategy
    {
        #region Implementation of IState<T>
        public void Enter(T entity)
        {
            EventManager.Publish(EventId.EraseStarted, this, new PathStrategyEventArgs(entity));
        }

        public void UpdateLogic(T entity)
        {
            if (!entity.ActiveSegment.CanErasePoint(ref entity.interactingController)) return;
            entity.ActiveSegment.RemovePoint();
        }

        public void UpdatePhysics(T entity) { }

        public void Exit(T entity)
        {
            EventManager.Publish(EventId.EraseEnded, this, new PathStrategyEventArgs(entity));
        }
        #endregion
    }
    
}