namespace PathNav.Interaction
{
    using Input;
    using UnityEngine;

    public class ControllerCollider : MonoBehaviour
    {
         [InterfaceType(typeof(IController))]
         [SerializeField] private Object controller;
        
        public IController Controller => controller as IController;
        
        public void OnTriggerEnter(Collider other)
        { 
            Controller.HapticFeedback();
        }
        public void TriggerExited(Collider other) { }
        public void TriggerStayed(Collider other) { }
        
    }
}
