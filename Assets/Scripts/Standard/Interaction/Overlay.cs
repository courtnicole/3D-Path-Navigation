using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathNav
{
    public class Overlay : MonoBehaviour
    {
        private Renderer _renderer;
        private Material _material;
        private Coroutine _fadeCoroutine;
        private Color _colorBlack = Color.black;
        private Color _colorClear = Color.clear;
        private const float _fadeTime = 1.5f;

        private void Awake()
        {
            _renderer       = GetComponent<Renderer>();
            _material       = _renderer.material;
            _material.color = _colorBlack;
        }
        public void FadeToClear()
        {
            StartCoroutine(Fade(_colorBlack, _colorClear));
        }
        public void FadeToBlack()
        {
            StartCoroutine(Fade(_colorClear, _colorBlack));
        }

        private IEnumerator Fade(Color start, Color end)
        {
            float elapsedTime = 0.0f;
            while (elapsedTime < _fadeTime) {
                elapsedTime     += Time.deltaTime;
                _material.color = Color.Lerp(start, end, (elapsedTime / _fadeTime));
                yield return null;
            }
        }
    }
}
