namespace PathNav.ExperimentControl
{
    using UnityEngine;
    using UnityEngine.Events;

    public class NavigationEndPoint : MonoBehaviour
    {
        public UnityEvent onEndReached;
        
        public void Place(Vector3 endPoint)
        {
            transform.position = endPoint;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            onEndReached?.Invoke();
        }
    }
}
