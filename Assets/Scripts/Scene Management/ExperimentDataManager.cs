
namespace PathNav.ExperimentControl
{
    using UnityEngine;
    public class ExperimentDataManager : MonoBehaviour
    {
        [SerializeField] private ExperimentManager experimentManager;
        public static ExperimentDataManager Instance { get; private set; }
        
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

        public void GetCurrentModel() => experimentManager.GetModel();
    }
}
