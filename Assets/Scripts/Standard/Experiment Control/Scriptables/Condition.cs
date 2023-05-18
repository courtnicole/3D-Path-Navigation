namespace PathNav.ExperimentControl
{
    using UnityEngine;
    [CreateAssetMenu(fileName = "Condition", menuName = "Scriptables/Standard/Condition", order = 25)]
    public class Condition : ScriptableObject
    {
        public Tutorial tutorial;
        public Trial trial;
        public Test test;
        public Survey survey;

        public bool showTutorial;
    }
}
