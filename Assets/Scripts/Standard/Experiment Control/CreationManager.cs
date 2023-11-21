namespace PathNav.ExperimentControl
{
    using Events;
    using Interaction;
    using PathPlanning;
    using SceneManagement;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class CreationManager : MonoBehaviour
    {
        [SerializeField] private List<AudioClipMap> audioMap;
        [SerializeField] private GameObject seq;
        [SerializeField] private Transform teleportLocation;
        [SerializeField] private Teleporter teleporter;

        private bool _enableTeleportation;
        
        public InputActionReference debugEndTutorial;

        internal void Enable()
        {
            //debugEndTutorial.action.Enable();
            //debugEndTutorial.action.started += TutorialComplete;
            
            _enableTeleportation            =  CheckTeleportation();
            SubscribeToEvents();
            StartCreation();
        }
        
        private void OnDisable()
        {
            UnsubscribeToEvents();
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
        
        private void TutorialComplete(InputAction.CallbackContext callbackContext)
        {
            //ExperimentDataManager.Instance.TutorialComplete();
            seq.SetActive(true);
        }
        
        private void SeqComplete(object sender, SceneControlEventArgs args)
        {
            seq.SetActive(false);
            ExperimentDataManager.Instance.CreationComplete();
        }
        
        private void SubscribeToEvents()
        {
            EventManager.Subscribe<SegmentEventArgs>(EventId.SegmentComplete, SplineComplete);
            EventManager.Subscribe<SceneControlEventArgs>(EventId.SeqComplete, SeqComplete);
        }
        
        private void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<SegmentEventArgs>(EventId.SegmentComplete, SplineComplete);
            EventManager.Unsubscribe<SceneControlEventArgs>(EventId.SeqComplete, SeqComplete);
        }

        private void SplineComplete(object sender, SegmentEventArgs args)
        {
            seq.SetActive(true);
        }

        private async void StartCreation()
        {
            if (_enableTeleportation)
            {
                teleporter.Teleport(teleportLocation);
            }
            
            await Task.Delay(3000);
            
        }
    }
}
