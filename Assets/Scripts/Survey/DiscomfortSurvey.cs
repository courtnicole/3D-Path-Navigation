namespace PathNav.SceneManagement
{
    using System.Collections;
    using Events;
    using ExperimentControl;
    using TMPro;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.UI;

    public class DiscomfortSurvey : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text text;
        private int _value;
        private Coroutine _updateValueCoroutine;

        private void OnEnable()
        {
            _value    = (int) slider.minValue;
            text.text = _value.ToString();
        }

        private void InputPerformed(InputAction.CallbackContext obj)
        {
            if (_updateValueCoroutine != null) return;
            float change = obj.ReadValue<Vector2>().x;
            _updateValueCoroutine = StartCoroutine(UpdateValue(change));
        }
        
        private IEnumerator UpdateValue(float change)
        {
            float sign = Mathf.Sign(change);
            float temp = slider.value + sign;
            slider.value = Mathf.Clamp(temp, slider.minValue, slider.maxValue);
            _value       = (int)slider.value;
            text.text    = _value.ToString();
            
            yield return new WaitForSeconds(0.5f);
            
            _updateValueCoroutine = null;
        }

        public void RecordValue()
        {
            _value = (int)slider.value;
            text.text = _value.ToString();
        }

        public void RecordResponse()
        {
            ExperimentDataManager.Instance.RecordDiscomfortScore(_value);
            EventManager.Publish(EventId.DiscomfortScoreComplete, this, new SceneControlEventArgs());
        }
    }
}
