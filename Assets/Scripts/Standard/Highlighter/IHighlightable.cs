namespace PathNav.UI
{
    using UnityEngine;

    public interface IHighlightable
    {
        Highlighter Highlighter { get; set; }

        Color HighlightColor { get; }
        float HighlighterWidth { get; }

        void SetHighlighter(Highlighter highlighter)
        {
            Highlighter = highlighter;
        }

        bool InitializeHighlighter()
        {
            if (Highlighter.initialized) return true;

            Highlighter.CurrentOutlineMode  = Highlighter.Mode.OutlineVisible;
            Highlighter.CurrentOutlineColor = HighlightColor;
            Highlighter.CurrentOutlineWidth = HighlighterWidth;
            Highlighter.initialized         = true;
            return true;
        }

        void UpdateHighlight()
        {
            Highlighter.UpdateMaterialProperties();
        }

        void Highlight()
        {
            if (Highlighter is null) return;

            if (!InitializeHighlighter()) return;

            Highlighter.LoadSmoothNormals();
            Highlighter.UpdateMaterialProperties();
            Highlighter.AddOutlineMaterialsToObject();
            Highlighter.OutlineActive = true;
        }

        void Unhighlight()
        {
            if (Highlighter is null) return;

            Highlighter.RemoveOutlineMaterialsFromObject();
            Highlighter.OutlineActive = false;
        }
    }
}