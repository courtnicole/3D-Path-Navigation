using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathNav.ExperimentControl
{
    using System;
    using UnityEngine.AddressableAssets;
    using UnityEngine.SceneManagement;
    public enum Steps
    {
        Tutorial = 1,
        Trial = 2,
        Test = 3,
        Surveys = 4,
        Complete = 5,
    }

    [CreateAssetMenu(fileName = "ExperimentManager", menuName = "Scriptables/Standard/ExperimentManager", order = 0)]
    public class ExperimentManager : ScriptableObject
    {
        public List<Condition> conditions;
        public List<Model> models;
        public string loadingScene;
        public int currentModelIndex = 0;
        public int currentConditionIndex = 0;
        public Steps nextStep = Steps.Tutorial;

        public AssetReferenceGameObject GetModel() => models[currentModelIndex].assetReference;

        public void LoadNextStep()
        {
            
            switch (nextStep)
            {
                case Steps.Tutorial:
                    LoadTutorial();
                    nextStep = Steps.Trial;
                    break;
                case Steps.Trial:
                    LoadTrial();
                    nextStep = Steps.Test;
                    break;
                case Steps.Test:
                    LoadTest();
                    nextStep = Steps.Surveys;
                    break;
                case Steps.Surveys:
                    LoadSurvey();
                    if (currentConditionIndex == conditions.Count - 1)
                    {
                        nextStep = Steps.Complete;
                    }
                    else
                    {
                        currentConditionIndex++;
                        nextStep = Steps.Tutorial;
                    }
                    break;
                case Steps.Complete:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public void LoadTutorial()
        {
            if (conditions[currentConditionIndex].showTutorial)
            {
                LoadScene(conditions[currentConditionIndex].tutorial.sceneName);
            }
            else
            {
                LoadNextStep();
            }
        }
        public void LoadTrial()
        {
            LoadScene(conditions[currentConditionIndex].trial.sceneName);
        }
        public void LoadTest()
        {
            LoadScene(conditions[currentConditionIndex].test.sceneName);
        }
        public void LoadSurvey()
        {
            LoadScene(conditions[currentConditionIndex].survey.sceneName);
        }

        public void LoadLoadingScene()
        {
            SceneManager.LoadScene(loadingScene);
            LoadNextStep();
        }
        public void LoadScene(string scene)
        {
            SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        }
    }
}
