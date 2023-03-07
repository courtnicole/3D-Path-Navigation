namespace PathNav.PathPlanning
{
    using Patterns.FSM;
    using UnityEngine;

    public class BulldozerEvaluatorDraw<T> : IState<T> where T : BulldozerStrategy
    {
        #region Implementation of IState<T>
        public void Enter(T entity) { }

        public void UpdateLogic(T entity)
        {
            AddPoint(entity);
        }

        public void UpdatePhysics(T entity) { }

        public void Exit(T entity) { }

        private static void AddPoint(T entity)
        {
            float segmentLength = Vector3.Distance(entity.lastHandPosition, entity.interactingController.PointerPosition);

            if (segmentLength < entity.minimumDelta) return;
            
            entity.ActiveSegment.AddPoint(entity.interactingController.PointerPosition);
            entity.lastHandPosition = entity.interactingController.PointerPosition;
        }
        #endregion
    }
}