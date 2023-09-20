namespace PathNav.ExperimentControl
{
    using Interaction;
    using UnityEngine;
    [CreateAssetMenu(fileName = "Condition", menuName = "Scriptables/Standard/Condition", order = 25)]
    public class Condition : ScriptableObject
    {
        public PathStrategy pathStrategy;
        public Model model;
    }
}
