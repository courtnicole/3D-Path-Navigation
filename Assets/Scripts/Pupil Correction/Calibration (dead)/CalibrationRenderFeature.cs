namespace PathNav
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering.Universal;
    public class CalibrationRenderFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class CalibrationSettings
        {
            public ComputeShader calibrationComputeShader;
            public Material calibrationMaterial;
            public float calibrationTarget;
        }
        
        public CalibrationSettings settings = new();
        private CalibrationPass _calibrationPass;
        private ComputeShader _calibrationCompute;
        public override void Create()
        {
            if(settings.calibrationComputeShader == null) return;
            
            _calibrationCompute = settings.calibrationComputeShader;
            _calibrationPass = new CalibrationPass(_calibrationCompute, settings.calibrationMaterial, settings.calibrationTarget, "Calibration");
        }
        
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if(settings.calibrationComputeShader == null) return;
            if(settings.calibrationMaterial == null) return;
            
            _calibrationPass.ConfigureInput(ScriptableRenderPassInput.Color);
            renderer.EnqueuePass(_calibrationPass);
        }
    }
}