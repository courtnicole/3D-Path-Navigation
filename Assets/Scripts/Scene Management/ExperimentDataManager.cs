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
    using UnityEngine;
    using UnityEngine.SceneManagement;
    #endregion

    public class ExperimentDataManager : MonoBehaviour
    {
        private enum TrialState
        {
            Tutorial,
            Trial,
        }

        [SerializeField] private ConditionBlock[] conditionBlocks;
        private static UserInfo _userInfo;
        public static ExperimentDataManager Instance { get; private set; }

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

        private static SceneDataFormat _sceneData;
        private static string _logDirectory;
        private static string _logFilePath;
        private static string _logFilePathActions;
        private static string _logFilePathNavigation;
        private static string _logDirectorySpline;
        private static string _logFilePathSpline;

        //trialCount
        private static int _currentTrialCount;

        private static int _modelIndex;

        //trialIndex
        private static int _currentTrialStageIndex;

        private static string _activeSceneName;
        private const int _totalTrialCount = 1;
        private const int _totalTrialStages = 4;

        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            _logDirectory                   =  Application.dataPath + "/Data/";
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        public void RecordUserId(int id, float height)
        {
            _userInfo       = new UserInfo(id, height);
            _userId         = _userInfo.UserId;
            _conditionBlock = conditionBlocks[_userInfo.BlockId];

            _logDirectorySpline = $"{_logDirectory}{_userId}_splines/";
        }

        public void RecordHandedness(bool useLeftHand)
        {
            _handedness = useLeftHand ? Handedness.Left : Handedness.Right;
        }

        public async void BeginSession()
        {
            _logFilePath           = _logDirectory + _userInfo.DataFile;
            _logFilePathActions    = _logDirectory + _userInfo.ActionFile;
            _logFilePathNavigation = _logDirectory + _userInfo.NavigationFile;

            _sceneData = new SceneDataFormat
            {
                ID       = _userInfo.UserId,
                BLOCK_ID = _userInfo.BlockId,
            };

            await CsvLogger.InitSceneDataLog(_logDirectory, _logFilePath);

            _currentTrialStageIndex = 0;
            _currentTrialCount      = 0;
            _modelIndex             = 0;

            _currentTrial = _conditionBlock.GetCurrentTrial(_currentTrialStageIndex);

            _trialState = TrialState.Tutorial;

            LoadNextScene();
        }

        #region Data Recording
        public void RecordSeqScore(int value)
        {
            _sceneData.SEQ_SCORE = value;
        }

        public void RecordDiscomfortScore(int value)
        {
            _sceneData.DISCOMFORT_SCORE = value;
        }

        private void InitSceneData()
        {
            _sceneData.TRIAL_TYPE = _currentTrial.GetTrialTypeString();
            _sceneData.TRIAL_ID   = _currentTrialCount;
            _sceneData.SCENE_ID   = GetSceneId();

            _sceneData.METHOD = _currentTrial.trialType == TrialType.PathCreation
                ? _currentTrial.GetPathStrategyString()
                : _currentTrial.GetLocomotionDofString();
            _sceneData.MODEL            = _conditionBlock.GetCurrentModel(_currentTrialStageIndex, _modelIndex).Id;
            _sceneData.ACTIONS_TOTAL    = -99;
            _sceneData.ACTIONS_EDIT     = -99;
            _sceneData.TASK_TIME_TOTAL  = -99;
            _sceneData.TASK_TIME_EDIT   = -99;
            _sceneData.TASK_TIME_CREATE = -99;
            _sceneData.SEQ_SCORE        = -99;
            _sceneData.DISCOMFORT_SCORE = -99;
        }

        public void RecordActionData(int totalActions, int editActions, TimeSpan taskTimeTotal, TimeSpan taskTimeEdit, TimeSpan taskTimeCreate)
        {
            _sceneData.ACTIONS_TOTAL    = totalActions;
            _sceneData.ACTIONS_EDIT     = editActions;
            _sceneData.TASK_TIME_TOTAL  = taskTimeTotal.Seconds;
            _sceneData.TASK_TIME_EDIT   = taskTimeEdit.Seconds;
            _sceneData.TASK_TIME_CREATE = taskTimeCreate.Seconds;
        }

        public void RecordNavigationData(TimeSpan taskTimeTotal)
        {
            _sceneData.TASK_TIME_TOTAL = taskTimeTotal.Seconds;
        }
        #endregion

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

        public SplinePoint[] GetSavedSpline()
        {
            _savedModel = _conditionBlock.GetCurrentModel(_currentTrialStageIndex, _modelIndex);

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
                _savedSpline = _savedModel.Id switch
                               {
                                   "Model_A" => _currentTrialCount % 2 == 0 ? _drawingModelASpline : _interpolationModelASpline,
                                   "Model_B" => _currentTrialCount % 2 == 0 ? _drawingModelBSpline : _interpolationModelBSpline,
                                   "Model_C" => _currentTrialCount % 2 == 0 ? _drawingModelCSpline : _interpolationModelCSpline,
                                   "Model_D" => _currentTrialCount % 2 == 0 ? _drawingModelDSpline : _interpolationModelDSpline,
                                   _         => throw new ArgumentOutOfRangeException(),
                               };
            }

            return _savedSpline;
        }

        public Model GetSplineModel() => _savedModel;
        #endregion

        #region Public Getters
        public int GetId()    => _userId;
        public int GetBlock() => _userInfo.BlockId;

        public float         GetHeight()                 => _userInfo.Height;
        public PathStrategy  GetCreationMethod()         => _currentTrial.pathStrategy;
        public string        GetCreationMethodString()   => _currentTrial.GetPathStrategyString();
        public LocomotionDof GetNavigationMethod()       => _currentTrial.locomotionDof;
        public string        GetNavigationMethodString() => _currentTrial.GetLocomotionDofString();
        public string        GetModel()                  => _conditionBlock.GetCurrentModel(_currentTrialStageIndex, _modelIndex).Id;
        public string        GetLogDirectory()           => _logDirectory;
        public string        GetActionLogFilePath()      => _logFilePathActions;

        public bool UseTargetPoints1()
        {
            if (_userId % 2 == 0)
            {
                return _currentTrialStageIndex == 0;
            }

            return _currentTrialStageIndex != 0;
        }

        public Handedness GetHandedness() => _handedness;

        public bool CreationTutorialActive()
        {
            if (_currentTrial.trialType != TrialType.PathCreation) return false;

            return _trialState == TrialState.Tutorial;
        }

        public string GetNavigationLogFilePath() => _logFilePathNavigation;
        #endregion

        #region Scene Control
        private void SetupTutorialCreation()
        {
            TutorialCreation tutorialCreation = FindObjectOfType<TutorialCreation>();
            tutorialCreation.Enable();

            EventManager.Publish(EventId.SetPathStrategy, this, new SceneControlEventArgs(_currentTrial.pathStrategy, _handedness));
        }

        private void SetupTutorialNavigation()
        {
            TutorialNavigation tutorialNavigation = FindObjectOfType<TutorialNavigation>();
            tutorialNavigation.Enable();

            EventManager.Publish(EventId.SetLocomotionStrategy, this, new SceneControlEventArgs(_currentTrial.locomotionDof, _handedness));
        }

        private void SetupCreation()
        {
            InitSceneData();

            GameObject holder = new();

            CreationActionMonitor creationActionMonitor = holder.AddComponent<CreationActionMonitor>();
            creationActionMonitor.Enable();

            CreationManager creationManager = FindObjectOfType<CreationManager>();
            creationManager.Enable();

            EventManager.Publish(EventId.SetPathStrategy, this, new SceneControlEventArgs(_currentTrial.pathStrategy, _handedness));
        }

        private void SetupNavigation()
        {
            InitSceneData();

            NavigationManager navigationManager = FindObjectOfType<NavigationManager>();
            navigationManager.Enable();

            EventManager.Publish(EventId.SetLocomotionStrategy, this, new SceneControlEventArgs(_currentTrial.locomotionDof, _handedness));

            EventManager.Publish(EventId.EnableLocomotion, this, EventArgs.Empty);
        }

        internal void TutorialComplete()
        {
            _trialState        = TrialState.Trial;
            _currentTrialCount = 0;
            _modelIndex        = 0;
            LoadNextScene();
        }

        internal async void CreationComplete()
        {
            await CsvLogger.LogSceneData(_sceneData);
            IncrementTrial();
        }

        internal async void NavigationComplete()
        {
            await CsvLogger.LogSceneData(_sceneData);
            IncrementTrial();
        }

        internal async void EndExperimentImmediately()
        {
            Task<bool> writeData = CsvLogger.LogSceneData(_sceneData);
            bool       result    = await writeData;

            if (!result) return;

            Task<bool> endLog = CsvLogger.FinalizeDataLog();
            result = await endLog;

            if (!result) return;

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

        private void OnActiveSceneChanged(Scene replaced, Scene next)
        {
            _activeSceneName = next.name;

            if (_activeSceneName.Contains("Loading")) return;

            if (_activeSceneName.Contains("Creation_Tutorial"))
            {
                SetupTutorialCreation();
                return;
            }

            if (_activeSceneName.Contains("Navigation_Tutorial"))
            {
                SetupTutorialNavigation();
                return;
            }

            if (_activeSceneName.Contains("Creation"))
            {
                SetupCreation();
                return;
            }

            if (_activeSceneName.Contains("Navigation"))
            {
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

        private async void EndExperiment()
        {
            Task<bool> writeData = CsvLogger.FinalizeDataLog();
            bool       result    = await writeData;

            if (!result) return;
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