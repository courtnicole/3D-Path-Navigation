namespace PathNav.Interaction
{
    using Events;
    using ExperimentControl;
    using Input;
    using UnityEngine;
    public class Locomotion6D : MonoBehaviour
    {
        private IController _interactingController;
        private void SetController(IController controller) => _interactingController = controller;
        private void ClearController()                     => _interactingController = null;
        
        private const float _maxSpeed = 3.65f;
        private const float _minSpeed = 0f;
        private const float _acceleration = 0.58f;
        private float _currentVelocity;
        private float _elapsedTime;
        private int _horizontalDirection;
        private Vector3 _travelDirection;
        private Vector3 _shift;

        private bool _updateDirection;
        
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
            if(!_updateDirection) return;
            _elapsedTime += Time.deltaTime;
            
            if (_interactingController?.JoystickPose.y > 0)
            {
                _currentVelocity     = Mathf.Lerp(_minSpeed, _maxSpeed, _acceleration * _elapsedTime);
            }
            else
            {
                _currentVelocity     = Mathf.Lerp(_maxSpeed, _minSpeed, _acceleration * _elapsedTime);
            }
        }
        
        private void SubscribeToEvents()
        {
            EventManager.Subscribe<SceneControlEventArgs>(EventId.FollowPathReady, EnableLocomotion);
            EventManager.Subscribe<Locomotion6DEvaluatorArgs>(EventId.StartHorizontalUpdate, StartHorizontalUpdate);
            EventManager.Subscribe<Locomotion6DEvaluatorArgs>(EventId.EndHorizontalUpdate,   EndHorizontalUpdate);
            EventManager.Subscribe<Locomotion6DEvaluatorArgs>(EventId.VerticalUpdate,        VerticalUpdate);
        }

        private void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<SceneControlEventArgs>(EventId.FollowPathReady, EnableLocomotion);
            EventManager.Unsubscribe<Locomotion6DEvaluatorArgs>(EventId.StartHorizontalUpdate, StartHorizontalUpdate);
            EventManager.Unsubscribe<Locomotion6DEvaluatorArgs>(EventId.EndHorizontalUpdate,   EndHorizontalUpdate);
            EventManager.Unsubscribe<Locomotion6DEvaluatorArgs>(EventId.VerticalUpdate,        VerticalUpdate);
        }

        private void EnableLocomotion(object sender, SceneControlEventArgs args)
        {
            
        }
        
        private void StartHorizontalUpdate(object sender, Locomotion6DEvaluatorArgs args)
        {
            
        }
        private void EndHorizontalUpdate(object sender, Locomotion6DEvaluatorArgs args)
        {
            
        }
        private void VerticalUpdate(object sender, Locomotion6DEvaluatorArgs args)
        {
            
        }

    }
}
