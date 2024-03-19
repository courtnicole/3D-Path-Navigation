using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathNav
{
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.XR;

    public class LuminosityController : MonoBehaviour
    {

        public Material blitMaterial;
        private CommandBuffer _commandBuffer;
        private RenderTexture _rTexture;
        private RenderTexture _source;
        
        public void LuminosityBlit()
        {
            if (XRSettings.enabled && XRSettings.isDeviceActive)
            {
                RenderTextureDescriptor desc = GetXREyeTexDesc();
                _rTexture                   = RenderTexture.GetTemporary(desc);
                _rTexture.enableRandomWrite = true;
                _source                     = RenderTexture.active;
                
                _commandBuffer.Blit(_source, _rTexture, blitMaterial);
                RenderTexture.ReleaseTemporary(_rTexture);

                // Tech specific
            }
        }
        private RenderTextureDescriptor GetXREyeTexDesc()
        {
            RenderTextureDescriptor desc = XRSettings.eyeTextureDesc;
            // XRSettings.eyeTextureDesc is using srgb while src is using linear color space, so we have to set color space manually
            desc.sRGB = false;
            return desc;
        }
        
        private void OnAfterRenderingPostProcessing(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            LuminosityBlit();
        }
    }
}
