namespace PathNav.ExperimentControl
{
    using UnityEngine;
    using UnityEngine.Events;

    public class NavigationEndPoint : MonoBehaviour
    {
        public UnityEvent onEndReached;
        
        private bool _isPlaced;
        
        public void Place(Vector3 endPoint)
        {
            transform.position = endPoint;
            _isPlaced = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isPlaced) return;
            if (!other.gameObject.CompareTag("Player")) return;
            onEndReached?.Invoke();
        }
    }
}
