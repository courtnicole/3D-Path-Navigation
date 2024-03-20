
namespace PathNav.SceneManagement
{
    using Events;
    using ExperimentControl;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    public class SeqSurvey : MonoBehaviour
    {
        [SerializeField] private ToggleGroup toggleGroup;
        private int _value;
        
        public void RecordValue()
        {
            IEnumerable<Toggle> toggles = toggleGroup.ActiveToggles();
            
            if (toggles is null) return;

            toggles = toggles.ToList();

            if(toggles.Any()) 
                _value    = int.Parse(toggles.First().name);
        }

        public void RecordResponse()
        {
            ExperimentDataLogger.Instance.RecordSurveyData("SEQ", _value.ToString());
            EventManager.Publish(EventId.SeqComplete, this, new SceneControlEventArgs());
        }
    }
}
