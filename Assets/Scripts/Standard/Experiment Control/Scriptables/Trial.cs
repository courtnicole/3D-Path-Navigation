namespace PathNav.ExperimentControl
{
    using Interaction;
    using PathPlanning;
    using System;
    using UnityEngine;

    public enum TrialType
    {
        PathCreation,
        PathNavigation,
    }
    
    [CreateAssetMenu(fileName = "Trial", menuName = "Scriptables/Standard/Trial", order = 25)]
    public class Trial : ScriptableObject
    {
        public string trialId;
        public TrialType trialType;
        public PathStrategy pathStrategy;
        public LocomotionDof locomotionDof;

        public string GetTrialTypeString()
        {
            return trialType switch
                   {
                       TrialType.PathCreation   => "Creation",
                       TrialType.PathNavigation => "Navigation",
                       _                        => throw new ArgumentOutOfRangeException(),
                   };
        }
    }
}
