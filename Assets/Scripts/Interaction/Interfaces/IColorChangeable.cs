namespace PathNav.Interaction
{
    using UnityEngine;

    public interface IColorChangeable 
    {
        Renderer Renderer { get; }

        void ChangeMaterial(Material material)
        {
            Renderer.material = material;
        }

        void ChangeColor(Color color)
        {
            if (Renderer is null) return;
            Renderer.material.color = color;
        }
    }
}
