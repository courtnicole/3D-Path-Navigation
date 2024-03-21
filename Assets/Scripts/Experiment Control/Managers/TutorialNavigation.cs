namespace PathNav.ExperimentControl
{
    using Dreamteck.Splines;
    using Events;
    using HTC.UnityPlugin.Vive;
    using Interaction;
    using SceneManagement;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Animations;
    using UnityEngine.XR.OpenXR.Samples.ControllerSample;

    public class TutorialNavigation : MonoBehaviour
    {
        private enum TutorialStage
        {
            Start,
            Finish,
        }

        [SerializeField] private Overlay overlay;
        [SerializeField] private AudioManager audioManager;
        
        [SerializeField] private PointerEvaluator pointerLeft;
        [SerializeField] private PointerEvaluator pointerRight;
        
        [SerializeField] private DefaultTooltipRenderer tooltipRendererLeft;
        [SerializeField] private DefaultTooltipRenderer tooltipRendererRight;
        [SerializeField] private DefaultTooltipRenderDataAsset fourDofTooltip;
        [SerializeField] private DefaultTooltipRenderDataAsset sixDofTooltip;

        [SerializeField] private SplineFollower follower;
        [SerializeField] private Transform teleportLocation;
        [SerializeField] private Teleporter teleporter;
        [SerializeField] private ParentConstraint parentConstraint;
        
        [SerializeField] private NavigationEndPoint endPoint;
        [SerializeField] private Transform footVisualMarker;
        
        [SerializeField] private GameObject discomfortScore;

        private List<string> _audioMap;
        private List<string> _fourDofAudio = new() { "navigation_4dof", "finish_navigation", };
        private List<string> _sixDofAudio = new() { "navigation_6dof", "finish_navigation", };
        private int _audioIndex;

        private TutorialStage _stage;

        internal void Enable()
        {
            audioManager.LoadAudio(new List<string> { "Audio_Tutorial_Navigation", });
            discomfortScore.SetActive(false);
            StartTutorial();
        }

        #region Event Callbacks
        public void OnEndReached()
        {
            if (_stage                                               != TutorialStage.Finish) return;
            if (ExperimentDataManager.Instance.GetNavigationMethod() != LocomotionDof.FourDoF) return;

            NavigationComplete();
        }

        private void NavigationComplete()
        {
            ActionAssetEnabler actionController = FindObjectOfType<ActionAssetEnabler>();
            actionController.EnableUiInput();
            discomfortScore.SetActive(true);
            pointerLeft.Enable();
            pointerRight.Enable();
            EventManager.Subscribe<SceneControlEventArgs>(EventId.DiscomfortScoreComplete, DiscomfortComplete);
        }
        
        private void DiscomfortComplete(object sender, SceneControlEventArgs args)
        {
            discomfortScore.SetActive(false);
            EndTutorial();
        }

        public void OnEndCollision()
        {
            if (_stage != TutorialStage.Finish) return;
            if (ExperimentDataManager.Instance.GetNavigationMethod() != LocomotionDof.SixDof) return;
            NavigationComplete();
        }

        private async void InitialPracticeEnded(object sender, LocomotionEvaluatorEventArgs args)
        {
            EventManager.Unsubscribe<LocomotionEvaluatorEventArgs>(EventId.LocomotionStarted, InitialPracticeEnded);
            
            await Task.Delay(3400);
            _stage = TutorialStage.Finish;

            _audioIndex++;
            HandleStage();
        }

        public void StopImmediately()
        {
            overlay.FadeToBlackImmediate();

            ExperimentDataManager.Instance.EndExperimentImmediately();
        }
        #endregion

        #region Logic
        private void HandleStage()
        {
            switch (_stage)
            {
                case TutorialStage.Start:
                    PlayAudio(0.5f);
                    EventManager.Subscribe<LocomotionEvaluatorEventArgs>(EventId.LocomotionStarted, InitialPracticeEnded);
                    break;
                case TutorialStage.Finish:
                    PlayAudio(0.5f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async void StartTutorial()
        {
            _audioIndex = 0;

            await Task.Delay(50);

            SetupNavigation();

            await Task.Delay(500);

            follower.followSpeed              = 0;
            parentConstraint.constraintActive = ExperimentDataManager.Instance.GetNavigationMethod() == LocomotionDof.FourDoF;
            follower.follow                   = true; //ExperimentDataManager.Instance.GetNavigationMethod() == LocomotionDof.FourDoF;
            
            if (ExperimentDataManager.Instance.GetNavigationMethod() == LocomotionDof.SixDof)
            {
                pointerLeft.EnableLocomotion();
                pointerRight.EnableLocomotion();
            }
            await Task.Delay(100);
            
            overlay.FadeToClear();

            await Task.Delay(1000);
            
            EventManager.Publish(EventId.FollowPathReady, this, new SceneControlEventArgs());
            
            await Task.Delay(200);
            _stage = TutorialStage.Start;
            HandleStage();
        }

        private async void EndTutorial()
        {
            await Task.Delay(100);

            overlay.FadeToBlack();

            await Task.Delay(1500);

            ExperimentDataManager.Instance.TutorialComplete();
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

        private void SetupNavigation()
        {
            switch (ExperimentDataManager.Instance.GetNavigationMethod())
            {
                case LocomotionDof.None:
                    break;
                case LocomotionDof.FourDoF:
                    tooltipRendererLeft.SetTooltipData(fourDofTooltip);
                    tooltipRendererRight.SetTooltipData(fourDofTooltip);
                    _audioMap       = _fourDofAudio;
                    break;
                case LocomotionDof.SixDof:
                    tooltipRendererLeft.SetTooltipData(sixDofTooltip);
                    tooltipRendererRight.SetTooltipData(sixDofTooltip);
                    _audioMap       = _sixDofAudio;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            EventManager.Publish(EventId.EnableLocomotion, this, EventArgs.Empty);

            if (!CheckTeleportation()) return;
            
            float height = ExperimentDataManager.Instance.GetHeight();
            footVisualMarker.localPosition =  new Vector3(0, -height, 0);
            height                         *= 0.5f;

            SplineSample sample = follower.spline.Evaluate(0);
            teleportLocation.position = sample.position + new Vector3(0, height, 0);
            teleportLocation.forward  = sample.forward;
            teleporter.Teleport(teleportLocation);

            sample = follower.spline.Evaluate(follower.spline.pointCount - 1);
            endPoint.Place(sample.position);
            
        }
        #endregion

        #region Helper Functions
        private async void PlayAudio(float delay = 0)
        {
            while (AudioManager.IsPlaying)
            {
                await Task.Yield();
            }

            audioManager.PlayClip(_audioMap[_audioIndex], delay);
        }
        #endregion
    }
}