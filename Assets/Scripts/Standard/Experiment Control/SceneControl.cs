namespace ExperimentControl
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class SceneControl : MonoBehaviour
    {
        public void LoadNewScene(string scene)
        {
            StartCoroutine(PlayNextScene(scene));
        }
        private static IEnumerator PlayNextScene(string scene)
        {
            // Set the current Scene to be able to unload it later
            Scene          currentScene = SceneManager.GetActiveScene();
            AsyncOperation asyncLoad    = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        
            // Wait until the last operation fully loads to return anything
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            SceneManager.UnloadSceneAsync(currentScene);
        }
    }
}