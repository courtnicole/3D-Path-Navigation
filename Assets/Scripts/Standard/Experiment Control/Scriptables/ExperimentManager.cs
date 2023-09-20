
namespace PathNav.ExperimentControl
{
    using System.Collections.Generic;
    using UnityEngine;

    using System;
    using UnityEngine.AddressableAssets;
    using UnityEngine.SceneManagement;
    public enum Steps
    {
        Tutorial = 1,
        Trial = 2,
        Test = 3,
        Surveys = 4,
        Complete = 5,
    }

    [CreateAssetMenu(fileName = "ExperimentManager", menuName = "Scriptables/Standard/ExperimentManager", order = 0)]
    public class ExperimentManager : ScriptableObject
    {
        public List<Condition> conditions;
        public string loadingScene;
        public int currentModelIndex = 0;
        public int currentConditionIndex = 0;
        public Steps nextStep = Steps.Tutorial;
        
        public Condition GetCondition() => conditions[currentConditionIndex];
    }
}
