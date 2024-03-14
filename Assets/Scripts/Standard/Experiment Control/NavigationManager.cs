namespace PathNav.ExperimentControl
{
    using CsvHelper;
    using CsvHelper.Configuration;
    using Dreamteck.Splines;
    using Events;
    using Interaction;
    using SceneManagement;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;
    using Tobii.XR;
    using UnityEngine;
    using UnityEngine.Animations;
    using UnityEngine.InputSystem.XR;
    using UnityEngine.XR.OpenXR.Samples.ControllerSample;
    using ViveSR.anipal.Eye;
    using Debug = UnityEngine.Debug;

    public class NavigationManager : MonoBehaviour
    {
        [SerializeField] private List<AudioClipMap> audioMap;
        [SerializeField] private SplineComputer targetSpline;
        [SerializeField] private SplineFollower follower;
        [SerializeField] private SplineProjector projector;
        
        [SerializeField] private Transform teleportLocation;
        [SerializeField] private Teleporter teleporter;
        [SerializeField] private ParentConstraint parentConstraint;
        [SerializeField] private GameObject discomfortScore;
        [SerializeField] private GameObject seq;
        [SerializeField] private Overlay overlay;
        [SerializeField] private PointerEvaluator pointerLeft;
        [SerializeField] private PointerEvaluator pointerRight;
        [SerializeField] private NavigationEndPoint endPoint;
        [SerializeField] private Transform footVisualMarker;

        
        [Header("Data Logging Variables")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;
        [SerializeField] private TrackedPoseDriver headPoseDriver;
        [SerializeField] private TrackedPoseDriver leftHandPoseDriver;
        [SerializeField] private TrackedPoseDriver rightHandPoseDriver;
        
        private SplineComputer _splineComputer;
        private SplinePoint[] _spline;
        private Vector3 _deltaTranslation;
        private float _deltaScale;
        private LocomotionDof _locomotionDof;

        private Stopwatch _taskTimerTotal;
        
        private NavigationDataFormat _navigationData;
        private GazeDataFormat _gazeData;
        private PoseDataFormat _poseData;
        
        private static string _navFile, _gazeFile, _poseFile;
        private static EyeData _pupilData = new();
        private static TobiiXR_EyeTrackingData _eyeData = new();
        private static readonly CsvConfiguration Config = new(CultureInfo.InvariantCulture);
        private bool _recordData;

        private int _userId, _blockId;
        private string _modelId, _methodId;
        
        private Queue<GazeDataFormat> _gazeQueue;
        private Queue<PoseDataFormat> _poseQueue;
        private Queue<NavigationDataFormat> _navigationQueue;
        
        private Vector3 HeadPosition => playerTransform.position;
        private Quaternion HeadRotation => playerTransform.rotation;
        private Vector3 LeftHandPosition => leftHand.position;
        private Quaternion LeftHandRotation => leftHand.rotation;
        private Vector3 RightHandPosition => rightHand.position;
        private Quaternion RightHandRotation => rightHand.rotation;

        #region Enable/Disable/Update
        internal void Enable()
        {
            _taskTimerTotal = new Stopwatch();
            _locomotionDof  = ExperimentDataManager.Instance.GetNavigationMethod();

            StartNavigation();
        }

        private async void OnDisable()
        {
            UnsubscribeToEvents();
            
            if (_gazeQueue.Count > 0)
            {
                Config.BufferSize = 2048;
                await WriteData<GazeDataFormatMap>(_gazeFile, _gazeQueue);
                _gazeQueue.Clear();
            }
            
            if (_poseQueue.Count > 0)
            {
                Config.BufferSize = 2048;
                await WriteData<PoseDataFormatMap>(_poseFile, _poseQueue);
                _poseQueue.Clear();
            }
            
            if (_navigationQueue.Count > 0)
            {
                Config.BufferSize = 2048;
                await WriteData<NavigationDataFormatMap>(_navFile, _navigationQueue);
                _navigationQueue.Clear();
            }
        }

        private async void OnApplicationQuit()
        {
            if (_gazeQueue.Count > 0)
            {
                Config.BufferSize = 2048;
                await WriteData<GazeDataFormatMap>(_gazeFile, _gazeQueue);
                _gazeQueue.Clear();
            }
            
            if (_poseQueue.Count > 0)
            {
                Config.BufferSize = 2048;
                await WriteData<PoseDataFormatMap>(_poseFile, _poseQueue);
                _poseQueue.Clear();
            }
            
            if (_navigationQueue.Count > 0)
            {
                Config.BufferSize = 2048;
                await WriteData<NavigationDataFormatMap>(_navFile, _navigationQueue);
                _navigationQueue.Clear();
            }
        }

        private void LateUpdate()
        {
            if (!_recordData) return;

            QueueData();
        }
        #endregion

        #region Event Subscription Management
        private void SubscribeToEvents()
        {
            EventManager.Subscribe<SceneControlEventArgs>(EventId.SeqComplete,             SeqComplete);
            EventManager.Subscribe<SceneControlEventArgs>(EventId.DiscomfortScoreComplete, DiscomfortComplete);
        }

        private void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<SceneControlEventArgs>(EventId.SeqComplete,             SeqComplete);
            EventManager.Unsubscribe<SceneControlEventArgs>(EventId.DiscomfortScoreComplete, DiscomfortComplete);
        }

        private static SceneControlEventArgs GetSceneControlEventArgs() => new();
        #endregion

        #region Event Callbacks
        private void SeqComplete(object sender, SceneControlEventArgs args)
        {
            seq.SetActive(false);
            EndNavigation();
        }

        private void DiscomfortComplete(object sender, SceneControlEventArgs args)
        {
            discomfortScore.SetActive(false);
            seq.SetActive(true);
        }

        public void OnEndReached()
        {
            if (_locomotionDof != LocomotionDof.FourDoF) return;

            NavigationComplete();
        }

        public void OnEndCollision()
        {
            if (_locomotionDof != LocomotionDof.SixDof) return;

            NavigationComplete();
        }

        private void NavigationComplete()
        {
            _taskTimerTotal.Stop();
            EventManager.Publish(EventId.SplineNavigationComplete, this, GetSceneControlEventArgs());
            ExperimentDataManager.Instance.RecordNavigationData(_taskTimerTotal.Elapsed);
            ActionAssetEnabler actionController = FindObjectOfType<ActionAssetEnabler>();
            actionController.EnableUiInput();
            discomfortScore.SetActive(true);
            pointerLeft.Enable();
            pointerRight.Enable();
        }
        #endregion

        #region Logic
        private async void StartNavigation()
        {
            InitializeDataFiles();
            
            await Task.Delay(10);

            SetSpline();
            
            await Task.Delay(50);

            SubscribeToEvents();
            
            await Task.Delay(10);

            SetupNavigation();
            
            await Task.Delay(500);

            follower.followSpeed              = 0;
            parentConstraint.constraintActive = _locomotionDof == LocomotionDof.FourDoF;
            follower.follow                   = true; //_locomotionDof == LocomotionDof.FourDoF;

            if (_locomotionDof == LocomotionDof.SixDof)
            {
                pointerLeft.EnableLocomotion();
                pointerRight.EnableLocomotion();
            }

            overlay.FadeToClear();

            await Task.Delay(1000);

            _taskTimerTotal.Start();
            _recordData = true;
        }

        private async void EndNavigation()
        {
            await Task.Delay(100);

            overlay.FadeToBlack();

            await Task.Delay(500);

            ExperimentDataManager.Instance.NavigationComplete();
        }

        private void SetSpline()
        {
            if (ExperimentDataManager.Instance != null)
            {
                _spline           = ExperimentDataManager.Instance.GetSavedSpline();
                _deltaTranslation = ExperimentDataManager.Instance.GetSplineModel().Translation;
                _deltaScale       = ExperimentDataManager.Instance.GetSplineModel().Scale;

                SetupSpline();
            }
            else
            {
                throw new Exception("SceneDataManager is null!");
            }
        }

        private void SetupSpline()
        {
            SplinePoint[] points         = _spline;
            var           newPoints      = new SplinePoint[points.Length];
            var           pointPositions = new Vector3[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                Vector3 newPose = points[i].position;
                newPose           += _deltaTranslation;
                newPose           /= _deltaScale;
                pointPositions[i] =  newPose;

                SplinePoint pt = new()
                {
                    color    = Color.white,
                    normal   = Vector3.up, //points[i].normal,
                    size     = 0.01f,
                    tangent  = default, //points[i].tangent,
                    tangent2 = default, //points[i].tangent2,
                    position = pointPositions[i],
                };
                newPoints[i] = pt;
            }

            targetSpline.SetPoints(newPoints);
            EventManager.Publish(EventId.FollowPathReady, this, GetSceneControlEventArgs());
        }

        public void StopImmediately()
        {
            _recordData = false;
            overlay.FadeToBlackImmediate();
            _taskTimerTotal.Stop();

            EventManager.Publish(EventId.SplineNavigationComplete, this, GetSceneControlEventArgs());
            UnsubscribeToEvents();

            ExperimentDataManager.Instance.RecordNavigationData(_taskTimerTotal.Elapsed);
            ExperimentDataManager.Instance.RecordDiscomfortScore(10);
            ExperimentDataManager.Instance.EndExperimentImmediately();
        }

        private bool CheckTeleportation()
        {
            if (teleportLocation is not null)
            {
                if (teleporter is null)
                {
                    teleporter = FindObjectOfType<Teleporter>();

                    if (teleporter is null)
                    {
                        Debug.LogError("Teleporter not found in scene!");
                    }
                }
            }

            return (teleporter is not null) && (teleportLocation is not null);
        }

        private void SetupNavigation()
        {
            if (!CheckTeleportation()) return;

            float height = ExperimentDataManager.Instance.GetHeight();
            footVisualMarker.localPosition =  new Vector3(0, -height, 0);
            height                         *= 0.5f;
            SplineSample sample = follower.spline.Evaluate(0);
            teleportLocation.position = sample.position + new Vector3(0, height, 0);
            teleportLocation.forward  = sample.forward;
            teleporter.Teleport(teleportLocation);

            sample = follower.spline.Evaluate(follower.spline.pointCount - 1);
            endPoint.Place(sample.position);
        }
        #endregion

        #region Data Logging
        private void InitializeDataFiles()
        {
            _gazeQueue       = new Queue<GazeDataFormat>();
            _poseQueue       = new Queue<PoseDataFormat>();
            _navigationQueue = new Queue<NavigationDataFormat>();
            
            _navFile = InitializeDataLog<NavigationDataFormatMap, NavigationDataFormat>(ExperimentDataManager.Instance.GetLogDirectory(), ExperimentDataManager.Instance.GetNavigationLogFilePath());
            _gazeFile = InitializeDataLog<GazeDataFormatMap, GazeDataFormat>(ExperimentDataManager.Instance.GetLogDirectory(), ExperimentDataManager.Instance.GetGazeLogFilePath());
            _poseFile = InitializeDataLog<PoseDataFormatMap, PoseDataFormat>(ExperimentDataManager.Instance.GetLogDirectory(), ExperimentDataManager.Instance.GetPoseLogFilePath());
            
            Config.HasHeaderRecord = true;
            _userId                = ExperimentDataManager.Instance.GetId();
            _blockId               = ExperimentDataManager.Instance.GetBlock();
            _modelId               = ExperimentDataManager.Instance.GetModel();
            _methodId              = ExperimentDataManager.Instance.GetNavigationMethodString();
        }

        private static string InitializeDataLog<T1, T2>(string logDirectory, string filePath) where T1 : ClassMap<T2>
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
            
            csvWriter.Context.RegisterClassMap<T1>();
            csvWriter.WriteHeader<T2>();
            csvWriter.NextRecord();
            
            return filePath;
        }
        
        private void QueueData()
        {
            _gazeData = new GazeDataFormat
            {
                ID       = _userId,
                BLOCK_ID = _blockId,
                MODEL    = _modelId,
                METHOD   = _methodId,
            };
            
            _poseData = new PoseDataFormat
            {
                ID       = _userId,
                BLOCK_ID = _blockId,
                MODEL    = _modelId,
                METHOD   = _methodId,
            };
            
            _navigationData = new NavigationDataFormat
            {
                ID       = _userId,
                BLOCK_ID = _blockId,
                MODEL    = _modelId,
                METHOD   = _methodId,
            };
            
            double time = LSL.LSL.local_clock();
            GetGazeData();
            GetPoseData();
            GetNavigationData();
            
            _gazeData.TIMESTAMP       = time;
            _poseData.TIMESTAMP       = time;
            _navigationData.TIMESTAMP = time;
            
            _gazeQueue.Enqueue(_gazeData);
            _poseQueue.Enqueue(_poseData);
            _navigationQueue.Enqueue(_navigationData);
        }

        private void GetGazeData()
        {
            _eyeData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);
            SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out Vector3 _, out Vector3 _, _pupilData);
            
            _gazeData.CONVERGENCE_DISTANCE = _eyeData.ConvergenceDistance;
            _gazeData.CONVERGENCE_VALID    = _eyeData.ConvergenceDistanceIsValid;
            
            _gazeData.GAZERAY_ORIGIN_X     = _eyeData.GazeRay.Origin.x;
            _gazeData.GAZERAY_ORIGIN_Y     = _eyeData.GazeRay.Origin.y;
            _gazeData.GAZERAY_ORIGIN_Z     = _eyeData.GazeRay.Origin.z;
            _gazeData.GAZERAY_DIRECTION_X  = _eyeData.GazeRay.Direction.x;
            _gazeData.GAZERAY_DIRECTION_Y  = _eyeData.GazeRay.Direction.y;
            _gazeData.GAZERAY_DIRECTION_Z  = _eyeData.GazeRay.Direction.z;
            _gazeData.GAZERAY_VALID        = _eyeData.GazeRay.IsValid;
            
            _gazeData.LEFT_IS_BLINKING     = _eyeData.IsLeftEyeBlinking;
            _gazeData.RIGHT_IS_BLINKING     = _eyeData.IsRightEyeBlinking;
            
            _gazeData.LEFT_EYE_PUPIL_DIAMETER = _pupilData.verbose_data.left.pupil_diameter_mm;
            _gazeData.RIGHT_EYE_PUPIL_DIAMETER = _pupilData.verbose_data.right.pupil_diameter_mm;
        }

        private void GetPoseData()
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

        private void GetNavigationData()
        {
            _navigationData.SPEED = follower.followSpeed;
            _navigationData.SPLINE_POSITION = _locomotionDof == LocomotionDof.FourDoF ? follower.result.position.ToString("F3") : projector.result.position.ToString("F3");
            _navigationData.SPLINE_PERCENT = _locomotionDof == LocomotionDof.FourDoF ? follower.result.percent : projector.result.percent;
        }
        
        private static async Task WriteData<T>(string file, IEnumerable queue) where T : ClassMap
        {
            await using StreamWriter streamWriter = new(file, true);
            await using CsvWriter    csvWriter    = new(streamWriter, Config);
            
            csvWriter.Context.RegisterClassMap<T>();
            await csvWriter.WriteRecordsAsync(queue);
        }
        
        #endregion
    }
}