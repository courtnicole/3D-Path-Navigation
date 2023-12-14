
using UnityEngine;

namespace PathNav.ExperimentControl
{
    using Input;

    public class Test : MonoBehaviour
    {
        public XRController PointerRight;
        public XRController ControllerRight;
        public Transform pointer;
        public Transform controller;
        private void Update()
        {
            pointer.position    = PointerRight.PointerPosition;
            pointer.rotation    = PointerRight.PointerRotation;
            controller.position = ControllerRight.Position;
            controller.rotation = ControllerRight.Rotation;
        }
    }
}
