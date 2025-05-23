namespace PathNav.PathPlanning
{
    using Events;
    using Patterns.FSM;
    using UnityEngine;

    public class BulldozerEvaluatorDraw<T> : IState<T> where T : BulldozerStrategy
    {
        #region Implementation of IState<T>
        public void Enter(T entity)
        {
            EventManager.Publish(EventId.DrawStarted, this, new PathStrategyEventArgs(entity));
        }

        public void UpdateLogic(T entity)
        {
            CheckAddPoint(entity);
        }

        public void UpdatePhysics(T entity) { }

        public void Exit(T entity)
        {
            EventManager.Publish(EventId.DrawEnded, this, new PathStrategyEventArgs(entity));
        }

        private static void CheckAddPoint(T entity)
        {
            if (!entity.bounds.Contains(entity.interactingController.PointerPosition)) return;
            
            float segmentLength = Vector3.Distance(entity.lastHandPosition, entity.interactingController.PointerPosition);

            if (segmentLength < BulldozerStrategy.minimumDelta) return;
            
            entity.ActiveSegment.AddPoint(entity.interactingController.PointerPosition);
            entity.lastHandPosition = entity.interactingController.PointerPosition;
        }
        #endregion
    }
}