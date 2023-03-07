namespace PathNav
{
    using Dreamteck.Splines;
    using UnityEngine;

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
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }

        public void SaveSplineComputer(SplineComputer splineToSave)
        {
            _savedSplineComputer = splineToSave;
        }
    }
}
