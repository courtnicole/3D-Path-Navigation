namespace PathNav.ExperimentControl
{
    using Dreamteck.Splines;
    using Events;
    using Extensions;
    using Interaction;
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

    public class ExperimentDataManager : MonoBehaviour
    {
        [SerializeField] private ExperimentManager experimentManager;
        public static ExperimentDataManager Instance { get; private set; }
        
        private SplineComputer _savedSplineComputer;
        
        private Dictionary<string, SplineComputer> _savedSplineComputers = new();
        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
                Destroy(gameObject);
        }

        protected void SetupPathCreationTrial()
        {
            Condition condition = experimentManager.GetCondition();

            CreateModel(condition.model);

            PathStrategy pathStrategy = condition.pathStrategy;
            EventManager.Publish(EventId.SetPathStrategy, this, new SceneControlEventArgs(pathStrategy));

            Counting counting = gameObject.AddComponent<Counting>();
            counting.Enable();

            Timing timing = gameObject.AddComponent<Timing>();
            timing.Enable();
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
        
        #region Spline
        public void SaveSplineComputer(SplineComputer splineToSave)
        {
            _savedSplineComputer = splineToSave;
            
            SceneManager.LoadScene("Loading");
        }
        #endregion
        
        #region Scene Control
        public void LoadNextScene()
        {
            StartCoroutine(PlayNextScene("Castle"));
        }
        
        private static IEnumerator PlayNextScene(string scene)
        {
            // Set the current Scene to be able to unload it later
            Scene          currentScene = SceneManager.GetActiveScene();
            AsyncOperation asyncLoad    = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            SceneManager.UnloadSceneAsync(currentScene);
        }
        
        public SplineComputer GetSavedSplineComputer() => _savedSplineComputer;
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