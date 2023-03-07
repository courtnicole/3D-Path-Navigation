namespace PathNav.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;

    public class IconElement : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer IconCanvasRenderer;
        [SerializeField] private SpriteRenderer IconSpriteRenderer;
        [SerializeField] private TMP_Text iconText;

        internal void SetColor(Color color)
        {
            IconCanvasRenderer.color = color;
        }

        internal void SetText(string text)
        {
            iconText.text = text.ToUpper(); 
        }
    }
}
