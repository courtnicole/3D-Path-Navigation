namespace PathNav.Interaction
{
    using System;
    using Extensions.Editor;
    using Events;
    using Extensions;
    using UnityEngine;

    public class RaycastVisualizer : MonoBehaviour
    {
        [SerializeField] [InterfaceType(typeof(IRaycastResultProvider))]
        private UnityEngine.Object raycastSource;

        public IRaycastResultProvider RaycastSource => raycastSource as IRaycastResultProvider;
        
        [SerializeField] private RaycastVisualInfo visualInfo;
        [SerializeField] private Renderer rayRenderer;
        [SerializeField] private MaterialPropertyBlockEditor materialPropertyBlockEditor;

        private UniqueId _source;
        
        private float MaxRayLength => visualInfo.MaxRayLength;
        private float DefaultRayLength => visualInfo.DefaultRayLength;

        private Transform RayTransform => transform;

        private Color IdleColor0 => visualInfo.IdleColor0;
        private Color IdleColor1 => visualInfo.IdleColor1;

        private Color HoverColor0 => visualInfo.HoverColor0;
        private Color HoverColor1 => visualInfo.HoverColor1;

        private Color SpawnColor0 => visualInfo.SpawnColor0;
        private Color SpawnColor1 => visualInfo.SpawnColor1;

        private Color SelectColor0 => visualInfo.SelectColor0;
        private Color SelectColor1 => visualInfo.SelectColor1;

        private int _shaderColor0 = Shader.PropertyToID("_Color0");
        private int _shaderColor1 = Shader.PropertyToID("_Color1");

        private bool _initialized;

        private void OnEnable()
        {
            _source = RaycastSource.Id;

            if (!_initialized)
            {
                Initialize();
            }

            if (rayRenderer.enabled) rayRenderer.enabled = false;
            SubscribeToRaycastEvents();
        }

        private void OnDisable()
        {
            if (rayRenderer.enabled) rayRenderer.enabled = false;
            UnsubscribeToRaycastEvents();
        }

        private void Initialize()
        {
            _initialized = true;
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

            if (args.Result.SurfaceHit.HitSurface)
            {
                UpdateVisualOnHit(args.Result);
            }
            else
            {
                UpdateVisual(args.Result);
            }
        }

        private void UpdateVisualOnHit(IRaycastResultProvider result)
        {
            if (CheckIfProviderDisabled())
            {
                UpdateRendererEnabledState(false);
                return;
            }

            UpdateRendererEnabledState(true);

            UpdatePositionRotationLook(result.RayOrigin, result.Controller.Rotation, result.RayHitPoint);

            UpdateScale(MaxRayLength, result.RayHitPoint);

            UpdateColor();
        }

        private void UpdateVisual(IRaycastResultProvider result)
        {
            if (CheckIfProviderDisabled())
            {
                UpdateRendererEnabledState(false);
                return;
            }

            UpdateRendererEnabledState(true);

            float   rayLength = result.SurfaceHit.Distance > 100f ? DefaultRayLength : result.SurfaceHit.Distance;
            Vector3 end       = result.RayOrigin + rayLength * result.RayDirection;

            UpdatePositionRotationLook(result.RayOrigin, result.Controller.Rotation, end);

            UpdateScale(rayLength);

            UpdateColor();
        }

        private bool CheckIfProviderDisabled() => false; //_providerState == RaycastState.Disabled;

        private void UpdateRendererEnabledState(bool state)
        {
            if (rayRenderer.enabled != state)
            {
                rayRenderer.enabled = state;
            }
        }

        private void UpdatePositionRotationLook(Vector3 position, Quaternion rotation, Vector3 lookPoint)
        {
            RayTransform.SetPositionAndRotation(position, rotation);
            RayTransform.LookAt(lookPoint);
        }

        private void UpdateScale(float distance)
        {
            Vector3 localScale = RayTransform.localScale;

            localScale              = new Vector3(localScale.x, localScale.y, distance);
            RayTransform.localScale = localScale;
        }

        private void UpdateScale(float maxLength, Vector3 endPoint)
        {
            float   scale      = Mathf.Min(maxLength, (endPoint - RayTransform.position).magnitude);
            Vector3 localScale = RayTransform.localScale;

            localScale              = new Vector3(localScale.x, localScale.y, scale);
            RayTransform.localScale = localScale;
        }

        private void UpdateColor()
        {
            //switch (_providerState)
            //{
            //    case RaycastState.Idle:
            //        materialPropertyBlockEditor.MaterialPropertyBlock.SetColor(_shaderColor0, IdleColor0);
            //        materialPropertyBlockEditor.MaterialPropertyBlock.SetColor(_shaderColor1, IdleColor1);
            //        break;
            //    case RaycastState.Hover:
            //        materialPropertyBlockEditor.MaterialPropertyBlock.SetColor(_shaderColor0, HoverColor0);
            //        materialPropertyBlockEditor.MaterialPropertyBlock.SetColor(_shaderColor1, HoverColor1);
            //        break;
            //    case RaycastState.Select:
            //        materialPropertyBlockEditor.MaterialPropertyBlock.SetColor(_shaderColor0, SelectColor0);
            //        materialPropertyBlockEditor.MaterialPropertyBlock.SetColor(_shaderColor1, SelectColor1);
            //        break;
            //    case RaycastState.CanSpawn:
            //        materialPropertyBlockEditor.MaterialPropertyBlock.SetColor(_shaderColor0, SpawnColor0);
            //        materialPropertyBlockEditor.MaterialPropertyBlock.SetColor(_shaderColor1, SpawnColor1);
            //        break;
            //    case RaycastState.Disabled:
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException();
            //}
        }
    }
}