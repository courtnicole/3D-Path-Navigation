namespace PathNav.PathPlanning
{
    using Dreamteck.Splines;
    using Events;
    using ExperimentControl;
    using Extensions;
    using Input;
    using Interaction;
    using Patterns.Factory;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

    public class SegmentElement : MonoBehaviour, ISegment, IColorChangeable
    {
        #region Local Variables
        public GameObject Root => gameObject;
        public Transform RootTransform => Root.transform;

        private bool _configured;
        private SegmentInfo _segmentInfo;
        public SplineComputer Spline { get; private set; }

        private bool _useNodeVisuals;
        #endregion

        #region Unity Methods
        private void OnEnable()
        {
            if (_configured) Enable();
        }

        private void OnDisable()
        {
            UnsubscribeToEvents();
            Disable();
        }
        #endregion

        #region Manage Event Subscriptions
        private void SubscribeToEvents()
        {
            EventManager.Subscribe<PointVisualEventArgs>(EventId.PointVisualTriggered,   PointVisualTriggered);
            EventManager.Subscribe<PointVisualEventArgs>(EventId.PointVisualUntriggered, PointVisualUntriggered);
        }

        private void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<PointVisualEventArgs>(EventId.PointVisualTriggered,   PointVisualTriggered);
            EventManager.Unsubscribe<PointVisualEventArgs>(EventId.PointVisualUntriggered, PointVisualUntriggered);
        }
        #endregion

        #region ISegment Methods
        public UniqueId Id => Data.Id;
        public int Index => Data.Index;

        private const float _width = 0.005f;

        public IData Data { get; private set; }

        public int CurrentPointCount => Spline.pointCount;
        public int SelectedPointVisualIndex { get; private set; }

        public int SelectedSegmentIndex { get; private set; }

        private SplinePoint[] CurrentPoints
        {
            get => Spline.GetPoints();
            set => Spline.SetPoints(value);
        }

        #region Point Addition/Removal
        public void AddFirstPoint(Vector3 newPosition, Vector3 heading)
        {
            //Vector3 end = newPosition + (heading.normalized * 0.01f);

            SplinePoint pt1 = new()
            {
                type     = SplinePoint.Type.SmoothFree,
                color    = Color.white,
                normal   = Vector3.up,
                size     = _width,
                tangent  = Vector3.forward,
                position = newPosition,
            };

            // SplinePoint pt2 = new()
            // {
            //     type     = SplinePoint.Type.SmoothFree,
            //     color    = Color.white,
            //     normal   = Vector3.up,
            //     size     = _width,
            //     tangent  = Vector3.forward,
            //     position = end,
            // };

            var points = new SplinePoint[1];
            points[0] = pt1;
            //points[1] = pt2;

            CurrentPoints = points;
        }

        public void AddPoint(Vector3 position)
        {
            SplinePoint[] oldPoints = CurrentPoints;
            var           newPoints = new SplinePoint[CurrentPointCount + 1];

            for (int i = 0; i < CurrentPointCount; i++)
            {
                newPoints[i] = oldPoints[i];
            }

            newPoints[^1]      = new SplinePoint(position);
            newPoints[^1].size = _width;
            CurrentPoints      = newPoints;

            if (!_useNodeVisuals) return;

            AddPointVisual(CurrentPoints.Length - 1);
        }

        public void MovePoint(int pointIndex, Vector3 newPosition)
        {
            SplinePoint[] points = CurrentPoints;
            points[pointIndex].SetPosition(newPosition);
            CurrentPoints = points;

            if (!_useNodeVisuals) return;

            MovePointVisual(pointIndex);
        }

        public void RemovePoint(int index)
        {
            if (_useNodeVisuals)
            {
                RemovePointVisual(index);
            }
            
            SplinePoint[] oldPoints = CurrentPoints;
            var           newPoints = new SplinePoint[CurrentPointCount - 1];

            for (int i = 0, j=0; i < oldPoints.Length; i++)
            {
                if (i == index) continue;

                newPoints[j] = oldPoints[i];
                j++;
            }

            CurrentPoints = newPoints;
        }

        public void RemovePoint()
        {
            if (_useNodeVisuals)
            {
                RemovePointVisual(CurrentPointCount - 1);
            }
            
            SplinePoint[] oldPoints = CurrentPoints;
            var           newPoints = new SplinePoint[CurrentPointCount - 1];
            
            for (int i = 0; i < newPoints.Length; i++)
            {
                newPoints[i] = oldPoints[i];
            }

            CurrentPoints = newPoints;
        }
        #endregion

        #region Configuration
        public bool Configure()
        {
            if (_configured) return true;

            _configured = true;

            Enable();
            EmitSegmentEvent(EventId.SegmentConfigured);
            return true;
        }

        public void ConfigureData()
        {
            if (!gameObject.TryGetComponent(out SegmentDataContainer container)) return;

            Data = container.SegmentData;

            if (!gameObject.TryGetComponent(out SegmentInfoContainer info)) return;

            _segmentInfo = info.Info;
            Spline       = info.Spline;
            
            if (!gameObject.GetComponent<InteractableElement>()) return;

            gameObject.GetComponent<InteractableElement>().Id = Id;
        }

        public void ConfigureNodeVisuals(PathStrategy strategy)
        {
            if (strategy == PathStrategy.Bulldozer) return;

            SelectedPointVisualIndex = -1;

            EnablePointVisuals();
            SubscribeToEvents();
        }

        public void SaveSpline()
        {
            if (ExperimentDataManager.Instance != null)
            {
                ExperimentDataManager.Instance.SaveSplineComputer(Spline);
                EmitSegmentEvent(EventId.SegmentComplete);
            }
            else
            {
                throw new Exception("ExperimentDataManager is null!");
            }
        }
        #endregion

        #region Logic
        public bool IsCloseToPoint(out int pointIndex)
        {
            pointIndex = SelectedPointVisualIndex;
            return SelectedPointVisualIndex > -1;
        }

        public bool CanErasePoint(ref IController controller)
        {
            if (CurrentPointCount           < 3) return false;
            if (controller?.CollisionBounds == null) return false;
            if (controller is null) return false;

            bool isInsideBounds = controller.CollisionBounds.Contains(CurrentPoints[CurrentPointCount - 1].position);
            SelectedSegmentIndex = isInsideBounds ? CurrentPointCount - 1 : -1;
            return isInsideBounds;
        }

        public int CompareTo(IPathElement other) => Index.CompareTo(other.Index);
        #endregion
        #endregion

        #region INodeVisuals
        private List<PointVisualElement> _pointVisuals = new();
        private Dictionary<int, int> _pointVisualsIdIndexMap = new();
        private Factory _factory = new();

        private const string _nodeKey = "NodeVisual";
        private GameObject _pointPrefab;

        private int _selectedId;

        private async void EnablePointVisuals()
        {
            Task<bool> task = LoadPrefab();
            await task;

            if (!task.Result) return;

            CalculatePointVisuals();
            _useNodeVisuals = true;
        }

        private void CalculatePointVisuals()
        {
            if (CurrentPoints.Length < 1) return;

            for (int i = 0; i < CurrentPoints.Length; i++)
            {
                AddPointVisual(i);
            }

            _pointVisuals[0].Hide();
            //_pointVisuals[1].Hide();
        }

        private void AddPointVisual(int index)
        {
            GameObject pointVisual = InstantiatePrefab(CurrentPoints[index].position);

            UniqueId        newId           = UniqueId.Generate();
            PointVisualData pointVisualData = new(index, newId);

            PointVisualDataContainer container = pointVisual.AddComponent<PointVisualDataContainer>();
            container.Assign(pointVisualData);

            PointVisualElement element = pointVisual.AddComponent<PointVisualElement>();
            element.ConfigureData();

            element.Configure();

            _pointVisuals.Add(element);
            _pointVisualsIdIndexMap.Add(element.Id.ID, index);
        }

        private void RemovePointVisual(int index)
        {
            bool result = _pointVisualsIdIndexMap.TryGetValue(_selectedId, out int selectedIndex);
            if (!result) return;
            if (selectedIndex != index) return;
            
            GameObject selectedVisual = _pointVisuals[index].gameObject;
            Destroy(selectedVisual);
            _pointVisualsIdIndexMap.Remove(_selectedId);
            _pointVisuals.RemoveAt(index);
            
            
            _selectedId              = -1;
            SelectedPointVisualIndex = -1;

            UpdateIdIndexMap();
        }

        private void UpdateIdIndexMap()
        {
            for (int index = 0; index < _pointVisuals.Count; index++)
            {
                PointVisualElement element = _pointVisuals[index];
                int                id      = element.Id.ID;
                element.UpdateIndex(index);
                
                if (_pointVisualsIdIndexMap.ContainsKey(id))
                {
                    _pointVisualsIdIndexMap[id] = element.Index;
                }
            }
        }

        private void MovePointVisual(int index)
        {
            _pointVisuals[index].Move(CurrentPoints[index].position);
        }

        private void PointVisualTriggered(object sender, PointVisualEventArgs args)
        {
            int id = args.Id.ID;
            if (!_pointVisualsIdIndexMap.TryGetValue(id, out int index)) return;

            _selectedId              = id;
            SelectedPointVisualIndex = index;
        }

        private void PointVisualUntriggered(object sender, PointVisualEventArgs args)
        {
            _selectedId              = -1;
            SelectedPointVisualIndex = -1;
        }

        private async Task<bool> LoadPrefab()
        {
            Task<GameObject> task = _factory.LoadFromStringAsync<GameObject>(_nodeKey);
            _pointPrefab = await task;
            return _pointPrefab is not null;
        }

        private GameObject InstantiatePrefab(Vector3 position)
        {
            GameObject node = Instantiate(_pointPrefab, position, Quaternion.identity);
            node.SetActive(true);
            return node;
        }
        #endregion

        #region Event Logic
        private void Enable()
        {
            EmitSegmentEvent(EventId.SegmentEnabled);
        }

        private void Disable()
        {
            EmitSegmentEvent(EventId.SegmentDisabled);
        }

        private void EmitSegmentEvent(EventId id)
        {
            EventManager.Publish(id, this, GetSegmentEventArgs());
        }

        private SegmentEventArgs GetSegmentEventArgs() => new(this);
        #endregion

        #region Implementation of IColorChangeable
        internal IColorChangeable ColorChangeable => this;
        public Renderer Renderer => Root.GetComponent<Renderer>();

        internal Material DefaultMaterial => _segmentInfo.DefaultMaterial;
        internal Material PathbendMaterial => _segmentInfo.PathbendMaterial;
        internal Material BulldozerDrawMaterial => _segmentInfo.BulldozerDrawMaterial;
        internal Material BulldozerEraseMaterial => _segmentInfo.BulldozerEraseMaterial;
        #endregion
    }
}