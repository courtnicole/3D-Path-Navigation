namespace PathNav.PathPlanning
{
    using Patterns.FSM;

    public class SpatulaStrategyMovePoints<T> : IState<T> where T : SpatulaStrategy
    {
        #region Implementation of IState<T>
        public void Enter(T entity)
        {
            if (entity.pointIndexToMove < 0)
            {
                entity.StopMovePoint();
            }
        }

        public void UpdateLogic(T entity)
        {
            entity.ActiveSegment.MovePoint(entity.pointIndexToMove, entity.interactingController.PointerPosition);
        }

        public void UpdatePhysics(T entity) { }

        public void Exit(T entity)
        {
            entity.pointIndexToMove = -1;
        }
        #endregion
    }
}
