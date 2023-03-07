namespace PathNav.Interaction
{
    using Events;
    using Extensions;
    using UnityEngine;

    public class CursorVisualizer : MonoBehaviour
    {
        [SerializeField] [InterfaceType(typeof(IRaycastResultProvider))]
        private UnityEngine.Object raycastSource;

        public IRaycastResultProvider RaycastSource => raycastSource as IRaycastResultProvider;

        [SerializeField] private CursorInfo visualInfo;
        [SerializeField] private Renderer cursorRenderer;

        private UniqueId _source;

        //private RaycastState _providerState;
        private bool _initialized;
        
        private float OffsetAlongNormal => visualInfo.OffsetAlongNormal;

        private Color HoverColor => visualInfo.HoverColor;
        private Color SpawnColor => visualInfo.SpawnColor;
        private Color SelectColor => visualInfo.SelectColor;
        private Color OutlineColor => visualInfo.OutlineColor;

        private Color ColorFromState
        {
            get
            {
                Color color = SelectColor;

                //switch (_providerState)
                //{
                //    case RaycastState.Idle:
                //        break;
                //    case RaycastState.Hover:
                //        color = HoverColor;
                //        break;
                //    case RaycastState.Select:
                //        color = SelectColor;
                //        break;
                //    case RaycastState.CanSpawn:
                //        color = SpawnColor;
                //        break;
                //    case RaycastState.Disabled:
                //        break;
                //}

                return color;
            }
        }

        private int _shaderRadialGradientScale = Shader.PropertyToID("_RadialGradientScale");
        private int _shaderRadialGradientIntensity = Shader.PropertyToID("_RadialGradientIntensity");
        private int _shaderRadialGradientBackgroundOpacity = Shader.PropertyToID("_RadialGradientBackgroundOpacity");
        private int _shaderInnerColor = Shader.PropertyToID("_Color");
        private int _shaderOutlineColor = Shader.PropertyToID("_OutlineColor");

        private void OnEnable()
        {
            _source = RaycastSource.Id;

            if (!_initialized) Initialize();

            cursorRenderer.enabled = false;
            SubscribeToRaycastEvents();
        }

        private void OnDisable()
        {
            UnsubscribeToRaycastEvents();
            cursorRenderer.enabled = false;
        }

        private void Initialize()
        {
            _initialized                = true;
        }

        private void SubscribeToRaycastEvents()
        {
            EventManager.Subscribe<RaycastResultEventArgs>(EventId.RaycastUpdated, OnRaycastUpdated);
        }

        private void UnsubscribeToRaycastEvents()
        {
            EventManager.Unsubscribe<RaycastResultEventArgs>(EventId.RaycastUpdated, OnRaycastUpdated);
        }

        private void OnRaycastUpdated(object sender, RaycastResultEventArgs args)
        {
            if (args.Id.ID != _source.ID) return;
            UpdateVisual(args.Result);
        }

        private void UpdateVisual(IRaycastResultProvider result)
        {
            if (CheckIfProviderDisabled())
            {
                UpdateRendererEnabledState(false);
                return;
            }

            //if (_providerState == RaycastState.Idle)
            //{
            //    UpdateRendererEnabledState(false);
            //    return;
            //}

            if (!result.SurfaceHit.HitSurface)
            {
                //UpdateRendererEnabledState(false);
                UpdateSpatulaVisual(result);
                return;
            }

            UpdateRendererEnabledState(true);

            transform.position = result.RayHitPoint + result.SurfaceHit.Normal * OffsetAlongNormal;
            transform.rotation = Quaternion.LookRotation(result.SurfaceHit.Normal);

            //UpdateMaterial(_providerState == RaycastState.Select);
        }

        private void UpdateSpatulaVisual(IRaycastResultProvider result)
        {
            UpdateRendererEnabledState(true);

            Vector3 position = result.RayOrigin + result.SurfaceHit.Distance * result.RayDirection;
            
            transform.position = position + (-result.RayDirection) * OffsetAlongNormal;
            transform.rotation = Quaternion.LookRotation(Vector3.up);
        }

        private bool CheckIfProviderDisabled() => false; //_providerState == RaycastState.Disabled;

        private void UpdateRendererEnabledState(bool state)
        {
            if (cursorRenderer.enabled != state) cursorRenderer.enabled = state;
        }

        private void UpdateMaterial(bool state)
        {
            cursorRenderer.material.SetFloat(_shaderRadialGradientScale,             state ? 0.2f : 0.101f);
            cursorRenderer.material.SetFloat(_shaderRadialGradientIntensity,         1f);
            cursorRenderer.material.SetFloat(_shaderRadialGradientBackgroundOpacity, 1f);
            cursorRenderer.material.SetColor(_shaderInnerColor,   ColorFromState);
            cursorRenderer.material.SetColor(_shaderOutlineColor, OutlineColor);
        }
    }
}