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
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.CanErase,     ShowEraser);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.EraseStarted, ShowEraser);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.EraseEnded,   HideEraser);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.CannotErase,  HideEraser);
        }

        private void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.CanErase,     ShowEraser);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.EraseStarted, ShowEraser);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.EraseEnded,   HideEraser);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.CannotErase,  HideEraser);
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