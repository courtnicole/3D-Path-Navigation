namespace PathNav.Interaction
{
    using Events;
    using Extensions;
    using PathPlanning;
    using UnityEngine;

    public class PointVisualElement : MonoBehaviour, IPointVisual
    {
        private IInteractable _interactable;
        #region Implementation of IPointVisual
        public UniqueId Id => Data.Id;
        public int Index => Data.Index;

        public IData Data { get; private set; }

        private bool _configured;

        public bool Configure()
        {
            if (_configured) return true;

            _configured = true;

            return true;
        }

        public bool ConfigureData()
        {
            if (!gameObject.TryGetComponent(out PointVisualDataContainer container)) return false;

            Data = container.PointVisualData;

            if (!transform.GetChild(0).TryGetComponent(out InteractableElement interactable)) return false;
            
            interactable.Id = Id;
            _interactable   = interactable;
            

            if (!transform.GetChild(0).TryGetComponent(out PointCollider pointCollider)) return false;
            
            pointCollider.SetPointVisualElement(this);

            return true;
        }

        public void Move(Vector3 position)
        {
            transform.position = position;
        }

        public void Hide()
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        internal void ChildTriggerEnter()
        {
            if (Id == null) return;
            EventManager.Publish(EventId.PointVisualTriggered, this, GetPointVisualEventArgs());
            _interactable.OnHover();
        }

        internal void ChildTriggerExit()
        {
            EventManager.Publish(EventId.PointVisualUntriggered, this, GetPointVisualEventArgs());
            _interactable.OnUnhover();
        }

        private PointVisualEventArgs GetPointVisualEventArgs() => new(this);

        public int CompareTo(IPathElement other) => Index.CompareTo(other.Index);
        #endregion
    }
}
