namespace PathNav.Interaction
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "RaycastVisualInfo", menuName = "Scriptables/UI/RaycastVisualInfo", order = 2)]
    public class RaycastVisualInfo : ScriptableObject
    {
        [SerializeField] private float defaultRayLength = 5f;
        public float DefaultRayLength
        {
            get => defaultRayLength;
            set => defaultRayLength = value;
        }

        [SerializeField] private float maxRayLength = 18f;
        public float MaxRayLength
        {
            get => maxRayLength;
            set => maxRayLength = value;
        }

        [SerializeField] private Material laserMaterial;
        public Material LaserMaterial
        {
            get => laserMaterial;
            set => laserMaterial = value;
        }

        [SerializeField] private Color idleColor0 = Color.white;
        public Color IdleColor0
        {
            get => idleColor0;
            set => idleColor0 = value;
        }

        [SerializeField] private Color idleColor1 = Color.white;
        public Color IdleColor1
        {
            get => idleColor1;
            set => idleColor1 = value;
        }
        
        [SerializeField] private Color hoverColor0 = Color.white;
        public Color HoverColor0
        {
            get => hoverColor0;
            set => hoverColor0 = value;
        }

        [SerializeField] private Color hoverColor1 = Color.white;
        public Color HoverColor1
        {
            get => hoverColor1;
            set => hoverColor1 = value;
        }

        [SerializeField] private Color spawnColor0 = Color.white;
        public Color SpawnColor0
        {
            get => spawnColor0;
            set => spawnColor0 = value;
        }

        [SerializeField] private Color spawnColor1 = Color.white;
        public Color SpawnColor1
        {
            get => spawnColor1;
            set => spawnColor1 = value;
        }

        [SerializeField] private Color selectColor0 = Color.blue;
        public Color SelectColor0
        {
            get => selectColor0;
            set => selectColor0 = value;
        }

        [SerializeField] private Color selectColor1 = Color.blue;
        public Color SelectColor1
        {
            get => selectColor1;
            set => selectColor1 = value;
        }
    }
}
