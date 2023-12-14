namespace PathNav.ExperimentControl
{
    using HTC.UnityPlugin.Vive;
    using Interaction;
    using PathPlanning;
    using SceneManagement;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private List<AudioClipMap> audioMap;
        [SerializeField] private GameObject interpolationExample;
        [SerializeField] private GameObject drawingExample;
        [SerializeField] private Transform teleportLocation;
        [SerializeField] private Teleporter teleporter;
        [SerializeField] private Overlay overlay;
        [SerializeField] private DefaultTooltipRenderer tooltipRendererLeft;
        [SerializeField] private DefaultTooltipRenderer tooltipRendererRight;
        [SerializeField] private DefaultTooltipRenderDataAsset drawingTooltip;
        [SerializeField] private DefaultTooltipRenderDataAsset interpolationTooltip;

        public InputActionReference debugEndTutorial;

        private bool _enableTeleportation;

        internal void Enable()
        {
            debugEndTutorial.action.Enable();
            debugEndTutorial.action.started += TutorialComplete;

            _enableTeleportation = CheckTeleportation();

            StartTutorial();
        }

        private void OnDisable()
        {
            debugEndTutorial.action.Disable();
            debugEndTutorial.action.started -= TutorialComplete;
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
            switch (ExperimentDataManager.Instance.GetCreationMethod())
            {
                case PathStrategy.Bulldozer:
                    drawingExample?.SetActive(show);
                    tooltipRendererLeft.SetTooltipData(drawingTooltip);
                    tooltipRendererRight.SetTooltipData(drawingTooltip);
                    break;
                case PathStrategy.Spatula:
                    interpolationExample?.SetActive(show);
                    tooltipRendererLeft.SetTooltipData(interpolationTooltip);
                    tooltipRendererRight.SetTooltipData(interpolationTooltip);
                    break;
                case PathStrategy.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void TutorialComplete(InputAction.CallbackContext callbackContext)
        {
            EndTutorial();
        }

        private async void EndTutorial()
        {
            await Task.Delay(100);

            overlay.FadeToBlack();

            await Task.Delay(1500);

            ExperimentDataManager.Instance.TutorialComplete();
        }

        private async void StartTutorial()
        {
            await Task.Delay(500);

            ShowExample(true);

            await Task.Delay(500);

            if (_enableTeleportation)
            {
                teleporter.Teleport(teleportLocation);
            }

            await Task.Delay(500);

            overlay.FadeToClear();
        }
    }
}