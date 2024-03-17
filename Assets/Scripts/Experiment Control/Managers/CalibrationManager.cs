using UnityEngine.InputSystem.XR;

namespace PathNav.ExperimentControl
{
    using System.Diagnostics;
    using UnityEngine.UI;
    using UnityEngine;
    public class CalibrationManager : MonoBehaviour
    {
        [SerializeField] private Image calibrationImage;
        
        [Header("Data Logging Variables")]
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;
        [SerializeField] private TrackedPoseDriver headPoseDriver;
        [SerializeField] private TrackedPoseDriver leftHandPoseDriver;
        [SerializeField] private TrackedPoseDriver rightHandPoseDriver;

        private Color[] _calibrationColors = new Color[]
        {
            new(127.0f / 255.0f, 127.0f / 255.0f, 127.0f / 255.0f, 1.0f),
            new(27.0f  / 255.0f, 27.0f  / 255.0f, 27.0f  / 255.0f, 1.0f),
            new(190.0f / 255.0f, 190.0f / 255.0f, 190.0f / 255.0f, 1.0f),
            new(76.0f  / 255.0f, 76.0f  / 255.0f, 76.0f  / 255.0f, 1.0f),
            new(229.0f / 255.0f, 229.0f / 255.0f, 229.0f / 255.0f, 1.0f),
            new(0.0f   / 255.0f, 0.0f   / 255.0f, 0.0f   / 255.0f, 1.0f),
            new(216.0f / 255.0f, 216.0f / 255.0f, 216.0f / 255.0f, 1.0f),
            new(166.0f / 255.0f, 166.0f / 255.0f, 166.0f / 255.0f, 1.0f),
        };

        private int _calibrationIndex = 0;

        private Stopwatch _calibrationStopwatch;
        
        protected void Start()
        {
            if(ExperimentDataLogger.Instance == null)
            {
                UnityEngine.Debug.LogError("ExperimentDataLogger is not present in the scene. Disabling CalibrationManager.");
                enabled = false;
                return;
            }
            ExperimentDataLogger.Instance.SetTransformData(headTransform, leftHand, rightHand);
            ExperimentDataLogger.Instance.SetPoseDriverData(headPoseDriver, leftHandPoseDriver, rightHandPoseDriver);
            ExperimentDataLogger.Instance.Enable(99.9f, 99.9f);
            calibrationImage.color = _calibrationColors[_calibrationIndex];
            _calibrationStopwatch  = new Stopwatch();
            _calibrationStopwatch.Start();
        }

        protected void Update()
        {
            //Check if 6 seconds have elapsed
            if (_calibrationStopwatch.ElapsedMilliseconds > 3000)
            {
                UpdateCalibrationColor();
                _calibrationStopwatch.Reset();
                _calibrationStopwatch.Start();
            }
        }

        protected void LateUpdate()
        {
            ExperimentDataLogger.Instance.RecordGazeData();
            ExperimentDataLogger.Instance.RecordPoseData();
        }

        private void UpdateCalibrationColor()
        {
            _calibrationIndex++;

            if (_calibrationIndex < _calibrationColors.Length)
            {
                calibrationImage.color = _calibrationColors[_calibrationIndex];
            }
            else
            {
                EndCalibration();
            }
        }

        private void EndCalibration()
        {
            ExperimentDataLogger.Instance.Disable();
            ExperimentDataManager.Instance.CalibrationComplete();
        }
    }
}