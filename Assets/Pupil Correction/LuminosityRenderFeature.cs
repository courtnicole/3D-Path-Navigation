namespace PathNav
{
	using System;
	using UnityEditor;
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
            BlitSettings _settings;

            private RenderTargetIdentifier _sourceTargetId; 
            private RenderTargetIdentifier _temporaryTargetId; 
			private RenderTargetIdentifier _destinationTargetId; 

			private RenderTargetIdentifier _cameraDepthTarget;

			private string _mProfilerTag;
			private int _blitTextureID = Shader.PropertyToID("_MainTex");
			private Material _blitDirectlyMaterial;
			private bool _hasPrintedError;

			public LuminosityPass(RenderPassEvent renderPassEvent, BlitSettings settings, BlitShaderResources shaders, string tag) {
				this.renderPassEvent = renderPassEvent;
				_settings = settings;
				_mProfilerTag = tag;
				//_temporaryTargetHandle.Init("_TemporaryColorTexture");
				if (shaders.blitDirectly == null) {
					Debug.LogError("shaders.blitDirectly is null?");
				} else {
					_blitDirectlyMaterial = new Material(shaders.blitDirectly);
				}
			}

			public void ConfigureTargets(RenderTargetIdentifier temporary, RenderTargetIdentifier destination) {
				_temporaryTargetId     = temporary;
				_destinationTargetId   = destination;
			}
			
			public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
				//ConfigureTarget(new RenderTargetIdentifier(_temporaryTargetId,   0, CubemapFace.Unknown, -1));
				//ConfigureTarget(new RenderTargetIdentifier(_destinationTargetId, 0, CubemapFace.Unknown, -1));
			}
			
			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
				
				CommandBuffer cmd = CommandBufferPool.Get(_mProfilerTag);
				//RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
				//opaqueDesc.depthBufferBits = 0;
				
				ScriptableRenderer renderer = renderingData.cameraData.renderer;
				_cameraDepthTarget = renderer.cameraDepthTarget;

				_sourceTargetId = renderer.cameraColorTarget;

				// _destinationTargetId = new RenderTargetIdentifier(_settings.dstTextureObject);

				// opaqueDesc.autoGenerateMips = true;
				// opaqueDesc.useMipMap        = true;
				// opaqueDesc.colorFormat      = RenderTextureFormat.ARGBFloat;
				// int mipLevel = 0;
				// if(opaqueDesc.mipCount > 1) {
				// 	mipLevel = opaqueDesc.mipCount - 1;
				// }
				
				//cmd.GetTemporaryRT(_temporaryTargetHandle.id, opaqueDesc, _settings.filterMode);
				
				cmd.SetGlobalTexture(_blitTextureID, _sourceTargetId);
				cmd.SetRenderTarget(new RenderTargetIdentifier(_temporaryTargetId, 0, CubemapFace.Unknown, -1)); // THIS IS BAD
				cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _settings.blitMaterial, 0, _settings.blitMaterialPassIndex);
				
				cmd.SetGlobalTexture(_blitTextureID, _temporaryTargetId);
				cmd.SetRenderTarget(new RenderTargetIdentifier(_destinationTargetId, 8, CubemapFace.Unknown, -1));
				cmd.CopyTexture(_temporaryTargetId, 0, 8, _destinationTargetId, 0, 0);
				//cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _blitDirectlyMaterial, 0, 0);

				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();
				CommandBufferPool.Release(cmd);
			}
			public override void FrameCleanup(CommandBuffer cmd) {
				//cmd.ReleaseTemporaryRT(_temporaryTargetHandle.id);
			}
        }

       [Serializable]
		public class BlitSettings {
			public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
			public Material blitMaterial;
			public int blitMaterialPassIndex;
			public RenderTexture tempTextureObject;
			public RenderTexture dstTextureObject;
		}

		[Serializable][ReloadGroup]
		public class BlitShaderResources {
			[Reload("LuminosityCalculation.shader")]
			public Shader blitDirectly;
		}
		
		public BlitSettings settings = new ();
		public BlitShaderResources shaders = new ();
		private LuminosityPass _luminosityPass;

		public override void Create() {
#if UNITY_EDITOR
			// This triggers the BlitShaderResources ReloadGroup to reload
			string path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)).Replace("/LuminosityRenderFeature.cs", "");
			ResourceReloader.ReloadAllNullIn(this, path);
#endif

			int passIndex = settings.blitMaterial != null ? settings.blitMaterial.passCount - 1 : 1;
			settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);
			
			_luminosityPass = new LuminosityPass(settings.renderPassEvent, settings, shaders, name);
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {

			if (settings.blitMaterial == null) {
				Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
				return;
			}
			
			_luminosityPass.ConfigureInput(ScriptableRenderPassInput.Color);
			_luminosityPass.ConfigureTargets(new RenderTargetIdentifier(settings.tempTextureObject), new RenderTargetIdentifier(settings.dstTextureObject));
			renderer.EnqueuePass(_luminosityPass);
		}
	}
}