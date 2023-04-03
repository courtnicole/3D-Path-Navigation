namespace PathNav.PathPlanning
{
    using Dreamteck.Splines;
    using Events;
    using Extensions;
    using Interaction;
    using Patterns.Factory;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

    public class SegmentElement : MonoBehaviour, ISegment, IColorChangeable, IBulldozable
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

        public IData Data { get; private set; }

        public int CurrentPointCount => Spline.pointCount;

        private SplinePoint[] CurrentPoints
        {
            get => Spline.GetPoints();
            set => Spline.SetPoints(value);
        }

        public void AddFirstPoint(Vector3 newPosition, Vector3 heading)
        {
            Vector3 end = newPosition + (heading.normalized * 0.01f);

            SplinePoint pt1 = new()
            {
                color    = Color.white,
                normal   = Vector3.up,
                size     = 0.01f,
                tangent  = default,
                tangent2 = default,
                position = newPosition,
            };

            SplinePoint pt2 = new()
            {
                color    = Color.white,
                normal   = Vector3.up,
                size     = 0.01f,
                tangent  = default,
                tangent2 = default,
                position = end,
            };

            var points = new SplinePoint[2];
            points[0] = pt1;
            points[1] = pt2;

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

            newPoints[^1] = newPoints[^2];
            newPoints[^1].SetPosition(position);

            CurrentPoints = newPoints;

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
            SplinePoint[] oldPoints = CurrentPoints;
            var           newPoints = new SplinePoint[CurrentPointCount - 1];

            for (int i = 0; i < newPoints.Length; i++)
            {
                if (i == index) continue;

                newPoints[i] = oldPoints[i];
            }

            CurrentPoints = newPoints;

            if (!_useNodeVisuals) return;

            RemovePointVisual(index);
        }

        public void RemovePoint()
        {
            SplinePoint[] oldPoints = CurrentPoints;
            var           newPoints = new SplinePoint[CurrentPointCount - 1];

            for (int i = 0; i < newPoints.Length; i++)
            {
                newPoints[i] = oldPoints[i];
            }

            CurrentPoints = newPoints;

            if (!_useNodeVisuals) return;

            RemovePointVisual(oldPoints.Length - 1);
        }

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

            _triggeredIndex = -1;

            EnablePointVisuals();
            SubscribeToEvents();
        }

        public int CompareTo(IPathElement other) => Index.CompareTo(other.Index);

        public bool IsCloseToPoint(out int pointIndex)
        {
            pointIndex = _triggeredIndex;
            return _triggeredIndex > -1;
        }

        public void SaveSpline()
        {
            if (SceneDataManager.Instance != null)
            {
                SceneDataManager.Instance.SaveSplineComputer(Spline);
                EmitSegmentEvent(EventId.SegmentComplete);
            }
            else
            {
                throw new Exception("SceneDataManager is null!");
            }
        }
        #endregion

        #region INodeVisuals
        private List<PointVisualElement> _pointVisuals = new();
        private Dictionary<int, int> _pointVisualsIdIndexMap = new();
        private Factory _factory = new();

        private const string _key = "NodeVisual";
        private GameObject _pointPrefab;

        private int _triggeredIndex;

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
            if (CurrentPoints.Length < 2) return;

            for (int i = 0; i < CurrentPoints.Length; i++)
            {
                AddPointVisual(i);
            }

            _pointVisuals[0].Hide();
            _pointVisuals[1].Hide();
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

        private void RemovePointVisual(int id)
        {
            bool result = _pointVisualsIdIndexMap.TryGetValue(id, out int index);
            if (!result) return;

            _pointVisualsIdIndexMap.Remove(id);
            Destroy(_pointVisuals[index]);
            _pointVisuals.RemoveAt(index);

            UpdateIdIndexMap();
        }

        private void UpdateIdIndexMap()
        {
            foreach (PointVisualElement element in _pointVisuals)
            {
                int id = element.Id.ID;

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

            _triggeredIndex = index;
        }

        private void PointVisualUntriggered(object sender, PointVisualEventArgs args)
        {
            _triggeredIndex = -1;
        }

        private async Task<bool> LoadPrefab()
        {
            Task<GameObject> task = _factory.LoadFromStringAsync<GameObject>(_key);
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