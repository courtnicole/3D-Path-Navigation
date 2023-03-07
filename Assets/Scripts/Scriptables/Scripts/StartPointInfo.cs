namespace PathNav.Interaction
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "StartPointInfo", menuName = "Scriptables/Interaction/StartPointInfo", order = 0)]
    public class StartPointInfo : ScriptableObject
    {
        [SerializeField] private Vector3 startPointScale = new (0.01f, 0.01f, 0.01f);

        public Vector3 StartPointScale
        {
            get => startPointScale;
            set => startPointScale = value;
        }
    }
}
