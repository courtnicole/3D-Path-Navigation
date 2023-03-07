namespace PathNav
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "PersistentData", menuName = "Scriptables/PersistentData", order = 1)]
    public class PersistentData : ScriptableObject
    {
        [SerializeField] private Vector3 modelPose;
        public Vector3 ModelPose
        {
            get => modelPose;
            set => modelPose = value;
        }

        [SerializeField] private Vector3 modelRotation;
        public Vector3 ModelRotation
        {
            get => modelRotation;
            set => modelRotation = value;
        }

        [SerializeField] private Vector3 modelScale;
        public Vector3 ModelScale
        {
            get => modelScale;
            set => modelScale = value;
        }
    }
}
