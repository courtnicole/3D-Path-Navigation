namespace PathNav.PathPlanning
{
    using Dreamteck.Splines;
    using Events;
    using ExperimentControl;
    using Input;
    using Interaction;
    using UnityEngine;

    public class FollowController : MonoBehaviour
    {
        [SerializeField] private SplineFollower follower;
        private const float _maxSpeed = 3.65f;
        private const float _minSpeed = 0f;
        private float _from;
        private float _to;
        private float _startTime;
        private const float _duration = 4.5f;
        private bool _updateSpeed;
        private IController _interactingController;
        private void SetController(IController controller) => _interactingController = controller;
        private void ClearController() => _interactingController = null;
        
        private const float Acceleration = 0.58f;
        private float currentVelocity;
        private float elapsedTime;
        
        private void OnEnable()
        {
            SubscribeToEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeToEvents();
        }

        private void Update()
        {
            if(!_updateSpeed) return;
            elapsedTime += Time.deltaTime;
            
            if (_interactingController?.JoystickPose.y > 0)
            {
                currentVelocity      = Mathf.Lerp(_minSpeed, _maxSpeed, Acceleration * elapsedTime);
                follower.followSpeed = currentVelocity;
            }
            else
            {
                currentVelocity      = Mathf.Lerp(_maxSpeed, _minSpeed, Acceleration * elapsedTime);
                follower.followSpeed = currentVelocity;
            }
        }
        private void SubscribeToEvents()
        {
            EventManager.Subscribe<SceneControlEventArgs>(EventId.FollowPathReady, FollowPath);
            EventManager.Subscribe<FollowerEvaluatorEventArgs>(EventId.StartSpeedUpdate, StartSpeedUpdate);
            EventManager.Subscribe<FollowerEvaluatorEventArgs>(EventId.EndSpeedUpdate,   EndSpeedUpdate);
        }

        private void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<SceneControlEventArgs>(EventId.FollowPathReady, FollowPath);
            EventManager.Unsubscribe<FollowerEvaluatorEventArgs>(EventId.StartSpeedUpdate, StartSpeedUpdate);
            EventManager.Unsubscribe<FollowerEvaluatorEventArgs>(EventId.EndSpeedUpdate,   EndSpeedUpdate);
        }

        private void StartSpeedUpdate(object sender, FollowerEvaluatorEventArgs args)
        {
            elapsedTime          = 0;
            follower.followSpeed = _minSpeed;
            _updateSpeed         = true;
            _startTime           = Time.time;
            SetController(args.Controller);
        }
        
        private void EndSpeedUpdate(object sender, FollowerEvaluatorEventArgs args)
        {
            elapsedTime          = 0;
            follower.followSpeed = _minSpeed;
            _updateSpeed         = false;
            _startTime           = 0;
            ClearController();
        }

        private void FollowPath(object sender, SceneControlEventArgs args)
        {
            follower.followSpeed = _minSpeed;
            follower.follow      = true;
        }
    }
}