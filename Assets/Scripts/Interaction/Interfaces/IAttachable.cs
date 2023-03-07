namespace PathNav.Interaction
{
    using UnityEngine;

    public interface IAttachable
    {
        bool Configured { get; }

        void Attach(Transform t);
        void Detach();

        void Hide();
        void Show();
    }
}
