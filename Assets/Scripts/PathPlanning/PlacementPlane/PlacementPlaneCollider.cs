namespace PathNav.PathPlanning
{
    using Events;
    using ExperimentControl;
    using Input;
    using Interaction;
    using System.Threading.Tasks;
    using UnityEngine;

    public class PlacementPlaneCollider : MonoBehaviour
    {
        [SerializeField] private InteractableElement interactable;
        [SerializeField] private PlacementPlaneElement placementPlaneElement;
        [SerializeField] private GameObject ghostPointVisualPrefab;

        private IInteractable   Interactable   => interactable;
        private IPlacementPlane PlacementPlane => placementPlaneElement;

        private bool _hasValidCollision;
        private bool _hidePointVisual;
        private bool _startPointRegistered;

        private const float _hideAfterPlacement = 1.25f;

        private IController _interactingController;
        private GameObject _ghostPointVisual;

        #region Unity Methods
        private void OnEnable()
        {
            _ghostPointVisual = Instantiate(ghostPointVisualPrefab);
            _ghostPointVisual.SetActive(false);
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            SubscribeToEvents();
        }

        private void Update()
        {
            if (!_startPointRegistered) return;
            if (!_hasValidCollision) return;
            if (_hidePointVisual) return;

            _ghostPointVisual.SetActive(true);
            _ghostPointVisual.transform.position = _interactingController.PointerPosition;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_startPointRegistered) return;
            if (!other.gameObject.CompareTag($"Hand")) return;

            _interactingController = other.gameObject.GetComponent<ControllerCollider>().Controller;
            _hasValidCollision     = true;
            PlacementPlane.OnTriggerEntered();
            Interactable.OnHover();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_hasValidCollision) return;

            _hasValidCollision     = false;
            _interactingController = null;

            PlacementPlane.OnTriggerExited();
            Interactable.OnUnhover();

            _ghostPointVisual.SetActive(false);
        }
        #endregion

        #region Event Subscriptions
        public void SubscribeToEvents()
        {
            EventManager.Subscribe<SceneControlEventArgs>(EventId.RegisterStartPoint, StartPointPlaced);
        }

        public void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<SceneControlEventArgs>(EventId.RegisterStartPoint, StartPointPlaced);
        }
        #endregion

        #region Logic
        public async void DelayVisibility()
        {
            _hidePointVisual = true;
            await Task.Delay((int)(_hideAfterPlacement * 1000));
            _hidePointVisual = false;
        }

        private void StartPointPlaced(object args, SceneControlEventArgs placementEventArgs)
        {
            _startPointRegistered = true;
        }
        #endregion
    }
}