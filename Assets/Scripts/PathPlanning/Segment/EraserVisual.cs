namespace PathNav.PathPlanning
{
    using Events;
    using Input;
    using Interaction;
    using UnityEngine;

    public class EraserVisual : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [InterfaceType(typeof(IController))] [SerializeField]
        private Object controller;
        private IController Controller => controller as IController;
        private void OnEnable()
        {
            meshRenderer.enabled = false;
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.EraseToggleOn, ShowEraser);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.EraseToggleOff,   HideEraser);
        }

        private void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.EraseToggleOn, ShowEraser);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.EraseToggleOff,   HideEraser);
        }

        private void ShowEraser(object sender, PathStrategyEventArgs args)
        {
            if(args.Controller.InputDevice == Controller.InputDevice) 
                meshRenderer.enabled = true;
        }

        private void HideEraser(object sender, PathStrategyEventArgs args)
        {
            meshRenderer.enabled = false;
        }
    }
}