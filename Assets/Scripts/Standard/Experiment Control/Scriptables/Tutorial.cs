namespace PathNav.ExperimentControl
{
    using UnityEngine;
    [CreateAssetMenu(fileName = "Tutorial", menuName = "Scriptables/Standard/Tutorial", order = 100)]
    public class Tutorial : ScriptableObject
    {
        [Header("Scene Information")]
        public string sceneName;
    }
}
