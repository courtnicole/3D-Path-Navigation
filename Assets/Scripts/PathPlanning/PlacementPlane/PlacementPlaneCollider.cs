namespace PathNav.PathPlanning
{
    using Interaction;
    using System.Threading.Tasks;
    using UnityEngine;
    public class PlacementPlaneCollider : MonoBehaviour
    {
        [SerializeField] private InteractableElement interactable;
        [SerializeField] private PlacementPlaneElement placementPlaneElement;
        [SerializeField] private GameObject ghostPointVisualPrefab;
        
        private IInteractable Interactable => interactable;
        private IPlacementPlane PlacementPlane => placementPlaneElement;

        private bool _hasValidCollision;
        private bool _hidePointVisual;

        private const float _hideAfterPlacement = 1.25f;
        
        private Transform _collidingTransform;
        private GameObject _ghostPointVisual;

        private void OnEnable()
        {
            _ghostPointVisual = Instantiate(ghostPointVisualPrefab);
            _ghostPointVisual.SetActive(false);
        }

        private void Update()
        {
            if (!_hasValidCollision) return;
            if (_hidePointVisual) return;
            
            _ghostPointVisual.SetActive(true);
            _ghostPointVisual.transform.position = _collidingTransform.position;
            
        }

        public void SubscribeToEvents() { }
        public void UnsubscribeToEvents() { }
        

        public async void DelayVisibility()
        {
            _hidePointVisual = true;
            await Task.Delay((int)(_hideAfterPlacement * 1000));
            _hidePointVisual = false;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag($"Hand")) return;

            _hasValidCollision = true;
            PlacementPlane.OnTriggerEntered();
            Interactable.OnHover();
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!_hasValidCollision) return;
            
            _hasValidCollision = false;
            PlacementPlane.OnTriggerExited();
            Interactable.OnUnhover();
            _ghostPointVisual.SetActive(false);
        }
    }
}
