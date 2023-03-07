namespace PathNav.PathPlanning
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "CreatorBulldozerInfo", menuName = "Scriptables/Paths/BulldozerInfo", order = 1)]
    public class CreatorBulldozerInfo : ScriptableObject
    {
        [SerializeField] private float pointSize = 0.05f;

        public float PointSize
        {
            get => pointSize;
            set => pointSize = value;
        }

        [SerializeField] private Color pointColor = Color.white;
        
    }
}
