using Unity.Collections;

namespace PathNav
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.Experimental.Rendering;

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

            private readonly string _profilerTag;
            private readonly int _blitTextureID = Shader.PropertyToID("_LuminosityTex");

            private readonly ComputeShader _pixelComputeShader;
            private readonly int _linearToXYZKernel;
            private readonly int _readLuminanceKernel;

            private readonly ComputeBuffer _luminanceBuffer;
            private NativeArray<float> _buffer;
            private readonly Queue<float> _luminance;

            private int _width, _height;
            private readonly int _groupSizeX;
            private readonly int _groupSizeY;
            private int _threadsX, _threadsY;
            
            public LuminosityPass(RenderPassEvent         renderPassEvent, BlitSettings            settings,
             RenderTextureDescriptor descriptor, RenderTargetIdentifier  destinationId,
             ComputeShader           shader, ref ComputeBuffer       buffer,
             ref Queue<float>        queue, string                  tag)
            {
                this.renderPassEvent = renderPassEvent;
                _settings            = settings;
                _profilerTag         = tag;
                _tempDescriptor      = descriptor;
                _destinationTargetId = destinationId;
                _temporaryTargetHandle.Init("_LuminosityTex");

                _pixelComputeShader = shader;

                _readLuminanceKernel = _pixelComputeShader.FindKernel("read_luminance");
                _luminanceBuffer     = buffer;
                _luminance           = queue;

                _linearToXYZKernel = _pixelComputeShader.FindKernel("linear_to_xyz");

                if (_linearToXYZKernel < 0) return;

                _pixelComputeShader.GetKernelThreadGroupSizes(_linearToXYZKernel, out uint sizeX, out uint sizeY, out var _);
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
                _width    = _tempDescriptor.width;
                _height   = _tempDescriptor.height;
                _threadsX = Mathf.CeilToInt(_tempDescriptor.width  / (float)_groupSizeX);
                _threadsY = Mathf.CeilToInt(_tempDescriptor.height / (float)_groupSizeY);
                _buffer   = new NativeArray<float>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

                //Cannot use ConfigureTarget(RenderTargetIdentifier[] colorAttachment) here? Forces output to screen...
                ConfigureTarget(new RenderTargetIdentifier(_destinationTargetId,                0, CubemapFace.Unknown, -1));
                ConfigureTarget(new RenderTargetIdentifier(_temporaryTargetHandle.Identifier(), 0, CubemapFace.Unknown, -1));
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                // if (renderingData.cameraData.camera.cameraType != CameraType.Game)
                //     return;

                if (_linearToXYZKernel   < 0) return;
                if (_readLuminanceKernel < 0) return;

                CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);

                cmd.SetGlobalTexture(_blitTextureID, _sourceTargetId);
                //XR SPI workaround
                //cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _settings.blitMaterial, 0,
                //     _settings.blitMaterialPassIndex);
                cmd.Blit(_sourceTargetId, _temporaryTargetHandle.Identifier(), _settings.blitMaterial, _settings.blitMaterialPassIndex);

                cmd.SetComputeTextureParam(_pixelComputeShader, _linearToXYZKernel, "linear_source", _temporaryTargetHandle.Identifier(), 0);
                cmd.SetComputeIntParam(_pixelComputeShader, "source_width", _width);
                cmd.SetComputeIntParam(_pixelComputeShader, "source_height", _width);
                cmd.DispatchCompute(_pixelComputeShader, _linearToXYZKernel, _threadsX, _threadsY, 1);
                cmd.GenerateMips(_temporaryTargetHandle.Identifier());

                cmd.SetComputeTextureParam(_pixelComputeShader, _readLuminanceKernel, "mip_source", _temporaryTargetHandle.Identifier(), 8);
                cmd.SetComputeBufferParam(_pixelComputeShader, _readLuminanceKernel, "luminance", _luminanceBuffer);
                cmd.DispatchCompute(_pixelComputeShader, _readLuminanceKernel, 1, 1, 1);

                cmd.RequestAsyncReadback(_luminanceBuffer,
                                         request =>
                                         {
                                             _buffer = request.GetData<float>();
                                             _luminance.Enqueue(_buffer[0]);
                                         });

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }

            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                _buffer.Dispose();
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

            public ComputeShader luminanceComputeShader;
        }

        public BlitSettings settings = new();
        private LuminosityPass _luminosityPass;
        private RenderTextureDescriptor _tempDescriptor;
        private RenderTexture _dstTextureObject;

        private ComputeShader _luminanceCompute;
        private Queue<float> _luminanceQueue;
        private ComputeBuffer _luminanceBuffer;

        public override void Create()
        {
            _tempDescriptor = new RenderTextureDescriptor(256, 256)
            {
                graphicsFormat    = GraphicsFormat.R16G16B16A16_SFloat,
                autoGenerateMips  = false,
                useMipMap         = true,
                depthBufferBits   = 0,
                enableRandomWrite = true,
            };

            _dstTextureObject = settings.dstTextureObject;
            int passIndex = settings.blitMaterial != null ? settings.blitMaterial.passCount - 1 : 1;
            settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);

            _luminanceCompute = settings.luminanceComputeShader;
            _luminanceBuffer  = new ComputeBuffer(1, sizeof(float), ComputeBufferType.Structured);
            _luminanceQueue   = new Queue<float>();

            _luminosityPass = new LuminosityPass(settings.renderPassEvent,
                                                 settings,
                                                 _tempDescriptor,
                                                 new RenderTargetIdentifier(_dstTextureObject),
                                                 _luminanceCompute,
                                                 ref _luminanceBuffer,
                                                 ref _luminanceQueue,
                                                 name);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // if (renderingData.cameraData.cameraType != CameraType.Game)
            //     return;

            if (settings.luminanceComputeShader == null) return;
            if (settings.blitMaterial == null)
            {
                Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.",
                                       GetType().Name);
                return;
            }

            _luminosityPass.ConfigureInput(ScriptableRenderPassInput.Color);
            renderer.EnqueuePass(_luminosityPass);
        }

        protected override void Dispose(bool disposing)
        {
            Debug.Log(_luminanceQueue.Count);
            if (!disposing) return;

            _luminanceBuffer.Dispose();
        }
    }
}