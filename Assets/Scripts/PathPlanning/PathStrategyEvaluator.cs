namespace PathNav.PathPlanning
{
    using Events;
    using Extensions;
    using Input;
    using Interaction;
    using Patterns.Factory;
    using System;
    using System.Collections;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    public class PathStrategyEvaluator : MonoBehaviour
    {
        [SerializeField] private AssetReferenceGameObject segmentKey;

        #region Factory Variables
        private Factory _factory = new();
        private ISegment _activeSegment;
        #endregion

        #region Start Point Variables
        private GameObject _startObject;
        private IAttachable _attachable;
        private IPlaceable _placeable;

        private bool _startPointPlaced; 
        #endregion

        #region Controller Variables
        private IController _interactingController;
        private void SetController(IController controller) => _interactingController = controller;
        private void ClearController() => _interactingController = null;
        #endregion

        #region Strategy Variables
        private PathStrategy _strategy;
        private IPathStrategy _activeStrategy;
        private IPathStrategy _bulldozerStrategy = new BulldozerStrategy();
        private IPathStrategy _spatulaStrategy = new SpatulaStrategy();
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
            EventManager.Subscribe<ControllerEvaluatorEventArgs>(EventId.SetPathStrategy, SetPathStrategy);
        }

        private void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.BeginPlacingStartPoint,  BeginPlacingStartPoint);
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.FinishPlacingStartPoint, FinishPlacingStartPoint);

            EventManager.Unsubscribe<PlacementEventArgs>(EventId.StartPointPlaced, StartPointPlaced);
            EventManager.Unsubscribe<ControllerEvaluatorEventArgs>(EventId.SetPathStrategy, SetPathStrategy);
        } 
        #endregion

        #region Event Callbacks
        private void StartPointPlaced(object sender, PlacementEventArgs args)
        {
            _activeStrategy.SetStartPosition(args.Position, -args.Heading);
            ConfigureSegment();
        }

        private void SetPathStrategy(object sender, ControllerEvaluatorEventArgs args)
        {
            _strategy = args.Strategy;
            switch (_strategy)
            {
                case PathStrategy.Bulldozer:
                    SetStrategy(_bulldozerStrategy);
                    break;
                case PathStrategy.Spatula:
                    SetStrategy(_spatulaStrategy);
                    break;
                case PathStrategy.TwoRays:
                    SetStrategy(_spatulaStrategy);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        #endregion

        #region Strategy Management
        private void SetStrategy(IPathStrategy strategy)
        {
            _activeStrategy = strategy;
        }

        private void ConfigureStrategy()
        {
            _activeStrategy.Enable();
            _activeStrategy.SubscribeToEvents();
        }

        private void ClearStrategy()
        {
            _activeStrategy.UnsubscribeToEvents();
            _activeStrategy = null;
        }
        #endregion

        #region Start Point Creation
        private void BeginPlacingStartPoint(object obj, ControllerEvaluatorEventArgs args)
        {
            if (_startPointPlaced) return;
            SetController(args.Controller);
            CreateStartPoint();
        }

        private void FinishPlacingStartPoint(object obj, ControllerEvaluatorEventArgs args)
        {
            if (!_attachable.Configured) return;

            _placeable.Place(EventId.StartPointPlaced, _startObject.transform);
            _attachable.Detach();
            _startPointPlaced = true;
            ClearController();
        }

        private void CreateStartPoint()
        {
            _startObject = new GameObject("Start Point", typeof(SegmentStartPoint));
            _attachable  = _startObject.GetComponent<SegmentStartPoint>();
            _placeable   = _startObject.GetComponent<SegmentStartPoint>();
            StartCoroutine(AttachStartPoint());
        }

        private IEnumerator AttachStartPoint()
        {
            yield return new WaitWhile(() => _attachable.Configured != true);

            _attachable?.Attach(_interactingController.AttachmentPoint);
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
            Task<GameObject> segmentTask = _factory.InstantiateObjectAsync(segmentKey, Utility.Parameterize(transform), CancellationToken.None);
            GameObject segment = await segmentTask;

            if (!segmentTask.IsCompletedSuccessfully) return null;
            if (segment == null) return null;

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