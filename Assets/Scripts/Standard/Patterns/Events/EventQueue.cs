namespace PathNav.Events
{
    using System;
    using System.Collections.Generic;
    using UnityEngine.Events;

    public class EventQueue<TEventArgs> : IEventQueue<TEventArgs> where TEventArgs : EventArgs
    {
        private readonly Dictionary<EventId, UnityEvent<object, TEventArgs>> _eventDictionary;

        public EventQueue() => _eventDictionary = new Dictionary<EventId, UnityEvent<object, TEventArgs>>();

        public void Subscribe(EventId eventId, UnityAction<object, TEventArgs> listener)
        {
            if (_eventDictionary.TryGetValue(eventId, out UnityEvent<object, TEventArgs> thisEvent))
            {
                //event exists, add listener to existing event
                thisEvent.AddListener(listener);
                _eventDictionary[eventId] =  thisEvent;
            }
            else
            {
                thisEvent = new UnityEvent<object, TEventArgs>();
                thisEvent.AddListener(listener);
                _eventDictionary.Add(eventId, thisEvent);
            }
        }

        public void Unsubscribe(EventId eventId, UnityAction<object, TEventArgs> listener)
        {
            if (_eventDictionary.TryGetValue(eventId, out UnityEvent<object, TEventArgs> thisEvent))
            {
                thisEvent.RemoveListener(listener);
                _eventDictionary[eventId] =  thisEvent;
            }
        }

        public void Publish(EventId eventId, object sender, TEventArgs args)
        {
            if (_eventDictionary.TryGetValue(eventId, out UnityEvent<object, TEventArgs> thisEvent))
            {
                thisEvent.Invoke(sender, args);
            }
        }
    }
}