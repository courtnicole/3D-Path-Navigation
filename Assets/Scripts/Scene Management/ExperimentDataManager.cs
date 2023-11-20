namespace PathNav.ExperimentControl
{
    #region Imports
    using DataLogging;
    using Dreamteck.Splines;
    using Events;
    using Extensions;
    using Interaction;
    using PathPlanning;
    using Patterns.Factory;
    using SceneManagement;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
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
        public static ExperimentDataManager Instance { get; private set; }

        private SplineComputer _drawingModelASpline;
        private SplineComputer _drawingModelBSpline;
        private SplineComputer _drawingModelCSpline;
        private SplineComputer _drawingModelDSpline;

        private SplineComputer _interpolationModelASpline;
        private SplineComputer _interpolationModelBSpline;
        private SplineComputer _interpolationModelCSpline;
        private SplineComputer _interpolationModelDSpline;

        private SplineComputer _savedSpline;
        private Model _savedModel;

        private Dictionary<string, int> _savedSeqScores = new();
        private Dictionary<string, int> _savedDiscomfortScores = new();

        private int _userId;
        private UserInfo _userInfo;

        private TrialState _trialState;
        private Trial _currentTrial;
        private ConditionBlock _conditionBlock;

        private SceneDataFormat _sceneData;
        private string _logDirectory;
        private string _logFilePath;

        private int _trialIndex = 0;
        private int _modelIndex = 0;
        private int _sceneIndex = 0;

        private string _activeSceneName;

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

        public void RecordUserId(int id)
        {
            _userInfo = ScriptableObject.CreateInstance<UserInfo>();
            _userInfo.Initialize(id);
            _userId         = _userInfo.UserId;
            _conditionBlock = conditionBlocks[_userInfo.BlockId];
        }

        public async void BeginSession()
        {
            _logFilePath = _logDirectory + _userInfo.DataFile;

            _sceneData = new SceneDataFormat
            {
                ID       = _userInfo.UserId,
                BLOCK_ID = _userInfo.BlockId,
            };

            await CsvLogger.InitSceneDataLog(_logDirectory, _logFilePath);

            _sceneIndex = 0;
            _trialIndex = 0;
            _modelIndex = 0;

            _currentTrial = _conditionBlock.GetCurrentTrial(_trialIndex);
            _sceneIndex++;

            _trialState = TrialState.Tutorial;
            LoadNextScene();
        }

        #region Model
        private async void CreateModel(Model modelInfo)
        {
            bool result = await PlaceModelAsync(modelInfo);

            if (!result)
                Debug.LogError("Model Failed To Load! Unable to continue.");
        }

        private async Task<bool> PlaceModelAsync(Model modelInfo)
        {
            Task<GameObject> modelTask = InstantiateModelAsync(modelInfo.assetReference);
            GameObject       model     = await modelTask;

            if (model is null) return false;

            model.transform.position   = modelInfo.Translation;
            model.transform.localScale = modelInfo.Scale * Vector3.one;

            return true;
        }

        private async Task<GameObject> InstantiateModelAsync(AssetReferenceGameObject key)
        {
            Task<GameObject> modelTask = Factory.InstantiateObjectAsync(key, Utility.Parameterize(transform), CancellationToken.None);
            GameObject       model     = await modelTask;

            if (!modelTask.IsCompletedSuccessfully) return null;
            if (model == null) return null;

            model.transform.SetParent(null);
            model.AddComponent<ReleaseAddressable>();

            return model;
        }
        #endregion

        #region Data Recording
        internal void RecordSeqScore(int value)
        {
            _savedSeqScores.Add(GetSceneId(), value);
            _sceneData.SEQ_SCORE = value;
        }

        internal void RecordDiscomfortScore(int value)
        {
            _savedDiscomfortScores.Add(GetSceneId(), value);
            _sceneData.DISCOMFORT_SCORE = value;
        }

        internal void RecordActionData(int totalActions, int editActions, TimeSpan taskTimeTotal, TimeSpan taskTimeEdit, TimeSpan taskTimeCreate)
        {
            _sceneData.ACTIONS_TOTAL    = totalActions;
            _sceneData.ACTIONS_EDIT     = editActions;
            _sceneData.TASK_TIME_TOTAL  = taskTimeTotal.Seconds;
            _sceneData.TASK_TIME_EDIT   = taskTimeEdit.Seconds;
            _sceneData.TASK_TIME_CREATE = taskTimeCreate.Seconds;
        }

        public void RecordSceneData()
        {
            FileWriterInterface.RecordData("ID",         _userId.ToString());
            FileWriterInterface.RecordData("TRIAL_TYPE", _currentTrial.GetTrialTypeString());
            FileWriterInterface.RecordData("TRIAL_ID",   _trialIndex.ToString());
            FileWriterInterface.RecordData("SCENE_ID",   _currentTrial.GetTrialTypeString());
            FileWriterInterface.RecordData("METHOD",     _trialIndex.ToString());

            FileWriterInterface.RecordData("MODEL",            _currentTrial.GetTrialTypeString());
            FileWriterInterface.RecordData("ACTIONS_TOTAL",    _trialIndex.ToString());
            FileWriterInterface.RecordData("ACTIONS_EDIT",     _currentTrial.GetTrialTypeString());
            FileWriterInterface.RecordData("ACTIONS_CREATE",   _trialIndex.ToString());
            FileWriterInterface.RecordData("TASK_TIME_TOTAL",  _currentTrial.GetTrialTypeString());
            FileWriterInterface.RecordData("TASK_TIME_EDIT",   _trialIndex.ToString());
            FileWriterInterface.RecordData("TASK_TIME_CREATE", _currentTrial.GetTrialTypeString());
            FileWriterInterface.RecordData("SEQ_SCORE",        _trialIndex.ToString());
            FileWriterInterface.RecordData("DISCOMFORT_SCORE", _currentTrial.GetTrialTypeString());

            FileWriterInterface.WriteRecordedData();
        }

        private void SetupCsvLog()
        {
            bool success = FileWriterInterface.SetupLogFile(gameObject, _userInfo);
        }

        private void AddDataFieldsToCsvLog()
        {
            FileWriterInterface.AddLoggedItem<string>("ID");
            FileWriterInterface.AddLoggedItem<string>("TRIAL_TYPE");
            FileWriterInterface.AddLoggedItem<string>("TRIAL_ID");
            FileWriterInterface.AddLoggedItem<string>("SCENE_ID");
            FileWriterInterface.AddLoggedItem<string>("METHOD");
            FileWriterInterface.AddLoggedItem<string>("MODEL");
            FileWriterInterface.AddLoggedItem<string>("ACTIONS_TOTAL");
            FileWriterInterface.AddLoggedItem<string>("ACTIONS_EDIT");
            FileWriterInterface.AddLoggedItem<string>("ACTIONS_CREATE");
            FileWriterInterface.AddLoggedItem<string>("TASK_TIME_TOTAL");
            FileWriterInterface.AddLoggedItem<string>("TASK_TIME_EDIT");
            FileWriterInterface.AddLoggedItem<string>("TASK_TIME_CREATE");
            FileWriterInterface.AddLoggedItem<string>("SEQ_SCORE");
            FileWriterInterface.AddLoggedItem<string>("DISCOMFORT_SCORE");
        }

        private void InitializeCsvLog()
        {
            bool success = FileWriterInterface.InitializeLogging();
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

        #region Scene Control
        private void DebugSetup()
        {
            if (_userInfo == null)
            {
                RecordUserId(13);
                _currentTrial = _conditionBlock.GetCurrentTrial(_trialIndex);
            }
        }

        private void SetupTutorial()
        {
            if (_userInfo == null)
            {
                DebugSetup();
            }

            TutorialManager tutorialManager = FindObjectOfType<TutorialManager>();
            tutorialManager.Enable(_currentTrial);

            PathStrategy pathStrategy = _currentTrial.pathStrategy;
            EventManager.Publish(EventId.SetPathStrategy, this, new SceneControlEventArgs(pathStrategy));
        }

        private void SetupCreation()
        {
            if (_userInfo == null)
            {
                DebugSetup();
            }

            _sceneData.TRIAL_TYPE = _currentTrial.GetTrialTypeString();
            _sceneData.TRIAL_ID   = _trialIndex;
            _sceneData.SCENE_ID   = GetSceneId();
            _sceneData.MODEL      = _conditionBlock.GetCurrentModel(_modelIndex).name;
            _sceneData.METHOD     = _currentTrial.GetPathStrategyString();

            ActionMonitor actionMonitor = gameObject.AddComponent<ActionMonitor>();
            actionMonitor.Enable(_userId, _activeSceneName, _conditionBlock.conditionId);

            CreationManager creationManager = FindObjectOfType<CreationManager>();
            creationManager.Enable(_currentTrial, _sceneData);

            PathStrategy pathStrategy = _currentTrial.pathStrategy;
            EventManager.Publish(EventId.SetPathStrategy, this, new SceneControlEventArgs(pathStrategy));
        }

        private void SetupNavigation()
        {
            if (_userInfo == null)
            {
                DebugSetup();
            }

            _sceneData.TRIAL_TYPE = _currentTrial.GetTrialTypeString();
            _sceneData.TRIAL_ID   = _trialIndex;
            _sceneData.SCENE_ID   = GetSceneId();
            _sceneData.MODEL      = _conditionBlock.GetCurrentModel(_modelIndex).name;
            _sceneData.METHOD     = _currentTrial.GetLocomotionDofString();

            ActionMonitor actionMonitor = gameObject.AddComponent<ActionMonitor>();
            actionMonitor.Enable(_userId, _activeSceneName, _conditionBlock.conditionId);

            NavigationManager creationManager = FindObjectOfType<NavigationManager>();
            creationManager.Enable(_currentTrial, _sceneData);

            LocomotionDof locomotionDof = _currentTrial.locomotionDof;
            EventManager.Publish(EventId.SetLocomotionStrategy, this, new SceneControlEventArgs(locomotionDof));

            EventManager.Publish(EventId.EnableLocomotion, this, EventArgs.Empty);
        }

        internal void TutorialComplete()
        {
            _trialState = TrialState.Trial;
            _sceneIndex++;
            LoadNextScene();
        }

        internal async void CreationComplete()
        {
            await CsvLogger.LogSceneData(_sceneData);
        }

        private string GetSceneId()
        {
            string trial = _currentTrial.GetTrialTypeString();
            string model = _conditionBlock.GetCurrentModel(_modelIndex).Id;

            return $"{trial}_{model}";
        }

        private void LoadNextScene()
        {
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

        private IEnumerator PlayNextScene(string scene)
        {
            SceneManager.LoadScene("Loading");

            yield return new WaitUntil(() => _activeSceneName == "Loading");
            yield return new WaitForSeconds(0.8f);

            // Set the current Scene to be able to unload it later
            Scene          currentScene = SceneManager.GetActiveScene();
            AsyncOperation asyncLoad    = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            SceneManager.UnloadSceneAsync(currentScene);
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