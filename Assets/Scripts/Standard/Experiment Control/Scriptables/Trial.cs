namespace PathNav.ExperimentControl
{
    using UnityEngine;

    public enum PlacementMethod
    {
        Drawing,
        Spatula,
        Free,
    }

    [CreateAssetMenu(fileName = "Trial", menuName = "Scriptables/Standard/Trial", order = 200)]
    public class Trial : ScriptableObject
    {
        [Header("Trial Information")] [SerializeField]
        private PlacementMethod placementMethod;

        public PlacementMethod PlacementMethod => placementMethod;

        [Header("Scene Information")]
        public string sceneName;
    }
}