namespace PathNav.Interaction
{
    using UnityEngine;

    public enum LocomotionType
    {
        Default,
        FourDof,
        SixDof,
    }

    [CreateAssetMenu(fileName = "LocomotionInfo", menuName = "Scriptables/Interaction/LocomotionInfo", order = 1)]
    public class LocomotionInfo : ScriptableObject
    {
        [SerializeField, Range(1.5f, 7.0f),] private float maxVelocity = 2.05f;

        public float MaxVelocity
        {
            get => maxVelocity;
            set => maxVelocity = value;
        }

        [SerializeField, Range(-7.00f, 0.0f),] private float minVelocity = 0.35f;

        public float MinVelocity
        {
            get => minVelocity;
            set => minVelocity = value;
        }

        [SerializeField, Range(0.1f, 1.0f),] private float acceleration = 0.58f;

        public float Acceleration
        {
            get => acceleration;
            set => acceleration = value;
        }
    }
}