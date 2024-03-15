namespace PathNav
{
    using ExperimentControl;
    using System.Diagnostics;
    using UnityEngine.UI;
    using UnityEngine;
    public class CalibrationManager : MonoBehaviour
    {
        [SerializeField] private Image calibrationImage;

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
            ExperimentDataLogger.Instance.Enable("Calibration", "Calibration");
            calibrationImage.color = _calibrationColors[_calibrationIndex];
            _calibrationStopwatch  = new Stopwatch();
            InvokeRepeating(nameof(LogLuminance), 0.5f, 1.0f);
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

        protected void LogLuminance()
        {
            ExperimentDataLogger.Instance.WriteLuminanceData();
        }

        protected void FixedUpdate()
        {
            ExperimentDataLogger.Instance.RecordGazeData();
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

        private async void EndCalibration()
        {
            await ExperimentDataLogger.Instance.WriteGazeData();
            await ExperimentDataLogger.Instance.WriteAllData();
            ExperimentDataLogger.Instance.Disable();
            ExperimentDataManager.Instance.CalibrationComplete();
        }
    }
}