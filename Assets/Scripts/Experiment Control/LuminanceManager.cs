using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PathNav.ExperimentControl
{
    using System.Collections.Generic;
    using UnityEngine;
    
    public class LuminanceManager : MonoBehaviour
    {
        [SerializeField] private ComputeShader _luminanceCompute;
        [SerializeField] private RenderTexture dstTextureObject;
        
        private static LuminanceManager _instance;
        private LuminosityPass _luminosityPass;
        private Queue<Data> _luminanceQueue;
        private ComputeBuffer _luminanceBuffer;
        private RenderTextureDescriptor _tempDescriptor;
        private RenderTargetIdentifier _dstTextureObject;

        private void OnEnable()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(_instance);
                Setup();
            }
        }

        private void Setup()
        {
            _luminanceBuffer  = new ComputeBuffer(1, sizeof(float), ComputeBufferType.Structured);
            _luminanceQueue = new Queue<Data>();
            _dstTextureObject = new RenderTargetIdentifier(dstTextureObject);
            _tempDescriptor = new RenderTextureDescriptor(256, 256)
            {
                graphicsFormat    = GraphicsFormat.R16G16B16A16_SFloat,
                autoGenerateMips  = false,
                useMipMap         = true,
                depthBufferBits   = 0,
                enableRandomWrite = true,
            };
            RenderPipelineManager.beginCameraRendering += OnBeginCamera;
            RenderPipelineManager.endContextRendering += OnEndContext;
        }
        private void OnBeginCamera(ScriptableRenderContext context, Camera renderCamera)
        {
            if (_instance == null) return;
            
            _tempDescriptor.width  = renderCamera.pixelWidth;
            _tempDescriptor.height = renderCamera.pixelHeight;
            _luminosityPass = new LuminosityPass(_tempDescriptor, _dstTextureObject, _luminanceCompute, _luminanceBuffer, _luminanceQueue, name);
            _luminosityPass.ConfigureInput(ScriptableRenderPassInput.Color);
            renderCamera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(_luminosityPass);
        }

        private async void OnEndContext(ScriptableRenderContext context, List<Camera> renderCameras)
        {
            if (_instance == null) return;
            if (ExperimentDataLogger.Instance == null) return;
            
            await ExperimentDataLogger.Instance.RecordLuminanceData(_luminanceQueue);
            _luminanceQueue.Clear();
        }

        private void OnDisable()
        {
            if (_instance == null) return;
            
            RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
            RenderPipelineManager.endContextRendering -= OnEndContext;
            _luminanceBuffer.Dispose();
            _luminanceQueue.Clear();
            
        }
    }
}
