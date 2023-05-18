using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathNav.ExperimentControl
{
    public class LoadingScene : MonoBehaviour
    {
        
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1f);
            if (SceneDataManager.Instance != null)
            {
                SceneDataManager.Instance.LoadNextScene();
            }
        }
        
    }
}
