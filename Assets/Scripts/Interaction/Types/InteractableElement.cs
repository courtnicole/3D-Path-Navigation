namespace PathNav.Interaction
{
    using System.Collections.Generic;
    using Extensions;
    using Input;
    using UI;
    using UnityEngine;

    public class InteractableElement : MonoBehaviour, IInteractable, IHighlightable
    {
        #region Implementation of IInteractable
        [SerializeField] private InteractableTypes type = InteractableTypes.Default;
        public InteractableTypes Type => type;

        public UniqueId Id { get; set; }

        private List<IController> _interactors = new();
        private List<IController> _selectors = new();

        private void Awake()
        {
            gameObject.TryGetComponent(out Highlighter highlighter);

            Highlightable.SetHighlighter(highlighter);

            if (type.HasFlag(InteractableTypes.Node) || type.HasFlag(InteractableTypes.Segment)) return;

            Id = UniqueId.Generate();
        }

        private void Update()
        {
            if (!_isHighlighted) return;
            Highlightable.UpdateHighlight();
        }

        private void OnDestroy()
        {
            if (type.HasFlag(InteractableTypes.Node) || type.HasFlag(InteractableTypes.Segment)) return;

            UniqueId.Release(Id);
        }

        public void OnHover()
        {
            Highlightable.Highlight();
            _isHighlighted = true;
        }

        public void OnUnhover()
        {
            Highlightable.Unhighlight();
            _isHighlighted = false;
        }

        public void AddInteractor(IController interactor)
        {
            if (_interactors.Contains(interactor)) return;

            _interactors.Add(interactor);
        }

        public void RemoveInteractor(IController interactor)
        {
            if (!_interactors.Contains(interactor)) return;

            _interactors.Remove(interactor);
        }

        public void AddSelectingInteractor(IController selector)
        {
            if (_selectors.Contains(selector)) return;

            _selectors.Add(selector);
        }

        public void RemoveSelectingInteractor(IController selector)
        {
            if (!_selectors.Contains(selector)) return;

            _selectors.Remove(selector);
        }
        #endregion

        #region Implementation of IHighlightable
        private IHighlightable Highlightable => this;

        [SerializeField] private Color highlightColor = Color.green;

        public Highlighter Highlighter { get; set; }

        public Color HighlightColor => highlightColor;
        public float HighlighterWidth => 7f;

        private bool _isHighlighted;
        #endregion
    }
}