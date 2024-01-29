using UnityEngine.InputSystem;

namespace UnityEngine.XR.OpenXR.Samples.ControllerSample
{
    using System.Collections.Generic;

    public class ActionAssetEnabler : MonoBehaviour
    {
        [SerializeField] private InputActionAsset actionAsset;
        private PlayerInput input;
        public InputActionAsset ActionAsset
        {
            get => actionAsset;
            set => actionAsset = value;
        }

        private const string _uiString = "UI";
        private InputActionMap _uiMap;
        private bool _canChangeMap;
        private void OnEnable()
        {
            if (actionAsset == null) return;

            actionAsset.Enable();
            
            _uiMap        = actionAsset.FindActionMap(_uiString);
            
            if (_uiMap == null) return;

            _canChangeMap = true;
            _uiMap.Disable();
        }

        public void EnableUiInput()
        {
            if (!_canChangeMap)
            {
                Debug.LogWarning("Request to change map denied");
            };
            _uiMap.Enable();
        }

        public void DisableUiInput()
        {
            if (!_canChangeMap) return;
            _uiMap.Disable();
            
        }

    }
}
