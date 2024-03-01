namespace PathNav
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    /*
     * https://github.com/Cyanilux/URP_BlitRenderFeature/blob/cmd-drawMesh/Blit.cs
     * Based on the Blit from the UniversalRenderingExamples :
     * https://github.com/Unity-Technologies/UniversalRenderingExamples/tree/master/Assets/Scripts/Runtime/RenderPasses
     * And Blit in XR from URP docs :
     * https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@12.1/manual/renderer-features/how-to-fullscreen-blit-in-xr-spi.html
     */
    public class LuminosityRenderFeature : ScriptableRendererFeature
    {
        public class LuminosityPass : ScriptableRenderPass
        {
            private LuminosityRenderFeatureSettings settings;
            ProfilingSampler m_ProfilingSampler = new ProfilingSampler("ColorBlit");

            private string m_profilerTag;

            private RenderTargetIdentifier source;
            private RenderTargetIdentifier destination;
            private RenderTargetIdentifier cameraDepthTarget;

            private RenderTargetHandle _temporaryColorTexture;
            private readonly int _luminosityTextureID = Shader.PropertyToID("_MainTex");
            private Material _material;
            public LuminosityPass(RenderPassEvent renderPassEvent, LuminosityRenderFeatureSettings settings, Material material, string tag)
            {
                m_profilerTag = tag;
                this.renderPassEvent = renderPassEvent;
                this.settings = settings;
                _material = material;
                _temporaryColorTexture.Init("_TemporaryColorTexture");
            }
            
            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                var opaqueDesc = cameraTextureDescriptor;
                opaqueDesc.depthBufferBits = 0;
                
                cmd.GetTemporaryRT(_temporaryColorTexture.id, opaqueDesc, settings.filterMode);
                
                ConfigureTarget(
                    new RenderTargetIdentifier(_temporaryColorTexture.Identifier(), 0, CubemapFace.Unknown, -1),
                    cameraDepthTarget);
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                var renderer = renderingData.cameraData.renderer;
                cameraDepthTarget = renderer.cameraDepthTarget;

                source = renderer.cameraColorTarget;
                
                destination = new RenderTargetIdentifier(_temporaryColorTexture.id);
            }
            
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var cmd = CommandBufferPool.Get(m_profilerTag);

                if(_material != null)
                {
                    using (new ProfilingScope(cmd, m_ProfilingSampler))
                    {
                        cmd.SetGlobalTexture(_luminosityTextureID, source);
                        cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _material, 0,
                            0);
                    }
                }
                
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                cmd.ReleaseTemporaryRT(_temporaryColorTexture.id);
            }
        }

        [System.Serializable]
        public class LuminosityRenderFeatureSettings
        {
            public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingPostProcessing;
            public Shader shader;
            public FilterMode filterMode = FilterMode.Bilinear;
        }

        public LuminosityRenderFeatureSettings settings = new();
        private LuminosityPass _luminosityPass;

        private Material _material;

        public override void Create()
        {
            if (settings.shader != null)
            {
                _material = CoreUtils.CreateEngineMaterial(settings.shader);
            }
            
            _luminosityPass = new LuminosityPass(settings.renderEvent, settings, _material, name);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (settings.shader != null)
            {
                _luminosityPass.ConfigureInput(ScriptableRenderPassInput.Color);
                renderer.EnqueuePass(_luminosityPass);
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (_material != null)
            {
                CoreUtils.Destroy(_material);
            }
        }
    }
}