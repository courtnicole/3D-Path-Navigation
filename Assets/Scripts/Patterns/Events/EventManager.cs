namespace PathNav.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.Events;

    public class EventManager : MonoBehaviour
    {
        private static readonly Dictionary<Type, object> Queues = new();

        public static void Subscribe<TEventArgs>(EventId eventId, UnityAction<object, TEventArgs> listener) where TEventArgs : EventArgs
        {
            if (!IsValidType<TEventArgs>(eventId))
            {
                Debug.LogError($"Exception attaching event {eventId} to args type {typeof(TEventArgs)}");
                return;
            }

            IEventQueue<TEventArgs> queue = GetQueue<TEventArgs>();
            queue.Subscribe(eventId, listener);
        }

        public static void Unsubscribe<TEventArgs>(EventId eventId, UnityAction<object, TEventArgs> listener) where TEventArgs : EventArgs
        {
            IEventQueue<TEventArgs> queue = GetQueue<TEventArgs>();
            queue.Unsubscribe(eventId, listener);
        }

        public static void Publish<TEventArgs>(EventId eventId, object sender, TEventArgs args) where TEventArgs : EventArgs
        {
            IEventQueue<TEventArgs> queue = GetQueue<TEventArgs>();
            queue.Publish(eventId, sender, args);
        }

        private static IEventQueue<TEventArgs> GetQueue<TEventArgs>() where TEventArgs : EventArgs
        {
            if (Queues.TryGetValue(typeof(TEventArgs), out object queue))
            {
                return queue as IEventQueue<TEventArgs>;
            }
            else
            {
                EventQueue<TEventArgs> newQueue = new();
                Queues.Add(typeof(TEventArgs), newQueue);
                return newQueue;
            }
        }

        private static bool IsValidType<TEventArgs>(EventId eventId)
        {
            Type         enumType       = typeof(EventId);
            MemberInfo[] memberInfos    = enumType.GetMember(eventId.ToString());
            MemberInfo   enumMemberInfo = memberInfos.FirstOrDefault(p => p.DeclaringType == enumType);
            object[]     valueAttribute = enumMemberInfo!.GetCustomAttributes(typeof(EventIdAttribute), inherit: false);
            Type         eType          = ((EventIdAttribute)valueAttribute[0]).ArgType;

            return eType == typeof(TEventArgs);
        }
    }
}