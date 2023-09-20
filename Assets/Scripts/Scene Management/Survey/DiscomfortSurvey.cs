using System.Collections;
using System.Collections.Generic;


namespace PathNav.SceneManagement
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.UI;

    public class DiscomfortSurvey : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text text;
        public InputActionReference inputProvider;
        private int _value;
        private Coroutine _updateValueCoroutine;

        private void OnEnable()
        {
            _value    = (int) slider.minValue;
            text.text = _value.ToString();
            EnableActions();
        }
        private void EnableActions()
        {
            inputProvider.action.Enable();
            inputProvider.action.performed += InputPerformed;
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
            Debug.Log(_value);
        }

        public void RecordResponse()
        {
            Debug.Log(_value + " submitted");
        }
    }
}
