namespace PathNav.PathPlanning
{
    using Patterns.FSM;

    public class SpatulaStrategyMovePoints<T> : IState<T> where T : SpatulaStrategy
    {
        #region Implementation of IState<T>
        public void Enter(T entity)
        {
            if (entity.PointIndexToMove < 0)
            {
                entity.StopMovePoint();
            }
        }

        public void UpdateLogic(T entity)
        {
            entity.ActiveSegment.MovePoint(entity.PointIndexToMove, entity.interactingController.PointerPosition);
        }

        public void UpdatePhysics(T entity) { }

        public void Exit(T entity)
        {
           
        }
        #endregion
    }
}
