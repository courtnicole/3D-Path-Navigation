namespace PathNav.Input
{
    using System;
    using Interaction;
    using PathPlanning;
    using UnityEngine;

    public interface IController
    {
        public Transform Transform { get; }

        public ControllerInfo ControllerInfo { get; }
        public Vector3 PointerPosition { get; }

        public Vector3 Position { get; }
        public Quaternion Rotation { get; }

        public Vector3 Forward { get; }
        public Vector3 Up { get; }

        public Vector2 TouchPose { get; }
        public Vector2 TouchPoseDelta { get; }

        public Vector2 JoystickPose { get; }
        public Vector2 JoystickPoseDelta { get; }

        public Transform AttachmentPoint { get; }

        public void HapticFeedback();
        
    }

    public class ControllerEventArgs : EventArgs
    {
        public ControllerEventArgs(IController controller) => Controller = controller;

        public IController Controller { get; }
    }
}