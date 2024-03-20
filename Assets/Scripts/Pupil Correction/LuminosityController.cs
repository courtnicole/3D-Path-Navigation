namespace PathNav.ExperimentControl
{
    using Unity.Collections;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.XR;
    using UnityEngine.InputSystem.XR;

    public class LuminosityController : MonoBehaviour
    {
        public ComputeShader luminanceComputeShader;
        public RenderTexture permanentTexture;

        [Header("Data Logging Variables")] [SerializeField]
        private Transform headTransform;

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
        private Camera _mainCamera;
        private AsyncGPUReadbackRequest _request;

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

            _mainCamera    = GetComponent<Camera>();
            _logData       = false;

            ExperimentDataLogger.Instance.SetPoseTransformData(headTransform, leftHand, rightHand);
            ExperimentDataLogger.Instance.SetPoseDriverData(headPoseDriver, leftHandPoseDriver, rightHandPoseDriver);

            Setup();
        }

        private void Setup()
        {
            _width  = XRSettings.eyeTextureDesc.width;
            _height = XRSettings.eyeTextureDesc.height;

            _linearToXyzKernel = luminanceComputeShader.FindKernel("linear_to_xyz");
            _luminanceKernel   = luminanceComputeShader.FindKernel("read_luminance");

            luminanceComputeShader.GetKernelThreadGroupSizes(_luminanceKernel, out uint sizeX, out uint sizeY, out uint _);
            _luminanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, sizeof(float));

            luminanceComputeShader.SetBuffer(_luminanceKernel, Luminance, _luminanceBuffer);
            _groupSizeX = (int)sizeX;
            _groupSizeY = (int)sizeY;
            _logData    = true;
        }

        private void LateUpdate()
        {
            if (!_logData) return;

            ExperimentDataLogger.Instance.RecordUserGazeData();
            ExperimentDataLogger.Instance.RecordPoseData();
            ExperimentDataLogger.Instance.RecordTrackedPoseData();
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!XRSettings.enabled)
            {
                Graphics.Blit(source, destination);
                return;
            }
            if (_mainCamera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right)
            {
                _tempTexture                   = RenderTexture.GetTemporary(_width, _height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                _tempTexture.enableRandomWrite = true;
                _tempTexture.autoGenerateMips  = false;
                _tempTexture.useMipMap         = true;

                _threadsX = Mathf.CeilToInt(_width  / (float)_groupSizeX);
                _threadsY = Mathf.CeilToInt(_height / (float)_groupSizeY);
                
                Graphics.Blit(source, permanentTexture);

                luminanceComputeShader.SetTexture(_linearToXyzKernel, LinearSource, permanentTexture);
                luminanceComputeShader.SetTexture(_linearToXyzKernel, XYZSource,    _tempTexture);
                luminanceComputeShader.Dispatch(_linearToXyzKernel, _threadsX, _threadsY, 1);

                _tempTexture.GenerateMips();

                luminanceComputeShader.SetTexture(_luminanceKernel, MipSource, _tempTexture, 11);
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
                                                        _dataToLog    = _outputBuffer[0];
                                                        _outputBuffer.Dispose();
                                                        if (ExperimentDataLogger.Instance == null) return;
                                                        ExperimentDataLogger.Instance.RecordUserLuminanceData(_dataToLog);
                                                    });
                Graphics.Blit(source, destination);
                RenderTexture.ReleaseTemporary(_tempTexture);
            }
            else
            {
                Graphics.Blit(source, destination);
            }
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
            _logData       = false;

            if (!_request.done)
            {
                _request.WaitForCompletion();
                ExperimentDataLogger.Instance.RecordUserLuminanceData(_dataToLog);
                ExperimentDataLogger.Instance.RecordUserGazeData();
                ExperimentDataLogger.Instance.RecordPoseData();
                ExperimentDataLogger.Instance.RecordTrackedPoseData();
            }

            Cleanup();
        }
    }
}