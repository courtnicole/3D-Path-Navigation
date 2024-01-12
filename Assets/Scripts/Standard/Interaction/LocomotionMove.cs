namespace PathNav.Patterns.FSM
{
    using Interaction;
    using System;
    using UnityEngine;

    public class LocomotionMove<T> : IState<T> where T : LocomotionEvaluator
    {
        private float _elapsedTime;
        private float _currentVelocity;

        private Vector3 _travelDirection;
        private float _vertical;
        private Vector3 _shift;

        public void Enter(T entity)
        {
            entity.OnLocomotionStart();

            _elapsedTime     = 0;
            _currentVelocity = entity.MinVelocity;
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
            if (entity.dof == LocomotionDof.FourDoF)
                entity.follower.followSpeed = entity.MinVelocity;

            entity.ClearActiveController();
            entity.OnLocomotionEnd();
        }

        private void Update4DoF(T entity)
        {
            _elapsedTime     += Time.deltaTime;
            _currentVelocity =  Mathf.Lerp(entity.MinVelocity, entity.MaxVelocity, entity.Acceleration * _elapsedTime);
        }

        private void Update6DoF(T entity)
        {
            _elapsedTime += Time.deltaTime;

            if (entity.useVerticalShift)
            {
                _vertical = Vector3.Angle(entity.VerticalShift, Vector3.forward);
                _vertical = Map(0, 180, 0, 0.8f, _vertical);

                if (Vector3.Cross(entity.VerticalShift, Vector3.forward).y < 0)
                    _vertical = -_vertical;
            }
            else
            {
                _vertical = 0;
            }

            _travelDirection = new Vector3(entity.InputPose.x, _vertical, entity.InputPose.y).normalized;
            _currentVelocity = Mathf.Lerp(entity.MinVelocity, entity.MaxVelocity, entity.Acceleration * _elapsedTime);
            _shift           = _travelDirection * (_currentVelocity * Time.deltaTime);
        }

        private void Shift4DoF(T entity)
        {
            entity.follower.followSpeed = _currentVelocity;
        }

        private void Shift6DoF(T entity)
        {
            entity.PlayerTransform.position += _shift;
        }

        private float Map(float a1, float a2, float b1, float b2, float s) => b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }
}