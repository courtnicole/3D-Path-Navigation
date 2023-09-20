namespace PathNav
{
    using Dreamteck.Splines;
    using PathPlanning;
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
            SceneManager.LoadScene("Loading");
        }

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
    }
    

}
