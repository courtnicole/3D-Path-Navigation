namespace PathNav.Patterns.FSM
{
    using Interaction;
    using System;
    using UnityEngine;

    public class LocomotionMove<T> : IState<T> where T : LocomotionEvaluator
    {
        private float _elapsedTime;
        private float _currentVelocity;
        private float _direction;

        private Vector3 _travelDirection;
        private Vector3 _shift;

        public void Enter(T entity)
        {
            entity.OnLocomotionStart();

            _elapsedTime     = 0;
            _currentVelocity = 0; //entity.dof == LocomotionDof.FourDoF ? entity.follower.followSpeed : 0;
            _travelDirection = Vector3.zero;
            _shift           = Vector3.zero;
        }

        public void UpdateLogic(T entity)
        {
            switch (entity.dof)
            {
                case LocomotionDof.FourDoF:
                    Update4DoF(entity);
                    break;
                case LocomotionDof.SixDof:
                    Update6DoF(entity);
                    break;
                case LocomotionDof.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void UpdatePhysics(T entity)
        {
            switch (entity.dof)
            {
                case LocomotionDof.FourDoF:
                    Shift4DoF(entity);
                    break;
                case LocomotionDof.SixDof:
                    Shift6DoF(entity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            entity.OnLocomotionUpdate();
        }

        public void Exit(T entity)
        {
            entity.ClearActiveMotionController();
            entity.ClearActiveVerticalController();
            entity.OnLocomotionEnd();
        }

        private void Update4DoF(T entity)
        {
            _elapsedTime     += Time.deltaTime;
            _direction       =  entity.InputPose.y > 0 ? 1 : -1;
            _currentVelocity =  Mathf.Clamp(_currentVelocity + (_direction * entity.Acceleration) * Time.deltaTime, entity.MinVelocity, entity.MaxVelocity);
        }

        private void Update6DoF(T entity)
        {
            _elapsedTime += Time.deltaTime;
            
            _travelDirection = entity.InputPose.normalized;
            _currentVelocity   = Mathf.Clamp(_currentVelocity + (entity.Acceleration * Time.deltaTime), entity.MinVelocity, entity.MaxVelocity);
            _shift             = _travelDirection * (_currentVelocity * Time.deltaTime);
        }
        
        private void Shift4DoF(T entity)
        {
            entity.follower.followSpeed = _currentVelocity;
        }

        private void Shift6DoF(T entity)
        {
            entity.follower.followSpeed     =  _currentVelocity;
            entity.PlayerTransform.position += _shift;
        }
    }
}