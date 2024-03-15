namespace ExperimentControl
{
    using System;
    
    [Flags][Serializable]
    public enum Conditions
    {
        None = 0,
        ConditionA = 1,
        ConditionB = 2,
    }

    [Serializable]
    public class SceneConditionMap
    {
        public string sceneName;
        public Conditions condition;
    }
}
