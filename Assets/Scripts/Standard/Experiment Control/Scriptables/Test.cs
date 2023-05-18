namespace PathNav.ExperimentControl
{
    using UnityEngine;
    [CreateAssetMenu(fileName = "Test", menuName = "Scriptables/Standard/Test", order = 250)]
    public class Test : ScriptableObject
    {
        [Header("Scene Information")]
        public string sceneName;
    }
}
