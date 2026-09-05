using System;
using System.Collections.Generic;

namespace IIMLib.Core.Message
{
    public sealed class MessageService : IMessageService
    {
        private readonly Dictionary<Type, Delegate[]> _subscriptions = new();

        public void Initialize()
        {
        }

        public void Subscribe<T>(in Action<T> message) where T : struct, IMessage
        {
            if (message == null)
                return;

            var type = typeof(T);

            if (!_subscriptions.TryGetValue(type, out var handlers))
            {
                _subscriptions[type] = new Delegate[] { message };
                return;
            }

            for (var i = 0; i < handlers.Length; i++)
            {
                if (Equals(handlers[i], message))
                    return;
            }

            var next = new Delegate[handlers.Length + 1];
            Array.Copy(handlers, next, handlers.Length);
            next[^1] = message;
            _subscriptions[type] = next;
        }

        public void UnSubscribe<T>(in Action<T> message) where T : struct, IMessage
        {
            if (message == null)
                return;

            var type = typeof(T);
            if (!_subscriptions.TryGetValue(type, out var handlers))
                return;

            var index = -1;
            for (var i = 0; i < handlers.Length; i++)
            {
                if (!Equals(handlers[i], message))
                    continue;

                index = i;
                break;
            }

            if (index < 0)
                return;

            if (handlers.Length == 1)
            {
                _subscriptions.Remove(type);
                return;
            }

            var next = new Delegate[handlers.Length - 1];
            if (index > 0)
                Array.Copy(handlers, 0, next, 0, index);
            if (index < handlers.Length - 1)
                Array.Copy(handlers, index + 1, next, index, handlers.Length - index - 1);

            _subscriptions[type] = next;
        }

        public void Publish<T>(in T message) where T : struct, IMessage
        {
            if (!_subscriptions.TryGetValue(typeof(T), out var handlers))
                return;

            // Copy-on-write subscription storage means the captured array remains safe
            // even if a handler subscribes/unsubscribes while the message is being published.
            for (var i = 0; i < handlers.Length; i++)
                ((Action<T>)handlers[i]).Invoke(message);
        }
    }
}
