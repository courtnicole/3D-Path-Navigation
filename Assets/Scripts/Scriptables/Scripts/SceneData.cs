namespace PathNav.ExperimentControl
{
    using UnityEngine;
    [CreateAssetMenu(fileName = "SceneData", menuName = "Scriptables/Standard/SceneData", order = 0)]
    public class SceneData : ScriptableObject
    {
        [Header("Scene Information")]
        public string sceneName;
    }
}
