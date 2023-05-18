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
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.EraseStarted, ToggleEraserVisibility);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.EraseEnded,   ToggleEraserVisibility);
        }

        private void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.EraseStarted, ToggleEraserVisibility);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.EraseEnded,   ToggleEraserVisibility);
        }

        private void ToggleEraserVisibility(object sender, PathStrategyEventArgs args)
        {
            meshRenderer.enabled = !meshRenderer.enabled;
        }
    }
}