
namespace ExperimentControl
{
    using UnityEngine;
    public class ExperimentInfo : ScriptableObject
    {
        private Handedness _handedness;
        private int _participantID;
        private int _conditionId;
        
        public ExperimentInfo(Handedness handedness, int participantID)
        {
            _handedness = handedness;
            _participantID = participantID;
        }
    }
}
