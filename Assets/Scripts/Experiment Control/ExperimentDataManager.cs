using UnityEngine.XR;
namespace PathNav.ExperimentControl
{
    #region Imports
    using Dreamteck.Splines;
    using Events;
    using global::ExperimentControl;
    using Interaction;
    using PathPlanning;
    using SceneManagement;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tobii.XR;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    #endregion

    [DefaultExecutionOrder(-10)]
    public class ExperimentDataManager : MonoBehaviour
    {
        private enum TrialState
        {
            Tutorial,
            Trial,
        }

        [SerializeField] private ConditionBlock[] conditionBlocks;
        public TobiiXR_Settings settings;
        public static ExperimentDataManager Instance { get; private set; }

        private static UserInfo _userInfo;

        private static SplinePoint[] _drawingModelASpline;
        private static SplinePoint[] _drawingModelBSpline;
        private static SplinePoint[] _drawingModelCSpline;
        private static SplinePoint[] _drawingModelDSpline;

        private static SplinePoint[] _interpolationModelASpline;
        private static SplinePoint[] _interpolationModelBSpline;
        private static SplinePoint[] _interpolationModelCSpline;
        private static SplinePoint[] _interpolationModelDSpline;

        private static SplinePoint[] _savedSpline;
        private static Model _savedModel;

        private static int _userId;
        private static Handedness _handedness;
        private static bool _useSplineFile;

        private static TrialState _trialState;
        private static Trial _currentTrial;
        private static ConditionBlock _conditionBlock;
        
        private static string _logDirectory;
        private static string _logDirectorySpline;
        private static string _logFilePathSpline;

        //trialCount
        private static int _currentTrialCount;
        private static int _modelIndex;

        //trialIndex
        private static int _currentTrialStageIndex;

        private static string _activeSceneName;
        private const int _totalTrialCount = 4;
        private const int _totalTrialStages = 4;

        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
                TobiiXR.Start(settings);
                _logDirectory                   =  Application.persistentDataPath + "/Data/";
                SceneManager.activeSceneChanged += OnActiveSceneChanged;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RecordUserId(int id, float height)
        {
            _userInfo       = new UserInfo(id, height);
            _userId         = _userInfo.UserId;
            _conditionBlock = conditionBlocks[_userInfo.BlockId];

            _logDirectorySpline = $"{_logDirectory}{_userId}_splines/";
            
            ExperimentDataLogger logger = new (_userId, _userInfo.BlockId);
        }

        public void RecordHandedness(bool useLeftHand)
        {
            _handedness = useLeftHand ? Handedness.Left : Handedness.Right;
            ExperimentDataLogger.Instance.RecordExperimentEvent($"HandednessSet_{(useLeftHand ? "Left" : "Right")}", "Handedness");
        }

        public void BeginSession()
        {
            RunGazeCalibration();
        }
        
        #region Spline
        public void SaveSplineComputer(SplineComputer splineToSave)
        {
            if (_trialState != TrialState.Trial) return;

            switch (_conditionBlock.GetCurrentModel(_currentTrialStageIndex, _modelIndex).Id)
            {
                case "Model_A":
                    switch (_currentTrial.pathStrategy)
                    {
                        case PathStrategy.Bulldozer:
                            _drawingModelASpline = splineToSave.GetPoints();
                            _logFilePathSpline   = $"{_logDirectorySpline}drawing_A.csv";
                            break;
                        case PathStrategy.Spatula:
                            _interpolationModelASpline = splineToSave.GetPoints();
                            _logFilePathSpline         = $"{_logDirectorySpline}interpolating_A.csv";
                            break;
                        case PathStrategy.None:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case "Model_B":
                    switch (_currentTrial.pathStrategy)
                    {
                        case PathStrategy.Bulldozer:
                            _drawingModelBSpline = splineToSave.GetPoints();
                            _logFilePathSpline   = $"{_logDirectorySpline}drawing_B.csv";
                            break;
                        case PathStrategy.Spatula:
                            _interpolationModelBSpline = splineToSave.GetPoints();
                            _logFilePathSpline         = $"{_logDirectorySpline}interpolating_B.csv";
                            break;
                        case PathStrategy.None:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case "Model_C":
                    switch (_currentTrial.pathStrategy)
                    {
                        case PathStrategy.Bulldozer:
                            _drawingModelCSpline = splineToSave.GetPoints();
                            _logFilePathSpline   = $"{_logDirectorySpline}drawing_C.csv";
                            break;
                        case PathStrategy.Spatula:
                            _interpolationModelCSpline = splineToSave.GetPoints();
                            _logFilePathSpline         = $"{_logDirectorySpline}interpolating_C.csv";
                            break;
                        case PathStrategy.None:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case "Model_D":
                    switch (_currentTrial.pathStrategy)
                    {
                        case PathStrategy.Bulldozer:
                            _drawingModelDSpline = splineToSave.GetPoints();
                            _logFilePathSpline   = $"{_logDirectorySpline}drawing_D.csv";
                            break;
                        case PathStrategy.Spatula:
                            _interpolationModelDSpline = splineToSave.GetPoints();
                            _logFilePathSpline         = $"{_logDirectorySpline}interpolating_D.csv";
                            break;
                        case PathStrategy.None:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
            }

            WriteSpline(splineToSave.GetPoints());
        }

        private static async void WriteSpline(IEnumerable<SplinePoint> splinePoints)
        {
            await CsvLogger.LogSpline(_logDirectorySpline, _logFilePathSpline, splinePoints);
        }

        [Flags]
        public enum ModelSource
        {
            None = 0,
            A    = 1,
            B    = 2,
            C    = 4,
            D    = 8,
            Drawing = 16,
            Interpolating = 32,
        }

        private ModelSource _modelSource;
        public ModelSource GetModelSource() => _modelSource;
        public SplinePoint[] GetSavedSpline()
        {
            _savedModel = _conditionBlock.GetCurrentModel(_currentTrialStageIndex, _modelIndex);
            _modelSource = ModelSource.None;

            if (_useSplineFile)
            {
                string splineFile = _savedModel.Id switch
                                    {
                                        "Model_A" => _currentTrialCount % 2 == 0
                                            ? $"{_logDirectorySpline}drawing_A.csv"
                                            : $"{_logDirectorySpline}interpolating_A.csv",
                                        "Model_B" => _currentTrialCount % 2 == 0
                                            ? $"{_logDirectorySpline}drawing_B.csv"
                                            : $"{_logDirectorySpline}interpolating_B.csv",
                                        "Model_C" => _currentTrialCount % 2 == 0
                                            ? $"{_logDirectorySpline}drawing_C.csv"
                                            : $"{_logDirectorySpline}interpolating_C.csv",
                                        "Model_D" => _currentTrialCount % 2 == 0
                                            ? $"{_logDirectorySpline}drawing_D.csv"
                                            : $"{_logDirectorySpline}interpolating_D.csv",
                                        _ => throw new ArgumentOutOfRangeException(),
                                    };
                SplinePoint[] splinePoints = CsvLogger.ReadSpline(splineFile);
                _savedSpline = splinePoints;
            }
            else
            {
                switch (_savedModel.Id)
                {
                    case "Model_A":
                        _modelSource =  ModelSource.A;
                        _savedSpline =  _currentTrialStageIndex % 2 == 0 ? _drawingModelASpline : _interpolationModelASpline;
                        _modelSource |= _currentTrialStageIndex % 2 == 0 ? ModelSource.Drawing : ModelSource.Interpolating;
                        break;
                    case "Model_B":
                        _modelSource =  ModelSource.B;
                        _savedSpline =  _currentTrialStageIndex % 2 == 0 ? _drawingModelBSpline : _interpolationModelBSpline;
                        _modelSource |= _currentTrialStageIndex % 2 == 0 ? ModelSource.Drawing : ModelSource.Interpolating;
                        
                        break;
                    case "Model_C":
                        _modelSource =  ModelSource.C;
                        _savedSpline =  _currentTrialStageIndex % 2 == 0 ? _drawingModelCSpline : _interpolationModelCSpline;
                        _modelSource |= _currentTrialStageIndex % 2 == 0 ? ModelSource.Drawing : ModelSource.Interpolating;
                        break;
                    case "Model_D":
                        _modelSource =  ModelSource.D;
                        _savedSpline =  _currentTrialStageIndex % 2 == 0 ? _drawingModelDSpline : _interpolationModelDSpline;
                        _modelSource |= _currentTrialStageIndex % 2 == 0 ? ModelSource.Drawing : ModelSource.Interpolating;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return _savedSpline;
        }

        public Model GetSplineModel() => _savedModel;
        #endregion

        #region Getters
        //Model Map:
        //Model_A: 0
        //Model_B: 1
        //Model_C: 2
        //Model_D: 3
        //Tutorial: 4
        //Calibration: 99
        
        //Method Map:
        //Bulldozer: 0
        //Spatula: 1
        //FourDoF: 2
        //SixDoF: 3
        //Calibration: 99
        private int GetModelInt()
        {
            return _trialState switch
                   {
                       TrialState.Tutorial => 4,
                       TrialState.Trial    => _modelIndex,
                       _                   => throw new ArgumentOutOfRangeException()
                   };
        }

        private int GetCreationMethodInt()
        {
            switch (_currentTrial.pathStrategy)
            {
                case PathStrategy.Bulldozer:
                    return 0;
                case PathStrategy.Spatula:
                    return 1;
                case PathStrategy.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int GetNavigationMethodInt()
        {
            switch (_currentTrial.locomotionDof)
            {
                case LocomotionDof.FourDoF:
                    return 2;
                case LocomotionDof.SixDof:
                    return 3;
                case LocomotionDof.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public float         GetHeight()                 => _userInfo.Height;
        public PathStrategy  GetCreationMethod()         => _currentTrial.pathStrategy;
        public LocomotionDof GetNavigationMethod()       => _currentTrial.locomotionDof;
        public bool UseTargetPoints1()
        {
            bool useTargetSet1;
            if (_userId % 2 == 0)
            {
                useTargetSet1 = _currentTrialStageIndex == 0;
                ExperimentDataLogger.Instance.RecordExperimentEvent($"TargetsLoaded_Set{(useTargetSet1 ? "1" : "2")}", "TargetPointsSet");
                return useTargetSet1;
            }

            useTargetSet1 = _currentTrialStageIndex != 0;
            ExperimentDataLogger.Instance.RecordExperimentEvent($"TargetsLoaded_Set{(useTargetSet1 ? "1" : "2")}", "TargetPointsSet");
            return useTargetSet1;
        }
        public bool CreationTutorialActive()
        {
            if (_currentTrial.trialType != TrialType.PathCreation) return false;

            return _trialState == TrialState.Tutorial;
        }
        #endregion

        #region Scene Control
        private void SetupTutorialCreation()
        {
            ExperimentDataLogger.Instance.RecordExperimentEvent($"SetupTutorialCreation", "Setup");
            
            TutorialCreation tutorialCreation = FindObjectOfType<TutorialCreation>();
            tutorialCreation.Enable();

            EventManager.Publish(EventId.SetPathStrategy, this, new SceneControlEventArgs(_currentTrial.pathStrategy, _handedness));
        }

        private void SetupTutorialNavigation()
        {
            ExperimentDataLogger.Instance.RecordExperimentEvent($"SetupTutorialNavigation", "Setup");
            
            TutorialNavigation tutorialNavigation = FindObjectOfType<TutorialNavigation>();
            tutorialNavigation.Enable();

            EventManager.Publish(EventId.SetLocomotionStrategy, this, new SceneControlEventArgs(_currentTrial.locomotionDof, _handedness));
        }

        private void SetupCreation()
        {
            ExperimentDataLogger.Instance.RecordExperimentEvent($"SetupCreation", "Setup");
            
            GameObject holder = new();

            CreationActionMonitor creationActionMonitor = holder.AddComponent<CreationActionMonitor>();
            creationActionMonitor.Enable();

            CreationManager creationManager = FindObjectOfType<CreationManager>();
            creationManager.Enable();

            EventManager.Publish(EventId.SetPathStrategy, this, new SceneControlEventArgs(_currentTrial.pathStrategy, _handedness));
        }

        private void SetupNavigation()
        {
            ExperimentDataLogger.Instance.RecordExperimentEvent($"SetupNavigation", "Setup");
            
            NavigationManager navigationManager = FindObjectOfType<NavigationManager>();
            navigationManager.Enable();

            EventManager.Publish(EventId.SetLocomotionStrategy, this, new SceneControlEventArgs(_currentTrial.locomotionDof, _handedness));

            EventManager.Publish(EventId.EnableLocomotion, this, EventArgs.Empty);
        }

        private void RunGazeCalibration()
        {
            StartCoroutine(PlayNextScene("Calibration"));
        }

        internal void TutorialComplete()
        {
            string trialType = _currentTrial.trialType switch
                               {
                                   TrialType.PathCreation   => "Creation",
                                   TrialType.PathNavigation => "Navigation",
                                   _                        => throw new ArgumentOutOfRangeException()
                               };
            ExperimentDataLogger.Instance.RecordExperimentEvent($"{trialType}_Tutorial_Complete", "Completion");
            ExperimentDataLogger.Instance.ResetSceneVariables();
            
            _trialState        = TrialState.Trial;
            _currentTrialCount = 0;
            _modelIndex        = 0;
            LoadNextScene();
        }

        internal void CalibrationComplete()
        {
            ExperimentDataLogger.Instance.RecordExperimentEvent("CalibrationComplete", "Completion");
            ExperimentDataLogger.Instance.ResetSceneVariables();
            
            _currentTrialStageIndex = 0;
            _currentTrialCount      = 0;
            _modelIndex             = 0;

            _currentTrial = _conditionBlock.GetCurrentTrial(_currentTrialStageIndex);

            _trialState = TrialState.Tutorial;

            LoadNextScene();
        }

        internal void CreationComplete()
        {
            ExperimentDataLogger.Instance.RecordExperimentEvent("CreationComplete", "Completion");
            ExperimentDataLogger.Instance.ResetSceneVariables();
            IncrementTrial();
        }

        internal void NavigationComplete()
        {
            ExperimentDataLogger.Instance.RecordExperimentEvent("NavigationComplete", "Completion");
            ExperimentDataLogger.Instance.ResetSceneVariables();
            IncrementTrial();
        }

        internal void EndExperimentImmediately()
        {
            
            ExperimentDataLogger.Instance.RecordExperimentEvent("EarlyTermination", "EndExperiment");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void IncrementTrial()
        {
            _currentTrialCount++;

            if (_currentTrialCount < _totalTrialCount)
            {
                _modelIndex++;
            }
            else
            {
                _currentTrialStageIndex++;

                if (_currentTrialStageIndex < _totalTrialStages)
                {
                    _currentTrialCount = 0;
                    _modelIndex        = 0;
                    _currentTrial      = _conditionBlock.GetCurrentTrial(_currentTrialStageIndex);
                    _trialState        = TrialState.Tutorial;
                }
                else
                {
                    EndExperiment();
                    return;
                }
            }

            LoadNextScene();
        }

        private string GetSceneId()
        {
            string trial = _currentTrial.GetTrialTypeString();
            string model = _conditionBlock.GetCurrentModel(_currentTrialStageIndex, _modelIndex).Id;

            return $"{trial}_{model}";
        }

        private void LoadNextScene()
        {
            string sceneId = _currentTrial.trialType switch
                             {
                                 TrialType.PathCreation   => _trialState == TrialState.Tutorial ? "Creation_Tutorial" : GetSceneId(),
                                 TrialType.PathNavigation => _trialState == TrialState.Tutorial ? "Navigation_Tutorial" : GetSceneId(),
                                 _                        => throw new ArgumentOutOfRangeException()
                             };

            StartCoroutine(PlayNextScene(sceneId));
        }
        
        //Model Map:
        //Model_A: 0
        //Model_B: 1
        //Model_C: 2
        //Model_D: 3
        //Tutorial: 4
        //Calibration: 99
        
        //Method Map:
        //Bulldozer: 0
        //Spatula: 1
        //FourDoF: 2
        //SixDoF: 3
        //Calibration: 99
        private void OnActiveSceneChanged(Scene replaced, Scene next)
        {
            _activeSceneName = next.name;

            if (_activeSceneName.Contains("Loading")) return;
            if (_activeSceneName.Contains("Calibration"))
            {
                ExperimentDataLogger.Instance.SetSceneVariables(99.0f, 99.0f);
                ExperimentDataLogger.Instance.RecordExperimentEvent("Calibration", "SceneLoaded");
                return;
            }

            if (_activeSceneName.Contains("Creation_Tutorial"))
            {
                ExperimentDataLogger.Instance.SetSceneVariables(GetModelInt(), GetCreationMethodInt());
                ExperimentDataLogger.Instance.RecordExperimentEvent("Creation_Tutorial", "SceneLoaded");
                SetupTutorialCreation();
                return;
            }

            if (_activeSceneName.Contains("Navigation_Tutorial"))
            {
                ExperimentDataLogger.Instance.SetSceneVariables(GetModelInt(), GetNavigationMethodInt());
                ExperimentDataLogger.Instance.RecordExperimentEvent("Navigation_Tutorial", "SceneLoaded");
                SetupTutorialNavigation();
                return;
            }

            if (_activeSceneName.Contains("Creation"))
            {
                ExperimentDataLogger.Instance.SetSceneVariables(GetModelInt(), GetCreationMethodInt());
                ExperimentDataLogger.Instance.RecordExperimentEvent("Creation_Trial", "SceneLoaded");
                SetupCreation();
                return;
            }

            if (_activeSceneName.Contains("Navigation"))
            {
                ExperimentDataLogger.Instance.SetSceneVariables(GetModelInt(), GetNavigationMethodInt());
                ExperimentDataLogger.Instance.RecordExperimentEvent("Navigation_Trial", "SceneLoaded");
                SetupNavigation();
            }
        }

        private static IEnumerator PlayNextScene(string scene)
        {
            SceneManager.LoadScene("Loading");

            yield return new WaitUntil(() => _activeSceneName == "Loading");
            yield return new WaitForSeconds(0.9f);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        private void EndExperiment()
        {
            ExperimentDataLogger.Instance.RecordExperimentEvent("ExperimentCompleted", "EndExperiment");
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion

        #region Emit Events
        private void OnSceneControlEvent(EventId id)
        {
            EventManager.Publish(id, this, GetSceneControlEventArgs());
        }

        private static SceneControlEventArgs GetSceneControlEventArgs() => new();
        #endregion
    }

    public class SceneControlEventArgs : EventArgs
    {
        public SceneControlEventArgs() { }

        public SceneControlEventArgs(PathStrategy strategy, Handedness handedness)
        {
            Handedness = handedness;
            Strategy   = strategy;
        }

        public SceneControlEventArgs(LocomotionDof locomotionDof, Handedness handedness)
        {
            Handedness    = handedness;
            LocomotionDof = locomotionDof;
        }

        public Handedness    Handedness    { get; }
        public LocomotionDof LocomotionDof { get; }
        public PathStrategy  Strategy      { get; }
    }
}