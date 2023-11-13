namespace PathNav.ExperimentControl
{
    #region Imports
    using DataLogging;
    using Dreamteck.Splines;
    using Events;
    using Extensions;
    using PathPlanning;
    using Patterns.Factory;
    using SceneManagement;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
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

        private SplineComputer _savedSplineComputer;

        private Dictionary<string, SplineComputer> _savedSplineComputers = new();
        private Dictionary<string, int> _savedSeqScores = new();
        private Dictionary<string, int> _savedDiscomfortScores = new();

        private int _userId;
        private UserInfo _userInfo;

        private TrialState _trialState;
        private Trial _currentTrial;
        private ConditionBlock _conditionBlock;

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
            
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        public void RecordUserId(int id)
        {
            _userInfo = ScriptableObject.CreateInstance<UserInfo>();
            _userInfo.Initialize(id);
            _conditionBlock = conditionBlocks[_userInfo.BlockId];
        }

        public void BeginSession()
        {
            _userId     = _userInfo.UserId;
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
        public void SaveSplineComputer(SplineComputer splineToSave)
        {
            _savedSplineComputer = splineToSave;
            _savedSplineComputers.Add(GetSceneId(), _savedSplineComputer);
        }
        public SplineComputer GetSavedSplineComputer() => _savedSplineComputer;

        internal void RecordSeqScore(int value)
        {
            _savedSeqScores.Add(GetSceneId(), value);
        }
        internal void RecordDiscomfortScore(int value)
        {
            _savedDiscomfortScores.Add(GetSceneId(), value);
        }

        public void RecordSceneData()
        {
            FileWriterInterface.RecordData("ID",         _userId.ToString());
            FileWriterInterface.RecordData("TRIAL_TYPE", _currentTrial.GetTrialTypeString());
            FileWriterInterface.RecordData("TRIAL_ID",   _trialIndex.ToString());
            FileWriterInterface.RecordData("SCENE_ID",   _currentTrial.GetTrialTypeString());
            FileWriterInterface.RecordData("METHOD",     _trialIndex.ToString());
            
            FileWriterInterface.RecordData("MODEL", _currentTrial.GetTrialTypeString());
            FileWriterInterface.RecordData("ACTIONS_TOTAL",   _trialIndex.ToString());
            FileWriterInterface.RecordData("ACTIONS_EDIT",   _currentTrial.GetTrialTypeString());
            FileWriterInterface.RecordData("ACTIONS_CREATE",     _trialIndex.ToString());
            FileWriterInterface.RecordData("TASK_TIME_TOTAL",   _currentTrial.GetTrialTypeString());
            FileWriterInterface.RecordData("TASK_TIME_EDIT",     _trialIndex.ToString());
            FileWriterInterface.RecordData("TASK_TIME_CREATE",   _currentTrial.GetTrialTypeString());
            FileWriterInterface.RecordData("SEQ_SCORE",   _trialIndex.ToString());
            FileWriterInterface.RecordData("DISCOMFORT_SCORE",     _currentTrial.GetTrialTypeString());

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
            if(_userInfo == null)
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
            if(_userInfo == null)
            {
                DebugSetup();
            }
            
            ActionMonitor actionMonitor = gameObject.AddComponent<ActionMonitor>();
            actionMonitor.Enable(_userId, _activeSceneName, _conditionBlock.conditionId);

            CreationManager creationManager = FindObjectOfType<CreationManager>();
            creationManager.Enable(_currentTrial);

            PathStrategy pathStrategy = _currentTrial.pathStrategy;
            EventManager.Publish(EventId.SetPathStrategy, this, new SceneControlEventArgs(pathStrategy));
        }

        internal void TutorialComplete()
        {
            _trialState = TrialState.Trial;
            _sceneIndex++;
            LoadNextScene();
        }

        internal void CreationComplete() { }

        private string GetSceneId()
        {
            string trial = _currentTrial.trialType switch
                           {
                               TrialType.PathCreation   => "Creation",
                               TrialType.PathNavigation => "Navigation",
                               _                        => throw new ArgumentOutOfRangeException()
                           };
            string model = _conditionBlock.GetCurrentModel(_modelIndex).name;
            
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
            
            switch (_activeSceneName)
            {
                case "Loading":
                    return;
                case "Tutorial":
                    SetupTutorial();
                    return;
                case "Creation":
                    SetupCreation();
                    return;
            }
        }

        private IEnumerator PlayNextScene(string scene)
        {
            SceneManager.LoadScene("Loading");
            
            yield return new WaitUntil(()=>_activeSceneName == "Loading");
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
        public SceneControlEventArgs(PathStrategy strategy) => Strategy = strategy;
        public SceneControlEventArgs(Model        model) => Model = model;

        public PathStrategy Strategy { get; }
        public Model        Model    { get; }
    }
}