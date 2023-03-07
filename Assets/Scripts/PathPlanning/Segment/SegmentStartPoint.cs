namespace PathNav.PathPlanning
{
    using Interaction;
    using Patterns.Factory;
    using System.Threading.Tasks;
    using UnityEngine;

    public class SegmentStartPoint : MonoBehaviour, IAttachable, IPlaceable
    {
        private const string _key = "PathStartPointPrefab";

        public Vector3 Position
        {
            set => transform.position = value;
        }

        public void Hide() => _object.SetActive(false);
        public void Show() => _object.SetActive(true);

        private Factory _factory = new();
        private GameObject _prefab;
        private GameObject _object;
        private Animator _animator;
        private Transform RootTransform => transform;

        public bool Configured { get; private set; }

        private StartPointInfo _startPointInfo;
        private Vector3 Scale => _startPointInfo.StartPointScale;
        private static readonly int IsBeingPlaced = Animator.StringToHash("IsBeingPlaced");

        private void Awake()
        {
            _startPointInfo = ScriptableObject.CreateInstance<StartPointInfo>();
        }

        private async void OnEnable()
        {
            if (Configured) return;

            Task<bool> task = LoadPrefab();
            await task;

            if (!task.Result) return;

            InstantiatePrefab();
            Configured = true;
        }

        private async Task<bool> LoadPrefab()
        {
            Task<GameObject> task = _factory.LoadFromStringAsync<GameObject>(_key);
            _prefab = await task;
            return _prefab is not null;
        }

        private void InstantiatePrefab()
        {
            _object = Instantiate(_prefab, RootTransform);
            _object.transform.rotation = Quaternion.identity;
            _object.transform.position = Vector3.zero;
            _object.transform.localScale = Scale;
            _animator = _object.GetComponentInChildren<Animator>();
            _object.SetActive(true);
        }

        public void Attach(Transform t)
        {
            RootTransform.parent = t;
            RootTransform.localRotation = Quaternion.identity;
            RootTransform.localPosition = Vector3.zero;
        }

        public void Detach()
        {
            if (_animator != null)
            {
                _animator.SetBool(IsBeingPlaced, false);
            }

            RootTransform.parent = null;
            RootTransform.rotation = Quaternion.Euler(0, RootTransform.rotation.y, 0);
        }
    }
}
