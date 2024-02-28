namespace PathNav.ExperimentControl
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using System.IO;
    using Unity.Collections;
    using UnityEngine.Rendering;

    //based on: https://gist.github.com/stonstad/8db9bfd80d189b55ec7d9edf810e18b7
    public class Luminosity : MonoBehaviour
    {
       private RenderTexture _renderTexture;
       private int _width;
       private int _height;
       private bool _linear;
       private TextureFormat _format;
       
       public Camera cameraUsedToRender;
       public Material calculation;
       RenderTexture _RenderTexture;
       RenderTexture _RenderTexture2;
        
       Texture2D debugRTBackground;
       public float SizeScalar = 0.25f;
        private void Start()
        {
            debugRTBackground = new Texture2D(2, 2);
            debugRTBackground.SetPixels(new Color[4] { Color.yellow, Color.yellow, Color.yellow, Color.yellow });
            debugRTBackground.Apply();
            
             (_width, _height) = ((cameraUsedToRender.pixelWidth), (cameraUsedToRender.pixelHeight));
            _RenderTexture     = new RenderTexture(_width, _height, 0);
            _RenderTexture.Create();
            _RenderTexture2 = new RenderTexture(_width, _height, 0);
            _RenderTexture2.Create();
            
            RenderPipelineManager.endContextRendering += OnEndContextRendering;
        }
        
        private void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            //RenderTexture.active = _RenderTexture;
            Graphics.Blit( cameraUsedToRender.targetTexture, _RenderTexture);
            cameraUsedToRender.targetTexture = null;
        }
        
        private void OnDisable()
        {
            RenderPipelineManager.endContextRendering -= OnEndContextRendering;
        }
        
        void OnGUI()
        {
            // Draw the debug render target (which should have the SceneView RT copied to it)
            GUI.DrawTexture(new Rect(0, 0, 145, 65), debugRTBackground);
            GUI.DrawTexture(new Rect(1, 1, 140, 60), _RenderTexture);
        }
    }
}
