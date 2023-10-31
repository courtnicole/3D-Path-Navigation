namespace PathNav.PathPlanning
{
    using Events;
    using Patterns.FSM;
    using UnityEngine;

    public class SpatulaStrategyCreatePoints<T> : IState<T> where T : SpatulaStrategy
    {
        #region Implementation of IState<T>
        public void Enter(T entity) { }

        public void UpdateLogic(T entity)
        {
            AddPoint(entity);

            entity.StopCreatePoint();
        }

        public void UpdatePhysics(T entity) { }

        public void Exit(T entity) { }

        private void AddPoint(T entity)
        {
            float segmentLength = Vector3.Distance(entity.lastHandPosition, entity.interactingController.PointerPosition);

            if (segmentLength < SpatulaStrategy.minimumDelta) return;
            
            entity.ActiveSegment.AddPoint(entity.interactingController.PointerPosition);
            entity.lastHandPosition = entity.interactingController.PointerPosition;
            EventManager.Publish(EventId.PointPlaced, this, new PathStrategyEventArgs(entity));
        }
        #endregion
    }
}
