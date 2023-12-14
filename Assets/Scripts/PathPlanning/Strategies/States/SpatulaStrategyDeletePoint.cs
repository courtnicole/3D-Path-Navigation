namespace PathNav.PathPlanning
{
    using Events;
    using Patterns.FSM;
    using UnityEngine;

    public class SpatulaStrategyDeletePoint<T> : IState<T> where T : SpatulaStrategy
    {
        #region Implementation of IState<T>
        private bool _didDelete;
        public void Enter(T entity)
        {
            if (entity.PointIndexToMoveOrDelete < 0)
            {
                _didDelete = false;
                entity.StopDeletePoint();
            }
            else
            {
                _didDelete = true;
            }
        }

        public void UpdateLogic(T entity)
        {
            if (!_didDelete) return;
            entity.ActiveSegment.RemovePoint(entity.PointIndexToMoveOrDelete);
            EventManager.Publish(EventId.PointDeleted, this, new PathStrategyEventArgs(entity));
            entity.StopDeletePoint();
        }

        public void UpdatePhysics(T entity) { }

        public void Exit(T entity) { }
        #endregion
    }
}