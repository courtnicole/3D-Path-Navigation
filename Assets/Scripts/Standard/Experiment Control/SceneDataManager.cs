namespace PathNav
{
    using Dreamteck.Splines;
    using ExperimentControl;
    using PathPlanning;
    using UnityEngine;

    public class SceneDataManager : MonoBehaviour
    {
        public static SceneDataManager Instance { get; private set; }

        public PersistentData persistentData;
        private SplineComputer _savedSplineComputer;
        public SceneControl sceneControl;

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
        
        public async void SaveSplineComputer(SplineComputer splineToSave, GameObject model)
        {
            _savedSplineComputer = splineToSave;
            sceneControl.LoadNewScene("CoralReef");
        }
        
        public SplineComputer GetSavedSplineComputer() => _savedSplineComputer;
    }
    
}
