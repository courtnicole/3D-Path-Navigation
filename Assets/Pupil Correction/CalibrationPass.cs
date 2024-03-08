namespace PathNav
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    public class CalibrationPass : ScriptableRenderPass
    {
        private RenderTargetIdentifier _sourceTargetId;
        private RenderTargetIdentifier _temporaryTargetId;
        private readonly RenderTargetIdentifier _destinationTargetId;

        private RenderTargetHandle _temporaryTargetHandle;
        private readonly RenderTextureDescriptor _tempDescriptor;

        private readonly string _profilerTag;
        private readonly int _blitTextureID = Shader.PropertyToID("_LuminosityTex");

        private readonly ComputeShader _pixelComputeShader;
        private readonly int _linearToXYZKernel;

        private readonly int _groupSizeX;
        private readonly int _groupSizeY;
        private int _threadsX, _threadsY;

        public CalibrationPass(RenderPassEvent renderPassEvent, RenderTextureDescriptor descriptor, RenderTargetIdentifier destinationId, ComputeShader shader, string tag)
        {
            this.renderPassEvent = renderPassEvent;
            _profilerTag = tag;
            _tempDescriptor = descriptor;
            _destinationTargetId = destinationId;
            _temporaryTargetHandle.Init("_LuminosityTex");

            _pixelComputeShader = shader;
            _linearToXYZKernel = _pixelComputeShader.FindKernel("linear_to_xyz");

            if (_linearToXYZKernel < 0) return;

            _pixelComputeShader.GetKernelThreadGroupSizes(_linearToXYZKernel, out uint sizeX, out uint sizeY,
                out var _);
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
            _threadsX = Mathf.CeilToInt(_tempDescriptor.width / (float)_groupSizeX);
            _threadsY = Mathf.CeilToInt(_tempDescriptor.height / (float)_groupSizeY);

            //Cannot use ConfigureTarget(RenderTargetIdentifier[] colorAttachment) here? Forces output to screen...
            ConfigureTarget(new RenderTargetIdentifier(_destinationTargetId, 0, CubemapFace.Unknown, -1));
            ConfigureTarget(new RenderTargetIdentifier(_temporaryTargetHandle.Identifier(), 0, CubemapFace.Unknown,
                -1));
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.camera.cameraType != CameraType.Game)
                return;

            if (_linearToXYZKernel < 0) return;

            CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);

            cmd.SetGlobalTexture(_blitTextureID, _sourceTargetId);
            //XR SPI workaround?
            //cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _settings.blitMaterial, 0,
            //     _settings.blitMaterialPassIndex);
            cmd.Blit(_sourceTargetId, _temporaryTargetHandle.Identifier());

            cmd.SetComputeTextureParam(_pixelComputeShader, _linearToXYZKernel, "linear_source",
                _temporaryTargetHandle.Identifier(), 0);
            cmd.DispatchCompute(_pixelComputeShader, _linearToXYZKernel, _threadsX, _threadsY, 1);
            cmd.GenerateMips(_temporaryTargetHandle.Identifier());
            
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