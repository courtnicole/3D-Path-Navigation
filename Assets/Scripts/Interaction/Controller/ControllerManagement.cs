namespace PathNav.Input
{
    using UnityEngine;

    public class ControllerManagement : MonoBehaviour
    {
        [SerializeField] [InterfaceType(typeof(IController))]
        private Object controllerLeftHand;

        public static IController controllerLeft;

        [SerializeField] [InterfaceType(typeof(IController))]
        private Object controllerRightHand;

        public static IController controllerRight;

        public static IController[] controllers;

        private void Awake()
        {
            if (controllerLeftHand is null || controllerRightHand is null) 
            {
                Debug.Log("Controllers need to be assigned to hand manager in the inspector!");
                enabled = false;
            }
            controllerLeft  = controllerLeftHand as IController;
            controllerRight = controllerRightHand as IController;
            controllers     = new[] { controllerLeft, controllerRight, };
        }
    }
}