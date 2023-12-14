namespace PathNav.Input
{
    using Events;
    using Interaction;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.XR;
    using UnityEngine.XR.OpenXR.Input;

    public class XRController : MonoBehaviour, IController
    {
        private enum Hand
        {
            Right,
            Left,
        }

        [Header("Controller Info")] [SerializeField]
        private TrackedPoseDriver controllerPose;

        [SerializeField] private TrackedPoseDriver pointerPose;
        [SerializeField] private ControllerInfo controllerInfo;
        [SerializeField] private AttachmentPoint attachmentPoint;
        [SerializeField] private Collider eraserCollider;
        [SerializeField] private Hand hand;

        [Header("Input Actions")]

        #region Input Actions
        public InputActionReference triggerClick;

        public InputActionReference systemClick;
        public InputActionReference buttonAClick;
        public InputActionReference buttonBClick;
        public InputActionReference touchpadPose;
        public InputActionReference trackpadTouch;
        public InputActionReference gripClick;
        public InputActionReference joystickPose;
        public InputActionReference joystickTouch;
        public InputActionReference joystickClick;
        public InputActionReference hapticAction;
        #endregion

        #region Implementation of IController
        public Transform Transform => controllerPose.transform;
        public ControllerInfo ControllerInfo => controllerInfo;
        public Vector3 Position => controllerPose.transform.position;
        public Quaternion Rotation => controllerPose.transform.rotation;
        public Vector3 Forward => Rotation * Vector3.forward;
        public Vector3 Up => Rotation      * Vector3.up;
        public Vector3 PointerPosition => pointerPose.transform.position;
        public Quaternion PointerRotation => pointerPose.transform.rotation;
        public Vector3 PointerForward => pointerPose.transform.forward;
        public Vector3 PointerUp => PointerRotation      * Vector3.up;
        public Vector2 TouchPose { get; private set; }
        public Vector2 TouchPoseDelta { get; private set; }
        public Vector2 JoystickPose { get; private set; }
        public Vector2 JoystickPoseDelta { get; private set; }
        public Transform AttachmentPoint => attachmentPoint.transform;
        public Bounds CollisionBounds => eraserCollider.bounds;

        public InputDevice InputDevice =>
            hand == Hand.Left ? UnityEngine.InputSystem.XR.XRController.leftHand : UnityEngine.InputSystem.XR.XRController.rightHand;

        public void HapticFeedback()
        {
            OpenXRInput.SendHapticImpulse(hapticAction.action, 0.80f, 0.0f, 0.1f, InputDevice);
        }
        #endregion

        private ControllerEventArgs ControllerEventArgs => new(this);

        private void OnEnable()
        {
            EnableActions();
        }

        private void OnDisable()
        {
            DisableActions();
        }

        private void EnableActions()
        {
            triggerClick.action.Enable();
            triggerClick.action.started += TriggerDown;
            triggerClick.action.canceled  += TriggerUp;

            systemClick.action.Enable();
            systemClick.action.performed += SystemClick;

            buttonAClick.action.Enable();
            buttonAClick.action.started  += ButtonAPressDown;
            buttonAClick.action.canceled += ButtonAPressUp;

            buttonBClick.action.Enable();
            buttonBClick.action.performed += ButtonBPress;

            touchpadPose.action.Enable();
            TouchPose                     =  Vector2.zero;
            TouchPoseDelta                =  Vector2.zero;
            touchpadPose.action.performed += TouchpadTouchUpdate;

            trackpadTouch.action.Enable();
            trackpadTouch.action.started += TouchpadTouchStart;
            trackpadTouch.action.canceled  += TouchpadTouchEnd;

            gripClick.action.Enable();
            gripClick.action.performed += GripClick;

            joystickPose.action.Enable();
            JoystickPose                  =  Vector2.zero;
            JoystickPoseDelta             =  Vector2.zero;
            joystickPose.action.performed += JoystickPoseUpdate;

            joystickTouch.action.Enable();
            joystickTouch.action.started += JoystickTouchStart;
            joystickTouch.action.canceled  += JoystickTouchEnd;

            joystickClick.action.Enable();
            joystickClick.action.performed += JoystickClick;
        }

        private void DisableActions()
        {
            triggerClick.action.started  -= TriggerDown;
            triggerClick.action.canceled -= TriggerUp;
            
            systemClick.action.performed   -= SystemClick;
            
            buttonAClick.action.started  -= ButtonAPressDown;
            buttonAClick.action.canceled -= ButtonAPressUp;
            
            buttonBClick.action.performed  -= ButtonBPress;
            
            touchpadPose.action.performed  -= TouchpadTouchUpdate;
            
            trackpadTouch.action.started -= TouchpadTouchStart;
            trackpadTouch.action.canceled  -= TouchpadTouchEnd;
            
            gripClick.action.performed     -= GripClick;
            
            joystickPose.action.performed  -= JoystickPoseUpdate;
            
            joystickTouch.action.started  -= JoystickTouchStart;
            joystickTouch.action.canceled -= JoystickTouchEnd;
            
            joystickClick.action.performed -= JoystickClick;

            triggerClick.action.Disable();
            systemClick.action.Disable();
            buttonAClick.action.Disable();
            buttonBClick.action.Disable();
            touchpadPose.action.Disable();
            trackpadTouch.action.Disable();
            gripClick.action.Disable();
            joystickPose.action.Disable();
            joystickTouch.action.Disable();
            joystickClick.action.Disable();
        }

        private void SystemClick(InputAction.CallbackContext callbackContext)
        {
            EventManager.Publish(EventId.SystemClick, this, ControllerEventArgs);
        }

        private void TouchpadTouchStart(InputAction.CallbackContext callbackContext)
        {
            EventManager.Publish(EventId.TouchpadTouchStart, this, ControllerEventArgs);
        }

        private void TouchpadTouchUpdate(InputAction.CallbackContext callbackContext)
        {
            Vector2 previousPose = TouchPose;
            TouchPose      = callbackContext.action.ReadValue<Vector2>();
            TouchPoseDelta = TouchPose - previousPose;
            EventManager.Publish(EventId.TouchpadTouchUpdate, this, ControllerEventArgs);
        }

        private void TouchpadTouchEnd(InputAction.CallbackContext callbackContext)
        {
            TouchPose      = Vector2.zero;
            TouchPoseDelta = Vector2.zero;
            EventManager.Publish(EventId.TouchpadTouchEnd, this, ControllerEventArgs);
        }

        private void TriggerDown(InputAction.CallbackContext callbackContext)
        {
            HapticFeedback();
            EventManager.Publish(EventId.TriggerDown, this, ControllerEventArgs);
        }

        private void TriggerUp(InputAction.CallbackContext callbackContext)
        {
            EventManager.Publish(EventId.TriggerUp, this, ControllerEventArgs);
        }

        private void GripClick(InputAction.CallbackContext callbackContext)
        {
            EventManager.Publish(EventId.GripClick, this, ControllerEventArgs);
        }

        private void ButtonAPressDown(InputAction.CallbackContext callbackContext)
        {
            EventManager.Publish(EventId.ButtonAClickDown, this, ControllerEventArgs);
        }
        
        private void ButtonAPressUp(InputAction.CallbackContext callbackContext)
        {
            EventManager.Publish(EventId.ButtonAClickUp, this, ControllerEventArgs);
        }

        private void ButtonBPress(InputAction.CallbackContext callbackContext)
        {
            EventManager.Publish(EventId.ButtonBClick, this, ControllerEventArgs);
        }

        private void JoystickTouchStart(InputAction.CallbackContext callbackContext)
        {
            EventManager.Publish(EventId.JoystickTouchStart, this, ControllerEventArgs);
        }

        private void JoystickPoseUpdate(InputAction.CallbackContext callbackContext)
        {
            Vector2 previousPose = JoystickPose;
            JoystickPose      = callbackContext.action.ReadValue<Vector2>();
            JoystickPoseDelta = JoystickPose - previousPose;
            EventManager.Publish(EventId.JoystickPoseUpdate, this, ControllerEventArgs);
        }

        private void JoystickTouchEnd(InputAction.CallbackContext callbackContext)
        {
            JoystickPose      = Vector2.zero;
            JoystickPoseDelta = Vector2.zero;
            EventManager.Publish(EventId.JoystickTouchEnd, this, ControllerEventArgs);
        }

        private void JoystickClick(InputAction.CallbackContext callbackContext)
        {
            EventManager.Publish(EventId.JoystickClickStart, this, ControllerEventArgs);
        }
    }
}