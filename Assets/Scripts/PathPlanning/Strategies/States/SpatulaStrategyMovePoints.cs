namespace PathNav.PathPlanning
{
    using Events;
    using Patterns.FSM;

    public class SpatulaStrategyMovePoints<T> : IState<T> where T : SpatulaStrategy
    {
        private bool _didMove;
        #region Implementation of IState<T>
        public void Enter(T entity)
        {
            if (entity.PointIndexToMove < 0)
            {
                _didMove = false;
                entity.StopMovePoint();
            }
            else
            {
                _didMove = true;
                EventManager.Publish(EventId.MoveStarted, this, new PathStrategyEventArgs(entity));
            }
        }

        public void UpdateLogic(T entity)
        {
            entity.ActiveSegment.MovePoint(entity.PointIndexToMove, entity.interactingController.PointerPosition);
        }

        public void UpdatePhysics(T entity) { }

        public void Exit(T entity)
        {
            if (_didMove)
            {
                EventManager.Publish(EventId.MoveEnded, this, new PathStrategyEventArgs(entity));
            }
        }
        #endregion
    }
}
