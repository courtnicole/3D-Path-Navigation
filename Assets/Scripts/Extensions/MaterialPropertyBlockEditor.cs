namespace PathNav.Extensions.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public struct MaterialPropertyVector
    {
        public string name;
        public Vector4 value;
    }

    [Serializable]
    public struct MaterialPropertyColor
    {
        public string name;
        public Color value;
    }

    [Serializable]
    public struct MaterialPropertyFloat
    {
        public string name;
        public float value;
    }

    [ExecuteAlways]
    public class MaterialPropertyBlockEditor : MonoBehaviour
    {
        [SerializeField] private List<Renderer> renderers;

        [SerializeField] private List<MaterialPropertyVector> vectorProperties;

        [SerializeField] private List<MaterialPropertyColor> colorProperties;

        [SerializeField] private List<MaterialPropertyFloat> floatProperties;

        [SerializeField] private bool updateEveryFrame = true;

        public List<Renderer> Renderers
        {
            get => renderers;
            set => renderers = value;
        }

        public List<MaterialPropertyVector> VectorProperties
        {
            get => vectorProperties;
            set => vectorProperties = value;
        }

        public List<MaterialPropertyColor> ColorProperties
        {
            get => colorProperties;
            set => colorProperties = value;
        }

        public List<MaterialPropertyFloat> FloatProperties
        {
            get => floatProperties;
            set => floatProperties = value;
        }

        public MaterialPropertyBlock MaterialPropertyBlock
        {
            get
            {
                if (_materialPropertyBlock == null)
                {
                    _materialPropertyBlock = new MaterialPropertyBlock();
                }

                return _materialPropertyBlock;
            }
        }

        private MaterialPropertyBlock _materialPropertyBlock = null;

        protected virtual void Awake()
        {
            if (renderers == null)
            {
                Renderer r = GetComponent<Renderer>();

                if (r != null)
                {
                    renderers = new List<Renderer>()
                    {
                        r
                    };
                }
            }

            UpdateMaterialPropertyBlock();
        }

        public void UpdateMaterialPropertyBlock()
        {
            MaterialPropertyBlock materialPropertyBlock = MaterialPropertyBlock;

            if (vectorProperties != null)
            {
                foreach (MaterialPropertyVector property in vectorProperties)
                {
                    _materialPropertyBlock.SetVector(property.name, property.value);
                }
            }

            if (colorProperties != null)
            {
                foreach (MaterialPropertyColor property in colorProperties)
                {
                    _materialPropertyBlock.SetColor(property.name, property.value);
                }
            }

            if (floatProperties != null)
            {
                foreach (MaterialPropertyFloat property in floatProperties)
                {
                    _materialPropertyBlock.SetFloat(property.name, property.value);
                }
            }

            if (renderers != null)
            {
                foreach (Renderer r in renderers)
                {
                    if (r == null) continue;

                    r.SetPropertyBlock(materialPropertyBlock);
                }
            }
        }

        protected virtual void Update()
        {
            if (updateEveryFrame)
            {
                UpdateMaterialPropertyBlock();
            }
        }
    }
}