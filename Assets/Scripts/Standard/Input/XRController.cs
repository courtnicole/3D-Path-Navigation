namespace PathNav.Input
{
    using Interaction;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.XR;

    public class XRController : MonoBehaviour, IController
    {
        [Header("Controller Info")]
        [SerializeField] private TrackedPoseDriver controllerPose;
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
        
        #endregion
        
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
        public void HapticFeedback() { }
        #endregion
    }
}
