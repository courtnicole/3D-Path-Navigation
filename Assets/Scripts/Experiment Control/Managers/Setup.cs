namespace  PathNav.ExperimentControl
{
    using global::ExperimentControl;
    using TMPro;
    using UnityEngine;
    using UnityEngine.InputSystem.XR;
    using UnityEngine.XR.OpenXR.Samples.ControllerSample;

    public class Setup : MonoBehaviour
    {
        [SerializeField] private TMP_Text userIdText;
        [SerializeField] private GameObject idButtons;
        [SerializeField] private GameObject beginButton;    
        [SerializeField] private GameObject handednessButtons;   
        [SerializeField] private string fileName = "user_id_list.json";
        [SerializeField] private Transform headPose;
        private string _filePath = Application.streamingAssetsPath + "/";
        private string File => _filePath + fileName;
        
        public async void GetRecentId()
        {
            int id = await FileParser.GetMostRecentUserIdAsync(File);
            ExperimentDataManager.Instance.RecordUserId(id, headPose.position.y);
            userIdText.text = "User ID: " + id;
            ToggleHandedness();
        }
        
        public async void GetNewId()
        {
#if UNITY_EDITOR
            int id = await FileParser.GetMostRecentUserIdAsync(File);
#else
            int id = await FileParser.GetNewUserIdAsync(File);
#endif            
            ExperimentDataManager.Instance.RecordUserId(id, headPose.position.y);
            userIdText.text = "User ID: " + id;
            ToggleHandedness();
        }

        private void ToggleHandedness()
        {
            userIdText.gameObject.SetActive(true);
            idButtons.SetActive(false);
            handednessButtons.SetActive(true);
        }
        
        public void SetLeftHand(bool useLeft)
        {
            ExperimentDataManager.Instance.RecordHandedness(useLeft);
            handednessButtons.SetActive(false);
            beginButton.SetActive(true);
        }

        public void Begin()
        {
            ExperimentDataManager.Instance.BeginSession();
        }
    }
}
