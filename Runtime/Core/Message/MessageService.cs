using System;
using System.Collections.Generic;

namespace IIMLib.Core
{
    public class MessageService : IMessageService
    {
        private readonly Dictionary<Type, HashSet<Delegate>> _Subscriptions = new();
        
        public void Initialize() { }

        public void Subscribe<T>(in Action<T> message) where T : struct, IMessage
        {
            if (_Subscriptions.TryGetValue(typeof(T), out var actionHolder))
            {
                actionHolder.Add(message);;
                return;
            }
            
            _Subscriptions.Add(typeof(T), new HashSet<Delegate>() {message});
        }

        public void Unsubscribe<T>(in Action<T> message) where T : struct, IMessage
        {
            if (!_Subscriptions.TryGetValue(typeof(T), out var actionHolder)) return;
            actionHolder.Remove(message);
            if(actionHolder.Count == 0) _Subscriptions.Remove(typeof(T));
        }

        public void Publish<T>(T message) where T : struct, IMessage
        {
            if (!_Subscriptions.TryGetValue(typeof(T), out var handlers)) return;
            
            foreach (var action in handlers)
            {
                (action as Action<T>)?.Invoke(message);
            }
        }
    }
}