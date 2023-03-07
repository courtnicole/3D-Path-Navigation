namespace PathNav.PathPlanning
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "SegmentInfo", menuName = "Scriptables/PathPlanning/SegmentInfo", order = 1)]
    public class SegmentInfo : ScriptableObject
    {
        [SerializeField] private Material defaultMaterial;
        public Material DefaultMaterial
        {
            get => defaultMaterial;
            set => defaultMaterial = value;
        }

        [SerializeField] private Material pathbendMaterial;
        public Material PathbendMaterial
        {
            get => pathbendMaterial;
            set => pathbendMaterial = value;
        }

        [SerializeField] private Material bulldozerDrawMaterial;
        public Material BulldozerDrawMaterial
        {
            get => bulldozerDrawMaterial;
            set => bulldozerDrawMaterial = value;
        }

        [SerializeField] private Material bulldozerEraseMaterial;
        public Material BulldozerEraseMaterial
        {
            get => bulldozerEraseMaterial;
            set => bulldozerEraseMaterial = value;
        }
    }
}
