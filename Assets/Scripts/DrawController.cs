using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathNav
{
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.XR;

    public class DrawController : MonoBehaviour
    {
        [SerializeField] private ComputeShader drawComputeShader;
        [SerializeField] private TrackedPoseDriver controllerPose;
        
        public InputActionReference triggerClick;

        private RenderTexture _canvasRenderTexture;
        
        private bool _drawingEnabled;
        
        private int _updateKernel;
        private int _initBackgroundKernel;
        private Vector3 _previousPosition;
        
        private Vector3 Position => controllerPose.transform.position;

        private void OnEnable()
        {
            _drawingEnabled      = false;
            _updateKernel         = drawComputeShader.FindKernel("draw");
            _initBackgroundKernel = drawComputeShader.FindKernel("init");
            
            _canvasRenderTexture                   = new RenderTexture(Screen.width, Screen.height, 24);
            _canvasRenderTexture.filterMode        = FilterMode.Point;
            _canvasRenderTexture.enableRandomWrite = true;
            _canvasRenderTexture.Create();
            
            drawComputeShader.SetTexture(_initBackgroundKernel, "canvas", _canvasRenderTexture);
            drawComputeShader.GetKernelThreadGroupSizes(_initBackgroundKernel,
                                                        out uint xGroupSize, out uint yGroupSize, out _);
            drawComputeShader.Dispatch(_initBackgroundKernel,  Mathf.CeilToInt(_canvasRenderTexture.width / (float) xGroupSize),
                                       Mathf.CeilToInt(_canvasRenderTexture.height                        / (float) yGroupSize),
                                       1);
            
            EnableActions();
            _previousPosition = Position;
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
        }

        private void Update()
        {
            drawComputeShader.SetBool("is_drawing", _drawingEnabled);
            drawComputeShader.SetVector("position", Position);
            drawComputeShader.SetVector("position", Position);
            drawComputeShader.SetTexture(_updateKernel, "canvas", _canvasRenderTexture);
            drawComputeShader.Dispatch(_updateKernel, _canvasRenderTexture.width / 8,
                                       _canvasRenderTexture.height              / 8, 1);
            _previousPosition = Position;
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Graphics.Blit(_canvasRenderTexture, dest);
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
