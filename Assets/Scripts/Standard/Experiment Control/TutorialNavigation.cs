namespace PathNav.ExperimentControl
{
    using Events;
    using HTC.UnityPlugin.Vive;
    using Interaction;
    using SceneManagement;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

    public class TutorialNavigation : MonoBehaviour
    {
        private enum TutorialStage
        {
            Start,
            MoveForwardBackward,
            MoveUpDown,
            Finish,
        }

        [SerializeField] private Overlay overlay;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private DefaultTooltipRenderer tooltipRendererLeft;
        [SerializeField] private DefaultTooltipRenderer tooltipRendererRight;
        [SerializeField] private DefaultTooltipRenderDataAsset fourDofTooltip;
        [SerializeField] private DefaultTooltipRenderDataAsset sixDofTooltip;

        private List<string> _audioMap;
        private List<string> _fourDofAudio = new() { "forward_backward_4dof", "finish_navigation", };
        private List<string> _sixDofAudio = new() { "forward_backward_6dof", "up_down_6dof", "finish_navigation", };
        private int _audioIndex;

        private TutorialStage _stage;

        internal void Enable()
        {
            audioManager.LoadAudio(new List<string> { "Audio_Tutorial_Navigation", });

            StartTutorial();
        }

        #region Event Callbacks
        public void OnEndReached()
        {
            EndTutorial();
        }

        private void MoveForwardBackwardEnded(object sender, LocomotionEvaluatorEventArgs args)
        {
            EventManager.Unsubscribe<LocomotionEvaluatorEventArgs>(EventId.LocomotionEnded, MoveForwardBackwardEnded);

            _stage = ExperimentDataManager.Instance.GetNavigationMethod() == LocomotionDof.FourDoF ? TutorialStage.Finish : TutorialStage.MoveUpDown;

            _audioIndex++;
            HandleStage();
        }

        private void MoveUpDownEnded(object sender, LocomotionEvaluatorEventArgs args)
        {
            EventManager.Unsubscribe<LocomotionEvaluatorEventArgs>(EventId.LocomotionEnded, MoveUpDownEnded);

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
                    PlayAudio();
                    break;
                case TutorialStage.MoveForwardBackward:
                    PlayAudio(0.5f);
                    EventManager.Subscribe<LocomotionEvaluatorEventArgs>(EventId.LocomotionEnded, MoveForwardBackwardEnded);
                    break;
                case TutorialStage.MoveUpDown:
                    PlayAudio(0.5f);
                    EventManager.Subscribe<LocomotionEvaluatorEventArgs>(EventId.LocomotionEnded, MoveUpDownEnded);
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

            await Task.Delay(500);

            SetupNavigation();

            await Task.Delay(500);

            overlay.FadeToClear();

            await Task.Delay(1000);

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

        private void SetupNavigation()
        {
            switch (ExperimentDataManager.Instance.GetNavigationMethod())
            {
                case LocomotionDof.None:
                    break;
                case LocomotionDof.FourDoF:
                    tooltipRendererLeft.SetTooltipData(fourDofTooltip);
                    tooltipRendererRight.SetTooltipData(fourDofTooltip);
                    _audioMap = _fourDofAudio;
                    break;
                case LocomotionDof.SixDof:
                    tooltipRendererLeft.SetTooltipData(sixDofTooltip);
                    tooltipRendererRight.SetTooltipData(sixDofTooltip);
                    _audioMap = _sixDofAudio;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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