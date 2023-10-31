namespace PathNav.ExperimentControl
{
    using Interaction;
    using SceneManagement;
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

        private void OnEnable()
        {
            _enableTeleportation = CheckTeleportation();
        }

        private void Start()
        {
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

        private void TutorialComplete()
        {
            
        }

        private async void StartTutorial()
        {
            if (_enableTeleportation)
            {
                teleporter.Teleport(teleportLocation);
            }
            
            //wait for 3 seconds before continuing
            await Task.Delay(3000);
        }
    }
}