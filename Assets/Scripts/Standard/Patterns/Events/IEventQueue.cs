namespace PathNav.Events
{
    using UnityEngine.Events;

    public interface IEventQueue<TEventArgs>
    {
        void Subscribe(EventId eventId, UnityAction<object, TEventArgs> listener);
        void Unsubscribe(EventId eventId, UnityAction<object, TEventArgs> listener);
        void Publish(EventId eventId, object sender, TEventArgs args);
    }
}