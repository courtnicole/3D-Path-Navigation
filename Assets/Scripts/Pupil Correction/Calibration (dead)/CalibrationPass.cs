namespace PathNav
{
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    public class CalibrationPass : ScriptableRenderPass
    {
        private RenderTargetIdentifier _sourceTargetId;
        private RenderTargetIdentifier _temporaryTargetId;
        private RenderTargetIdentifier _cameraDepthTarget;

        private RenderTargetHandle _temporaryTargetHandle;
        
        private Material _material;

        private readonly string _profilerTag;
        private readonly int _blitTextureID = Shader.PropertyToID("_LuminosityTex");

        private readonly ComputeShader _computeShader;
        private readonly int _linearToXYZKernel;
        private readonly int _luminanceCalibrationKernel;

        private readonly float _calibrationTarget;
        private int _mipCount;
        private int _width, _height;
        private readonly int _groupSizeX;
        private readonly int _groupSizeY;
        private int _threadsX, _threadsY;

        public CalibrationPass(ComputeShader shader, Material material, float target, string tag)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            _profilerTag    = tag;
            
            _material = material;
            _calibrationTarget = target;
            
            _temporaryTargetHandle.Init("_LuminosityTex");

            _computeShader              = shader;
            _linearToXYZKernel          = _computeShader.FindKernel("linear_to_xyz");
            _luminanceCalibrationKernel = _computeShader.FindKernel("average_luminance_calibration");

            if (_linearToXYZKernel          < 0) return;
            if (_luminanceCalibrationKernel < 0) return;

            _computeShader.GetKernelThreadGroupSizes(_linearToXYZKernel, out uint sizeX, out uint sizeY,
                out var _);
            _groupSizeX = (int)sizeX;
            _groupSizeY = (int)sizeY;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ScriptableRenderer renderer = renderingData.cameraData.renderer;
            _sourceTargetId   = renderer.cameraColorTarget;
            _cameraDepthTarget = renderer.cameraDepthTarget;
            
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderTextureDescriptor opaqueDesc = cameraTextureDescriptor;
            opaqueDesc.depthBufferBits   = 0;
            opaqueDesc.autoGenerateMips  = false;
            opaqueDesc.useMipMap         = true;
            opaqueDesc.enableRandomWrite = true;
            opaqueDesc.graphicsFormat    = GraphicsFormat.R32G32B32A32_SFloat;
            opaqueDesc.sRGB              = true;
            
            cmd.GetTemporaryRT(_temporaryTargetHandle.id, opaqueDesc, FilterMode.Bilinear);
            
            _width    = opaqueDesc.width;
            _height   = opaqueDesc.height;
            
            _mipCount = (int)Mathf.Floor(Mathf.Log(Mathf.Max(_width, _height), 2));
            
            _threadsX = Mathf.CeilToInt(_width  / (float)_groupSizeX);
            _threadsY = Mathf.CeilToInt(_height / (float)_groupSizeY);
           
            ConfigureTarget(new RenderTargetIdentifier(_temporaryTargetHandle.Identifier(), 0, CubemapFace.Unknown, -1));
            ConfigureTarget(new RenderTargetIdentifier(_sourceTargetId,                     0, CubemapFace.Unknown, -1), _cameraDepthTarget);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // if (renderingData.cameraData.camera.cameraType != CameraType.Game)
            //     return;

            // if (_linearToXYZKernel < 0) return;
            

            ScriptableRenderer renderer = renderingData.cameraData.renderer;
            _sourceTargetId = renderer.cameraColorTarget;
            
            CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);
            
            cmd.GenerateMips(_temporaryTargetHandle.Identifier());

            cmd.SetGlobalTexture(_blitTextureID, _sourceTargetId);
            //cmd.SetRenderTarget(new RenderTargetIdentifier(_temporaryTargetHandle.Identifier(), 0, CubemapFace.Unknown, -1));
            cmd.Blit(_sourceTargetId, _temporaryTargetHandle.Identifier());
            //cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _material);

            cmd.SetComputeTextureParam(_computeShader, _linearToXYZKernel,          "source", _temporaryTargetHandle.Identifier(), 0);
            //cmd.SetComputeTextureParam(_computeShader, _luminanceCalibrationKernel, "linear_source", _temporaryTargetHandle.Identifier(), 0);
            cmd.SetComputeTextureParam(_computeShader, _linearToXYZKernel, "mip_source", _temporaryTargetHandle.Identifier(), _mipCount - 1);
            cmd.SetComputeIntParam(_computeShader, "source_width",  _width);
            cmd.SetComputeIntParam(_computeShader, "source_height", _width);
            cmd.SetComputeFloatParam(_computeShader, "target_luminance", _calibrationTarget);
            cmd.DispatchCompute(_computeShader, _linearToXYZKernel, _threadsX, _threadsY, 1);
            
            GraphicsFence fence = cmd.CreateAsyncGraphicsFence();
            cmd.WaitOnAsyncGraphicsFence(fence);
            
            //cmd.GenerateMips(_temporaryTargetHandle.Identifier());
          
            
            //cmd.SetComputeTextureParam(_computeShader, _luminanceCalibrationKernel, "mip_source",    _temporaryTargetHandle.Identifier(), _mipCount - 1);
            //cmd.DispatchCompute(_computeShader, _luminanceCalibrationKernel, _threadsX, _threadsY, 1);
            
            //cmd.WaitOnAsyncGraphicsFence(fence);
            
            cmd.SetGlobalTexture(_blitTextureID, _temporaryTargetHandle.Identifier());
            //cmd.SetRenderTarget(new RenderTargetIdentifier(_sourceTargetId, 0, CubemapFace.Unknown, -1), _cameraDepthTarget);
            cmd.Blit(_temporaryTargetHandle.Identifier(), _sourceTargetId);
            //cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _material);
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_temporaryTargetHandle.id);
        }
    }
}