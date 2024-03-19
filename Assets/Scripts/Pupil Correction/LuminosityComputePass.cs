namespace PathNav.ExperimentControl
{
    using Unity.Collections;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
    using System.Collections.Generic;
    using Unity.Mathematics;
    using UnityEngine;
    public class LuminosityComputePass : ScriptableRenderPass
    {
        private RenderTargetIdentifier _sourceTargetId;
        private RenderTargetIdentifier _temporaryTargetId;
        private readonly RenderTargetIdentifier _destinationTargetId;

        private RenderTargetHandle _temporaryTargetHandle;
        private readonly RenderTextureDescriptor _tempDescriptor;

        private readonly string _profilerTag;
        private readonly int _blitTextureID = Shader.PropertyToID("_LuminosityTex");

        private readonly ComputeShader _luminanceCompute;
        private readonly int _luminanceKernel;

        private readonly ComputeBuffer _luminanceBuffer;
        private NativeArray<float3> _buffer;
        private Data _data;
        private Vector3 _eyeDirection;
        private readonly Queue<Data> _luminance;

        private int _width, _height;
        private readonly int _groupSizeX;
        private readonly int _groupSizeY;
        private int _threadsX, _threadsY;
        private float _totalPixels;
        
        private float _tanHalfVerticalFov;
        private float _tanHalfHorizontalFov;

        public LuminosityComputePass
        (RenderTextureDescriptor descriptor,
         RenderTargetIdentifier  destinationId,
         ComputeShader           shader,
         ComputeBuffer           buffer,
         Queue<Data>             queue,
         Vector3                 eye,
         string                  tag)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            _profilerTag         = tag;
            _tempDescriptor      = descriptor;
            _destinationTargetId = destinationId;
            _temporaryTargetHandle.Init("_LuminosityTex");
            _eyeDirection = eye;

            _luminanceCompute = shader;

            _luminanceKernel = _luminanceCompute.FindKernel("compute_luminance");
            _luminanceBuffer = buffer;
            _luminance       = queue;

            if (_luminanceKernel < 0) return;

            _luminanceCompute.GetKernelThreadGroupSizes(_luminanceKernel, out uint sizeX, out uint sizeY, out uint _);
            _groupSizeX = (int)sizeX;
            _groupSizeY = (int)sizeY;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ScriptableRenderer renderer = renderingData.cameraData.renderer;
            _sourceTargetId       = renderer.cameraColorTarget;
            _tanHalfVerticalFov   = Mathf.Tan(Mathf.Deg2Rad * renderingData.cameraData.camera.fieldOfView / 2.0f);
            _tanHalfHorizontalFov = _tanHalfVerticalFov * renderingData.cameraData.camera.aspect;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(_temporaryTargetHandle.id, _tempDescriptor, FilterMode.Bilinear);
            _width    = _tempDescriptor.width;
            _height   = _tempDescriptor.height;
            _threadsX = Mathf.CeilToInt(_width  / (float)_groupSizeX);
            _threadsY = Mathf.CeilToInt(_height / (float)_groupSizeY);
            _totalPixels = _width * _height;
            _buffer   = new NativeArray<float3>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            //Cannot use ConfigureTarget(RenderTargetIdentifier[] colorAttachment) here? Forces output to screen...
            ConfigureTarget(new RenderTargetIdentifier(_destinationTargetId,                0, CubemapFace.Unknown, -1));
            ConfigureTarget(new RenderTargetIdentifier(_temporaryTargetHandle.Identifier(), 0, CubemapFace.Unknown, -1));
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_luminanceCompute    == null) return;
            if (_luminanceKernel     < 0) return;

            CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);
            
            
            cmd.SetGlobalTexture(_blitTextureID, _sourceTargetId);
            //XR SPI workaround?
            //cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _settings.blitMaterial, 0,
            //     _settings.blitMaterialPassIndex);
            cmd.Blit(_sourceTargetId, _temporaryTargetHandle.Identifier());

            cmd.SetComputeTextureParam(_luminanceCompute, _luminanceKernel, "linear_source", _temporaryTargetHandle.Identifier(), 0);
            cmd.SetComputeIntParams(_luminanceCompute, "source_size", new int[] { _width, _height, });
            cmd.SetComputeFloatParams(_luminanceCompute, "gaze_direction", new float[] { _eyeDirection.x, _eyeDirection.y, _eyeDirection.z, });
            cmd.SetComputeFloatParams(_luminanceCompute, "tan_half_fov", new float[] { _tanHalfHorizontalFov, _tanHalfVerticalFov, });
            cmd.SetComputeBufferParam(_luminanceCompute, _luminanceKernel, "average_luminance", _luminanceBuffer);
            cmd.DispatchCompute(_luminanceCompute, _luminanceKernel, _threadsX, _threadsY, 1);
            cmd.RequestAsyncReadback(_luminanceBuffer,
                                     request =>
                                     {
                                         _buffer = request.GetData<float3>();

                                         _data = new Data
                                         {
                                             luminance           = _buffer[0].x /_totalPixels,
                                             foveaLuminance      = _buffer[0].y /_totalPixels,
                                             backgroundLuminance = _buffer[0].z,
                                             timestamp           = LSL.LSL.local_clock(),
                                         };

                                         _luminance.Enqueue(_data);
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
}