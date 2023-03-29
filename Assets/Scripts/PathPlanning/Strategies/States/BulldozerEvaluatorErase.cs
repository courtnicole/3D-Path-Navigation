namespace PathNav.PathPlanning
{
    using Patterns.FSM;
    using UnityEngine;

    public class BulldozerEvaluatorErase<T> : IState<T> where T : BulldozerStrategy
    {
        #region Implementation of IState<T>
        public void Enter(T entity) { }

        public void UpdateLogic(T entity)
        {
            if (entity.CanStartErasing) RemovePoint(entity);
        }

        public void UpdatePhysics(T entity) { }

        public void Exit(T entity) { }

        private static void RemovePoint(T entity)
        {
            float segmentLength = Vector3.Distance(entity.lastHandPosition, entity.interactingController.PointerPosition);

            if (segmentLength < BulldozerStrategy.minimumDelta) return;

            entity.ActiveSegment.RemovePoint();

            entity.lastHandPosition = entity.interactingController.PointerPosition;
        }
        #endregion
    }
}