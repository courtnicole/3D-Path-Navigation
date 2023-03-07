namespace PathNav.Interaction
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "RaycastInfo", menuName = "Scriptables/UI/RaycastInfo", order = 1)]
    public class RaycastInfo : ScriptableObject
    {
        [SerializeField, Range(0.04f, 0.11f),] private float raycastSphereSize = 0.08f;
        public float RaycastSphereSize
        {
            get => raycastSphereSize;
            set => raycastSphereSize = value;
        }

        [SerializeField] private float maxRaycastDistance = 18f;
        public float MaxRaycastDistance
        {
            get => maxRaycastDistance;
            set => maxRaycastDistance = value;
        }
    }
}
