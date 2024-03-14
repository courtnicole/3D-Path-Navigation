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
        private Queue<Data> _luminanceQueue;
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
            _luminanceQueue   = new Queue<Data>();

            _luminosityPass = new LuminosityPass(settings.renderPassEvent,
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