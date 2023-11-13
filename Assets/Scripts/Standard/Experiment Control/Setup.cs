namespace  PathNav.ExperimentControl
{
    using global::ExperimentControl;
    using TMPro;
    using UnityEngine;
    public class Setup : MonoBehaviour
    {
        [SerializeField] private TMP_Text userIdText;
        [SerializeField] private GameObject idButtons;
        [SerializeField] private GameObject beginButton;    
        [SerializeField] private string fileName = "test.json";
        private string _filePath = Application.streamingAssetsPath + "/";
        private string File => _filePath + fileName;
        
        public async void GetRecentId()
        {
            int id = await FileParser.GetNewUserIdAsync(File);
            ExperimentDataManager.Instance.RecordUserId(id);
            userIdText.text = "User ID: " + id;
            idButtons.SetActive(false);
            beginButton.SetActive(true);
        }
        
        public async void GetNewId()
        {
            int id = await FileParser.GetMostRecentUserIdAsync(File);
            ExperimentDataManager.Instance.RecordUserId(id);
            userIdText.text = "User ID: " + id;
            idButtons.SetActive(false);
            beginButton.SetActive(true);
        }

        public void Begin()
        {
            ExperimentDataManager.Instance.BeginSession();
        }
    }
}
