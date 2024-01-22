namespace PathNav.ExperimentControl
{
    using Events;
    using HTC.UnityPlugin.Vive;
    using Interaction;
    using PathPlanning;
    using SceneManagement;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TMPro;
    using UnityEngine;

    public class TutorialCreation : MonoBehaviour
    {
        private enum TutorialStage
        {
            Start,
            Create,
            Move,
            Undo,
            Finish,
        }

        [SerializeField] private Transform teleportLocation;
        [SerializeField] private Teleporter teleporter;
        [SerializeField] private Overlay overlay;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private DefaultTooltipRenderer tooltipRendererLeft;
        [SerializeField] private DefaultTooltipRenderer tooltipRendererRight;
        [SerializeField] private DefaultTooltipRenderDataAsset drawingTooltip;
        [SerializeField] private DefaultTooltipRenderDataAsset interpolationTooltip;
        [SerializeField] private TextMeshPro textMesh;

        private bool _enableTeleportation;
        private List<string> _audioMap;
        private List<string> _drawingAudio = new() { "place_start", "draw_path", "erase_path", "finish_path", };
        private List<string> _interpolationAudio = new() { "place_start", "place_point", "move_point", "delete_point", "finish_path", };
        private int _audioIndex;

        private TutorialStage _stage;

        private const string _placeStart = "Press and hold the trigger button to place the start point.";
        private const string _drawPath = "Use the trigger button to draw a path.";
        private const string _placePoint = "Use the plane and the controller to view points. Press the trigger button to create the point.";
        private const string _deletePoint = "Hover over a point and press the yellow button to delete it.";
        private const string _movePoint = "Hover over a point, then press and hold the trigger button to move it to a new location.";
        private const string _erasePath = "Press and hold the yellow button to view the eraser. Use it to remove part of your path.";
        private const string _finishPath = "When you are finished, press the green button to complete your path.";

        internal void Enable()
        {
            _enableTeleportation = CheckTeleportation();

            audioManager.LoadAudio(new List<string> { "Audio_Tutorial_Creation", });

            StartTutorial();
        }
        
        #region Event Callbacks
        private void SplineComplete(object sender, SegmentEventArgs args)
        {
            EventManager.Unsubscribe<SegmentEventArgs>(EventId.SegmentComplete, SplineComplete);
            EndTutorial();
        }
        private void StartPointPlaced(object sender, PlacementEventArgs args)
        {
            EventManager.Unsubscribe<PlacementEventArgs>(EventId.StartPointPlaced, StartPointPlaced);
            _stage = TutorialStage.Create;
            _audioIndex++;
            HandleStage();
        }
        private void PointPlaced(object sender, PathStrategyEventArgs args)
        {
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.PointPlaced, PointPlaced);
            _stage = TutorialStage.Move;
            _audioIndex++;
            HandleStage();
        }
        private void DrawEnded(object sender, PathStrategyEventArgs args)
        {
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.DrawEnded, DrawEnded);
            _stage = TutorialStage.Undo;
            _audioIndex++;
            HandleStage();
        }
        private void MoveEnded(object sender, PathStrategyEventArgs args)
        {
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.MoveEnded, MoveEnded);
            _stage = TutorialStage.Undo;
            _audioIndex++;
            HandleStage();
        }
        private void PointDeleted(object sender, PathStrategyEventArgs args)
        {
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.PointDeleted, PointDeleted);
            _stage = TutorialStage.Finish;
            _audioIndex++;
            HandleStage();
        }
        private void EraseEnded(object sender, PathStrategyEventArgs args)
        {
            EventManager.Unsubscribe<PathStrategyEventArgs>(EventId.EraseToggleOff, EraseEnded);
            _stage = TutorialStage.Finish;
            _audioIndex++;
            HandleStage();
        }
        #endregion

        #region Stage Logic
        private void SetupStrategy()
        {
            switch (ExperimentDataManager.Instance.GetCreationMethod())
            {
                case PathStrategy.Bulldozer:
                    tooltipRendererLeft.SetTooltipData(drawingTooltip);
                    tooltipRendererRight.SetTooltipData(drawingTooltip);
                    _audioMap = _drawingAudio;
                    break;
                case PathStrategy.Spatula:
                    tooltipRendererLeft.SetTooltipData(interpolationTooltip);
                    tooltipRendererRight.SetTooltipData(interpolationTooltip);
                    _audioMap = _interpolationAudio;
                    break;
                case PathStrategy.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            
        }

        private void HandleStage()
        {
            switch (_stage)
            {
                case TutorialStage.Start:
                    PlayAudio();
                    textMesh.text = _placeStart;
                    EventManager.Subscribe<PlacementEventArgs>(EventId.StartPointPlaced, StartPointPlaced);
                    break;
                case TutorialStage.Create:
                    PlayAudio(0.5f);
                    if (ExperimentDataManager.Instance.GetCreationMethod() == PathStrategy.Bulldozer)
                    {
                        textMesh.text = _drawPath;
                        EventManager.Subscribe<PathStrategyEventArgs>(EventId.DrawEnded, DrawEnded);
                    }
                    else
                    {
                        textMesh.text = _placePoint;
                        EventManager.Subscribe<PathStrategyEventArgs>(EventId.PointPlaced, PointPlaced);
                    }
                    break;
                case TutorialStage.Move:
                    PlayAudio(0.5f);
                    textMesh.text = _movePoint;
                    EventManager.Subscribe<PathStrategyEventArgs>(EventId.MoveEnded, MoveEnded);
                    break;
                case TutorialStage.Undo:
                    PlayAudio(0.5f);
                    if (ExperimentDataManager.Instance.GetCreationMethod() == PathStrategy.Bulldozer)
                    {
                        textMesh.text = _erasePath;
                        EventManager.Subscribe<PathStrategyEventArgs>(EventId.EraseToggleOff, EraseEnded);
                    }
                    else
                    {
                        textMesh.text = _deletePoint;
                        EventManager.Subscribe<PathStrategyEventArgs>(EventId.PointDeleted, PointDeleted);
                    }
                    break;
                case TutorialStage.Finish:
                    PlayAudio(0.5f);
                    textMesh.text = _finishPath;
                    EventManager.Publish(EventId.AllowPathCompletion, this, new SceneControlEventArgs());
                    EventManager.Subscribe<SegmentEventArgs>(EventId.SegmentComplete, SplineComplete);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async void EndTutorial()
        {
            while (AudioManager.IsPlaying)
            {
                await Task.Yield();
            }
            
            await Task.Delay(100);

            overlay.FadeToBlack();

            await Task.Delay(1500);

            ExperimentDataManager.Instance.TutorialComplete();
        }

        private async void StartTutorial()
        {
            _audioIndex = 0;

            await Task.Delay(500);

            SetupStrategy();

            if (_enableTeleportation)
            {
                teleporter.Teleport(teleportLocation);
            }

            await Task.Delay(1000);

            overlay.FadeToClear();

            await Task.Delay(1000);

            _stage = TutorialStage.Start;
            HandleStage();
        }
        #endregion

        #region Helper Functions
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