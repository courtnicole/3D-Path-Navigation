using System.Collections;
using System.Collections.Generic;
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
            SceneDataFormat data = new SceneDataFormat();
            data.ID = 22;
            data.METHOD = "Tester";
            
            await CsvLogger.LogSceneData(data);
            
            Debug.Log(path);
            
            await CsvLogger.EndLogging();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
