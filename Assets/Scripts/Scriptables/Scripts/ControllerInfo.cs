namespace PathNav.Interaction
{
    using UnityEngine;

    public enum ControllerType
    {
        None,
        Vive,
        Oculus,
    }

    [CreateAssetMenu(fileName = "ControllerInfo", menuName = "Scriptables/Input/ControllerInfo", order = 1)]
    public class ControllerInfo : ScriptableObject
    {
        [SerializeField] private float laserAngleOffsetDegrees = -45f;
        public float LaserAngleOffsetDegrees
        {
            get => laserAngleOffsetDegrees;
            set => laserAngleOffsetDegrees = value;
        }
        public float LaserAngleOffsetRadians
        {
            get => laserAngleOffsetDegrees * Mathf.Deg2Rad;
            set => laserAngleOffsetDegrees = value * Mathf.Rad2Deg;
        }

        [SerializeField] private float wandAngleOffsetDegrees = 145f;
        public float WandAngleOffsetDegrees
        {
            get => wandAngleOffsetDegrees;
            set => wandAngleOffsetDegrees = value;
        }
        public float WandAngleOffsetRadians
        {
            get => wandAngleOffsetDegrees * Mathf.Deg2Rad;
            set => wandAngleOffsetDegrees = value * Mathf.Rad2Deg;
        }

        [SerializeField] private ControllerType controllerType = ControllerType.Vive;
        public ControllerType ControllerType
        {
            get => controllerType;
            set => controllerType = value;
        }
    }
}
