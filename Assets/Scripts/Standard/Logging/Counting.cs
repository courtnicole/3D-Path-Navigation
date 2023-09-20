namespace PathNav.SceneManagement
{
    using Events;
    using PathPlanning;
    using UnityEngine;

    public class Counting : MonoBehaviour
    {
        private int _editCount;

        public void Enable()
        {
            _editCount = 0;
            SubscribeToEvents();
        }

        public void Disable()
        {
            UnsubscribeFromEvents();
        }

        private void IncrementEditCount()
        {
            _editCount++;
        }

        private void SubscribeToEvents()
        {
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.EraseStarted, EditMade);
            EventManager.Subscribe<PathStrategyEventArgs>(EventId.MoveStarted,  EditMade);
        }

        private void UnsubscribeFromEvents()
        {
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.EraseStarted, EditMade);
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.MoveStarted,  EditMade);
        }

        private void EditMade(object sender, PathStrategyEventArgs args)
        {
            IncrementEditCount();
        }

        public int GetEditCount() => _editCount;
    }
}