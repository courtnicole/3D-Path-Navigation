namespace PathNav.PathPlanning
{
    using Events;
    using ExperimentControl;
    using Extensions;
    using Input;
    using Interaction;
    using Patterns.Factory;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    
    public enum PathStrategy
    {
        None,
        Bulldozer,
        Spatula,
    }

    public class PathStrategyEvaluator : MonoBehaviour
    {
        [SerializeField] private AssetReferenceGameObject segmentKey;
        [SerializeField] private GameObject startPointPrefab;
        [SerializeField] private PlacementPlaneElement placementPlane;

        #region Factory Variables
        private ISegment _activeSegment;
        #endregion

        #region Start Point Variables
        private GameObject _startObject;
        private SegmentStartPoint _startObjectController;
        private bool _startPointActive;
        private bool StartPointExists => _startObject != null;
        private bool _startPointPlaced; 
        #endregion

        #region Controller Variables
        private IController _interactingController;
        private void SetController(IController controller) => _interactingController = controller;
        private void ClearController() => _interactingController = null;
        #endregion

        #region Strategy Variables
        private bool _strategySet;
        private PathStrategy _strategy;
        private IPathStrategy _activeStrategy;
        private IPathStrategy _bulldozerStrategy;
        private IPathStrategy _spatulaStrategy; 
        #endregion

        #region Unity Events
        private void OnEnable()
        {
            Enable();
        }

        private void OnDisable()
        {
            Disable();
        }

        private void Update()
        {
            _activeStrategy?.Run();
        }
        #endregion

        #region Initialization
        private void Enable()
        {
            SubscribeToEvents();
        }

        private void Disable()
        {
            UnsubscribeToEvents();
            ClearStrategy();
        } 
        #endregion

        #region Manage Event Subscriptions
        private void SubscribeToEvents()
        {
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.BeginPlacingStartPoint,  BeginPlacingStartPoint);
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.FinishPlacingStartPoint, FinishPlacingStartPoint);

            EventManager.Subscribe<PlacementEventArgs>(EventId.StartPointPlaced, StartPointPlaced);
            EventManager.Subscribe<SceneControlEventArgs>(EventId.SetPathStrategy, SetPathStrategy);
        }

        private void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.BeginPlacingStartPoint,  BeginPlacingStartPoint);
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.FinishPlacingStartPoint, FinishPlacingStartPoint);

            EventManager.Unsubscribe<PlacementEventArgs>(EventId.StartPointPlaced, StartPointPlaced);
            EventManager.Unsubscribe<SceneControlEventArgs>(EventId.SetPathStrategy, SetPathStrategy);
        } 
        #endregion

        #region Event Callbacks
        private void StartPointPlaced(object sender, PlacementEventArgs args)
        {
            if (!_strategySet) return;
            _activeStrategy.SetStartPosition(args.Position, -args.Heading);
            ConfigureSegment();
        }

        private void SetPathStrategy(object sender, SceneControlEventArgs args)
        {
            _strategy = args.Strategy;
            switch (_strategy)
            {
                case PathStrategy.Bulldozer:
                    _bulldozerStrategy = new BulldozerStrategy();
                    SetStrategy(_bulldozerStrategy);
                    break;
                case PathStrategy.Spatula:
                    _spatulaStrategy = new SpatulaStrategy(placementPlane);
                    InitializeSpatulaStrategy();
                    SetStrategy(_spatulaStrategy);
                    break;
                default:
                    _bulldozerStrategy = new BulldozerStrategy();
                    InitializeBulldozerStrategy();
                    SetStrategy(_bulldozerStrategy);
                    break;
            }
        }
        #endregion

        #region Strategy Management

        private void InitializeSpatulaStrategy()
        {
            placementPlane.Enable();
        }

        private void InitializeBulldozerStrategy()
        {
            placementPlane.Disable();
        }
        
        private void SetStrategy(IPathStrategy strategy)
        {
            _activeStrategy = strategy;
            _strategySet    = true;
        }

        private void ConfigureStrategy()
        {
            _activeStrategy.Enable();
            _activeStrategy.SubscribeToEvents();
        }

        private void ClearStrategy()
        {
            if(!_strategySet) return;
            _activeStrategy.UnsubscribeToEvents();
            _activeStrategy = null;
        }
        #endregion

        #region Start Point Creation
        private void BeginPlacingStartPoint(object obj, ControllerEvaluatorEventArgs args)
        {
            if (_startPointPlaced) return;
            SetController(args.Controller);
            AttachStartPoint();
        }

        private void FinishPlacingStartPoint(object obj, ControllerEvaluatorEventArgs args)
        {
            if (!StartPointExists) return;
            if (!_startPointActive) return;
            if (_startPointPlaced) return;
            
            (_startObjectController as IPlaceable).Place(EventId.StartPointPlaced, _startObject.transform);
            _startObjectController.Detach();
            _startPointPlaced = true;
            ClearController();
        }

        private void AttachStartPoint()
        {
            if (_startPointActive) return;
            if (_startPointPlaced) return;
            
            _startObject = Instantiate(startPointPrefab);

            if (_startObject.TryGetComponent(out SegmentStartPoint startObjectController))
            {
                _startObjectController = startObjectController;
                _startObjectController.Attach(_interactingController.AttachmentPoint);
                _startPointActive = true;
            }
            else
            {
                Destroy(_startObject);
            }
        }
        #endregion

        #region Segment Creation
        private async void ConfigureSegment()
        {
            bool result = await CreateSegment();

            if (!result) return;

            _activeStrategy.SetActiveSegment(_activeSegment);
            _activeStrategy.AddFirstPoint();
            ConfigureStrategy();
        }

        private async Task<bool> CreateSegment()
        {
            Task<ISegment> segmentTask = AddSegmentToPathAsync(0);
            ISegment segment = await segmentTask;

            if (segment is null) return false;

            segment.Configure();
            segment.ConfigureNodeVisuals(_strategy);
            _activeSegment = segment;

            return true;
        }

        private async Task<ISegment> AddSegmentToPathAsync(int index)
        {
            Task<GameObject> segmentTask = Factory.InstantiateObjectAsync(segmentKey, Utility.Parameterize(transform), CancellationToken.None);
            GameObject       segment     = await segmentTask;

            if (!segmentTask.IsCompletedSuccessfully) return null;
            if (segment == null) return null;
            
            segment.transform.SetParent(null);

            UniqueId newId = UniqueId.Generate();
            SegmentData segmentData = new(index, newId);

            SegmentDataContainer container = segment.AddComponent<SegmentDataContainer>();
            container.Assign(segmentData);

            SegmentElement element = segment.AddComponent<SegmentElement>();
            element.ConfigureData();

            return element;
        } 
        #endregion
    }
}