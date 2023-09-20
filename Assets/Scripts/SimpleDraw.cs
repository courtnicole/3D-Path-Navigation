using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathNav
{
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.XR;
    using UnityEngine.UIElements;

    public class SimpleDraw : MonoBehaviour
    {
        [SerializeField] private TrackedPoseDriver controllerPose;
        
        public InputActionReference triggerClick;
        private static readonly int CurrentRotation = Shader.PropertyToID("_CurrentRotation");
        private static readonly int CurrentPosition = Shader.PropertyToID("_CurrentPosition");
        
        private bool _drawingEnabled;
        private Vector3 Position => controllerPose.transform.position;
        private Vector3 Rotation => controllerPose.transform.rotation.eulerAngles;
         private void OnEnable()
        {
            EnableActions();
        }

        private void OnDisable()
        {
            DisableActions();
        }

        private void EnableActions()
        {
            triggerClick.action.Enable();
            triggerClick.action.performed += TriggerDown;
            triggerClick.action.canceled  += TriggerUp;
        }

        private void DisableActions()
        {
            triggerClick.action.performed -= TriggerDown;
            triggerClick.action.canceled  -= TriggerUp;
        }

        private void Update()
        {
            if (!_drawingEnabled) return;
            //raycast from controller to transform
            if (Physics.Raycast(Position, Rotation, out RaycastHit hit))
            {
                Shader.SetGlobalVector(CurrentPosition, hit.textureCoord);
                Shader.SetGlobalVector(CurrentRotation, Rotation);
            }
        }

        private void TriggerUp(InputAction.CallbackContext obj)
        {
            _drawingEnabled = false;
        }

        private void TriggerDown(InputAction.CallbackContext obj)
        {
            _drawingEnabled = true;
        }
        
    }
}
