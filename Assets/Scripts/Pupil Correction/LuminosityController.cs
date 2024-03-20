using UnityEngine.InputSystem.XR;
namespace PathNav.ExperimentControl
{
    using Unity.Collections;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.XR;

    public class LuminosityController : MonoBehaviour
    {
        public ComputeShader luminanceComputeShader;
        public RenderTexture permanentTexture;

        [Header("Data Logging Variables")]
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;
        [SerializeField] private TrackedPoseDriver headPoseDriver;
        [SerializeField] private TrackedPoseDriver leftHandPoseDriver;
        [SerializeField] private TrackedPoseDriver rightHandPoseDriver;

        private RenderTexture _tempTexture;

        private int _linearToXyzKernel;
        private int _luminanceKernel;

        private int _width, _height;
        private int _threadsX, _threadsY;
        private int _groupSizeX, _groupSizeY;

        private GraphicsBuffer _luminanceBuffer;
        private NativeArray<float> _outputBuffer;
        private float _dataToLog;
        private AsyncGPUReadbackRequest _request;

        private bool _blitLuminance;
        private bool _logData;

        private static readonly int LinearSource = Shader.PropertyToID("linear_source");
        private static readonly int Luminance = Shader.PropertyToID("luminance");
        private static readonly int XYZSource = Shader.PropertyToID("xyz_source");
        private static readonly int MipSource = Shader.PropertyToID("mip_source");

        private void Awake()
        {
            if (ExperimentDataLogger.Instance == null)
            {
                Debug.Log("Data Logger not found in scene!");
                enabled = false;
                return;
            }

            _blitLuminance = false;
            _logData = false;

            ExperimentDataLogger.Instance.SetTransformData(headTransform, leftHand, rightHand);
            ExperimentDataLogger.Instance.SetPoseDriverData(headPoseDriver, leftHandPoseDriver, rightHandPoseDriver);

            Setup();
        }
        private void Setup()
        {

            _width = XRSettings.eyeTextureDesc.width;
            _height = XRSettings.eyeTextureDesc.height;


            _linearToXyzKernel = luminanceComputeShader.FindKernel("linear_to_xyz");
            _luminanceKernel = luminanceComputeShader.FindKernel("read_luminance");

            luminanceComputeShader.GetKernelThreadGroupSizes(_luminanceKernel, out uint sizeX, out uint sizeY, out uint _);
            _luminanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, sizeof(float));

            luminanceComputeShader.SetBuffer(_luminanceKernel, Luminance, _luminanceBuffer);

            _groupSizeX = (int)sizeX;
            _groupSizeY = (int)sizeY;
        }

        private void LateUpdate()
        {
            if (!_request.done)
                return;

            if (_logData)
            {
                ExperimentDataLogger.Instance.RecordLuminanceData(_dataToLog);
                ExperimentDataLogger.Instance.RecordGazeData();
                ExperimentDataLogger.Instance.RecordPoseData();
                _logData = false;
            }

            _blitLuminance = true;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        { 
            if (!XRSettings.enabled) return;
            if (ExperimentDataLogger.Instance == null) return;
            if (!_blitLuminance) return;

            _logData = false;
            _blitLuminance = false;

            _tempTexture = RenderTexture.GetTemporary(_width, _height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _tempTexture.enableRandomWrite = true;
            _tempTexture.autoGenerateMips = false;
            _tempTexture.useMipMap = true;

            _threadsX = Mathf.CeilToInt(_width / (float)_groupSizeX);
            _threadsY = Mathf.CeilToInt(_height / (float)_groupSizeY);

            Graphics.Blit(source, permanentTexture);

            luminanceComputeShader.SetTexture(_linearToXyzKernel, LinearSource, permanentTexture);
            luminanceComputeShader.SetTexture(_linearToXyzKernel, XYZSource, _tempTexture);
            luminanceComputeShader.Dispatch(_linearToXyzKernel, _threadsX, _threadsY, 1);

            _tempTexture.GenerateMips();

            luminanceComputeShader.SetTexture(_luminanceKernel, MipSource, _tempTexture, 8);
            luminanceComputeShader.Dispatch(_luminanceKernel, 1, 1, 1);

            _request = AsyncGPUReadback.Request(_luminanceBuffer,
                readback =>
                {
                    if (readback.hasError)
                    {
                        return;
                    }

                    _outputBuffer = new NativeArray<float>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                    _outputBuffer = readback.GetData<float>();
                    _dataToLog = _outputBuffer[0];
                    _logData = true;
                    _outputBuffer.Dispose();
                });
            Graphics.Blit(source, destination);
            RenderTexture.ReleaseTemporary(_tempTexture);
        }

        private void Cleanup()
        {
            if (_tempTexture != null)
            {
                RenderTexture.ReleaseTemporary(_tempTexture);
            }

            _luminanceBuffer?.Dispose();
        }

        private void OnDisable()
        {
            _logData = false;
            _blitLuminance = false;

            if (!_request.done)
            {
                _request.WaitForCompletion();
                ExperimentDataLogger.Instance.RecordLuminanceData(_dataToLog);
                ExperimentDataLogger.Instance.RecordGazeData();
                ExperimentDataLogger.Instance.RecordPoseData();
            }

            Cleanup();
        }
    }
}