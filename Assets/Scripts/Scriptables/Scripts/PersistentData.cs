namespace PathNav.PathPlanning
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

        [SerializeField] private Quaternion modelRotation;

        public Quaternion ModelRotation
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
        public Matrix4x4 LocalToWorld
        {
            get
            {
                Matrix4x4 localToWorld = default;
                localToWorld.SetTRS(modelPose, modelRotation, modelScale);
                return localToWorld;
            }
        }
    }
}