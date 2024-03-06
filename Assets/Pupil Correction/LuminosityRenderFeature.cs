namespace PathNav
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.Experimental.Rendering;

    /*
     * Based on: 
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
            private readonly BlitSettings _settings;

            private RenderTargetIdentifier _sourceTargetId;
            private RenderTargetIdentifier _temporaryTargetId;
            private readonly RenderTargetIdentifier _destinationTargetId;

            private RenderTargetHandle _temporaryTargetHandle;
            private readonly RenderTextureDescriptor _tempDescriptor;

            private readonly string _mProfilerTag;
            private readonly int _blitTextureID = Shader.PropertyToID("_LuminosityTex");

            private ComputeShader _readPixelCompute;
            private int _readPixelKernel, _groupSizeX, _groupSizeY, threadsX, threadsY;
            private float[] _luminance;

            public LuminosityPass(RenderPassEvent renderPassEvent, BlitSettings settings,
                RenderTextureDescriptor descriptor, RenderTargetIdentifier destinationId,
                ComputeShader shader, string tag)
            {
                this.renderPassEvent = renderPassEvent;
                _settings = settings;
                _mProfilerTag = tag;
                _tempDescriptor = descriptor;
                _destinationTargetId = destinationId;
                _temporaryTargetHandle.Init("_LuminosityTex");
                _readPixelCompute = shader;
                _readPixelKernel = _readPixelCompute.FindKernel("readPixel");
                _readPixelCompute.GetKernelThreadGroupSizes(_readPixelKernel,
                    out uint sizeX, out uint sizeY, out var _);
                _groupSizeX = (int)sizeX;
                _groupSizeY = (int)sizeY;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                ScriptableRenderer renderer = renderingData.cameraData.renderer;
                _sourceTargetId = renderer.cameraColorTarget;
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                cmd.GetTemporaryRT(_temporaryTargetHandle.id, _tempDescriptor, FilterMode.Bilinear);
                threadsX = Mathf.CeilToInt(_tempDescriptor.width / (float)_groupSizeX);
                threadsY = Mathf.CeilToInt(_tempDescriptor.height / (float)_groupSizeY);

                //Cannot use ConfigureTarget(RenderTargetIdentifier[] colorAttachment) here? Forces output to screen...
                ConfigureTarget(new RenderTargetIdentifier(_destinationTargetId, 0, CubemapFace.Unknown, -1));
                ConfigureTarget(new RenderTargetIdentifier(_temporaryTargetHandle.Identifier(), 0, CubemapFace.Unknown,
                    -1));
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get(_mProfilerTag);

                cmd.SetGlobalTexture(_blitTextureID, _sourceTargetId);
                //cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _settings.blitMaterial, 0,
                //     _settings.blitMaterialPassIndex);
                cmd.Blit(_sourceTargetId, _temporaryTargetHandle.Identifier(), _settings.blitMaterial,
                    _settings.blitMaterialPassIndex);

                cmd.SetComputeTextureParam(_readPixelCompute, _readPixelKernel, "source",
                    _temporaryTargetHandle.Identifier(), 0);
                cmd.DispatchCompute(_readPixelCompute, _readPixelKernel, threadsX, threadsY, 1);
                cmd.GenerateMips(_temporaryTargetHandle.Identifier());
                cmd.CopyTexture(_temporaryTargetHandle.Identifier(), 0, 0, _destinationTargetId, 0, 0);

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }

            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                cmd.ReleaseTemporaryRT(_temporaryTargetHandle.id);
            }
        }

        [Serializable]
        public class BlitSettings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            public Material blitMaterial;
            public int blitMaterialPassIndex;
            public RenderTexture dstTextureObject;

            public ComputeShader _readPixelCompute;
        }

        public BlitSettings settings = new();
        private LuminosityPass _luminosityPass;
        private RenderTextureDescriptor _tempDescriptor;
        private RenderTexture _dstTextureObject;

        private ComputeShader readPixelCompute;

        // private void SetupComputeVariables()
        // {
        //     _outputBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, sizeof(float));
        // }
        // private void DisposeComputeVariables()
        // {
        //     _outputBuffer?.Dispose();
        // }

        public override void Create()
        {
            _tempDescriptor = new RenderTextureDescriptor(256, 256)
            {
                graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat,
                autoGenerateMips = false,
                useMipMap = true,
                depthBufferBits = 0,
                enableRandomWrite = true,
            };

            //_dstTextureObject = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            _dstTextureObject = settings.dstTextureObject;
            int passIndex = settings.blitMaterial != null ? settings.blitMaterial.passCount - 1 : 1;
            settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);

            readPixelCompute = settings._readPixelCompute;
            //SetupComputeVariables();

            _luminosityPass = new LuminosityPass(settings.renderPassEvent, settings, _tempDescriptor,
                new RenderTargetIdentifier(_dstTextureObject),
                readPixelCompute,
                name);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game)
                return;

            if (settings.blitMaterial == null)
            {
                Debug.LogWarningFormat(
                    "Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.",
                    GetType().Name);
                return;
            }

            _luminosityPass.ConfigureInput(ScriptableRenderPassInput.Color);
            renderer.EnqueuePass(_luminosityPass);
        }

        protected override void Dispose(bool disposing)
        {
            //DisposeComputeVariables();
        }
    }
}