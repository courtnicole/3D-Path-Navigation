namespace PathNav.PathPlanning
{
    using System.Threading.Tasks;
    using Patterns.Factory;
    using UnityEngine;

    public class SegmentManipulationPoint : MonoBehaviour
    {
        private const string _key = "ManipulationPointPrefab";

        public Vector3 Position
        {
            set => transform.position = value;
        }
        
        public void Hide() => _object.SetActive(false);
        public void Show() => _object.SetActive(true);

        private Factory _factory = new();
        private GameObject _prefab;
        private GameObject _object;
        private bool _pointConfigured;

        private Vector3 _scale = new(0.075f, 0.075f, 0.075f);

        private async void OnEnable()
        {
            if (_pointConfigured) return;

            Task<bool> task = LoadPrefab();
            await task;

            if (!task.Result) return;

            InstantiatePrefab();
            _pointConfigured = true;
        }

        private async Task<bool> LoadPrefab()
        {
            Task<GameObject> task = _factory.LoadFromStringAsync<GameObject>(_key);
            _prefab = await task;
            return _prefab is not null;
        }

        private void InstantiatePrefab()
        {
            _object = Instantiate(_prefab, transform);
            _object.SetActive(false);
            _object.transform.rotation   = Quaternion.identity;
            _object.transform.position   = Vector3.zero;
            _object.transform.localScale = _scale;
        }
    }
}