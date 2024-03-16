using System;

namespace PathNav.ExperimentControl
{
    using CsvHelper;
    using CsvHelper.Configuration;
    using LSL;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;
    using Tobii.XR;
    using UnityEngine;
    using UnityEngine.InputSystem.XR;
    using ViveSR.anipal.Eye;

    public class ExperimentDataLogger : MonoBehaviour
    {
        public static ExperimentDataLogger Instance { get; private set; }

        public Task gazeTask;
        public Task poseTask;
        public Task navigationTask;
        public Task luminanceTask;

        public IList<Task> tasks = new List<Task>();

        #region Logging Variables

        private string _logDirectory;
        private string _gazePath;
        private string _posePath;
        private string _navigationPath;
        private string _luminancePath;
        private static readonly CsvConfiguration LuminanceConfig = new(CultureInfo.InvariantCulture);
        private static readonly CsvConfiguration GazeConfig = new(CultureInfo.InvariantCulture);
        private static readonly CsvConfiguration PoseConfig = new(CultureInfo.InvariantCulture);
        private static readonly CsvConfiguration NavigationConfig = new(CultureInfo.InvariantCulture);

        #endregion

        #region Experiment Data

        private int _userId;
        private int _blockId;
        private string _modelId, _methodId;

        private Transform _playerTransform;
        private Transform _leftHand;
        private Transform _rightHand;
        private TrackedPoseDriver _headPoseDriver;
        private TrackedPoseDriver _leftHandPoseDriver;
        private TrackedPoseDriver _rightHandPoseDriver;

        private static EyeData _pupilData = new();
        private static TobiiXR_EyeTrackingData _eyeData = new();

        private Vector3 HeadPosition => _playerTransform.position;
        private Quaternion HeadRotation => _playerTransform.rotation;
        private Vector3 LeftHandPosition => _leftHand.position;
        private Quaternion LeftHandRotation => _leftHand.rotation;
        private Vector3 RightHandPosition => _rightHand.position;
        private Quaternion RightHandRotation => _rightHand.rotation;

        private Vector3 TrackedHeadPosition => _headPoseDriver.positionInput.action.ReadValue<Vector3>();
        private Vector3 TrackedRightHandPosition => _rightHandPoseDriver.positionInput.action.ReadValue<Vector3>();
        private Vector3 TrackedLeftHandPosition => _leftHandPoseDriver.positionInput.action.ReadValue<Vector3>();
        private Quaternion TrackedHeadRotation => _headPoseDriver.rotationInput.action.ReadValue<Quaternion>();

        private Quaternion TrackedRightHandRotation =>
            _rightHandPoseDriver.rotationInput.action.ReadValue<Quaternion>();

        private Quaternion TrackedLeftHandRotation => _leftHandPoseDriver.rotationInput.action.ReadValue<Quaternion>();

        #endregion

        #region Data Collections

        private NavigationDataFormat _navigationData;
        private GazeDataFormat _gazeData;
        private PoseDataFormat _poseData;
        private LuminanceDataFormat _luminanceData;

        private Queue<GazeDataFormat> _gazeQueue;
        private Queue<PoseDataFormat> _poseQueue;
        private Queue<NavigationDataFormat> _navigationQueue;
        private Queue<LuminanceDataFormat> _luminanceQueue;

        #endregion

        #region Status

        private static bool _enabled;

        #endregion

        public void Setup(int user, int block, string logDirectory, string gazePath, string posePath,
            string navigationPath, string luminancePath)
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                return;
            }

            _userId = user;
            _blockId = block;
            _logDirectory = logDirectory;
            _gazePath = gazePath;
            _posePath = posePath;
            _navigationPath = navigationPath;
            _luminancePath = luminancePath;

            _enabled = false;
            InitializeGazeLogging();
            InitializePoseLogging();
            InitializeNavigationLogging();
            InitializeLuminanceLogging();
        }

        public bool Enable(string model, string method)
        {
            if (Instance == null)
            {
                return false;
            }

            _modelId = model;
            _methodId = method;

            _enabled = true;

            return true;
        }

        public bool Disable()
        {
            if (Instance == null)
            {
                return false;
            }

            _modelId = string.Empty;
            _methodId = string.Empty;
            _enabled = false;

            return true;
        }

        public async Task WriteLuminanceData()
        {
            if (!_enabled)
            {
                return;
            }

            if (_luminanceQueue.Count == 0)
            {
                return;
            }
            
            await using StreamWriter streamWriter = new(_luminancePath, true);
            await using CsvWriter csvWriter = new(streamWriter, LuminanceConfig);

            csvWriter.Context.RegisterClassMap<LuminanceDataFormatMap>();
            luminanceTask = csvWriter.WriteRecordsAsync(_luminanceQueue);
            await luminanceTask;

            if (luminanceTask.IsCompleted)
            {
                luminanceTask.Dispose();
            }
        }

        private void InitializeGazeLogging()
        {
            _gazeQueue = new Queue<GazeDataFormat>();

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            if (File.Exists(_gazePath))
            {
                return;
            }

            using StreamWriter streamWriter = new(_gazePath);
            using CsvWriter csvWriter = new(streamWriter, GazeConfig);

            csvWriter.Context.RegisterClassMap<GazeDataFormatMap>();
            csvWriter.WriteHeader<GazeDataFormat>();
            csvWriter.NextRecord();

            GazeConfig.HasHeaderRecord = false;
        }

        private void InitializePoseLogging()
        {
            _poseQueue = new Queue<PoseDataFormat>();

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            if (File.Exists(_posePath))
            {
                return;
            }

            using StreamWriter streamWriter = new(_posePath);
            using CsvWriter csvWriter = new(streamWriter, PoseConfig);

            csvWriter.Context.RegisterClassMap<PoseDataFormatMap>();
            csvWriter.WriteHeader<PoseDataFormat>();
            csvWriter.NextRecord();

            PoseConfig.HasHeaderRecord = false;
        }

        private void InitializeNavigationLogging()
        {
            _navigationQueue = new Queue<NavigationDataFormat>();

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            if (File.Exists(_navigationPath))
            {
                return;
            }

            using StreamWriter streamWriter = new(_navigationPath);
            using CsvWriter csvWriter = new(streamWriter, NavigationConfig);

            csvWriter.Context.RegisterClassMap<NavigationDataFormatMap>();
            csvWriter.WriteHeader<NavigationDataFormat>();
            csvWriter.NextRecord();

            NavigationConfig.HasHeaderRecord = false;
        }

        private void InitializeLuminanceLogging()
        {
            _luminanceQueue = new Queue<LuminanceDataFormat>();

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            if (File.Exists(_luminancePath))
            {
                return;
            }

            using StreamWriter streamWriter = new(_luminancePath);
            using CsvWriter csvWriter = new(streamWriter, LuminanceConfig);

            csvWriter.Context.RegisterClassMap<LuminanceDataFormatMap>();
            csvWriter.WriteHeader<LuminanceDataFormat>();
            csvWriter.NextRecord();

            LuminanceConfig.HasHeaderRecord = false;
        }

        protected void OnApplicationQuit()
        {
            if (Instance == null) return;
            _enabled = false;

            if (!Instance.gazeTask.IsCompleted)
            {
                Instance.gazeTask.Wait();
            }

            if (!Instance.poseTask.IsCompleted)
            {
                Instance.poseTask.Wait();
            }

            if (!Instance.navigationTask.IsCompleted)
            {
                Instance.navigationTask.Wait();
            }

            if (!Instance.luminanceTask.IsCompleted)
            { 
                Instance.luminanceTask.Wait();
            }
        }

        public async Task WriteAllData()
        {
            if (!_enabled)
            {
                return;
            }

            _enabled = false;

            if (_gazeQueue.Count > 0)
            {
                await WriteGazeData();
            }

            if (_poseQueue.Count > 0)
            {
                await WritePoseData();
            }

            if (_navigationQueue.Count > 0)
            {
                await WriteNavigationData();
            }

            if (tasks.Count > 0)
            {
                foreach (Task task in tasks)
                {
                    await task;
                }
            }
        }

        public async Task WriteGazeData()
        {
            if (!_enabled)
            {
                return;
            }

            await using StreamWriter streamWriter = new(_gazePath, true);
            await using CsvWriter csvWriter = new(streamWriter, GazeConfig);

            csvWriter.Context.RegisterClassMap<GazeDataFormatMap>();
            gazeTask = csvWriter.WriteRecordsAsync(_gazeQueue.ToArray());
            await gazeTask;

            if (gazeTask.IsCompleted)
            {
                _gazeQueue.Clear();
                gazeTask.Dispose();
            }
        }

        public async Task WritePoseData()
        {
            if (!_enabled)
            {
                return;
            }

            await using StreamWriter streamWriter = new(_posePath, true);
            await using CsvWriter csvWriter = new(streamWriter, PoseConfig);

            csvWriter.Context.RegisterClassMap<PoseDataFormatMap>();
            poseTask = csvWriter.WriteRecordsAsync(_poseQueue);
            await poseTask;

            if (poseTask.IsCompleted)
            {
                _poseQueue.Clear();
                poseTask.Dispose();
            }
        }

        public async Task WriteNavigationData()
        {
            if (!_enabled)
            {
                return;
            }

            await using StreamWriter streamWriter = new(_navigationPath, true);
            await using CsvWriter csvWriter = new(streamWriter, NavigationConfig);

            csvWriter.Context.RegisterClassMap<NavigationDataFormatMap>();
            navigationTask = csvWriter.WriteRecordsAsync(_navigationQueue);
            await navigationTask;

            if (navigationTask.IsCompleted)
            {
                _navigationQueue.Clear();
                navigationTask.Dispose();
            }
        }

        public void SetTransformData(Transform head, Transform left, Transform right)
        {
            _playerTransform = head;
            _leftHand = left;
            _rightHand = right;
        }

        public void SetPoseDriverData(TrackedPoseDriver head, TrackedPoseDriver left, TrackedPoseDriver right)
        {
            _headPoseDriver = head;
            _leftHandPoseDriver = left;
            _rightHandPoseDriver = right;
        }

        public void RecordNavigationData(float speed, double splinePercent, Vector3 splinePosition)
        {
            if (!_enabled)
            {
                return;
            }

            _navigationData = new NavigationDataFormat
            {
                ID = _userId,
                BLOCK_ID = _blockId,
                MODEL = _modelId,
                METHOD = _methodId,
                SPEED = speed,
                SPLINE_PERCENT = splinePercent,
                SPLINE_POSITION = splinePosition.ToString("F3"),
                TIMESTAMP = LSL.local_clock(),
            };

            _navigationQueue.Enqueue(_navigationData);
        }

        public void RecordPoseData()
        {
            if (!_enabled)
            {
                return;
            }

            if (_playerTransform is null ||
                _leftHand is null ||
                _rightHand is null ||
                _headPoseDriver is null ||
                _leftHandPoseDriver is null ||
                _rightHandPoseDriver is null)
            {
                return;
            }

            _poseData = new PoseDataFormat
            {
                ID = _userId,
                BLOCK_ID = _blockId,
                MODEL = _modelId,
                METHOD = _methodId,
                HEAD_POSITION_X = HeadPosition.x,
                HEAD_POSITION_Y = HeadPosition.y,
                HEAD_POSITION_Z = HeadPosition.z,
                HEAD_ROTATION_X = HeadRotation.x,
                HEAD_ROTATION_Y = HeadRotation.y,
                HEAD_ROTATION_Z = HeadRotation.z,
                HEAD_ROTATION_W = HeadRotation.w,
                LEFT_POSITION_X = LeftHandPosition.x,
                LEFT_POSITION_Y = LeftHandPosition.y,
                LEFT_POSITION_Z = LeftHandPosition.z,
                LEFT_ROTATION_X = LeftHandRotation.x,
                LEFT_ROTATION_Y = LeftHandRotation.y,
                LEFT_ROTATION_Z = LeftHandRotation.z,
                LEFT_ROTATION_W = LeftHandRotation.w,
                RIGHT_POSITION_X = RightHandPosition.x,
                RIGHT_POSITION_Y = RightHandPosition.y,
                RIGHT_POSITION_Z = RightHandPosition.z,
                RIGHT_ROTATION_X = RightHandRotation.x,
                RIGHT_ROTATION_Y = RightHandRotation.y,
                RIGHT_ROTATION_Z = RightHandRotation.z,
                RIGHT_ROTATION_W = RightHandRotation.w,
                TRACKED_HEAD_POSITION_X = TrackedHeadPosition.x,
                TRACKED_HEAD_POSITION_Y = TrackedHeadPosition.y,
                TRACKED_HEAD_POSITION_Z = TrackedHeadPosition.z,
                TRACKED_HEAD_ROTATION_X = TrackedHeadRotation.x,
                TRACKED_HEAD_ROTATION_Y = TrackedHeadRotation.y,
                TRACKED_HEAD_ROTATION_Z = TrackedHeadRotation.z,
                TRACKED_HEAD_ROTATION_W = TrackedHeadRotation.w,
                TRACKED_LEFT_POSITION_X = TrackedLeftHandPosition.x,
                TRACKED_LEFT_POSITION_Y = TrackedLeftHandPosition.y,
                TRACKED_LEFT_POSITION_Z = TrackedLeftHandPosition.z,
                TRACKED_LEFT_ROTATION_X = TrackedLeftHandRotation.x,
                TRACKED_LEFT_ROTATION_Y = TrackedLeftHandRotation.y,
                TRACKED_LEFT_ROTATION_Z = TrackedLeftHandRotation.z,
                TRACKED_LEFT_ROTATION_W = TrackedLeftHandRotation.w,
                TRACKED_RIGHT_POSITION_X = TrackedRightHandPosition.x,
                TRACKED_RIGHT_POSITION_Y = TrackedRightHandPosition.y,
                TRACKED_RIGHT_POSITION_Z = TrackedRightHandPosition.z,
                TRACKED_RIGHT_ROTATION_X = TrackedRightHandRotation.x,
                TRACKED_RIGHT_ROTATION_Y = TrackedRightHandRotation.y,
                TRACKED_RIGHT_ROTATION_Z = TrackedRightHandRotation.z,
                TRACKED_RIGHT_ROTATION_W = TrackedRightHandRotation.w,
                TIMESTAMP = LSL.local_clock(),
            };

            _poseQueue.Enqueue(_poseData);
        }

        public void RecordGazeData()
        {
            if (!_enabled)
            {
                return;
            }

            _eyeData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);
            SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out Vector3 _, out Vector3 _, _pupilData);

            _gazeData = new GazeDataFormat
            {
                ID = _userId,
                BLOCK_ID = _blockId,
                MODEL = _modelId,
                METHOD = _methodId,
                TIMESTAMP = LSL.local_clock(),
                CONVERGENCE_DISTANCE = _eyeData.ConvergenceDistance,
                CONVERGENCE_VALID = _eyeData.ConvergenceDistanceIsValid,
                GAZERAY_ORIGIN_X = _eyeData.GazeRay.Origin.x,
                GAZERAY_ORIGIN_Y = _eyeData.GazeRay.Origin.y,
                GAZERAY_ORIGIN_Z = _eyeData.GazeRay.Origin.z,
                GAZERAY_DIRECTION_X = _eyeData.GazeRay.Direction.x,
                GAZERAY_DIRECTION_Y = _eyeData.GazeRay.Direction.y,
                GAZERAY_DIRECTION_Z = _eyeData.GazeRay.Direction.z,
                GAZERAY_VALID = _eyeData.GazeRay.IsValid,
                LEFT_IS_BLINKING = _eyeData.IsLeftEyeBlinking,
                RIGHT_IS_BLINKING = _eyeData.IsRightEyeBlinking,
                LEFT_EYE_PUPIL_DIAMETER = _pupilData.verbose_data.left.pupil_diameter_mm,
                RIGHT_EYE_PUPIL_DIAMETER = _pupilData.verbose_data.right.pupil_diameter_mm,
            };

            _gazeQueue.Enqueue(_gazeData);
        }

        public async Task RecordLuminanceData(IEnumerable<Data> luminanceQueue)
        {
            foreach (Data data in luminanceQueue)
            {
                _luminanceData = new LuminanceDataFormat
                {
                    ID = _userId,
                    BLOCK_ID = _blockId,
                    MODEL = _modelId,
                    METHOD = _methodId,
                    LUMINANCE = data.luminance,
                    TIMESTAMP = data.timestamp,
                };

                _luminanceQueue.Enqueue(_luminanceData);
            }

            await WriteLuminanceData();
            _luminanceQueue.Clear();    
        }
    }
}