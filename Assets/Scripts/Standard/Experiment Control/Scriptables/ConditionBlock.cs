
namespace PathNav.ExperimentControl
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    [CreateAssetMenu(fileName = "ConditionBlock", menuName = "Scriptables/Standard/ConditionBlock", order = 0)]
    public class ConditionBlock : ScriptableObject
    {
        [SerializeField] private Trial[] trials;
        [SerializeField] private Model[] models;
        [SerializeField] private ModelOrder[] modelOrder;
        [SerializeField] private Scene[] scenes;

        public string conditionId;
        public Trial GetCurrentTrial(int index) => trials[index];

        public Model GetCurrentModel(int trialIndex, int modelIndex) => models[modelOrder[trialIndex].ModelIndex[modelIndex]];
    }
}
