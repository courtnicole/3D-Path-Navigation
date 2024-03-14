using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathNav
{
    using ExperimentControl;
    using Unity.Collections;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    public struct Data
    {
        public float luminance;
        public double timestamp;
    }
    public class LuminosityPass : ScriptableRenderPass
        {

            
            //private readonly LuminosityRenderFeature.BlitSettings _settings;

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
            private Data _data;
            private readonly Queue<Data> _luminance;

            private int _width, _height;
            private readonly int _groupSizeX;
            private readonly int _groupSizeY;
            private int _threadsX, _threadsY;
            
            public LuminosityPass(RenderPassEvent renderPassEvent, RenderTextureDescriptor              descriptor,      RenderTargetIdentifier               destinationId,
             ComputeShader                        shader,          ref ComputeBuffer                    buffer,
             ref Queue<Data>                      queue,           string                               tag)
            {
                this.renderPassEvent = renderPassEvent;
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
                cmd.Blit(_sourceTargetId, _temporaryTargetHandle.Identifier());

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
                                             _buffer                                       = request.GetData<float>();
                                             _data.luminance                               = _buffer[0];
                                             _data.timestamp                            = LSL.LSL.local_clock();
                                             if (DataLogger.Instance != null)
                                             {
                                                 DataLogger.Instance.LogGazeData();
                                             }
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
