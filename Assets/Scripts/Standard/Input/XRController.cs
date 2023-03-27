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
        [Header("Controller Info")] [SerializeField]
        private TrackedPoseDriver controllerPose;

        [SerializeField] private ControllerInfo controllerInfo;
        [SerializeField] private AttachmentPoint attachmentPoint;

        [Header("Input Actions")]

        #region Input Actions
        public InputActionReference triggerClick;

        public InputActionReference systemClick;
        public InputActionReference buttonAClick;
        public InputActionReference buttonBClick;
        public InputActionReference trackpadPose;
        public InputActionReference trackpadTouch;
        public InputActionReference gripClick;
        public InputActionReference joystickPose;
        public InputActionReference joystickTouch;
        public InputActionReference joystickClick;
        public InputActionReference hapticAction;
        #endregion

        private InputDevice InputDevice => controllerPose.positionInput.action.activeControl.device;

        #region Implementation of IController
        public Transform Transform => controllerPose.transform;
        public ControllerInfo ControllerInfo => controllerInfo;
        public Vector3 PointerPosition => controllerPose.transform.position;
        public Vector3 Position => controllerPose.transform.position;
        public Quaternion Rotation => controllerPose.transform.rotation;
        public Vector3 Forward => Rotation * Vector3.forward;
        public Vector3 Up => Rotation      * Vector3.up;
        public Vector2 TouchPose { get; private set; }
        public Vector2 TouchPoseDelta { get; private set; }
        public Vector2 JoystickPose { get; private set; }
        public Vector2 JoystickPoseDelta { get; private set; }
        public Transform AttachmentPoint => attachmentPoint.transform;

        public void HapticFeedback()
        {
            OpenXRInput.SendHapticImpulse(hapticAction.action, 1.0f, 0.0f, 0.1f, InputDevice);
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
            triggerClick.action.performed += TriggerDown;
            triggerClick.action.canceled  += TriggerUp;

            systemClick.action.Enable();
            systemClick.action.performed += SystemClick;

            buttonAClick.action.Enable();
            buttonAClick.action.performed += ButtonAStart;

            buttonBClick.action.Enable();
            buttonBClick.action.performed += ButtonBStart;

            trackpadPose.action.Enable();
            TouchPose                     =  Vector2.zero;
            TouchPoseDelta                =  Vector2.zero;
            trackpadPose.action.performed += TouchpadTouchUpdate;

            trackpadTouch.action.Enable();
            trackpadTouch.action.performed += TouchpadTouchStart;
            trackpadTouch.action.canceled  += TouchpadTouchEnd;

            gripClick.action.Enable();
            gripClick.action.performed += GripClick;

            joystickPose.action.Enable();
            JoystickPose                  =  Vector2.zero;
            JoystickPoseDelta             =  Vector2.zero;
            joystickPose.action.performed += JoystickPoseUpdate;

            joystickTouch.action.Enable();
            joystickTouch.action.performed += JoystickTouchStart;
            joystickTouch.action.canceled  += JoystickTouchEnd;

            joystickClick.action.Enable();
            joystickClick.action.performed += JoystickClick;
        }

        private void DisableActions()
        {
            triggerClick.action.performed  -= TriggerDown;
            triggerClick.action.canceled   -= TriggerUp;
            systemClick.action.performed   -= SystemClick;
            buttonAClick.action.performed  -= ButtonAStart;
            buttonBClick.action.performed  -= ButtonBStart;
            trackpadPose.action.performed  -= TouchpadTouchUpdate;
            trackpadTouch.action.performed -= TouchpadTouchStart;
            trackpadTouch.action.canceled  -= TouchpadTouchEnd;
            gripClick.action.performed     -= GripClick;
            joystickPose.action.performed  -= JoystickPoseUpdate;
            joystickTouch.action.performed -= JoystickTouchStart;
            joystickClick.action.performed -= JoystickClick;

            triggerClick.action.Disable();
            systemClick.action.Disable();
            buttonAClick.action.Disable();
            buttonBClick.action.Disable();
            trackpadPose.action.Disable();
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

        private void ButtonAStart(InputAction.CallbackContext callbackContext)
        {
            EventManager.Publish(EventId.ButtonAClickStart, this, ControllerEventArgs);
        }

        private void ButtonAEnd(InputAction fromAction, InputDevice fromSource)
        {
            EventManager.Publish(EventId.ButtonAClickEnd, this, ControllerEventArgs);
        }

        private void ButtonBStart(InputAction.CallbackContext callbackContext)
        {
            EventManager.Publish(EventId.ButtonBClickStart, this, ControllerEventArgs);
        }

        private void ButtonBEnd(InputAction fromAction, InputDevice fromSource)
        {
            EventManager.Publish(EventId.ButtonBClickEnd, this, ControllerEventArgs);
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