namespace PathNav
{
    using UnityEngine.Rendering;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;

    public class Dummy : MonoBehaviour
    {
        public RenderTexture rt;
        private Texture2D _texture;
        private RenderTexture _activeRT;
        private Rect _rect;
        void Start()
        {
            var format = rt.graphicsFormat;
            _texture                                  =  new Texture2D(1, 1, format, TextureCreationFlags.None);
            _rect                                     =  new Rect(0, 0, 1, 1);
            RenderPipelineManager.endContextRendering += OnEndContextRendering;
        }

        void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            _activeRT = RenderTexture.active;
            RenderTexture.active = rt;
            _texture.ReadPixels(_rect, 0, 0);
            //_texture.Apply();
            var pixel = _texture.GetPixel(0, 0);
            Debug.Log(pixel);
            RenderTexture.active = _activeRT;
            
        }

        void OnDestroy()
        {
            RenderPipelineManager.endContextRendering -= OnEndContextRendering;
        }

        // public void OnGUI()
        // {
        //     if (rt != null)
        //     {
        //         //
        //         GUI.DrawTexture(new Rect(0, 0, 100, 100), rt);
        //     }
        // }
    }
}