using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PathNav.ExperimentControl
{
    using System.Collections.Generic;
    using UnityEngine;
    
    public class LuminanceManager : MonoBehaviour
    {
        [SerializeField] private ComputeShader luminanceCompute;
        [SerializeField] private RenderTexture dstTextureObject;
        
        private static LuminanceManager _instance;
        private LuminosityPass _luminosityPass;
        private LuminosityComputePass _luminosityPass2;
        private Queue<Data> _luminanceQueue;
        private Data _luminanceData;
        private ComputeBuffer _luminanceBuffer;
        private RenderTextureDescriptor _tempDescriptor;
        private RenderTargetIdentifier _dstTextureObject;
        private bool _queuePassNextBegin;
        private bool _recordDataThisPass;
        
        private void OnEnable()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(_instance);
                Setup();
                _queuePassNextBegin = false;
            }
        }
        
        protected void LateUpdate()
        {
            _queuePassNextBegin = true;
        }

        private void Setup()
        {
            //Vive: 1160x578
            _luminanceBuffer  = new ComputeBuffer(1,      sizeof(float), ComputeBufferType.Structured);
            _luminanceQueue   = new Queue<Data>();
            _luminanceData    = new Data();
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
            if (_queuePassNextBegin == false) return;
            _queuePassNextBegin    = false;
            _recordDataThisPass    = true;

            
            //_tempDescriptor.width  = renderCamera.pixelWidth;
            //_tempDescriptor.height = renderCamera.pixelHeight;
            _luminosityPass        = new LuminosityPass(_tempDescriptor, _dstTextureObject, luminanceCompute, _luminanceBuffer, _luminanceData, _luminanceQueue, name);
            _luminosityPass.ConfigureInput(ScriptableRenderPassInput.Color);
            renderCamera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(_luminosityPass);
        }

        private void OnEndContext(ScriptableRenderContext context, List<Camera> renderCameras)
        {
            if (_instance == null) return;
            if (ExperimentDataLogger.Instance == null) return;
            if (_recordDataThisPass == false) return;
            _recordDataThisPass = false;
            
            ExperimentDataLogger.Instance.RecordGazeData();
            ExperimentDataLogger.Instance.RecordLuminanceData(_luminanceData);
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


// private void SetupLuminosityComputePass()
// {
//     _luminanceBuffer  = new ComputeBuffer(1, sizeof(float) * 3, ComputeBufferType.Structured);
//     _luminanceQueue   = new Queue<Data>();
//     _dstTextureObject = new RenderTargetIdentifier(dstTextureObject);
//     _tempDescriptor = new RenderTextureDescriptor(256, 256)
//     {
//         graphicsFormat    = GraphicsFormat.R16G16B16A16_SFloat,
//         autoGenerateMips  = false,
//         useMipMap         = true,
//         depthBufferBits   = 0,
//         enableRandomWrite = true,
//     };
//     RenderPipelineManager.beginCameraRendering += OnBeginCamera2;
//     RenderPipelineManager.endContextRendering  += OnEndContext2;
// }
//         
// private void OnBeginCamera2(ScriptableRenderContext context, Camera renderCamera)
// {
//     if (_instance                     == null) return;
//     if (ExperimentDataLogger.Instance == null) return;
//             
//     _tempDescriptor.width  = renderCamera.pixelWidth;
//     _tempDescriptor.height = renderCamera.pixelHeight;
//             
//     _luminosityPass2        = new LuminosityComputePass(_tempDescriptor, _dstTextureObject, luminanceCompute, _luminanceBuffer, _luminanceQueue, ExperimentDataLogger.Instance.CurrentCombinedGazeDirection, name);
//     _luminosityPass2.ConfigureInput(ScriptableRenderPassInput.Color);
//     renderCamera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(_luminosityPass2);
// }
//
// private void OnEndContext2(ScriptableRenderContext context, List<Camera> renderCameras)
// {
//     if (_instance                     == null) return;
//     if (ExperimentDataLogger.Instance == null) return;
//             
//     ExperimentDataLogger.Instance.RecordLuminanceData(_luminanceQueue);
//     _luminanceQueue.Clear();
// }