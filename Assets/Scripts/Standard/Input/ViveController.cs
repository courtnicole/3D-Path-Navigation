namespace PathNav.Input
{
    using Events;
    using Interaction;
    using UnityEngine;
    using Valve.VR;

    public class ViveController : MonoBehaviour, IController
    {
        [SerializeField] private SteamVR_Behaviour_Pose controllerPose;
        [SerializeField] private ControllerInfo controllerInfo;
        [SerializeField] private AttachmentPoint attachmentPoint;

        public Transform AttachmentPoint => attachmentPoint.transform;
        private SteamVR_Input_Sources InputSource => controllerPose.inputSource;

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

        private SteamVR_Action_Boolean _triggerClick = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TriggerClick");
        private SteamVR_Action_Boolean _longHold = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TriggerHold");

        private SteamVR_Action_Boolean _systemClick = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SystemClick");
        private SteamVR_Action_Boolean _buttonAClick = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("ButtonA");
        private SteamVR_Action_Boolean _buttonBClick = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("ButtonB");

        private SteamVR_Action_Vector2 _trackpadPose = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("TrackpadPose");
        private SteamVR_Action_Boolean _trackpadTouch = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TrackpadTouch");

        private SteamVR_Action_Boolean _gripClick = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SqueezeGrip");
        private SteamVR_Action_Boolean _pinchClick = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Pinch");

        private SteamVR_Action_Vector2 _joystickPose = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("JoystickPose");
        private SteamVR_Action_Boolean _joystickTouch = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("JoystickTouch");
        private SteamVR_Action_Boolean _joystickClick = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("JoystickClick");

        private SteamVR_Action_Vibration _hapticAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");

        private void OnEnable()
        {
            EnableSteamActions();
        }

        private void OnDisable()
        {
            DisableSteamActions();
        }

        private void EnableSteamActions()
        {
            _triggerClick.AddOnStateDownListener(TriggerDown, InputSource);
            _triggerClick.AddOnStateUpListener(TriggerUp, InputSource);

            _longHold.AddOnStateDownListener(LongHoldStart, InputSource);
            _longHold.AddOnStateUpListener(LongHoldEnd, InputSource);

            _systemClick.AddOnStateDownListener(SystemClick, InputSource);

            _trackpadTouch.AddOnStateDownListener(TouchpadTouchStart, InputSource);
            _trackpadTouch.AddOnStateUpListener(TouchpadTouchEnd, InputSource);

            _trackpadPose.AddOnChangeListener(TouchpadTouchUpdate, InputSource);

            _gripClick.AddOnStateDownListener(GripStart, InputSource);
            _gripClick.AddOnStateUpListener(GripEnd, InputSource);

            _pinchClick.AddOnStateDownListener(PinchStart, InputSource);
            _pinchClick.AddOnStateUpListener(PinchEnd, InputSource);

            _buttonAClick.AddOnStateDownListener(ButtonAStart, InputSource);
            _buttonAClick.AddOnStateUpListener(ButtonAEnd, InputSource);

            _buttonBClick.AddOnStateDownListener(ButtonBStart, InputSource);
            _buttonBClick.AddOnStateUpListener(ButtonBEnd, InputSource);

            _joystickTouch.AddOnStateDownListener(JoystickTouchStart, InputSource);
            _joystickTouch.AddOnStateUpListener(JoystickTouchEnd, InputSource);

            _joystickPose.AddOnChangeListener(JoystickTouchUpdate, InputSource);

            _joystickClick.AddOnStateDownListener(JoystickClickStart, InputSource);
            _joystickClick.AddOnStateUpListener(JoystickClickEnd, InputSource);
        }

        private void DisableSteamActions()
        {
            _triggerClick.RemoveOnStateDownListener(TriggerDown, InputSource);
            _triggerClick.RemoveOnStateUpListener(TriggerUp, InputSource);

            _longHold.RemoveOnStateDownListener(LongHoldStart, InputSource);
            _longHold.RemoveOnStateUpListener(LongHoldEnd, InputSource);

            _systemClick.RemoveOnStateDownListener(SystemClick, InputSource);

            _trackpadTouch.RemoveOnStateDownListener(TouchpadTouchStart, InputSource);
            _trackpadTouch.RemoveOnStateUpListener(TouchpadTouchEnd, InputSource);

            _trackpadPose.RemoveOnChangeListener(TouchpadTouchUpdate, InputSource);

            _gripClick.RemoveOnStateDownListener(GripStart, InputSource);
            _gripClick.RemoveOnStateUpListener(GripEnd, InputSource);

            _pinchClick.RemoveOnStateDownListener(PinchStart, InputSource);
            _pinchClick.RemoveOnStateUpListener(PinchEnd, InputSource);

            _buttonAClick.RemoveOnStateDownListener(ButtonAStart, InputSource);
            _buttonAClick.RemoveOnStateUpListener(ButtonAEnd, InputSource);

            _buttonBClick.RemoveOnStateDownListener(ButtonBStart, InputSource);
            _buttonBClick.RemoveOnStateUpListener(ButtonBEnd, InputSource);

            _joystickTouch.RemoveOnStateDownListener(JoystickTouchStart, InputSource);
            _joystickTouch.RemoveOnStateUpListener(JoystickTouchEnd, InputSource);

            _joystickPose.RemoveOnChangeListener(JoystickTouchUpdate, InputSource);

            _joystickClick.RemoveOnStateDownListener(JoystickClickStart, InputSource);
            _joystickClick.RemoveOnStateUpListener(JoystickClickEnd, InputSource);
        }

        public void HapticFeedback()
        {
            _hapticAction.Execute(0, 0.05f, 0.001f, 1, InputSource);
        }

        private ControllerEventArgs ControllerEventArgs => new(this);

        private void LongHoldStart(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            EventManager.Publish(EventId.LongHoldStart, this, ControllerEventArgs);
        }

        private void LongHoldEnd(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            EventManager.Publish(EventId.LongHoldEnd, this, ControllerEventArgs);
        }

        private void SystemClick(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            EventManager.Publish(EventId.SystemClick, this, ControllerEventArgs);
        }

        private void TouchpadTouchStart(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            EventManager.Publish(EventId.TouchpadTouchStart, this, ControllerEventArgs);
        }

        private void TouchpadTouchUpdate(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
        {
            TouchPose      = axis;
            TouchPoseDelta = delta;
            EventManager.Publish(EventId.TouchpadTouchUpdate, this, ControllerEventArgs);
        }

        private void TouchpadTouchEnd(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            TouchPose      = Vector2.zero;
            TouchPoseDelta = Vector2.zero;
            EventManager.Publish(EventId.TouchpadTouchEnd, this, ControllerEventArgs);
        }

        private void TriggerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            EventManager.Publish(EventId.TriggerDown, this, ControllerEventArgs);
        }

        private void TriggerUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            EventManager.Publish(EventId.TriggerUp, this, ControllerEventArgs);
        }

        private void GripStart(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            EventManager.Publish(EventId.GripStart, this, ControllerEventArgs);
        }

        private void GripEnd(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            EventManager.Publish(EventId.GripEnd, this, ControllerEventArgs);
        }

        private void PinchStart(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            EventManager.Publish(EventId.PinchStart, this, ControllerEventArgs);
        }

        private void PinchEnd(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            EventManager.Publish(EventId.PinchEnd, this, ControllerEventArgs);
        }

        private void ButtonAStart(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            EventManager.Publish(EventId.ButtonAClickStart, this, ControllerEventArgs);
        }

        private void ButtonAEnd(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            EventManager.Publish(EventId.ButtonAClickEnd, this, ControllerEventArgs);
        }

        private void ButtonBStart(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            EventManager.Publish(EventId.ButtonBClickStart, this, ControllerEventArgs);
        }

        private void ButtonBEnd(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            EventManager.Publish(EventId.ButtonBClickEnd, this, ControllerEventArgs);
        }

        private void JoystickTouchStart(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            EventManager.Publish(EventId.JoystickTouchStart, this, ControllerEventArgs);
        }

        private void JoystickTouchUpdate(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
        {
            JoystickPose      = axis;
            JoystickPoseDelta = delta;
            EventManager.Publish(EventId.JoystickTouchUpdate, this, ControllerEventArgs);
        }

        private void JoystickTouchEnd(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            JoystickPose      = Vector2.zero;
            JoystickPoseDelta = Vector2.zero;
            EventManager.Publish(EventId.JoystickTouchEnd, this, ControllerEventArgs);
        }

        private void JoystickClickStart(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            EventManager.Publish(EventId.JoystickClickStart, this, ControllerEventArgs);
        }

        private void JoystickClickEnd(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            EventManager.Publish(EventId.JoystickClickEnd, this, ControllerEventArgs);
        }
    }
}