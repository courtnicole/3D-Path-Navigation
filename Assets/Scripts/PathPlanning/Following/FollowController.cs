namespace PathNav.PathPlanning
{
    using Dreamteck.Splines;
    using Events;
    using Interaction;
    using System;
    using UnityEngine;

    public class FollowController : MonoBehaviour
    {
        [SerializeField] private SplineFollower follower;
        [SerializeField] private float maxSpeed = 2.05f;
        private const float _minSpeed = 0.35f;
        private const float _acceleration = 0.58f;
        private float _speedRatio;
        private const float _wave = 1.59155f;
        
        private void OnEnable()
        {
            SubscribeToEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            EventManager.Subscribe<SceneControlEventArgs>(EventId.FollowPathReady, FollowPath);
            EventManager.Subscribe<FollowerEvaluatorEventArgs>(EventId.ChangeSpeed,     SetSpeed);
        }

        private void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<SceneControlEventArgs>(EventId.FollowPathReady, FollowPath);
            EventManager.Unsubscribe<FollowerEvaluatorEventArgs>(EventId.ChangeSpeed, SetSpeed);
        }

        private void FollowPath(object sender, SceneControlEventArgs args)
        {
            follower.followSpeed = _minSpeed;
            follower.follow      = true;
            _speedRatio = 0;
        }

        private void SetSpeed(object sender, FollowerEvaluatorEventArgs args)
        {
            _speedRatio          = (follower.followSpeed + (args.Sign * _acceleration * Time.deltaTime)) / maxSpeed;
            follower.followSpeed = _minSpeed + (maxSpeed * Mathf.Sin(_speedRatio * _wave));
        }
    }
}