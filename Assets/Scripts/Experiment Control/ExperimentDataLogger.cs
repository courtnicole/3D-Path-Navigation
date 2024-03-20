using System;
using LSL4Unity.Utils;
using System.Globalization;

namespace PathNav.ExperimentControl
{
    using LSL;
    using Tobii.XR;
    using UnityEngine;
    using UnityEngine.InputSystem.XR;
    using ViveSR.anipal.Eye;

    public class ExperimentDataLogger
    {
        public static ExperimentDataLogger Instance { get; private set; }

        public Vector3 CurrentCombinedGazeDirection { get; private set; }

        #region Logging Variables
        private static StreamOutlet _luminanceOutlet;
        private const string _luminanceStreamName = "LuminanceStream";
        private const string _luminanceStreamType = "Luminance";
        private const string _luminanceStreamID = "XRData_Luminance_01";
        private static float[] _luminanceSample;

        private static StreamOutlet _gazeOutlet;
        private const string _gazeStreamName = "GazeStream";
        private const string _gazeStreamType = "Gaze";
        private const string _gazeStreamID = "XRData_Gaze_01";
        private static float[] _gazeSample;

        private static StreamOutlet _poseOutlet;
        private const string _poseStreamName = "PoseStream";
        private const string _poseStreamType = "Pose";
        private const string _poseStreamID = "XRData_Pose_01";
        private static float[] _poseSample;

        private static StreamOutlet _navigationOutlet;
        private const string _navigationStreamName = "NavigationStream";
        private const string _navigationStreamType = "Navigation";
        private const string _navigationStreamID = "XRData_Navigation_01";
        private static float[] _navigationSample;

        private static StreamOutlet _creationOutlet;
        private const string _creationStreamName = "CreationStream";
        private const string _creationStreamType = "Creation";
        private const string _creationStreamID = "XRData_Creation_01";
        private static string[] _creationSample;

        private static StreamOutlet _surveyOutlet;
        private const string _surveyStreamName = "SurveyStream";
        private const string _surveyStreamType = "Survey";
        private const string _surveyStreamID = "XRData_Survey_01";
        private static string[] _surveySample;

        private static StreamOutlet _experimentOutlet;
        private const string _experimentStreamName = "ExperimentStream";
        private const string _experimentStreamType = "Experiment";
        private const string _experimentStreamID = "XRData_Experiment_01";
        private static string[] _experimentSample;
        
        #endregion

        #region Experiment Data
        private readonly float _userId;
        private readonly float _blockId;
        private float _modelId;
        private float _methodId;

        private Transform _playerTransform;
        private Transform _leftHand;
        private Transform _rightHand;

        private TrackedPoseDriver _headPoseDriver;
        private TrackedPoseDriver _leftHandPoseDriver;
        private TrackedPoseDriver _rightHandPoseDriver;

        private static VerboseData _pupilData = new();
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

        private Quaternion TrackedRightHandRotation => _rightHandPoseDriver.rotationInput.action.ReadValue<Quaternion>();

        private Quaternion TrackedLeftHandRotation => _leftHandPoseDriver.rotationInput.action.ReadValue<Quaternion>();
        #endregion

        #region Status
        private static bool _enabled;
        #endregion

        public ExperimentDataLogger(float userID, float blockId)
        {
            Instance = this;
            _userId = userID;
            _blockId = blockId;
            CurrentCombinedGazeDirection = Vector3.zero;

            CreateExperimentStream();
            CreateSurveyStream();
            CreateGazeStream();
            CreatePoseStream();
            CreateNavigationStream();
            CreateCreationStream();
            CreateLuminanceStream();
        }

        public void Enable(float model, float method)
        {
            _modelId = model;
            _methodId = method;
            _enabled = true;
        }

        public void Disable()
        {
            _enabled = false;
        }

        private void CreateGazeStream()
        {
            StreamInfo streamInfo = new(_gazeStreamName, _gazeStreamType, 17, LSLCommon.GetSamplingRateFor(MomentForSampling.LateUpdate), channel_format_t.cf_float32, _gazeStreamID);
            XMLElement channels = streamInfo.desc().append_child("channels");

            channels.append_child("channel").append_child_value("label", "UserID");
            channels.append_child("channel").append_child_value("label", "BlockID");
            channels.append_child("channel").append_child_value("label", "ModelID");
            channels.append_child("channel").append_child_value("label", "MethodID");

            channels.append_child("channel").append_child_value("label", "ConvergenceDistance");
            channels.append_child("channel").append_child_value("label", "ConvergenceDistanceIsValid");

            channels.append_child("channel").append_child_value("label", "GazeOriginX").append_child_value("eye", "Both").append_child_value("type", "PositionX").append_child_value("coordinate_system", "world-space");
            channels.append_child("channel").append_child_value("label", "GazeOriginY").append_child_value("eye", "Both").append_child_value("type", "PositionY").append_child_value("coordinate_system", "world-space");
            channels.append_child("channel").append_child_value("label", "GazeOriginZ").append_child_value("eye", "Both").append_child_value("type", "PositionZ").append_child_value("coordinate_system", "world-space");

            channels.append_child("channel").append_child_value("label", "GazeDirectionNormalizedX").append_child_value("eye", "Both").append_child_value("type", "DirectionX").append_child_value("coordinate_system", "world-space").append_child_value("unit", "Normalized");
            channels.append_child("channel").append_child_value("label", "GazeDirectionNormalizedY").append_child_value("eye", "Both").append_child_value("type", "DirectionY").append_child_value("coordinate_system", "world-space").append_child_value("unit", "Normalized");
            channels.append_child("channel").append_child_value("label", "GazeDirectionNormalizedZ").append_child_value("eye", "Both").append_child_value("type", "DirectionZ").append_child_value("coordinate_system", "world-space").append_child_value("unit", "Normalized");

            channels.append_child("channel").append_child_value("label", "GazeRayIsValid");

            channels.append_child("channel").append_child_value("label", "LeftEyeIsBlinking").append_child_value("eye", "Left");
            channels.append_child("channel").append_child_value("label", "RightEyeIsBlinking").append_child_value("eye", "Right");

            channels.append_child("channel").append_child_value("label", "LeftPupilDiameter").append_child_value("eye", "Left").append_child_value("type", "Diameter");
            channels.append_child("channel").append_child_value("label", "RightPupilDiameter").append_child_value("eye", "Right").append_child_value("type", "Diameter");

            _gazeSample = new float[17];
            _gazeOutlet = new StreamOutlet(streamInfo);

            _gazeSample[0] = _userId;
            _gazeSample[1] = _blockId;
            _gazeSample[2] = _modelId;
            _gazeSample[3] = _methodId;
        }

        private void CreatePoseStream()
        {
            StreamInfo streamInfo = new(_poseStreamName, _poseStreamType, 46, LSLCommon.GetSamplingRateFor(MomentForSampling.LateUpdate), channel_format_t.cf_float32, _poseStreamID);

            XMLElement channels = streamInfo.desc().append_child("channels");

            channels.append_child("channel").append_child_value("label", "UserID");
            channels.append_child("channel").append_child_value("label", "BlockID");
            channels.append_child("channel").append_child_value("label", "ModelID");
            channels.append_child("channel").append_child_value("label", "MethodID");

            channels.append_child("channel").append_child_value("label", "Head_PosX");
            channels.append_child("channel").append_child_value("label", "Head_PosY");
            channels.append_child("channel").append_child_value("label", "Head_PosZ");
            channels.append_child("channel").append_child_value("label", "Head_RotW");
            channels.append_child("channel").append_child_value("label", "Head_RotX");
            channels.append_child("channel").append_child_value("label", "Head_RotY");
            channels.append_child("channel").append_child_value("label", "Head_RotZ");

            channels.append_child("channel").append_child_value("label", "LefHand_PosX");
            channels.append_child("channel").append_child_value("label", "LefHand_PosY");
            channels.append_child("channel").append_child_value("label", "LefHand_PosZ");
            channels.append_child("channel").append_child_value("label", "LefHand_RotW");
            channels.append_child("channel").append_child_value("label", "LefHand_RotX");
            channels.append_child("channel").append_child_value("label", "LefHand_RotY");
            channels.append_child("channel").append_child_value("label", "LefHand_RotZ");

            channels.append_child("channel").append_child_value("label", "RightHand_PosX");
            channels.append_child("channel").append_child_value("label", "RightHand_PosY");
            channels.append_child("channel").append_child_value("label", "RightHand_PosZ");
            channels.append_child("channel").append_child_value("label", "RightHand_RotW");
            channels.append_child("channel").append_child_value("label", "RightHand_RotX");
            channels.append_child("channel").append_child_value("label", "RightHand_RotY");
            channels.append_child("channel").append_child_value("label", "RightHand_RotZ");

            channels.append_child("channel").append_child_value("label", "Tracked_Head_PosX");
            channels.append_child("channel").append_child_value("label", "Tracked_Head_PosY");
            channels.append_child("channel").append_child_value("label", "Tracked_Head_PosZ");
            channels.append_child("channel").append_child_value("label", "Tracked_Head_RotW");
            channels.append_child("channel").append_child_value("label", "Tracked_Head_RotX");
            channels.append_child("channel").append_child_value("label", "Tracked_Head_RotY");
            channels.append_child("channel").append_child_value("label", "Tracked_Head_RotZ");

            channels.append_child("channel").append_child_value("label", "Tracked_LefHand_PosX");
            channels.append_child("channel").append_child_value("label", "Tracked_LefHand_PosY");
            channels.append_child("channel").append_child_value("label", "Tracked_LefHand_PosZ");
            channels.append_child("channel").append_child_value("label", "Tracked_LefHand_RotW");
            channels.append_child("channel").append_child_value("label", "Tracked_LefHand_RotX");
            channels.append_child("channel").append_child_value("label", "Tracked_LefHand_RotY");
            channels.append_child("channel").append_child_value("label", "Tracked_LefHand_RotZ");

            channels.append_child("channel").append_child_value("label", "Tracked_RightHand_PosX");
            channels.append_child("channel").append_child_value("label", "Tracked_RightHand_PosY");
            channels.append_child("channel").append_child_value("label", "Tracked_RightHand_PosZ");
            channels.append_child("channel").append_child_value("label", "Tracked_RightHand_RotW");
            channels.append_child("channel").append_child_value("label", "Tracked_RightHand_RotX");
            channels.append_child("channel").append_child_value("label", "Tracked_RightHand_RotY");
            channels.append_child("channel").append_child_value("label", "Tracked_RightHand_RotZ");

            _poseSample = new float[46];
            _poseOutlet = new StreamOutlet(streamInfo);
        }

        private void CreateNavigationStream()
        {
            StreamInfo streamInfo = new(_navigationStreamName, _navigationStreamType, 9, LSLCommon.GetSamplingRateFor(MomentForSampling.LateUpdate), channel_format_t.cf_float32, _navigationStreamID);

            XMLElement channels = streamInfo.desc().append_child("channels");

            channels.append_child("channel").append_child_value("label", "UserID");
            channels.append_child("channel").append_child_value("label", "BlockID");
            channels.append_child("channel").append_child_value("label", "ModelID");
            channels.append_child("channel").append_child_value("label", "MethodID");

            channels.append_child("channel").append_child_value("label", "speed");
            channels.append_child("channel").append_child_value("label", "spline_percent");
            channels.append_child("channel").append_child_value("label", "spline_position_x");
            channels.append_child("channel").append_child_value("label", "spline_position_y");
            channels.append_child("channel").append_child_value("label", "spline_position_z");

            _navigationSample = new float[9];
            _navigationOutlet = new StreamOutlet(streamInfo);

            _navigationSample[0] = _userId;
            _navigationSample[1] = _blockId;
            _navigationSample[2] = _modelId;
            _navigationSample[3] = _methodId;
        }

        private void CreateSurveyStream()
        {
            StreamInfo streamInfo = new(_surveyStreamName, _surveyStreamType, 6, LSL.IRREGULAR_RATE, channel_format_t.cf_string, _surveyStreamID);

            XMLElement channels = streamInfo.desc().append_child("channels");

            channels.append_child("channel").append_child_value("label", "UserID");
            channels.append_child("channel").append_child_value("label", "BlockID");
            channels.append_child("channel").append_child_value("label", "ModelID");
            channels.append_child("channel").append_child_value("label", "MethodID");

            channels.append_child("channel").append_child_value("label", "SurveyType");
            channels.append_child("channel").append_child_value("label", "Value");

            _surveySample = new string[6];
            _surveyOutlet = new StreamOutlet(streamInfo);

            _surveySample[0] = _userId.ToString(CultureInfo.InvariantCulture);
            _surveySample[1] = _blockId.ToString(CultureInfo.InvariantCulture);
            _surveySample[2] = _modelId.ToString(CultureInfo.InvariantCulture);
            _surveySample[3] = _methodId.ToString(CultureInfo.InvariantCulture);
        }

        private void CreateCreationStream()
        {
            StreamInfo streamInfo = new(_creationStreamName, _creationStreamType, 7, LSL.IRREGULAR_RATE, channel_format_t.cf_string, _creationStreamID);

            XMLElement channels = streamInfo.desc().append_child("channels");

            channels.append_child("channel").append_child_value("label", "UserID");
            channels.append_child("channel").append_child_value("label", "BlockID");
            channels.append_child("channel").append_child_value("label", "ModelID");
            channels.append_child("channel").append_child_value("label", "MethodID");

            channels.append_child("channel").append_child_value("label", "EventName");
            channels.append_child("channel").append_child_value("label", "EventType");
            channels.append_child("channel").append_child_value("label", "Duration");

            _creationSample = new string[7];
            _creationOutlet = new StreamOutlet(streamInfo);

            _creationSample[0] = _userId.ToString(CultureInfo.InvariantCulture);
            _creationSample[1] = _blockId.ToString(CultureInfo.InvariantCulture);
            _creationSample[2] = _modelId.ToString(CultureInfo.InvariantCulture);
            _creationSample[3] = _methodId.ToString(CultureInfo.InvariantCulture);
        }

        private void CreateLuminanceStream()
        {
            StreamInfo streamInfo = new(_luminanceStreamName, _luminanceStreamType, 5, LSLCommon.GetSamplingRateFor(MomentForSampling.LateUpdate), channel_format_t.cf_float32, _luminanceStreamID);

            XMLElement channels = streamInfo.desc().append_child("channels");

            channels.append_child("channel").append_child_value("label", "UserID");
            channels.append_child("channel").append_child_value("label", "BlockID");
            channels.append_child("channel").append_child_value("label", "ModelID");
            channels.append_child("channel").append_child_value("label", "MethodID");
            channels.append_child("channel").append_child_value("label", "Luminance");

            _luminanceSample = new float[5];
            _luminanceOutlet = new StreamOutlet(streamInfo);

            _luminanceSample[0] = _userId;
            _luminanceSample[1] = _blockId;
            _luminanceSample[2] = _modelId;
            _luminanceSample[3] = _methodId;
        }

        private void CreateExperimentStream()
        {
            StreamInfo streamInfo = new(_experimentStreamName, _experimentStreamType, 5, LSL.IRREGULAR_RATE, channel_format_t.cf_string, _experimentStreamID);
            XMLElement channels = streamInfo.desc().append_child("channels");

            channels.append_child("channel").append_child_value("label", "UserID");
            channels.append_child("channel").append_child_value("label", "BlockID");
            channels.append_child("channel").append_child_value("label", "ModelID");
            channels.append_child("channel").append_child_value("label", "MethodID");
            channels.append_child("channel").append_child_value("label", "SceneEvent");
            channels.append_child("channel").append_child_value("label", "EventType");
            
            _experimentSample = new string[5];
            _experimentOutlet = new StreamOutlet(streamInfo);
            
            _experimentSample[0] = _userId.ToString(CultureInfo.InvariantCulture);
            _experimentSample[1] = _blockId.ToString(CultureInfo.InvariantCulture);
            _experimentSample[2] = _modelId.ToString(CultureInfo.InvariantCulture);
            _experimentSample[3] = _methodId.ToString(CultureInfo.InvariantCulture);
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

            _navigationSample[4] = speed;
            _navigationSample[5] = (float)splinePercent;
            _navigationSample[6] = splinePosition.x;
            _navigationSample[7] = splinePosition.y;
            _navigationSample[8] = splinePosition.z;

            _navigationOutlet.push_sample(_navigationSample, TimeSync.Instance.LateUpdateTimeStamp);
        }

        public void RecordCreationData(string eventName, string eventType, string duration)
        {
            if (!_enabled)
            {
                return;
            }
            
            _creationSample[4] = eventName;
            _creationSample[5] = eventType;
            _creationSample[6] = duration;
            _creationOutlet.push_sample(_creationSample);
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

            _poseSample[4] = HeadPosition.x;
            _poseSample[5] = HeadPosition.y;
            _poseSample[6] = HeadPosition.z;
            _poseSample[7] = HeadRotation.w;
            _poseSample[8] = HeadRotation.x;
            _poseSample[9] = HeadRotation.y;
            _poseSample[10] = HeadRotation.z;

            _poseSample[11] = LeftHandPosition.x;
            _poseSample[12] = LeftHandPosition.y;
            _poseSample[13] = LeftHandPosition.z;
            _poseSample[14] = LeftHandRotation.w;
            _poseSample[15] = LeftHandRotation.x;
            _poseSample[16] = LeftHandRotation.y;
            _poseSample[17] = LeftHandRotation.z;

            _poseSample[18] = RightHandPosition.x;
            _poseSample[19] = RightHandPosition.y;
            _poseSample[20] = RightHandPosition.z;
            _poseSample[21] = RightHandRotation.w;
            _poseSample[22] = RightHandRotation.x;
            _poseSample[23] = RightHandRotation.y;
            _poseSample[24] = RightHandRotation.z;

            _poseSample[25] = TrackedHeadPosition.x;
            _poseSample[26] = TrackedHeadPosition.y;
            _poseSample[27] = TrackedHeadPosition.z;
            _poseSample[28] = TrackedHeadRotation.w;
            _poseSample[29] = TrackedHeadRotation.x;
            _poseSample[30] = TrackedHeadRotation.y;
            _poseSample[31] = TrackedHeadRotation.z;

            _poseSample[32] = TrackedLeftHandPosition.x;
            _poseSample[33] = TrackedLeftHandPosition.y;
            _poseSample[34] = TrackedLeftHandPosition.z;
            _poseSample[35] = TrackedLeftHandRotation.w;
            _poseSample[36] = TrackedLeftHandRotation.x;
            _poseSample[37] = TrackedLeftHandRotation.y;
            _poseSample[38] = TrackedLeftHandRotation.z;

            _poseSample[39] = TrackedRightHandPosition.x;
            _poseSample[40] = TrackedRightHandPosition.y;
            _poseSample[41] = TrackedRightHandPosition.z;
            _poseSample[42] = TrackedRightHandRotation.w;
            _poseSample[43] = TrackedRightHandRotation.x;
            _poseSample[44] = TrackedRightHandRotation.y;
            _poseSample[45] = TrackedRightHandRotation.z;

            _poseOutlet.push_sample(_poseSample, TimeSync.Instance.LateUpdateTimeStamp);
        }
        
        public void RecordSurveyData(string surveyType, string value)
        {
            if (!_enabled)
            {
                return;
            }
            
            _surveySample[4] = surveyType;
            _surveySample[5] = value;
            _surveyOutlet.push_sample(_surveySample);
        }

        public void RecordLuminanceData(float luminance)
        {
            _luminanceSample[4] = luminance;
            _luminanceOutlet.push_sample(_luminanceSample, TimeSync.Instance.LateUpdateTimeStamp);
        }

        public void RecordGazeData()
        {
            if (!_enabled)
            {
                return;
            }

            _eyeData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);
            SRanipal_Eye.GetVerboseData(out _pupilData);
            CurrentCombinedGazeDirection = _eyeData.GazeRay.Direction;

            _gazeSample[4] = _eyeData.ConvergenceDistance;
            _gazeSample[5] = Convert.ToSingle(_eyeData.ConvergenceDistanceIsValid);

            _gazeSample[6] = _eyeData.GazeRay.Origin.x;
            _gazeSample[7] = _eyeData.GazeRay.Origin.y;
            _gazeSample[8] = _eyeData.GazeRay.Origin.z;

            _gazeSample[9] = _eyeData.GazeRay.Direction.x;
            _gazeSample[10] = _eyeData.GazeRay.Direction.y;
            _gazeSample[11] = _eyeData.GazeRay.Direction.z;

            _gazeSample[12] = Convert.ToSingle(_eyeData.GazeRay.IsValid);
            _gazeSample[13] = Convert.ToSingle(_eyeData.IsLeftEyeBlinking);
            _gazeSample[14] = Convert.ToSingle(_eyeData.IsRightEyeBlinking);

            _gazeSample[15] = _pupilData.left.pupil_diameter_mm;
            _gazeSample[16] = _pupilData.right.pupil_diameter_mm;

            _gazeOutlet.push_sample(_gazeSample, TimeSync.Instance.LateUpdateTimeStamp);
        }
        
        public void RecordExperimentEvent(string eventName, string eventType)
        {
            _experimentSample[4] = eventName;
            _experimentSample[5] = eventType;
            _experimentOutlet.push_sample(_experimentSample);
        }
    }
}