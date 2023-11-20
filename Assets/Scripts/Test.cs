
using UnityEngine;

namespace PathNav.ExperimentControl
{
    public class Test : MonoBehaviour
    {
        // Start is called before the first frame update
        async void Start()
        {
            string path = Application.dataPath              + "/Data/test_01.csv";
            await CsvLogger.InitSceneDataLog(Application.dataPath + "/Data/", path);
            SceneDataFormat data = new SceneDataFormat
            {
                ID     = 22,
                METHOD = "Tester",
            };

            await CsvLogger.LogSceneData(data);
            
            Debug.Log(path);
        }
    }
}
