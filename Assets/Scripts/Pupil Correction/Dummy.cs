using UnityEngine.Experimental.Rendering;

namespace PathNav
{
    using UnityEngine.Rendering;
    using System.Collections.Generic;
    using UnityEngine;

    public class Dummy : MonoBehaviour
    {
        public RenderTexture rt;
        
        public ComputeShader readPixelCompute;
        private GraphicsBuffer  _outputBuffer;
        private int _readPixelKernel, _threadGroupSize;
        private float[] _luminance;
        
        private Texture2D _texture;
        private RenderTexture _activeRT;
        private Rect _rect;
        void Start()
        {
            var format = rt.graphicsFormat;
            _texture                                  =  new Texture2D(1, 1, format, TextureCreationFlags.None);
            _rect                                     =  new Rect(0, 0, 1, 1);
            // if (rt != null)
            // {
            //     SetupComputeVariables();
            //     RenderPipelineManager.endContextRendering += RunComputeShader;
            // }
            RenderPipelineManager.endContextRendering += OnEndContextRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.endContextRendering -= OnEndContextRendering;
            //RenderPipelineManager.endContextRendering -= RunComputeShader;
            //DisposeComputeVariables();
        }

        private void SetupComputeVariables()
        {
            _readPixelKernel = readPixelCompute.FindKernel("readPixel");
            
            readPixelCompute.GetKernelThreadGroupSizes(_readPixelKernel,
                out uint sizeX, out uint _, out var _);
            _threadGroupSize = (int)sizeX;
            _luminance = new float[1];
            _outputBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, sizeof(float));
            readPixelCompute.SetBuffer(_readPixelKernel, "luminance", _outputBuffer);
            readPixelCompute.SetTexture(_readPixelKernel, "source", rt);
        }

        private void DispatchComputeShader()
        {
            readPixelCompute.Dispatch(_readPixelKernel, 1, 1, 1);
            _outputBuffer.GetData(_luminance);
            //Debug.Log(_luminance[0]);
        }
        
        private void DisposeComputeVariables()
        {
            _outputBuffer?.Dispose();
        }

        void RunComputeShader(ScriptableRenderContext context, List<Camera> cameras)
        {
            DispatchComputeShader();
        }
        void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            _activeRT = RenderTexture.active;
            RenderTexture.active = rt;
            _texture.ReadPixels(_rect, 0, 0);
            //_texture.Apply();
            //var pixel = _texture.GetPixel(0, 0);
            //Debug.Log(pixel);
            RenderTexture.active = _activeRT;
        }
        
        public void OnGUI()
        {
            if (rt != null)
            {
                //
                GUI.DrawTexture(new Rect(0, 0, 100, 100), rt);
            }
        }
    }
}