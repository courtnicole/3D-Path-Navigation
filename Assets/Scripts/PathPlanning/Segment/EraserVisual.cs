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
            EventManager.Subscribe<BulldozeEventArgs>(EventId.EraseStarted, ToggleEraserVisibility);
            EventManager.Subscribe<BulldozeEventArgs>(EventId.EraseEnded,   ToggleEraserVisibility);
        }

        private void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<BulldozeEventArgs>(EventId.EraseStarted, ToggleEraserVisibility);
            EventManager.Unsubscribe<BulldozeEventArgs>(EventId.EraseEnded,   ToggleEraserVisibility);
        }

        private void ToggleEraserVisibility(object sender, BulldozeEventArgs args)
        {
            meshRenderer.enabled = !meshRenderer.enabled;
        }
    }
}