namespace PathNav.Input
{
    using Events;
    using Interaction;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.XR.Interaction.Toolkit;
    using InputDevice = UnityEngine.XR.InputDevice;

    public class ViveController : MonoBehaviour, IController
    {
        [SerializeField] private XRController controllerPose;
        [SerializeField] private ControllerInfo controllerInfo;
        [SerializeField] private AttachmentPoint attachmentPoint;

        public Transform AttachmentPoint => attachmentPoint.transform;
        private InputDevice InputSource => controllerPose.inputDevice;
        
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

        private InputActionReference _triggerClick;

        private InputActionReference _systemClick;
        private InputActionReference _buttonAClick; 
        private InputActionReference _buttonBClick;

        private InputActionReference _trackpadPose;
        private InputActionReference _trackpadTouch; 

        private InputActionReference _gripClick; 

        private InputActionReference _joystickPose; 
        private InputActionReference _joystickTouch; 
        private InputActionReference _joystickClick; 

        // private SteamVR_Action_Vibration _hapticAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }
        
        public void HapticFeedback()
        {
            //_hapticAction.Execute(0, 0.05f, 0.001f, 1, InputSource);
        }

        private ControllerEventArgs ControllerEventArgs => new(this);

        private void LongHoldStart(InputAction fromAction, InputDevice fromSource)
        {
            EventManager.Publish(EventId.LongHoldStart, this, ControllerEventArgs);
        }

        private void LongHoldEnd(InputAction fromAction, InputDevice fromSource)
        {
            EventManager.Publish(EventId.LongHoldEnd, this, ControllerEventArgs);
        }

        private void SystemClick(InputAction fromAction, InputDevice fromSource)
        {
            EventManager.Publish(EventId.SystemClick, this, ControllerEventArgs);
        }

        private void TouchpadTouchStart(InputAction fromAction, InputDevice fromSource)
        {
            EventManager.Publish(EventId.TouchpadTouchStart, this, ControllerEventArgs);
        }

        private void TouchpadTouchUpdate(InputAction fromAction, InputDevice fromSource, Vector2 axis, Vector2 delta)
        {
            TouchPose      = axis;
            TouchPoseDelta = delta;
            EventManager.Publish(EventId.TouchpadTouchUpdate, this, ControllerEventArgs);
        }

        private void TouchpadTouchEnd(InputAction fromAction, InputDevice fromSource)
        {
            TouchPose      = Vector2.zero;
            TouchPoseDelta = Vector2.zero;
            EventManager.Publish(EventId.TouchpadTouchEnd, this, ControllerEventArgs);
        }

        private void TriggerDown(InputAction fromAction, InputDevice fromSource)
        {
            EventManager.Publish(EventId.TriggerDown, this, ControllerEventArgs);
        }

        private void TriggerUp(InputAction fromAction, InputDevice fromSource)
        {
            EventManager.Publish(EventId.TriggerUp, this, ControllerEventArgs);
        }

        private void GripStart(InputAction fromAction, InputDevice fromSource)
        {
            EventManager.Publish(EventId.GripStart, this, ControllerEventArgs);
        }

        private void GripEnd(InputAction fromAction, InputDevice fromSource)
        {
            EventManager.Publish(EventId.GripEnd, this, ControllerEventArgs);
        }

        private void PinchStart(InputAction fromAction, InputDevice fromSource)
        {
            EventManager.Publish(EventId.PinchStart, this, ControllerEventArgs);
        }

        private void PinchEnd(InputAction fromAction, InputDevice fromSource)
        {
            EventManager.Publish(EventId.PinchEnd, this, ControllerEventArgs);
        }

        private void ButtonAStart(InputAction fromAction, InputDevice fromSource)
        {
            EventManager.Publish(EventId.ButtonAClickStart, this, ControllerEventArgs);
        }

        private void ButtonAEnd(InputAction fromAction, InputDevice fromSource)
        {
            EventManager.Publish(EventId.ButtonAClickEnd, this, ControllerEventArgs);
        }

        private void ButtonBStart(InputAction fromAction, InputDevice fromSource)
        {
            EventManager.Publish(EventId.ButtonBClickStart, this, ControllerEventArgs);
        }

        private void ButtonBEnd(InputAction fromAction, InputDevice fromSource)
        {
            EventManager.Publish(EventId.ButtonBClickEnd, this, ControllerEventArgs);
        }

        private void JoystickTouchStart(InputAction fromAction, InputDevice fromSource)
        {
            EventManager.Publish(EventId.JoystickTouchStart, this, ControllerEventArgs);
        }

        private void JoystickTouchUpdate(InputAction fromAction, InputDevice fromSource, Vector2 axis, Vector2 delta)
        {
            JoystickPose      = axis;
            JoystickPoseDelta = delta;
            EventManager.Publish(EventId.JoystickTouchUpdate, this, ControllerEventArgs);
        }

        private void JoystickTouchEnd(InputAction fromAction, InputDevice fromSource)
        {
            JoystickPose      = Vector2.zero;
            JoystickPoseDelta = Vector2.zero;
            EventManager.Publish(EventId.JoystickTouchEnd, this, ControllerEventArgs);
        }

        private void JoystickClickStart(InputAction fromAction, InputDevice fromSource)
        {
            EventManager.Publish(EventId.JoystickClickStart, this, ControllerEventArgs);
        }

        private void JoystickClickEnd(InputAction fromAction, InputDevice fromSource)
        {
            EventManager.Publish(EventId.JoystickClickEnd, this, ControllerEventArgs);
        }
    }
}