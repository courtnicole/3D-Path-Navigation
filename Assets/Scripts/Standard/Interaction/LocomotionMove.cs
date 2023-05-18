namespace PathNav.Patterns.FSM
{
    using Extensions;
    using Interaction;
    using UnityEngine;

    public class LocomotionMove<T> : IState<T> where T : LocomotionEvaluator
    {
        internal float elapsedTime;
        internal float currentVelocity;

        internal int direction;
        internal Vector3 travelDirection;
        internal Vector3 shift;

        public void Enter(T entity)
        {
            entity.OnLocomotionStart();
            entity.ShowHintVisual();
            elapsedTime     = 0;
            currentVelocity = entity.MinVelocity;
            direction       = entity.TouchPose.y > 0 ? 1 : -1;
        }

        public void UpdateLogic(T entity)
        {
            direction       =  entity.TouchPose.y > 0 ? 1 : -1;
            elapsedTime     += Time.deltaTime;
            travelDirection =  entity.activeController.Forward.FlattenY() * direction;
            currentVelocity =  Mathf.Lerp(entity.MinVelocity, entity.MaxVelocity, entity.Acceleration * elapsedTime);
            shift           =  travelDirection * (currentVelocity * Time.deltaTime);
            entity.OnLocomotionUpdate();
        }

        public void UpdatePhysics(T entity)
        {
            entity.PlayerTransform.position += shift;
        }

        public void Exit(T entity)
        {
            currentVelocity   = 0;
            direction         = 0;

            entity.HideHintVisual();
            entity.ClearActiveController();
            entity.OnLocomotionEnd();
        }
    }
}