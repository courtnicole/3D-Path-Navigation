namespace PathNav.Interaction
{
    using UnityEngine;
    public class IgnoreParent : MonoBehaviour
    {
        [SerializeField] private Transform parent;
        private Vector3 _euler;
        private Transform _transform;
        private void Start()
        {
            _euler = new Vector3();
            _transform = transform;
        }
        private void Update()
        {
            _transform.position    = parent.position;
            _euler.y               = parent.eulerAngles.y;
            _transform.eulerAngles = _euler;
        }
    }
}
