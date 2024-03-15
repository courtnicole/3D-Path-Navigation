namespace PathNav.ExperimentControl
{
    using Events;
    using Interaction;
    using PathPlanning;
    using SceneManagement;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.XR.OpenXR.Samples.ControllerSample;

    public class CreationManager : MonoBehaviour
    {
        [SerializeField] private List<AudioClipMap> audioMap;
        [SerializeField] private GameObject seq;
        [SerializeField] private GameObject instructions;
        [SerializeField] private Transform teleportLocation;
        [SerializeField] private Teleporter teleporter;
        [SerializeField] private Overlay overlay;
        [SerializeField] private PointerEvaluator pointerLeft;
        [SerializeField] private PointerEvaluator pointerRight;

        private bool _enableTeleportation;
        internal void Enable()
        {
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
        
        private void SeqComplete(object sender, SceneControlEventArgs args)
        {
            seq.SetActive(false);
            EndCreation();
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
            ActionAssetEnabler actionController = FindObjectOfType<ActionAssetEnabler>();
            actionController.EnableUiInput();
            instructions.SetActive(false);
            seq.SetActive(true);
            pointerLeft.Enable();
            pointerRight.Enable();
        }

        private async void EndCreation()
        {
            await Task.Delay(100);
            
            overlay.FadeToBlack();
            
            await Task.Delay(1500);
            
            ExperimentDataManager.Instance.CreationComplete();
        }

        private async void StartCreation()
        {
            if (_enableTeleportation)
            {
                teleporter.Teleport(teleportLocation);
            }
            
            await Task.Delay(500);
            
            overlay.FadeToClear();
        }
        
        public void StopImmediately()
        {
            overlay.FadeToBlackImmediate();
            
            EventManager.Publish(EventId.PathCreationComplete, this, new ControllerEvaluatorEventArgs(null));
            UnsubscribeToEvents();
            
            ExperimentDataManager.Instance.CreationComplete();
        }
    }
}
