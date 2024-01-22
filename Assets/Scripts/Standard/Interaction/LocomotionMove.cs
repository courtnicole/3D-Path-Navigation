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
        private float _vertical;
        private Vector3 _shift;

        public void Enter(T entity)
        {
            entity.OnLocomotionStart();

            _elapsedTime     = 0;
            _currentVelocity = entity.dof == LocomotionDof.FourDoF ? entity.follower.followSpeed : 0;
            _travelDirection = Vector3.zero;
            _vertical        = 0;
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
            
            if (entity.ShouldUseVertical)
            {
                _vertical = Vector3.Angle(entity.VerticalShift, Vector3.up);
                if (_vertical is < 95 and > 85)
                {
                    _vertical = 0;
                    return;
                }
                _vertical = Map(0.0f, 180.0f, 1.0f, -1.0f, _vertical);
            }
            else
            {
                _vertical = 0;
            }
            
            _travelDirection   = new Vector3(entity.InputPose.x, 0, entity.InputPose.y);
            _travelDirection   = entity.CameraTransform.TransformDirection(_travelDirection);
            _travelDirection.y = _vertical;
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

        private static float Map(float a1, float a2, float b1, float b2, float s) => b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }
}