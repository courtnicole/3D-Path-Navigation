namespace PathNav.PathPlanning
{
    using Events;
    using Interaction;
    using UnityEngine;

    public class EraserVisual : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;

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
            meshRenderer.enabled = true;
        }

        private void HideEraser(object sender, PathStrategyEventArgs args)
        {
            meshRenderer.enabled = false;
        }
    }
}