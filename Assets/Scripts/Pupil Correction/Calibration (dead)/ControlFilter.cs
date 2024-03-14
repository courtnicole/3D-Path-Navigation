using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathNav
{
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    public class ControlFilter : MonoBehaviour
    {
        public Volume postProcessingVolume;

        protected void Update()
        {
            if (postProcessingVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
            {
                colorAdjustments.colorFilter.value = new Color(0.85f, Random.Range(0.0f,1.0f), 0.5f, 1);
            }
        }
    }
}
