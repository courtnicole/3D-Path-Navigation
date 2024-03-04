namespace PathNav
{
	using System;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.Experimental.Rendering;
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
            BlitSettings _settings;

            private RenderTargetIdentifier _sourceTargetId; 
            private RenderTargetIdentifier _temporaryTargetId; 
			private RenderTargetIdentifier _destinationTargetId; 

			private RenderTargetHandle _temporaryTargetHandle;
			private RenderTextureDescriptor _tempDescriptor;

			private string _mProfilerTag;
			private int _blitTextureID = Shader.PropertyToID("_MainTex");
			//private Material _blitDirectlyMaterial;
			private bool _hasPrintedError;

			public LuminosityPass(RenderPassEvent renderPassEvent, BlitSettings settings, RenderTextureDescriptor descriptor, string tag) {
				this.renderPassEvent = renderPassEvent;
				_settings            = settings;
				_mProfilerTag        = tag;
				_tempDescriptor      = descriptor;
				_temporaryTargetHandle.Init("_TemporaryColorTexture");
				// if (shaders.blitDirectly == null) {
				// 	Debug.LogError("shaders.blitDirectly is null?");
				// } else {
				// 	_blitDirectlyMaterial = new Material(shaders.blitDirectly);
				// }
			}

			public void ConfigureTargets(RenderTargetIdentifier destination) {
				
				_destinationTargetId = destination;
			}
			
			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
				
				CommandBuffer cmd = CommandBufferPool.Get(_mProfilerTag);
				
				ScriptableRenderer renderer = renderingData.cameraData.renderer;

				_sourceTargetId = renderer.cameraColorTarget;
				
				cmd.GetTemporaryRT(_temporaryTargetHandle.id, _tempDescriptor, FilterMode.Bilinear);
				
				cmd.SetGlobalTexture(_blitTextureID, _sourceTargetId);
				cmd.SetRenderTarget(new RenderTargetIdentifier(_temporaryTargetHandle.Identifier(), 0, CubemapFace.Unknown, -1)); // THIS IS BAD
				cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _settings.blitMaterial, 0, _settings.blitMaterialPassIndex);
				
				//cmd.SetGlobalTexture(_blitTextureID, _temporaryTargetHandle.Identifier());
				cmd.SetRenderTarget(new RenderTargetIdentifier(_destinationTargetId, 8, CubemapFace.Unknown, -1));
				cmd.CopyTexture(_temporaryTargetHandle.Identifier(), 0, 8, _destinationTargetId, 0, 0);
				//cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _blitDirectlyMaterial, 0, 0);

				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();
				CommandBufferPool.Release(cmd);
			}
			public override void OnCameraCleanup(CommandBuffer cmd) {
				cmd.ReleaseTemporaryRT(_temporaryTargetHandle.id);
			}
        }

       [Serializable]
		public class BlitSettings {
			public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
			public Material blitMaterial;
			public int blitMaterialPassIndex;
			public RenderTexture dstTextureObject;
			public RenderTexture tempTextureObject;
		}
		
		public BlitSettings settings = new ();
		private LuminosityPass _luminosityPass;
		private RenderTextureDescriptor _tempDescriptor;
		private RenderTexture _dstTextureObject;

		public override void Create() {
			 _tempDescriptor = new RenderTextureDescriptor(256, 256)
			{
				colorFormat      = RenderTextureFormat.ARGBHalf,
				autoGenerateMips = true,
				useMipMap        = true,
				sRGB             = false,
			};
			
			//_tempDescriptor   = settings.tempTextureObject.descriptor;
			_dstTextureObject = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
			//_dstTextureObject = settings.dstTextureObject;
			int passIndex = settings.blitMaterial != null ? settings.blitMaterial.passCount - 1 : 1;
			settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);
			
			_luminosityPass = new LuminosityPass(settings.renderPassEvent, settings, _tempDescriptor, name);
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {

			if (settings.blitMaterial == null) {
				Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
				return;
			}
			
			_luminosityPass.ConfigureInput(ScriptableRenderPassInput.Color);
			_luminosityPass.ConfigureTargets(new RenderTargetIdentifier(_dstTextureObject));
			renderer.EnqueuePass(_luminosityPass);
		}
	}
}