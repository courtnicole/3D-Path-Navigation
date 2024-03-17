namespace PathNav.ExperimentControl
{
    using CsvHelper;
    using CsvHelper.Configuration;
    using LSL;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Tobii.XR;
    using UnityEngine.InputSystem.XR;
    using ViveSR.anipal.Eye;

    public class DataLogger : MonoBehaviour
    { 
        public static DataLogger Instance { get; private set; }
        [Header("Data Logging Variables")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;
        [SerializeField] private TrackedPoseDriver headPoseDriver;
        [SerializeField] private TrackedPoseDriver leftHandPoseDriver;
        [SerializeField] private TrackedPoseDriver rightHandPoseDriver;
        
        private static float _userId;
        private static float _blockId;
        private static float _modelId;
        private static float _methodId;
        
        private NavigationDataFormat _navigationData;
        private GazeDataFormat _gazeData;
        private PoseDataFormat _poseData;
        
        private static string _navFile, _gazeFile, _poseFile;
        private Vector3 gazeOrigin, gazeDirection;
        private static EyeData pupilData = new EyeData();
        private static TobiiXR_EyeTrackingData eyeData = new TobiiXR_EyeTrackingData();
        private static readonly CsvConfiguration Config = new(CultureInfo.InvariantCulture);
        private bool _recordData;
        
        private Queue<GazeDataFormat> _gazeQueue;
        
        private Vector3    HeadPosition      => playerTransform.position;
        private Quaternion HeadRotation      => playerTransform.rotation;
        private Vector3    LeftHandPosition  => leftHand.position;
        private Quaternion LeftHandRotation  => leftHand.rotation;
        private Vector3    RightHandPosition => rightHand.position;
        private Quaternion RightHandRotation => rightHand.rotation;
        
        private bool _allowLogging;
        
        public void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
                return;
            }
            
            _gazeFile     = InitializeDataLog(Application.dataPath + "/Data/GazeData/", Application.dataPath + "/Data/GazeData/GazeData.csv");
            _gazeQueue    = new Queue<GazeDataFormat>();
            _allowLogging = true;
        }

        public async void OnDisable()
        {
            Config.HasHeaderRecord = true;
            Config.BufferSize      = 2048;
            await using StreamWriter streamWriter2 = new(_gazeFile, true);
            {
                await using CsvWriter csvWriter = new(streamWriter2, Config);
            
                {
                    csvWriter.Context.RegisterClassMap<GazeDataFormatMap>();
                    await csvWriter.WriteRecordsAsync(_gazeQueue);
                    await csvWriter.NextRecordAsync();
                }
            }
        }

        private static string InitializeDataLog(string logDirectory, string filePath)
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            if (File.Exists(filePath))
            {
                return filePath;
            }

            
            using StreamWriter streamWriter = new(filePath);
            using CsvWriter    csvWriter    = new(streamWriter, Config);
            csvWriter.Context.RegisterClassMap<GazeDataFormatMap>();
           
            return filePath;
        }

        public void LogGazeData()
        {
            if (!_allowLogging) return;
            
            GazeData();
        }
        
        private void GazeData()
        {
            _gazeData = new GazeDataFormat
            {
                ID       = 12,
                BLOCK_ID = 2,
                MODEL    = "MM",
                METHOD   = "TBD",
            };
            
            eyeData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);
            SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out gazeOrigin, out gazeDirection, pupilData);
            
            _gazeData.CONVERGENCE_DISTANCE = eyeData.ConvergenceDistance;
            _gazeData.CONVERGENCE_VALID    = eyeData.ConvergenceDistanceIsValid;
            
            _gazeData.GAZERAY_ORIGIN_X    = eyeData.GazeRay.Origin.x;
            _gazeData.GAZERAY_ORIGIN_Y    = eyeData.GazeRay.Origin.y;
            _gazeData.GAZERAY_ORIGIN_Z    = eyeData.GazeRay.Origin.z;
            _gazeData.GAZERAY_DIRECTION_X = eyeData.GazeRay.Direction.x;
            _gazeData.GAZERAY_DIRECTION_Y = eyeData.GazeRay.Direction.y;
            _gazeData.GAZERAY_DIRECTION_Z = eyeData.GazeRay.Direction.z;
            _gazeData.GAZERAY_VALID       = eyeData.GazeRay.IsValid;
            
            _gazeData.LEFT_IS_BLINKING  = eyeData.IsLeftEyeBlinking;
            _gazeData.RIGHT_IS_BLINKING = eyeData.IsRightEyeBlinking;
            
            _gazeData.LEFT_EYE_PUPIL_DIAMETER  = pupilData.verbose_data.left.pupil_diameter_mm;
            _gazeData.RIGHT_EYE_PUPIL_DIAMETER = pupilData.verbose_data.right.pupil_diameter_mm;
            
            _gazeData.TIMESTAMP      = LSL.local_clock();
            _gazeQueue.Enqueue(_gazeData);
        }

        private void PoseData()
        {
            _poseData.HEAD_POSITION_X = HeadPosition.x;
            _poseData.HEAD_POSITION_Y = HeadPosition.y;
            _poseData.HEAD_POSITION_Z = HeadPosition.z;
            
            _poseData.HEAD_ROTATION_X = HeadRotation.x;
            _poseData.HEAD_ROTATION_Y = HeadRotation.y;
            _poseData.HEAD_ROTATION_Z = HeadRotation.z;
            _poseData.HEAD_ROTATION_W = HeadRotation.w;

            _poseData.LEFT_POSITION_X = LeftHandPosition.x;
            _poseData.LEFT_POSITION_Y = LeftHandPosition.y;
            _poseData.LEFT_POSITION_Z = LeftHandPosition.z;

            _poseData.LEFT_ROTATION_X = LeftHandRotation.x;
            _poseData.LEFT_ROTATION_Y = LeftHandRotation.y;
            _poseData.LEFT_ROTATION_Z = LeftHandRotation.z;
            _poseData.LEFT_ROTATION_W = LeftHandRotation.w;

            _poseData.RIGHT_POSITION_X = RightHandPosition.x;
            _poseData.RIGHT_POSITION_Y = RightHandPosition.y;
            _poseData.RIGHT_POSITION_Z = RightHandPosition.z;
            
            _poseData.RIGHT_ROTATION_X = RightHandRotation.x;
            _poseData.RIGHT_ROTATION_Y = RightHandRotation.y;
            _poseData.RIGHT_ROTATION_Z = RightHandRotation.z;
            _poseData.RIGHT_ROTATION_W = RightHandRotation.w;

            _poseData.TRACKED_HEAD_POSITION_X = headPoseDriver.positionInput.action.ReadValue<Vector3>().x;
            _poseData.TRACKED_HEAD_POSITION_Y = headPoseDriver.positionInput.action.ReadValue<Vector3>().y;
            _poseData.TRACKED_HEAD_POSITION_Z = headPoseDriver.positionInput.action.ReadValue<Vector3>().z;
            
            _poseData.TRACKED_HEAD_ROTATION_X = headPoseDriver.rotationInput.action.ReadValue<Quaternion>().x;
            _poseData.TRACKED_HEAD_ROTATION_Y = headPoseDriver.rotationInput.action.ReadValue<Quaternion>().y;
            _poseData.TRACKED_HEAD_ROTATION_Z = headPoseDriver.rotationInput.action.ReadValue<Quaternion>().z;
            _poseData.TRACKED_HEAD_ROTATION_W = headPoseDriver.rotationInput.action.ReadValue<Quaternion>().w;
            
            _poseData.TRACKED_LEFT_POSITION_X = leftHandPoseDriver.positionInput.action.ReadValue<Vector3>().x;
            _poseData.TRACKED_LEFT_POSITION_Y = leftHandPoseDriver.positionInput.action.ReadValue<Vector3>().y;
            _poseData.TRACKED_LEFT_POSITION_Z = leftHandPoseDriver.positionInput.action.ReadValue<Vector3>().z;
            
            _poseData.TRACKED_LEFT_ROTATION_X = leftHandPoseDriver.rotationInput.action.ReadValue<Quaternion>().x;
            _poseData.TRACKED_LEFT_ROTATION_Y = leftHandPoseDriver.rotationInput.action.ReadValue<Quaternion>().y;
            _poseData.TRACKED_LEFT_ROTATION_Z = leftHandPoseDriver.rotationInput.action.ReadValue<Quaternion>().z;
            _poseData.TRACKED_LEFT_ROTATION_W = leftHandPoseDriver.rotationInput.action.ReadValue<Quaternion>().w;
            
            _poseData.TRACKED_RIGHT_POSITION_X = rightHandPoseDriver.positionInput.action.ReadValue<Vector3>().x;
            _poseData.TRACKED_RIGHT_POSITION_Y = rightHandPoseDriver.positionInput.action.ReadValue<Vector3>().y;
            _poseData.TRACKED_RIGHT_POSITION_Z = rightHandPoseDriver.positionInput.action.ReadValue<Vector3>().z;
            
            _poseData.TRACKED_RIGHT_ROTATION_X = rightHandPoseDriver.rotationInput.action.ReadValue<Quaternion>().x;
            _poseData.TRACKED_RIGHT_ROTATION_Y = rightHandPoseDriver.rotationInput.action.ReadValue<Quaternion>().y;
            _poseData.TRACKED_RIGHT_ROTATION_Z = rightHandPoseDriver.rotationInput.action.ReadValue<Quaternion>().z;
            _poseData.TRACKED_RIGHT_ROTATION_W = rightHandPoseDriver.rotationInput.action.ReadValue<Quaternion>().w;
        }
        
    }
}