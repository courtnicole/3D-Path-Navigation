namespace PathNav
{
    using Dreamteck.Splines;
    using PathPlanning;
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class SceneDataManager : MonoBehaviour
    {
        public static SceneDataManager Instance { get; private set; }

        public PersistentData persistentData;
        private SplineComputer _savedSplineComputer;

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
        
        public void SaveSplineComputer(SplineComputer splineToSave)
        {
            _savedSplineComputer = splineToSave;
            LoadNewScene("CoralReef");
        }
        
        private void LoadNewScene(string scene)
        {
            StartCoroutine(PlayNextScene(scene));
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
    }
    
    public class SceneControlEventArgs : EventArgs
    {
        public SceneControlEventArgs() {}
    }
}
