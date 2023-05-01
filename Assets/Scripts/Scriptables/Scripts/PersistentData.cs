namespace PathNav.PathPlanning
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "PersistentData", menuName = "Scriptables/PersistentData", order = 1)]
    public class PersistentData : ScriptableObject
    {
        [SerializeField] private Vector3 deltaTranslation;
        public Vector3 DeltaTranslation
        {
            get => deltaTranslation;
            set => deltaTranslation = value;
        }

        [SerializeField] private Vector3 deltaRotation;

        public Vector3 DeltaRotation
        {
            get => deltaRotation;
            set => deltaRotation = value;
        }

        [SerializeField] private float deltaScale;
        public float DeltaScale
        {
            get => deltaScale;
            set => deltaScale = value;
        }
    }
}