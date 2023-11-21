namespace PathNav.ExperimentControl
{
    #region Imports
    using DataLogging;
    using Dreamteck.Splines;
    using Events;
    using Interaction;
    using PathPlanning;
    using SceneManagement;
    using System;
    using System.Collections;
    using System.Collections.Generic;
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

        private static SplineComputer _drawingModelASpline;
        private static SplineComputer _drawingModelBSpline;
        private static SplineComputer _drawingModelCSpline;
        private static SplineComputer _drawingModelDSpline;

        private static SplineComputer _interpolationModelASpline;
        private static SplineComputer _interpolationModelBSpline;
        private static SplineComputer _interpolationModelCSpline;
        private static SplineComputer _interpolationModelDSpline;

        private static SplineComputer _savedSpline;
        private static Model _savedModel;

        private static Dictionary<string, int> _savedSeqScores = new();
        private static Dictionary<string, int> _savedDiscomfortScores = new();

        private static int _userId;
        private static TrialState _trialState;
        private static Trial _currentTrial;
        private static ConditionBlock _conditionBlock;

        private static SceneDataFormat _sceneData;
        private static string _logDirectory;
        private static string _logFilePath;
        private static string _logFilePathActions;

        private static int _trialIndex = 0;
        private static int _modelIndex = 0;
        private static int _sceneIndex = 0;

        private static string _activeSceneName;

        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
                Destroy(gameObject);

            _logDirectory = Application.dataPath + "/Data/";

            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }
        
        protected void OnDestroy(){}

        public void RecordUserId(int id)
        {
            _userInfo       = new UserInfo(id);
            _userId         = _userInfo.UserId;
            _conditionBlock = conditionBlocks[_userInfo.BlockId];
        }

        public async void BeginSession()
        {
            _logFilePath        = _logDirectory + _userInfo.DataFile;
            _logFilePathActions = _logDirectory + _userInfo.ActionFile;

            _sceneData = new SceneDataFormat
            {
                ID       = _userInfo.UserId,
                BLOCK_ID = _userInfo.BlockId,
            };

            await CsvLogger.InitSceneDataLog(_logDirectory, _logFilePath);

            _sceneIndex = 0;

            _currentTrial = _conditionBlock.GetCurrentTrial(_trialIndex);

            _trialState = TrialState.Tutorial;
            
            LoadNextScene();
        }

        #region Data Recording
        public void RecordSeqScore(int value)
        {
            _savedSeqScores.Add(GetSceneId(), value);
            _sceneData.SEQ_SCORE = value;
        }

        public void RecordDiscomfortScore(int value)
        {
            _savedDiscomfortScores.Add(GetSceneId(), value);
            _sceneData.DISCOMFORT_SCORE = value;
        }

        public void RecordActionData(int totalActions, int editActions, TimeSpan taskTimeTotal, TimeSpan taskTimeEdit, TimeSpan taskTimeCreate)
        {
            _sceneData.ACTIONS_TOTAL    = totalActions;
            _sceneData.ACTIONS_EDIT     = editActions;
            _sceneData.TASK_TIME_TOTAL  = taskTimeTotal.Seconds;
            _sceneData.TASK_TIME_EDIT   = taskTimeEdit.Seconds;
            _sceneData.TASK_TIME_CREATE = taskTimeCreate.Seconds;
        }
        
        #endregion

        #region Spline
        public void SaveSplineComputer(SplineComputer splineToSave)
        {
            switch (_conditionBlock.GetCurrentModel(_modelIndex).Id)
            {
                case "Model_A":
                    switch (_currentTrial.pathStrategy)
                    {
                        case PathStrategy.Bulldozer:
                            _drawingModelASpline = splineToSave;
                            break;
                        case PathStrategy.Spatula:
                            _interpolationModelASpline = splineToSave;
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
                            _drawingModelBSpline = splineToSave;
                            break;
                        case PathStrategy.Spatula:
                            _interpolationModelBSpline = splineToSave;
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
                            _drawingModelCSpline = splineToSave;
                            break;
                        case PathStrategy.Spatula:
                            _interpolationModelCSpline = splineToSave;
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
                            _drawingModelDSpline = splineToSave;
                            break;
                        case PathStrategy.Spatula:
                            _interpolationModelDSpline = splineToSave;
                            break;
                        case PathStrategy.None:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
            }
        }

        public SplineComputer GetSavedSplineComputer()
        {
            _savedModel = _conditionBlock.GetCurrentModel(_modelIndex);

            _savedSpline = _conditionBlock.GetCurrentModel(_modelIndex).Id switch
                           {
                               "Model_A" => _trialIndex % 2 == 0 ? _drawingModelASpline : _interpolationModelASpline,
                               "Model_B" => _trialIndex % 2 == 0 ? _drawingModelBSpline : _interpolationModelBSpline,
                               "Model_C" => _trialIndex % 2 == 0 ? _drawingModelCSpline : _interpolationModelCSpline,
                               "Model_D" => _trialIndex % 2 == 0 ? _drawingModelDSpline : _interpolationModelDSpline,
                               _         => throw new ArgumentOutOfRangeException(),
                           };

            return _savedSpline;
        }

        public Model GetSplineModel() => _savedModel;
        #endregion

        #region Public Getters
        public int           GetId()                   => _userId;
        public int           GetBlock()                => _userInfo.BlockId;
        public PathStrategy  GetCreationMethod()       => _currentTrial.pathStrategy;
        public string        GetCreationMethodString() => _currentTrial.GetPathStrategyString();
        public LocomotionDof GetNavigationMethod()     => _currentTrial.locomotionDof;
        public string        GetModel()                => _conditionBlock.GetCurrentModel(_modelIndex).Id;
        public string        GetLogDirectory()         => _logDirectory;
        public string        GetActionLogFilePath()    => _logFilePathActions;
        public string        GetLogFilePath()          => _logFilePath;
        #endregion

        #region Scene Control
        private void SetupTutorial()
        {
            TutorialManager tutorialManager = FindObjectOfType<TutorialManager>();
            tutorialManager.Enable();

            PathStrategy pathStrategy = _currentTrial.pathStrategy;
            EventManager.Publish(EventId.SetPathStrategy, this, new SceneControlEventArgs(pathStrategy));
        }

        private void SetupCreation()
        {
            _sceneData.TRIAL_TYPE = _currentTrial.GetTrialTypeString();
            _sceneData.TRIAL_ID   = _trialIndex;
            _sceneData.SCENE_ID   = GetSceneId();
            _sceneData.MODEL      = _conditionBlock.GetCurrentModel(_modelIndex).Id;
            _sceneData.METHOD     = _currentTrial.GetPathStrategyString();

            GameObject holder = new ();
            
            CreationActionMonitor creationActionMonitor = holder.AddComponent<CreationActionMonitor>();
            creationActionMonitor.Enable();

            CreationManager creationManager = FindObjectOfType<CreationManager>();
            creationManager.Enable();

            PathStrategy pathStrategy = _currentTrial.pathStrategy;
            EventManager.Publish(EventId.SetPathStrategy, this, new SceneControlEventArgs(pathStrategy));
        }

        private void SetupNavigation()
        {
            _sceneData.TRIAL_TYPE = _currentTrial.GetTrialTypeString();
            _sceneData.TRIAL_ID   = _trialIndex;
            _sceneData.SCENE_ID   = GetSceneId();
            _sceneData.MODEL      = _conditionBlock.GetCurrentModel(_modelIndex).name;
            _sceneData.METHOD     = _currentTrial.GetLocomotionDofString();

            NavigationManager navigationManager = FindObjectOfType<NavigationManager>();
            navigationManager.Enable();

            LocomotionDof locomotionDof = _currentTrial.locomotionDof;
            EventManager.Publish(EventId.SetLocomotionStrategy, this, new SceneControlEventArgs(locomotionDof));

            EventManager.Publish(EventId.EnableLocomotion, this, EventArgs.Empty);
        }

        internal void TutorialComplete()
        {
            _sceneIndex++;
            _trialState = TrialState.Trial;
            _trialIndex = 0;
            _modelIndex = 0;
            LoadNextScene();
        }

        internal async void CreationComplete()
        {
            await CsvLogger.LogSceneData(_sceneData);
            _trialIndex++;
            _modelIndex++;
            LoadNextScene();
        }

        internal async void NavigationComplete()
        {
            await CsvLogger.LogSceneData(_sceneData);
            _trialIndex++;
            _modelIndex++;
            LoadNextScene();
        }

        private string GetSceneId()
        {
            string trial = _currentTrial.GetTrialTypeString();
            string model = _conditionBlock.GetCurrentModel(_modelIndex).Id;

            return $"{trial}_{model}";
        }

        private void LoadNextScene()
        {
            //Check if trialCount > Max trials
            //If so, increment condition????????
            string sceneId = _currentTrial.trialType switch
                             {
                                 TrialType.PathCreation   => _trialState == TrialState.Tutorial ? "Tutorial" : GetSceneId(),
                                 TrialType.PathNavigation => _trialState == TrialState.Tutorial ? "TutorialNavigation" : GetSceneId(),
                                 _                        => throw new ArgumentOutOfRangeException()
                             };

            StartCoroutine(PlayNextScene(sceneId));
        }

        private void OnActiveSceneChanged(Scene replaced, Scene next)
        {
            if (_activeSceneName == next.name) return;
            _activeSceneName = next.name;

            if (_activeSceneName.Contains("Loading")) return;
            
            if (_activeSceneName.Contains("Tutorial"))
            {
                SetupTutorial();
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
                return;
            }
        }

        private static IEnumerator PlayNextScene(string scene)
        {
            SceneManager.LoadScene("Loading");

            yield return new WaitUntil(() => _activeSceneName == "Loading");
            yield return new WaitForSeconds(0.8f);

            // Set the current Scene to be able to unload it later
            //Scene          currentScene = SceneManager.GetActiveScene();
            AsyncOperation asyncLoad    = SceneManager.LoadSceneAsync(scene);
           
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
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
        public SceneControlEventArgs(PathStrategy  strategy) => Strategy = strategy;
        public SceneControlEventArgs(LocomotionDof locomotionDof) => LocomotionDof = locomotionDof;

        public LocomotionDof LocomotionDof { get; }
        public PathStrategy  Strategy      { get; }
    }
}