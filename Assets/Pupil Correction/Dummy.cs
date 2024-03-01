namespace PathNav
{
    using UnityEngine.Rendering;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;

    public class Dummy : MonoBehaviour
    {
        public RenderTexture rt;

        void Start()
        {
            RenderPipelineManager.endContextRendering += OnEndContextRendering;
        }

        void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            //sample from rt mip level 2
            
            
        }

        void OnDestroy()
        {
            RenderPipelineManager.endContextRendering -= OnEndContextRendering;
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