namespace PathNav.ExperimentControl
{
    using Interaction;
    using PathPlanning;
    using SceneManagement;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private List<AudioClipMap> audioMap;
        [SerializeField] private GameObject interpolationExample;
        [SerializeField] private GameObject drawingExample;
        [SerializeField] private Transform teleportLocation;
        [SerializeField] private Teleporter teleporter;

        private bool _enableTeleportation;
        
        private Trial _trial;

        internal void Enable(Trial trial)
        {
            _trial = trial;
            _enableTeleportation = CheckTeleportation();
            StartTutorial();
        }

        private bool CheckTeleportation()
        {
            if (teleportLocation is not null)
            {
                if (teleporter is null)
                {
                    teleporter = FindObjectOfType<Teleporter>();

                    if (teleporter is null)
                    {
                        Debug.LogError("Teleporter not found in scene!");
                    }
                }
            }

            return (teleporter is not null) && (teleportLocation is not null);
        }

        private void ShowExample(bool show)
        {
            switch (_trial.pathStrategy)
            {
                case PathStrategy.Bulldozer:
                    drawingExample?.SetActive(show);
                    break;
                case PathStrategy.Spatula:
                    interpolationExample?.SetActive(show);
                    break;
                case PathStrategy.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void TutorialComplete()
        {
            ExperimentDataManager.Instance.TutorialComplete();
        }

        private async void StartTutorial()
        {
            await Task.Delay(1000);
            
            ShowExample(true);
            
            await Task.Delay(1000);
            
            if (_enableTeleportation)
            {
                teleporter.Teleport(teleportLocation);
            }
        }
    }
}