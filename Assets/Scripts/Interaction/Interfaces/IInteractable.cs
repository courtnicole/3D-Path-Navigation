namespace PathNav.Interaction
{
    using System;
    using Extensions;
    using Input;

    [Flags]
    public enum InteractableTypes
    {
        Default = 0,
        Ui = 1      << 0,
        Node = 1    << 1,
        Segment = 1 << 2,
        Mini = 1    << 3,
    }

    public interface IInteractable
    {
        InteractableTypes Type { get; }
        UniqueId Id { get; }

        void OnHover();
        void OnUnhover();

        void AddInteractor(IController interactor);
        void RemoveInteractor(IController interactor);

        void AddSelectingInteractor(IController interactor);
        void RemoveSelectingInteractor(IController interactor);
    }
}