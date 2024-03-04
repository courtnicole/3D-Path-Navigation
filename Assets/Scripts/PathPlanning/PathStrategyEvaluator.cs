namespace PathNav.PathPlanning
{
    using Events;
    using ExperimentControl;
    using Extensions;
    using Input;
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
        [SerializeField] private Renderer model;
        [SerializeField] private PlacementPlaneElement placementPlaneLeft;
        [SerializeField] private PlacementPlaneElement placementPlaneRight;
        [SerializeField] private Transform startPointTransform1;
        [SerializeField] private Transform startPointTransform2;
        [SerializeField] private GameObject targetPoints1;
        [SerializeField] private GameObject targetPoints2;

        #region Factory Variables
        private ISegment _activeSegment;
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
        private PlacementPlaneElement _placementPlane;
        private Bounds _bounds;
        private Transform _startPointTransform;
        #endregion

        #region Unity Events
        private void OnEnable()
        {
            Enable();
            
        }

        private void OnDisable()
        {
            _activeStrategy?.Disable();
            Disable();
        }

        private void Start()
        {
            EventManager.Publish(EventId.RegisterStartPoint, this, new SceneControlEventArgs());
        }

        private void Update()
        {
            _activeStrategy?.Run();
        }
        #endregion

        #region Initialization
        private void Enable()
        {
            
            placementPlaneLeft.Disable();
            placementPlaneRight.Disable();
            _bounds = model.bounds;
            bool useTargetPoints1 = ExperimentDataManager.Instance != null ? ExperimentDataManager.Instance.UseTargetPoints1() : true;
            _startPointTransform = useTargetPoints1 ? startPointTransform1 : startPointTransform2;
            if (useTargetPoints1)
            {
                targetPoints1.SetActive(true);
                targetPoints2.SetActive(false);
            }
            else
            {
                targetPoints1.SetActive(false);
                targetPoints2.SetActive(true);
            }
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
            EventManager.Subscribe<SceneControlEventArgs>(EventId.RegisterStartPoint, RegisterStartPoint);
            EventManager.Subscribe<SceneControlEventArgs>(EventId.SetPathStrategy, SetPathStrategy);
        }

        private void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<SceneControlEventArgs>(EventId.RegisterStartPoint, RegisterStartPoint);
            EventManager.Unsubscribe<SceneControlEventArgs>(EventId.SetPathStrategy, SetPathStrategy);
        } 
        #endregion

        #region Event Callbacks
        private void RegisterStartPoint(object sender, SceneControlEventArgs args)
        {
            if (!_strategySet) return;
            _activeStrategy.SetStartPosition(_startPointTransform.position, _startPointTransform.forward);
            ConfigureSegment();
        }

        private void SetPathStrategy(object sender, SceneControlEventArgs args)
        {
            _strategy       = args.Strategy;
            _placementPlane = args.Handedness == Handedness.Left ? placementPlaneRight : placementPlaneLeft;
            
            switch (_strategy)
            {
                case PathStrategy.Bulldozer:
                    _bulldozerStrategy = new BulldozerStrategy(_bounds);
                    SetStrategy(_bulldozerStrategy);
                    break;
                case PathStrategy.Spatula:
                    _spatulaStrategy = new SpatulaStrategy(_placementPlane, _bounds);
                    InitializeSpatulaStrategy();
                    SetStrategy(_spatulaStrategy);
                    break;
                case PathStrategy.None:
                    break;
                default:
                    _bulldozerStrategy = new BulldozerStrategy(_bounds);
                    InitializeBulldozerStrategy();
                    SetStrategy(_bulldozerStrategy);
                    break;
            }
        }
        #endregion

        #region Strategy Management

        private void InitializeSpatulaStrategy()
        {
            _placementPlane.Enable();
        }

        private void InitializeBulldozerStrategy()
        {
            _placementPlane.Disable();
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
            _activeStrategy.Disable();
            _activeStrategy = null;
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