
namespace PathNav.SceneManagement
{
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
            _value = int.Parse(toggles.First().name);
            Debug.Log(_value);
        }

        public void RecordResponse()
        {
            Debug.Log(_value + " submitted");
        }
    }
}
