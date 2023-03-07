namespace PathNav.Interaction
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "CursorInfo", menuName = "Scriptables/UI/CursorInfo", order = 3)]
    public class CursorInfo : ScriptableObject
    {
        [SerializeField] private Material defaultCursorMaterial;
        public Material DefaultCursorMaterial
        {
            get => defaultCursorMaterial;
            set => defaultCursorMaterial = value;
        }

        [SerializeField] private Material spawnPointCursorMaterial;
        public Material SpawnPointCursorMaterial
        {
            get => spawnPointCursorMaterial;
            set => spawnPointCursorMaterial = value;
        }

        [SerializeField]
        private Color hoverColor = Color.black;
        public Color HoverColor
        {
            get => hoverColor;
            set => hoverColor = value;
        }

        [SerializeField]
        private Color spawnColor = Color.black;
        public Color SpawnColor
        {
            get => spawnColor;
            set => spawnColor = value;
        }

        [SerializeField]
        private Color selectColor = Color.black;
        public Color SelectColor
        {
            get => selectColor;
            set => selectColor = value;
        }

        [SerializeField]
        private Color outlineColor = Color.black;
        public Color OutlineColor
        {
            get => outlineColor;
            set => outlineColor = value;
        }

        [SerializeField]
        private float offsetAlongNormal = 0.005f;

        public float OffsetAlongNormal
        {
            get => offsetAlongNormal;
            set => offsetAlongNormal = value;
        }
    }
}
