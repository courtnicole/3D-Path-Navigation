namespace PathNav.Interaction
{
    using UnityEngine;

    public class PointCollider : MonoBehaviour
    {
        private PointVisualElement _pointVisualElement;

        internal void SetPointVisualElement(PointVisualElement pointVisualElement)
        {
            _pointVisualElement = pointVisualElement;
        }

        private void OnTriggerEnter(Collider other)
        {
            _pointVisualElement.ChildTriggerEnter();
        }

        private void OnTriggerExit(Collider other)
        {
            _pointVisualElement.ChildTriggerExit();
        }

    }
}
