namespace ExperimentControl
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    [CreateAssetMenu(fileName = "SceneOrder", menuName = "Scriptables/Experiment/SceneOrder", order = 1)]
    public class SceneOrder : ScriptableObject
    {
        [SerializeField] private Conditions condition;
        public Conditions Condition
        {
            get => condition;
            set => condition = value;
        }
        
        [SerializeField] private string[] scenesToRun;
        public string[] ScenesToRun => scenesToRun;
    }
}
