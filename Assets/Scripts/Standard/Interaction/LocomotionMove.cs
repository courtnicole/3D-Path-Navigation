namespace PathNav.Patterns.FSM
{
    using Interaction;
    using System;
    using UnityEngine;

    public class LocomotionMove<T> : IState<T> where T : LocomotionEvaluator
    {
        private float _currentVelocity;
        private float _direction;

        private Vector3 _travelDirection;
        private Vector3 _shift;
        private bool _useShift;

        public void Enter(T entity)
        {
            entity.OnLocomotionStart();

            _currentVelocity = 0; //entity.dof == LocomotionDof.FourDoF ? entity.follower.followSpeed : 0;
            _travelDirection = Vector3.zero;
            _shift           = Vector3.zero;
        }

        public void UpdateLogic(T entity)
        {
            _useShift = Mathf.Abs(entity.JoystickPose.y) > 0.125f;
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
            _currentVelocity = 0; 
            entity.follower.followSpeed = 0;
            
            entity.ClearActiveMotionController();
            entity.OnLocomotionEnd();
        }

        private void Update4DoF(T entity)
        {
            if (!_useShift)
            {
                _currentVelocity = 0;
            }
            _direction       =  entity.JoystickPose.y > 0 ? 1 : -1;
            _currentVelocity = Mathf.Clamp(_currentVelocity + (_direction * entity.Acceleration) * Time.deltaTime, entity.MaxVelocity * -1 , entity.MaxVelocity);
            if (Mathf.Abs(_currentVelocity) < entity.MinVelocity)
            {
                _currentVelocity = entity.MinVelocity * _direction;
            }
        }

        private void Update6DoF(T entity)
        {
            if (!_useShift)
            {
                _currentVelocity = 0;
            }
            _direction       =  entity.JoystickPose.y > 0 ? 1 : -1;
            _travelDirection =  entity.InputPose.normalized;
            _currentVelocity =  Mathf.Clamp(_currentVelocity + (_direction * entity.Acceleration) * Time.deltaTime, entity.MaxVelocity * -1, entity.MaxVelocity);
            if (Mathf.Abs(_currentVelocity) < entity.MinVelocity)
            {
                _currentVelocity = entity.MinVelocity * _direction;
            }
            _shift           =  _travelDirection * (_currentVelocity * Time.deltaTime);
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